namespace MultipleContentSourceExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for documentation pages.
/// </summary>
public record DocsFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IOrderable, ICrossReferenceable, IRedirectable
{
    public string Title { get; init; } = "Empty title";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? RedirectUrl { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string? Uid { get; init; }
}
