namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to parameter metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.ParameterInfo"/> class.
    /// </remarks>
    public interface IApiParameter : IApiSymbol
    {
        /// <summary>
        ///     Gets the type of this parameter.
        /// </summary>
        IApiType ParameterType { get; }

        /// <summary>
        ///     Gets a value that indicates whether this parameter has a default value.
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        ///     Gets a value indicating the default value if the parameter has a default value.
        /// </summary>
        /// <value>
        ///     The default value of the parameter, or System.DBNull.Value if the parameter has no
        ///     default value.
        /// </value>
        object DefaultValue { get; }

        /// <summary>
        ///     Gets a value that indicates how parameters are referenced.
        /// </summary>
        ReferenceKind ReferenceKind { get; }
    }
}
