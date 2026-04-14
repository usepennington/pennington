namespace ExtensibilityLabExample;

using Pennington.Infrastructure;
using Pennington.Markdown.Extensions.Tabs;

/// <summary>
/// Overrides the CSS class names emitted by the tabbed-code-block renderer
/// by handing <see cref="PenningtonOptions.TabbedCodeBlockOptions"/> a factory
/// that returns a modified <see cref="TabbedCodeBlockRenderOptions"/>.
/// Fenced by step 4 of how-to/content-authoring/tabbed-code.
/// </summary>
public static class TabbedCodeBlockStyling
{
    /// <summary>
    /// Swap the default <c>tab-container</c> / <c>tab-list</c> / <c>tab-button</c>
    /// / <c>tab-panel</c> class names for the <c>lab-tabs*</c> variants this
    /// example ships with.
    /// </summary>
    public static void ConfigureTabbedCodeBlocksOverride(PenningtonOptions penn)
    {
        penn.TabbedCodeBlockOptions = () => TabbedCodeBlockRenderOptions.Default with
        {
            OuterWrapperCss = "not-prose",
            ContainerCss = "lab-tabs",
            TabListCss = "lab-tabs-list",
            TabButtonCss = "lab-tabs-button",
            TabPanelCss = "lab-tabs-panel",
        };
    }
}
