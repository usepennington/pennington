namespace Pennington.FrontMatter;

/// <summary>
/// Covers the BlogSite use case — implements blog capabilities.
/// </summary>
public record BlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable,
    ISearchable, ILlmsIndexable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }
    public string? Author { get; init; }
    public string? Series { get; init; }
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
}
