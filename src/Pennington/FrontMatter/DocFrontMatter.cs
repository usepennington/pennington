namespace Pennington.FrontMatter;

/// <summary>
/// Core-library front matter for documentation pages on bare
/// <see cref="Infrastructure.PenningtonExtensions.AddPennington"/> hosts.
/// Implements <see cref="IFrontMatter"/>, <see cref="ITaggable"/>, <see cref="ISectionable"/>,
/// and <see cref="IOrderable"/> — the default capability shape for doc content without the
/// DocSite template. Hosts using <c>AddDocSite</c> bind
/// <see cref="DocSite.DocSiteFrontMatter"/> instead.
/// </summary>
public record DocFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
}