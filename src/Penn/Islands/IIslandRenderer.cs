namespace Pennington.Islands;

using Pennington.Routing;

public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
