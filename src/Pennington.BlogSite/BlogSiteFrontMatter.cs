namespace Pennington.BlogSite;

using FrontMatter;

/// <summary>
/// Front matter bound by <see cref="BlogSiteServiceExtensions.AddBlogSite"/>. Consolidates all
/// post-authoring fields (<see cref="Author"/>, <see cref="Repository"/>, <see cref="Series"/>,
/// <see cref="Date"/>, <see cref="RedirectUrl"/>) in one contract. Implements
/// <see cref="IFrontMatter"/>, <see cref="ITaggable"/>, <see cref="ISectionable"/>, and
/// <see cref="IRedirectable"/>.
/// </summary>
public record BlogSiteFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IRedirectable
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
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
}