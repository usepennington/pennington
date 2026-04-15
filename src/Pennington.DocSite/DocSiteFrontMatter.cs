namespace Pennington.DocSite;

using FrontMatter;

/// <summary>
/// Front matter bound by <see cref="DocSiteServiceExtensions.AddDocSite"/>. Extends the
/// <see cref="FrontMatter.DocFrontMatter"/> shape with <see cref="RedirectUrl"/> via
/// <see cref="IRedirectable"/>. Implements <see cref="IFrontMatter"/>, <see cref="ITaggable"/>,
/// <see cref="ISectionable"/>, <see cref="IOrderable"/>, and <see cref="IRedirectable"/>.
/// </summary>
public record DocSiteFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public int Order { get; init; } = int.MaxValue;
    public string? RedirectUrl { get; init; }
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
}