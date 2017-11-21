namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the role of an interface.
    /// </summary>
    public enum InterfaceRole
    {
        /// <summary>
        ///     The interface contains the instance members of the represented type.
        /// </summary>
        Instance,

        /// <summary>
        ///     The interface contains the constructors of the represented type.
        /// </summary>
        Factory,

        /// <summary>
        ///     The interface contains the static members of the represented type.
        /// </summary>
        Manager,
    }
}
