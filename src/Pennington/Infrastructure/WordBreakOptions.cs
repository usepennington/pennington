namespace Pennington.Infrastructure;

/// <summary>
/// Configures <see cref="WordBreakHtmlRewriter"/> — which elements receive
/// word-break opportunities and how each break is rendered.
/// </summary>
public sealed class WordBreakOptions
{
    /// <summary>
    /// CSS selector identifying the elements whose text gets word-break
    /// opportunities. Only elements that contain text and no child elements are
    /// rewritten; to reach text wrapped in an inline element, name that element
    /// directly (for example add <c>span</c>) rather than using a descendant
    /// combinator like <c>h1 *</c> — descendant combinators force AngleSharp to
    /// scan every element on the page. Defaults to headings, spans, and the
    /// <c>.text-break</c> class.
    /// </summary>
    public string CssSelector { get; set; } =
        "h1, h2, h3, h4, h5, h6, span, .text-break";

    /// <summary>
    /// Minimum length a word must reach before any break is inserted. Defaults to 20.
    /// </summary>
    public int MinimumCharacters { get; set; } = 20;

    /// <summary>
    /// Markup inserted at each break opportunity. Defaults to <c>&lt;wbr&gt;</c>.
    /// </summary>
    public string WordBreakCharacters { get; set; } = "<wbr>";
}
