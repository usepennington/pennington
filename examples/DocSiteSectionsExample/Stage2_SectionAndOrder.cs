namespace DocSiteSectionsExample;

/// <summary>
/// Stage 2 — the same page moved under a <c>getting-started/</c> subfolder
/// and decorated with <c>sectionLabel:</c> + <c>order:</c>. The subfolder is
/// what actually drives sidebar grouping: <c>NavigationBuilder</c>
/// auto-creates a "Getting Started" section node for the folder (kebab-case
/// gets title-cased). The <c>order: 10</c> key controls where the page sits
/// among its siblings inside that group — smaller numbers sort first, ties
/// break on title. The <c>sectionLabel:</c> key is metadata on the TOC entry
/// (surfaced as <c>NavigationInfo.SectionName</c>); it is the folder, not
/// the key, that groups the sidebar.
/// </summary>
public static class Stage2
{
    /// <summary>The stage-2 markdown — sectionLabel + order added, lives under getting-started/.</summary>
    public static string Source() => """
        ---
        title: Install Pennington
        description: Add the Pennington package to a new or existing ASP.NET project.
        sectionLabel: Getting Started
        order: 10
        ---

        # Install Pennington

        The first thing every new Pennington site needs is the package itself.
        """;
}