using Microsoft.VisualStudio.TestTools.UnitTesting;
using Potter.ApiExtraction.Core.Generation;
using SimpleInjector;

namespace Potter.ApiExtraction.Core.Tests
{
    [TestClass]
    public partial class IntegrationTests
    {
        [TestMethod]
        public void GenerateSourceCode()
        {
            Assert.Inconclusive();

            // Arrange
            string selectionXmlContent = Properties.Resources.ApiSelection;
            var configDocument = System.Xml.Linq.XDocument.Parse(selectionXmlContent);

            var container = new Container().InstallApiExtraction();

            var configurationReader = container.GetInstance<IXmlApiConfigurationReader>();
            IApiConfiguration configuration = configurationReader.Parse(configDocument.CreateReader());

            var reader = container.GetInstance<IApiExtractor>();
            reader.Read(configuration);

            var generator = container.GetInstance<IApiGenerator>();

            ITypeWriter writer = null;
            IApiType type = null;

            // Act
            generator.Write(type, writer);

            // Assert
        }
    }
}
