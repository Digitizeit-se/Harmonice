using Harmonize.IO;
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
            if (_debug) _logger.LogDebug($"{assemblyName} loaded from cache.");

            //Return the loaded assembly
            return LoadedAssemblies[assemblyName];
        }


        //Chain file finders to find file
        var pathFinder = ChainFileFinders(_debug);

        //Try to get file location
        var pluginLocation = pathFinder.FindFileLocation(rootFolder, assemblyName);

        var loadContext = new LoaderContext(pluginLocation);
        if (_debug) _logger.LogDebug($"loading assembly: {assemblyName} from path: {pluginLocation}");
        var assembly =
            loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));

        if (_debug) _logger.LogDebug($"Adding assembly {assembly.FullName} to cache.");
        LoadedAssemblies.Add(assemblyName, assembly);

        return assembly;
    }

    /// <summary>
    /// Chain file finders
    /// </summary>
    /// <returns>FileLocationFinder with chained file finders</returns>
    private static FileLocationFinder ChainFileFinders(bool isDebug)
    {
        //Get the execution / Entry application paths
        var entryRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var executingRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        return new FindFileInPluginNameFolder(
            _logger!, entryRoot, new FindFileInPluginNameFolder(
                _logger!, executingRoot, new FindFileInPluginRootFolder(
                    _logger!, entryRoot, new FindFileInPluginRootFolder(
                        _logger!, executingRoot, new FindFileInApplicationFolder(
                            _logger!, entryRoot, new FindFileInApplicationFolder(
                                _logger!, executingRoot,null,isDebug:isDebug),isDebug),isDebug),isDebug),isDebug),isDebug);
    }
}   