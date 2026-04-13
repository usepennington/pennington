namespace Pennington.Markdown.Extensions.Tabs;

using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;

/// <summary>
/// Markdig extension that transforms consecutive code blocks marked with tabs=true into tabbed containers.
/// </summary>
internal sealed class TabbedCodeBlocksExtension : IMarkdownExtension
{
    private readonly Func<TabbedCodeBlockRenderOptions> _optionsFactory;

    public TabbedCodeBlocksExtension(Func<TabbedCodeBlockRenderOptions>? optionsFactory = null)
    {
        _optionsFactory = optionsFactory ?? (() => TabbedCodeBlockRenderOptions.Default);
    }

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed += DocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new TabbedCodeBlockRenderer(_optionsFactory));
        }
    }

    private static void DocumentProcessed(MarkdownDocument document)
    {
        var allBlocks = document.ToList();
        document.Clear();

        for (var i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i] is FencedCodeBlock codeBlock)
            {
                var attributes = codeBlock.GetArgumentPairs();
                if (!attributes.TryGetValue("tabs", out var tabs) || tabs != "true")
                {
                    document.Add(allBlocks[i]);
                    continue;
                }

                // Look ahead to find consecutive code blocks
                var consecutiveCodeBlocks = new List<FencedCodeBlock> { codeBlock };
                var j = i + 1;

                while (j < allBlocks.Count && allBlocks[j] is FencedCodeBlock nextCodeBlock)
                {
                    consecutiveCodeBlocks.Add(nextCodeBlock);
                    j++;
                }

                if (consecutiveCodeBlocks.Count > 1)
                {
                    var tabbedBlock = new TabbedCodeBlock();
                    foreach (var block in consecutiveCodeBlocks)
                    {
                        tabbedBlock.Add(block);
                    }

                    document.Add(tabbedBlock);
                    i = j - 1; // -1 because the loop will increment i
                }
                else
                {
                    document.Add(codeBlock);
                }
            }
            else
            {
                document.Add(allBlocks[i]);
            }
        }
    }
}