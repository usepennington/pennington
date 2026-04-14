namespace DocSiteAuthorExample;

/// <summary>
/// Stage 1 — a DocSite page at its bare minimum: a YAML front-matter block
/// with just the keys <c>DocSiteFrontMatter</c> expects for the sidebar and
/// meta description, followed by a single <c>&lt;h1&gt;</c>. No alert, no
/// tabs — just the page skeleton the later stages build on. Tutorial prose
/// extracts the body of <see cref="Source"/> via <c>xmldocid,bodyonly</c>.
/// </summary>
public static class Stage1
{
    /// <summary>The stage-1 markdown — front matter plus an h1.</summary>
    public static string Source() => """
        ---
        title: Authoring a doc page
        description: Populate DocSiteFrontMatter, add an alert, and group code samples into tabs.
        tags:
          - authoring
          - front-matter
          - markdown
        sectionLabel: Guides
        order: 20
        ---

        # Authoring a doc page

        This page demonstrates every authoring surface a typical DocSite page uses.
        """;
}
