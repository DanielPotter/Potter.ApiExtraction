namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Discovers the attributes of a method and provides access to method metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.MethodInfo"/> class.
    /// </remarks>
    public interface IApiMethod : IApiMethodBase
    {
        /// <summary>
        ///     Gets a parameter object that contains information about the return type of the
        ///     method, such as whether the return type has custom modifiers.
        /// </summary>
        IApiParameter ReturnParameter { get; }

        /// <summary>
        ///     Gets the return type of this method.
        /// </summary>
        IApiType ReturnType { get; }
    }
}
