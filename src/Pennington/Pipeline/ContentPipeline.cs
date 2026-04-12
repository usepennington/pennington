namespace Pennington.Pipeline;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Routing;

/// <summary>
/// Orchestrates the content processing pipeline.
/// </summary>
public sealed class ContentPipeline : IContentPipeline
{
    private readonly IReadOnlyList<IContentService> _services;
    private readonly IContentParser _parser;
    private readonly IContentRenderer _renderer;

    public ContentPipeline(
        IEnumerable<IContentService> services,
        IContentParser parser,
        IContentRenderer renderer)
    {
        _services = services.ToList();
        _parser = parser;
        _renderer = renderer;
    }

    public async IAsyncEnumerable<ContentItem> DiscoverAsync()
    {
        foreach (var service in _services)
        {
            await foreach (var discovered in service.DiscoverAsync())
            {
                yield return discovered;
            }
        }
    }

    public async IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items)
    {
        await foreach (var item in items)
        {
            // FailedItems pass through unchanged
            if (item is FailedItem)
            {
                yield return item;
                continue;
            }

            if (item is DiscoveredItem discovered)
            {
                ContentItem result;
                try
                {
                    result = await _parser.ParseAsync(discovered);
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

    public async IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items)
    {
        await foreach (var item in items)
        {
            // FailedItems pass through unchanged
            if (item is FailedItem)
            {
                yield return item;
                continue;
            }

            if (item is ParsedItem parsed)
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

    public async Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items, OutputOptions options)
    {
        var reportBuilder = new BuildReportBuilder();

        await foreach (var item in items)
        {
            switch (item)
            {
                case RenderedItem rendered:
                    // Check for drafts
                    if (rendered.Metadata.IsDraft)
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
    public async Task<BuildReport> RunAsync(OutputOptions options)
    {
        var discovered = DiscoverAsync();
        var parsed = ParseAsync(discovered);
        var rendered = RenderAsync(parsed);
        return await GenerateAsync(rendered, options);
    }
}
