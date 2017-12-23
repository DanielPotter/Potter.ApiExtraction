using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Potter.ApiExtraction.Core.Configuration;
using Potter.ApiExtraction.Core.Generation;
using Potter.Reflection;

namespace Potter.ApiExtraction.Core.Utilities
{
    public static class ApiTypeReaderExtensions
    {
        #region Configuration Reading

        public static IEnumerable<ApiExtractionResult> ReadResults(this ApiTypeReader apiTypeReader, string configurationFile)
        {
            if (apiTypeReader == null)
            {
                throw new ArgumentNullException(nameof(apiTypeReader));
            }

            if (configurationFile == null)
            {
                throw new ArgumentNullException(nameof(configurationFile));
            }

            foreach (var sourceFileInfo in Read(apiTypeReader, configurationFile))
            {
                string sourceCode = sourceFileInfo.CompilationUnit.NormalizeWhitespace().ToFullString();

                yield return new ApiExtractionResult(
                    name: sourceFileInfo.Name,
                    namespaceName: sourceFileInfo.NamespaceName,
                    group: sourceFileInfo.Group,
                    sourceCode: sourceCode);
            }
        }

        public static IEnumerable<SourceFileInfo> Read(this ApiTypeReader apiTypeReader, string configurationFile)
        {
            if (string.IsNullOrEmpty(configurationFile))
            {
                throw new ArgumentException($"{nameof(configurationFile)} cannot be null or empty.", nameof(configurationFile));
            }

            // Reference: https://stackoverflow.com/q/25991963/2503153 (11/18/2017)
            var xmlReaderSettings = new System.Xml.XmlReaderSettings();

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ApiConfiguration));

            using (var reader = System.Xml.XmlReader.Create(configurationFile, xmlReaderSettings))
            {
                var apiConfiguration = (ApiConfiguration) serializer.Deserialize(reader);

                return Read(apiTypeReader, apiConfiguration);
            }
        }

        public static IEnumerable<SourceFileInfo> Read(this ApiTypeReader apiTypeReader, ApiConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var typeNameResolver = new TypeNameResolver(configuration);

            foreach (var assemblyConfiguration in configuration.Assemblies)
            {
                string assemblyLocation = assemblyConfiguration.Location;

                Assembly assembly;

                using (var assemblyLoader = new AssemblyLoader())
                {
                    if (assemblyLocation != null)
                    {
                        assembly = assemblyLoader.LoadFile(assemblyLocation);
                    }
                    else
                    {
                        assembly = assemblyLoader.Load(assemblyConfiguration.Name);
                    }
                }

                foreach (var sourceFileInfo in apiTypeReader.ReadAssembly(assembly, configuration, typeNameResolver))
                {
                    yield return sourceFileInfo;
                }
            }
        }

        public static IEnumerable<string> ReadNormalizedString(this ApiTypeReader apiTypeReader, string configurationFile)
        {
            foreach (var unit in Read(apiTypeReader, configurationFile))
            {
                yield return unit.CompilationUnit.NormalizeWhitespace().ToString();
            }
        }

        public static IEnumerable<string> ReadNormalizedString(this ApiTypeReader apiTypeReader, ApiConfiguration configuration)
        {
            foreach (var unit in Read(apiTypeReader, configuration))
            {
                yield return unit.CompilationUnit.NormalizeWhitespace().ToString();
            }
        }

        #endregion
    }
}
