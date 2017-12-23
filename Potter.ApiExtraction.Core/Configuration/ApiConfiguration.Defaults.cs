using System.Reflection;

namespace Potter.ApiExtraction.Core.Configuration
{
    partial class ApiConfiguration
    {
        /// <summary>
        ///     Creates an <see cref="ApiConfiguration"/> instance that will allow the generation of
        ///     all types within an assembly with the specified name.
        /// </summary>
        /// <param name="assemblyName">
        ///     The name of the assembly.
        /// </param>
        /// <returns>
        ///     A new <see cref="ApiConfiguration"/> instance.
        /// </returns>
        public static ApiConfiguration CreateDefault(AssemblyName assemblyName)
        {
            return new ApiConfiguration
            {
                SimplifyNamespaces = true,
                Assemblies = new AssemblyConfiguration[]
                {
                    new AssemblyConfiguration
                    {
                        Name = assemblyName.FullName,
                    },
                },
                Groups = new GroupConfiguration[]
                {
                    new GroupConfiguration
                    {
                        Name = assemblyName.Name,
                        Types = new TypeConfiguration
                        {
                            Mode = TypeMode.Blacklist,
                        },
                    },
                },
            };
        }
    }
}
