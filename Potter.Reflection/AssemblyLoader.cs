using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Potter.Reflection
{
    // Reference:
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/56a79592-4e11-4f4b-9b0b-73905b56bfe0/how-to-load-an-assembly-in-new-appdomain?forum=clr (11/19/2017)
    // https://stackoverflow.com/q/35249342/2503153 (11/19/2017)

    public class AssemblyLoader : MarshalByRefObject, IDisposable
    {
        public AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += resolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveAssembly;
            WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve += resolveNamespace;
        }

        public virtual void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= resolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveAssembly;
            WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve -= resolveNamespace;
        }

        public Assembly Load(string assemblyName)
        {
            var name = new AssemblyName(assemblyName);

            Assembly assembly;
            switch (name.ContentType)
            {
                default:
                    assembly = Assembly.ReflectionOnlyLoad(assemblyName);
                    break;

                case AssemblyContentType.WindowsRuntime:
                    assembly = resolveAssemblyFromNamespace(assemblyName);
                    break;
            }

            // Force dependency resolution.
            assembly.GetTypes();

            return assembly;
        }

        public Assembly LoadFile(string assemblyFile)
        {
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

            return assembly;
        }

        private Assembly resolveAssembly(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        private void resolveNamespace(object sender, NamespaceResolveEventArgs args)
        {
            Assembly assembly = resolveAssemblyFromNamespace(args.NamespaceName);

            if (assembly != null)
            {
                args.ResolvedAssemblies.Add(assembly);
            }
        }

        private static Assembly resolveAssemblyFromNamespace(string namespaceName)
        {
            string path = WindowsRuntimeMetadata.ResolveNamespace(namespaceName, Enumerable.Empty<string>()).FirstOrDefault();

            var expectedAssemblyName = new AssemblyName(namespaceName);
            if (path == null)
            {
                path = resolveAssemblyLocation(expectedAssemblyName);
            }
            else
            {
                var resolvedAssemblyName = AssemblyName.GetAssemblyName(path);

                // Unfortunately, this happens a lot.
                if (expectedAssemblyName.Name != resolvedAssemblyName.Name)
                {
                    string newPath = resolveAssemblyLocation(expectedAssemblyName);

                    if (newPath != null)
                    {
                        path = newPath;
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Could not resolve assembly: {namespaceName}. Defaulting to {path}");
                    }
                }
            }

            if (path == null)
            {
                return null;
            }

            return Assembly.ReflectionOnlyLoadFrom(path);
        }

        // TODO: Remove this hard-coded path. (Daniel Potter, 11/21/2017)
        private static readonly string WindowsApiContractsFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\10\References");

        private static string resolveAssemblyLocation(AssemblyName assemblyName)
        {
            Console.WriteLine($"Resolving assembly: {assemblyName.FullName}");

            var contractsFolder = new DirectoryInfo(WindowsApiContractsFolder);

            foreach (var versionFolder in contractsFolder.EnumerateDirectories("10.*"))
            {
                string path = searchContracts(versionFolder);

                if (path != null)
                {
                    return path;
                }
            }

            return searchContracts(contractsFolder);

            string searchContracts(DirectoryInfo directoryInfo)
            {
                Console.WriteLine($"Searching: {directoryInfo.FullName} For: {assemblyName.Name}");
                foreach (var contractFolder in directoryInfo.EnumerateDirectories(assemblyName.Name))
                {
                    Console.WriteLine($"Searching: {contractFolder.FullName} For: {assemblyName.Version.ToString()}");
                    foreach (var apiVersionFolder in contractFolder.EnumerateDirectories(assemblyName.Version.ToString()))
                    {
                        Console.WriteLine($"Searching: {apiVersionFolder.FullName} For: {assemblyName.Name}");
                        string path = WindowsRuntimeMetadata.ResolveNamespace(assemblyName.Name, apiVersionFolder.FullName, Enumerable.Empty<string>()).FirstOrDefault();

                        if (path != null)
                        {
                            Console.WriteLine($"Located: {path}");
                            return path;
                        }
                    }
                }

                return null;
            }
        }
    }

    public class RemoteAssemblyLoader : IDisposable
    {
        // TODO: This class is ineffective when loading WinMD files. (Daniel Potter, 11/20/2017)

        private static readonly string _assemblyName = typeof(AssemblyLoader).Assembly.FullName;
        private static readonly string _assemblyLocation = System.IO.Path.GetDirectoryName(typeof(AssemblyLoader).Assembly.Location);
        private static readonly string _assemblyLoaderTypeName = typeof(AssemblyLoader).FullName;

        public static RemoteAssemblyLoader Create()
        {
            AppDomain appDomain = AppDomain.CreateDomain(friendlyName: "Volatile", securityInfo: null,
                info: new AppDomainSetup
                {
                    ApplicationBase = _assemblyLocation,
                });

            try
            {
                var remoteLoader = (AssemblyLoader) appDomain.CreateInstanceAndUnwrap(
                    assemblyName: _assemblyName, typeName: _assemblyLoaderTypeName);

                return new RemoteAssemblyLoader(appDomain, remoteLoader);
            }
            catch
            {
                AppDomain.Unload(appDomain);
                throw;
            }
        }

        private RemoteAssemblyLoader(AppDomain appDomain, AssemblyLoader assemblyLoader)
        {
            AppDomain = appDomain;
            AssemblyLoader = assemblyLoader;
        }

        public AppDomain AppDomain { get; private set; }

        public AssemblyLoader AssemblyLoader { get; private set; }

        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    AssemblyLoader.Dispose();
                    AppDomain.Unload(AppDomain);
                }

                // Set large fields to null.
                AppDomain = null;
                AssemblyLoader = null;

                _isDisposed = true;
            }
        }

        // This code was added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
