using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core.Generation
{
    public class ApiTypeReader
    {
        public CompilationUnitSyntax ReadCompilationUnit(Type type, TypeNameResolver typeNameResolver = null)
        {
            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver
                {
                    SimplifyNamespaces = false,
                };
            }

            var compilationUnit = CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(ReadNamespace(type, typeNameResolver)));

            compilationUnit = compilationUnit
                .WithUsings(List(typeNameResolver.GetRegisteredNamespaces().Select(qualifiedNamespace => UsingDirective(ParseName(qualifiedNamespace)))));

            return compilationUnit;
        }

        public NamespaceDeclarationSyntax ReadNamespace(Type type, TypeNameResolver typeNameResolver = null)
        {
            if (typeNameResolver == null)
            {
                typeNameResolver = new TypeNameResolver
                {
                    SimplifyNamespaces = false,
                };
            }

            return NamespaceDeclaration(IdentifierName(type.Namespace))
                .WithMembers(SingletonList(readType(type, typeNameResolver)));
        }

        private MemberDeclarationSyntax readType(Type type, TypeNameResolver typeNameResolver)
        {
            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                var declaration = InterfaceDeclaration(getApiTypeIdentifier(type))
                    .WithBaseList(getBaseList(type))
                    .WithMembers(List(getMembers(type, typeNameResolver)))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

                if (type.IsGenericType)
                {
                    declaration = declaration
                        .WithConstraintClauses(List(getTypeConstraints(type)))
                        .WithTypeParameterList(TypeParameterList(SeparatedList(getTypeParameters(type))));
                }

                return declaration;
            }

            return null;
        }

        private IEnumerable<MemberDeclarationSyntax> getMembers(Type type, TypeNameResolver typeNameResolver)
        {
            // TODO: Add static members to a manager interface. (Daniel Potter, 11/8/2017)
            foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (memberInfo.GetCustomAttribute(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) != null)
                {
                    continue;
                }

                MemberExtensionKind extensionKind;
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Constructor:
                        // TODO: Constructors should be added to a manager interface. (Daniel
                        //       Potter, 11/8/2017)
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

                        yield return eventDeclaration;
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

                        yield return methodDeclaration;
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

                            yield return indexerDeclaration;
                        }
                        else
                        {
                            PropertyDeclarationSyntax propertyDeclaration = getProperty(propertyInfo, typeNameResolver);

                            if (extensionKind == MemberExtensionKind.New)
                            {
                                propertyDeclaration = propertyDeclaration
                                    .AddModifiers(Token(SyntaxKind.NewKeyword));
                            }

                            yield return propertyDeclaration;
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
                    .WithConstraintClauses(List(getTypeConstraints(methodInfo)))
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
            if (type.IsGenericType == false)
            {
                yield break;
            }

            Type typeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

            foreach (Type typeArgument in typeDefinition.GetGenericArguments())
            {
                yield return TypeParameter(typeArgument.Name);
            }
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(Type type)
        {
            if (type.IsGenericType == false)
            {
                yield break;
            }

            Type typeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();

            foreach (Type typeArgument in typeDefinition.GetGenericArguments())
            {
                Type[] typeConstraints = typeArgument.GetGenericParameterConstraints();

                if (typeConstraints.Length == 0)
                {
                    continue;
                }

                yield return TypeParameterConstraintClause(typeArgument.Name)
                    .WithConstraints(SeparatedList(
                        typeConstraints.Select<Type, TypeParameterConstraintSyntax>(
                            typeConstraint => TypeConstraint(IdentifierName(typeConstraint.Name))
                        )
                    ));
            }
        }

        private IEnumerable<TypeParameterSyntax> getTypeParameters(MethodInfo methodInfo)
        {
            MethodInfo methodDefinition = methodInfo.IsGenericMethodDefinition ? methodInfo : methodInfo.GetGenericMethodDefinition();

            foreach (Type typeArgument in methodDefinition.GetGenericArguments())
            {
                TypeParameterSyntax typeParameterDeclaration = TypeParameter(typeArgument.Name);

                // TODO: Verify this the right order.
                if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
                {
                    typeParameterDeclaration = typeParameterDeclaration
                        .WithVarianceKeyword(Token(SyntaxKind.OutKeyword));
                }
                else if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
                {
                    typeParameterDeclaration = typeParameterDeclaration
                        .WithVarianceKeyword(Token(SyntaxKind.InKeyword));
                }

                yield return typeParameterDeclaration;
            }
        }

        private IEnumerable<TypeParameterConstraintClauseSyntax> getTypeConstraints(MethodInfo methodInfo)
        {
            MethodInfo methodDefinition = methodInfo.IsGenericMethodDefinition ? methodInfo : methodInfo.GetGenericMethodDefinition();

            foreach (Type typeArgument in methodDefinition.GetGenericArguments())
            {
                Type[] typeConstraints = typeArgument.GetGenericParameterConstraints();

                var constraintList = new List<TypeParameterConstraintSyntax>();

                if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    constraintList.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));

                    if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                    {
                        constraintList.Add(ConstructorConstraint());
                    }
                }
                else if (typeArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    constraintList.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
                }

                if (typeConstraints.Length == 0)
                {
                    constraintList.AddRange(
                        typeConstraints.Select<Type, TypeParameterConstraintSyntax>(
                            typeConstraint => TypeConstraint(IdentifierName(typeConstraint.Name))
                        )
                    );
                }

                if (constraintList.Count > 0)
                {
                    yield return TypeParameterConstraintClause(typeArgument.Name)
                        .WithConstraints(SeparatedList(constraintList));
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

        private IEnumerable<BaseTypeSyntax> getBaseTypes(Type type)
        {
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                yield return SimpleBaseType(IdentifierName(getApiTypeIdentifier(type.BaseType)));
            }

            foreach (var implementedInterface in type.GetInterfaces())
            {
                yield return SimpleBaseType(IdentifierName(getApiTypeIdentifier(implementedInterface)));
            }
        }

        private SyntaxToken getApiTypeIdentifier(Type type)
        {
            return Identifier("I" + type.Name);
        }
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
