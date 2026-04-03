namespace Penn.Infrastructure;

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Penn.Generation;

/// <summary>Rewrites URLs in HTML responses to include the configured base URL.</summary>
public sealed partial class BaseUrlRewritingProcessor : IResponseProcessor
{
    private readonly string _baseUrl;

    public BaseUrlRewritingProcessor(OutputOptions? outputOptions)
    {
        _baseUrl = outputOptions?.BaseUrl.Value.TrimEnd('/') ?? "";
    }

    public int Order => 0;

    public bool ShouldProcess(HttpContext context)
    {
        if (string.IsNullOrEmpty(_baseUrl) || _baseUrl == "/")
            return false;

        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && (contentType.Contains("text/html") || contentType.Contains("application/json"));
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        if (contentType.Contains("text/html"))
        {
            responseBody = RewriteHtml(responseBody);
        }
        return Task.FromResult(responseBody);
    }

    private string RewriteHtml(string html)
    {
        // Rewrite href="/..." and src="/..." to include base URL
        html = HrefRegex().Replace(html, match =>
        {
            var attr = match.Groups[1].Value;
            var quote = match.Groups[2].Value;
            var url = match.Groups[3].Value;
            if (url.StartsWith('/') && !url.StartsWith("//"))
                url = _baseUrl + url;
            return $"{attr}={quote}{url}{quote}";
        });

        // Add data-base-url to body
        html = html.Replace("<body", $"<body data-base-url=\"{_baseUrl}\"", StringComparison.OrdinalIgnoreCase);

        return html;
    }

    [GeneratedRegex("""(href|src|action)\s*=\s*(["'])(\/[^"']*?)\2""", RegexOptions.IgnoreCase)]
    private static partial Regex HrefRegex();
}
