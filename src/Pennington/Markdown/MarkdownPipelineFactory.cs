namespace Pennington.Markdown;

using Extensions;
using Extensions.Tabs;
using Markdig;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.Tables;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Mdazor;

/// <summary>
/// Creates a configured Markdig MarkdownPipeline.
/// </summary>
public static class MarkdownPipelineFactory
{
    /// <summary>Creates a basic Markdig pipeline with advanced extensions and YAML front matter.</summary>
    public static MarkdownPipeline CreateDefault()
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
    }

    /// <summary>
    /// Creates a pipeline with syntax highlighting, tabbed code blocks, custom alerts, and
    /// Mdazor component rendering. The optional <paramref name="configure"/> hook runs after
    /// built-in extensions so consumers can add their own.
    /// </summary>
    public static MarkdownPipeline CreateWithExtensions(
        IServiceProvider serviceProvider,
        CodeBlockRenderingService renderingService,
        Func<CodeHighlightRenderOptions>? codeOptions = null,
        Func<TabbedCodeBlockRenderOptions>? tabOptions = null,
        Action<MarkdownPipelineBuilder, IServiceProvider>? configure = null)
    {
        var builder = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UseSyntaxHighlighting(renderingService, codeOptions)
            .UseTabbedCodeBlocks(tabOptions)
            .UseContentTabs()
            .UseCustomAlerts()
            .UseScrollableTables()
            // Mdazor resolves IComponentRegistry lazily at render time, so the pipeline
            // is always wired when AddPennington has registered Mdazor. The old shape
            // of gating on a registry lookup here captured an empty registry when
            // AddMdazorComponent<T>() calls ran *after* AddPennington (as they do in
            // AddDocSite/AddBlogSite), leaving built-in components rendering as literal
            // lowercased HTML in DocSite.
            .UseMdazor(serviceProvider);

        configure?.Invoke(builder, serviceProvider);
        return builder.Build();
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
        CodeBlockRenderingService renderingService,
        Func<CodeHighlightRenderOptions>? options = null)
    {
        builder.Extensions.AddIfNotAlready(new CodeHighlightingExtension(renderingService, options));
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
    /// Adds support for DocFX-style content tabs built from <c>#&#160;[Label](#tab/id)</c> headings.
    /// </summary>
    public static MarkdownPipelineBuilder UseContentTabs(this MarkdownPipelineBuilder builder)
    {
        builder.Extensions.AddIfNotAlready(new ContentTabsExtension());
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

    /// <summary>
    /// Wraps every rendered <c>&lt;table&gt;</c> in a <c>&lt;div class="overflow-x-auto"&gt;</c>
    /// so wide markdown tables scroll horizontally inside their content column instead of
    /// forcing the surrounding layout to scroll.
    /// </summary>
    public static MarkdownPipelineBuilder UseScrollableTables(this MarkdownPipelineBuilder builder)
    {
        builder.Extensions.AddIfNotAlready(new ScrollableTablesExtension());
        return builder;
    }

    private sealed class CodeHighlightingExtension(
        CodeBlockRenderingService renderingService,
        Func<CodeHighlightRenderOptions>? options) : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer)
            {
                return;
            }

            var codeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (codeBlockRenderer is not null)
            {
                htmlRenderer.ObjectRenderers.Remove(codeBlockRenderer);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(
                new CodeHighlightRenderer(renderingService, options));
        }
    }

    private sealed class CustomAlertsExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            var existing = pipeline.InlineParsers.Find<AlertInlineParser>();
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

    private sealed class ScrollableTablesExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer)
            {
                return;
            }

            var existing = htmlRenderer.ObjectRenderers.FindExact<HtmlTableRenderer>();
            if (existing is null)
            {
                return;
            }

            htmlRenderer.ObjectRenderers.Remove(existing);
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new ScrollableTableRenderer());
        }
    }

    private sealed class ScrollableTableRenderer : HtmlTableRenderer
    {
        protected override void Write(HtmlRenderer renderer, Table table)
        {
            renderer.Write("<div class=\"overflow-x-auto\">");
            base.Write(renderer, table);
            renderer.Write("</div>");
        }
    }
}