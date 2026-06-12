namespace ExtensibilityLabExample;

using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Search;
using Pennington.StructuredData;

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
    /// <c>MapGet</c> endpoint in <c>Program.cs</c> produces the HTML. Because the
    /// endpoint serves real canonical HTML, these routes are included in
    /// <c>sitemap.xml</c> like any other page.
    /// <para>
    /// Each release item carries its <see cref="ReleaseEntry"/> as
    /// <see cref="DiscoveredItem.Metadata"/>. That single assignment surfaces the records to
    /// discovery: the default <c>GetRecordsAsync</c> bridge picks them up, so the browse-by-channel
    /// taxonomy, the custom <c>channel</c> search facet, and the per-page JSON-LD all light up from
    /// the one record — no separate index page, the same treatment markdown gets.
    /// The index item carries no metadata, so it is not itself a record.
    /// </para>
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
            yield return new DiscoveredItem(route, new EndpointSource()) { Metadata = entry };
        }

        await Task.CompletedTask;
    }

    /// <summary>No static files to copy — JSON sources are transformed, not republished.</summary>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

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
        {
            return [];
        }

        var builder = ImmutableList.CreateBuilder<ReleaseEntry>();
        foreach (var file in Directory.EnumerateFiles(_releasesDirectory, "*.json"))
        {
            var json = File.ReadAllText(file);
            var dto = JsonSerializer.Deserialize<ReleaseJson>(json, JsonOptions);
            if (dto is null)
            {
                continue;
            }

            builder.Add(new ReleaseEntry
            {
                Version = dto.Version,
                Title = dto.Title,
                Date = DateTime.TryParse(dto.Date, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
                        ? parsed
                        : null,
                Channel = string.IsNullOrWhiteSpace(dto.Channel) ? "stable" : dto.Channel!,
                Tags = dto.Tags?.ToArray() ?? [],
                Highlights = dto.Highlights ?? [],
                SourcePath = file,
            });
        }

        return [.. builder.OrderBy(e => e.Version, StringComparer.OrdinalIgnoreCase)];
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed record ReleaseJson(
        string Version,
        string Title,
        string Date,
        string? Channel,
        List<string>? Tags,
        List<string>? Highlights);
}

/// <summary>
/// One parsed release record. Implements <see cref="IFrontMatter"/> plus the discovery capability
/// mixins, so a single record feeds the browse-by-channel taxonomy (<see cref="ITaggable"/> /
/// the <c>Channel</c> key), the custom <c>channel</c> search facet (<see cref="IHasSearchFacets"/>),
/// and the per-page JSON-LD (<see cref="IHasStructuredData"/>) — the same treatment markdown front
/// matter gets, with no extra wiring beyond attaching it to the discovered item.
/// </summary>
public sealed record ReleaseEntry : IFrontMatter, ITaggable, IHasSearchFacets, IHasStructuredData
{
    /// <summary>Semantic version, used as the route slug (e.g. <c>1.1.0</c>).</summary>
    public string Version { get; init; } = "";

    /// <inheritdoc/>
    public string Title { get; init; } = "";

    /// <inheritdoc/>
    public DateTime? Date { get; init; }

    /// <summary>Release channel (<c>stable</c>, <c>beta</c>, ...) — the taxonomy key and custom search facet.</summary>
    public string Channel { get; init; } = "stable";

    /// <inheritdoc/>
    public string[] Tags { get; init; } = [];

    /// <summary>Bullet highlights rendered on the detail page.</summary>
    public IReadOnlyList<string> Highlights { get; init; } = [];

    /// <summary>Absolute path of the JSON source file (drives file-watching).</summary>
    public string SourcePath { get; init; } = "";

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string[]> SearchFacets => new Dictionary<string, string[]>
    {
        ["channel"] = [Channel],
    };

    /// <inheritdoc/>
    public IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context) =>
    [
        new ReleaseJsonLd
        {
            Name = Title,
            SoftwareVersion = Version,
            DatePublished = Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Url = context.CanonicalUrl,
        },
    ];
}

/// <summary>A schema.org <c>SoftwareApplication</c> describing one release, emitted as JSON-LD.</summary>
public sealed record ReleaseJsonLd : JsonLdEntity
{
    /// <inheritdoc/>
    [JsonPropertyName("@type")]
    public override string Type => "SoftwareApplication";

    /// <summary>Release title.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Semantic version string.</summary>
    [JsonPropertyName("softwareVersion")]
    public string? SoftwareVersion { get; init; }

    /// <summary>ISO publication date.</summary>
    [JsonPropertyName("datePublished")]
    public string? DatePublished { get; init; }

    /// <summary>Canonical URL of the release page.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}