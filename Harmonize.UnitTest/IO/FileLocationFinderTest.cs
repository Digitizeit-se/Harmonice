using Harmonize.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using FluentAssertions;

namespace Harmonize.UnitTest.IO;

public class FileLocationFinderTest
{
    [Fact]
    public void Given_a_call_to_FindFileLocation_with_chained_find_methods_correct_file_is_found()
    {
        // Arrange
        var fileFinder = ChainFileFinders(false);
        var pluginRoot = "PluginRoot";
        var assemblyName = "fileInAppRoot.txt";

        // Act
        var result = fileFinder.FindFileLocation(pluginRoot, assemblyName);

        // Assert
        result.Should().NotBeNull();
    }
    private static FileLocationFinder ChainFileFinders(bool isDebug)
    {
        //Get the execution / Entry application paths
        var entryRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var executingRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        ILogger logger = new NullLoggerFactory().CreateLogger("test");

        return new FindFileInPluginNameFolder(
            logger!, entryRoot, new FindFileInPluginNameFolder(
                logger!, executingRoot, new FindFileInPluginRootFolder(
                    logger!, entryRoot, new FindFileInPluginRootFolder(
                        logger!, executingRoot, new FindFileInApplicationFolder(
                            logger!, entryRoot, new FindFileInApplicationFolder(
                                logger!, executingRoot, null, isDebug: isDebug), isDebug), isDebug), isDebug), isDebug), isDebug);
    }
}