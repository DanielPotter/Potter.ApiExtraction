namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to parameter metadata.
    /// </summary>
    public class MutableApiParameter : MutableApiMember, IApiParameter
    {
        /// <summary>
        ///     Gets a value indicating type of member - method, constructor, event, and so on.
        /// </summary>
        public override MemberTypes MemberType => MemberTypes.Parameter;

        /// <summary>
        ///     Gets the type of this parameter.
        /// </summary>
        public IApiType ParameterType { get; set; }

        /// <summary>
        ///     Gets a value that indicates whether this parameter has a default value.
        /// </summary>
        public bool HasDefaultValue { get; set; }

        /// <summary>
        ///     Gets a value indicating the default value if the parameter has a default value.
        /// </summary>
        /// <value>
        ///     The default value of the parameter, or System.DBNull.Value if the parameter has no
        ///     default value.
        /// </value>
        public object DefaultValue { get; set; }

        /// <summary>
        ///     Gets a value that indicates how parameters are referenced.
        /// </summary>
        public ReferenceKind ReferenceKind { get; set; }
    }
}
