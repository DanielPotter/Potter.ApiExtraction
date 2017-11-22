using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Represents the syntax for a resolved type.
    /// </summary>
    public struct TypeResolution
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeResolution"/> structure.
        /// </summary>
        /// <param name="baseName">
        ///     The name of the resolved type without generic parameters.
        /// </param>
        /// <param name="typeSyntax">
        ///     The full, qualified name of the type.
        /// </param>
        public TypeResolution(SyntaxToken baseName, TypeSyntax typeSyntax)
        {
            BaseIdentifier = baseName;
            TypeSyntax = typeSyntax;
        }

        /// <summary>
        ///     Gets the name of the resolved type without generic parameters.
        /// </summary>
        public SyntaxToken BaseIdentifier { get; }

        /// <summary>
        ///     Gets the full, qualified name of the type.
        /// </summary>
        public TypeSyntax TypeSyntax { get; }
    }
}
