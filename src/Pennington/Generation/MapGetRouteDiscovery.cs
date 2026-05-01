namespace Pennington.Generation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;
using Routing;

/// <summary>
/// Walks an <see cref="EndpointDataSource"/> to surface non-parameterized GET
/// endpoints (e.g. <c>/styles.css</c>, <c>/sitemap.xml</c>) so build-time and
/// audit-time consumers can treat them as known routes alongside content
/// service discoveries. Skips Razor component routes, framework routes, and
/// anything registered through <see cref="StaticAssetDescriptor"/> — those
/// either flow through their own copy phase or aren't link targets.
/// </summary>
public static class MapGetRouteDiscovery
{
    /// <summary>Yields a <see cref="ContentRoute"/> for every static GET endpoint discovered on the data source.</summary>
    public static IEnumerable<ContentRoute> Discover(EndpointDataSource endpointDataSource)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint) continue;

            var httpMethods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (httpMethods?.HttpMethods.Contains("GET") != true) continue;

            if (routeEndpoint.Metadata.GetMetadata<StaticAssetDescriptor>() is not null) continue;

            var rawText = routeEndpoint.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText)) continue;

            if (rawText.Contains('{') ||
                rawText.Contains("_framework") ||
                rawText.Contains("_blazor") ||
                endpoint.DisplayName?.Contains("static files") == true)
                continue;

            if (endpoint.Metadata.Any(m => m.GetType().Name == "FallbackMetadata")) continue;
            if (endpoint.Metadata.Any(m => m.GetType().Name == "ComponentTypeMetadata")) continue;

            var url = rawText.StartsWith('/') ? rawText : "/" + rawText;
            var outputPath = url.TrimStart('/');
            if (string.IsNullOrEmpty(outputPath)) outputPath = "index.html";

            yield return new ContentRoute
            {
                CanonicalPath = new UrlPath(url),
                OutputFile = new FilePath(outputPath),
            };
        }
    }
}
