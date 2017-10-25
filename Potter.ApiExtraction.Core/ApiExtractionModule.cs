using SimpleInjector;

namespace Potter.ApiExtraction.Core
{
    /// <summary>
    ///     Provides static methods for installing the API Extraction services into a
    ///     <see cref="Container"/>.
    /// </summary>
    public static class ApiExtractionModule
    {
        /// <summary>
        ///     Installs the API Extraction services into a container.
        /// </summary>
        /// <param name="container">
        ///     The container into which the services should install.
        /// </param>
        /// <returns>
        ///     The input <paramref name="container"/>.
        /// </returns>
        public static Container InstallApiExtraction(this Container container)
        {
            container.Register<IApiExtractor, ApiExtractor>(Lifestyle.Transient);
            container.Register<IApiGenerator, ApiGenerator>(Lifestyle.Transient);
            container.Register<IXmlApiConfigurationReader, XmlApiConfigurationReader>(Lifestyle.Transient);

            return container;
        }
    }
}
