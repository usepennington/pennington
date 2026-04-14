namespace Pennington.FrontMatter;

/// <summary>
/// Covers the DocSite use case — implements common doc capabilities.
/// </summary>
public record DocFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
}