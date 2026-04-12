namespace MultipleContentSourceExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for blog posts.
/// </summary>
public record BlogFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IRedirectable
{
    public string Title { get; init; } = "Empty title";
    public string? Description { get; init; }
    public DateTime? Date { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Uid { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
}
