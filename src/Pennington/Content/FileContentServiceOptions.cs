namespace Pennington.Content;

using System.Collections.Immutable;
using Routing;

/// <summary>
/// Configuration for a <see cref="FileContentService{T}"/> — the discovery source for a custom file
/// format registered via <c>AddContentFormat</c>.
/// </summary>
public sealed class FileContentServiceOptions
{
    /// <summary>Filesystem path to the directory containing the format's source files.</summary>
    public required FilePath ContentPath { get; init; }

    /// <summary>Format key stamped onto discovered items, selecting the parser and renderer.</summary>
    public required string Format { get; init; }

    /// <summary>URL prefix prepended to routes generated from this content directory.</summary>
    public UrlPath BasePageUrl { get; init; } = new("/");

    /// <summary>Glob pattern used to enumerate source files (for example <c>*.cook</c>).</summary>
    public string FilePattern { get; init; } = "*.*";

    /// <summary>Default section label applied to entries when front matter doesn't specify one.</summary>
    public string? SectionLabel { get; init; }

    /// <summary>Relative ordering priority for this source's entries in the search index.</summary>
    public int SearchPriority { get; init; } = 10;

    /// <summary>
    /// Relative paths (forward-slash, from <see cref="ContentPath"/>) whose subtrees are skipped
    /// during discovery. Matching is case-insensitive and segment-based.
    /// </summary>
    public ImmutableArray<string> ExcludePaths { get; init; } = ImmutableArray<string>.Empty;
}
