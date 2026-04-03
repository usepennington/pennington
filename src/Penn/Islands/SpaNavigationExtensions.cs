namespace Penn.Islands;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Penn.Infrastructure;
using Penn.Routing;

/// <summary>Extension methods for SPA navigation registration.</summary>
public static class SpaNavigationExtensions
{
    /// <summary>Register SPA navigation services.</summary>
    public static IServiceCollection AddSpaNavigation(this IServiceCollection services, Action<SpaNavigationOptions>? configure = null)
    {
        var options = new SpaNavigationOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddTransient<SpaPageDataService>();

        // Register a default RenderContext so SpaPageDataService can be resolved.
        // If AddPenn() already registered one, TryAdd semantics aren't needed —
        // the last registration wins, but AddSpaNavigation is typically called
        // before any override, so this provides a safe default.
        services.AddSingleton(sp =>
        {
            var pennOptions = sp.GetService<PennOptions>();
            return new RenderContext(
                BaseUrl: new UrlPath(pennOptions?.CanonicalBaseUrl ?? "/"),
                SiteTitle: pennOptions?.SiteTitle ?? "",
                Locale: null
            );
        });

        return services;
    }

    /// <summary>Map the SPA data endpoint.</summary>
    public static IEndpointRouteBuilder UseSpaNavigation(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<SpaNavigationOptions>();
        var dataPath = options.DataPath.TrimStart('/');

        app.MapGet($"/{dataPath}/{{*slug}}", async (string? slug, SpaPageDataService service) =>
        {
            if (slug == null) return Results.NotFound();

            // Strip .json extension if present
            if (slug.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                slug = slug[..^5];

            var url = SpaSlug.ToUrl(slug);
            var route = new Routing.ContentRoute
            {
                CanonicalPath = new Routing.UrlPath(url),
                OutputFile = new Routing.FilePath($"{url.TrimStart('/')}/index.html".TrimStart('/'))
            };

            // For now, use slug as title - the DocSite will provide proper metadata
            var data = await service.GetPageDataAsync(route, slug);
            return data is null
                ? Results.NotFound()
                : Results.Content(SpaEnvelopeSerializer.Serialize(data), "application/json");
        });

        return app;
    }
}
