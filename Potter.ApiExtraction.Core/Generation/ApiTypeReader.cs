using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core.Generation
{
    public class ApiTypeReader
    {
        private const BindingFlags AllPublicMembers = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static;

        #region Assembly Reading

        public IEnumerable<CompilationUnitSyntax> ReadAssembly(Assembly assembly, TypeNameResolver typeNameResolver = null, ApiConfiguration configuration = null)
        {
            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver();
            }

            foreach (Type type in findConfiguredTypes(assembly.ExportedTypes, configuration?.Types ?? new ApiConfigurationTypes()))
            {
                yield return ReadCompilationUnit(type, typeNameResolver);
            }
        }

        private IEnumerable<Type> findConfiguredTypes(IEnumerable<Type> types, ApiConfigurationTypes typesConfiguration)
        {
            bool addMatches = typesConfiguration.Mode == TypeMode.Whitelist;

            foreach (Type type in types)
            {
                bool matches = isMatch(type, typesConfiguration.Items);

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

        public CompilationUnitSyntax ReadCompilationUnit(Type type, TypeNameResolver typeNameResolver = null)
        {
            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver();
            }

            NamespaceDeclarationSyntax namespaceDeclaration = ReadNamespace(type, typeNameResolver);
            IEnumerable<string> usings = typeNameResolver.GetRegisteredNamespaces();

            var compilationUnit = CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));

            compilationUnit = compilationUnit
                .WithUsings(List(usings.Select(qualifiedNamespace => UsingDirective(ParseName(qualifiedNamespace)))));

            return compilationUnit;
        }

        public NamespaceDeclarationSyntax ReadNamespace(Type type, TypeNameResolver typeNameResolver = null)
        {
            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver();
            }

            return NamespaceDeclaration(IdentifierName(type.Namespace))
                .WithMembers(List<MemberDeclarationSyntax>(readInterface(type, typeNameResolver)));
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
            var instanceDeclaration = InterfaceDeclaration(typeNameResolver.GetApiTypeIdentifier(type, role))
                .WithBaseList(role == InterfaceRole.Instance ? getBaseList(type, typeNameResolver) : null)
                .WithMembers(List(members))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (type.IsGenericType)
            {
                instanceDeclaration = instanceDeclaration
                    .WithConstraintClauses(List(getTypeConstraints(type, typeNameResolver)))
                    .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(type))));
            }

            return instanceDeclaration;
        }

        private static readonly Type _compilerGeneratedType = typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute);

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
                bool isCompilerGenerated;
                if (type.Assembly.ReflectionOnly)
                {
                    isCompilerGenerated = memberInfo.GetCustomAttributesData().Any(attribute => _compilerGeneratedType.IsEquivalentTo(attribute.AttributeType));
                }
                else
                {
                    isCompilerGenerated = memberInfo.GetCustomAttribute(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) != null;
                }

                if (isCompilerGenerated)
                {
                    continue;
                }

                MemberExtensionKind extensionKind;
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Constructor:
                        var constructorInfo = (ConstructorInfo) memberInfo;

                        MethodDeclarationSyntax constructoMethodDeclaration = getConstructorMethod(constructorInfo, typeNameResolver);

                        yield return (constructoMethodDeclaration, InterfaceRole.Factory);
                        break;

                    case MemberTypes.Event:
                        var eventInfo = (EventInfo) memberInfo;

                        extensionKind = eventInfo.GetExtensionKind();
                        if (extensionKind == MemberExtensionKind.Override)
                        {
                            continue;
                        }

                        EventFieldDeclarationSyntax eventDeclaration = getEvent(eventInfo, typeNameResolver);

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
            TypeSyntax rawTypeSyntax = typeNameResolver.ResolveTypeName(type, includeTypeArguments: false, ignoreNamespace: true);

            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(returnTypeSyntax, Identifier("Create" + rawTypeSyntax.ToString()))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        private MethodDeclarationSyntax getConstructorMethod(ConstructorInfo constructorInfo, TypeNameResolver typeNameResolver)
        {
            TypeSyntax returnTypeSyntax = typeNameResolver.GetApiTypeIdentifierName(constructorInfo.DeclaringType, InterfaceRole.Instance);
            TypeSyntax rawTypeSyntax = typeNameResolver.ResolveTypeName(constructorInfo.DeclaringType, includeTypeArguments: false, ignoreNamespace: true);

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
                else if (parameterInfo.IsIn)
                {
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

            if (type.BaseType != null && _ignoredBaseTypes.Contains(type.BaseType) == false)
            {
                yield return SimpleBaseType(typeNameResolver.GetApiTypeIdentifierName(type.BaseType, InterfaceRole.Instance));
            }

            foreach (var implementedInterface in type.GetInterfaces())
            {
                yield return SimpleBaseType(typeNameResolver.GetApiTypeIdentifierName(implementedInterface, InterfaceRole.Instance));
            }
        }

        #endregion

        #region Helpers

        // NOTE: This method is necessary because PowerShell has great difficulty calling generic
        //       methods (like NormalizeWhitespace).

        public static Microsoft.CodeAnalysis.SyntaxNode NormalizeWhitespace(Microsoft.CodeAnalysis.SyntaxNode syntaxNode)
        {
            return Microsoft.CodeAnalysis.SyntaxNodeExtensions.NormalizeWhitespace(syntaxNode);
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
    }

    internal enum MemberExtensionKind
    {
        Normal,
        New,
        Override,
    }
}
