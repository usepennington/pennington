namespace Pennington.Feeds;

using System.Collections.Immutable;
using FrontMatter;
using Routing;

/// <summary>
/// A candidate row for the sitemap — a route and (optionally) the front matter
/// metadata that was parsed for it. For markdown sources, metadata carries
/// <see cref="IFrontMatter.Date"/> (lastmod) and <see cref="IFrontMatter.IsDraft"/> (filter).
/// For programmatic / Razor sources we typically have no metadata, which is
/// fine: those entries are emitted with their URL and no lastmod.
/// </summary>
/// <param name="Route">Route to emit in the sitemap.</param>
/// <param name="Metadata">Front matter for the route, when available.</param>
public sealed record SitemapCandidate(ContentRoute Route, IFrontMatter? Metadata);

/// <summary>
/// Builds sitemap entries from a set of <see cref="SitemapCandidate"/> rows.
/// </summary>
public sealed class SitemapBuilder
{
    private readonly UrlPath _canonicalBase;
    private readonly TimeProvider _clock;

    /// <summary>Canonical site base URL used when resolving absolute entry URLs.</summary>
    public UrlPath CanonicalBase => _canonicalBase;

    /// <summary>
    /// Initializes the builder with the canonical site base URL used to produce absolute entry URLs
    /// and the wall clock used to filter out future-dated (scheduled) entries.
    /// </summary>
    public SitemapBuilder(UrlPath canonicalBase, TimeProvider? clock = null)
    {
        _canonicalBase = canonicalBase;
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>
    /// Build sitemap entries from candidate rows. Excludes drafts and future-dated (scheduled) entries.
    /// </summary>
    public ImmutableList<SitemapEntry> Build(IReadOnlyList<SitemapCandidate> candidates)
    {
        var builder = ImmutableList.CreateBuilder<SitemapEntry>();

        foreach (var candidate in candidates)
        {
            if (candidate.Metadata is { } metadata && metadata.IsHiddenFromBuild(_clock))
            {
                continue;
            }
            // Redirects have no sitemap meaning — they aren't canonical URLs.
            if (candidate.Metadata is IRedirectable { RedirectUrl: { Length: > 0 } })
            {
                continue;
            }

            var absoluteUrl = candidate.Route.AbsoluteUrl(_canonicalBase);
            var lastModified = candidate.Metadata?.Date;

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