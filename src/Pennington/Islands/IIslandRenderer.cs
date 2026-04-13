namespace Pennington.Islands;

using Routing;

public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}