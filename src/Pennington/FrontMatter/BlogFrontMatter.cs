namespace Pennington.FrontMatter;

/// <summary>
/// Core-library front matter for blog posts on bare
/// <see cref="Infrastructure.PenningtonExtensions.AddPennington"/> hosts.
/// Carries <see cref="Date"/>, <see cref="Author"/>, and <see cref="Series"/> alongside the
/// <see cref="IFrontMatter"/> defaults, and implements <see cref="IFrontMatter"/> and
/// <see cref="ITaggable"/>. Not the record bound by <c>AddBlogSite</c> — see
/// <see cref="BlogSite.BlogSiteFrontMatter"/> for that.
/// </summary>
public record BlogFrontMatter : IFrontMatter, ITaggable
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