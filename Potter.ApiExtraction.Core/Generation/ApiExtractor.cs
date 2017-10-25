using System.Collections.Generic;
using Potter.ApiExtraction.Core.Generation;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Extracts API information from a source.
    /// </summary>
    public class ApiExtractor : IApiExtractor
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
        public IEnumerable<IApiType> Read(IApiConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}
