namespace Pennington.Content;

using Pipeline;
using Routing;

/// <summary>
/// Resolves a requested URL to a fully rendered page by walking the registered
/// content services, parsing the matching item, and rendering it.
/// </summary>
public interface IPageResolver
{
    /// <summary>
    /// Returns the rendered page whose canonical route matches <paramref name="requested"/>,
    /// or <c>null</c> when nothing matches or the match fails to parse or render.
    /// </summary>
    Task<RenderedItem?> ResolveAsync(UrlPath requested);
}
