using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Potter.ApiExtraction.Core.Configuration;
using Potter.ApiExtraction.Core.Generation;
using Potter.Reflection;

namespace Potter.ApiExtraction.Core.Utilities
{
    public static class ApiTypeReaderExtensions
    {
        #region Configuration Reading

        public static IEnumerable<CompilationUnitSyntax> Read(this ApiTypeReader apiTypeReader, string configurationFile)
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

        public static IEnumerable<CompilationUnitSyntax> Read(this ApiTypeReader apiTypeReader, ApiConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string assemblyLocation = configuration.Assembly.Location;

            Assembly assembly;

            using (var assemblyLoader = new AssemblyLoader())
            {
                if (assemblyLocation != null)
                {
                    assembly = assemblyLoader.LoadFile(assemblyLocation);
                }
                else
                {
                    assembly = assemblyLoader.Load(configuration.Assembly.Name);
                }
            }

            return apiTypeReader.ReadAssembly(assembly, configuration: configuration);
        }

        public static IEnumerable<string> ReadNormalizedString(this ApiTypeReader apiTypeReader, string configurationFile)
        {
            foreach (var unit in Read(apiTypeReader, configurationFile))
            {
                yield return unit.NormalizeWhitespace().ToString();
            }
        }

        public static IEnumerable<string> ReadNormalizedString(this ApiTypeReader apiTypeReader, ApiConfiguration configuration)
        {
            foreach (var unit in Read(apiTypeReader, configuration))
            {
                yield return unit.NormalizeWhitespace().ToString();
            }
        }

        #endregion
    }
}
