namespace Pennington.Markdown.Extensions.Tabs;

using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

/// <summary>
/// Markdig extension that transforms DocFX-style tab headings — <c>#&#160;[Label](#tab/id)</c>,
/// optionally <c>#tab/id/condition</c> for dependent tabs — into <see cref="ContentTabsBlock"/>
/// containers. A run of consecutive tab headings forms one group, ended by a thematic break.
/// </summary>
internal sealed class ContentTabsExtension(Func<ContentTabsRenderOptions>? optionsFactory = null)
    : IMarkdownExtension
{
    private readonly Func<ContentTabsRenderOptions> _optionsFactory =
        optionsFactory ?? (() => ContentTabsRenderOptions.Default);

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed += DocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new ContentTabsBlockRenderer(_optionsFactory));
        }
    }

    private static void DocumentProcessed(MarkdownDocument document)
    {
        var allBlocks = document.ToList();
        if (allBlocks.All(b => !IsTabHeading(b, out _, out _, out _)))
        {
            return;
        }

        document.Clear();

        var i = 0;
        while (i < allBlocks.Count)
        {
            if (!IsTabHeading(allBlocks[i], out _, out _, out _))
            {
                document.Add(allBlocks[i]);
                i++;
                continue;
            }

            var tabs = new ContentTabsBlock();
            while (i < allBlocks.Count
                   && IsTabHeading(allBlocks[i], out var tabId, out var condition, out var title))
            {
                var tab = new ContentTabBlock { TabId = tabId, Condition = condition, Title = title };
                i++;

                // The panel runs until the next tab heading or the group-ending thematic break.
                while (i < allBlocks.Count
                       && allBlocks[i] is not ThematicBreakBlock
                       && !IsTabHeading(allBlocks[i], out _, out _, out _))
                {
                    tab.Add(allBlocks[i]);
                    i++;
                }

                tabs.Add(tab);
            }

            // Consume the thematic break that terminates the group.
            if (i < allBlocks.Count && allBlocks[i] is ThematicBreakBlock)
            {
                i++;
            }

            document.Add(tabs);
        }
    }

    private static bool IsTabHeading(
        Block block, out string tabId, out string? condition, out ContainerInline? title)
    {
        tabId = string.Empty;
        condition = null;
        title = null;

        if (block is not HeadingBlock heading
            || heading.Inline?.FirstChild is not LinkInline link
            || link.NextSibling is not null
            || link.Url is null
            || !link.Url.StartsWith("#tab/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var spec = link.Url["#tab/".Length..];
        if (spec.Length == 0)
        {
            return false;
        }

        var slash = spec.IndexOf('/');
        if (slash >= 0)
        {
            tabId = spec[..slash];
            condition = spec[(slash + 1)..];
            if (tabId.Length == 0 || condition.Length == 0)
            {
                return false;
            }
        }
        else
        {
            tabId = spec;
        }

        // A LinkInline is itself a ContainerInline; its children are the button label.
        title = link;
        return true;
    }
}
