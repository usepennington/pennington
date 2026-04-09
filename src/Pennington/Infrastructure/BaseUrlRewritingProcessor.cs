namespace Pennington.Infrastructure;

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;
using Pennington.Generation;

/// <summary>
/// Rewrites URLs in HTML responses to include the configured base URL.
/// Uses AngleSharp for robust HTML parsing and attribute manipulation.
/// </summary>
public sealed class BaseUrlRewritingProcessor : IResponseProcessor
{
    private readonly string _baseUrl;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

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

    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        if (contentType.Contains("text/html"))
        {
            return await RewriteHtmlAsync(responseBody);
        }
        return responseBody;
    }

    private async Task<string> RewriteHtmlAsync(string html)
    {
        var document = await _browsingContext.OpenAsync(req => req.Content(html));

        // Add data-base-url to body
        document.Body?.SetAttribute("data-base-url", _baseUrl);

        // Rewrite root-relative URLs in href, src, and action attributes
        foreach (var element in document.QuerySelectorAll("[href], [src], [action]"))
        {
            RewriteAttribute(element, "href");
            RewriteAttribute(element, "src");
            RewriteAttribute(element, "action");
        }

        return document.ToHtml();
    }

    private void RewriteAttribute(IElement element, string attrName)
    {
        var value = element.GetAttribute(attrName);
        if (value is not null && value.StartsWith('/') && !value.StartsWith("//"))
        {
            element.SetAttribute(attrName, _baseUrl + value);
        }
    }
}
