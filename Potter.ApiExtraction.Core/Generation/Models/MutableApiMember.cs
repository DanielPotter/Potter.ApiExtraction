namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Obtains information about the attributes of a member and provides access to member
    ///     metadata.
    /// </summary>
    public abstract class MutableApiMember : MutableApiSymbol, IApiMember
    {
        /// <summary>
        ///     Gets a value indicating type of member - method, constructor, event, and so on.
        /// </summary>
        public abstract MemberTypes MemberType { get; }

        /// <summary>
        ///     Gets a value indicating the accessibility.
        /// </summary>
        public MemberAccess Access { get; set; }

        /// <summary>
        ///     Gets a value indicating how this member may be extended.
        /// </summary>
        public ExtensionModifers ExtensionModifier { get; set; }
    }
}
