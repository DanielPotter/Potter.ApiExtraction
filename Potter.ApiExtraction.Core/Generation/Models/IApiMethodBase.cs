using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides information about methods and constructors.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the <see cref="System.Reflection.MethodBase"/> class.
    /// </remarks>
    public interface IApiMethodBase : IApiMember
    {
        /// <summary>
        ///     Gets the parameters of the specified method or constructor.
        /// </summary>
        IReadOnlyList<IApiParameter> Parameters { get; }

        /// <summary>
        ///     Gets a value indicating whether the generic method contains unassigned generic type
        ///     parameters.
        /// </summary>
        bool ContainsGenericParameters { get; }

        /// <summary>
        ///     Gets the type parameters of a generic method definition.
        /// </summary>
        IReadOnlyList<IApiTypeParameter> GenericTypeParameters { get; }
    }
}
