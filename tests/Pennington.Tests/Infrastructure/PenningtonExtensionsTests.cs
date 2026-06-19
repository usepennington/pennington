using System.Reflection;
using Microsoft.Extensions.Configuration;
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

    private static IConfiguration Config(params (string Key, string Value)[] entries) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(entries.Select(e => new KeyValuePair<string, string?>(e.Key, e.Value)))
            .Build();

    [Fact]
    public void ShouldUseEphemeralPort_NoPortConfigured_ReturnsTrue()
    {
        PenningtonExtensions.ShouldUseEphemeralPort(Config()).ShouldBeTrue();
    }

    [Fact]
    public void ShouldUseEphemeralPort_UrlsConfigured_ReturnsFalse()
    {
        var config = Config(("urls", "http://localhost:5005"));

        PenningtonExtensions.ShouldUseEphemeralPort(config).ShouldBeFalse();
    }

    [Fact]
    public void ShouldUseEphemeralPort_KestrelEndpointConfigured_ReturnsFalse()
    {
        var config = Config(("Kestrel:Endpoints:Http:Url", "http://localhost:5006"));

        PenningtonExtensions.ShouldUseEphemeralPort(config).ShouldBeFalse();
    }
}
