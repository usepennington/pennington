namespace Pennington.Search;

using System.Text.RegularExpressions;
using Content;

/// <summary>
/// Builds search index documents from TOC entries plus post-pipeline HTML
/// fetched from the running host.
/// </summary>
public sealed partial class SearchIndexBuilder
{
    private readonly int _defaultPriority;

    /// <summary>Creates the builder with the default search priority applied when a TOC entry has none.</summary>
    public SearchIndexBuilder(int defaultPriority = 5)
    {
        _defaultPriority = defaultPriority;
    }

    /// <summary>
    /// Build a SearchIndexDocument from a TOC entry plus its fetched body HTML and
    /// pre-extracted headings text. Draft filtering is handled upstream by each
    /// <c>IContentService</c>'s TOC builder.
    /// </summary>
    public SearchIndexDocument Build(ContentTocItem toc, string bodyHtml, string headings)
    {
        var body = StripHtml(bodyHtml);

        return new SearchIndexDocument(
            Title: toc.Title,
            Description: toc.Description,
            Headings: headings,
            Body: body,
            Url: toc.Route.CanonicalPath.Value,
            SectionLabel: toc.SectionLabel,
            Locale: toc.Route.Locale,
            Priority: _defaultPriority
        );
    }

    /// <summary>
    /// Strip HTML tags from content to get plain text for search indexing.
    /// </summary>
    internal static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return "";
        }

        // Remove HTML tags
        var text = HtmlTagRegex().Replace(html, " ");
        // Decode common HTML entities
        text = text.Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Replace("&quot;", "\"")
                   .Replace("&#39;", "'")
                   .Replace("&nbsp;", " ");
        // Collapse whitespace
        text = WhitespaceRegex().Replace(text, " ");
        return text.Trim();
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}