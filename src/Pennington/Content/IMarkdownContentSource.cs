namespace Pennington.Content;

using Pennington.Routing;

/// <summary>
/// Non-generic capability interface exposing the root directory and base URL of
/// a markdown content source. Used by <see cref="Pennington.Markdown.MarkdownLinkResolver"/>
/// to compute URL paths for relative asset references without having to deal with
/// the generic <see cref="MarkdownContentService{TFrontMatter}"/> type.
/// </summary>
public interface IMarkdownContentSource
{
    /// <summary>Absolute path to the content directory (default locale root).</summary>
    string AbsoluteContentRoot { get; }

    /// <summary>Base URL under which this source's content is served.</summary>
    UrlPath BasePageUrl { get; }
}
