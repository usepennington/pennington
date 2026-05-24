namespace Pennington.Infrastructure;

using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Inserts <c>&lt;wbr&gt;</c> word-break opportunities into long identifiers
/// (dotted namespaces, PascalCase) within the elements named by
/// <see cref="WordBreakOptions.CssSelector"/>. Mutates the shared parsed
/// document, so it adds no parse/serialize cycle — replacing the former
/// third-party <c>WordBreakMiddleware</c>, which re-parsed the response string
/// the rewriting pipeline had already parsed.
/// </summary>
public sealed class WordBreakHtmlRewriter : IHtmlResponseRewriter
{
    private readonly WordBreakOptions _options;
    private readonly WordBreakProcessor _processor;

    /// <summary>Creates the rewriter from the configured <see cref="WordBreakOptions"/>.</summary>
    public WordBreakHtmlRewriter(WordBreakOptions options)
    {
        _options = options;
        _processor = new WordBreakProcessor(options);
    }

    // Typographic pass over body text; independent of the URL/link rewriters
    // (xref 10, locale 20, base-url 30, fallback-lang 40, canonical 50), so it
    // runs last and they don't observe the inserted <wbr> elements.
    /// <inheritdoc/>
    public int Order => 60;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context)
        => !string.IsNullOrWhiteSpace(_options.CssSelector);

    /// <inheritdoc/>
    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        // ToArray snapshots the match set before we start mutating innerHTML.
        foreach (var element in document.QuerySelectorAll(_options.CssSelector).ToArray())
        {
            // Only rewrite text-only elements. The selector's descendant terms
            // (such as "h1 *") reach text nested inside containers, so an
            // element that still has child markup is left for its leaf children.
            if (element.InnerHtml.Trim() != element.TextContent.Trim())
            {
                continue;
            }

            var processed = _processor.ProcessText(element.TextContent);
            if (processed != element.TextContent)
            {
                element.InnerHtml = processed;
            }
        }

        return Task.CompletedTask;
    }
}
