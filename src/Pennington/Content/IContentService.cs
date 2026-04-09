namespace Pennington.Content;

using System.Collections.Immutable;
using Pennington.Pipeline;

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
    /// Cross-references for xref resolution.
    /// </summary>
    Task<ImmutableList<CrossReference>> GetCrossReferencesAsync();

    string DefaultSection { get; }
    int SearchPriority { get; }
}
