namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides common information about a type.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the common responsibilities of the <see cref="System.Type"/>
    ///     class.
    /// </remarks>
    public interface IApiTypeBase : IApiMember
    {
        /// <summary>
        ///     Gets the namespace of the type.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        ///     Gets the fully qualified name of the type, including its namespace but not its
        ///     assembly.
        /// </summary>
        string FullName { get; }
    }
}
