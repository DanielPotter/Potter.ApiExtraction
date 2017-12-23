using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using CompilerGeneratedAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Generates interfaces for each type exported from an assembly as specified by a
    ///     configuration file.
    /// </summary>
    public class ApiTypeReader
    {
        #region Assembly Reading

        /// <summary>
        ///     Generates interfaces for each type exported from the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///     The assembly from which to generate interfaces.
        /// </param>
        /// <param name="configuration">
        ///     Specifies which and how types should be read.
        /// </param>
        /// <param name="typeNameResolver">
        ///     Specifies a resolver to use.
        /// </param>
        /// <returns>
        ///     A sequence of <see cref="SourceFileInfo"/> objects representing type source code.
        /// </returns>
        public IEnumerable<SourceFileInfo> ReadAssembly(Assembly assembly, ApiConfiguration configuration = null, TypeNameResolver typeNameResolver = null)
        {
            if (configuration == null)
            {
                configuration = ApiConfiguration.CreateDefault(assembly.GetName());
            }

            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver(configuration);
            }

            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Break();
            //}
            //else
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            Console.WriteLine($"Reading assembly: {assembly.FullName} ({assembly.Location})");

            var generators = new Dictionary<string, CompilationUnitGenerator>();

            foreach (var groupConfiguration in configuration.Groups)
            {
                TypeConfiguration typeConfiguration = groupConfiguration.Types;

                generators[groupConfiguration.Name] = new CompilationUnitGenerator(typeNameResolver, typeConfiguration);
            }

            foreach (Type type in assembly.ExportedTypes)
            {
                var typeResolution = typeNameResolver.ResolveType(type);

                if (typeResolution.ShouldGenerate == false)
                {
                    continue;
                }

                GroupConfiguration groupConfiguration = typeResolution.Group;
                if (groupConfiguration == null)
                {
                    continue;
                }

                if (groupConfiguration.Types.IncludeObsolete == false && type.IsDeprecated(out string reason))
                {
                    continue;
                }

                typeNameResolver.ClearRegisteredNamespaces();

                CompilationUnitSyntax compilationUnit = generators[groupConfiguration.Name].ReadCompilationUnit(type);

                yield return new SourceFileInfo(
                    name: typeResolution.InstanceIdentifier.ValueText,
                    namespaceName: typeResolution.NamespaceName.ToString(),
                    group: groupConfiguration.Name,
                    compilationUnit: compilationUnit);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        ///     Creates a new syntax node with all whitespace and end of line trivia replaced with
        ///     regularly formatted trivia.
        /// </summary>
        /// <param name="node">
        ///     The node to format.
        /// </param>
        public static SyntaxNode NormalizeWhitespace(SyntaxNode node)
        {
            // NOTE: This method is necessary because PowerShell has great difficulty calling
            //       generic methods (like NormalizeWhitespace).
            return SyntaxNodeExtensions.NormalizeWhitespace(node);
        }

        #endregion
    }

    /// <summary>
    ///     Generates source code for a specified type.
    /// </summary>
    public class CompilationUnitGenerator
    {
        private const BindingFlags AllPublicMembers = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompilationUnitGenerator"/> class.
        /// </summary>
        /// <param name="typeNameResolver">
        ///     The name resolver for types.
        /// </param>
        /// <param name="typeConfiguration">
        ///     The type configuration information.
        /// </param>
        public CompilationUnitGenerator(TypeNameResolver typeNameResolver, TypeConfiguration typeConfiguration)
        {
            _typeNameResolver = typeNameResolver;
            _typeConfiguration = typeConfiguration;
        }

        private readonly TypeNameResolver _typeNameResolver;
        private readonly TypeConfiguration _typeConfiguration;

        #region Type Reading

        /// <summary>
        ///     Generates an interface for a type.
        /// </summary>
        /// <param name="type">
        ///     The type for which to generate.
        /// </param>
        /// <returns>
        ///     A <see cref="CompilationUnitSyntax"/> object representing the source code for a
        ///     namespace with an interface.
        /// </returns>
        public CompilationUnitSyntax ReadCompilationUnit(Type type)
        {
            NamespaceDeclarationSyntax namespaceDeclaration = readNamespace(type);

            string currentNamespace = namespaceDeclaration.Name.ToString();
            IEnumerable<UsingDirectiveSyntax> usings = _typeNameResolver.GetRegisteredNamespaces()
                .Where(qualifiedNamespace => qualifiedNamespace.ToString() != currentNamespace)
                .OrderBy(qualifiedNamespace => qualifiedNamespace.ToString())
                .Select(qualifiedNamespace => UsingDirective(qualifiedNamespace));

            var compilationUnit = CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));

            compilationUnit = compilationUnit
                .WithUsings(List(usings));

            return compilationUnit;
        }

        private NamespaceDeclarationSyntax readNamespace(Type type)
        {
            SyntaxList<MemberDeclarationSyntax> typeDeclarations;
            if (type.IsEnum)
            {
                typeDeclarations = SingletonList<MemberDeclarationSyntax>(createEnum(type, _typeNameResolver));
            }
            else
            {
                typeDeclarations = List<MemberDeclarationSyntax>(readInterface(type));
            }

            var typeResolution = _typeNameResolver.ResolveType(type);

            return NamespaceDeclaration(typeResolution.NamespaceName)
                .WithMembers(typeDeclarations);
        }

        private IEnumerable<InterfaceDeclarationSyntax> readInterface(Type type)
        {
            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                var members = getMembers(type);

                var instanceMembers = new List<MemberDeclarationSyntax>();
                var factoryMembers = new List<MemberDeclarationSyntax>();
                var managerMembers = new List<MemberDeclarationSyntax>();

                foreach (var member in members)
                {
                    switch (member.role)
                    {
                        case TypeRole.Instance:
                            instanceMembers.Add(member.member);
                            break;

                        case TypeRole.Factory:
                            factoryMembers.Add(member.member);
                            break;

                        case TypeRole.Manager:
                            managerMembers.Add(member.member);
                            break;
                    }
                }

                bool isStatic = type.IsSealed && type.IsAbstract;

                if (isStatic == false)
                {
                    yield return createInterface(type, instanceMembers, TypeRole.Instance);

                    if (factoryMembers.Count > 0)
                    {
                        yield return createInterface(type, factoryMembers, TypeRole.Factory);
                    }
                }

                if (isStatic || managerMembers.Count > 0)
                {
                    yield return createInterface(type, managerMembers, TypeRole.Manager);
                }
            }
        }

        private InterfaceDeclarationSyntax createInterface(Type type, IEnumerable<MemberDeclarationSyntax> members, TypeRole role)
        {
            if (_typeNameResolver.TryGetApiTypeIdentifier(type, role, out SyntaxToken identifier) == false)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot get a generation identifier for a type that should not generate. Type: {type}");
                return null;
            }

            var interfaceDeclaration = InterfaceDeclaration(identifier)
                .WithBaseList(role == TypeRole.Instance ? getBaseList(type) : null)
                .WithMembers(List(members))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (type.IsGenericType)
            {
                interfaceDeclaration = interfaceDeclaration
                    .WithConstraintClauses(List(getTypeConstraints(type)))
                    .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(type))));
            }

            if (type.IsDeprecated(out string reason))
            {
                interfaceDeclaration = interfaceDeclaration
                    .AddAttributeLists(
                        createObsoleteAttribute(reason)
                    );
            }

            return interfaceDeclaration;
        }

        private AttributeListSyntax createObsoleteAttribute(string reason)
        {
            AttributeSyntax attribute = Attribute(
                name: _typeNameResolver.ResolveAttributeTypeName(typeof(ObsoleteAttribute))
            );

            if (reason != null)
            {
                attribute = attribute
                    .WithArgumentList(
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    LiteralExpression(
                                        kind: SyntaxKind.StringLiteralExpression,
                                        token: Literal(reason)
                                    )
                                )
                            )
                        )
                    );
            }

            return AttributeList(
                SingletonSeparatedList(
                    attribute
                )
            );
        }

        private EnumDeclarationSyntax createEnum(Type type, TypeNameResolver typeNameResolver)
        {
            if (typeNameResolver.TryGetApiTypeIdentifier(type, TypeRole.Instance, out SyntaxToken identifier) == false)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot get a generation identifier for a type that should not generate. Type: {type}");
                return null;
            }

            var enumDeclaration = EnumDeclaration(identifier)
                .WithMembers(SeparatedList(getEnumMembers(type)))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (type.IsDeprecated(out string reason))
            {
                enumDeclaration = enumDeclaration
                    .AddAttributeLists(
                        createObsoleteAttribute(reason)
                    );
            }

            return enumDeclaration;
        }

        private IEnumerable<EnumMemberDeclarationSyntax> getEnumMembers(Type type)
        {
            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldInfo.IsSpecialName || fieldInfo.HasAttribute<CompilerGeneratedAttribute>(checkAccessors: false))
                {
                    continue;
                }

                bool isObsolete = type.IsDeprecated(out string reason);
                if (isObsolete && _typeConfiguration.IncludeObsolete == false)
                {
                    continue;
                }

                var enumMemberDeclaration = getEnumMember(fieldInfo);

                if (isObsolete)
                {
                    enumMemberDeclaration = enumMemberDeclaration
                        .AddAttributeLists(
                            createObsoleteAttribute(reason)
                        );
                }

                yield return enumMemberDeclaration;
            }
        }

        private EnumMemberDeclarationSyntax getEnumMember(FieldInfo fieldInfo)
        {
            return EnumMemberDeclaration(fieldInfo.Name);
        }

        private IEnumerable<(MemberDeclarationSyntax member, TypeRole role)> getMembers(Type type)
        {
            // Structs do not contain default constructors.  Therefore, we must add one ourselves.
            if (type.IsValueType)
            {
                MethodDeclarationSyntax constructoMethodDeclaration = getDefaultConstructorMethod(type);

                yield return (constructoMethodDeclaration, TypeRole.Factory);
            }

            foreach (MemberInfo memberInfo in type.GetMembers(AllPublicMembers))
            {
                if (memberInfo.HasAttribute<CompilerGeneratedAttribute>(checkAccessors: false))
                {
                    continue;
                }

                bool isObsolete = memberInfo.IsDeprecated(out string reason);

                if (isObsolete && _typeConfiguration.IncludeObsolete == false)
                {
                    continue;
                }

                MemberExtensionKind extensionKind;
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Constructor:
                        var constructorInfo = (ConstructorInfo) memberInfo;

                        MethodDeclarationSyntax constructorMethodDeclaration = getConstructorMethod(constructorInfo);

                        if (isObsolete)
                        {
                            constructorMethodDeclaration = constructorMethodDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(reason)
                                );
                        }

                        yield return (constructorMethodDeclaration, TypeRole.Factory);
                        break;

                    case MemberTypes.Event:
                        var eventInfo = (EventInfo) memberInfo;

                        extensionKind = eventInfo.GetExtensionKind();
                        if (extensionKind == MemberExtensionKind.Override)
                        {
                            continue;
                        }

                        EventFieldDeclarationSyntax eventDeclaration = getEvent(eventInfo);

                        if (isObsolete)
                        {
                            eventDeclaration = eventDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(reason)
                                );
                        }

                        if (extensionKind == MemberExtensionKind.New)
                        {
                            eventDeclaration = eventDeclaration
                                .AddModifiers(Token(SyntaxKind.NewKeyword));
                        }

                        yield return (eventDeclaration, eventInfo.AddMethod.IsStatic ? TypeRole.Manager : TypeRole.Instance);
                        break;

                    case MemberTypes.Field:
                        var fieldInfo = (FieldInfo) memberInfo;

                        if (fieldInfo.IsSpecialName)
                        {
                            continue;
                        }

                        // TODO: Constants need to be handled. (Daniel Potter, 11/8/2017)
                        if (fieldInfo.IsLiteral)
                        {
                            continue;
                        }

                        PropertyDeclarationSyntax fieldPropertyDeclaration = getFieldProperty(fieldInfo);

                        if (isObsolete)
                        {
                            fieldPropertyDeclaration = fieldPropertyDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(reason)
                                );
                        }

                        yield return (fieldPropertyDeclaration, fieldInfo.IsStatic ? TypeRole.Manager : TypeRole.Instance);
                        break;

                    case MemberTypes.Method:
                        var methodInfo = (MethodInfo) memberInfo;

                        if (methodInfo.IsSpecialName)
                        {
                            continue;
                        }

                        extensionKind = methodInfo.GetExtensionKind();
                        if (extensionKind == MemberExtensionKind.Override)
                        {
                            continue;
                        }

                        MethodDeclarationSyntax methodDeclaration = getMethod(methodInfo);

                        if (isObsolete)
                        {
                            methodDeclaration = methodDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(reason)
                                );
                        }

                        if (extensionKind == MemberExtensionKind.New)
                        {
                            methodDeclaration = methodDeclaration
                                .AddModifiers(Token(SyntaxKind.NewKeyword));
                        }

                        yield return (methodDeclaration, methodInfo.IsStatic ? TypeRole.Manager : TypeRole.Instance);
                        break;

                    case MemberTypes.Property:
                        var propertyInfo = (PropertyInfo) memberInfo;

                        extensionKind = propertyInfo.GetExtensionKind();
                        if (extensionKind == MemberExtensionKind.Override)
                        {
                            continue;
                        }

                        if (propertyInfo.GetIndexParameters().Length > 0)
                        {
                            IndexerDeclarationSyntax indexerDeclaration = getIndexer(propertyInfo);

                            if (isObsolete)
                            {
                                indexerDeclaration = indexerDeclaration
                                    .AddAttributeLists(
                                        createObsoleteAttribute(reason)
                                    );
                            }

                            if (extensionKind == MemberExtensionKind.New)
                            {
                                indexerDeclaration = indexerDeclaration
                                    .AddModifiers(Token(SyntaxKind.NewKeyword));
                            }

                            // Indexers cannot be static.
                            yield return (indexerDeclaration, TypeRole.Instance);
                        }
                        else
                        {
                            PropertyDeclarationSyntax propertyDeclaration = getProperty(propertyInfo);

                            if (isObsolete)
                            {
                                propertyDeclaration = propertyDeclaration
                                    .AddAttributeLists(
                                        createObsoleteAttribute(reason)
                                    );
                            }

                            if (extensionKind == MemberExtensionKind.New)
                            {
                                propertyDeclaration = propertyDeclaration
                                    .AddModifiers(Token(SyntaxKind.NewKeyword));
                            }

                            yield return (propertyDeclaration, propertyInfo.GetAccessors()[0].IsStatic ? TypeRole.Manager : TypeRole.Instance);
                        }
                        break;

                    case MemberTypes.NestedType:
                        // TODO: Nested types need to be handled somehow. (Daniel Potter, 11/8/2017)
                        break;

                    default:
                        // TODO: Log these for diagnostics. (Daniel Potter, 11/8/2017)
                        break;
                }
            }
        }

        private MethodDeclarationSyntax getDefaultConstructorMethod(Type type)
        {
            TypeSyntax returnTypeSyntax = _typeNameResolver.ResolveTypeName(type);
            string baseName = getBaseTypeNameWithoutPrefix(type);

            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(returnTypeSyntax, Identifier("Create" + baseName))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        private MethodDeclarationSyntax getConstructorMethod(ConstructorInfo constructorInfo)
        {
            TypeSyntax returnTypeSyntax = _typeNameResolver.ResolveTypeName(constructorInfo.DeclaringType);
            string baseName = getBaseTypeNameWithoutPrefix(constructorInfo.DeclaringType);

            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(returnTypeSyntax, Identifier("Create" + baseName))
                .WithParameterList(ParameterList(SeparatedList(getParameters(constructorInfo.GetParameters()))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        private string getBaseTypeNameWithoutPrefix(Type type)
        {
            if (_typeNameResolver.TryGetApiTypeIdentifier(type, TypeRole.Instance, out SyntaxToken identifier) == false)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot get a generation identifier for a type that should not generate. Type: {type}");
                return string.Empty;
            }

            string baseName = identifier.Text;

            if ((type.IsClass || type.IsInterface || type.IsSealed)
                && baseName.Length > 1 && baseName[0] == 'I' && char.IsUpper(baseName[1]))
            {
                // Remove the 'I' interface prefix if one exists.
                baseName = baseName.Substring(1);
            }

            return baseName;
        }

        private EventFieldDeclarationSyntax getEvent(EventInfo eventInfo)
        {
            return EventFieldDeclaration(VariableDeclaration(
                type: _typeNameResolver.ResolveTypeName(eventInfo.EventHandlerType),
                variables: SingletonSeparatedList(VariableDeclarator(eventInfo.Name))
            ));
        }

        private MethodDeclarationSyntax getMethod(MethodInfo methodInfo)
        {
            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(_typeNameResolver.ResolveTypeName(methodInfo.ReturnType), methodInfo.Name)
                .WithParameterList(ParameterList(SeparatedList(getParameters(methodInfo.GetParameters()))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            if (methodInfo.IsGenericMethod)
            {
                methodDeclaration = methodDeclaration
                    .WithConstraintClauses(List(getTypeConstraints(methodInfo)))
                    .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(methodInfo))));
            }

            return methodDeclaration;
        }

        private PropertyDeclarationSyntax getFieldProperty(FieldInfo fieldInfo)
        {
            IEnumerable<AccessorDeclarationSyntax> getAccessors()
            {
                yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

                if (fieldInfo.IsInitOnly == false)
                {
                    yield return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
            }

            return PropertyDeclaration(_typeNameResolver.ResolveTypeName(fieldInfo.FieldType), fieldInfo.Name)
                .WithAccessorList(AccessorList(List(getAccessors())));
        }

        private IEnumerable<ParameterSyntax> getParameters(params ParameterInfo[] parameters)
        {
            foreach (var parameterInfo in parameters)
            {
                var parameterSyntax = Parameter(Identifier(parameterInfo.Name))
                    .WithType(_typeNameResolver.ResolveTypeName(parameterInfo.ParameterType));

                if (parameterInfo.IsOut)
                {
                    parameterSyntax = parameterSyntax.AddModifiers(Token(SyntaxKind.OutKeyword));
                }
                else if (parameterInfo.ParameterType.IsByRef)
                {
                    // Since a by-ref type can only be either an out or a ref parameter and we have
                    // already ruled out this being an out parameter, this parameter is a ref
                    // parameter.
                    parameterSyntax = parameterSyntax.AddModifiers(Token(SyntaxKind.RefKeyword));
                }
                else if (parameterInfo.IsOptional)
                {
                    // TODO: Handle optional parameters. (Daniel Potter, 11/8/2017)
                }
                else if (parameterInfo.HasAttribute<ParamArrayAttribute>())
                {
                    parameterSyntax = parameterSyntax.AddModifiers(Token(SyntaxKind.ParamKeyword));
                }

                yield return parameterSyntax;
            }
        }

        private PropertyDeclarationSyntax getProperty(PropertyInfo propertyInfo)
        {
            IEnumerable<AccessorDeclarationSyntax> getAccessors()
            {
                if (propertyInfo.CanRead)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }

                if (propertyInfo.CanWrite)
                {
                    yield return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
            }

            return PropertyDeclaration(_typeNameResolver.ResolveTypeName(propertyInfo.PropertyType), propertyInfo.Name)
                .WithAccessorList(AccessorList(List(getAccessors())));
        }

        private IndexerDeclarationSyntax getIndexer(PropertyInfo propertyInfo)
        {
            IEnumerable<AccessorDeclarationSyntax> getAccessors()
            {
                if (propertyInfo.CanRead)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }

                if (propertyInfo.CanWrite)
                {
                    yield return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
            }

            return IndexerDeclaration(_typeNameResolver.ResolveTypeName(propertyInfo.PropertyType))
                .WithAccessorList(AccessorList(List(getAccessors())))
                .WithParameterList(BracketedParameterList(SeparatedList(getParameters(propertyInfo.GetIndexParameters()))));
        }

        private IEnumerable<TypeParameterSyntax> getTypeParameters(Type type)
        {
            Type typeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

            return getTypeParameters(typeDefinition.GetGenericArguments());
        }

        private IEnumerable<TypeParameterSyntax> getTypeParameters(MethodInfo methodInfo)
        {
            MethodInfo methodDefinition = methodInfo.IsGenericMethodDefinition ? methodInfo : methodInfo.GetGenericMethodDefinition();

            return getTypeParameters(methodDefinition.GetGenericArguments());
        }

        private IEnumerable<TypeParameterSyntax> getTypeParameters(IEnumerable<Type> genericArguments)
        {
            foreach (Type typeArgument in genericArguments)
            {
                TypeParameterSyntax typeParameter = TypeParameter(typeArgument.Name);

                if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
                {
                    typeParameter = typeParameter
                        .WithVarianceKeyword(Token(SyntaxKind.OutKeyword));
                }
                else if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
                {
                    typeParameter = typeParameter
                        .WithVarianceKeyword(Token(SyntaxKind.InKeyword));
                }

                yield return typeParameter;
            }
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(Type type)
        {
            Type typeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

            return getTypeConstraints(typeDefinition.GetGenericArguments());
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(MethodInfo methodInfo)
        {
            MethodInfo methodDefinition = methodInfo.IsGenericMethodDefinition ? methodInfo : methodInfo.GetGenericMethodDefinition();

            return getTypeConstraints(methodDefinition.GetGenericArguments());
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(IEnumerable<Type> genericArguments)
        {
            foreach (Type typeArgument in genericArguments)
            {
                var typeConstraints = new List<TypeParameterConstraintSyntax>();

                if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    typeConstraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));

                    if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                    {
                        typeConstraints.Add(ConstructorConstraint());
                    }
                }
                else if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    typeConstraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
                }

                Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();

                foreach (Type constraintType in constraintTypes)
                {
                    if (constraintType == typeof(ValueType))
                    {
                        continue;
                    }

                    typeConstraints.Add(TypeConstraint(_typeNameResolver.ResolveTypeName(constraintType)));
                }

                if (typeConstraints.Count > 0)
                {
                    yield return TypeParameterConstraintClause(typeArgument.Name)
                        .WithConstraints(SeparatedList(typeConstraints));
                }
            }
        }

        private BaseListSyntax getBaseList(Type type)
        {
            var baseTypes = getBaseTypes(type).ToList();

            if (baseTypes.Count == 0)
            {
                return null;
            }

            return BaseList(SeparatedList(baseTypes));
        }

        private readonly HashSet<Type> _ignoredBaseTypes = new HashSet<Type>
        {
            typeof(object),
            typeof(ValueType),
        };

        private IEnumerable<BaseTypeSyntax> getBaseTypes(Type type)
        {
            if (type.BaseType != null
                && _ignoredBaseTypes.Contains(type.BaseType) == false
                && type.BaseType.FullName != "System.Runtime.InteropServices.WindowsRuntime.RuntimeClass")
            {
                yield return SimpleBaseType(_typeNameResolver.ResolveTypeName(type.BaseType));
            }

            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (implementedInterface.IsNested == false && implementedInterface.IsPublic == false)
                {
                    continue;
                }

                yield return SimpleBaseType(_typeNameResolver.ResolveTypeName(implementedInterface));
            }
        }

        #endregion
    }
}
