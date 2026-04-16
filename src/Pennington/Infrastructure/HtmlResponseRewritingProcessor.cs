namespace Pennington.Infrastructure;

using AngleSharp;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Single <see cref="IResponseProcessor"/> that owns the HTML rewriting
/// pipeline. Invokes every registered <see cref="IHtmlResponseRewriter"/>
/// in <see cref="IHtmlResponseRewriter.Order"/> order, parsing and
/// serializing the shared AngleSharp document exactly once per response.
/// <para>
/// Before this orchestrator, xref resolution, locale prefixing, and
/// base-URL rewriting each ran as their own <see cref="IResponseProcessor"/>
/// with its own parse/serialize cycle and its own <see cref="IBrowsingContext"/>.
/// </para>
/// </summary>
public sealed class HtmlResponseRewritingProcessor : IResponseProcessor
{
    private readonly IReadOnlyList<IHtmlResponseRewriter> _rewriters;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    /// <summary>Initializes the orchestrator with the registered rewriters, ordered by <see cref="IHtmlResponseRewriter.Order"/>.</summary>
    public HtmlResponseRewritingProcessor(IEnumerable<IHtmlResponseRewriter> rewriters)
    {
        _rewriters = rewriters.OrderBy(r => r.Order).ToArray();
    }

    /// <inheritdoc/>
    public int Order => 10;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        if (!contentType.Contains("text/html")) return false;
        if (context.Response.StatusCode is < 200 or >= 300) return false;
        return _rewriters.Any(r => r.ShouldApply(context));
    }

    /// <inheritdoc/>
    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var applicable = _rewriters.Where(r => r.ShouldApply(context)).ToList();
        if (applicable.Count == 0) return responseBody;

        // Phase 1: regex / string pre-pass for constructs that are not
        // valid HTML (currently only raw <xref:uid> tags).
        foreach (var rewriter in applicable)
        {
            responseBody = await rewriter.PreParseAsync(responseBody, context);
        }

        // Phase 2: parse once, walk the DOM with every applicable rewriter
        // in order, serialize once.
        var document = await _browsingContext.OpenAsync(req => req.Content(responseBody));
        foreach (var rewriter in applicable)
        {
            await rewriter.ApplyAsync(document, context);
        }

        return document.ToHtml();
    }
}