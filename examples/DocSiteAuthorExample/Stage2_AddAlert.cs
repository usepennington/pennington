namespace DocSiteAuthorExample;

/// <summary>
/// Stage 2 — Stage 1 plus a GitHub-style alert. The syntax is a plain
/// block quote whose first line is <c>[!KIND]</c>. Pennington's
/// <c>CustomAlertInlineParser</c> recognizes <c>NOTE</c>, <c>TIP</c>,
/// <c>IMPORTANT</c>, <c>WARNING</c>, and <c>CAUTION</c> and rewrites the
/// surrounding quote block into a <c>markdown-alert</c> container. Tutorial
/// prose extracts the body of <see cref="Source"/> via
/// <c>xmldocid,bodyonly</c>.
/// </summary>
public static class Stage2
{
    /// <summary>The stage-2 markdown — adds an alert block.</summary>
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

        ## Callouts

        > [!NOTE]
        > Alerts render with a coloured left border and an icon matching the kind.
        > Supported kinds include `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and
        > `CAUTION`.
        """;
}