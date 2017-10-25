using Microsoft.VisualStudio.TestTools.UnitTesting;
using Potter.ApiExtraction.Core.Generation;
using SimpleInjector;

namespace Potter.ApiExtraction.Core.Tests
{
    [TestClass]
    public class ApiGeneratorTests
    {
        [TestMethod]
        public void Write_SimpleInterface()
        {
            // Arrange
            string expected = @"namespace Test
{
    public interface ITestInterface
    {
        string HelloWorld();
    }
}";

            var typeModel = new MutableApiType
            {
                Access = MemberAccess.Public,
                ExtensionModifier = ExtensionModifers.Abstract,
                Name = "ITestInterface",
                Namespace = "Test",
                Kind = TypeKind.Interface,
                Members =
                {
                    new MutableApiMethod
                    {
                        Access = MemberAccess.Public,
                        ExtensionModifier = ExtensionModifers.Abstract | ExtensionModifers.Virtual,
                        Name = "HelloWorld",
                        ReturnParameter = new MutableApiParameter
                        {
                            ParameterType = new MutableApiType
                            {
                                Name = "String",
                                Namespace = "System",
                            },
                        },
                    },
                },
            };

            var container = new Container().InstallApiExtraction();
            var typeWriter = new MockTypeWriter();

            var generator = container.GetInstance<IApiGenerator>();

            // Act
            generator.Write(typeModel, typeWriter);

            string actual = typeWriter.GetContent(typeModel.FullName);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
