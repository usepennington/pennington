namespace NorthwindHandbookExample;

using Pennington.FrontMatter;

public record ChangelogFrontMatter : IFrontMatter, IOrderable, ITaggable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public DateTime? Date { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string[] Tags { get; init; } = [];
    public string Version { get; init; } = "";
    public bool IsBreaking { get; init; }
}
