namespace Penn.Infrastructure;

using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.DependencyInjection;
using Penn.Diagnostics;

/// <summary>
/// Resolves xref: cross-reference links in HTML strings.
/// Used by both the response processor (full HTML pages) and
/// the SPA data endpoint (island HTML fragments).
/// Resolves XrefResolver lazily from IServiceProvider so it always
/// gets the current instance from the FileWatchDependencyFactory.
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
    /// Resolve all xref patterns in an HTML string.
    /// Returns the resolved HTML.
    /// </summary>
    public async Task<string> ResolveAsync(string html, DiagnosticContext? diagnostics = null)
    {
        if (!html.Contains("xref:", StringComparison.OrdinalIgnoreCase))
            return html;

        // Phase 1: Resolve <xref:uid> raw tags via string replacement (not valid HTML elements)
        html = await ResolveXrefTagsAsync(html, diagnostics);

        // Phase 2: Parse with AngleSharp and resolve <a href="xref:..."> links
        var document = await _browsingContext.OpenAsync(req => req.Content(html));
        var modified = await ResolveXrefLinksAsync(document, diagnostics);

        return modified ? document.ToHtml() : html;
    }

    private async Task<string> ResolveXrefTagsAsync(string html, DiagnosticContext? diagnostics)
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
                replacement = $"""<a href="{xref.Route.CanonicalPath.Value}">{xref.Title}</a>""";
            }
            else
            {
                diagnostics?.AddWarning($"Unresolved xref: {uid}", "XrefResolver");
                replacement = $"""<a href="xref:{uid}" data-xref-error="Reference not found" data-xref-uid="{uid}">{uid}</a>""";
            }

            html = string.Concat(html.AsSpan(0, match.Index), replacement, html.AsSpan(match.Index + match.Length));
        }

        return html;
    }

    private async Task<bool> ResolveXrefLinksAsync(IDocument document, DiagnosticContext? diagnostics)
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
