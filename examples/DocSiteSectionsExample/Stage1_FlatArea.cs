namespace DocSiteSectionsExample;

/// <summary>
/// Stage 1 — a page that lives directly under an area folder with neither
/// <c>section:</c> nor <c>order:</c> in front matter. The sidebar lists it
/// as a flat entry at the top of the area's TOC because there is no
/// subfolder to group it under, and <c>order</c> defaults to
/// <c>int.MaxValue</c> so it sorts after anything with an explicit order.
/// Tutorial prose extracts <see cref="Source"/> via <c>xmldocid,bodyonly</c>.
/// </summary>
public static class Stage1
{
    /// <summary>The stage-1 markdown — bare front matter, no section or order.</summary>
    public static string Source() => """
        ---
        title: Install Pennington
        description: Add the Pennington package to a new or existing ASP.NET project.
        ---

        # Install Pennington

        The first thing every new Pennington site needs is the package itself.
        """;
}
