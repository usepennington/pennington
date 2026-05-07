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
    /// <summary>Page title rendered in the browser tab and page heading.</summary>
    public string Title { get; init; } = "";

    /// <summary>Short description used for the meta description and social cards.</summary>
    public string? Description { get; init; }

    /// <summary>When true, the page is skipped during production builds.</summary>
    public bool IsDraft { get; init; }

    /// <summary>Tags applied to this page for filtering and the tag index.</summary>
    public string[] Tags { get; init; } = [];

    /// <summary>Sort order within the containing section. Lower values appear first.</summary>
    public int Order { get; init; } = int.MaxValue;

    /// <summary>When set, the page emits a client-side redirect to this URL instead of normal content.</summary>
    public string? RedirectUrl { get; init; }

    /// <summary>Section heading this page belongs under in navigation.</summary>
    public string? SectionLabel { get; init; }

    /// <summary>Stable identifier used for cross-references (<c>[text](xref:uid)</c>).</summary>
    public string? Uid { get; init; }

    /// <summary>When false, the page is excluded from the search index.</summary>
    public bool Search { get; init; } = true;

    /// <summary>When false, the page is excluded from the generated llms.txt output.</summary>
    public bool Llms { get; init; } = true;

    /// <summary>When true, the page is indexed for search/llms but hidden from the rendered navigation tree.</summary>
    public bool SearchOnly { get; init; }
}