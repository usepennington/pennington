namespace Pennington.Infrastructure;

using System.Net;
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
internal sealed class WordBreakHtmlRewriter : IHtmlResponseRewriter
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
            // Only rewrite pure-text elements. An element that still holds child
            // markup is left alone; any inline wrapper named by the selector
            // (such as a nested <span>) is matched and rewritten on its own.
            // ChildElementCount avoids serializing InnerHtml just to test this.
            if (element.ChildElementCount > 0)
            {
                continue;
            }

            // Highlighted code is always rendered as <pre><code> … </code></pre>; never
            // splice <wbr> into its token spans. Guard on <pre> (not <code>) so an explicit
            // .text-break opt-in on a standalone <code> still word-breaks.
            if (element.Closest("pre") is not null)
            {
                continue;
            }

            // TextContent is decoded, so re-encode before splicing: assigning to InnerHtml
            // re-parses the string, and any '<', '>', '&' in the text (a generic type, an
            // HTML string literal) would otherwise become real markup. Breaks land only at
            // dots and uppercase transitions, never inside an entity, so encoding first is safe.
            var text = element.TextContent;
            var encoded = WebUtility.HtmlEncode(text);
            var processed = _processor.ProcessText(encoded);
            if (processed != encoded)
            {
                element.InnerHtml = processed;
            }
        }

        return Task.CompletedTask;
    }
}
