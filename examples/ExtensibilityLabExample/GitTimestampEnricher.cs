namespace ExtensibilityLabExample;

using Pennington.Pipeline;

/// <summary>
/// Implements <see cref="IMetadataEnricher"/>. Contributes a last-modified date for
/// each page under the <see cref="Key"/> key by reading the source file's filesystem
/// timestamp — the value a real enricher would instead pull from <c>git log -1</c>.
/// Pages with no source file on disk (generated or in-memory content) contribute
/// nothing. Backs how-to 2.3.35 <c>/how-to/markdown-pipeline/metadata-enrichers</c>.
/// </summary>
public sealed class GitTimestampEnricher : IMetadataEnricher
{
    /// <summary>Key written into <see cref="ParsedItem.Derived"/>.</summary>
    public const string Key = "git_last_modified";

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, object?>> EnrichAsync(ParsedItem item)
    {
        var path = item.Route.SourceFile?.Value;
        if (path is null || !File.Exists(path))
        {
            return Task.FromResult<IReadOnlyDictionary<string, object?>>(
                new Dictionary<string, object?>());
        }

        var modified = File.GetLastWriteTimeUtc(path).ToString("yyyy-MM-dd");
        return Task.FromResult<IReadOnlyDictionary<string, object?>>(
            new Dictionary<string, object?> { [Key] = modified });
    }
}
