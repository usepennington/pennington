namespace Pennington.Content;

using System.Collections.Immutable;
using Pipeline;

/// <summary>
/// Discovers and provides content for the pipeline.
/// </summary>
public interface IContentService : IContentEmitter
{
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
    /// Cross-references for xref resolution.
    /// </summary>
    Task<ImmutableList<CrossReference>> GetCrossReferencesAsync();

    /// <summary>
    /// Redirect sources this service emits (each item's <see cref="DiscoveredItem.Source"/>
    /// is a <see cref="Pipeline.RedirectSource"/>). Consumed by
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