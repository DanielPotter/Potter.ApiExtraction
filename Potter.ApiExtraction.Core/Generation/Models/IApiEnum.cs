using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about enumeration types.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the enum responsibilities of the <see cref="System.Type"/>
    ///     class.
    /// </remarks>
    public interface IApiEnum : IApiTypeBase
    {
        /// <summary>
        ///     Returns the underlying type of the current enumeration type.
        /// </summary>
        IApiTypeBase UnderlyingType { get; }

        /// <summary>
        ///     Gets the constants in the current enumeration type.
        /// </summary>
        IReadOnlyList<IApiEnumField> Members { get; }
    }
}
