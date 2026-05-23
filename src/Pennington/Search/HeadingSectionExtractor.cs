namespace Pennington.Search;

using System.Text;
using AngleSharp.Dom;

/// <summary>
/// One heading-delimited section of a rendered page: a heading plus the text beneath it, down to
/// the next heading of the same or higher level. The text before the first heading is the
/// <see cref="IsLead"/> section.
/// </summary>
/// <param name="AnchorId">The heading's anchor id (for deep-linking), or null for the lead section.</param>
/// <param name="Title">The heading text, or empty for the lead section (the host supplies the page title).</param>
/// <param name="Level">The heading level (2–6), or 1 for the lead section.</param>
/// <param name="Crumbs">Ancestor heading texts (excluding this heading and the page title), nearest-last.</param>
/// <param name="Text">Plain-text content of the section, whitespace-collapsed.</param>
/// <param name="IsLead">True for the page-lead section (content before the first heading).</param>
public sealed record HeadingSection(
    string? AnchorId,
    string Title,
    int Level,
    IReadOnlyList<string> Crumbs,
    string Text,
    bool IsLead);

/// <summary>
/// Splits post-pipeline page HTML into one <see cref="HeadingSection"/> per heading (plus a lead
/// section) so the search index can carry heading-level records that deep-link to anchors. Walks
/// the rendered content element in document order; <c>h2</c>–<c>h6</c> with an id start a new
/// section, <c>h1</c> is treated as the page title (not indexed into a section body), and
/// <c>&lt;pre&gt;</c> subtrees are dropped when code blocks are excluded.
/// </summary>
public sealed class HeadingSectionExtractor
{
    /// <summary>Extracts the lead section plus one section per anchored heading from <paramref name="content"/>.</summary>
    public IReadOnlyList<HeadingSection> Extract(IElement content, bool excludeCodeBlocks)
    {
        var sections = new List<HeadingSection>();
        var trail = new List<(int Level, string Title)>();
        var current = new Section(anchorId: null, title: "", level: 1, crumbs: [], isLead: true);

        void Flush()
        {
            sections.Add(new HeadingSection(
                current.AnchorId, current.Title, current.Level, current.Crumbs, Collapse(current.Text.ToString()), current.IsLead));
        }

        void Walk(INode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child is IText text)
                {
                    current.Text.Append(text.Data);
                    continue;
                }

                if (child is not IElement el)
                {
                    continue;
                }

                var tag = el.LocalName;
                if (tag == "h1")
                {
                    continue; // page title — represented as the lead record's title, not section body
                }

                if (HeadingLevel(tag) is { } level && !string.IsNullOrEmpty(el.Id))
                {
                    Flush();
                    while (trail.Count > 0 && trail[^1].Level >= level)
                    {
                        trail.RemoveAt(trail.Count - 1);
                    }

                    var title = Collapse(el.TextContent);
                    current = new Section(el.Id, title, level, [.. trail.Select(t => t.Title)], isLead: false);
                    trail.Add((level, title));
                    continue; // the heading text is the section title, not its body
                }

                if (excludeCodeBlocks && tag == "pre")
                {
                    continue; // drop the code block subtree
                }

                Walk(el);
            }
        }

        Walk(content);
        Flush();
        return sections;
    }

    private static int? HeadingLevel(string tag) => tag switch
    {
        "h2" => 2,
        "h3" => 3,
        "h4" => 4,
        "h5" => 5,
        "h6" => 6,
        _ => null,
    };

    // Collapses runs of whitespace to a single space and trims, so section bodies match the
    // plain-text shape the index expects regardless of source HTML formatting.
    private static string Collapse(string text)
    {
        var sb = new StringBuilder(text.Length);
        var prevSpace = false;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!prevSpace && sb.Length > 0)
                {
                    sb.Append(' ');
                }

                prevSpace = true;
            }
            else
            {
                sb.Append(ch);
                prevSpace = false;
            }
        }

        return sb.ToString().TrimEnd();
    }

    private sealed class Section(string? anchorId, string title, int level, IReadOnlyList<string> crumbs, bool isLead)
    {
        public string? AnchorId { get; } = anchorId;
        public string Title { get; } = title;
        public int Level { get; } = level;
        public IReadOnlyList<string> Crumbs { get; } = crumbs;
        public bool IsLead { get; } = isLead;
        public StringBuilder Text { get; } = new();
    }
}
