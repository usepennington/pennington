namespace Pennington.Feeds;

using Routing;

/// <summary>
/// Single entry in a generated RSS feed.
/// </summary>
/// <param name="Title">Item title shown in the feed.</param>
/// <param name="Description">Optional summary or excerpt.</param>
/// <param name="Url">Site-relative canonical path of the entry; <see cref="RssFeedWriter"/> composes the absolute link.</param>
/// <param name="PublishDate">Publication date, when known.</param>
/// <param name="Author">Optional author name or email.</param>
public record RssFeedItem(
    string Title,
    string? Description,
    UrlPath Url,
    DateTime? PublishDate,
    string? Author
);