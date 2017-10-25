using System.Xml;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Reads API extraction configuration from XML.
    /// </summary>
    public interface IXmlApiConfigurationReader
    {
        /// <summary>
        ///     Reads API extraction configuration from XML.
        /// </summary>
        /// <param name="xmlReader">
        ///     The XML reader.
        /// </param>
        /// <returns>
        ///     The API extraction configuration.
        /// </returns>
        IApiConfiguration Parse(XmlReader xmlReader);
    }
}
