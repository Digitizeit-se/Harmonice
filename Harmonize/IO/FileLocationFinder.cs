using Microsoft.Extensions.Logging;

namespace Harmonize.IO;
internal class FileLocationFinder
{
    protected readonly ILogger Logger;
    protected readonly string? AppRoot;
    private FileLocationFinder? _nextFinder;
    protected readonly bool IsDebug;
    protected string? PluginLocation;

    protected FileLocationFinder(ILogger logger,string? appRoot, FileLocationFinder? nextFinder = null, bool isDebug = false )
    {
        Logger = logger;
        AppRoot = appRoot;
        _nextFinder = nextFinder;
        IsDebug = isDebug;
    }

    /// <summary>
    /// Find file location calls FindFileLogic on all chained classes until the file is found.
    /// </summary>
    /// <param name="rootFolder">The plugin root folder</param>
    /// <param name="assemblyName">The file assembly name</param>
    /// <returns>string file path if file is found</returns>
    /// <exception cref="FileNotFoundException">thrown if file is not found calling FindFileLogic on all chained classes</exception>
    public string FindFileLocation(string? rootFolder, string assemblyName)
    {
        if(string.IsNullOrEmpty(assemblyName))
            throw new ArgumentException("Assembly name can´t be null or empty", nameof(assemblyName));

        //run logic to find file
        FindFileLogic(rootFolder,assemblyName);
        
        //if pluginLocation is not null then return it
        if (!string.IsNullOrEmpty(PluginLocation)) return PluginLocation;
        
        while (_nextFinder != null)
        {
            //Call Find file logic on next finder
            _nextFinder?.FindFileLogic(rootFolder, assemblyName);

            //if next finder pluginLocation is not null then return it
            if (!string.IsNullOrEmpty(_nextFinder.PluginLocation)) return _nextFinder.PluginLocation;

            //Set next finder from next finder
            _nextFinder = _nextFinder!._nextFinder;
        }

        //If not found at all then throw exception
        Logger.LogError($"Plug-in file: {assemblyName}.dll  not found in plug-in path. ");
        throw new FileNotFoundException($"Plug-in file: {assemblyName}.dll  not found in any path. ");

    }

    /// <summary>
    /// Method to run logic to find file for child class to implement.
    /// </summary>
    /// <param name="rootFolder">Tbe plugin root folder</param>
    /// <param name="assemblyName">The file assembly name</param>
    public virtual void FindFileLogic(string? rootFolder, string assemblyName){}
}