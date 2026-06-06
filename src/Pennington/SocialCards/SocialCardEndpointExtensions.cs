namespace Pennington.SocialCards;

using Content;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Endpoint helper that renders social-card images on demand.</summary>
public static class SocialCardEndpointExtensions
{
    /// <summary>
    /// Maps <c>{BaseUrl}/{**slug}.png</c> to the host's <see cref="SocialCardOptions.Render"/> hook.
    /// Serves cards live during development and is the route the static build crawler fetches to bake
    /// each card discovered by <see cref="SocialCardContentService"/>. Resolving page metadata from
    /// <see cref="ContentRecordRegistry"/> (a discovery-time join, not the request-path-forbidden
    /// <see cref="Pipeline.ISiteProjection"/>) keeps this safe to consume from a live request.
    /// </summary>
    public static IEndpointRouteBuilder MapSocialCards(this IEndpointRouteBuilder routes)
    {
        var options = routes.ServiceProvider.GetRequiredService<SocialCardOptions>();

        routes.MapGet(options.BaseUrl.TrimEnd('/') + "/{**slug}", async (
            string slug,
            HttpContext ctx,
            ContentRecordRegistry registry,
            PenningtonOptions site) =>
        {
            if (!slug.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            var key = SocialCardUrl.SlugToRecordKey(slug);
            var records = await registry.GetSnapshotAsync();
            if (!records.TryGetValue(key, out var record))
            {
                return Results.NotFound();
            }

            var cardUrl = SocialCardUrl.For(record.Route.CanonicalPath, options.BaseUrl, site.CanonicalBaseUrl);
            var request = new SocialCardRequest(
                Title: record.Metadata.Title,
                Description: record.Metadata.Description,
                Date: record.Metadata.Date,
                CanonicalPath: record.Route.CanonicalPath,
                CardUrl: cardUrl,
                Locale: string.IsNullOrEmpty(record.Route.Locale) ? null : record.Route.Locale,
                SiteTitle: site.SiteTitle,
                SiteDescription: site.SiteDescription,
                Metadata: record.Metadata,
                Width: options.Width,
                Height: options.Height);

            var bytes = await options.Render(request, ctx.RequestAborted);
            return bytes is null
                ? Results.NotFound()
                : Results.Bytes(bytes, options.ContentType);
        });

        return routes;
    }
}
