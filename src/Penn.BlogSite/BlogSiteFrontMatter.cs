namespace Penn.BlogSite;

using Penn.FrontMatter;

/// <summary>
/// Front matter for blog site posts.
/// </summary>
public record BlogSiteFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable, ISectionable, IRedirectable
{
    public string Title { get; init; } = "Empty title";
    public string Author { get; init; } = "";
    public string? Description { get; init; }
    public string Repository { get; init; } = "";
    public DateTime? Date { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string Series { get; init; } = "";
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
    public string? Uid { get; init; }
}
