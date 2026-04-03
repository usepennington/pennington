namespace Penn.Markdown;

using Penn.FrontMatter;
using Penn.Pipeline;

/// <summary>
/// Parses discovered markdown files into ParsedItems using FrontMatterParser.
/// </summary>
public sealed class MarkdownContentParser<TFrontMatter> : IContentParser
    where TFrontMatter : IFrontMatter, new()
{
    private readonly FrontMatterParser _frontMatterParser;

    public MarkdownContentParser(FrontMatterParser frontMatterParser)
    {
        _frontMatterParser = frontMatterParser;
    }

    public async Task<ContentItem> ParseAsync(DiscoveredItem item)
    {
        if (item.Source is not MarkdownFileSource markdownSource)
        {
            return new ContentItem(new FailedItem(item.Route,
                new ContentError("Unsupported content source type for parser")));
        }

        try
        {
            var filePath = markdownSource.Path.Value;
            var content = await File.ReadAllTextAsync(filePath);

            var result = _frontMatterParser.Parse<TFrontMatter>(content);
            var metadata = result.Metadata ?? new TFrontMatter();

            return new ContentItem(new ParsedItem(item.Route, metadata, result.Body));
        }
        catch (Exception ex)
        {
            return new ContentItem(new FailedItem(item.Route,
                new ContentError($"Failed to parse {markdownSource.Path}: {ex.Message}", ex)));
        }
    }
}
