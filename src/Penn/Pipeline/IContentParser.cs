namespace Pennington.Pipeline;

/// <summary>
/// Parses a discovered item into a parsed item (extracts front matter + markdown body).
/// </summary>
public interface IContentParser
{
    /// <summary>
    /// Parse a discovered item. Returns ParsedItem on success, FailedItem on failure.
    /// </summary>
    Task<ContentItem> ParseAsync(DiscoveredItem item);
}
