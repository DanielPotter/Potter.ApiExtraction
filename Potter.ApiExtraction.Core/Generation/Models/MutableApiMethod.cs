namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Discovers the attributes of a method and provides access to method metadata.
    /// </summary>
    public class MutableApiMethod : MutableApiMethodBase, IApiMethod
    {
        /// <summary>
        ///     Gets a value indicating type of member - method, constructor, event, and so on.
        /// </summary>
        public override MemberTypes MemberType => MemberTypes.Method;

        /// <summary>
        ///     Gets a parameter object that contains information about the return type of the
        ///     method, such as whether the return type has custom modifiers.
        /// </summary>
        public IApiParameter ReturnParameter { get; set; }

        /// <summary>
        ///     Gets the return type of this method.
        /// </summary>
        public IApiType ReturnType => ReturnParameter?.ParameterType;
    }
}
