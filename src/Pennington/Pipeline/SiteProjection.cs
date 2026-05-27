namespace Pennington.Pipeline;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Content;
using Infrastructure;
using LlmsTxt;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Routing;
using Search;

/// <summary>
/// Default <see cref="ISiteProjection"/>. Walks
/// <see cref="ContentServiceExtensions.CollectIndexableEntriesAsync"/> plus any
/// <c>WithLlmsTxtEntry</c> endpoint registrations, fetches post-pipeline HTML
/// for each route in parallel via <see cref="RenderedHtmlFetcher"/>, and folds
/// every result into a stable index-keyed array (deterministic ordering for
/// snapshot tests). Llms-only items have no HTTP route, so the projection
/// renders them in-process via <see cref="IContentRenderer"/> +
/// <see cref="XrefResolvingService"/> instead.
/// <para>
/// Registered as <see cref="IFileWatchAware"/> with
/// <see cref="FileWatchResponse.Recreate"/> — file-watch invalidation drops the
/// instance and the next access rebuilds the full projection.
/// </para>
/// </summary>
public sealed class SiteProjection : IFileWatchAware, ISiteProjection
{
    private readonly AsyncLazy<ImmutableArray<RenderedPage>> _pagesLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the projection; the corpus is materialized lazily on first access.</summary>
    public SiteProjection(
        IEnumerable<IContentService> contentServices,
        MetadataEnrichmentService enrichment,
        IContentRenderer renderer,
        XrefResolvingService xrefResolver,
        RenderedHtmlFetcher fetcher,
        HeadingSectionExtractor extractor,
        SiteProjectionOptions options,
        EndpointDataSource endpointDataSource,
        ILogger<SiteProjection> logger)
    {
        _pagesLazy = new AsyncLazy<ImmutableArray<RenderedPage>>(
            () => BuildAsync(
                contentServices, enrichment, renderer, xrefResolver, fetcher,
                extractor, options, endpointDataSource, logger));
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<RenderedPage> GetPagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var pages = await _pagesLazy.Value;
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return page;
        }
    }

    /// <inheritdoc/>
    public async Task<RenderedPage?> GetPageAsync(UrlPath canonicalPath, CancellationToken cancellationToken = default)
    {
        var pages = await _pagesLazy.Value;
        foreach (var page in pages)
        {
            if (string.Equals(page.Route.CanonicalPath.Value, canonicalPath.Value, StringComparison.OrdinalIgnoreCase))
            {
                return page;
            }
        }
        return null;
    }

    private static async Task<ImmutableArray<RenderedPage>> BuildAsync(
        IEnumerable<IContentService> contentServices,
        MetadataEnrichmentService enrichment,
        IContentRenderer renderer,
        XrefResolvingService xrefResolver,
        RenderedHtmlFetcher fetcher,
        HeadingSectionExtractor extractor,
        SiteProjectionOptions options,
        EndpointDataSource endpointDataSource,
        ILogger logger)
    {
        // 1) Build the ParsedItem map so MarkdownOrigin pages carry enriched front-matter
        //    and derived metadata, and so Llms-only items can be rendered in-process.
        var parsedByPath = new Dictionary<string, ParsedItem>(StringComparer.OrdinalIgnoreCase);
        await foreach (var parsed in contentServices.ParseAllContentAsync())
        {
            var enriched = await enrichment.EnrichAsync(parsed);
            parsedByPath[Normalize(enriched.Route.CanonicalPath.Value)] = enriched;
        }

        // 2) Build the source-type map. We need it to distinguish LlmsOnlySource (no HTTP
        //    page; renders in-process) from regular markdown sources (HTTP-fetched). Walks
        //    DiscoverAllAsync once.
        var sourceByPath = new Dictionary<string, ContentSource>(StringComparer.OrdinalIgnoreCase);
        await foreach (var discovered in contentServices.DiscoverAllAsync())
        {
            sourceByPath[Normalize(discovered.Route.CanonicalPath.Value)] = discovered.Source;
        }

        // 3) Renderable TOC entries. CollectIndexableEntriesAsync is the broadest "has a
        //    page (or sidecar) at a URL" set — search/llms each filter further at consume
        //    time via ExcludeFromSearch / ExcludeFromLlms.
        var tocItems = await contentServices.CollectIndexableEntriesAsync();

        // 4) Endpoint entries opted in via WithLlmsTxtEntry. These do not get fetched — the
        //    endpoint URL itself is the link target downstream consumers (llms.txt) point at.
        var endpointEntries = CollectEndpointEntries(endpointDataSource).ToList();

        // Compose the entry list. Index-keyed slots so the parallel build can write
        // results without coordination and the fold-out yields deterministic order.
        var total = tocItems.Count + endpointEntries.Count;
        var results = new RenderedPage?[total];

        await Parallel.ForEachAsync(Enumerable.Range(0, tocItems.Count), async (i, ct) =>
        {
            var toc = tocItems[i];
            var key = Normalize(toc.Route.CanonicalPath.Value);
            sourceByPath.TryGetValue(key, out var source);

            try
            {
                // Llms-only items have a route but no HTTP page (OutputGenerationService
                // skips them in Phase 1 and they're never registered as routable content),
                // so render them in-process. xref resolution + locale rewriting that the
                // response pipeline applies to fetched HTML is replicated here as best we
                // can — xref via XrefResolvingService; locale/base-URL rewrites cannot apply
                // because llms-only pages have no canonical HTTP URL to rewrite from.
                if (source is LlmsOnlySource && parsedByPath.TryGetValue(key, out var llmsParsed))
                {
                    var rendered = await RenderInProcessAsync(renderer, xrefResolver, llmsParsed);
                    if (rendered is null)
                    {
                        logger.LogWarning("SiteProjection: failed to render llms-only item {Path}", toc.Route.CanonicalPath.Value);
                        return;
                    }

                    var (html, element) = rendered.Value;
                    results[i] = new RenderedPage(
                        Route: toc.Route,
                        Toc: toc,
                        Origin: new MarkdownOrigin(llmsParsed),
                        Html: html,
                        Content: element,
                        Sections: BuildSectionsLazy(extractor, element, options.ExcludeCodeBlocks));
                    return;
                }

                // Default path: fetch through the live pipeline so every response-stage
                // transform (xref, locale, base URL, canonical) is reflected exactly the
                // way a human reader would see it. BuildHtmlCache collapses repeat fetches
                // across the build.
                var fetched = await fetcher.FetchContentAsync(toc.Route.CanonicalPath.Value, options.ContentSelector, ct);
                if (fetched is null)
                {
                    return;
                }

                var origin = parsedByPath.TryGetValue(key, out var parsed)
                    ? (PageOrigin)new MarkdownOrigin(parsed)
                    : new RazorOrigin();

                results[i] = new RenderedPage(
                    Route: toc.Route,
                    Toc: toc,
                    Origin: origin,
                    Html: fetched.OuterHtml,
                    Content: fetched,
                    Sections: BuildSectionsLazy(extractor, fetched, options.ExcludeCodeBlocks));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "SiteProjection: failed to project {Path}, skipping", toc.Route.CanonicalPath.Value);
            }
        });

        for (var j = 0; j < endpointEntries.Count; j++)
        {
            var (toc, url) = endpointEntries[j];
            results[tocItems.Count + j] = new RenderedPage(
                Route: toc.Route,
                Toc: toc,
                Origin: new EndpointOrigin(url),
                Html: "",
                Content: null,
                Sections: new Lazy<IReadOnlyList<HeadingSection>>(() => []));
        }

        return [.. results.Where(p => p is not null).Select(p => p!)];
    }

    private static Lazy<IReadOnlyList<HeadingSection>> BuildSectionsLazy(
        HeadingSectionExtractor extractor, IElement element, bool excludeCodeBlocks)
        => new(() => extractor.Extract(element, excludeCodeBlocks));

    private static async Task<(string Html, IElement Element)?> RenderInProcessAsync(
        IContentRenderer renderer, XrefResolvingService xrefResolver, ParsedItem parsed)
    {
        var result = await renderer.RenderAsync(parsed);
        if (result.Value is not RenderedItem rendered)
        {
            return null;
        }

        var resolved = await xrefResolver.ResolveAsync(rendered.Content.Html);

        // Per-page browsing context — IBrowsingContext is not safe for concurrent OpenAsync
        // and we may be inside Parallel.ForEachAsync. The context stays alive for the
        // returned IElement's lifetime, i.e. as long as the projection holds the RenderedPage.
        var browsingContext = BrowsingContext.New(Configuration.Default);
        var document = await browsingContext.OpenAsync(req => req.Content(resolved));
        var element = document.Body;
        if (element is null)
        {
            return null;
        }

        return (resolved, element);
    }

    /// <summary>
    /// Walks the registered endpoints looking for routes opted into llms.txt
    /// via <see cref="LlmsTxtEndpointExtensions.WithLlmsTxtEntry{TBuilder}"/>
    /// and yields synthetic TOC items keyed off the endpoint URL. Skips
    /// endpoints whose URL contains a route parameter — the projection needs a
    /// concrete URL, not a pattern.
    /// </summary>
    private static IEnumerable<(ContentTocItem Toc, string Url)> CollectEndpointEntries(EndpointDataSource endpointDataSource)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint route)
            {
                continue;
            }

            var meta = route.Metadata.GetMetadata<LlmsTxtEntryMetadata>();
            if (meta is null)
            {
                continue;
            }

            var rawText = route.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText) || rawText.Contains('{'))
            {
                continue;
            }

            var url = rawText.StartsWith('/') ? rawText : "/" + rawText;
            var canonicalPath = new UrlPath(url);
            var contentRoute = new ContentRoute
            {
                CanonicalPath = canonicalPath,
                OutputFile = new FilePath(url.TrimStart('/')),
            };
            var hierarchyParts = url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            yield return (
                new ContentTocItem(
                    Title: meta.Title,
                    Route: contentRoute,
                    Order: int.MaxValue,
                    HierarchyParts: hierarchyParts,
                    SectionLabel: null,
                    Locale: null)
                {
                    Description = meta.Description,
                },
                url);
        }
    }

    private static string Normalize(string canonicalPath) => canonicalPath.Trim('/');
}
