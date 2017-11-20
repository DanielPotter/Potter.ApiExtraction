using System;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Potter.Reflection
{
    // Reference:
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/56a79592-4e11-4f4b-9b0b-73905b56bfe0/how-to-load-an-assembly-in-new-appdomain?forum=clr (11/19/2017)
    // https://stackoverflow.com/q/35249342/2503153 (11/19/2017)

    public class AssemblyLoader : MarshalByRefObject
    {
        private static readonly string _currentAssemblyName = Assembly.GetExecutingAssembly().Location;
        private static readonly string _assemblyLoaderTypeName = typeof(AssemblyLoader).FullName;

        public static Assembly Load(string assemblyFile)
        {
            AppDomain appDomain = AppDomain.CreateDomain("Volatile");

            try
            {
                var remoteLoader = (AssemblyLoader) appDomain.CreateInstanceAndUnwrap(
                    _currentAssemblyName, _assemblyLoaderTypeName);

                return remoteLoader.load(assemblyFile);
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        private Assembly load(string assemblyFile)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveAssembly;
            WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve += resolveNamespace;

            try
            {
                Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

                return assembly;
            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveAssembly;
                WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve -= resolveNamespace;
            }
        }

        private Assembly resolveAssembly(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        private void resolveNamespace(object sender, NamespaceResolveEventArgs args)
        {
            foreach (string path in WindowsRuntimeMetadata.ResolveNamespace(args.NamespaceName, Array.Empty<string>()))
            {
                args.ResolvedAssemblies.Add(Assembly.ReflectionOnlyLoadFrom(path));
            }
        }
    }
}
