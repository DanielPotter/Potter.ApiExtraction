using System;
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

        public Assembly Load(string assemblyFile)
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
            string path = WindowsRuntimeMetadata.ResolveNamespace(args.NamespaceName, Enumerable.Empty<string>()).FirstOrDefault();

            if (path == null)
            {
                return;
            }

            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(path);

            args.ResolvedAssemblies.Add(assembly);
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
