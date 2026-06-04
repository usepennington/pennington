namespace Pennington.Pipeline;

using Content;
using FrontMatter;
using Generation;
using Infrastructure;
using Routing;

/// <summary>
/// Orchestrates the content processing pipeline.
/// </summary>
public sealed class ContentPipeline : IContentPipeline
{
    private readonly IReadOnlyList<IContentService> _services;
    private readonly IContentParser? _parser;
    private readonly IContentRenderer _renderer;
    private readonly TimeProvider _clock;

    /// <summary>
    /// Creates the pipeline from the registered content services and renderer. The
    /// <paramref name="parser"/> is optional: a bare host that registers no markdown source has no
    /// <see cref="IContentParser"/>, so discovered items pass through the parse stage unchanged.
    /// </summary>
    public ContentPipeline(
        IEnumerable<IContentService> services,
        IContentRenderer renderer,
        IContentParser? parser = null,
        TimeProvider? clock = null)
    {
        _services = services.ToList();
        _renderer = renderer;
        _parser = parser;
        _clock = clock ?? TimeProvider.System;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ContentItem> DiscoverAsync()
    {
        await foreach (var discovered in _services.DiscoverAllAsync())
        {
            yield return discovered;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items)
    {
        await foreach (var item in items)
        {
            // FailedItems pass through unchanged
            if (item.Value is FailedItem)
            {
                yield return item;
                continue;
            }

            if (item.Value is DiscoveredItem discovered)
            {
                // RedirectSource items are handled by PenningtonRedirectMiddleware at
                // request time (dev) and captured as 301 responses by the build crawler;
                // they don't participate in parse/render and must not reach the parser.
                // EndpointSource items (e.g., /sitemap.xml, /llms.txt) are produced by
                // a live HTTP endpoint — there's no file to parse, same skip applies.
                if (discovered.Source.Value is RedirectSource or EndpointSource)
                {
                    continue;
                }

                // No markdown parser registered (bare host): nothing can turn a DiscoveredItem
                // into a ParsedItem, so pass it through. Razor @page routes are served by Blazor
                // routing and custom sources resolve through their own endpoints.
                if (_parser is not { } parser)
                {
                    yield return item;
                    continue;
                }

                ContentItem result;
                try
                {
                    result = await parser.ParseAsync(discovered);
                }
                catch (Exception ex)
                {
                    result = new FailedItem(discovered.Route,
                        new ContentError($"Parse failed: {ex.Message}", ex));
                }
                yield return result;
            }
            else
            {
                // Already parsed or rendered — pass through
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items)
    {
        await foreach (var item in items)
        {
            // FailedItems pass through unchanged
            if (item.Value is FailedItem)
            {
                yield return item;
                continue;
            }

            if (item.Value is ParsedItem parsed)
            {
                ContentItem result;
                try
                {
                    result = await _renderer.RenderAsync(parsed);
                }
                catch (Exception ex)
                {
                    result = new FailedItem(parsed.Route,
                        new ContentError($"Render failed: {ex.Message}", ex));
                }

                yield return result;
            }
            else
            {
                // Not a ParsedItem (Discovered or Rendered) — pass through
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items)
    {
        var reportBuilder = new BuildReportBuilder();

        await foreach (var item in items)
        {
            switch (item.Value)
            {
                case RenderedItem rendered:
                    // Drafts and future-dated (scheduled) pages don't ship.
                    if (rendered.Metadata.IsHiddenFromBuild(_clock))
                    {
                        reportBuilder.AddSkippedPage(rendered.Route);
                    }
                    else
                    {
                        reportBuilder.AddGeneratedPage(rendered.Route);

                        // Check for internal links missing trailing slashes
                        var badLinks = LinkVerificationService.FindLinksWithoutTrailingSlash(rendered.Content.Html);
                        foreach (var badLink in badLinks)
                        {
                            reportBuilder.AddWarning(rendered.Route,
                                $"Internal link \"{badLink}\" is missing a trailing slash");
                        }
                    }
                    break;

                case FailedItem failed:
                    reportBuilder.AddError(failed.Route, failed.Error.Message, failed.Error.Exception);
                    break;

                default:
                    // Items that didn't reach Rendered state — warn
                    reportBuilder.AddWarning(item.Route, "Item did not complete pipeline");
                    break;
            }
        }

        // Check for Razor @page directives missing trailing slashes
        foreach (var service in _services)
        {
            if (service is RazorPageContentService razorService)
            {
                foreach (var (template, typeName) in razorService.MissingTrailingSlashPages)
                {
                    var route = ContentRouteFactory.FromRazorPage(template);
                    reportBuilder.AddWarning(route,
                        $"Razor @page directive \"{template}\" is missing a trailing slash (in {typeName})");
                }
            }
        }

        return reportBuilder.Build();
    }

    /// <summary>
    /// Convenience: run the full pipeline end-to-end.
    /// </summary>
    public async Task<BuildReport> RunAsync()
    {
        var discovered = DiscoverAsync();
        var parsed = ParseAsync(discovered);
        var rendered = RenderAsync(parsed);
        return await GenerateAsync(rendered);
    }
}