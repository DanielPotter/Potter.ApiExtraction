namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to metadata for enumeration constants.
    /// </summary>
    public interface IApiEnumField : IApiField
    {
        /// <summary>
        ///     Gets the value of the enum constant.
        /// </summary>
        object Value { get; }
    }
}
