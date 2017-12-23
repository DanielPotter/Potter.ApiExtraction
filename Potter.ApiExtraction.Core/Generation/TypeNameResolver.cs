using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Resolves the name to use for API interfaces and their members.
    /// </summary>
    public class TypeNameResolver
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeNameResolver"/> with configuration.
        /// </summary>
        /// <param name="configuration">
        ///     The type configuration specifying how names should resolve.
        /// </param>
        public TypeNameResolver(ApiConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _configuredAssemblies = new Lazy<HashSet<string>>(initializeAssemblyNames);

            HashSet<string> initializeAssemblyNames()
            {
                var assemblyNames = new HashSet<string>();

                foreach (var assembly in configuration.Assemblies)
                {
                    if (string.IsNullOrEmpty(assembly.Name) == false)
                    {
                        assemblyNames.Add(new AssemblyName(assembly.Name).FullName);
                    }
                    else if (string.IsNullOrEmpty(assembly.Location) == false)
                    {
                        assemblyNames.Add(AssemblyName.GetAssemblyName(assembly.Location).FullName);
                    }
                }

                return assemblyNames;
            }
        }

        #region Configuration

        private readonly Lazy<HashSet<string>> _configuredAssemblies;

        /// <summary>
        ///     Gets the type configuration specifying how names should resolve.
        /// </summary>
        public ApiConfiguration Configuration { get; }

        /// <summary>
        ///     Determines whether the specified type matches the configuration parameters.
        /// </summary>
        /// <param name="type">
        ///     The type to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> exists in the configured assembly and matches
        ///     the type filter; otherwise, <c>false</c>.
        /// </returns>
        public bool IsApiType(Type type)
        {
            return resolveTypeWithCaching(type, registerNamespace: false).ShouldGenerate;
        }

        #endregion

        #region Namespaces

        private readonly Dictionary<NameSyntax, HashSet<Type>> _referencedNamespaces = new Dictionary<NameSyntax, HashSet<Type>>(new NameSyntaxEqualityComparer());

        private class NameSyntaxEqualityComparer : EqualityComparer<NameSyntax>
        {
            public override bool Equals(NameSyntax x, NameSyntax y)
            {
                return string.Equals(x.ToString(), y.ToString());
            }

            public override int GetHashCode(NameSyntax obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        /// <summary>
        ///     Gets the registered namespaces.
        /// </summary>
        /// <returns>
        ///     A new list of the registered namespaces. This instance will not change.
        /// </returns>
        public IReadOnlyList<NameSyntax> GetRegisteredNamespaces() => _referencedNamespaces.Keys.ToList();

        /// <summary>
        ///     Clears the registered namespaces.
        /// </summary>
        public void ClearRegisteredNamespaces() => _referencedNamespaces.Clear();

        /// <summary>
        ///     Registers the namespace of the specified type.
        /// </summary>
        /// <param name="type">
        ///     The type used.
        /// </param>
        public void RegisterTypeUsage(Type type)
        {
            var typeResolution = resolveTypeWithCaching(type, registerNamespace: false);

            registerNamespaceForType(typeResolution.NamespaceName, type);
        }

        private void registerNamespaceForType(NameSyntax namespaceName, Type type)
        {
            if (_referencedNamespaces.TryGetValue(namespaceName, out var list) == false)
            {
                list = _referencedNamespaces[namespaceName] = new HashSet<Type>();
            }

            System.Console.WriteLine($"Register namespace: {namespaceName} for type: {type.FullName}");
            list.Add(type);
        }

        #endregion

        #region API Type Names

        /// <summary>
        ///     Attempts to get the type name for an API type using the provided configuration.
        /// </summary>
        /// <param name="type">
        ///     The type for which to get a name.
        /// </param>
        /// <param name="role">
        ///     The intended role for the identifier.
        /// </param>
        /// <param name="nameSyntax">
        ///     When this method returns, contains a <see cref="NameSyntax"/> representing the API
        ///     type name for <paramref name="type"/> if <paramref name="type"/> is to be generated;
        ///     otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is to be generated; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetApiTypeNameSyntax(Type type, TypeRole role, out NameSyntax nameSyntax)
        {
            TypeResolution typeResolution = resolveTypeWithCaching(type, registerNamespace: false);

            if (typeResolution.ShouldGenerate == false)
            {
                nameSyntax = null;
                return false;
            }

            SyntaxToken baseTypeIdentifier = typeResolution.InstanceIdentifier;

            switch (role)
            {
                default:
                    if (type.IsGenericType)
                    {
                        nameSyntax = GenericName(
                            identifier: baseTypeIdentifier,
                            typeArgumentList: TypeArgumentList(
                                SeparatedList(
                                    type.GetGenericArguments()
                                        .Select(typeArgument => resolveTypeWithCaching(typeArgument).TypeSyntax)
                                )
                            )
                        );
                    }
                    else
                    {
                        nameSyntax = IdentifierName(baseTypeIdentifier);
                    }

                    return true;

                case TypeRole.Factory:
                    baseTypeIdentifier = typeResolution.FactoryIdentifier;
                    break;

                case TypeRole.Manager:
                    baseTypeIdentifier = typeResolution.ManagerIdentifier;
                    break;
            }

            nameSyntax = IdentifierName(baseTypeIdentifier);
            return true;
        }

        /// <summary>
        ///     Attempts to get the identifier for an API type using the provided configuration.
        /// </summary>
        /// <param name="type">
        ///     The type for which to get a name.
        /// </param>
        /// <param name="role">
        ///     The intended role for the identifier.
        /// </param>
        /// <param name="identifier">
        ///     When this method returns, contains a <see cref="SyntaxToken"/> representing an
        ///     identifier for <paramref name="type"/> if <paramref name="type"/> is to be
        ///     generated; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is to be generated; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetApiTypeIdentifier(Type type, TypeRole role, out SyntaxToken identifier)
        {
            TypeResolution typeResolution = resolveTypeWithCaching(type, registerNamespace: false);

            if (typeResolution.ShouldGenerate == false)
            {
                identifier = default(SyntaxToken);
                return false;
            }

            switch (role)
            {
                default:
                    identifier = typeResolution.InstanceIdentifier;
                    break;

                case TypeRole.Factory:
                    identifier = typeResolution.FactoryIdentifier;
                    break;

                case TypeRole.Manager:
                    identifier = typeResolution.ManagerIdentifier;
                    break;
            }

            return true;
        }

        #endregion

        #region Name Resolution

        private static readonly Dictionary<string, Type> _externalTypeTransformations = new Dictionary<string, Type>
        {
            ["Windows.Foundation.IAsyncAction"] = typeof(System.Threading.Tasks.Task),
            ["Windows.Foundation.IAsyncOperation`1"] = typeof(System.Threading.Tasks.Task<>),
        };

        /// <summary>
        ///     Resolve the type names for a type reference.
        /// </summary>
        /// <param name="type">
        ///     The type for which to resolve.
        /// </param>
        /// <returns>
        ///     The resolved type names for the type reference.
        /// </returns>
        public TypeResolution ResolveType(Type type)
        {
            return resolveTypeWithCaching(type);
        }

        /// <summary>
        ///     Resolves the type name for a type reference.
        /// </summary>
        /// <param name="type">
        ///     The type for which to resolve a type name.
        /// </param>
        /// <returns>
        ///     The type name for the type reference.
        /// </returns>
        public TypeSyntax ResolveTypeName(Type type)
        {
            return resolveTypeWithCaching(type).TypeSyntax;
        }

        /// <summary>
        ///     Resolves the type name for an attribute definition.
        /// </summary>
        /// <param name="type">
        ///     The type of attribute.
        /// </param>
        /// <returns>
        ///     The name of the attribute type without the "Attribute" suffix.
        /// </returns>
        public NameSyntax ResolveAttributeTypeName(Type type)
        {
            // Attributes will not have generic arguments so the base name is sufficient.
            return IdentifierName(
                resolveTypeWithCaching(type, isAttributeDefinition: true).InstanceIdentifier
            );
        }

        private struct TypeResolutionArgs
        {
            public Type Type;

            public bool IsAttributeDefinition;
            public bool QualifyType;

            public override bool Equals(object obj)
            {
                return EasyHash.EasyHash<TypeResolutionArgs>.Equals(this, obj);
            }

            public override int GetHashCode()
            {
                return EasyHash.EasyHash<TypeResolutionArgs>.GetHashCode(this);
            }
        }

        private struct MutableTypeResolution
        {
            public TypeSyntax TypeSyntax;
            public NameSyntax NamespaceName;
            public GroupConfiguration Group;
            public bool ShouldRegisterNamespace;
            public bool ShouldGenerate;
            public SyntaxToken InstanceIdentifier;
            public SyntaxToken FactoryIdentifier;
            public SyntaxToken ManagerIdentifier;
        }

        private TypeResolution resolveTypeWithCaching(Type type, bool isAttributeDefinition = false, bool registerNamespace = true)
        {
            var args = new TypeResolutionArgs
            {
                Type = type,
                IsAttributeDefinition = isAttributeDefinition,
                QualifyType = Configuration.SimplifyNamespaces == false,
            };

            if (_typeCache.TryGetValue(args, out TypeResolution typeResolution) == false)
            {
                _typeCache[args] = typeResolution = resolveType(args);
            }

            if (registerNamespace && typeResolution.HasRegisteredNamespace)
            {
                // Ensure that the namespace is actually registered.
                registerNamespaceForType(typeResolution.NamespaceName, type);
            }

            return typeResolution;
        }

        private TypeResolution resolveType(TypeResolutionArgs args)
        {
            // Determine whether the type has configuration. If it does not, simply return the type
            // syntax.
            Type type = args.Type;

            // Unwrap by-ref types.
            if (type.IsByRef)
            {
                type = type.GetElementType();
            }

            if (type.IsGenericParameter)
            {
                return new TypeResolution(IdentifierName(type.Name), null, hasRegisteredNamespace: false);
            }

            // Handle type transformations.
            string qualifiedBaseTypeNameWithArity = $"{type.Namespace}.{type.Name}";

            if (_externalTypeTransformations.TryGetValue(qualifiedBaseTypeNameWithArity, out Type transformType))
            {
                if (transformType.IsGenericType && type.IsConstructedGenericType)
                {
                    Type constructedType = transformType.MakeGenericType(type.GenericTypeArguments);

                    return resolveTypeWithCaching(constructedType);
                }
                else
                {
                    return resolveTypeWithCaching(transformType);
                }
            }

            var mutableTypeResolution = new MutableTypeResolution();

            resolveConfiguredName();
            void resolveConfiguredName()
            {
                // Provide the default namespace name.
                mutableTypeResolution.NamespaceName = ParseName(type.Namespace);

                if (_configuredAssemblies.Value.Count == 0 || _configuredAssemblies.Value.Contains(type.Assembly.FullName) == false)
                {
                    // The type does not exist in any of the assemblies.
                    return;
                }

                if (Configuration.Groups == null)
                {
                    return;
                }

                foreach (var groupConfiguration in Configuration.Groups)
                {
                    bool shouldGenerateMatches = groupConfiguration.Types.Mode == TypeMode.Whitelist;

                    if (groupConfiguration.Types?.Items == null)
                    {
                        mutableTypeResolution.ShouldGenerate = shouldGenerateMatches == false;
                        if (mutableTypeResolution.ShouldGenerate)
                        {
                            mutableTypeResolution.Group = groupConfiguration;
                        }
                        continue;
                    }

                    foreach (var selector in groupConfiguration.Types.Items)
                    {
                        switch (selector)
                        {
                            case TypeSelector typeSelector:
                                if (string.Equals($"{type.Namespace}.{type.Name}", selector.Name)
                                    || string.Equals(type.Name, selector.Name))
                                {
                                    if (string.IsNullOrEmpty(typeSelector.NewName) == false)
                                    {
                                        mutableTypeResolution.InstanceIdentifier = Identifier(typeSelector.NewName);
                                    }

                                    if (string.IsNullOrEmpty(typeSelector.FactoryName) == false)
                                    {
                                        mutableTypeResolution.FactoryIdentifier = Identifier(typeSelector.FactoryName);
                                    }

                                    if (string.IsNullOrEmpty(typeSelector.ManagerName) == false)
                                    {
                                        mutableTypeResolution.ManagerIdentifier = Identifier(typeSelector.ManagerName);
                                    }

                                    mutableTypeResolution.ShouldGenerate = shouldGenerateMatches;
                                    mutableTypeResolution.Group = groupConfiguration;
                                    return;
                                }
                                break;

                            case NamespaceSelector namespaceSelector:
                                if (string.Equals(type.Namespace, selector.Name))
                                {
                                    mutableTypeResolution.ShouldGenerate = shouldGenerateMatches;
                                    mutableTypeResolution.Group = groupConfiguration;

                                    if (string.IsNullOrEmpty(namespaceSelector.NewName) == false)
                                    {
                                        mutableTypeResolution.NamespaceName = ParseName(namespaceSelector.NewName);
                                        return;
                                    }
                                }
                                else if (namespaceSelector.Recursive && type.Namespace.StartsWith(selector.Name + "."))
                                {
                                    mutableTypeResolution.ShouldGenerate = shouldGenerateMatches;
                                    mutableTypeResolution.Group = groupConfiguration;

                                    if (string.IsNullOrEmpty(namespaceSelector.NewName) == false)
                                    {
                                        string namespaceName = $"{namespaceSelector.NewName}.{type.Namespace.Remove(selector.Name.Length + 1)}";

                                        mutableTypeResolution.NamespaceName = ParseName(namespaceName);
                                        return;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            // Resolve default identifiers.
            if (mutableTypeResolution.ShouldGenerate)
            {
                if (mutableTypeResolution.InstanceIdentifier == default(SyntaxToken))
                {
                    string baseName = type.Name;
                    if (type.IsGenericType)
                    {
                        baseName = baseName.Remove(baseName.IndexOf('`'));
                    }

                    // Prefix classes and structs with "I". Exclude enumeration and delegate types.
                    if ((type.IsClass && type.IsSubclassOf(typeof(Delegate)) == false)
                        || (type.IsValueType && type.IsEnum == false))
                    {
                        baseName = "I" + baseName;
                    }

                    mutableTypeResolution.InstanceIdentifier = Identifier(baseName);
                }

                if (mutableTypeResolution.FactoryIdentifier == default(SyntaxToken))
                {
                    mutableTypeResolution.FactoryIdentifier = Identifier(mutableTypeResolution.InstanceIdentifier.ValueText + "Factory");
                }

                if (mutableTypeResolution.ManagerIdentifier == default(SyntaxToken))
                {
                    mutableTypeResolution.ManagerIdentifier = Identifier(mutableTypeResolution.InstanceIdentifier.ValueText + "Manager");
                }
            }

            // Resolve type syntax.
            mutableTypeResolution.ShouldRegisterNamespace = false;
            mutableTypeResolution.TypeSyntax = resolveTypeSyntax();
            TypeSyntax resolveTypeSyntax()
            {
                string baseName = type.Name;
                if (mutableTypeResolution.InstanceIdentifier != default(SyntaxToken))
                {
                    baseName = mutableTypeResolution.InstanceIdentifier.ValueText;
                }
                else if (type.IsGenericType)
                {
                    baseName = baseName.Remove(baseName.IndexOf('`'));
                }

                // Check if it is an array type.
                if (type.IsArray)
                {
                    return ArrayType(
                        elementType: resolveTypeWithCaching(type.GetElementType()).TypeSyntax,
                        rankSpecifiers: SingletonList(ArrayRankSpecifier(SeparatedList<ExpressionSyntax>(
                            Enumerable.Repeat(OmittedArraySizeExpression(), type.GetArrayRank())
                        )))
                    );
                }

                // Check if it is a nullable type.
                if (type.IsGenericType && typeof(Nullable<>).IsEquivalentTo(type.GetGenericTypeDefinition()))
                {
                    return NullableType(
                        elementType: resolveTypeWithCaching(type.GetGenericArguments()[0]).TypeSyntax
                    );
                }

                // Check if it is a predefined type.
                SyntaxKind? predefinedTypeKind = tryGetPredefinedSyntaxKind(type);
                if (predefinedTypeKind.HasValue)
                {
                    return PredefinedType(Token(predefinedTypeKind.Value));
                }

                SimpleNameSyntax typeNameSyntax;
                if (type.IsGenericType)
                {
                    var typeArguments = type.GetGenericArguments()
                        .Select(typeArgument => resolveTypeWithCaching(typeArgument).TypeSyntax);

                    typeNameSyntax = GenericName(
                        identifier: Identifier(baseName),
                        typeArgumentList: TypeArgumentList(SeparatedList(typeArguments))
                    );
                }
                else
                {
                    typeNameSyntax = IdentifierName(baseName);
                }

                if (args.QualifyType)
                {
                    return QualifiedName(
                        left: AliasQualifiedName(
                            alias: IdentifierName(
                                Token(SyntaxKind.GlobalKeyword)
                            ),
                            name: IdentifierName(type.Namespace)
                        ),
                        right: typeNameSyntax
                    );
                }

                mutableTypeResolution.ShouldRegisterNamespace = true;

                return typeNameSyntax;
            }

            return new TypeResolution(
                typeSyntax: mutableTypeResolution.TypeSyntax,
                namespaceName: mutableTypeResolution.NamespaceName,
                group: mutableTypeResolution.Group,
                instanceIdentifier: mutableTypeResolution.InstanceIdentifier,
                factoryIdentifier: mutableTypeResolution.FactoryIdentifier,
                managerIdentifier: mutableTypeResolution.ManagerIdentifier,
                hasRegisteredNamespace: mutableTypeResolution.ShouldRegisterNamespace,
                shouldGenerate: mutableTypeResolution.ShouldGenerate);
        }

        private SyntaxKind? tryGetPredefinedSyntaxKind(Type type)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(bool))
                {
                    return SyntaxKind.BoolKeyword;
                }
                else if (type == typeof(byte))
                {
                    return SyntaxKind.ByteKeyword;
                }
                else if (type == typeof(sbyte))
                {
                    return SyntaxKind.SByteKeyword;
                }
                else if (type == typeof(short))
                {
                    return SyntaxKind.ShortKeyword;
                }
                else if (type == typeof(ushort))
                {
                    return SyntaxKind.UShortKeyword;
                }
                else if (type == typeof(int))
                {
                    return SyntaxKind.IntKeyword;
                }
                else if (type == typeof(uint))
                {
                    return SyntaxKind.UIntKeyword;
                }
                else if (type == typeof(long))
                {
                    return SyntaxKind.LongKeyword;
                }
                else if (type == typeof(ulong))
                {
                    return SyntaxKind.ULongKeyword;
                }
                else if (type == typeof(char))
                {
                    return SyntaxKind.CharKeyword;
                }
                else if (type == typeof(double))
                {
                    return SyntaxKind.DoubleKeyword;
                }
                else if (type == typeof(float))
                {
                    return SyntaxKind.FloatKeyword;
                }
            }
            else if (type == typeof(decimal))
            {
                return SyntaxKind.DecimalKeyword;
            }
            else if (type == typeof(void))
            {
                return SyntaxKind.VoidKeyword;
            }
            else if (type == typeof(string))
            {
                return SyntaxKind.StringKeyword;
            }
            else if (type == typeof(object))
            {
                return SyntaxKind.ObjectKeyword;
            }

            return null;
        }

        #endregion

        #region Type Caching

        private readonly Dictionary<TypeResolutionArgs, TypeResolution> _typeCache = new Dictionary<TypeResolutionArgs, TypeResolution>();

        /// <summary>
        ///     Clears the type cache.
        /// </summary>
        public void ClearTypeCache()
        {
            _typeCache.Clear();
        }

        #endregion
    }
}
