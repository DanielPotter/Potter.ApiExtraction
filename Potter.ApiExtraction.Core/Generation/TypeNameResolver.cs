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
    /// <summary>
    ///     Resolves the name to use for API interfaces and their members.
    /// </summary>
    public class TypeNameResolver
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeNameResolver"/> with the type
        ///     configuration.
        /// </summary>
        /// <param name="typeConfiguration">
        ///     The type configuration specifying how names should resolve.
        /// </param>
        public TypeNameResolver(TypeConfiguration typeConfiguration)
        {
            TypeConfiguration = typeConfiguration ?? throw new ArgumentNullException(nameof(typeConfiguration));
        }

        /// <summary>
        ///     Gets the type configuration specifying how names should resolve.
        /// </summary>
        public TypeConfiguration TypeConfiguration { get; }

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

        /// <summary>
        ///     Resolves the type name for a type reference.
        /// </summary>
        /// <param name="type">
        ///     The type for which to resolve a type name.
        /// </param>
        /// <param name="includeTypeArguments">
        ///     <c>true</c> if generic type arguments should be included in the name; otherwise,
        ///     <c>false</c>.
        /// </param>
        /// <param name="registerNamespace">
        ///     <c>true</c> if the <paramref name="type"/> namespace should be registered;
        ///     otherwise, <c>false</c>.
        /// </param>
        /// <returns>
        /// </returns>
        public TypeSyntax ResolveTypeName(Type type, bool includeTypeArguments = true, bool registerNamespace = true)
        {
            return resolveTypeName(type, includeTypeArguments, registerNamespace);
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
            return IdentifierName(
                resolveTypeName(type,
                    includeTypeArguments: false,
                    registerNamespace: true,
                    isAttributeDefinition: true).ToString()
            );
        }

        private TypeSyntax resolveTypeName(Type type, bool includeTypeArguments, bool registerNamespace, bool isAttributeDefinition = false)
        {
            if (type.IsGenericParameter)
            {
                return IdentifierName(type.Name);
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
                return PredefinedType(Token(predefinedSyntaxKind.Value));
            }

            // Check if it is a nullable type.
            if (type.IsGenericType && typeof(Nullable<>).IsEquivalentTo(type.GetGenericTypeDefinition()))
            {
                // TODO: What should we return when includeTypeArguments is false? (Daniel Potter,
                //       11/21/2017)
                return NullableType(
                    elementType: ResolveTypeName(type.GetGenericArguments()[0],
                        includeTypeArguments: false,
                        registerNamespace: registerNamespace)
                );
            }

            string typeName = getBaseTypeName(type);

            if (isAttributeDefinition && type.IsSubclassOf(typeof(Attribute)) && typeName.EndsWith(nameof(Attribute)))
            {
                typeName = typeName.Remove(typeName.Length - nameof(Attribute).Length);
            }

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
                if (registerNamespace)
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
