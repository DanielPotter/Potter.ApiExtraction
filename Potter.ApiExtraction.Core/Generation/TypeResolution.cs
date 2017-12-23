using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;

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
        /// <param name="hasRegisteredNamespace">
        ///     A value indicating whether <paramref name="namespaceName"/> has been registered.
        /// </param>
        /// <param name="shouldGenerate">
        ///     A value indicating whether a file should be generated for this type.
        /// </param>
        public TypeResolution(
            TypeSyntax typeSyntax,
            NameSyntax namespaceName,
            bool hasRegisteredNamespace,
            bool shouldGenerate = false)
            : this(typeSyntax,
                  namespaceName,
                  null,
                  default(SyntaxToken),
                  default(SyntaxToken),
                  default(SyntaxToken),
                  hasRegisteredNamespace: hasRegisteredNamespace,
                  shouldGenerate: shouldGenerate)
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
        /// <param name="group">
        ///     The group that contains this type.
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
        /// <param name="hasRegisteredNamespace">
        ///     A value indicating whether <paramref name="namespaceName"/> has been registered.
        /// </param>
        /// <param name="shouldGenerate">
        ///     A value indicating whether a file should be generated for this type.
        /// </param>
        public TypeResolution(
            TypeSyntax typeSyntax,
            NameSyntax namespaceName,
            GroupConfiguration group,
            SyntaxToken instanceIdentifier,
            SyntaxToken factoryIdentifier,
            SyntaxToken managerIdentifier,
            bool hasRegisteredNamespace,
            bool shouldGenerate = true)
        {
            TypeSyntax = typeSyntax;
            NamespaceName = namespaceName;
            Group = group;
            HasRegisteredNamespace = hasRegisteredNamespace;
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
        ///     Gets a value indicating whether <see cref="NamespaceName"/> has been registered.
        /// </summary>
        public bool HasRegisteredNamespace { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether a file should be generated for this type.
        /// </summary>
        public bool ShouldGenerate { get; set; }

        /// <summary>
        ///     Gets or sets the group that contains this type if it <see cref="ShouldGenerate"/>.
        /// </summary>
        public GroupConfiguration Group { get; set; }

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
