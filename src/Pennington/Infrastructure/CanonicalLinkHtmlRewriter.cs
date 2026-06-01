namespace Pennington.Infrastructure;

using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Auto-emits <c>&lt;link rel="canonical"&gt;</c> into <c>&lt;head&gt;</c> for
/// every HTML response when <see cref="PenningtonOptions.CanonicalBaseUrl"/>
/// is configured, unless the page already provides its own canonical link.
/// <para>
/// Page authors who want a custom canonical (cross-domain, alternate
/// representation, etc.) just emit a <c>&lt;link rel="canonical"&gt;</c> in
/// their layout — this rewriter only fills in the default and never overwrites.
/// </para>
/// </summary>
internal sealed class CanonicalLinkHtmlRewriter : IHtmlResponseRewriter
{
    private readonly PenningtonOptions _options;

    /// <summary>Creates the rewriter.</summary>
    public CanonicalLinkHtmlRewriter(PenningtonOptions options)
    {
        _options = options;
    }

    // Late order — runs after locale and base-URL rewriting so the canonical
    // path reflects the final URL the visitor sees.
    /// <inheritdoc/>
    public int Order => 50;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context)
        => !string.IsNullOrEmpty(_options.CanonicalBaseUrl);

    /// <inheritdoc/>
    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        var head = document.Head;
        if (head is null)
        {
            return Task.CompletedTask;
        }

        // Respect explicit canonical from the page — never overwrite.
        if (head.QuerySelector("link[rel=\"canonical\"]") is not null)
        {
            return Task.CompletedTask;
        }

        // PathBase carries the stripped locale segment ("/es") for non-default
        // locale requests; concatenate it back so the canonical URL matches
        // what the user typed in the address bar.
        var path = (context.Request.PathBase + context.Request.Path).ToString();
        if (string.IsNullOrEmpty(path))
        {
            path = "/";
        }

        var canonical = Combine(_options.CanonicalBaseUrl!, path);
        var link = document.CreateElement("link");
        link.SetAttribute("rel", "canonical");
        link.SetAttribute("href", canonical);
        head.AppendChild(link);
        return Task.CompletedTask;
    }

    private static string Combine(string baseUrl, string path)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = path.StartsWith('/') ? path : "/" + path;
        return trimmedBase + trimmedPath;
    }
}