namespace Pennington.Content;

using System.Collections.Immutable;
using Pipeline;

/// <summary>
/// Discovers and provides content for the pipeline.
/// </summary>
public interface IContentService
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
    /// Dynamically generated files (search index, etc.)
    /// </summary>
    Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync();

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

    string DefaultSectionLabel { get; }
    int SearchPriority { get; }
}