namespace ForgePortalExample;

using Penn.FrontMatter;

public record PageFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "";
}
