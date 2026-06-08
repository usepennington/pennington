namespace Pennington.Markdown;

using System.Collections.Immutable;
using System.IO.Abstractions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using Markdig.Renderers;
using Mdazor;
using Pipeline;
using Routing;
using Shortcodes;

/// <summary>
/// Renders parsed markdown items to HTML using Markdig.
/// After rendering, relative author-written links (e.g. <c>../how-to/foo.md</c>,
/// <c>sample-post</c>, <c>./image.png</c>) are rewritten to absolute canonical
/// URLs via <see cref="MarkdownLinkResolver"/>.
/// </summary>
public sealed class MarkdownContentRenderer : IContentRenderer
{
    private readonly MarkdownPipeline _pipeline;
    private readonly MarkdownLinkResolver? _linkResolver;
    private readonly IFileSystem? _fileSystem;
    private readonly ShortcodeExpander? _shortcodeExpander;

    /// <summary>Creates the renderer; the default Markdig pipeline is used when none is supplied.</summary>
    /// <param name="pipeline">Markdig pipeline; defaults to <see cref="MarkdownPipelineFactory.CreateDefault"/>.</param>
    /// <param name="linkResolver">Resolves author-written relative links to canonical URLs.</param>
    /// <param name="fileSystem">Backs <c>[!INCLUDE]</c> expansion; includes are skipped when null.</param>
    /// <param name="shortcodeExpander">Expands <c>&lt;?# Name ... ?&gt;</c> shortcodes before Markdig parses; shortcodes are skipped when null.</param>
    public MarkdownContentRenderer(
        MarkdownPipeline? pipeline = null,
        MarkdownLinkResolver? linkResolver = null,
        IFileSystem? fileSystem = null,
        ShortcodeExpander? shortcodeExpander = null)
    {
        _pipeline = pipeline ?? MarkdownPipelineFactory.CreateDefault();
        _linkResolver = linkResolver;
        _fileSystem = fileSystem;
        _shortcodeExpander = shortcodeExpander;
    }

    /// <inheritdoc/>
    public async Task<ContentItem> RenderAsync(ParsedItem item)
    {
        try
        {
            // Splice in [!INCLUDE [..](..)] partials before parsing. This is the single
            // render chokepoint, so every ParsedItem-producing path picks includes up here.
            var markdown = item.RawMarkdown;
            if (_fileSystem is not null && item.Route.SourceFile is { } includeBase)
            {
                markdown = IncludeExpander.Expand(markdown, includeBase, _fileSystem);
            }

            // Expand <?# Name ... ?> shortcodes after includes so directives authored inside
            // included partials still resolve, but before Markdig so handler output flows
            // through the rest of the pipeline.
            if (_shortcodeExpander is not null)
            {
                var shortcodeContext = new ShortcodeContext(item.Route, item.Metadata);
                markdown = await _shortcodeExpander.ExpandAsync(markdown, shortcodeContext, CancellationToken.None);
            }

            // Hand page facts (file name, route, front matter, derived metadata) to any Mdazor
            // components on the page. They read it via [CascadingParameter] MdazorContext. This is
            // captured at parse time and surfaced at render time; harmless when the pipeline has no Mdazor.
            var parserContext = BuildMdazorContext(item);
            var document = Markdown.Parse(markdown, _pipeline, parserContext);

            using var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            _pipeline.Setup(htmlRenderer);

            htmlRenderer.Render(document);
            writer.Flush();
            var html = writer.ToString();

            // Rewrite author-written relative links to absolute canonical URLs.
            if (_linkResolver is not null && item.Route.SourceFile is { } sourceFile)
            {
                html = await RewriteRelativeLinksAsync(html, sourceFile, _linkResolver);
            }

            var outline = MarkdownOutlineGenerator.GenerateOutline(document);

            var renderedContent = new RenderedContent(
                Html: html,
                Outline: outline,
                Tags: ImmutableList<Tag>.Empty,
                CrossReferences: ImmutableList<CrossReference>.Empty,
                Social: null
            );

            return new RenderedItem(item.Route, item.Metadata, renderedContent);
        }
        catch (Exception ex)
        {
            return new FailedItem(item.Route,
                new ContentError($"Render failed: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Builds the ambient <see cref="MdazorContext"/> handed to Mdazor components rendered from this
    /// page. Exposes a curated set of page facts under stable, case-insensitive keys: the source
    /// <c>FileName</c>/<c>FileNameWithoutExtension</c>/<c>SourceFile</c>, the canonical <c>Url</c>
    /// (also <c>CanonicalPath</c>), <c>OutputFile</c>, <c>Locale</c>, the front-matter
    /// <c>Metadata</c> object, and the <c>Derived</c> enricher dictionary. A component reads any of
    /// these via <c>[CascadingParameter] public MdazorContext? Context</c>.
    /// </summary>
    private static MarkdownParserContext BuildMdazorContext(ParsedItem item)
    {
        var route = item.Route;
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["SourceFile"] = route.SourceFile?.Value,
            ["FileName"] = route.SourceFile?.FileName,
            ["FileNameWithoutExtension"] = route.SourceFile?.FileNameWithoutExtension,
            ["Url"] = route.CanonicalPath.ToString(),
            ["CanonicalPath"] = route.CanonicalPath.ToString(),
            ["OutputFile"] = route.OutputFile.ToString(),
            ["Locale"] = route.Locale,
            ["Metadata"] = item.Metadata,
            ["Derived"] = item.Derived,
        };

        return new MarkdownParserContext().SetMdazorContext(values);
    }

    private static async Task<string> RewriteRelativeLinksAsync(
        string html, FilePath sourceFile, MarkdownLinkResolver resolver)
    {
        // Fast path: nothing that could be a rewritable relative link.
        if (!html.Contains("<a", StringComparison.OrdinalIgnoreCase)
            && !html.Contains("<img", StringComparison.OrdinalIgnoreCase))
        {
            return html;
        }

        // Stateless per-call parse (matching CodeTransformer): a shared browsing
        // context would race under the parallel render path in SiteProjection.
        var document = new HtmlParser().ParseDocument(html);
        var modified = false;

        foreach (var element in document.QuerySelectorAll("a[href]"))
        {
            var href = element.GetAttribute("href");
            if (href is null)
            {
                continue;
            }

            var resolved = await resolver.ResolveAsync(sourceFile, href);
            if (resolved is not null && !string.Equals(resolved, href, StringComparison.Ordinal))
            {
                element.SetAttribute("href", resolved);
                modified = true;
            }
        }

        foreach (var element in document.QuerySelectorAll("img[src]"))
        {
            var src = element.GetAttribute("src");
            if (src is null)
            {
                continue;
            }

            var resolved = await resolver.ResolveAsync(sourceFile, src);
            if (resolved is not null && !string.Equals(resolved, src, StringComparison.Ordinal))
            {
                element.SetAttribute("src", resolved);
                modified = true;
            }
        }

        if (!modified)
        {
            return html;
        }

        // AngleSharp renders the fragment inside <html><head></head><body>…</body></html>.
        // The rest of the pipeline expects the bare fragment, so emit only body contents.
        return document.Body?.InnerHtml ?? html;
    }
}