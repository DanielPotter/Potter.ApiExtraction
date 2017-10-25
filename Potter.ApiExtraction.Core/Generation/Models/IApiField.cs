namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to field metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.FieldInfo"/> class.
    /// </remarks>
    public interface IApiField : IApiMember
    {
        /// <summary>
        ///     Gets the type of this field object.
        /// </summary>
        IApiType FieldType { get; }
    }
}
