namespace ForgePortalExample;

using Pennington.FrontMatter;

public record PageFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "";
}
