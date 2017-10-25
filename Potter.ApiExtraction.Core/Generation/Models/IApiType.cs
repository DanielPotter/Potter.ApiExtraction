using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Represents type declarations: class types, interface types, array types, or value types.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Type"/> and
    ///     <see cref="System.Reflection.TypeInfo"/> classes.
    /// </remarks>
    public interface IApiType : IApiTypeBase
    {
        /// <summary>
        ///     Gets the kind of the current type (class, struct, or interface).
        /// </summary>
        TypeKind Kind { get; }

        /// <summary>
        ///     Gets the type from which the current type directly inherits.
        /// </summary>
        IApiType BaseType { get; }

        /// <summary>
        ///     Gets a value indicating whether the current type has type parameters that have not
        ///     been replaced by specific types.
        /// </summary>
        bool ContainsGenericParameters { get; }

        /// <summary>
        ///     Gets the generic type parameters of the current instance.
        /// </summary>
        IReadOnlyList<IApiTypeParameter> GenericTypeParameters { get; }

        /// <summary>
        ///     Gets the interfaces implemented by this interface.
        /// </summary>
        IReadOnlyList<IApiType> ImplementedInterfaces { get; }

        #region Members

        /// <summary>
        ///     Gets a collection of the constructors declared by the current type.
        /// </summary>
        IEnumerable<IApiContructor> Constructors { get; }

        /// <summary>
        ///     Gets a collection of the indexers defined by the current type.
        /// </summary>
        IEnumerable<IApiIndexer> Indexers { get; }

        /// <summary>
        ///     Gets a collection of the properties defined by the current type.
        /// </summary>
        IEnumerable<IApiProperty> Properties { get; }

        /// <summary>
        ///     Gets a collection of the nested types defined by the current type.
        /// </summary>
        IEnumerable<IApiTypeBase> NestedTypes { get; }

        /// <summary>
        ///     Gets a collection of the methods defined by the current type.
        /// </summary>
        IEnumerable<IApiMethod> Methods { get; }

        /// <summary>
        ///     Gets a collection of the members defined by the current type.
        /// </summary>
        IEnumerable<IApiMember> Members { get; }

        /// <summary>
        ///     Gets a collection of the fields defined by the current type.
        /// </summary>
        IEnumerable<IApiField> Fields { get; }

        /// <summary>
        ///     Gets a collection of the events defined by the current type.
        /// </summary>
        IEnumerable<IApiEvent> Events { get; }

        #endregion
    }
}
