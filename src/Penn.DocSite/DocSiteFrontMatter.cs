namespace Penn.DocSite;

using Penn.FrontMatter;

/// <summary>
/// Front matter for doc site pages.
/// </summary>
public record DocSiteFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public int Order { get; init; } = int.MaxValue;
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
    public string? Uid { get; init; }
}
