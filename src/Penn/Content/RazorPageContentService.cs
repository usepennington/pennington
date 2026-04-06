namespace Penn.Content;

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Discovers @page Razor components for the content pipeline.
/// Scans configured assemblies for types inheriting ComponentBase with
/// non-parameterized [RouteAttribute] routes.
/// </summary>
public sealed class RazorPageContentService : IContentService
{
    private readonly Assembly[] _assemblies;
    private readonly List<(string Template, string TypeName)> _missingTrailingSlashPages = [];

    public RazorPageContentService(Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    /// <summary>
    /// Razor @page directives that were missing a trailing slash.
    /// Populated after <see cref="DiscoverAsync"/> runs.
    /// </summary>
    public IReadOnlyList<(string Template, string TypeName)> MissingTrailingSlashPages => _missingTrailingSlashPages;

    public string DefaultSection => "";
    public int SearchPriority => 5;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var (route, componentType) in DiscoverRazorPages())
        {
            ContentSource source = new RazorPageSource(componentType);
            yield return new DiscoveredItem(route, source);
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    private List<(ContentRoute Route, string ComponentType)> DiscoverRazorPages()
    {
        var results = new List<(ContentRoute, string)>();

        foreach (var assembly in _assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(ComponentBase).IsAssignableFrom(type) || type.IsAbstract)
                        continue;

                    var routeAttributes = type.GetCustomAttributes<RouteAttribute>();
                    foreach (var attr in routeAttributes)
                    {
                        var template = attr.Template;

                        // Skip parameterized routes (contain {})
                        if (template.Contains('{'))
                            continue;

                        if (template != "/" && !template.EndsWith('/'))
                            _missingTrailingSlashPages.Add((template, type.FullName ?? type.Name));

                        var route = ContentRouteFactory.FromRazorPage(template);
                        results.Add((route, type.AssemblyQualifiedName ?? type.FullName ?? type.Name));
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned (e.g., dynamic assemblies)
            }
        }

        return results;
    }
}
