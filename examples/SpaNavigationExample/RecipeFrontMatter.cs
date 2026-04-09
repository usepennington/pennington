namespace SpaNavigationExample;

using Pennington.FrontMatter;

public record RecipeFrontMatter : IFrontMatter, IDescribable, IOrderable, ITaggable, IDraftable, ICrossReferenceable, ISectionable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public int PrepTime { get; init; }
    public int CookTime { get; init; }
    public int Servings { get; init; }
    public string Difficulty { get; init; } = "Easy";
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public string? Uid { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
    public int Order { get; init; } = int.MaxValue;
}
