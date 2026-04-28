namespace ExtensibilityLabExample;

using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Demonstrates <see cref="IContentService"/> by turning a folder of
/// <c>Content/releases/*.json</c> files into site pages, a navigation
/// section, and cross-reference entries.
/// <para>
/// Emits one <see cref="DiscoveredItem"/> per JSON file plus an index
/// route. The static-build crawler fetches each one over HTTP from the
/// running host, so a sibling <c>MapGet("/releases/{version}/")</c>
/// endpoint in <c>Program.cs</c> does the actual HTML rendering — the
/// same code path dev-mode uses. That keeps the service focused on
/// discovery, TOC, and cross-references and leaves presentation to the
/// endpoint.
/// </para>
/// <para>
/// Backs how-to 2.3.10 <c>/how-to/extensibility/custom-content-service</c>.
/// </para>
/// </summary>
public sealed class ReleaseNotesContentService : IContentService
{
    private readonly string _releasesDirectory;
    private readonly Lazy<ImmutableList<ReleaseEntry>> _entriesLazy;

    public ReleaseNotesContentService(IWebHostEnvironment environment)
    {
        _releasesDirectory = Path.Combine(environment.ContentRootPath, "Content", "releases");
        _entriesLazy = new Lazy<ImmutableList<ReleaseEntry>>(LoadEntries);
    }

    public string DefaultSectionLabel => "Releases";
    public int SearchPriority => 20;

    /// <summary>The full set of release entries this service knows about.</summary>
    public IReadOnlyList<ReleaseEntry> Entries => _entriesLazy.Value;

    /// <summary>Find a single release by version string, or null if no match.</summary>
    public ReleaseEntry? TryGet(string version) =>
        _entriesLazy.Value.FirstOrDefault(e => e.Version == version);

    /// <summary>
    /// One discovered item for the index plus one per JSON file. Each route is
    /// paired with <see cref="EndpointSource"/> — the build crawler discovers
    /// the URL and fetches it through the live pipeline, where the sibling
    /// <c>MapGet</c> endpoint in <c>Program.cs</c> produces the HTML. These
    /// items do not appear in <c>sitemap.xml</c>; that's the intended tradeoff
    /// for routes whose canonical HTML lives behind a custom endpoint.
    /// </summary>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        yield return new DiscoveredItem(
            ContentRouteFactory.FromUrl(new UrlPath("/releases/")),
            new EndpointSource());

        foreach (var entry in _entriesLazy.Value)
        {
            var route = ContentRouteFactory.FromCustom(
                url: new UrlPath($"/releases/{entry.Version}/"),
                sourceFile: new FilePath(entry.SourcePath));
            yield return new DiscoveredItem(route, new EndpointSource());
        }

        await Task.CompletedTask;
    }

    /// <summary>No static files to copy — JSON sources are transformed, not republished.</summary>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <summary>
    /// No dynamically generated files — each discovered route is served by a
    /// <c>MapGet</c> endpoint whose HTTP response the crawler writes to disk.
    /// Override this method when the output format is orthogonal to the site's
    /// HTML pages (see <c>LlmsTxtContentService</c> for an example).
    /// </summary>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <summary>TOC entries so the pages show up in navigation and the search index.</summary>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var entries = _entriesLazy.Value;
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        builder.Add(new ContentTocItem(
            Title: "Releases",
            Route: ContentRouteFactory.FromUrl(new UrlPath("/releases/")),
            Order: 100,
            HierarchyParts: ["releases"],
            SectionLabel: DefaultSectionLabel,
            Locale: null));

        var order = 110;
        foreach (var entry in entries)
        {
            builder.Add(new ContentTocItem(
                Title: entry.Title,
                Route: ContentRouteFactory.FromUrl(new UrlPath($"/releases/{entry.Version}/")),
                Order: order,
                HierarchyParts: ["releases", entry.Version],
                SectionLabel: DefaultSectionLabel,
                Locale: null));
            order += 10;
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <summary>One cross-reference per release so <c>&lt;xref:release-1.0.0&gt;</c> resolves.</summary>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var entries = _entriesLazy.Value;
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var entry in entries)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath($"/releases/{entry.Version}/"));
            builder.Add(new CrossReference($"release-{entry.Version}", entry.Title, route));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    private ImmutableList<ReleaseEntry> LoadEntries()
    {
        if (!Directory.Exists(_releasesDirectory))
            return [];

        var builder = ImmutableList.CreateBuilder<ReleaseEntry>();
        foreach (var file in Directory.EnumerateFiles(_releasesDirectory, "*.json"))
        {
            var json = File.ReadAllText(file);
            var dto = JsonSerializer.Deserialize<ReleaseJson>(json, JsonOptions);
            if (dto is null) continue;
            builder.Add(new ReleaseEntry(
                Version: dto.Version,
                Title: dto.Title,
                Date: dto.Date,
                Highlights: dto.Highlights ?? [],
                SourcePath: file));
        }

        return [.. builder.OrderBy(e => e.Version, StringComparer.OrdinalIgnoreCase)];
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed record ReleaseJson(string Version, string Title, string Date, List<string>? Highlights);
}

/// <summary>One parsed release record read from a JSON source file.</summary>
public sealed record ReleaseEntry(
    string Version,
    string Title,
    string Date,
    IReadOnlyList<string> Highlights,
    string SourcePath);