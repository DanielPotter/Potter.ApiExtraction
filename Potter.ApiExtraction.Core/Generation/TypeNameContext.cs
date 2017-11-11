using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Potter.ApiExtraction.Core.V2.Generation
{
    public class TypeNameResolver
    {
        public bool SimplifyNamespaces { get; set; }

        public IReadOnlyList<string> GetRegisteredNamespaces() => _namespaces.ToList();

        public void ClearRegisteredNamespaces() => _namespaces.Clear();

        private readonly HashSet<string> _namespaces = new HashSet<string>();

        public TypeSyntax ResolveTypeName(Type type)
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

            if (SimplifyNamespaces)
            {
                _namespaces.Add(type.Namespace);

                return IdentifierName(type.Name);
            }

            return ParseTypeName("global::" + type.FullName);
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
