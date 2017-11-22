namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the role of a type.
    /// </summary>
    public enum TypeRole
    {
        /// <summary>
        ///     The type contains the instance members of the represented type.
        /// </summary>
        Instance,

        /// <summary>
        ///     The type contains the constructors of the represented type.
        /// </summary>
        Factory,

        /// <summary>
        ///     The type contains the static members of the represented type.
        /// </summary>
        Manager,
    }
}
