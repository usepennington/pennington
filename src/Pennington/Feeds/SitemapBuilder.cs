namespace Pennington.Feeds;

using System.Collections.Immutable;
using Pennington.FrontMatter;
using Pennington.Routing;

/// <summary>
/// A candidate row for the sitemap — a route and (optionally) the front matter
/// metadata that was parsed for it. For markdown sources, metadata carries
/// <see cref="IDateable"/> (lastmod) and <see cref="IDraftable"/> (filter).
/// For programmatic / Razor sources we typically have no metadata, which is
/// fine: those entries are emitted with their URL and no lastmod.
/// </summary>
public sealed record SitemapCandidate(ContentRoute Route, IFrontMatter? Metadata);

/// <summary>
/// Builds sitemap entries from a set of <see cref="SitemapCandidate"/> rows.
/// </summary>
public sealed class SitemapBuilder
{
    private readonly UrlPath _canonicalBase;

    public UrlPath CanonicalBase => _canonicalBase;

    public SitemapBuilder(UrlPath canonicalBase)
    {
        _canonicalBase = canonicalBase;
    }

    /// <summary>
    /// Build sitemap entries from candidate rows. Excludes drafts.
    /// </summary>
    public ImmutableList<SitemapEntry> Build(IReadOnlyList<SitemapCandidate> candidates)
    {
        var builder = ImmutableList.CreateBuilder<SitemapEntry>();

        foreach (var candidate in candidates)
        {
            if (candidate.Metadata is IDraftable { IsDraft: true })
                continue;
            // Redirects have no sitemap meaning — they aren't canonical URLs.
            if (candidate.Metadata is IRedirectable { RedirectUrl: { Length: > 0 } })
                continue;

            var absoluteUrl = candidate.Route.AbsoluteUrl(_canonicalBase);
            var lastModified = (candidate.Metadata as IDateable)?.Date;

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
