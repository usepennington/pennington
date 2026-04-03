namespace Penn.Feeds;

using Penn.Routing;

public record SitemapEntry(
    UrlPath Url,
    DateTime? LastModified,
    string? ChangeFrequency,
    double? Priority
);
