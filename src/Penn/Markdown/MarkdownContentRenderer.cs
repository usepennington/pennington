namespace Pennington.Markdown;

using System.Collections.Immutable;
using Markdig;
using Markdig.Renderers;
using Pennington.Pipeline;

/// <summary>
/// Renders parsed markdown items to HTML using Markdig.
/// </summary>
public sealed class MarkdownContentRenderer : IContentRenderer
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownContentRenderer(MarkdownPipeline? pipeline = null)
    {
        _pipeline = pipeline ?? MarkdownPipelineFactory.CreateDefault();
    }

    public Task<ContentItem> RenderAsync(ParsedItem item)
    {
        try
        {
            var document = Markdown.Parse(item.RawMarkdown, _pipeline);

            using var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            _pipeline.Setup(htmlRenderer);
            htmlRenderer.Render(document);
            writer.Flush();
            var html = writer.ToString();

            var outline = MarkdownOutlineGenerator.GenerateOutline(document);

            var renderedContent = new RenderedContent(
                Html: html,
                Outline: outline,
                Tags: ImmutableList<Tag>.Empty,
                CrossReferences: ImmutableList<CrossReference>.Empty,
                SearchDocument: null,
                Social: null
            );

            return Task.FromResult<ContentItem>(
                new RenderedItem(item.Route, item.Metadata, renderedContent));
        }
        catch (Exception ex)
        {
            return Task.FromResult<ContentItem>(
                new FailedItem(item.Route,
                    new ContentError($"Render failed: {ex.Message}", ex)));
        }
    }
}
