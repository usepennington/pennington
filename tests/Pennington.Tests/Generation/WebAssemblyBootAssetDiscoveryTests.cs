namespace Pennington.Tests.Generation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;
using Pennington.Generation;
using Shouldly;

public class WebAssemblyBootAssetDiscoveryTests
{
    [Fact]
    public void Discovers_the_wasm_boot_manifest_module()
    {
        var source = new StubEndpointDataSource(
            Endpoint("_framework/resource-collection.-9WPH.js"));

        var routes = WebAssemblyBootAssetDiscovery.Discover(source).ToList();

        routes.Count.ShouldBe(1);
        routes[0].CanonicalPath.Value.ShouldBe("/_framework/resource-collection.-9WPH.js");
        routes[0].OutputFile.Value.ShouldBe("_framework/resource-collection.-9WPH.js");
    }

    [Fact]
    public void Includes_the_unfingerprinted_alias_and_dedupes_repeats()
    {
        var source = new StubEndpointDataSource(
            Endpoint("_framework/resource-collection.-9WPH.js"),
            Endpoint("_framework/resource-collection.-9WPH.js"), // content-negotiation duplicate
            Endpoint("_framework/resource-collection.js"));

        var routes = WebAssemblyBootAssetDiscovery.Discover(source).Select(r => r.OutputFile.Value).ToList();

        routes.ShouldBe(
            ["_framework/resource-collection.-9WPH.js", "_framework/resource-collection.js"],
            ignoreOrder: true);
    }

    [Fact]
    public void Skips_precompressed_variants_parameterized_and_unrelated_routes()
    {
        var source = new StubEndpointDataSource(
            Endpoint("_framework/resource-collection.-9WPH.js.gz"),  // precompressed
            Endpoint("_framework/dotnet.js"),                        // physical framework file (copied elsewhere)
            Endpoint("_framework/blazor.web.js"),                    // physical framework file
            Endpoint("_framework/{**path}"),                        // parameterized
            Endpoint("styles.css"));                                // not a framework route

        WebAssemblyBootAssetDiscovery.Discover(source).ShouldBeEmpty();
    }

    private static RouteEndpoint Endpoint(string pattern) =>
        new(
            requestDelegate: _ => Task.CompletedTask,
            routePattern: RoutePatternFactory.Parse(pattern),
            order: 0,
            metadata: EndpointMetadataCollection.Empty,
            displayName: pattern);

    private sealed class StubEndpointDataSource(params RouteEndpoint[] endpoints) : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints { get; } = endpoints;
        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }
}
