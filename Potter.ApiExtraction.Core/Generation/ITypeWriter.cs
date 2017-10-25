using System.IO;
using Potter.ApiExtraction.Core.Generation;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Opens streams for writing source for a type.
    /// </summary>
    public interface ITypeWriter
    {
        /// <summary>
        ///     Opens a text writer for a specified type.
        /// </summary>
        /// <param name="type">
        ///     The type to write.
        /// </param>
        /// <returns>
        ///     A new <see cref="TextWriter"/> for the source code of <paramref name="type"/>.
        /// </returns>
        TextWriter WriteType(IApiType type);

        /// <summary>
        ///     Opens a text reader for a specified type if possible.
        /// </summary>
        /// <param name="type">
        ///     The type to read.
        /// </param>
        /// <param name="reader">
        ///     When this method returns, contains a text reader for the specified type if
        ///     available; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a text reader is available for <paramref name="type"/>; otherwise,
        ///     <c>false</c>.
        /// </returns>
        bool TryReadType(IApiType type, out TextReader reader);
    }
}
