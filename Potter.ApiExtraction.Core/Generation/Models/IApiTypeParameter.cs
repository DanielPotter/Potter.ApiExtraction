using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about generic type parameters.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the generic type parameter responsibilities of the
    ///     <see cref="System.Type"/> class.
    /// </remarks>
    public interface IApiTypeParameter : IApiTypeBase
    {
        /// <summary>
        ///     Gets the method that represents the declaring method, if the current type represents
        ///     a type parameter of a generic method.
        /// </summary>
        IApiMethodBase DeclaringMethod { get; }

        /// <summary>
        ///     Returns a list of type objects that represent the constraints on the current generic
        ///     type parameter.
        /// </summary>
        IReadOnlyList<IApiTypeBase> GenericTypeConstraints { get; }
    }
}
