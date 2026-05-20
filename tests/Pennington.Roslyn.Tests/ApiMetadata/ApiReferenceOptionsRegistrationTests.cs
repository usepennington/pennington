namespace Pennington.Roslyn.Tests.ApiMetadata;

using Microsoft.Extensions.DependencyInjection;
using Pennington.Roslyn.ApiMetadata;

public sealed class ApiReferenceOptionsRegistrationTests
{
    [Fact]
    public void Default_name_registers_non_keyed_alias_pointing_at_keyed_instance()
    {
        var services = new ServiceCollection();
        services.AddApiMetadataFromRoslyn();

        var provider = services.BuildServiceProvider();

        var unkeyed = provider.GetRequiredService<ApiReferenceOptions>();
        var keyed = provider.GetRequiredKeyedService<ApiReferenceOptions>("default");

        unkeyed.ShouldBeSameAs(keyed);
    }

    [Fact]
    public void Non_default_name_does_not_register_non_keyed_alias()
    {
        var services = new ServiceCollection();
        services.AddApiMetadataFromRoslyn("custom");

        var provider = services.BuildServiceProvider();

        provider.GetService<ApiReferenceOptions>().ShouldBeNull();
        provider.GetRequiredKeyedService<ApiReferenceOptions>("custom").ShouldNotBeNull();
    }
}