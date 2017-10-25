namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about array types.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the array responsibilities of the <see cref="System.Type"/>
    ///     class.
    /// </remarks>
    public interface IApiArrayType : IApiTypeBase
    {
        /// <summary>
        ///     Gets the number of dimensions in an array.
        /// </summary>
        int Rank { get; }

        /// <summary>
        ///     Gets the type of the object encompassed or referred to by the current array.
        /// </summary>
        IApiTypeBase ElementType { get; }
    }
}
