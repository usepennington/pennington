namespace Pennington.Search;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Serves the sharded search artifacts under <c>/search/...</c> in dev mode, reading from
/// the same in-memory <see cref="SearchArtifactService"/> the build-time emitter uses.
/// Runs as middleware (like the llms.txt sidecars) because a <c>{locale}/{prefix}</c> route
/// can't be baked by the static crawler, and so it isn't claimed by content routes.
/// </summary>
internal sealed class SearchArtifactMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes the middleware.</summary>
    public SearchArtifactMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Serves a matching search artifact, otherwise calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context, SearchArtifactService service)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/search/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await service.GetArtifactAsync(path.TrimStart('/'));
            if (bytes is not null)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.Body.WriteAsync(bytes);
                return;
            }
        }

        await _next(context);
    }
}
