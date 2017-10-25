using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Potter.ApiExtraction.Core.Generation;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Generates interfaces for an API.
    /// </summary>
    public class ApiGenerator : IApiGenerator
    {
        /// <summary>
        ///     Asynchronously writes interface source code for multiple types.
        /// </summary>
        /// <param name="types">
        ///     The types for which to generate source code.
        /// </param>
        /// <param name="writer">
        ///     The object to write the source code.
        /// </param>
        /// <param name="documentationResolver">
        ///     An object to resolve source documentation.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> object representing the asynchronous operation.
        /// </returns>
        public async Task WriteAsync(IEnumerable<IApiType> types, ITypeWriter writer, IDocumentationResolver documentationResolver = null)
        {
            await Task.WhenAll(types.Select(type => Task.Run(() => Write(type, writer, documentationResolver))));
        }

        /// <summary>
        ///     Writes interface source code for a type.
        /// </summary>
        /// <param name="type">
        ///     The type for which to generate source code.
        /// </param>
        /// <param name="writer">
        ///     The object to write the source code.
        /// </param>
        /// <param name="documentationResolver">
        ///     An object to resolve source documentation.
        /// </param>
        public void Write(IApiType type, ITypeWriter writer, IDocumentationResolver documentationResolver = null)
        {
            SyntaxTree existingSyntaxTree;
            if (writer.TryReadType(type, out TextReader reader))
            {
                using (reader)
                {
                    SourceText text = SourceText.From(reader, length: int.MaxValue);
                    existingSyntaxTree = ParseSyntaxTree(text);
                }
            }

            var compilationUnit =
                CompilationUnit()
                .AddUsings()
                .AddMembers(
                    NamespaceDeclaration(IdentifierName(type.Namespace))
                    .WithMembers(SingletonList(getMember(type))))
                .NormalizeWhitespace();

            using (var textWriter = writer.WriteType(type))
            {
                compilationUnit.WriteTo(textWriter);
            }
        }

        private MemberDeclarationSyntax getMember(IApiMember member)
        {
            switch (member)
            {
                case IApiType type:
                    return getType(type);

                case IApiEnum enumType:
                    return getEnum(enumType);

                case IApiDelegate delegateType:
                    return getDelegate(delegateType);

                case IApiField field:
                    return getField(field);

                case IApiContructor constructor:
                    return getConstructor(constructor);

                case IApiIndexer indexer:
                    return getIndexer(indexer);

                case IApiProperty property:
                    return getProperty(property);

                case IApiEvent eventMember:
                    return getEvent(eventMember);

                case IApiMethod method:
                    return getMethod(method);

                default:
                    throw new NotImplementedException();
            }
        }

        private TypeDeclarationSyntax getType(IApiType type)
        {
            IEnumerable<BaseTypeSyntax> baseTypes;

            switch (type.Kind)
            {
                case Generation.TypeKind.Class:
                    var classDeclaration = ClassDeclaration(type.Name)
                        .WithAttributeLists(List(type.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                        .WithModifiers(TokenList(getMemberModifiers(type)))
                        .WithMembers(List(type.Members.Select(getMember)));

                    baseTypes = getBaseTypes(type);
                    if (baseTypes.Any())
                    {
                        classDeclaration = classDeclaration
                            .WithBaseList(BaseList(SeparatedList(baseTypes)));
                    }

                    if (type.ContainsGenericParameters)
                    {
                        classDeclaration = classDeclaration
                            .WithTypeParameterList(TypeParameterList(SeparatedList(type.GenericTypeParameters.Select(getTypeParameter))))
                            .WithConstraintClauses(
                                List(
                                    type.GenericTypeParameters.SelectMany(
                                        typeParameter => typeParameter.GenericTypeConstraints.Select(getTypeConstraint)
                                    )
                                )
                            );
                    }

                    return classDeclaration;

                case Generation.TypeKind.Struct:
                    var structDeclaration = StructDeclaration(type.Name)
                        .WithAttributeLists(List(type.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                        .WithModifiers(TokenList(getMemberModifiers(type)))
                        .WithMembers(List(type.Members.Select(getMember)));

                    baseTypes = getBaseTypes(type);
                    if (baseTypes.Any())
                    {
                        structDeclaration = structDeclaration
                            .WithBaseList(BaseList(SeparatedList(baseTypes)));
                    }

                    if (type.ContainsGenericParameters)
                    {
                        structDeclaration = structDeclaration
                            .WithTypeParameterList(TypeParameterList(SeparatedList(type.GenericTypeParameters.Select(getTypeParameter))))
                            .WithConstraintClauses(
                                List(
                                    type.GenericTypeParameters.SelectMany(
                                        typeParameter => typeParameter.GenericTypeConstraints.Select(getTypeConstraint)
                                    )
                                )
                            );
                    }

                    return structDeclaration;

                case Generation.TypeKind.Interface:
                    var interfaceDeclaration = InterfaceDeclaration(type.Name)
                        .WithAttributeLists(List(type.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                        .WithModifiers(TokenList(getMemberModifiers(type)))
                        .WithMembers(List(type.Members.Select(getMember)));

                    baseTypes = getBaseTypes(type);
                    if (baseTypes.Any())
                    {
                        interfaceDeclaration = interfaceDeclaration
                            .WithBaseList(BaseList(SeparatedList(baseTypes)));
                    }

                    if (type.ContainsGenericParameters)
                    {
                        interfaceDeclaration = interfaceDeclaration
                            .WithTypeParameterList(TypeParameterList(SeparatedList(type.GenericTypeParameters.Select(getTypeParameter))))
                            .WithConstraintClauses(
                                List(
                                    type.GenericTypeParameters.SelectMany(
                                        typeParameter => typeParameter.GenericTypeConstraints.Select(getTypeConstraint)
                                    )
                                )
                            );
                    }

                    return interfaceDeclaration;

                default:
                    throw new ArgumentException();
            }
        }

        private EnumDeclarationSyntax getEnum(IApiEnum enumType)
        {
            return EnumDeclaration(enumType.Name)
                .WithAttributeLists(List(enumType.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithBaseList(BaseList(SingletonSeparatedList(getBaseType(enumType.UnderlyingType))))
                .WithMembers(SeparatedList(enumType.Members.Select(getEnumMember)))
                .WithModifiers(TokenList(getMemberModifiers(enumType)));
        }

        private EnumMemberDeclarationSyntax getEnumMember(IApiEnumField field)
        {
            return EnumMemberDeclaration(field.Name)
                .WithAttributeLists(List(field.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithEqualsValue(EqualsValueClause(getLiteralValue(field.Value)));
        }

        private DelegateDeclarationSyntax getDelegate(IApiDelegate delegateType)
        {
            var delegateDeclaration = DelegateDeclaration(getTypeSyntax(delegateType.Method.ReturnType), delegateType.Name)
                .WithAttributeLists(List(delegateType.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(delegateType)))
                .WithParameterList(ParameterList(SeparatedList(delegateType.Method.Parameters.Select(getParameter))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            if (delegateType.Method.ContainsGenericParameters)
            {
                delegateDeclaration = delegateDeclaration
                    .WithTypeParameterList(TypeParameterList(SeparatedList(delegateType.Method.GenericTypeParameters.Select(getTypeParameter))))
                    .WithConstraintClauses(
                        List(
                            delegateType.Method.GenericTypeParameters.SelectMany(
                                typeParameter => typeParameter.GenericTypeConstraints.Select(getTypeConstraint)
                            )
                        )
                    );
            }

            return delegateDeclaration;
        }

        private FieldDeclarationSyntax getField(IApiField field)
        {
            return FieldDeclaration(VariableDeclaration(getTypeSyntax(field.FieldType), SingletonSeparatedList(VariableDeclarator(field.Name))))
                .WithAttributeLists(List(field.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(field))).
                WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private ConstructorDeclarationSyntax getConstructor(IApiContructor constructor)
        {
            return ConstructorDeclaration(constructor.DeclaringType.Name)
                .WithAttributeLists(List(constructor.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(constructor)))
                .WithParameterList(ParameterList(SeparatedList(constructor.Parameters.Select(getParameter))));
        }

        private PropertyDeclarationSyntax getProperty(IApiProperty property)
        {
            IEnumerable<AccessorDeclarationSyntax> getPropertyAccessors()
            {
                if (property.CanRead || property.CanWrite == false)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithModifiers(property.GetterAccess == property.Access ? TokenList() : TokenList(getAccessModifiers(property.GetterAccess)));
                }

                if (property.CanWrite)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithModifiers(property.SetterAccess == property.Access ? TokenList() : TokenList(getAccessModifiers(property.SetterAccess)));
                }
            }

            return PropertyDeclaration(getTypeSyntax(property.PropertyType), property.Name)
                .WithAccessorList(AccessorList(List(getPropertyAccessors())))
                .WithAttributeLists(List(property.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(property)));
        }

        private IndexerDeclarationSyntax getIndexer(IApiIndexer indexer)
        {
            IEnumerable<AccessorDeclarationSyntax> getIndexerAccessors()
            {
                if (indexer.CanRead || indexer.CanWrite == false)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithModifiers(indexer.GetterAccess == indexer.Access ? TokenList() : TokenList(getAccessModifiers(indexer.GetterAccess)));
                }

                if (indexer.CanWrite)
                {
                    yield return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithModifiers(indexer.SetterAccess == indexer.Access ? TokenList() : TokenList(getAccessModifiers(indexer.SetterAccess)));
                }
            }

            return IndexerDeclaration(getTypeSyntax(indexer.PropertyType))
                .WithAccessorList(AccessorList(List(getIndexerAccessors())))
                .WithAttributeLists(List(indexer.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(indexer)));
        }

        private MemberDeclarationSyntax getEvent(IApiEvent eventMember)
        {
            if (eventMember.HasAccessors)
            {
                return EventDeclaration(getTypeSyntax(eventMember.EventHandlerType), eventMember.Name)
                    .WithAccessorList(
                        AccessorList(
                            List(
                                new AccessorDeclarationSyntax[]
                                {
                                    AccessorDeclaration(SyntaxKind.AddAccessorDeclaration)
                                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                    AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration)
                                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                }
                            )
                        )
                    )
                    .WithAttributeLists(List(eventMember.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                    .WithModifiers(TokenList(getMemberModifiers(eventMember)));
            }
            else
            {
                return EventFieldDeclaration(
                        VariableDeclaration(
                            getTypeSyntax(eventMember.EventHandlerType),
                            SingletonSeparatedList(VariableDeclarator(eventMember.Name))
                        )
                    )
                    .WithAttributeLists(List(eventMember.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                    .WithModifiers(TokenList(getMemberModifiers(eventMember)));
            }
        }

        private MethodDeclarationSyntax getMethod(IApiMethod method)
        {
            var methodDeclaration = MethodDeclaration(getTypeSyntax(method.ReturnType), method.Name)
                .WithAttributeLists(List(method.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getMemberModifiers(method)))
                .WithParameterList(ParameterList(SeparatedList(method.Parameters.Select(getParameter))));

            if (method.DeclaringType.Kind == Generation.TypeKind.Interface)
            {
                methodDeclaration = methodDeclaration
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }

            if (method.ContainsGenericParameters)
            {
                methodDeclaration = methodDeclaration
                    .WithTypeParameterList(TypeParameterList(SeparatedList(method.GenericTypeParameters.Select(getTypeParameter))))
                    .WithConstraintClauses(List(method.GenericTypeParameters.SelectMany(typeParameter => typeParameter.GenericTypeConstraints.Select(getTypeConstraint))));
            }

            return methodDeclaration;
        }

        private ParameterSyntax getParameter(IApiParameter parameter)
        {
            SyntaxToken getParameterModifier()
            {
                switch (parameter.ReferenceKind)
                {
                    case ReferenceKind.Ref:
                        return Token(SyntaxKind.RefKeyword);

                    case ReferenceKind.Out:
                        return Token(SyntaxKind.OutKeyword);

                    case ReferenceKind.In:
                        return Token(SyntaxKind.InKeyword);

                    case ReferenceKind.This:
                        return Token(SyntaxKind.ThisKeyword);

                    case ReferenceKind.Params:
                        return Token(SyntaxKind.ParamsKeyword);

                    default:
                        return Token(SyntaxKind.None);
                }
            }

            ParameterSyntax parameterSyntax = Parameter(Identifier(parameter.Name))
                .WithAttributeLists(List(parameter.CustomAttributes.Select(attribute => AttributeList(SingletonSeparatedList(getAttribute(attribute))))))
                .WithModifiers(TokenList(getParameterModifier()))
                .WithType(getTypeSyntax(parameter.ParameterType));

            if (parameter.HasDefaultValue)
            {
                parameterSyntax = parameterSyntax
                    .WithDefault(EqualsValueClause(getLiteralValue(parameter.DefaultValue, includeNull: true)));
            }

            return parameterSyntax;
        }

        private AttributeSyntax getAttribute(IApiAttribute attribute)
        {
            AttributeArgumentSyntax getNamedAttributeArgument(IApiAttributeNamedArgument argument)
            {
                return AttributeArgument(getLiteralValue(argument.TypedValue.Value))
                    .WithNameEquals(NameEquals(IdentifierName(argument.MemberName)));
            }

            AttributeArgumentSyntax getTypedAttributeArgument(IApiAttributeTypedArgument argument)
            {
                return AttributeArgument(getLiteralValue(argument.Value));
            }

            return Attribute(
                    IdentifierName(attribute.AttributeType.Name)
                )
                .WithArgumentList(
                    AttributeArgumentList(
                        SeparatedList(
                            attribute.ConstructorArguments.Select(getTypedAttributeArgument)
                                .Concat(attribute.NamedArguments.Select(getNamedAttributeArgument))
                        )
                    )
                );
        }

        private IEnumerable<SyntaxToken> getAccessModifiers(MemberAccess access)
        {
            // Internal member.
            if (access.HasFlag(MemberAccess.Internal))
            {
                yield return Token(SyntaxKind.InternalKeyword);
            }
            // Protected member.
            else if (access.HasFlag(MemberAccess.Protected))
            {
                yield return Token(SyntaxKind.InternalKeyword);

                // Protected internal member.
                if (access.HasFlag(MemberAccess.Internal))
                {
                    yield return Token(SyntaxKind.InternalKeyword);
                }
            }
            // Public member.
            else if (access.HasFlag(MemberAccess.Public))
            {
                yield return Token(SyntaxKind.PublicKeyword);
            }
            // Private member.
            else if (access.HasFlag(MemberAccess.Private))
            {
                yield return Token(SyntaxKind.PrivateKeyword);
            }
        }

        private IEnumerable<SyntaxToken> getMemberModifiers(IApiMember member)
        {
            if (member.DeclaringType?.Kind == Generation.TypeKind.Interface)
            {
                yield break;
            }

            foreach (var modifier in getAccessModifiers(member.Access))
            {
                yield return modifier;
            }

            if (member is global::Potter.ApiExtraction.Core.Generation.IApiType type && type.Kind != global::Potter.ApiExtraction.Core.Generation.TypeKind.Class)
            {
                yield break;
            }

            if (member.ExtensionModifier != ExtensionModifers.None)
            {
                if (member.ExtensionModifier.HasFlag(ExtensionModifers.Sealed))
                {
                    if (member.ExtensionModifier.HasFlag(ExtensionModifers.Abstract))
                    {
                        yield return Token(SyntaxKind.StaticKeyword);
                    }
                    else
                    {
                        yield return Token(SyntaxKind.SealedKeyword);
                    }
                }
                else if (member.ExtensionModifier.HasFlag(ExtensionModifers.Abstract))
                {
                    yield return Token(SyntaxKind.AbstractKeyword);
                }
                else if (member.ExtensionModifier.HasFlag(ExtensionModifers.Virtual))
                {
                    yield return Token(SyntaxKind.VirtualKeyword);
                }
                else if (member.ExtensionModifier.HasFlag(ExtensionModifers.Override))
                {
                    yield return Token(SyntaxKind.OverrideKeyword);
                }
                else if (member.ExtensionModifier.HasFlag(ExtensionModifers.New))
                {
                    yield return Token(SyntaxKind.NewKeyword);
                }
            }
        }

        private TypeParameterSyntax getTypeParameter(IApiTypeParameter parameter)
        {
            return TypeParameter(parameter.Name);
        }

        private TypeParameterConstraintClauseSyntax getTypeConstraint(IApiTypeBase constraint)
        {
            return TypeParameterConstraintClause(constraint.Name);
        }

        private TypeSyntax getTypeSyntax(IApiTypeBase type)
        {
            bool tryGetPredefinedType(out TypeSyntax predefinedType)
            {
                SyntaxKind predifinedName;

                switch (type.FullName)
                {
                    case "System.String":
                        predifinedName = SyntaxKind.StringKeyword;
                        break;

                    case "System.Int32":
                        predifinedName = SyntaxKind.IntKeyword;
                        break;

                    default:
                        predefinedType = null;
                        return false;
                }

                predefinedType = PredefinedType(Token(predifinedName));
                return true;
            }

            if (tryGetPredefinedType(out TypeSyntax typeSyntax))
            {
                return typeSyntax;
            }

            return ParseTypeName(type.FullName);
        }

        private IEnumerable<BaseTypeSyntax> getBaseTypes(IApiType type)
        {
            if (type.BaseType != null)
            {
                yield return getBaseType(type.BaseType);
            }

            foreach (var interfaceType in type.ImplementedInterfaces)
            {
                yield return getBaseType(interfaceType);
            }
        }

        private BaseTypeSyntax getBaseType(IApiTypeBase type)
        {
            return SimpleBaseType(getTypeSyntax(type));
        }

        private LiteralExpressionSyntax getLiteralValue(object value, bool includeNull = false)
        {
            switch (value)
            {
                case null:
                    if (includeNull)
                    {
                        return LiteralExpression(SyntaxKind.NullLiteralExpression);
                    }

                    return null;

                case bool boolValue:
                    return LiteralExpression(boolValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

                case char charValue:
                    return LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(charValue));

                case string stringValue:
                    return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(stringValue));

                default:
                    SyntaxToken token;

                    switch (value)
                    {
                        case byte byteValue:
                            token = Literal(byteValue);
                            break;

                        case decimal decimalValue:
                            token = Literal(decimalValue);
                            break;

                        case double doubleValue:
                            token = Literal(doubleValue);
                            break;

                        case float floatValue:
                            token = Literal(floatValue);
                            break;

                        case int intValue:
                            token = Literal(intValue);
                            break;

                        case long longValue:
                            token = Literal(longValue);
                            break;

                        case sbyte sbyteValue:
                            token = Literal(sbyteValue);
                            break;

                        case short shortValue:
                            token = Literal(shortValue);
                            break;

                        case uint uintValue:
                            token = Literal(uintValue);
                            break;

                        case ulong ulongValue:
                            token = Literal(ulongValue);
                            break;

                        case ushort ushortValue:
                            token = Literal(ushortValue);
                            break;

                        default:
                            return null;
                    }

                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
        }
    }
}
