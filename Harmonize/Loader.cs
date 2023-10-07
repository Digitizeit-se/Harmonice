using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
namespace Harmonize;

/// <summary>
/// Assembly loader
/// </summary>
internal static class Loader
{
    /// <summary>
    /// A Collection of all the loaded assemblies as plugin´s to not load them again
    /// </summary>
    private static readonly Dictionary<string, Assembly> LoadedAssemblies = new();

    /// <summary>
    /// Logger set to "null logger" if no ILogger is found
    /// </summary>
    private static ILogger? _logger;

    /// <summary>
    /// Is debug enabled
    /// </summary>
    private static bool _debug;

    /// <summary>
    /// Loads the assembly from the specified path.
    /// </summary>
    /// <param name="rootFolder">Name of containing folder in application root folder</param>
    /// <param name="assemblyName">Assembly name of dll to load</param>
    /// <param name="services">The IServiceProvider to get logger from</param>
    /// <exception cref="FileNotFoundException">If file can´t be  found </exception>
    /// <returns>The loaded assembly </returns>
    public static Assembly Load(string rootFolder, string assemblyName, IServiceProvider services)
    {
        //Create new logger if null
        if (_logger is null)
        {
            //Get logger factory from service provider
            var loggerFactory = services.GetService<ILoggerFactory>();
            
            //Create logger , if not found create NullLogger
            _logger = loggerFactory?.CreateLogger("Loader")
                         ?? new NullLoggerFactory().CreateLogger("Loader");
            
            //Get if is debug enabled
            _debug = _logger.IsEnabled(LogLevel.Debug); //or any other level
        }

        //Look if we already loaded the assembly earlier
        if (LoadedAssemblies.ContainsKey(assemblyName))
        {
            if(_debug) _logger.LogDebug($"{assemblyName} loaded from cache.");

            //Return the loaded assembly
            return LoadedAssemblies[assemblyName];
        }

        //Get file location
        var pluginLocation = FindFileLocation(rootFolder, assemblyName);

        var loadContext = new LoaderContext(pluginLocation);
        if (_debug) _logger.LogDebug($"loading assembly: {assemblyName} from path: {pluginLocation}");
        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        
        if (_debug) _logger.LogDebug($"Adding assembly {assembly.FullName} to cache.");
        LoadedAssemblies.Add(assemblyName, assembly);

        return assembly;
    }

    /// <summary>
    /// Find assembly location
    /// </summary>
    /// <param name="rootFolder">The plugin root folder </param>
    /// <param name="assemblyName">The file assembly name</param>
    /// <returns>file path if found</returns>
    /// <exception cref="FileNotFoundException">when file can´t be found</exception>
    private static string FindFileLocation(string rootFolder, string assemblyName)
    {
        var pluginLocation = string.Empty;
        var fileFound = false;

        //Get the execution / Entry application paths
        var entryRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var executingRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        if (!fileFound)
        {
            if (entryRoot != null)
                fileFound = FindFileInPluginNameFolder(entryRoot, rootFolder, assemblyName, out pluginLocation);
        }

        if (!fileFound)
        {
            if (executingRoot != null)
                fileFound = FindFileInPluginNameFolder(executingRoot, rootFolder, assemblyName, out pluginLocation);
        }

        if (!fileFound)
        {
            if (entryRoot != null)
                fileFound = FindFileInPluginRootFolder(entryRoot, rootFolder, assemblyName, out pluginLocation);
        }

        if (!fileFound)
        {
            if (executingRoot != null)
                fileFound = FindFileInPluginRootFolder(executingRoot, rootFolder, assemblyName, out pluginLocation);
        }

        if (!fileFound)
        {
            if (entryRoot != null) fileFound = FindFileInApplicationFolder(entryRoot, assemblyName, out pluginLocation);
        }

        if (!fileFound)
        {
            if (executingRoot != null)
                fileFound = FindFileInApplicationFolder(executingRoot, assemblyName, out pluginLocation);
        }

        if (fileFound) return pluginLocation;

        //If not found at all then throw exception
        _logger!.LogError($"Plug-in file: {assemblyName}.dll  not found in plug-in path. ");
        throw new FileNotFoundException($"Plug-in file: {assemblyName}.dll  not found in any path. ");
    }


    /// <summary>
    /// Validate if file exists in plugin name folder
    /// </summary>
    /// <param name="appRoot">Application root folder </param>
    /// <param name="rootFolder">Plugins root folder </param>
    /// <param name="assemblyName">The dll file </param>
    /// <param name="pluginLocation">Out path if file was found</param>
    /// <returns>true if found else false</returns>
    private static bool FindFileInPluginNameFolder(string appRoot,string rootFolder, string assemblyName, out string pluginLocation)
    {
        //Try to find dll file in folder with same name as assembly
        pluginLocation = Path.GetFullPath(Path.Combine(appRoot, Path.Combine(Path.Combine(rootFolder, assemblyName), $"{assemblyName}.dll"))).Replace('\\', Path.DirectorySeparatorChar);

        if (_debug) _logger!.LogDebug($"look for Plug-in file: {assemblyName}.dll in path: {pluginLocation}");
        
        return File.Exists(pluginLocation);

    }

    /// <summary>
    /// Validate if file exists in plugin root folder
    /// </summary>
    /// <param name="appRoot">Application root folder </param>
    /// <param name="rootFolder">Plugins root folder </param>
    /// <param name="assemblyName">The dll file </param>
    /// <param name="pluginLocation">Out path if file was found</param>
    /// <returns>true if found else false</returns>
    private static bool FindFileInPluginRootFolder(string appRoot, string rootFolder, string assemblyName, out string pluginLocation)
    {
        //Try to find dll file in folder with same name as assembly
        pluginLocation = Path.GetFullPath(Path.Combine(appRoot, Path.Combine(rootFolder, $"{assemblyName}.dll"))).Replace('\\', Path.DirectorySeparatorChar);

        if (_debug) _logger!.LogDebug($"look for Plug-in file: {assemblyName}.dll in path: {pluginLocation}");

        return File.Exists(pluginLocation);

    }

    /// <summary>
    /// Validate if file exists in application folder
    /// </summary>
    /// <param name="appRoot">Application root folder </param>
    /// <param name="assemblyName">The dll file </param>
    /// <param name="pluginLocation">Out path if file was found</param>
    /// <returns>true if found else false</returns>
    private static bool FindFileInApplicationFolder(string appRoot, string assemblyName, out string pluginLocation)
    {
        //Try to find dll file in folder with same name as assembly
        pluginLocation = Path.GetFullPath(Path.Combine(appRoot, $"{assemblyName}.dll")).Replace('\\', Path.DirectorySeparatorChar);

        if (_debug) _logger!.LogDebug($"look for Plug-in file: {assemblyName}.dll in path: {pluginLocation}");

        return File.Exists(pluginLocation);

    }
}
