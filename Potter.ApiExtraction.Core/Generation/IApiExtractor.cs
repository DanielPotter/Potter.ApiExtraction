using System.Collections.Generic;
using Potter.ApiExtraction.Core.Generation;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Extracts API information from a source.
    /// </summary>
    public interface IApiExtractor
    {
        /// <summary>
        ///     Gets API types from a source specified by the configuration.
        /// </summary>
        /// <param name="configuration">
        ///     The extraction configuration.
        /// </param>
        /// <returns>
        ///     A sequence of types extracted from the source.
        /// </returns>
        IEnumerable<IApiType> Read(IApiConfiguration configuration);
    }
}
