namespace Penn.Feeds;

using Penn.Routing;

public record RssFeedItem(
    string Title,
    string? Description,
    UrlPath Url,
    DateTime? PublishDate,
    string? Author
);
