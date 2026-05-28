namespace Pennington.Infrastructure;

using System.Net;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
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
/// Registered as a transient so every resolution picks up the current
/// file-watched <see cref="XrefResolver"/> instance. The orchestrator
/// (and rewriter that wraps this service) are transient too; the whole
/// chain is rebuilt per request when the middleware resolves its
/// processors, so no capture pins a stale <see cref="XrefResolver"/>.
/// </para>
/// </summary>
public sealed partial class XrefResolvingService
{
    private readonly XrefResolver _resolver;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    /// <summary>Initializes the service with the current <see cref="XrefResolver"/> from DI.</summary>
    public XrefResolvingService(XrefResolver resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// Standalone entrypoint for callers that have a raw HTML string and
    /// no document (SPA island fragments). Returns resolved HTML.
    /// </summary>
    public async Task<string> ResolveAsync(string html, DiagnosticContext? diagnostics = null)
    {
        if (!html.Contains("xref:", StringComparison.OrdinalIgnoreCase))
        {
            return html;
        }

        html = await ResolveXrefTagsAsync(html, diagnostics);

        var document = await _browsingContext.OpenAsync(req => req.Content(html));
        var modified = await ResolveXrefLinksAsync(document, diagnostics);

        return modified ? document.ToHtml() : html;
    }

    /// <summary>
    /// Phase 1: regex substitution of raw <c>&lt;xref:uid&gt;</c> tags.
    /// Produces an <c>&lt;a&gt;</c> element that the later DOM phase
    /// (or any downstream HTML parser) can see as normal markup. Skips
    /// content inside <c>&lt;code&gt;</c> and <c>&lt;pre&gt;</c> blocks so
    /// authoring docs can show literal <c>&lt;xref:uid&gt;</c> samples in
    /// fenced code without the rewriter latching onto them — the highlighter
    /// also splits long tokens across span boundaries, which would otherwise
    /// let <c>[^&gt;]+</c> consume span markup as the uid.
    /// </summary>
    public async Task<string> ResolveXrefTagsAsync(string html, DiagnosticContext? diagnostics)
    {
        if (!html.Contains("<xref:", StringComparison.Ordinal))
        {
            return html;
        }

        // Build a mask of byte ranges to skip (inside <code> or <pre> elements).
        var skipRanges = BuildSkipRanges(html);

        var matches = XrefTagRegex().Matches(html);
        if (matches.Count == 0)
        {
            return html;
        }

        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            if (IsInsideSkipRange(match.Index, skipRanges))
            {
                continue;
            }

            var (uid, fragment) = SplitFragment(match.Groups[1].Value);
            var xref = await _resolver.ResolveAsync(uid);

            string replacement;
            if (xref is not null)
            {
                var href = xref.Route.CanonicalPath.Value + fragment;
                replacement = $"""<a href="{WebUtility.HtmlEncode(href)}">{WebUtility.HtmlEncode(xref.Title)}</a>""";
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

    private static List<(int Start, int End)> BuildSkipRanges(string html)
    {
        var ranges = new List<(int, int)>();
        foreach (var match in CodeOrPreRegex().EnumerateMatches(html))
        {
            ranges.Add((match.Index, match.Index + match.Length));
        }

        return ranges;
    }

    private static bool IsInsideSkipRange(int index, List<(int Start, int End)> ranges)
    {
        foreach (var (start, end) in ranges)
        {
            if (index >= start && index < end)
            {
                return true;
            }
        }

        return false;
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
        {
            return false;
        }

        foreach (var link in xrefLinks)
        {
            var href = link.GetAttribute("href");
            if (href is null || !href.StartsWith("xref:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var (uid, fragment) = SplitFragment(href[5..]);
            var xref = await _resolver.ResolveAsync(uid);

            if (xref is not null)
            {
                link.SetAttribute("href", xref.Route.CanonicalPath.Value + fragment);
                if (link.TextContent.StartsWith("xref:", StringComparison.OrdinalIgnoreCase))
                {
                    link.TextContent = xref.Title;
                }
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

    // A uid may carry a trailing #fragment to deep-link a heading on the target page.
    // uids are dotted identifiers and never contain '#', so split on the first one and
    // re-append the fragment (including '#') to the resolved canonical path.
    private static (string Uid, string Fragment) SplitFragment(string raw)
    {
        var hash = raw.IndexOf('#');
        return hash < 0 ? (raw, "") : (raw[..hash], raw[hash..]);
    }

    [GeneratedRegex("""<xref:([^>]+)>""")]
    private static partial Regex XrefTagRegex();

    [GeneratedRegex("""<(code|pre|script|style)\b[^>]*>.*?</\1\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CodeOrPreRegex();
}