using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Potter.Reflection.Tests
{
    [TestClass]
    public class AssemblyLoaderTests
    {
        private const string DistantAssemblyLocation = @"C:\Program Files (x86)\Windows Kits\10\References\10.0.16299.0\Windows.Media.Capture.CameraCaptureUIContract\1.0.0.0\Windows.Media.Capture.CameraCaptureUIContract.winmd";

        [TestMethod]
        public void Load_CurrentAssembly()
        {
            using (var assemblyLoader = new AssemblyLoader())
            {
                // Arrange
                Assembly currentAssembly = Assembly.GetExecutingAssembly();

                // Act
                loadAssembly(assemblyLoader, currentAssembly.Location);
            }
        }

        [TestMethod]
        public void Load_OtherAssembly()
        {
            using (var assemblyLoader = new AssemblyLoader())
            {
                // Arrange
                Assembly otherAssembly = typeof(Assert).Assembly;

                // Act
                loadAssembly(assemblyLoader, otherAssembly.Location);
            }
        }

        [TestMethod]
        public void Load_WinMDAssembly()
        {
            using (var assemblyLoader = new AssemblyLoader())
            {
                // Arrange
                string assemblyLocation = DistantAssemblyLocation;

                // Act
                loadAssembly(assemblyLoader, assemblyLocation);
            }
        }

        [TestMethod]
        public void Load_RemoteCurrentAssembly()
        {
            using (var remoteLoader = RemoteAssemblyLoader.Create())
            {
                // Arrange
                Assembly currentAssembly = Assembly.GetExecutingAssembly();

                // Act
                loadAssembly(remoteLoader.AssemblyLoader, currentAssembly.Location);
            }
        }

        [TestMethod]
        public void Load_RemoteOtherAssembly()
        {
            using (var remoteLoader = RemoteAssemblyLoader.Create())
            {
                // Arrange
                Assembly otherAssembly = typeof(Assert).Assembly;

                // Act
                loadAssembly(remoteLoader.AssemblyLoader, otherAssembly.Location);
            }
        }

        [TestMethod]
        public void Load_RemoteWinMDAssembly()
        {
            using (var remoteLoader = RemoteAssemblyLoader.Create())
            {
                // Arrange
                string assemblyLocation = DistantAssemblyLocation;

                // Act
                loadAssembly(remoteLoader.AssemblyLoader, assemblyLocation);
            }
        }

        private static void loadAssembly(AssemblyLoader assemblyLoader, string assemblyLocation)
        {
            try
            {
                // Act
                Assembly actualAssembly = assemblyLoader.LoadFile(assemblyLocation);

                // Assert
                Assert.IsNotNull(actualAssembly);
                Assert.IsNotNull(actualAssembly.GetTypes());
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                var uniqueMessages = typeLoadException.LoaderExceptions
                    .Select(exception => exception.Message)
                    .Distinct();

                foreach (var message in uniqueMessages)
                {
                    Debug.WriteLine(message);
                }

                throw;
            }
            catch (System.IO.FileLoadException fileLoadException)
            {
                Debug.WriteLine(fileLoadException.Message);
                Debug.WriteLine(fileLoadException.FileName);
                Debug.WriteLine(fileLoadException.FusionLog);
                throw;
            }
        }
    }
}
