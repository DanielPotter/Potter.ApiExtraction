namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to property metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.PropertyInfo"/> class.
    /// </remarks>
    public interface IApiProperty : IApiMember
    {
        /// <summary>
        ///     Gets the type of this property.
        /// </summary>
        IApiType PropertyType { get; }

        /// <summary>
        ///     Gets a value indicating whether the property can be read.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        ///     Gets a value indicating the accessibility of the get accessor.
        /// </summary>
        MemberAccess GetterAccess { get; }

        /// <summary>
        ///     Gets a value indicating the accessibility of the set accessor.
        /// </summary>
        MemberAccess SetterAccess { get; }
    }
}
