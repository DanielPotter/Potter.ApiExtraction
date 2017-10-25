namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Contains the API extraction configuration.
    /// </summary>
    public interface IApiConfiguration
    {
        /// <summary>
        ///     Gets the extraction configuration.
        /// </summary>
        ApiElement Configuration { get; }
    }
}
