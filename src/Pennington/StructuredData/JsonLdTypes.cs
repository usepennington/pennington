namespace Pennington.StructuredData;

/// <summary>JSON-LD Article schema for content pages.</summary>
/// <param name="Headline">Article headline.</param>
/// <param name="Description">Short description of the article.</param>
/// <param name="Url">Canonical URL of the article.</param>
/// <param name="DatePublished">Publication date, when known.</param>
/// <param name="AuthorName">Display name of the author, when known.</param>
public record JsonLdArticle(
    string Headline,
    string? Description,
    string Url,
    DateTime? DatePublished,
    string? AuthorName
);

/// <summary>A single item in a JSON-LD BreadcrumbList.</summary>
/// <param name="Position">1-based position of the item in the crumb trail.</param>
/// <param name="Name">Display name for this crumb.</param>
/// <param name="Url">URL the crumb links to, when known.</param>
public record JsonLdBreadcrumbItem(int Position, string Name, string? Url);

/// <summary>JSON-LD BreadcrumbList schema.</summary>
/// <param name="Items">Ordered crumb items forming the trail.</param>
public record JsonLdBreadcrumbList(IReadOnlyList<JsonLdBreadcrumbItem> Items);

/// <summary>JSON-LD WebSite schema for homepages.</summary>
/// <param name="Name">Site name.</param>
/// <param name="Url">Site canonical URL.</param>
/// <param name="Description">Short site description, when known.</param>
public record JsonLdWebSite(string Name, string Url, string? Description);