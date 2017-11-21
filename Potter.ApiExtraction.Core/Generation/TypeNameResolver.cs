using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core.Generation
{
    public class TypeNameResolver
    {
        public TypeNameResolver(TypeConfiguration typeConfiguration)
        {
            if (typeConfiguration == null)
            {
                throw new ArgumentNullException(nameof(typeConfiguration));
            }

            TypeConfiguration = typeConfiguration;
        }

        public TypeConfiguration TypeConfiguration { get; }

        public IReadOnlyList<string> GetRegisteredNamespaces() => _namespaces.ToList();

        public void ClearRegisteredNamespaces() => _namespaces.Clear();

        private readonly HashSet<string> _namespaces = new HashSet<string>();

        public NameSyntax GetApiTypeIdentifierName(Type type, InterfaceRole role, bool includeTypeArguments = true)
        {
            SyntaxToken baseTypeIdentifier = GetApiTypeIdentifier(type, role);

            if (includeTypeArguments && type.IsGenericType)
            {
                return GenericName(
                    identifier: baseTypeIdentifier,
                    typeArgumentList: TypeArgumentList(
                        SeparatedList(
                            type.GetGenericArguments().Select(typeArgument => ResolveTypeName(typeArgument, includeTypeArguments))
                        )
                    )
                );
            }
            else
            {
                return IdentifierName(baseTypeIdentifier);
            }
        }

        public SyntaxToken GetApiTypeIdentifier(Type type, InterfaceRole role)
        {
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
                case InterfaceRole.Factory:
                    nameBuilder.Append("Factory");
                    break;

                case InterfaceRole.Manager:
                    nameBuilder.Append("Manager");
                    break;
            }

            return Identifier(nameBuilder.ToString());
        }

        public TypeSyntax ResolveTypeName(Type type, bool includeTypeArguments = true, bool ignoreNamespace = false)
        {
            if (type.IsGenericParameter)
            {
                return IdentifierName(type.Name);
            }

            var predefinedSyntaxKind = tryGetPredefinedSyntaxKind(type);

            if (predefinedSyntaxKind.HasValue)
            {
                return PredefinedType(Token(predefinedSyntaxKind.Value));
            }

            string typeName = getBaseTypeName(type);

            SimpleNameSyntax typeNameSyntax;
            if (includeTypeArguments && type.IsGenericType)
            {
                typeNameSyntax = GenericName(
                    identifier: Identifier(typeName),
                    typeArgumentList: TypeArgumentList(
                        SeparatedList(
                            type.GetGenericArguments().Select(typeArgument => ResolveTypeName(typeArgument, includeTypeArguments))
                        )
                    )
                );
            }
            else
            {
                typeNameSyntax = IdentifierName(typeName);
            }

            if (TypeConfiguration.SimplifyNamespaces)
            {
                if (ignoreNamespace == false)
                {
                    _namespaces.Add(type.Namespace);
                }

                return typeNameSyntax;
            }

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

        private string getBaseTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Substring(0, type.Name.IndexOf('`'));
            }

            return type.Name;
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
    }
}
