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
        private const BindingFlags AllPublicMembers = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static;

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
        /// <returns>
        ///     A sequence of <see cref="CompilationUnitSyntax"/> objects representing interface source
        ///     code.
        /// </returns>
        public IEnumerable<CompilationUnitSyntax> ReadAssembly(Assembly assembly, TypeConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new TypeConfiguration();
            }

            Console.WriteLine($"Reading assembly: {assembly.FullName} ({assembly.Location})");

            foreach (Type type in findConfiguredTypes(assembly.ExportedTypes, configuration))
            {
                if (configuration.IncludeObsolete == false && type.IsDeprecated(out string reason))
                {
                    continue;
                }

                yield return ReadCompilationUnit(type, configuration);
            }
        }

        private IEnumerable<Type> findConfiguredTypes(IEnumerable<Type> types, TypeConfiguration typesConfiguration)
        {
            bool addMatches = typesConfiguration.Mode == TypeMode.Whitelist;

            foreach (Type type in types)
            {
                bool matches = isMatch(type, typesConfiguration.Items);

                System.Diagnostics.Debug.WriteLine($"Type: {type.FullName}, IsMatch: {matches}, AddingMatches: {addMatches}");
                if (matches == addMatches)
                {
                    yield return type;
                }
            }
        }

        private bool isMatch(Type type, IEnumerable<MemberSelector> selectors)
        {
            if (selectors == null)
            {
                return false;
            }

            foreach (var selector in selectors)
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

        #endregion

        #region Type Reading

        /// <summary>
        ///     Generates an interface for a type.
        /// </summary>
        /// <param name="type">
        ///     The type for which to generate.
        /// </param>
        /// <param name="configuration">
        ///     Specifies how the type should be read.
        /// </param>
        /// <returns>
        ///     A <see cref="CompilationUnitSyntax"/> object representing the source code for a
        ///     namespace with an interface.
        /// </returns>
        public CompilationUnitSyntax ReadCompilationUnit(Type type, TypeConfiguration configuration = null)
        {
            var typeNameResolver = new TypeNameResolver(configuration ?? new TypeConfiguration());

            NamespaceDeclarationSyntax namespaceDeclaration = readNamespace(type, typeNameResolver);
            IEnumerable<string> usings = typeNameResolver.GetRegisteredNamespaces();

            var compilationUnit = CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));

            compilationUnit = compilationUnit
                .WithUsings(List(usings.Select(qualifiedNamespace => UsingDirective(ParseName(qualifiedNamespace)))));

            return compilationUnit;
        }

        private NamespaceDeclarationSyntax readNamespace(Type type, TypeNameResolver typeNameResolver)
        {
            if (typeNameResolver == null)
            {
                throw new ArgumentNullException(nameof(typeNameResolver));
            }

            SyntaxList<MemberDeclarationSyntax> typeDeclarations;
            if (type.IsEnum)
            {
                typeDeclarations = SingletonList<MemberDeclarationSyntax>(createEnum(type, typeNameResolver));
            }
            else
            {
                typeDeclarations = List<MemberDeclarationSyntax>(readInterface(type, typeNameResolver));
            }

            return NamespaceDeclaration(IdentifierName(type.Namespace))
                .WithMembers(typeDeclarations);
        }

        private IEnumerable<InterfaceDeclarationSyntax> readInterface(Type type, TypeNameResolver typeNameResolver)
        {
            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                var members = getMembers(type, typeNameResolver);

                var instanceMembers = new List<MemberDeclarationSyntax>();
                var factoryMembers = new List<MemberDeclarationSyntax>();
                var managerMembers = new List<MemberDeclarationSyntax>();

                foreach (var member in members)
                {
                    switch (member.role)
                    {
                        case InterfaceRole.Instance:
                            instanceMembers.Add(member.member);
                            break;

                        case InterfaceRole.Factory:
                            factoryMembers.Add(member.member);
                            break;

                        case InterfaceRole.Manager:
                            managerMembers.Add(member.member);
                            break;
                    }
                }

                bool isStatic = type.IsSealed && type.IsAbstract;

                if (isStatic == false)
                {
                    yield return createInterface(type, instanceMembers, typeNameResolver, InterfaceRole.Instance);

                    if (factoryMembers.Count > 0)
                    {
                        yield return createInterface(type, factoryMembers, typeNameResolver, InterfaceRole.Factory);
                    }
                }

                if (isStatic || managerMembers.Count > 0)
                {
                    yield return createInterface(type, managerMembers, typeNameResolver, InterfaceRole.Manager);
                }
            }
        }

        private InterfaceDeclarationSyntax createInterface(Type type, IEnumerable<MemberDeclarationSyntax> members, TypeNameResolver typeNameResolver, InterfaceRole role)
        {
            var interfaceDeclaration = InterfaceDeclaration(typeNameResolver.GetApiTypeIdentifier(type, role))
                .WithBaseList(role == InterfaceRole.Instance ? getBaseList(type, typeNameResolver) : null)
                .WithMembers(List(members))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (type.IsGenericType)
            {
                interfaceDeclaration = interfaceDeclaration
                    .WithConstraintClauses(List(getTypeConstraints(type, typeNameResolver)))
                    .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(type))));
            }

            if (type.IsDeprecated(out string reason))
            {
                interfaceDeclaration = interfaceDeclaration
                    .AddAttributeLists(
                        createObsoleteAttribute(typeNameResolver, reason)
                    );
            }

            return interfaceDeclaration;
        }

        private static AttributeListSyntax createObsoleteAttribute(TypeNameResolver typeNameResolver, string reason)
        {
            AttributeSyntax attribute = Attribute(
                name: typeNameResolver.ResolveAttributeTypeName(typeof(ObsoleteAttribute))
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
            var enumDeclaration = EnumDeclaration(typeNameResolver.GetApiTypeIdentifier(type, InterfaceRole.Instance))
                .WithMembers(SeparatedList(getEnumMembers(type, typeNameResolver)))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (type.IsDeprecated(out string reason))
            {
                enumDeclaration = enumDeclaration
                    .AddAttributeLists(
                        createObsoleteAttribute(typeNameResolver, reason)
                    );
            }

            return enumDeclaration;
        }

        private IEnumerable<EnumMemberDeclarationSyntax> getEnumMembers(Type type, TypeNameResolver typeNameResolver)
        {
            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldInfo.IsSpecialName || fieldInfo.HasAttribute<CompilerGeneratedAttribute>(checkAccessors: false))
                {
                    continue;
                }

                bool isObsolete = type.IsDeprecated(out string reason);
                if (isObsolete && typeNameResolver.TypeConfiguration.IncludeObsolete == false)
                {
                    continue;
                }

                var enumMemberDeclaration = getEnumMember(fieldInfo, typeNameResolver);

                if (isObsolete)
                {
                    enumMemberDeclaration = enumMemberDeclaration
                        .AddAttributeLists(
                            createObsoleteAttribute(typeNameResolver, reason)
                        );
                }

                yield return enumMemberDeclaration;
            }
        }

        private EnumMemberDeclarationSyntax getEnumMember(FieldInfo fieldInfo, TypeNameResolver typeNameResolver)
        {
            return EnumMemberDeclaration(fieldInfo.Name);
        }

        private IEnumerable<(MemberDeclarationSyntax member, InterfaceRole role)> getMembers(Type type, TypeNameResolver typeNameResolver)
        {
            // Structs do not contain default constructors.  Therefore, we must add one ourselves.
            if (type.IsValueType)
            {
                MethodDeclarationSyntax constructoMethodDeclaration = getDefaultConstructorMethod(type, typeNameResolver);

                yield return (constructoMethodDeclaration, InterfaceRole.Factory);
            }

            foreach (MemberInfo memberInfo in type.GetMembers(AllPublicMembers))
            {
                if (memberInfo.HasAttribute<CompilerGeneratedAttribute>(checkAccessors: false))
                {
                    continue;
                }

                bool isObsolete = memberInfo.IsDeprecated(out string reason);

                if (isObsolete && typeNameResolver.TypeConfiguration.IncludeObsolete == false)
                {
                    continue;
                }

                MemberExtensionKind extensionKind;
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Constructor:
                        var constructorInfo = (ConstructorInfo) memberInfo;

                        MethodDeclarationSyntax constructorMethodDeclaration = getConstructorMethod(constructorInfo, typeNameResolver);

                        if (isObsolete)
                        {
                            constructorMethodDeclaration = constructorMethodDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(typeNameResolver, reason)
                                );
                        }

                        yield return (constructorMethodDeclaration, InterfaceRole.Factory);
                        break;

                    case MemberTypes.Event:
                        var eventInfo = (EventInfo) memberInfo;

                        extensionKind = eventInfo.GetExtensionKind();
                        if (extensionKind == MemberExtensionKind.Override)
                        {
                            continue;
                        }

                        EventFieldDeclarationSyntax eventDeclaration = getEvent(eventInfo, typeNameResolver);

                        if (isObsolete)
                        {
                            eventDeclaration = eventDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(typeNameResolver, reason)
                                );
                        }

                        if (extensionKind == MemberExtensionKind.New)
                        {
                            eventDeclaration = eventDeclaration
                                .AddModifiers(Token(SyntaxKind.NewKeyword));
                        }

                        yield return (eventDeclaration, eventInfo.AddMethod.IsStatic ? InterfaceRole.Manager : InterfaceRole.Instance);
                        break;

                    case MemberTypes.Field:
                        // TODO: Constants need to be handled somehow. (Daniel Potter, 11/8/2017)
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

                        MethodDeclarationSyntax methodDeclaration = getMethod(methodInfo, typeNameResolver);

                        if (isObsolete)
                        {
                            methodDeclaration = methodDeclaration
                                .AddAttributeLists(
                                    createObsoleteAttribute(typeNameResolver, reason)
                                );
                        }

                        if (extensionKind == MemberExtensionKind.New)
                        {
                            methodDeclaration = methodDeclaration
                                .AddModifiers(Token(SyntaxKind.NewKeyword));
                        }

                        yield return (methodDeclaration, methodInfo.IsStatic ? InterfaceRole.Manager : InterfaceRole.Instance);
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
                            IndexerDeclarationSyntax indexerDeclaration = getIndexer(propertyInfo, typeNameResolver);

                            if (isObsolete)
                            {
                                indexerDeclaration = indexerDeclaration
                                    .AddAttributeLists(
                                        createObsoleteAttribute(typeNameResolver, reason)
                                    );
                            }

                            if (extensionKind == MemberExtensionKind.New)
                            {
                                indexerDeclaration = indexerDeclaration
                                    .AddModifiers(Token(SyntaxKind.NewKeyword));
                            }

                            // Indexers cannot be static.
                            yield return (indexerDeclaration, InterfaceRole.Instance);
                        }
                        else
                        {
                            PropertyDeclarationSyntax propertyDeclaration = getProperty(propertyInfo, typeNameResolver);

                            if (isObsolete)
                            {
                                propertyDeclaration = propertyDeclaration
                                    .AddAttributeLists(
                                        createObsoleteAttribute(typeNameResolver, reason)
                                    );
                            }

                            if (extensionKind == MemberExtensionKind.New)
                            {
                                propertyDeclaration = propertyDeclaration
                                    .AddModifiers(Token(SyntaxKind.NewKeyword));
                            }

                            yield return (propertyDeclaration, propertyInfo.GetAccessors()[0].IsStatic ? InterfaceRole.Manager : InterfaceRole.Instance);
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

        private MethodDeclarationSyntax getDefaultConstructorMethod(Type type, TypeNameResolver typeNameResolver)
        {
            TypeSyntax returnTypeSyntax = typeNameResolver.GetApiTypeIdentifierName(type, InterfaceRole.Instance);
            TypeSyntax rawTypeSyntax = typeNameResolver.ResolveTypeName(type, includeTypeArguments: false, registerNamespace: false);

            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(returnTypeSyntax, Identifier("Create" + rawTypeSyntax.ToString()))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        private MethodDeclarationSyntax getConstructorMethod(ConstructorInfo constructorInfo, TypeNameResolver typeNameResolver)
        {
            TypeSyntax returnTypeSyntax = typeNameResolver.GetApiTypeIdentifierName(constructorInfo.DeclaringType, InterfaceRole.Instance);
            TypeSyntax rawTypeSyntax = typeNameResolver.ResolveTypeName(constructorInfo.DeclaringType, includeTypeArguments: false, registerNamespace: false);

            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(returnTypeSyntax, Identifier("Create" + rawTypeSyntax.ToString()))
                .WithParameterList(ParameterList(SeparatedList(getParameters(typeNameResolver, constructorInfo.GetParameters()))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        private EventFieldDeclarationSyntax getEvent(EventInfo eventInfo, TypeNameResolver typeNameResolver)
        {
            return EventFieldDeclaration(VariableDeclaration(
                type: typeNameResolver.ResolveTypeName(eventInfo.EventHandlerType),
                variables: SingletonSeparatedList(VariableDeclarator(eventInfo.Name))
            ));
        }

        private MethodDeclarationSyntax getMethod(MethodInfo methodInfo, TypeNameResolver typeNameResolver)
        {
            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(typeNameResolver.ResolveTypeName(methodInfo.ReturnType), methodInfo.Name)
                .WithParameterList(ParameterList(SeparatedList(getParameters(typeNameResolver, methodInfo.GetParameters()))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            if (methodInfo.IsGenericMethod)
            {
                methodDeclaration = methodDeclaration
                    .WithConstraintClauses(List(getTypeConstraints(methodInfo, typeNameResolver)))
                    .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(methodInfo))));
            }

            return methodDeclaration;
        }

        private IEnumerable<ParameterSyntax> getParameters(TypeNameResolver typeNameResolver, params ParameterInfo[] parameters)
        {
            foreach (var parameterInfo in parameters)
            {
                var parameterSyntax = Parameter(Identifier(parameterInfo.Name))
                    .WithType(typeNameResolver.ResolveTypeName(parameterInfo.ParameterType));

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
                else if (parameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
                {
                    parameterSyntax = parameterSyntax.AddModifiers(Token(SyntaxKind.ParamKeyword));
                }

                yield return parameterSyntax;
            }
        }

        private PropertyDeclarationSyntax getProperty(PropertyInfo propertyInfo, TypeNameResolver typeNameResolver)
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

            return PropertyDeclaration(typeNameResolver.ResolveTypeName(propertyInfo.PropertyType), propertyInfo.Name)
                .WithAccessorList(AccessorList(List(getAccessors())));
        }

        private IndexerDeclarationSyntax getIndexer(PropertyInfo propertyInfo, TypeNameResolver typeNameResolver)
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

            return IndexerDeclaration(typeNameResolver.ResolveTypeName(propertyInfo.PropertyType))
                .WithAccessorList(AccessorList(List(getAccessors())))
                .WithParameterList(BracketedParameterList(SeparatedList(getParameters(typeNameResolver, propertyInfo.GetIndexParameters()))));
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

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(Type type, TypeNameResolver typeNameResolver)
        {
            Type typeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

            return getTypeConstraints(typeDefinition.GetGenericArguments(), typeNameResolver);
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(MethodInfo methodInfo, TypeNameResolver typeNameResolver)
        {
            MethodInfo methodDefinition = methodInfo.IsGenericMethodDefinition ? methodInfo : methodInfo.GetGenericMethodDefinition();

            return getTypeConstraints(methodDefinition.GetGenericArguments(), typeNameResolver);
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(IEnumerable<Type> genericArguments, TypeNameResolver typeNameResolver)
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

                    typeConstraints.Add(TypeConstraint(typeNameResolver.ResolveTypeName(constraintType)));
                }

                if (typeConstraints.Count > 0)
                {
                    yield return TypeParameterConstraintClause(typeArgument.Name)
                        .WithConstraints(SeparatedList(typeConstraints));
                }
            }
        }

        private BaseListSyntax getBaseList(Type type, TypeNameResolver typeNameResolver)
        {
            var baseTypes = getBaseTypes(type, typeNameResolver).ToList();

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

        private IEnumerable<BaseTypeSyntax> getBaseTypes(Type type, TypeNameResolver typeNameResolver)
        {

            if (type.BaseType != null
                && _ignoredBaseTypes.Contains(type.BaseType) == false
                && type.BaseType.FullName != "System.Runtime.InteropServices.WindowsRuntime.RuntimeClass")
            {
                yield return SimpleBaseType(typeNameResolver.GetApiTypeIdentifierName(type.BaseType, InterfaceRole.Instance));
            }

            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (implementedInterface.IsNested == false && implementedInterface.IsPublic == false)
                {
                    continue;
                }

                yield return SimpleBaseType(typeNameResolver.GetApiTypeIdentifierName(implementedInterface, InterfaceRole.Instance));
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

    internal static class ReflectionExtensions
    {
        private const BindingFlags AnyMemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static MemberExtensionKind GetExtensionKind(this EventInfo eventInfo)
        {
            EventInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                EventInfo baseEventInfo = baseType.GetEvent(eventInfo.Name, AnyMemberFlags);
                if (baseEventInfo != null)
                {
                    return baseEventInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo firstAccessor = eventInfo.GetAddMethod();

            // Check if the event is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (firstAccessor.GetBaseDefinition().DeclaringType != firstAccessor.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            MethodInfo baseEventAccessor = locateBase(eventInfo.DeclaringType?.BaseType)?.GetAddMethod();
            bool hasBase = baseEventAccessor != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static MemberExtensionKind GetExtensionKind(this PropertyInfo propertyInfo)
        {
            PropertyInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                PropertyInfo basePropertyInfo = baseType.GetProperty(propertyInfo.Name, AnyMemberFlags);
                if (basePropertyInfo != null)
                {
                    return basePropertyInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo firstAccessor = propertyInfo.GetAccessors()[0];

            // Check if the property is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (firstAccessor.GetBaseDefinition().DeclaringType != firstAccessor.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            MethodInfo basePropertyAccessor = locateBase(propertyInfo.DeclaringType?.BaseType)?.GetAccessors()[0];
            bool hasBase = basePropertyAccessor != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static MemberExtensionKind GetExtensionKind(this MethodInfo methodInfo)
        {
            // Check if the method is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            Type[] parameterTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            MethodInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                MethodInfo basePropertyInfo = baseType.GetMethod(methodInfo.Name, AnyMemberFlags, null, parameterTypes, null);
                if (basePropertyInfo != null)
                {
                    return basePropertyInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo baseMethod = locateBase(methodInfo.DeclaringType?.BaseType);
            bool hasBase = baseMethod != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static bool IsDeprecated(this MemberInfo memberInfo, out string reason)
        {
            string innerReason = null;

            bool isDeprecated = HasAttribute(memberInfo, hasAttributeTest, checkAccessors: true);

            reason = innerReason;

            return isDeprecated;

            bool hasAttributeTest(CustomAttributeData attributeData)
            {
                if (typeof(ObsoleteAttribute).IsEquivalentTo(attributeData.AttributeType)
                    || attributeData.AttributeType.FullName == "Windows.Foundation.Metadata.DeprecatedAttribute")
                {
                    if (attributeData.ConstructorArguments?.Count > 0)
                    {
                        innerReason = attributeData.ConstructorArguments[0].Value?.ToString();
                    }

                    return true;
                }

                return false;
            }
        }

        public static bool HasAttribute<T>(this MemberInfo memberInfo, bool checkAccessors)
            where T : Attribute
        {
            Type attributeType = typeof(T);

            Type declaringType = memberInfo as Type ?? memberInfo.DeclaringType;
            bool isReflectionOnly = declaringType.Assembly.ReflectionOnly;

            return hasAttribute(memberInfo, hasAttributeTest, checkAccessors);

            bool hasAttributeTest(MemberInfo innerMemberInfo)
            {
                if (isReflectionOnly)
                {
                    IList<CustomAttributeData> customAttributes = innerMemberInfo.GetCustomAttributesData();

                    return customAttributes.Any(attribute => attributeType.IsEquivalentTo(attribute.AttributeType));
                }
                else
                {
                    return innerMemberInfo.GetCustomAttribute(attributeType) != null;
                }
            }
        }

        public static bool HasAttribute(this MemberInfo memberInfo, Func<CustomAttributeData, bool> attributeTest, bool checkAccessors)
        {
            return hasAttribute(memberInfo, hasAttributeTest, checkAccessors);

            bool hasAttributeTest(MemberInfo innerMemberInfo)
            {
                IList<CustomAttributeData> customAttributes = innerMemberInfo.GetCustomAttributesData();

                return customAttributes.Any(attributeTest);
            }
        }

        private static bool hasAttribute(MemberInfo memberInfo, Func<MemberInfo, bool> attributeTest, bool checkAccessors)
        {
            if (attributeTest(memberInfo))
            {
                return true;
            }

            if (checkAccessors)
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        EventInfo eventInfo = (EventInfo) memberInfo;
                        return attributeTest(eventInfo.AddMethod) || attributeTest(eventInfo.RemoveMethod);

                    case MemberTypes.Property:
                        return ((PropertyInfo) memberInfo).GetAccessors().Any(attributeTest);

                    default:
                        return false;
                }
            }

            return false;
        }
    }

    internal enum MemberExtensionKind
    {
        Normal,
        New,
        Override,
    }
}
