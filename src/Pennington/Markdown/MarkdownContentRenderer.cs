namespace Pennington.Markdown;

using System.Collections.Immutable;
using System.IO.Abstractions;
using AngleSharp;
using Markdig;
using Markdig.Renderers;
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
    private static readonly IBrowsingContext BrowsingContext =
        AngleSharp.BrowsingContext.New(Configuration.Default);

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

            var document = Markdown.Parse(markdown, _pipeline);

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

    private static async Task<string> RewriteRelativeLinksAsync(
        string html, FilePath sourceFile, MarkdownLinkResolver resolver)
    {
        // Fast path: nothing that could be a rewritable relative link.
        if (!html.Contains("<a", StringComparison.OrdinalIgnoreCase)
            && !html.Contains("<img", StringComparison.OrdinalIgnoreCase))
        {
            return html;
        }

        var document = await BrowsingContext.OpenAsync(req => req.Content(html));
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