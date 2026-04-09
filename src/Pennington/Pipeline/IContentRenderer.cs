namespace Pennington.Pipeline;

/// <summary>
/// Renders a parsed item into HTML.
/// </summary>
public interface IContentRenderer
{
    /// <summary>
    /// Render a parsed item. Returns RenderedItem on success, FailedItem on failure.
    /// </summary>
    Task<ContentItem> RenderAsync(ParsedItem item);
}
