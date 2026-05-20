namespace Pennington.LlmsTxt;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Intercepts requests for per-subtree <c>{prefix}llms.txt</c> and per-page
/// <c>{OutputDirectory}/{path}.md</c> sidecars before endpoint routing has a
/// chance to claim them. Without this, request paths like
/// <c>/reference/api/llms.txt</c> get caught by the API-reference Razor route
/// (whose <c>{slug}</c> segment matches <c>llms.txt</c>).
/// </summary>
public sealed class LlmsTxtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LlmsTxtOptions _options;

    /// <summary>Initializes the middleware.</summary>
    public LlmsTxtMiddleware(RequestDelegate next, LlmsTxtOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>Serves a subtree or sidecar file when the request path matches one; otherwise calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context, LlmsTxtService service)
    {
        var path = context.Request.Path.Value ?? "";

        // Per-subtree {prefix}llms.txt
        if (path.EndsWith("/llms.txt", StringComparison.OrdinalIgnoreCase) && path.Length > "/llms.txt".Length)
        {
            var key = path.TrimStart('/');
            var files = await service.GetSubtreeFilesAsync();
            var match = files.FirstOrDefault(f =>
                string.Equals(f.OutputPath.Value, key, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.Body.WriteAsync(match.Content);
                return;
            }
        }

        // Per-page sidecar /{OutputDirectory}/{path}.md
        var prefix = "/" + _options.OutputDirectory.Trim('/') + "/";
        if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            var key = path.TrimStart('/');
            var files = await service.GetMarkdownFilesAsync();
            var match = files.FirstOrDefault(f =>
                string.Equals(f.OutputPath.Value, key, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                context.Response.ContentType = "text/markdown; charset=utf-8";
                await context.Response.Body.WriteAsync(match.Content);
                return;
            }
        }

        await _next(context);
    }
}