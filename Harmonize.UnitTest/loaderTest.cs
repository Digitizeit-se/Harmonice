using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Harmonize.UnitTest;

public class LoaderTests
{
    [Fact]
    public void TestLoad()
    {
        //Arrange
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var rootFolder = "dlls";
        var assemblyName = "Serilog.Extensions.Logging";

        //Act
        var assembly = Loader.Load(rootFolder, assemblyName, serviceProvider);

        //Assert
        assembly.Should().NotBeNull();
        assembly.GetName().ToString().Should().Contain(assemblyName);

    }
}
