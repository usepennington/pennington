namespace Pennington.Pipeline;

using Generation;

/// <summary>
/// The content processing pipeline.
/// </summary>
public interface IContentPipeline
{
    /// <summary>
    /// Entry: content services produce discovered items.
    /// </summary>
    IAsyncEnumerable<ContentItem> DiscoverAsync();

    /// <summary>
    /// Transform: parse items (read files, extract YAML + body).
    /// </summary>
    IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items);

    /// <summary>
    /// Transform: render items (Markdig pipeline to HTML).
    /// </summary>
    IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items);

    /// <summary>
    /// Exit: generate output files.
    /// </summary>
    Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items, OutputOptions options);
}