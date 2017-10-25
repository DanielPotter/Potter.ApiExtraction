namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about delegate types.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the delegate responsibilities of the <see cref="System.Type"/>
    ///     class.
    /// </remarks>
    public interface IApiDelegate : IApiTypeBase
    {
        /// <summary>
        ///     Gets the method the represents this delegate.
        /// </summary>
        IApiMethod Method { get; }
    }
}
