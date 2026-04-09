namespace Pennington.Search;

using System.Text.RegularExpressions;
using Pennington.FrontMatter;
using Pennington.Pipeline;

/// <summary>
/// Builds search index documents from rendered content items.
/// </summary>
public sealed partial class SearchIndexBuilder
{
    private readonly int _defaultPriority;

    public SearchIndexBuilder(int defaultPriority = 5)
    {
        _defaultPriority = defaultPriority;
    }

    /// <summary>
    /// Build a SearchIndexDocument from a rendered item.
    /// Returns null if the item should be excluded (e.g., drafts).
    /// </summary>
    public SearchIndexDocument? Build(RenderedItem item)
    {
        // Skip drafts
        if (item.Metadata is IDraftable { IsDraft: true })
            return null;

        var body = StripHtml(item.Content.Html);
        var section = (item.Metadata as ISectionable)?.Section;

        return new SearchIndexDocument(
            Title: item.Metadata.Title,
            Body: body,
            Url: item.Route.CanonicalPath.Value,
            Section: section,
            Locale: item.Route.Locale,
            Priority: _defaultPriority
        );
    }

    /// <summary>
    /// Strip HTML tags from content to get plain text for search indexing.
    /// </summary>
    internal static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return "";

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
