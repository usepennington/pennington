namespace Pennington.Islands;

using Routing;

/// <summary>Renders a named island fragment embedded in an SPA envelope.</summary>
public interface IIslandRenderer
{
    /// <summary>Unique key identifying this island in the envelope's islands dictionary.</summary>
    string IslandName { get; }

    /// <summary>Renders the island HTML for the given route, or returns an empty string to skip.</summary>
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
