using Microsoft.Extensions.Logging;

namespace Harmonize.IO;

internal class FindFileInPluginRootFolder : FileLocationFinder
{
    /// <summary>
    /// Constructor to set base properties
    /// </summary>
    /// <param name="logger">ILogger for debug logging</param>
    /// <param name="appRoot">The app root folder</param>
    /// <param name="nextFinder">Next class for file search</param>
    /// <param name="isDebug">If true debug logging is enabled</param>
    public FindFileInPluginRootFolder(ILogger logger, string? appRoot, FileLocationFinder? nextFinder = null, bool isDebug = false) : base(logger, appRoot, nextFinder,isDebug) { }

    /// <summary>
    /// Check if file exists in plugin root folder, if so set plugin location
    /// </summary>
    /// <param name="rootFolder">The plugin root folder</param>
    /// <param name="assemblyName">The file assembly name</param>
    public override void FindFileLogic(string? rootFolder, string assemblyName)
    {
        //early return if rootFolder is null or empty
        if (string.IsNullOrEmpty(AppRoot)) return;

        //Try to find dll file in folder with same name as assembly
        var testLocation = Path.GetFullPath(Path.Combine(AppRoot, Path.Combine(rootFolder, $"{assemblyName}.dll"))).Replace('\\', Path.DirectorySeparatorChar);

        //debug logger
        if (IsDebug) Logger!.LogDebug($"look for Plug-in file: {assemblyName}.dll in path: {testLocation}");

        //Check if file exists
        if (File.Exists(testLocation))
        {
            //File exists so set plugin location
            PluginLocation = testLocation;
        }
    }
}