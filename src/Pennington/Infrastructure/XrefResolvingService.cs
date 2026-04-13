namespace Pennington.Infrastructure;

using System.Net;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.DependencyInjection;
using Diagnostics;

/// <summary>
/// Resolves xref: cross-reference links in HTML.
/// <para>
/// Two phases:
/// </para>
/// <list type="number">
///   <item><see cref="ResolveXrefTagsAsync"/> substitutes raw
///     <c>&lt;xref:uid&gt;</c> tags via regex. These are not valid HTML,
///     so they must be rewritten before any DOM parser sees them.</item>
///   <item><see cref="ResolveXrefLinksAsync"/> walks <c>a[href^='xref:']</c>
///     on an already-parsed document.</item>
/// </list>
/// <para>
/// The response pipeline calls both phases via <see cref="XrefHtmlRewriter"/>,
/// reusing the orchestrator's shared document. The SPA data endpoint for
/// island HTML fragments calls the combined <see cref="ResolveAsync"/>
/// entrypoint, which performs its own parse/serialize because it receives
/// raw HTML strings with no caller-owned document.
/// </para>
/// <para>
/// Resolves <see cref="XrefResolver"/> lazily from the service provider
/// so it always gets the current instance from the file-watch factory.
/// </para>
/// </summary>
public sealed partial class XrefResolvingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    public XrefResolvingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private XrefResolver Resolver => _serviceProvider.GetRequiredService<XrefResolver>();

    /// <summary>
    /// Standalone entrypoint for callers that have a raw HTML string and
    /// no document (SPA island fragments). Returns resolved HTML.
    /// </summary>
    public async Task<string> ResolveAsync(string html, DiagnosticContext? diagnostics = null)
    {
        if (!html.Contains("xref:", StringComparison.OrdinalIgnoreCase))
            return html;

        html = await ResolveXrefTagsAsync(html, diagnostics);

        var document = await _browsingContext.OpenAsync(req => req.Content(html));
        var modified = await ResolveXrefLinksAsync(document, diagnostics);

        return modified ? document.ToHtml() : html;
    }

    /// <summary>
    /// Phase 1: regex substitution of raw <c>&lt;xref:uid&gt;</c> tags.
    /// Produces an <c>&lt;a&gt;</c> element that the later DOM phase
    /// (or any downstream HTML parser) can see as normal markup.
    /// </summary>
    public async Task<string> ResolveXrefTagsAsync(string html, DiagnosticContext? diagnostics)
    {
        var matches = XrefTagRegex().Matches(html);
        if (matches.Count == 0) return html;

        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var uid = match.Groups[1].Value;
            var xref = await Resolver.ResolveAsync(uid);

            string replacement;
            if (xref is not null)
            {
                replacement = $"""<a href="{WebUtility.HtmlEncode(xref.Route.CanonicalPath.Value)}">{WebUtility.HtmlEncode(xref.Title)}</a>""";
            }
            else
            {
                diagnostics?.AddWarning($"Unresolved xref: {uid}", "XrefResolver");
                replacement = $"""<a href="xref:{WebUtility.HtmlEncode(uid)}" data-xref-error="Reference not found" data-xref-uid="{WebUtility.HtmlEncode(uid)}">{WebUtility.HtmlEncode(uid)}</a>""";
            }

            html = string.Concat(html.AsSpan(0, match.Index), replacement, html.AsSpan(match.Index + match.Length));
        }

        return html;
    }

    /// <summary>
    /// Phase 2: DOM rewrite of <c>a[href^='xref:']</c> links on an
    /// already-parsed document. Returns true when any link was rewritten
    /// — callers that used <see cref="ResolveAsync"/> use this to decide
    /// whether to re-serialize the document.
    /// </summary>
    public async Task<bool> ResolveXrefLinksAsync(IDocument document, DiagnosticContext? diagnostics)
    {
        var xrefLinks = document.QuerySelectorAll("a[href^='xref:']")
            .OfType<IHtmlAnchorElement>()
            .ToList();

        if (xrefLinks.Count == 0)
            return false;

        foreach (var link in xrefLinks)
        {
            var href = link.GetAttribute("href");
            if (href is null || !href.StartsWith("xref:", StringComparison.OrdinalIgnoreCase))
                continue;

            var uid = href[5..];
            var xref = await Resolver.ResolveAsync(uid);

            if (xref is not null)
            {
                link.SetAttribute("href", xref.Route.CanonicalPath.Value);
                if (link.TextContent.StartsWith("xref:", StringComparison.OrdinalIgnoreCase))
                    link.TextContent = xref.Title;
            }
            else
            {
                diagnostics?.AddWarning($"Unresolved xref: {uid}", "XrefResolver");
                link.SetAttribute("data-xref-error", "Reference not found");
                link.SetAttribute("data-xref-uid", uid);
            }
        }

        return true;
    }

    [GeneratedRegex("""<xref:([^>]+)>""")]
    private static partial Regex XrefTagRegex();
}