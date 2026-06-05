namespace Pennington.Content;

using System.Collections.Immutable;
using Pipeline;
using Routing;

/// <summary>
/// Configuration for a markdown content source.
/// </summary>
public sealed class MarkdownContentServiceOptions
{
    /// <summary>Filesystem path to the directory containing markdown files.</summary>
    public required FilePath ContentPath { get; init; }

    /// <summary>URL prefix prepended to routes generated from this content directory.</summary>
    public UrlPath BasePageUrl { get; init; } = new("/");

    /// <summary>
    /// Dispatch key stamped on this source's discovered items so the pipeline routes them to this
    /// source's front-matter parser rather than a shared one. The host assigns a distinct key per
    /// markdown source via <see cref="MarkdownFormat.SourceKey"/>; defaults to the shared
    /// <see cref="MarkdownFormat.Key"/> for standalone construction.
    /// </summary>
    public string Format { get; init; } = MarkdownFormat.Key;

    /// <summary>Default section label applied to discovered items when front matter doesn't specify one.</summary>
    public string? SectionLabel { get; init; }

    /// <summary>Glob pattern used to enumerate source files (defaults to <c>*.md</c>).</summary>
    public string FilePattern { get; init; } = "*.md";

    /// <summary>Locale code associated with single-locale content (ignored when multi-locale routing is active).</summary>
    public string Locale { get; init; } = "";

    /// <summary>Relative ordering priority for this source's entries in the search index.</summary>
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