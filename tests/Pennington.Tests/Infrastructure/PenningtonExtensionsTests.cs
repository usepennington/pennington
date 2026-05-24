using System.Reflection;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class PenningtonExtensionsTests
{
    private static readonly Assembly Entry = typeof(PenningtonExtensionsTests).Assembly;
    private static readonly Assembly Other = typeof(PenningtonExtensions).Assembly;

    [Fact]
    public void ResolveRoutingAssemblies_EmptyConfigured_ReturnsEntryAssembly()
    {
        var result = PenningtonExtensions.ResolveRoutingAssemblies([], Entry);

        result.ShouldBe([Entry]);
    }

    [Fact]
    public void ResolveRoutingAssemblies_ConfiguredWithoutEntry_AppendsEntry()
    {
        var result = PenningtonExtensions.ResolveRoutingAssemblies([Other], Entry);

        result.ShouldBe([Other, Entry]);
    }

    [Fact]
    public void ResolveRoutingAssemblies_EntryAlreadyConfigured_NoDuplicate()
    {
        var result = PenningtonExtensions.ResolveRoutingAssemblies([Other, Entry], Entry);

        result.ShouldBe([Other, Entry]);
    }

    [Fact]
    public void ResolveRoutingAssemblies_NullEntry_ReturnsConfiguredUnchanged()
    {
        var result = PenningtonExtensions.ResolveRoutingAssemblies([Other], null);

        result.ShouldBe([Other]);
    }
}
