namespace Pennington.Content;

using System.Collections.Immutable;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Discovers and provides content for the pipeline.
/// </summary>
public interface IContentService : IContentEmitter
{
    /// <summary>
    /// Maps a file-change notification to the set of routes this service projects from
    /// that file, without mutating any cached state. Consulted by file-watched caches
    /// (<see cref="Pennington.Pipeline.SiteProjection"/>,
    /// <see cref="Pennington.Infrastructure.BuildHtmlCache"/>) to invalidate only the
    /// affected entries instead of clearing wholesale. Default: <see cref="ContentChangeImpact.None"/>.
    /// </summary>
    ContentChangeImpact GetAffectedRoutes(FileChangeNotification change)
        => ContentChangeImpact.None;

    /// <summary>
    /// Discover all content items this service is responsible for.
    /// </summary>
    IAsyncEnumerable<DiscoveredItem> DiscoverAsync();

    /// <summary>
    /// Static files to copy to output (images, downloads, etc.)
    /// </summary>
    Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync();

    /// <summary>
    /// Navigation entries for table of contents.
    /// </summary>
    Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync();

    /// <summary>
    /// Entries that should appear in the search index and llms.txt.
    /// <para>
    /// Default: returns <see cref="GetContentTocEntriesAsync"/>. That is
    /// correct when "shown in navigation" ≡ "discoverable via search" —
    /// the default holds for markdown, because
    /// <see cref="MarkdownContentService{T}"/>'s TOC entries already honor
    /// <c>search:</c> and <c>llms:</c> front-matter fields via
    /// <see cref="ContentTocItem.ExcludeFromSearch"/> /
    /// <see cref="ContentTocItem.ExcludeFromLlms"/>.
    /// </para>
    /// <para>
    /// Override when the two sets diverge — for example,
    /// <see cref="RazorPageContentService"/> emits sidecar-less pages here
    /// (so users can search for them) without adding them to navigation
    /// (which would clutter the TOC with auto-titled entries).
    /// </para>
    /// <para>
    /// Implementors of custom content services that build
    /// <see cref="ContentTocItem"/>s directly: set
    /// <see cref="ContentTocItem.ExcludeFromSearch"/> and
    /// <see cref="ContentTocItem.ExcludeFromLlms"/> from your metadata's
    /// <see cref="FrontMatter.IFrontMatter.Search"/> /
    /// <see cref="FrontMatter.IFrontMatter.Llms"/> flags, or per-page
    /// opt-outs will be silently ignored.
    /// </para>
    /// </summary>
    async Task<ImmutableList<ContentTocItem>> GetIndexableEntriesAsync()
    {
        return await GetContentTocEntriesAsync();
    }

    /// <summary>
    /// Projects this service's routable content as <see cref="ContentRecord"/>s — the discovery
    /// seam consumed by taxonomy, search faceting, and structured-data emission.
    /// <para>
    /// Default: bridges from <see cref="DiscoverAsync"/>, yielding one record per discovered item
    /// that carries <see cref="DiscoveredItem.Metadata"/> and is neither a <see cref="RedirectSource"/>
    /// (transport, not content) nor an <see cref="LlmsOnlySource"/> (no human-facing URL). A service
    /// that attaches typed metadata to its discovered items — as <see cref="MarkdownContentService{T}"/>
    /// does — therefore participates with no extra code. Override only to project records that do
    /// not flow through <see cref="DiscoverAsync"/>, or to suppress records entirely.
    /// </para>
    /// <para>
    /// A service that emits routable content from <see cref="DiscoverAsync"/> but leaves
    /// <see cref="DiscoveredItem.Metadata"/> unset projects no records, and so silently sits out of
    /// taxonomy, search faceting, and structured data. Set the metadata (or override this) to opt in.
    /// </para>
    /// </summary>
    async IAsyncEnumerable<ContentRecord> GetRecordsAsync()
    {
        await foreach (var item in DiscoverAsync())
        {
            if (item.Metadata is null || item.Source.Value is RedirectSource or LlmsOnlySource)
            {
                continue;
            }

            yield return new ContentRecord(item.Route, item.Metadata);
        }
    }

    /// <summary>
    /// Cross-references for xref resolution.
    /// </summary>
    Task<ImmutableList<CrossReference>> GetCrossReferencesAsync();

    /// <summary>
    /// Discovers and parses this service's content with the service's own front-matter
    /// type, yielding <see cref="ParsedItem"/>s (typed metadata + body). Consumers like
    /// <c>LlmsTxtService</c> use this instead of re-parsing with a foreign parser, which
    /// would mis-flag valid keys from other content types. Default: empty — services whose
    /// content is sourced elsewhere (Razor/API pages fetched as rendered HTML) opt out.
    /// </summary>
    IAsyncEnumerable<ParsedItem> ParseContentAsync()
        => System.Linq.AsyncEnumerable.Empty<ParsedItem>();

    /// <summary>
    /// Redirect sources this service emits (each item's <see cref="DiscoveredItem.Source"/>
    /// is a <see cref="RedirectSource"/>). Consumed by
    /// <see cref="RedirectContentService"/> to build the unified redirect map without
    /// iterating every service's <see cref="DiscoverAsync"/> — which would force
    /// services that have no redirects to pay the full cost of discovery just to
    /// return nothing. Default: empty. Services backed by front-matter records that
    /// implement <see cref="FrontMatter.IRedirectable"/> override this.
    /// </summary>
    Task<ImmutableList<DiscoveredItem>> GetRedirectSourcesAsync()
        => Task.FromResult(ImmutableList<DiscoveredItem>.Empty);

    /// <summary>
    /// Default section label applied to discovered items that do not supply one via front matter.
    /// </summary>
    string DefaultSectionLabel { get; }

    /// <summary>
    /// Relative priority for ordering results in the search index (higher values rank first).
    /// </summary>
    int SearchPriority { get; }
}