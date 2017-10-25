using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    /// <summary>
    ///     Provides access to indexer property metadata.
    /// </summary>
    /// <remarks>
    ///     This interface estimates the indexer responsibilities of the
    ///     <see cref="System.Reflection.PropertyInfo"/> class.
    /// </remarks>
    public interface IApiIndexer : IApiProperty
    {
        /// <summary>
        ///     Gets the index parameters for the property.
        /// </summary>
        IReadOnlyList<IApiParameter> Parameters { get; }
    }
}
