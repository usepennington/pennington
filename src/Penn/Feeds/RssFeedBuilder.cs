namespace Penn.Feeds;

using System.Collections.Immutable;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Builds RSS feed items from rendered content.
/// </summary>
public sealed class RssFeedBuilder
{
    private readonly UrlPath _canonicalBase;

    public RssFeedBuilder(UrlPath canonicalBase)
    {
        _canonicalBase = canonicalBase;
    }

    /// <summary>
    /// Build RSS feed items from rendered items.
    /// Only includes items that are IDateable and not drafts.
    /// Sorted by date descending.
    /// </summary>
    public ImmutableList<RssFeedItem> Build(IReadOnlyList<RenderedItem> items)
    {
        var eligible = new List<(RenderedItem Item, DateTime Date)>();

        foreach (var item in items)
        {
            if (item.Metadata is IDraftable { IsDraft: true })
                continue;

            if (item.Metadata is IDateable { Date: { } date })
            {
                eligible.Add((item, date));
            }
        }

        return eligible
            .OrderByDescending(e => e.Date)
            .Select(e =>
            {
                var absoluteUrl = e.Item.Route.AbsoluteUrl(_canonicalBase);
                var description = (e.Item.Metadata as IDescribable)?.Description;

                return new RssFeedItem(
                    Title: e.Item.Metadata.Title,
                    Description: description,
                    Url: absoluteUrl,
                    PublishDate: e.Date,
                    Author: null
                );
            })
            .ToImmutableList();
    }
}
