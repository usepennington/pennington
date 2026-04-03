namespace Penn.Feeds;

using System.Collections.Immutable;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Builds sitemap entries from rendered content.
/// </summary>
public sealed class SitemapBuilder
{
    private readonly UrlPath _canonicalBase;

    public SitemapBuilder(UrlPath canonicalBase)
    {
        _canonicalBase = canonicalBase;
    }

    /// <summary>
    /// Build sitemap entries from rendered items. Excludes drafts.
    /// </summary>
    public ImmutableList<SitemapEntry> Build(IReadOnlyList<RenderedItem> items)
    {
        var builder = ImmutableList.CreateBuilder<SitemapEntry>();

        foreach (var item in items)
        {
            if (item.Metadata is IDraftable { IsDraft: true })
                continue;

            var absoluteUrl = item.Route.AbsoluteUrl(_canonicalBase);
            var lastModified = (item.Metadata as IDateable)?.Date;

            builder.Add(new SitemapEntry(
                Url: absoluteUrl,
                LastModified: lastModified,
                ChangeFrequency: null,
                Priority: null
            ));
        }

        return builder.ToImmutable();
    }
}
