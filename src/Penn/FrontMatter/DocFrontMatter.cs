namespace Penn.FrontMatter;

/// <summary>
/// Covers the DocSite use case — implements common doc capabilities.
/// </summary>
public record DocFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Section { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
}
