namespace Pennington.Markdown;

using Markdig;
using Markdig.Extensions.Alerts;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Pennington.Highlighting;
using Pennington.Markdown.Extensions;
using Pennington.Markdown.Extensions.Tabs;

/// <summary>
/// Creates a configured Markdig MarkdownPipeline.
/// </summary>
public static class MarkdownPipelineFactory
{
    public static MarkdownPipeline CreateDefault()
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
    }

    /// <summary>
    /// Creates a pipeline with syntax highlighting, tabbed code blocks, and custom alerts.
    /// </summary>
    public static MarkdownPipeline CreateWithExtensions(
        HighlightingService highlightingService,
        Func<CodeHighlightRenderOptions>? codeOptions = null,
        Func<TabbedCodeBlockRenderOptions>? tabOptions = null,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors = null)
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UseSyntaxHighlighting(highlightingService, codeOptions, preprocessors)
            .UseTabbedCodeBlocks(tabOptions)
            .UseCustomAlerts()
            .Build();
    }
}

/// <summary>
/// Extension methods for adding custom extensions to the Markdig pipeline.
/// </summary>
internal static class MarkdownPipelineBuilderExtensions
{
    /// <summary>
    /// Adds syntax highlighting support to the Markdig pipeline.
    /// </summary>
    public static MarkdownPipelineBuilder UseSyntaxHighlighting(
        this MarkdownPipelineBuilder builder,
        HighlightingService highlightingService,
        Func<CodeHighlightRenderOptions>? options = null,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors = null)
    {
        builder.Extensions.AddIfNotAlready(new CodeHighlightingExtension(highlightingService, options, preprocessors));
        return builder;
    }

    /// <summary>
    /// Adds support for tabbed code blocks.
    /// </summary>
    public static MarkdownPipelineBuilder UseTabbedCodeBlocks(
        this MarkdownPipelineBuilder builder,
        Func<TabbedCodeBlockRenderOptions>? options = null)
    {
        builder.Extensions.AddIfNotAlready(new TabbedCodeBlocksExtension(options));
        return builder;
    }

    /// <summary>
    /// Adds custom alert block support, replacing the built-in alert inline parser.
    /// </summary>
    public static MarkdownPipelineBuilder UseCustomAlerts(this MarkdownPipelineBuilder builder)
    {
        builder.UseAlertBlocks();
        builder.Extensions.AddIfNotAlready(new CustomAlertsExtension());
        return builder;
    }

    private sealed class CodeHighlightingExtension(
        HighlightingService highlightingService,
        Func<CodeHighlightRenderOptions>? options,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors) : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer) return;

            var codeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (codeBlockRenderer is not null)
            {
                htmlRenderer.ObjectRenderers.Remove(codeBlockRenderer);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(
                new CodeHighlightRenderer(highlightingService, options, preprocessors));
        }
    }

    private sealed class CustomAlertsExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            var existing = pipeline.InlineParsers.Find<Markdig.Extensions.Alerts.AlertInlineParser>();
            if (existing is not null)
            {
                pipeline.InlineParsers.Remove(existing);
            }

            pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new CustomAlertInlineParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var blockRenderer = renderer.ObjectRenderers.FindExact<AlertBlockRenderer>();
            if (blockRenderer == null)
            {
                renderer.ObjectRenderers.InsertBefore<QuoteBlockRenderer>(new AlertBlockRenderer()
                {
                    RenderKind = AlertBlockRenderer.DefaultRenderKind,
                });
            }
        }
    }
}
