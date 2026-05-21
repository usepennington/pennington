namespace Pennington.BlogSite;

using FrontMatter;
using Pennington.BlogSite.StructuredData;
using Pennington.StructuredData;

/// <summary>
/// Front matter bound by <see cref="BlogSiteServiceExtensions.AddBlogSite"/>. Consolidates all
/// post-authoring fields (<see cref="Author"/>, <see cref="Repository"/>, <see cref="Series"/>,
/// <see cref="Date"/>, <see cref="RedirectUrl"/>) in one contract. Implements
/// <see cref="IFrontMatter"/>, <see cref="ITaggable"/>, <see cref="ISectionable"/>,
/// <see cref="IRedirectable"/>, and <see cref="IHasStructuredData"/> (emits a
/// schema.org <c>Article</c>).
/// </summary>
public record BlogSiteFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IRedirectable, IHasStructuredData
{
    /// <summary>Post title.</summary>
    public string Title { get; init; } = "Empty title";

    /// <summary>Author name shown in the byline and RSS feed.</summary>
    public string Author { get; init; } = "";

    /// <summary>Short post description used for meta tags, RSS summary, and archive listings.</summary>
    public string? Description { get; init; }

    /// <summary>URL to the post's source repository or related project (optional).</summary>
    public string Repository { get; init; } = "";

    /// <summary>Publication date. Posts are ordered by this date in archives and feeds.</summary>
    public DateTime? Date { get; init; }

    /// <summary>When true, the post is skipped during production builds.</summary>
    public bool IsDraft { get; init; }

    /// <summary>Tags applied to the post for filtering and the tag index.</summary>
    public string[] Tags { get; init; } = [];

    /// <summary>Series name grouping related posts together.</summary>
    public string Series { get; init; } = "";

    /// <summary>When set, the post emits a client-side redirect to this URL instead of normal content.</summary>
    public string? RedirectUrl { get; init; }

    /// <summary>Section heading the post belongs under in navigation.</summary>
    public string? SectionLabel { get; init; }

    /// <summary>Stable identifier used for cross-references (<c>[text](xref:uid)</c>).</summary>
    public string? Uid { get; init; }

    /// <summary>When false, the post is excluded from the search index.</summary>
    public bool Search { get; init; } = true;

    /// <summary>When false, the post is excluded from the generated llms.txt output.</summary>
    public bool Llms { get; init; } = true;

    /// <summary>When true, the post is indexed for search/llms but hidden from the rendered navigation tree.</summary>
    public bool SearchOnly { get; init; }

    /// <inheritdoc />
    public IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context)
    {
        var authorName = !string.IsNullOrEmpty(Author) ? Author
            : !string.IsNullOrEmpty(context.FallbackAuthorName) ? context.FallbackAuthorName
            : null;

        yield return new JsonLdArticle
        {
            Headline = Title,
            Description = Description,
            Url = context.CanonicalUrl,
            DatePublished = Date,
            Author = authorName is null ? null : new JsonLdPerson { Name = authorName },
        };
    }
}