namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Represents an argument of a custom attribute, or an element of an array argument.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the
    ///     <see cref="System.Reflection.CustomAttributeTypedArgument"/> class.
    /// </remarks>
    public interface IApiAttributeTypedArgument
    {
        /// <summary>
        ///     Gets the type of the argument or of the array argument element.
        /// </summary>
        IApiType ArgumentType { get; }

        /// <summary>
        ///     Gets the value of the argument for a simple argument or for an element of an array
        ///     argument; gets a collection of values for an array argument.
        /// </summary>
        object Value { get; }
    }
}
