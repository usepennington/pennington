namespace YogaStudioExample.Models;

using Pennington.FrontMatter;

public record YogaBlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }
    public string? Uid { get; init; }
    public string? Author { get; init; }
    public string? FeaturedImage { get; init; }
    public int ReadingTimeMinutes { get; init; }
}
