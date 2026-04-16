namespace Pennington.Infrastructure;

using AngleSharp.Dom;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Resolves <c>xref:</c> cross-reference links on the shared response
/// document. Delegates both phases to <see cref="XrefResolvingService"/>.
/// Runs first in the HTML rewriting pipeline so the canonical paths it
/// emits are available to later locale and base-URL rewriters.
/// </summary>
public sealed class XrefHtmlRewriter : IHtmlResponseRewriter
{
    private readonly XrefResolvingService _service;

    /// <summary>Initializes the rewriter with the xref resolving service.</summary>
    public XrefHtmlRewriter(XrefResolvingService service)
    {
        _service = service;
    }

    /// <inheritdoc/>
    public int Order => 10;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context) => true;

    /// <inheritdoc/>
    public Task<string> PreParseAsync(string html, HttpContext context)
    {
        var diagnostics = context.RequestServices.GetRequiredService<DiagnosticContext>();
        return _service.ResolveXrefTagsAsync(html, diagnostics);
    }

    /// <inheritdoc/>
    public async Task ApplyAsync(IDocument document, HttpContext context)
    {
        var diagnostics = context.RequestServices.GetRequiredService<DiagnosticContext>();
        await _service.ResolveXrefLinksAsync(document, diagnostics);
    }
}