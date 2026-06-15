namespace Pennington.Docs;

// POCO targets for Data/showcase.yml. The showcase component deserializes the file
// with SharpYaml under a camelCase naming policy, so the file uses lowercase keys.

/// <summary>Root of the showcase data file — a list of sites to feature.</summary>
public sealed class ShowcaseData
{
    /// <summary>The featured sites, rendered top to bottom in file order.</summary>
    public List<ShowcaseEntry> Entries { get; set; } = [];
}

/// <summary>One featured site in the "Built with Pennington" gallery.</summary>
public sealed class ShowcaseEntry
{
    /// <summary>Display name (also the heading and outline label).</summary>
    public string Name { get; set; } = "";

    /// <summary>Display URL shown in the faux browser bar and under the title (no scheme).</summary>
    public string Url { get; set; } = "";

    /// <summary>Full URL the "Visit site" button links to.</summary>
    public string Href { get; set; } = "";

    /// <summary>Site-relative path to the screenshot, e.g. <c>/showcase/vcr.png</c>.</summary>
    public string Screenshot { get; set; } = "";

    /// <summary>
    /// Optional site-relative path to a dark-mode screenshot, e.g. <c>/showcase/vcr-dark.png</c>.
    /// When set, the gallery swaps to it while the docs site is in dark mode; when empty, the
    /// light <see cref="Screenshot"/> shows in both modes (for sites with no dark theme).
    /// </summary>
    public string ScreenshotDark { get; set; } = "";

    /// <summary>What the project is. Supports inline <c>`code`</c> spans.</summary>
    public string Description { get; set; } = "";

    /// <summary>The single most notable thing about how this site uses Pennington. Supports inline <c>`code`</c> spans.</summary>
    public string Notable { get; set; } = "";

    /// <summary>Short feature tags rendered as mono chips.</summary>
    public List<string> Chips { get; set; } = [];
}
