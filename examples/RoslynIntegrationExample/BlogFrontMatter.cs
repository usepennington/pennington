namespace RoslynIntegrationExample;

using Penn.FrontMatter;

/// <summary>
/// Front matter for the Roslyn integration example site.
/// </summary>
public record BlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable, IRedirectable, ISectionable
{
    public string Title { get; init; } = "Empty title";
    public string? Description { get; init; }
    public string? Uid { get; init; }
    public DateTime? Date { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
}
