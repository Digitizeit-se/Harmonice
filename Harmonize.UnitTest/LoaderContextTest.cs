using System.Reflection;
using FluentAssertions;

namespace Harmonize.UnitTest;

public class LoaderContextTest
{
    [Fact]
    public void Given_LoaderContext_Load_Returns_Assembly()
    {
        // Arrange
        string pluginPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dlls\\Serilog.Extensions.Logging.dll");
        
        var context = new TestLoadContext(pluginPath);

        // Act
        var result = context.TestLoad(AssemblyName.GetAssemblyName(pluginPath));

        // Assert
        result.Should().NotBeNull();
    }

}

/// <summary>
/// Derived class to access protected methods
/// </summary>
internal class TestLoadContext : LoaderContext
{
    public TestLoadContext(string dllPath) : base(dllPath) { }

    public Assembly? TestLoad(AssemblyName assemblyName)
    {
        return Load(assemblyName);
    }
}

