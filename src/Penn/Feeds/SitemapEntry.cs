namespace Pennington.Feeds;

using Pennington.Routing;

public record SitemapEntry(
    UrlPath Url,
    DateTime? LastModified,
    string? ChangeFrequency,
    double? Priority
);
