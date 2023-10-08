using FluentAssertions;
using Harmonize.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace Harmonize.UnitTest.IO;

public class FindFileInPluginRootFolderTest
{
    [Fact]
    public void Given_a_call_to_FindFileLocation_Then_the_file_is_found()
    {
        // Arrange
        ILogger logger = new NullLoggerFactory().CreateLogger("test");
        var executingRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var fileFinder = new FindFileInPluginRootFolder(logger, executingRoot);
        var pluginRoot = "IO/PluginRoot";
        var assemblyName = "fileInPluginRoot.txt";

        // Act
        var result = fileFinder.FindFileLocation(pluginRoot, assemblyName);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Given_a_call_to_FindFileLocation_when_file_not_exist_FileNotFoundException_is_thrown()
    {
        // Arrange
        ILogger logger = new NullLoggerFactory().CreateLogger("test");
        var executingRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var fileFinder = new FindFileInPluginRootFolder(logger, executingRoot);
        var pluginRoot = "PluginRoot";
        var assemblyName = "dontExist.txt";

        // Act with catch exception
        Action act = () => fileFinder.FindFileLocation(pluginRoot, assemblyName);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Given_a_call_to_FindFileLocation_when_assemblyName_is_null_ArgumentException_is_thrown()
    {
        // Arrange
        ILogger logger = new NullLoggerFactory().CreateLogger("test");
        var executingRoot = string.Empty;
        var fileFinder = new FindFileInPluginRootFolder(logger, executingRoot);
        var pluginRoot = "PluginRoot";
        var assemblyName = string.Empty;

        // Act with catch exception
        Action act = () => fileFinder.FindFileLocation(pluginRoot, assemblyName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}