namespace Pennington.Markdown.Extensions.Tabs;

/// <summary>
/// Options for customizing the CSS classes used in the content-tabs renderer. The tab strip
/// carries <c>not-prose</c> while the panels do not, so panel content keeps the page's prose
/// typography.
/// </summary>
public record ContentTabsRenderOptions
{
    /// <summary>Default CSS class configuration used by the content-tabs renderer.</summary>
    public static readonly ContentTabsRenderOptions Default = new()
    {
        ContainerCss = "ctabs",
        TabListCss = "ctabs-bar not-prose",
        TabButtonCss = "ctab-btn",
        TabPanelCss = "ctab-panel",
    };

    /// <summary>CSS classes for the outer container; intentionally not <c>not-prose</c>.</summary>
    public required string ContainerCss { get; init; }

    /// <summary>CSS classes for the tab strip; carries <c>not-prose</c> to isolate the buttons.</summary>
    public required string TabListCss { get; init; }

    /// <summary>CSS classes for each tab button.</summary>
    public required string TabButtonCss { get; init; }

    /// <summary>CSS classes for each tab panel; intentionally not <c>not-prose</c>.</summary>
    public required string TabPanelCss { get; init; }
}
