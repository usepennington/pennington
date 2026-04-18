namespace Pennington.Islands;

using System.Collections.Immutable;
using Content;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;
using Routing;

/// <summary>
/// Content service that registers <c>/_spa-data/{slug}.json</c> pages so the static
/// site generator produces a JSON data file alongside every rendered HTML page.
/// Collects pages from all registered <see cref="IContentService"/> instances
/// (excluding itself to avoid recursion).
/// </summary>
internal sealed class SpaNavigationContentService(
    IServiceProvider serviceProvider,
    SpaNavigationOptions options) : IContentService
{
    public string DefaultSectionLabel => "";
    public int SearchPriority => 0;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var contentServices = serviceProvider.GetServices<IContentService>();
        var dataPath = options.DataPath.TrimStart('/');

        foreach (var service in contentServices)
        {
            if (service is SpaNavigationContentService) continue;

            await foreach (var item in service.DiscoverAsync())
            {
                // Only generate JSON for HTML content pages
                var ext = Path.GetExtension(item.Route.OutputFile.Value);
                if (!string.IsNullOrEmpty(ext) &&
                    !ext.Equals(".html", StringComparison.OrdinalIgnoreCase) &&
                    !ext.Equals(".htm", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Razor pages and redirects can't be rendered as SPA island content
                if (item.Source.Value is RazorPageSource or RedirectSource) continue;

                var slug = SpaSlug.ToSlug(item.Route.CanonicalPath.Value);
                var route = new ContentRoute
                {
                    CanonicalPath = new UrlPath($"/{dataPath}/{slug}.json"),
                    OutputFile = new FilePath($"{dataPath}/{slug}.json"),
                };

                ContentSource source = new EndpointSource();
                yield return new DiscoveredItem(route, source);
            }
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
        Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
        Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
        Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
        Task.FromResult(ImmutableList<CrossReference>.Empty);
}