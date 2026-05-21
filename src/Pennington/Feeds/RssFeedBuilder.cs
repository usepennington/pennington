namespace Pennington.Feeds;

using System.Collections.Immutable;
using FrontMatter;
using Pipeline;
using Routing;

/// <summary>
/// Builds RSS feed items from rendered content.
/// </summary>
public sealed class RssFeedBuilder
{
    private readonly UrlPath _canonicalBase;
    private readonly TimeProvider _clock;

    /// <summary>
    /// Initializes the builder with the canonical site base URL used to produce absolute item links
    /// and the wall clock used to filter out future-dated (scheduled) posts.
    /// </summary>
    public RssFeedBuilder(UrlPath canonicalBase, TimeProvider? clock = null)
    {
        _canonicalBase = canonicalBase;
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>
    /// Build RSS feed items from rendered items.
    /// Only includes items that have a Date, are not drafts, and are not scheduled for the future.
    /// Sorted by date descending.
    /// </summary>
    public ImmutableList<RssFeedItem> Build(IReadOnlyList<RenderedItem> items)
    {
        var eligible = new List<(RenderedItem Item, DateTime Date)>();

        foreach (var item in items)
        {
            if (item.Metadata.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            if (item.Metadata.Date is { } date)
            {
                eligible.Add((item, date));
            }
        }

        return eligible
            .OrderByDescending(e => e.Date)
            .Select(e =>
            {
                var absoluteUrl = e.Item.Route.AbsoluteUrl(_canonicalBase);
                var description = e.Item.Metadata.Description;

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