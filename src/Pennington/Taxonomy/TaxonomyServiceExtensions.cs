namespace Pennington.Taxonomy;

using System.IO.Abstractions;
using Content;
using FrontMatter;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI + endpoint helpers for registering and mounting taxonomy axes
/// (browse-by-tag, browse-by-cuisine, browse-by-audience, ...).
/// </summary>
public static class TaxonomyServiceExtensions
{
    /// <summary>
    /// Registers a <see cref="TaxonomyContentService{TFrontMatter, TKey}"/> configured by
    /// <paramref name="configure"/>. Multiple <c>AddTaxonomy</c> calls with the same
    /// <typeparamref name="TFrontMatter"/>/<typeparamref name="TKey"/> pair coexist as long as
    /// each uses a distinct <see cref="TaxonomyOptions{TFrontMatter, TKey}.BaseUrl"/>.
    /// </summary>
    /// <typeparam name="TFrontMatter">Front-matter type used to parse source markdown items.</typeparam>
    /// <typeparam name="TKey">Taxonomy key type (typically <see cref="string"/>).</typeparam>
    public static IServiceCollection AddTaxonomy<TFrontMatter, TKey>(
        this IServiceCollection services,
        Action<TaxonomyOptions<TFrontMatter, TKey>> configure)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
    {
        var options = new TaxonomyOptions<TFrontMatter, TKey>();
        configure(options);
        options.Validate();

        services.AddSingleton<IContentService>(sp => new TaxonomyContentService<TFrontMatter, TKey>(
            options,
            sp,
            sp.GetRequiredService<FrontMatterParser>(),
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IFileWatcher>()));

        return services;
    }

    /// <summary>
    /// Mounts the live HTTP endpoints for every <see cref="TaxonomyContentService{TFrontMatter, TKey}"/>
    /// registered for the given type pair. The index URL renders <see cref="TaxonomyOptions{TFrontMatter, TKey}.IndexPage"/>
    /// with a <c>Terms</c> parameter; per-term URLs render <see cref="TaxonomyOptions{TFrontMatter, TKey}.TermPage"/>
    /// with a <c>Term</c> parameter.
    /// </summary>
    public static IEndpointRouteBuilder MapTaxonomy<TFrontMatter, TKey>(this IEndpointRouteBuilder routes)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
    {
        var taxonomies = routes.ServiceProvider.GetServices<IContentService>()
            .OfType<TaxonomyContentService<TFrontMatter, TKey>>()
            .ToList();

        foreach (var taxonomy in taxonomies)
        {
            // Capture loop variable so each lambda binds to its own taxonomy instance.
            var capturedTaxonomy = taxonomy;

            routes.MapGet(capturedTaxonomy.IndexUrl, async (HtmlRenderer renderer) =>
            {
                var terms = await capturedTaxonomy.GetTermsAsync();
                return await RenderComponentAsync(
                    renderer,
                    capturedTaxonomy.IndexPage,
                    new Dictionary<string, object?> { ["Terms"] = terms });
            });

            // Pennington's canonical URLs include a trailing slash. Register only the
            // trailing-slash form so the route is unambiguous; the redirect middleware
            // (or the build crawler) takes care of the canonical shape on its end.
            routes.MapGet(capturedTaxonomy.IndexUrl + "{slug}/", BuildTermHandler(capturedTaxonomy));
        }

        return routes;
    }

    private static Func<string, HtmlRenderer, Task<IResult>> BuildTermHandler<TFrontMatter, TKey>(
        TaxonomyContentService<TFrontMatter, TKey> taxonomy)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
    {
        return async (string slug, HtmlRenderer renderer) =>
        {
            var term = await taxonomy.TryGetTermAsync(slug);
            if (term is null)
            {
                return Results.NotFound();
            }

            return await RenderComponentAsync(
                renderer,
                taxonomy.TermPage,
                new Dictionary<string, object?> { ["Term"] = term });
        };
    }

    private static async Task<IResult> RenderComponentAsync(
        HtmlRenderer renderer,
        Type componentType,
        IDictionary<string, object?> parameters)
    {
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync(
                componentType,
                ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
        return Results.Content(html, "text/html");
    }
}