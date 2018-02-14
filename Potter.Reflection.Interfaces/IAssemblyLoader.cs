using System;
using System.Reflection;

namespace Potter.Reflection.Interfaces
{
    public interface IAssemblyLoader : IDisposable
    {
        Assembly Load(string assemblyName);

        Assembly LoadFile(string assemblyPath);
    }
}
