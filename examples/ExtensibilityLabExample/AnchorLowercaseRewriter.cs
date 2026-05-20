namespace ExtensibilityLabExample;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

/// <summary>
/// Implements <see cref="IHtmlResponseRewriter"/> and demonstrates both
/// halves of the contract:
/// <list type="bullet">
/// <item><description><see cref="PreParseAsync"/> runs a cheap string
///   replace over the raw HTML before AngleSharp parses it. We use it to
///   strip the <c>&lt;!--LOWERCASE-SENTINEL--&gt;</c> comment — the kind
///   of pre-parse cleanup a real rewriter does for non-HTML tokens like
///   <c>&lt;xref:uid&gt;</c>.</description></item>
/// <item><description><see cref="ApplyAsync"/> walks the parsed document
///   and lowercases the text content of every <c>&lt;a&gt;</c> tag
///   marked <c>data-lowercase</c>.</description></item>
/// </list>
/// <para>
/// <see cref="Order"/> is 500 — after the shipped xref (10), locale (20),
/// and base-URL (30) rewriters so our pass sees already-resolved hrefs.
/// </para>
/// <para>
/// Backs how-to 2.3.50 <c>/how-to/extensibility/html-rewriter</c>.
/// </para>
/// </summary>
public sealed class AnchorLowercaseRewriter : IHtmlResponseRewriter
{
    public int Order => 500;

    public bool ShouldApply(HttpContext context)
    {
        var contentType = context.Response.ContentType;
        return contentType is not null
               && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Pre-parse pass. Strip the sentinel comment so it is gone before
    /// AngleSharp runs. A string replace is the right tool when the
    /// target construct is not valid HTML structure (raw <c>&lt;xref&gt;</c>
    /// tags are the canonical example shipped with Pennington).
    /// </summary>
    public Task<string> PreParseAsync(string html, HttpContext context)
    {
        if (!html.Contains("<!--LOWERCASE-SENTINEL-->", StringComparison.Ordinal))
        {
            return Task.FromResult(html);
        }

        return Task.FromResult(html.Replace("<!--LOWERCASE-SENTINEL-->", string.Empty, StringComparison.Ordinal));
    }

    /// <summary>
    /// DOM pass. Walk the parsed document, find every <c>&lt;a&gt;</c>
    /// with <c>data-lowercase</c>, lowercase its text content.
    /// </summary>
    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        foreach (var element in document.QuerySelectorAll("a[data-lowercase]"))
        {
            if (element is not IHtmlAnchorElement anchor)
            {
                continue;
            }

            if (string.IsNullOrEmpty(anchor.TextContent))
            {
                continue;
            }

            anchor.TextContent = anchor.TextContent.ToLowerInvariant();
        }

        return Task.CompletedTask;
    }
}