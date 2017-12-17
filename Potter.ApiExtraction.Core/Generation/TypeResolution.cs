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
        /// <param name="typeSyntax">
        ///     The full, qualified name of the type.
        /// </param>
        /// <param name="namespaceName">
        ///     The namespace qualification of the type.
        /// </param>
        /// <param name="shouldGenerate">
        ///     A value indicating whether a file should be generated for this type.
        /// </param>
        public TypeResolution(TypeSyntax typeSyntax, NameSyntax namespaceName, bool shouldGenerate = false)
            : this(typeSyntax, namespaceName, default(SyntaxToken), default(SyntaxToken), default(SyntaxToken), shouldGenerate: shouldGenerate)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeResolution"/> structure.
        /// </summary>
        /// <param name="typeSyntax">
        ///     The full, qualified name of the type.
        /// </param>
        /// <param name="namespaceName">
        ///     The namespace qualification of the type.
        /// </param>
        /// <param name="instanceIdentifier">
        ///     The name of the resolved type without generic parameters.
        /// </param>
        /// <param name="factoryIdentifier">
        ///     The name of the resolved type without generic parameters when used as a factory.
        /// </param>
        /// <param name="managerIdentifier">
        ///     The name of the resolved type without generic parameters when used as a manager.
        /// </param>
        /// <param name="shouldGenerate">
        ///     A value indicating whether a file should be generated for this type.
        /// </param>
        public TypeResolution(
            TypeSyntax typeSyntax,
            NameSyntax namespaceName,
            SyntaxToken instanceIdentifier,
            SyntaxToken factoryIdentifier,
            SyntaxToken managerIdentifier,
            bool shouldGenerate = true)
        {
            TypeSyntax = typeSyntax;
            NamespaceName = namespaceName;
            ShouldGenerate = shouldGenerate;
            InstanceIdentifier = instanceIdentifier;
            FactoryIdentifier = factoryIdentifier;
            ManagerIdentifier = managerIdentifier;
        }

        /// <summary>
        ///     Gets the full, qualified name of the type.
        /// </summary>
        public TypeSyntax TypeSyntax { get; }

        /// <summary>
        ///     Gets the namespace qualification of the type.
        /// </summary>
        public NameSyntax NamespaceName { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether a file should be generated for this type.
        /// </summary>
        public bool ShouldGenerate { get; set; }

        /// <summary>
        ///     Gets the name of the resolved type without generic parameters.
        /// </summary>
        public SyntaxToken InstanceIdentifier { get; }

        /// <summary>
        ///     Gets the name of the resolved type without generic parameters when used as a
        ///     factory.
        /// </summary>
        public SyntaxToken FactoryIdentifier { get; }

        /// <summary>
        ///     Gets the name of the resolved type without generic parameters when used as a
        ///     manager.
        /// </summary>
        public SyntaxToken ManagerIdentifier { get; }
    }
}
