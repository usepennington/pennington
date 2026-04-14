namespace BlogSiteFirstPostExample;

/// <summary>
/// Stage 1 — the bare-minimum post front matter the reader starts with after
/// the scaffold tutorial: just <c>title</c>, <c>description</c>, and
/// <c>date</c>. This is enough for the post to render on the home listing,
/// the archive, and in the RSS feed, but leaves most of
/// <see cref="Pennington.BlogSite.BlogSiteFrontMatter"/> untouched. Tutorial
/// prose extracts the body of <see cref="Source"/> via
/// <c>csharp:xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>The stage-1 markdown — just title, description, and date.</summary>
    public static string Source() => """
        ---
        title: Shipping a tiny content engine for weekend projects
        description: Notes from the first month of building Pennington.
        date: 2026-04-10
        ---

        Welcome to the first real post on this blog. The scaffold from the
        previous tutorial gave us a running BlogSite with one placeholder
        post; this post replaces it.

        ## What's here so far

        Just title, description, and date. That is enough for the home
        listing, the archive page, and the RSS feed to light up.
        """;
}
