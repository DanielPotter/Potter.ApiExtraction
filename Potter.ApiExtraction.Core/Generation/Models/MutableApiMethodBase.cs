using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about methods and constructors.
    /// </summary>
    public abstract class MutableApiMethodBase : MutableApiMember, IApiMethodBase
    {
        /// <summary>
        ///     Gets the parameters of the specified method or constructor.
        /// </summary>
        public List<IApiParameter> Parameters { get; set; } = new List<IApiParameter>();

        IReadOnlyList<IApiParameter> IApiMethodBase.Parameters => Parameters;

        /// <summary>
        ///     Gets a value indicating whether the generic method contains unassigned generic type
        ///     parameters.
        /// </summary>
        public bool ContainsGenericParameters => GenericTypeParameters.Count > 0;

        /// <summary>
        ///     Gets the type parameters of a generic method definition.
        /// </summary>
        public List<IApiTypeParameter> GenericTypeParameters { get; set; } = new List<IApiTypeParameter>();

        IReadOnlyList<IApiTypeParameter> IApiMethodBase.GenericTypeParameters => GenericTypeParameters;
    }
}
