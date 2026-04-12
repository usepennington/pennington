namespace Pennington.Infrastructure;

using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A participant in the unified HTML response rewriting pipeline.
/// Multiple rewriters share a single parsed <see cref="IDocument"/> per
/// response, so the DOM is parsed and serialized exactly once regardless
/// of how many rewriters apply.
/// <para>
/// Consumed by <see cref="HtmlResponseRewritingProcessor"/>, which is the
/// single <see cref="IResponseProcessor"/> doing HTML rewriting.
/// </para>
/// </summary>
public interface IHtmlResponseRewriter
{
    /// <summary>
    /// Sort order within the HTML rewriting pipeline. Rewriters run in
    /// ascending <see cref="Order"/>. Xref resolution at 10, locale
    /// prefixing at 20, base-URL prefixing at 30 — the outside-in order
    /// that was previously expressed across separate <see cref="IResponseProcessor"/>s.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Cheap gate checked before parsing. Return false to skip both
    /// <see cref="PreParseAsync"/> and <see cref="ApplyAsync"/> for this
    /// response. If every rewriter returns false, the orchestrator skips
    /// parsing entirely.
    /// </summary>
    bool ShouldApply(HttpContext context);

    /// <summary>
    /// Regex / string pre-pass over the raw HTML before AngleSharp parses
    /// it. Exists for constructs that are not valid HTML (such as raw
    /// <c>&lt;xref:uid&gt;</c> tags) and therefore must be substituted
    /// out before the parser runs.
    /// <para>
    /// Default: pass-through. Rewriters that only touch the DOM should
    /// not override this.
    /// </para>
    /// </summary>
    Task<string> PreParseAsync(string html, HttpContext context)
        => Task.FromResult(html);

    /// <summary>
    /// Mutate the shared parsed document. The orchestrator serializes it
    /// once after every rewriter has run.
    /// </summary>
    Task ApplyAsync(IDocument document, HttpContext context);
}
