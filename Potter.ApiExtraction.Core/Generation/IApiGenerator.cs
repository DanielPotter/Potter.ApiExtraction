using Potter.ApiExtraction.Core.Generation;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Generates interfaces for an API.
    /// </summary>
    public interface IApiGenerator
    {
        /// <summary>
        ///     Writes interface source code for a type.
        /// </summary>
        /// <param name="type">
        ///     The type for which to generate source code.
        /// </param>
        /// <param name="writer">
        ///     The object to write the source code.
        /// </param>
        /// <param name="documentationResolver">
        ///     An object to resolve source documentation.
        /// </param>
        void Write(IApiType type, ITypeWriter writer, IDocumentationResolver documentationResolver = null);
    }
}
