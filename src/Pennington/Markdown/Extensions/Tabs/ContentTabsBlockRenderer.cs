namespace Pennington.Markdown.Extensions.Tabs;

using System.Net;
using Markdig.Renderers;
using Markdig.Renderers.Html;

/// <summary>
/// HTML renderer for <see cref="ContentTabsBlock"/>. Emits a <c>not-prose</c> tab strip above
/// prose-styled panels; panels with a <c>data-condition</c> participate in dependent-tab sync.
/// </summary>
internal sealed class ContentTabsBlockRenderer(Func<ContentTabsRenderOptions> optionsFactory)
    : HtmlObjectRenderer<ContentTabsBlock>
{
    protected override void Write(HtmlRenderer renderer, ContentTabsBlock obj)
    {
        var options = optionsFactory();
        var tabs = obj.OfType<ContentTabBlock>().ToList();
        if (tabs.Count == 0)
        {
            return;
        }

        // Distinct tab ids, in first-occurrence order, define the buttons. A dependent-tab
        // group repeats an id once per condition, so the same id collapses to one button.
        var buttonIds = new List<string>();
        foreach (var tab in tabs)
        {
            if (!buttonIds.Contains(tab.TabId, StringComparer.OrdinalIgnoreCase))
            {
                buttonIds.Add(tab.TabId);
            }
        }

        var activeButton = buttonIds[0];

        renderer.Write("<div class=\"").Write(options.ContainerCss).WriteLine("\" data-content-tabs>");

        // Tab strip — carries not-prose so the buttons stay out of page prose.
        renderer.Write("<div class=\"").Write(options.TabListCss).WriteLine("\" role=\"tablist\">");
        foreach (var id in buttonIds)
        {
            var labelTab = tabs.First(t => string.Equals(t.TabId, id, StringComparison.OrdinalIgnoreCase));
            var selected = string.Equals(id, activeButton, StringComparison.OrdinalIgnoreCase);

            renderer.Write("<button type=\"button\" role=\"tab\" class=\"").Write(options.TabButtonCss)
                .Write("\" data-tab=\"").Write(Attr(id))
                .Write("\" data-active=\"").Write(selected ? "true" : "false")
                .Write("\" aria-selected=\"").Write(selected ? "true" : "false").Write("\">");
            if (labelTab.Title is not null)
            {
                renderer.WriteChildren(labelTab.Title);
            }
            else
            {
                renderer.Write(Attr(id));
            }

            renderer.WriteLine("</button>");
        }

        renderer.WriteLine("</div>");

        // Panels — no not-prose, so panel content renders with the page's prose typography.
        // Exactly one panel is marked active server-side; the client recomputes on load.
        var activeRendered = false;
        foreach (var tab in tabs)
        {
            var isActive = !activeRendered
                && string.Equals(tab.TabId, activeButton, StringComparison.OrdinalIgnoreCase);
            if (isActive)
            {
                activeRendered = true;
            }

            renderer.Write("<div class=\"").Write(options.TabPanelCss)
                .Write("\" role=\"tabpanel\" data-tab=\"").Write(Attr(tab.TabId))
                .Write("\" data-condition=\"").Write(Attr(tab.Condition ?? string.Empty))
                .Write("\" data-active=\"").Write(isActive ? "true" : "false").WriteLine("\">");
            renderer.WriteChildren(tab);
            renderer.WriteLine("</div>");
        }

        renderer.WriteLine("</div>");
    }

    private static string Attr(string value) => WebUtility.HtmlEncode(value);
}
