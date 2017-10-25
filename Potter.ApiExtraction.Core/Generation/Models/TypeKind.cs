namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the kinds of types (class, struct, interface).
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        ///     A class type.
        /// </summary>
        Class,

        /// <summary>
        ///     A value type.
        /// </summary>
        Struct,

        /// <summary>
        ///     An interface type.
        /// </summary>
        Interface,
    }
}
