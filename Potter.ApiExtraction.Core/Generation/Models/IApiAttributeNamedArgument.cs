namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Represents a named argument of a custom attribute.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the
    ///     <see cref="System.Reflection.CustomAttributeNamedArgument"/> class.
    /// </remarks>
    public interface IApiAttributeNamedArgument
    {
        /// <summary>
        ///     Gets the name of the attribute member that would be used to set the named argument.
        /// </summary>
        string MemberName { get; }

        /// <summary>
        ///     Gets a value that can be used to obtain the type and value of the current named
        ///     argument.
        /// </summary>
        IApiAttributeTypedArgument TypedValue { get; }
    }
}
