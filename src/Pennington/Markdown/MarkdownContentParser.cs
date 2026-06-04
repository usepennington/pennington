namespace Pennington.Markdown;

using System.IO.Abstractions;
using FrontMatter;
using Pipeline;
using Routing;

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
        // FileSource and LlmsOnlySource both wrap a file on disk; they parse
        // identically here — the source-type discrimination only matters downstream
        // (HTML emission vs. llms-only sidecar). The dispatcher only routes
        // markdown-format FileSources to this parser.
        FilePath path;
        switch (item.Source)
        {
            case FileSource fs: path = fs.Path; break;
            case LlmsOnlySource llms: path = llms.Path; break;
            default:
                return new FailedItem(item.Route,
                    new ContentError("Unsupported content source type for parser"));
        }

        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(path.Value);

            var result = _frontMatterParser.Parse<TFrontMatter>(content, path.Value);
            var metadata = result.Metadata ?? new TFrontMatter();

            return new ParsedItem(item.Route, metadata, result.Body);
        }
        catch (Exception ex)
        {
            return new FailedItem(item.Route,
                new ContentError($"Failed to parse {path}: {ex.Message}", ex));
        }
    }
}