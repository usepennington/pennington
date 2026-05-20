namespace Pennington.Tui.Views;

using System.Text;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using static Pennington.Tui.Views.TuiMarkup;

/// <summary>
/// Content tab: every page grouped by the <see cref="Content.IContentService"/>
/// that returned it. Rendered as a <see cref="TreeView"/> so users can collapse a
/// noisy service (e.g. a redirect service with many aliases) and focus on another.
/// One row per item — kept concise (title + URL).
/// </summary>
internal static class ContentTab
{
    internal static Visual Build(TreeView pages) =>
        new Group()
            .TopLeftText("Pages by service")
            .Padding(1)
            .Content(pages)
            .Stretch();

    /// <summary>
    /// Rebuilds the tree when the content-groups reference changes. We swap the whole
    /// root list rather than mutating nodes in place — expansion state resets each
    /// refresh, but TOC refreshes only fire on real file changes so that's rare.
    /// </summary>
    internal static Action CreatePump(TuiState state, TreeView pages)
    {
        IReadOnlyList<ContentGroup>? lastRendered = null;
        string? lastAppUrl = null;

        return () =>
        {
            var groups = state.ContentGroups;
            var appUrl = state.AppUrl;
            if (ReferenceEquals(groups, lastRendered) && appUrl == lastAppUrl)
            {
                return;
            }

            lastRendered = groups;
            lastAppUrl = appUrl;

            var baseUrl = (appUrl ?? "").TrimEnd('/');
            var roots = new List<TreeNode>(groups.Count);
            foreach (var group in groups)
            {
                roots.Add(BuildServiceNode(group, baseUrl));
            }
            pages.Roots(roots);
        };
    }

    // TreeNode's Icon defaults to a page/folder glyph via the style's IconResolver.
    // Nulling the resolver on the style doesn't suppress it in every code path, so
    // we pin each node's Icon to a blank rune to guarantee no decorative chars.
    private static readonly Rune BlankIcon = new(' ');

    private static TreeNode BuildServiceNode(ContentGroup group, string baseUrl)
    {
        var sectionSuffix = string.IsNullOrEmpty(group.DefaultSectionLabel)
            ? ""
            : $" [dim]({Escape(group.DefaultSectionLabel)})[/]";
        var header = new Markup($"[bold]{Escape(group.ServiceLabel)}[/]{sectionSuffix}  [dim]{group.Items.Count} items[/]")
        {
            Wrap = false,
        };

        var node = new TreeNode(header) { IsExpanded = true, Icon = BlankIcon };

        foreach (var item in group.Items.OrderBy(e => e.SectionLabel ?? "").ThenBy(e => e.Order).ThenBy(e => e.Title))
        {
            node.Children.Add(BuildItemNode(item, baseUrl));
        }
        return node;
    }

    private static TreeNode BuildItemNode(Content.ContentTocItem item, string baseUrl)
    {
        var url = item.Route.CanonicalPath.Value;
        var full = baseUrl + url;
        var label = new Markup($"{Escape(item.Title)}  [cyan]{Escape(full)}[/]")
        {
            Wrap = false,
        };
        return new TreeNode(label) { Icon = BlankIcon };
    }
}