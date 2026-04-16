namespace Pennington.Markdown;

using System.IO.Abstractions;
using FrontMatter;
using Pipeline;

/// <summary>
/// Parses discovered markdown files into ParsedItems using FrontMatterParser.
/// </summary>
public sealed class MarkdownContentParser<TFrontMatter> : IContentParser
    where TFrontMatter : IFrontMatter, new()
{
    private readonly FrontMatterParser _frontMatterParser;
    private readonly IFileSystem _fileSystem;

    /// <summary>Creates the parser.</summary>
    public MarkdownContentParser(FrontMatterParser frontMatterParser, IFileSystem fileSystem)
    {
        _frontMatterParser = frontMatterParser;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<ContentItem> ParseAsync(DiscoveredItem item)
    {
        if (item.Source is not MarkdownFileSource markdownSource)
        {
            return new FailedItem(item.Route,
                new ContentError("Unsupported content source type for parser"));
        }

        try
        {
            var filePath = markdownSource.Path.Value;
            var content = await _fileSystem.File.ReadAllTextAsync(filePath);

            var result = _frontMatterParser.Parse<TFrontMatter>(content);
            var metadata = result.Metadata ?? new TFrontMatter();

            return new ParsedItem(item.Route, metadata, result.Body);
        }
        catch (Exception ex)
        {
            return new FailedItem(item.Route,
                new ContentError($"Failed to parse {markdownSource.Path}: {ex.Message}", ex));
        }
    }
}