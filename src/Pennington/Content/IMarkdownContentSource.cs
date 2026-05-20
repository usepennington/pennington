namespace Pennington.Content;

using System.Collections.Immutable;
using Routing;

/// <summary>
/// Non-generic capability interface exposing the root directory and base URL of
/// a markdown content source. Used by <see cref="Markdown.MarkdownLinkResolver"/>
/// to compute URL paths for relative asset references without having to deal with
/// the generic <see cref="MarkdownContentService{TFrontMatter}"/> type, and by the
/// overlap detector to diagnose misconfigurations where two markdown sources claim
/// overlapping subtrees.
/// </summary>
public interface IMarkdownContentSource
{
    /// <summary>Absolute path to the content directory (default locale root).</summary>
    string AbsoluteContentRoot { get; }

    /// <summary>Base URL under which this source's content is served.</summary>
    UrlPath BasePageUrl { get; }

    /// <summary>
    /// Normalized (forward-slash, lowercase) relative paths excluded from discovery
    /// and content copying. Empty for sources that don't opt out of any subtree.
    /// </summary>
    ImmutableArray<string> ExcludePaths { get; }
}