namespace Penn.Islands;

using Penn.Routing;

public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
