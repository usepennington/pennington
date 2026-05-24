namespace Pennington.Infrastructure;

/// <summary>
/// Configures <see cref="WordBreakHtmlRewriter"/> — which elements receive
/// word-break opportunities and how each break is rendered.
/// </summary>
public sealed class WordBreakOptions
{
    /// <summary>
    /// CSS selector identifying the elements whose text gets word-break
    /// opportunities. Only elements that contain text and no child elements
    /// are rewritten, so include descendant terms (such as <c>h1 *</c>) to
    /// reach text nested inside the matched elements. Defaults to headings,
    /// common block text elements, and the <c>.text-break</c> class.
    /// </summary>
    public string CssSelector { get; set; } =
        "h1, h1 *, h2, h2 *, h3, h3 *, h4, h4 *, h5, h5 *, h6, h6 *, .text-break, .text-break *";

    /// <summary>
    /// Minimum length a word must reach before any break is inserted. Defaults to 20.
    /// </summary>
    public int MinimumCharacters { get; set; } = 20;

    /// <summary>
    /// Markup inserted at each break opportunity. Defaults to <c>&lt;wbr&gt;</c>.
    /// </summary>
    public string WordBreakCharacters { get; set; } = "<wbr>";
}
