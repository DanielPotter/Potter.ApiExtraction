namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Defines the ways in which a parameter reference may be passed.
    /// </summary>
    public enum ReferenceKind
    {
        /// <summary>
        ///     The parameter is passed by reference.
        /// </summary>
        Ref,

        /// <summary>
        ///     The parameter is passed by reference and assigned within the method.
        /// </summary>
        Out,

        /// <summary>
        ///     The parameter is passed normally.
        /// </summary>
        In,

        /// <summary>
        ///     A reference to the target object is implicitly assigned.
        /// </summary>
        This,

        /// <summary>
        ///     A reference to the target array is implicitly assigned.
        /// </summary>
        Params,
    }
}
