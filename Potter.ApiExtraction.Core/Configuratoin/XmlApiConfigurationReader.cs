using System.Xml;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Reads API extraction configuration from XML.
    /// </summary>
    public class XmlApiConfigurationReader : IXmlApiConfigurationReader
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
        public IApiConfiguration Parse(XmlReader xmlReader)
        {
            throw new System.NotImplementedException();
        }
    }
}
