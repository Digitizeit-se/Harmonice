using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

[assembly: InternalsVisibleTo("Harmonize.UnitTest")]
namespace Harmonize;

internal class LoaderContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dllPath"> The path to the component or plugin's managed entry point.</param>
    public LoaderContext(string dllPath)
    {
        _resolver = new AssemblyDependencyResolver(dllPath);
    }

    /// <summary>
    /// Loads the assembly from the specified path.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly</param>
    /// <returns>The loaded assembly if success else null</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : default;
    }

    /// <summary>
    /// Loads the unmanaged DLL from the specified path.
    /// </summary>
    /// <param name="unmanagedDllName">The dll name</param>
    /// <returns>The loaded library if success else empty <see cref="IntPtr"/></returns>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
