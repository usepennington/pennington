namespace Pennington.Generation;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;
using Routing;

/// <summary>
/// Surfaces the Blazor WebAssembly boot manifest (<c>_framework/resource-collection*.js</c>)
/// so the static build materialises it.
///
/// A Blazor Web App with an interactive-WebAssembly island boots via <c>blazor.web.js</c>,
/// which fetches a <c>resource-collection</c> ES module listing every WASM resource to load.
/// Unlike the assemblies/runtime — which are physical files under <c>_framework/</c> that the
/// static-asset copy phase already emits — this manifest is generated at request time by an
/// endpoint the WebAssembly render mode registers (its fingerprint is computed from the loaded
/// resources at startup). It has no <see cref="StaticAssetDescriptor"/> and no backing file, so
/// the copy phase misses it and a purely static host (e.g. GitHub Pages) 404s on WASM boot.
///
/// The running host serves it during the crawl, so we treat it like any other GET route and let
/// the fetch phase write it to disk. Precompressed <c>.js.gz</c> variants are skipped: a dumb
/// static host serves the identity <c>.js</c> the boot script requests, uncompressed.
/// </summary>
internal static class WebAssemblyBootAssetDiscovery
{
    private const string ManifestPrefix = "_framework/resource-collection";

    /// <summary>Yields a <see cref="ContentRoute"/> for each WebAssembly boot-manifest module on the data source.</summary>
    public static IEnumerable<ContentRoute> Discover(EndpointDataSource endpointDataSource)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
            {
                continue;
            }

            // Physical assets flow through the static-asset copy phase; only the synthesized
            // manifest endpoint (no descriptor, no backing file) needs materializing here.
            if (routeEndpoint.Metadata.GetMetadata<StaticAssetDescriptor>() is not null)
            {
                continue;
            }

            var rawText = routeEndpoint.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText) || rawText.Contains('{'))
            {
                continue;
            }

            var path = rawText.StartsWith('/') ? rawText[1..] : rawText;

            // The manifest module — both the fingerprinted route and its unfingerprinted alias —
            // but not the .js.gz precompressed variants (a static host serves identity .js).
            if (!path.StartsWith(ManifestPrefix, StringComparison.OrdinalIgnoreCase) ||
                !path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!seen.Add(path))
            {
                continue;
            }

            yield return new ContentRoute
            {
                CanonicalPath = new UrlPath("/" + path),
                OutputFile = new FilePath(path),
            };
        }
    }
}
