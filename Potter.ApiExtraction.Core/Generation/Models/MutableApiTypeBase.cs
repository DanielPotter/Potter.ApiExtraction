namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides common information about a type.
    /// </summary>
    public abstract class MutableApiTypeBase : MutableApiMember, IApiTypeBase
    {
        /// <summary>
        ///     Gets the namespace of the type.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        ///     Gets the fully qualified name of the type, including its namespace but not its
        ///     assembly.
        /// </summary>
        public string FullName => Namespace == null ? Name : $"{Namespace}.{Name}";
    }
}
