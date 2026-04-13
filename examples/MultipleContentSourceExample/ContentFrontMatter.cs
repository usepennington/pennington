namespace MultipleContentSourceExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for general content pages (root-level markdown files).
/// </summary>
public record ContentFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable, IRedirectable
{
    public string Title { get; init; } = "Untitled";
    public int Order { get; init; }
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public string? Uid { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
}