using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pennington.Infrastructure;

namespace Pennington.MonorailCss;

/// <summary>
/// Extracts CSS class names from HTML and JSON responses and registers them
/// with <see cref="CssClassCollector"/> so MonorailCSS can generate the correct stylesheet.
/// This processor only observes — it never modifies the response body.
/// </summary>
public partial class CssClassCollectorProcessor(
    CssClassCollector collector,
    ILogger<CssClassCollectorProcessor> logger) : IResponseProcessor
{
    int IResponseProcessor.Order => 100;

    bool IResponseProcessor.ShouldProcess(HttpContext context)
    {
        var contentType = context.Response.ContentType;
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    Task<string> IResponseProcessor.ProcessAsync(string responseBody, HttpContext context)
    {
        var url = context.Request.Path;
        var contentType = context.Response.ContentType;
        var isJson = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;

        logger.LogTrace("Gathering CSS from {ContentType} response for {Url}", isJson ? "JSON" : "HTML", url);

        // JSON responses contain HTML with escaped quotes. JavaScriptEncoder.Default
        // encodes " as \u0022 and may also produce \". Unescape both forms so the
        // class-attribute regex can match.
        var textToScan = isJson
            ? JsonUnescapeRegex().Replace(responseBody, m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString())
                .Replace("\\\"", "\"")
            : responseBody;

        var classMatches = CssClassGatherRegex().Matches(textToScan);
        var allClasses = classMatches
            .SelectMany(m => m.Groups[1].Value.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
            .Select(WebUtility.HtmlDecode)
            .Where(c => c is not null)
            .Select(c => c!)
            .Distinct()
            .ToList();

        if (allClasses.Count > 0)
        {
            collector.BeginProcessing();
            try
            {
                logger.LogTrace("Gathered {Count} CSS classes", allClasses.Count);
                collector.AddClasses(url, allClasses);
            }
            finally
            {
                collector.EndProcessing();
            }
        }

        // Never modify the response body — just observe.
        return Task.FromResult(responseBody);
    }

    [GeneratedRegex("""class\s*=\s*["']([^"']+)["']""", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CssClassGatherRegex();

    [GeneratedRegex("""\\u([0-9a-fA-F]{4})""")]
    private static partial Regex JsonUnescapeRegex();
}