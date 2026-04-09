namespace YogaStudioExample.Models;

using Pennington.FrontMatter;

public record YogaFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Section { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string? HeroImage { get; init; }
    public string? Layout { get; init; }
}
