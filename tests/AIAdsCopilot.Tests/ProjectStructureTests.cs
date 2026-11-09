using System.Reflection;

namespace AIAdsCopilot.Tests;

public class ProjectStructureTests
{
    [Theory]
    [InlineData("AIAdsCopilot.Domain")]
    [InlineData("AIAdsCopilot.Application")]
    [InlineData("AIAdsCopilot.Infrastructure")]
    [InlineData("AIAdsCopilot.Cli")]
    public void ProjectAssemblyCanBeLoaded(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);

        Assert.Equal(assemblyName, assembly.GetName().Name);
    }
}
