namespace Pennington.FrontMatter;

/// <summary>
/// Minimum: every content page has a title.
/// Default members provide sensible opt-out values so implementations
/// only declare properties they actually parse from front matter.
/// </summary>
public interface IFrontMatter
{
    /// <summary>Page title rendered in the browser tab, navigation, and OpenGraph tags.</summary>
    string Title { get; }

    /// <summary>True when the page is a draft and should be excluded from builds.</summary>
    bool IsDraft => false;

    /// <summary>True when the page should be included in the search index.</summary>
    bool Search => true;

    /// <summary>True when the page should be included in llms.txt output.</summary>
    bool Llms => true;

    /// <summary>
    /// When true, the page is included in indexing channels (search, llms.txt) but excluded
    /// from the rendered navigation tree. Useful for FAQ entries, glossary terms, or other
    /// content that should be discoverable by search but should not clutter the sidebar.
    /// </summary>
    bool SearchOnly => false;

    /// <summary>Stable cross-reference identifier used by xref links.</summary>
    string? Uid => null;

    /// <summary>Short summary used in meta descriptions, OpenGraph tags, and listings.</summary>
    string? Description => null;

    /// <summary>
    /// Publication date surfaced in feeds and sitemaps. Also drives scheduled publishing:
    /// when this is set to a moment after the build clock, the page is excluded from build
    /// output (same dev-vs-build behavior as <see cref="IsDraft"/>) until the clock catches up.
    /// </summary>
    DateTime? Date => null;
}