using Pennington.FrontMatter;

namespace UserInterfaceExample;

public record DocsFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable, IRedirectable
{
    public string Title { get; init; } = "Empty title";
    public string? Description { get; init; }
    public bool IsDraft { get; init; } = false;
    public string[] Tags { get; init; } = [];
    public int Order { get; init; } = int.MaxValue;
    public string? Uid { get; init; } = null;
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
}
