using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        }

        /// <summary>
        ///     Gets the type configuration specifying how names should resolve.
        /// </summary>
        public ApiConfiguration Configuration { get; }

        #region Namespaces

        /// <summary>
        ///     Gets the registered namespaces.
        /// </summary>
        /// <returns>
        ///     A new list of the registered namespaces. This instance will not change.
        /// </returns>
        public IReadOnlyList<string> GetRegisteredNamespaces() => _namespaces.ToList();

        /// <summary>
        ///     Clears the registered namespaces.
        /// </summary>
        public void ClearRegisteredNamespaces() => _namespaces.Clear();

        private readonly HashSet<string> _namespaces = new HashSet<string>();

        #endregion

        #region API Type Names

        /// <summary>
        ///     Gets the identifier name for an API type as specified by the configuration.
        /// </summary>
        /// <param name="type">
        ///     The type for which to get a name.
        /// </param>
        /// <param name="role">
        ///     The intended role for the identifier.
        /// </param>
        /// <param name="includeTypeArguments">
        ///     <c>true</c> if generic type arguments should be included in the name; otherwise,
        ///     <c>false</c>.
        /// </param>
        /// <returns>
        ///     A <see cref="NameSyntax"/> representing the API type name for
        ///     <paramref name="type"/>.
        /// </returns>
        public NameSyntax GetApiTypeIdentifierName(Type type, TypeRole role, bool includeTypeArguments = true)
        {
            SyntaxToken baseTypeIdentifier = resolveTypeWithCaching(type, role).BaseIdentifier;

            if (includeTypeArguments && type.IsGenericType)
            {
                return GenericName(
                    identifier: baseTypeIdentifier,
                    typeArgumentList: TypeArgumentList(
                        SeparatedList(
                            type.GetGenericArguments()
                                .Select(typeArgument => resolveTypeWithCaching(typeArgument, TypeRole.Instance).TypeSyntax)
                        )
                    )
                );
            }
            else
            {
                return IdentifierName(baseTypeIdentifier);
            }
        }

        /// <summary>
        ///     Gets the identifier for an API type as specified by the configuration.
        /// </summary>
        /// <param name="type">
        ///     The type for which to get a name.
        /// </param>
        /// <param name="role">
        ///     The intended role for the identifier.
        /// </param>
        /// <returns>
        ///     A <see cref="SyntaxToken"/> representing the API type name for
        ///     <paramref name="type"/>.
        /// </returns>
        public SyntaxToken GetApiTypeIdentifier(Type type, TypeRole role)
        {
            return resolveTypeWithCaching(type, role).BaseIdentifier;
        }

        #endregion

        #region Name Resolution

        private static readonly Dictionary<string, Type> _externalTypeTransformations = new Dictionary<string, Type>
        {
            ["Windows.Foundation.IAsyncAction"] = typeof(System.Threading.Tasks.Task),
            ["Windows.Foundation.IAsyncOperation`1"] = typeof(System.Threading.Tasks.Task<>),
        };

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
            return resolveTypeWithCaching(type, TypeRole.Instance).TypeSyntax;
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
                resolveTypeWithCaching(type, TypeRole.Instance, isAttributeDefinition: true).BaseIdentifier
            );
        }

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
            // Generic type parameters never match.
            if (type.IsGenericParameter)
            {
                return false;
            }

            if (Configuration.Assembly == null)
            {
                return false;
            }

            AssemblyName configuredAssembly;
            if (string.IsNullOrEmpty(Configuration.Assembly.Name) == false)
            {
                configuredAssembly = new AssemblyName(Configuration.Assembly.Name);
            }
            else if (string.IsNullOrEmpty(Configuration.Assembly.Location) == false)
            {
                configuredAssembly = AssemblyName.GetAssemblyName(Configuration.Assembly.Location);
            }
            else
            {
                return false;
            }

            if (type.Assembly.FullName != configuredAssembly.FullName)
            {
                return false;
            }

            bool matches = isMatch(Configuration.Types);

            bool addMatches = Configuration.Types.Mode == TypeMode.Whitelist;

            System.Diagnostics.Debug.WriteLine($"Type: {type.FullName}, IsMatch: {matches}, AddingMatches: {addMatches}");
            return matches == addMatches;

            bool isMatch(TypeConfiguration typeConfiguration)
            {
                if (typeConfiguration.Items == null)
                {
                    return false;
                }

                foreach (var selector in typeConfiguration.Items)
                {
                    switch (selector)
                    {
                        case TypeSelector typeSelector:
                            if (string.Equals(type.Name, selector.Name))
                            {
                                return true;
                            }
                            break;

                        case NamespaceSelector namespaceSelector:
                            if (string.Equals(type.Namespace, selector.Name))
                            {
                                return true;
                            }

                            if (namespaceSelector.Recursive && type.Namespace.StartsWith(selector.Name + "."))
                            {
                                return true;
                            }
                            break;
                    }
                }

                return false;
            }
        }

        private struct TypeResolutionArgs
        {
            public Type Type;
            public TypeRole TypeRole;

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

        private TypeResolution resolveTypeWithCaching(Type type, TypeRole typeRole, bool isAttributeDefinition = false)
        {
            var args = new TypeResolutionArgs
            {
                Type = type,
                TypeRole = typeRole,
                IsAttributeDefinition = isAttributeDefinition,
                QualifyType = Configuration.Types.SimplifyNamespaces == false,
            };

            if (_typeCache.TryGetValue(args, out TypeResolution typeResolution) == false)
            {
                _typeCache[args] = typeResolution = resolveType(args);
            }

            return typeResolution;
        }

        private TypeResolution resolveType(TypeResolutionArgs args)
        {
            SyntaxToken baseName = resolveBaseToken(args.Type, args.TypeRole, args.IsAttributeDefinition);
            TypeSyntax fullName = resolveTypeName(args.Type, baseName);

            return new TypeResolution(baseName, fullName);

            SyntaxToken resolveBaseToken(Type type, TypeRole typeRole = TypeRole.Instance, bool isAttributeDefinition = false)
            {
                if (IsApiType(type))
                {
                    return resolveBaseApiTypeToken(type, typeRole);
                }

                return resolveBaseTypeToken(type, isAttributeDefinition);
            }

            SyntaxToken resolveBaseApiTypeToken(Type type, TypeRole role)
            {
                // TODO: Allow names from configuration. (Daniel Potter, 11/21/2017)
                var nameBuilder = new StringBuilder();

                if (type.IsInterface == false && type.IsEnum == false)
                {
                    nameBuilder.Append('I');
                }

                if (type.IsGenericType)
                {
                    nameBuilder.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
                }
                else
                {
                    nameBuilder.Append(type.Name);
                }

                switch (role)
                {
                    case TypeRole.Factory:
                        nameBuilder.Append("Factory");
                        break;

                    case TypeRole.Manager:
                        nameBuilder.Append("Manager");
                        break;
                }

                return Identifier(nameBuilder.ToString());
            }

            SyntaxToken resolveBaseTypeToken(Type type, bool isAttributeDefinition = false)
            {
                if (type.IsGenericParameter)
                {
                    return Identifier(type.Name);
                }

                // Unwrap by-ref types.
                if (type.IsByRef)
                {
                    type = type.GetElementType();
                }

                // Check if it is a predefined type.
                var predefinedSyntaxKind = tryGetPredefinedSyntaxKind(type);
                if (predefinedSyntaxKind.HasValue)
                {
                    return Token(predefinedSyntaxKind.Value);
                }

                if (type.IsGenericType)
                {
                    if (typeof(Nullable<>).IsEquivalentTo(type.GetGenericTypeDefinition()))
                    {
                        // Unwrap the nullable type.
                        return resolveBaseTypeToken(type.GetGenericArguments()[0]);
                    }

                    return Identifier(type.Name.Substring(0, type.Name.IndexOf('`')));
                }

                // Handle attributes definitions.
                if (isAttributeDefinition && type.IsSubclassOf(typeof(Attribute)) && type.Name.EndsWith(nameof(Attribute)))
                {
                    Identifier(type.Name.Remove(type.Name.Length - nameof(Attribute).Length));
                }

                return Identifier(type.Name);
            }

            TypeSyntax resolveTypeName(Type type, SyntaxToken? baseTypeName = null)
            {
                var typeName = baseTypeName ?? resolveBaseToken(type);

                // Check if it is a predefined type.
                if (typeName.IsKeyword())
                {
                    return PredefinedType(typeName);
                }

                // Check if it is a nullable type.
                if (type.IsGenericType && typeof(Nullable<>).IsEquivalentTo(type.GetGenericTypeDefinition()))
                {
                    return NullableType(
                        elementType: resolveTypeName(type.GetGenericArguments()[0])
                    );
                }

                // Handle type transformations.
                string qualifiedBaseTypeNameWithArity = $"{type.Namespace}.{type.Name}";

                if (_externalTypeTransformations.TryGetValue(qualifiedBaseTypeNameWithArity, out Type transformType))
                {
                    if (transformType.IsGenericType)
                    {
                        var constructedType = transformType.MakeGenericType(type.GenericTypeArguments);

                        return resolveTypeName(constructedType);
                    }
                    else
                    {
                        return resolveTypeName(transformType);
                    }
                }

                SimpleNameSyntax typeNameSyntax;
                if (type.IsGenericType)
                {
                    var typeArguments = type.GetGenericArguments()
                        .Select(typeArgument => resolveTypeName(typeArgument));

                    typeNameSyntax = GenericName(
                        identifier: typeName,
                        typeArgumentList: TypeArgumentList(SeparatedList(typeArguments))
                    );
                }
                else
                {
                    typeNameSyntax = IdentifierName(typeName);
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

                _namespaces.Add(type.Namespace);

                return typeNameSyntax;
            }
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
