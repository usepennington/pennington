namespace BeyondRemoteContentExample;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Sources release notes from the GitHub Releases API instead of the file system.
/// The one HTTP fetch is cached in an <see cref="AsyncLazy{T}"/> and shared by every
/// pipeline pass — discovery, the TOC, cross-references, and the rendering endpoint —
/// so a build calls the API exactly once.
/// <para>
/// Backs how-to <c>how-to/content-services/source-from-a-remote-api.md</c>.
/// </para>
/// </summary>
public sealed class GitHubReleasesContentService : IContentService
{
    private readonly AsyncLazy<ImmutableList<ReleaseEntry>> _entriesLazy;

    /// <summary>Creates the service; the fetch runs once on first access and is then cached.</summary>
    public GitHubReleasesContentService(GitHubReleasesClient client)
        => _entriesLazy = new AsyncLazy<ImmutableList<ReleaseEntry>>(() => LoadAsync(client));

    /// <inheritdoc/>
    public string DefaultSectionLabel => "Releases";

    /// <inheritdoc/>
    public int SearchPriority => 20;

    /// <summary>All releases, newest first. Awaits the one shared fetch.</summary>
    public Task<ImmutableList<ReleaseEntry>> GetEntriesAsync() => _entriesLazy.Task;

    /// <summary>Finds a single release by its URL slug, or null when none matches.</summary>
    public async Task<ReleaseEntry?> TryGetAsync(string version)
    {
        var entries = await _entriesLazy;
        return entries.FirstOrDefault(e => e.Version == version);
    }

    /// <summary>
    /// The index route plus one route per release, each paired with
    /// <see cref="EndpointSource"/>: the build crawler fetches the URL through the
    /// sibling <c>MapGet</c> endpoint, which renders the HTML. Because that endpoint
    /// serves real canonical HTML, these routes are included in <c>sitemap.xml</c>
    /// like any other page.
    /// </summary>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        yield return new DiscoveredItem(
            ContentRouteFactory.FromUrl(new UrlPath("/releases/")),
            new EndpointSource());

        foreach (var entry in await _entriesLazy)
        {
            yield return new DiscoveredItem(
                ContentRouteFactory.FromUrl(new UrlPath($"/releases/{entry.Version}/")),
                new EndpointSource());
        }
    }

    /// <summary>No static files to copy — the API response is transformed, not republished.</summary>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <summary>TOC entries place the releases in navigation and the search index.</summary>
    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var entries = await _entriesLazy;
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

        return builder.ToImmutable();
    }

    /// <summary>One cross-reference per release so <c>&lt;xref:release-9.1.0&gt;</c> resolves.</summary>
    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var entries = await _entriesLazy;
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var entry in entries)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath($"/releases/{entry.Version}/"));
            builder.Add(new CrossReference($"release-{entry.Version}", entry.Title, route));
        }

        return builder.ToImmutable();
    }

    private static async Task<ImmutableList<ReleaseEntry>> LoadAsync(GitHubReleasesClient client)
    {
        var releases = await client.GetReleasesAsync();
        return
        [
            .. releases
                .Select(r => new ReleaseEntry(
                    Version: ToSlug(r.TagName),
                    Title: string.IsNullOrWhiteSpace(r.Name) ? r.TagName : r.Name,
                    BodyMarkdown: r.Body,
                    Date: r.PublishedAt,
                    HtmlUrl: r.HtmlUrl))
                .OrderByDescending(e => e.Date ?? DateTimeOffset.MinValue)
        ];
    }

    // Tags are conventionally "v1.2.3"; the URL slug drops a leading "v" before a digit.
    private static string ToSlug(string tagName)
        => tagName.Length > 1 && tagName[0] is 'v' or 'V' && char.IsDigit(tagName[1])
            ? tagName[1..]
            : tagName;
}

/// <summary>One release projected from the API onto the fields the site renders.</summary>
/// <param name="Version">URL slug derived from the tag (for example <c>9.1.0</c>).</param>
/// <param name="Title">Display title.</param>
/// <param name="BodyMarkdown">Release notes as markdown, rendered by the endpoint.</param>
/// <param name="Date">Publication date.</param>
/// <param name="HtmlUrl">Canonical URL of the release on GitHub.</param>
public sealed record ReleaseEntry(
    string Version,
    string Title,
    string? BodyMarkdown,
    DateTimeOffset? Date,
    string HtmlUrl);
