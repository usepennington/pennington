namespace Pennington.StructuredData;

using AngleSharp.Dom;
using Content;
using Infrastructure;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Auto-emits a <c>&lt;script type="application/ld+json"&gt;</c> block into <c>&lt;head&gt;</c> for
/// every page whose <see cref="ContentRecord"/> front matter implements
/// <see cref="IHasStructuredData"/> — markdown, Razor, or a custom content service alike. This is
/// the central structured-data path: a record describes its schema.org entities once, and the
/// engine emits them wherever the record is served, so endpoint-backed pages no longer hand-splice
/// their own JSON-LD.
/// <para>
/// Runs only when <see cref="PenningtonOptions.CanonicalBaseUrl"/> is set, since the entities need
/// an absolute canonical URL. Template-specific structured data that this can't know — DocSite's
/// nav-derived breadcrumb trail, a fallback Article for front matter that describes none — stays in
/// the template layer.
/// </para>
/// </summary>
internal sealed class StructuredDataHtmlRewriter : IHtmlResponseRewriter
{
    private readonly PenningtonOptions _options;
    private readonly ContentRecordRegistry _records;

    /// <summary>Creates the rewriter from the site options and the aggregated record registry.</summary>
    public StructuredDataHtmlRewriter(PenningtonOptions options, ContentRecordRegistry records)
    {
        _options = options;
        _records = records;
    }

    // After canonical-link (50) so structured data lands alongside the canonical link in the head.
    /// <inheritdoc/>
    public int Order => 60;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context)
        => !string.IsNullOrEmpty(_options.CanonicalBaseUrl);

    /// <inheritdoc/>
    public async Task ApplyAsync(IDocument document, HttpContext context)
    {
        var head = document.Head;
        if (head is null)
        {
            return;
        }

        // PathBase carries the stripped locale segment ("/es") for non-default-locale requests;
        // concatenate it back so the key matches the record's locale-prefixed canonical path —
        // the same composition CanonicalLinkHtmlRewriter uses.
        var fullPath = (context.Request.PathBase + context.Request.Path).ToString();
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        var records = await _records.GetSnapshotAsync();
        if (!records.TryGetValue(fullPath.Trim('/'), out var record)
            || record.Metadata is not IHasStructuredData hasData)
        {
            return;
        }

        var ctx = new StructuredDataContext
        {
            // Compose the canonical URL from the record's canonical path (trailing-slash normalized),
            // not the raw request path — so a dev-server request to /page (no slash) still emits the
            // same /page/ URL the old templates did. The request path is only the join key above.
            CanonicalUrl = Combine(_options.CanonicalBaseUrl!, record.Route.CanonicalPath.EnsureTrailingSlash().Value),
            FallbackAuthorName = _options.StructuredDataAuthorName,
        };

        foreach (var entity in hasData.GetStructuredData(ctx))
        {
            if (entity is null)
            {
                continue;
            }

            var script = document.CreateElement("script");
            script.SetAttribute("type", "application/ld+json");
            // JsonLdSerializer escapes </ so the JSON is safe as raw script-tag text content.
            script.TextContent = JsonLdSerializer.Serialize(entity);
            head.AppendChild(script);
        }
    }

    private static string Combine(string baseUrl, string path)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = path.StartsWith('/') ? path : "/" + path;
        return trimmedBase + trimmedPath;
    }
}
