namespace BeyondCookFormatExample;

using System.IO.Abstractions;
using Pennington.FrontMatter;
using Pennington.Pipeline;

/// <summary>
/// Parses a discovered <c>.cook</c> file into a <see cref="ParsedItem"/>: it reads the file, splits the
/// YAML front matter into a typed <see cref="CookFrontMatter"/>, and hands the Cooklang body on as the
/// parsed body. The Cooklang markup itself is parsed later by <see cref="CookContentRenderer"/>. The
/// dispatching pipeline stamps the <c>"cook"</c> format onto the returned item so the matching renderer
/// is selected.
/// </summary>
public sealed class CookContentParser : IContentParser
{
    private readonly FrontMatterParser _frontMatter;
    private readonly IFileSystem _fileSystem;

    /// <summary>Creates the parser. Both dependencies are registered by <c>AddPennington</c>.</summary>
    public CookContentParser(FrontMatterParser frontMatter, IFileSystem fileSystem)
    {
        _frontMatter = frontMatter;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<ContentItem> ParseAsync(DiscoveredItem item)
    {
        if (item.Source.Value is not FileSource file)
        {
            return new FailedItem(item.Route, new ContentError("CookContentParser: unsupported content source."));
        }

        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(file.Path.Value);
            var result = _frontMatter.Parse<CookFrontMatter>(content, file.Path.Value);
            var metadata = result.Metadata ?? new CookFrontMatter();
            return new ParsedItem(item.Route, metadata, result.Body);
        }
        catch (Exception ex)
        {
            return new FailedItem(item.Route, new ContentError($"Failed to parse {file.Path}: {ex.Message}", ex));
        }
    }
}
