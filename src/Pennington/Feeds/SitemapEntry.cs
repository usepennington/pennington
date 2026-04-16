namespace Pennington.Feeds;

using Routing;

/// <summary>
/// Single <c>&lt;url&gt;</c> row written to sitemap.xml.
/// </summary>
/// <param name="Url">Absolute URL for this sitemap entry.</param>
/// <param name="LastModified">Last-modified timestamp, when available.</param>
/// <param name="ChangeFrequency">Optional sitemap <c>changefreq</c> hint (e.g. <c>weekly</c>).</param>
/// <param name="Priority">Optional sitemap <c>priority</c> hint between 0.0 and 1.0.</param>
public record SitemapEntry(
    UrlPath Url,
    DateTime? LastModified,
    string? ChangeFrequency,
    double? Priority
);