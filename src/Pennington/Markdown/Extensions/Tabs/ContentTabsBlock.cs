namespace Pennington.Markdown.Extensions.Tabs;

using Markdig.Syntax;
using Markdig.Syntax.Inlines;

/// <summary>
/// Container block holding one or more <see cref="ContentTabBlock"/> children rendered as a
/// tabset. Built during document processing from a run of DocFX-style tab headings.
/// </summary>
internal sealed class ContentTabsBlock() : ContainerBlock(null);

/// <summary>
/// A single tab within a <see cref="ContentTabsBlock"/>; its child blocks form the panel content.
/// </summary>
internal sealed class ContentTabBlock() : ContainerBlock(null)
{
    /// <summary>Inline content of the tab heading link, rendered as the tab button label.</summary>
    public ContainerInline? Title { get; set; }

    /// <summary>Tab identifier — the first path segment after <c>#tab/</c>.</summary>
    public required string TabId { get; init; }

    /// <summary>Optional dependent-tab condition — the second path segment after <c>#tab/</c>.</summary>
    public string? Condition { get; init; }
}
