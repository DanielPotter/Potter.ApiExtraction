namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Obtains information about the attributes of a member and provides access to member
    ///     metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.MemberInfo"/> class.
    /// </remarks>
    public interface IApiMember : IApiSymbol
    {
        /// <summary>
        ///     Gets a value indicating type of member - method, constructor, event, and so on.
        /// </summary>
        MemberTypes MemberType { get; }

        /// <summary>
        ///     Gets a value indicating the accessibility.
        /// </summary>
        MemberAccess Access { get; }

        /// <summary>
        ///     Gets a value indicating how this member may be extended.
        /// </summary>
        ExtensionModifers ExtensionModifier { get; }
    }
}
