namespace Pennington.Feeds;

using Routing;

/// <summary>
/// Single <c>&lt;url&gt;</c> row written to sitemap.xml.
/// </summary>
/// <param name="Url">Absolute URL for this sitemap entry.</param>
/// <param name="LastModified">Last-modified timestamp, when available.</param>
public record SitemapEntry(
    UrlPath Url,
    DateTime? LastModified
);