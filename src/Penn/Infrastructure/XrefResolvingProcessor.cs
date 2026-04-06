namespace Penn.Infrastructure;

using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Http;
using Penn.Generation;

/// <summary>
/// Response processor that resolves xref: cross-reference links in HTML using AngleSharp.
/// Handles three patterns:
///   1. &lt;xref:uid&gt; tags → converted to &lt;a href="url"&gt;title&lt;/a&gt;
///   2. &lt;a href="xref:uid"&gt;xref:uid&lt;/a&gt; → resolved href and title
///   3. &lt;a href="xref:uid"&gt;Custom Text&lt;/a&gt; → resolved href, text preserved
/// Unresolved references are rendered with data-xref-error attributes and reported to diagnostics.
/// </summary>
public sealed partial class XrefResolvingProcessor : IResponseProcessor
{
    private readonly XrefResolver _resolver;
    private readonly BuildDiagnosticsCollector _diagnostics;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    public XrefResolvingProcessor(XrefResolver resolver, BuildDiagnosticsCollector diagnostics)
    {
        _resolver = resolver;
        _diagnostics = diagnostics;
    }

    public int Order => -10; // Run before BaseUrlRewritingProcessor (Order 0)

    public bool ShouldProcess(HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        if (!responseBody.Contains("xref:", StringComparison.OrdinalIgnoreCase))
            return responseBody;

        // Phase 1: Resolve <xref:uid> raw tags via string replacement (not valid HTML elements)
        responseBody = await ResolveXrefTagsAsync(responseBody);

        // Phase 2: Parse with AngleSharp and resolve <a href="xref:..."> links
        var document = await _browsingContext.OpenAsync(req => req.Content(responseBody));
        var modified = await ResolveXrefLinksAsync(document);

        return modified ? document.ToHtml() : responseBody;
    }

    /// <summary>
    /// Finds &lt;xref:uid&gt; raw tags in HTML and converts them to anchor links.
    /// Must happen before AngleSharp parsing since these aren't valid HTML elements.
    /// </summary>
    private async Task<string> ResolveXrefTagsAsync(string html)
    {
        var matches = XrefTagRegex().Matches(html);
        if (matches.Count == 0) return html;

        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var uid = match.Groups[1].Value;
            var xref = await _resolver.ResolveAsync(uid);

            string replacement;
            if (xref is not null)
            {
                replacement = $"""<a href="{xref.Route.CanonicalPath.Value}">{xref.Title}</a>""";
            }
            else
            {
                _diagnostics.AddWarning(null, $"Unresolved xref: {uid}");
                replacement = $"""<a href="xref:{uid}" data-xref-error="Reference not found" data-xref-uid="{uid}">{uid}</a>""";
            }

            html = string.Concat(html.AsSpan(0, match.Index), replacement, html.AsSpan(match.Index + match.Length));
        }

        return html;
    }

    /// <summary>
    /// Uses AngleSharp to find and resolve all &lt;a href="xref:..."&gt; links.
    /// </summary>
    private async Task<bool> ResolveXrefLinksAsync(IDocument document)
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
            var xref = await _resolver.ResolveAsync(uid);

            if (xref is not null)
            {
                link.SetAttribute("href", xref.Route.CanonicalPath.Value);
                // If the link text was also "xref:uid", replace with the resolved title
                if (link.TextContent.StartsWith("xref:", StringComparison.OrdinalIgnoreCase))
                    link.TextContent = xref.Title;
            }
            else
            {
                _diagnostics.AddWarning(null, $"Unresolved xref: {uid}");
                link.SetAttribute("data-xref-error", "Reference not found");
                link.SetAttribute("data-xref-uid", uid);
            }
        }

        return true;
    }

    // Matches <xref:some.uid> (literal tag in HTML, not valid HTML — handled before parsing)
    [GeneratedRegex("""<xref:([^>]+)>""")]
    private static partial Regex XrefTagRegex();
}
