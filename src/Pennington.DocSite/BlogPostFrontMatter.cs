namespace Pennington.DocSite;

using FrontMatter;

/// <summary>
/// Front matter for blog posts under the content project's <c>blog</c> folder. Bound by
/// <see cref="DocSiteServiceExtensions.AddDocSite"/> when the blog is active. Implements
/// <see cref="IFrontMatter"/>, <see cref="ITaggable"/>, and <see cref="IRedirectable"/>.
/// </summary>
public record BlogPostFrontMatter : IFrontMatter, ITaggable, IRedirectable
{
    /// <summary>Post title rendered in the browser tab and post heading.</summary>
    public string Title { get; init; } = "";

    /// <summary>Short description used for the meta description and post listings.</summary>
    public string? Description { get; init; }

    /// <summary>Author name shown in the post byline and RSS feed.</summary>
    public string Author { get; init; } = "";

    /// <summary>Publication date. Posts are ordered by this date, newest first.</summary>
    public DateTime? Date { get; init; }

    /// <summary>When true, the post is skipped during production builds.</summary>
    public bool IsDraft { get; init; }

    /// <summary>Tags applied to the post for the tag index and browse-by-tag pages.</summary>
    public string[] Tags { get; init; } = [];

    /// <summary>When set, the post emits a client-side redirect to this URL instead of normal content.</summary>
    public string? RedirectUrl { get; init; }

    /// <summary>Stable identifier used for cross-references (<c>[text](xref:uid)</c>).</summary>
    public string? Uid { get; init; }

    /// <summary>When false, the post is excluded from the search index.</summary>
    public bool Search { get; init; } = true;

    /// <summary>When false, the post is excluded from the generated llms.txt output.</summary>
    public bool Llms { get; init; } = true;

    /// <summary>
    /// Always true: posts are indexed for search and llms.txt but kept out of the
    /// documentation navigation sidebar. Not author-settable — the blog has its own
    /// index and tag pages.
    /// </summary>
    public bool SearchOnly => true;
}