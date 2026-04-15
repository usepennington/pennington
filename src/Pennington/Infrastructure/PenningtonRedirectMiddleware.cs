namespace Pennington.Infrastructure;

using System.Text;
using Content;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Issues HTTP 301 responses for URLs in <see cref="RedirectContentService"/>'s
/// merged redirect map (<c>_redirects.yml</c> + per-page <c>redirectUrl:</c> front
/// matter). Runs before content routing so dev requests redirect cleanly; the
/// static build crawler captures the same 301 response and
/// <see cref="Generation.OutputGenerationService"/> writes the meta-refresh body
/// to disk. One code path for dev and publish.
/// </summary>
public sealed class PenningtonRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public PenningtonRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RedirectContentService redirects)
    {
        var path = context.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            await _next(context);
            return;
        }

        var normalized = path.TrimEnd('/');
        if (normalized.Length == 0) normalized = "/";

        var map = await redirects.GetRedirectMappingsAsync();
        if (map.TryGetValue(normalized, out var target))
        {
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Headers.Location = target;
            // Browsers cache 301s indefinitely by URL. Force revalidation so a
            // bad redirect (e.g., one shipped accidentally) can be corrected
            // without asking users to clear site data.
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.ContentType = "text/html; charset=utf-8";
            var body = BuildRedirectHtml(target);
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body));
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Builds the meta-refresh HTML body returned alongside the 301 status.
    /// Matches the body <see cref="Generation.OutputGenerationService"/> writes
    /// to disk when it intercepts the redirect, so dev and publish are byte-aligned
    /// in intent.
    /// </summary>
    internal static string BuildRedirectHtml(string target) => $"""
        <!DOCTYPE html>
        <html><head>
        <meta http-equiv="refresh" content="0;url={target}">
        <link rel="canonical" href="{target}">
        </head></html>
        """;
}