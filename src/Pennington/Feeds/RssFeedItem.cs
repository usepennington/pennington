namespace Pennington.Feeds;

using Pennington.Routing;

public record RssFeedItem(
    string Title,
    string? Description,
    UrlPath Url,
    DateTime? PublishDate,
    string? Author
);
