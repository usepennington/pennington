namespace Penn.StructuredData;

/// <summary>JSON-LD Article schema for content pages.</summary>
public record JsonLdArticle(
    string Headline,
    string? Description,
    string Url,
    DateTime? DatePublished,
    string? AuthorName
);

/// <summary>A single item in a JSON-LD BreadcrumbList.</summary>
public record JsonLdBreadcrumbItem(int Position, string Name, string? Url);

/// <summary>JSON-LD BreadcrumbList schema.</summary>
public record JsonLdBreadcrumbList(IReadOnlyList<JsonLdBreadcrumbItem> Items);

/// <summary>JSON-LD WebSite schema for homepages.</summary>
public record JsonLdWebSite(string Name, string Url, string? Description);
