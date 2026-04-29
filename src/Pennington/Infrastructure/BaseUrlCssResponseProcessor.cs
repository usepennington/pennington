namespace Pennington.Infrastructure;

using System.Text.RegularExpressions;
using Generation;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Prefixes root-relative <c>url(...)</c> references in CSS responses with
/// the configured base URL, mirroring <see cref="BaseUrlHtmlRewriter"/> for
/// stylesheets. Without this, hand-authored <c>@font-face</c> rules and
/// other asset references fed through <c>MonorailCssOptions.ExtraStyles</c>
/// resolve against the deployment root and 404 under sub-path deployments.
/// </summary>
public sealed partial class BaseUrlCssResponseProcessor : IResponseProcessor
{
    private readonly string _baseUrl;

    /// <summary>Initializes the processor with the base URL from <see cref="OutputOptions"/>.</summary>
    public BaseUrlCssResponseProcessor(OutputOptions? outputOptions)
    {
        _baseUrl = outputOptions?.BaseUrl.Value.TrimEnd('/') ?? "";
    }

    /// <inheritdoc/>
    public int Order => 10;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
    {
        if (string.IsNullOrEmpty(_baseUrl) || _baseUrl == "/") return false;
        if (context.Response.StatusCode is < 200 or >= 300) return false;
        var contentType = context.Response.ContentType ?? "";
        return contentType.Contains("text/css");
    }

    /// <inheritdoc/>
    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var rewritten = CssUrlRegex().Replace(responseBody, match =>
        {
            var quote = match.Groups["q"].Value;
            var path = match.Groups["p"].Value;
            return $"url({quote}{_baseUrl}{path}{quote})";
        });
        return Task.FromResult(rewritten);
    }

    // Matches url(...) where the path starts with a single '/' (root-relative)
    // but not '//' (protocol-relative). The optional matched quote is captured
    // so the replacement preserves the original quoting style.
    [GeneratedRegex("""url\(\s*(?<q>['"]?)(?<p>/(?!/)[^'")\s]*)\k<q>\s*\)""")]
    private static partial Regex CssUrlRegex();
}
