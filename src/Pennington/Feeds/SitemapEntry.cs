namespace Pennington.Feeds;

using Routing;

public record SitemapEntry(
    UrlPath Url,
    DateTime? LastModified,
    string? ChangeFrequency,
    double? Priority
);