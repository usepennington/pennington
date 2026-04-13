namespace Pennington.Content;

using System.Collections.Immutable;
using Routing;

/// <summary>
/// Configuration for a markdown content source.
/// </summary>
public sealed class MarkdownContentServiceOptions
{
    public required FilePath ContentPath { get; init; }
    public UrlPath BasePageUrl { get; init; } = new("/");
    public string? Section { get; init; }
    public string FilePattern { get; init; } = "*.md";
    public string Locale { get; init; } = "";
    public int SearchPriority { get; init; } = 10;

    /// <summary>
    /// Relative paths (forward-slash, from <see cref="ContentPath"/>) whose subtrees
    /// should be skipped during discovery and content copying. Use this when another
    /// content source already owns a subfolder — e.g. a default <c>DocFrontMatter</c>
    /// source rooted at <c>Content</c> can set <c>ExcludePaths = ["changelog"]</c>
    /// so a specialized <c>ChangelogFrontMatter</c> source rooted at
    /// <c>Content/changelog</c> is the sole owner of that subtree. Matching is
    /// case-insensitive and segment-based: <c>"a/b"</c> excludes <c>a/b</c> and
    /// anything beneath it, but not <c>a/bcd</c>.
    /// </summary>
    public ImmutableArray<string> ExcludePaths { get; init; } = ImmutableArray<string>.Empty;
}