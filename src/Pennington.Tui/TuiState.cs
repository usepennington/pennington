namespace Pennington.Tui;

using Pennington.Content;
using XenoAtom.Terminal.UI;

/// <summary>
/// Snapshot surface the TUI view reads each frame. Plain reference fields carry the
/// data; <see cref="RenderTick"/> is a reactive <see cref="State{Int32}"/> the view
/// subscribes to so it actually re-renders when those fields change — XenoAtom's
/// dependency tracker only observes <c>State&lt;T&gt;.Value</c> reads, so views that
/// read raw fields would paint once and never update.
/// </summary>
internal sealed class TuiState
{
    /// <summary>Base URL Kestrel bound to, or <c>null</c> if not yet known.</summary>
    public string? AppUrl { get; internal set; }

    /// <summary>Locale codes configured for this site.</summary>
    public IReadOnlyList<string> Locales { get; internal set; } = [];

    /// <summary>
    /// TOC entries grouped by the <see cref="IContentService"/> that produced them.
    /// The Content tab renders one TreeView root per group so authors can see which
    /// service is responsible for each page.
    /// </summary>
    public IReadOnlyList<ContentGroup> ContentGroups { get; internal set; } = [];

    /// <summary>
    /// Bumped on every TUI tick from <see cref="Views.TuiApp"/>. Every reactive
    /// <c>TextBlock(() =&gt; ...)</c> lambda reads <c>RenderTick.Value</c> so the
    /// framework's dependency tracker re-runs the lambda on each tick.
    /// </summary>
    public State<int> RenderTick { get; } = new(0);
}

/// <summary>Per-<see cref="IContentService"/> bundle of TOC items surfaced in the Content tab.</summary>
/// <param name="ServiceLabel">Pretty-printed service type name (e.g. <c>MarkdownContentService&lt;DocFrontMatter&gt;</c>).</param>
/// <param name="DefaultSectionLabel">The service's <see cref="IContentService.DefaultSectionLabel"/>, shown as a dim subtitle.</param>
/// <param name="Items">Items the service returned on the most recent refresh.</param>
internal readonly record struct ContentGroup(
    string ServiceLabel,
    string DefaultSectionLabel,
    IReadOnlyList<ContentTocItem> Items);