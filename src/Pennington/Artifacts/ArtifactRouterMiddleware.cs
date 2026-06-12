namespace Pennington.Artifacts;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Serves every registered <see cref="IArtifactContentService"/>'s claimed URL territory in dev
/// mode from the same resolver the static build writes from. Runs as middleware (not endpoints)
/// because the claimed URL sets are lazy and file-watched, a mid-path territory like
/// <c>**/llms.txt</c> has no route-template form, and a selected endpoint cannot decline — this
/// middleware claims only exact resolver hits and falls through for everything else, so real
/// content under a claimed prefix keeps working.
/// </summary>
internal sealed class ArtifactRouterMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes the middleware.</summary>
    public ArtifactRouterMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Serves a claimed, resolvable artifact; otherwise calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context, IEnumerable<IArtifactContentService> services)
    {
        var isHead = HttpMethods.IsHead(context.Request.Method);
        if (!HttpMethods.IsGet(context.Request.Method) && !isHead)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";
        foreach (var service in services)
        {
            if (!service.Claims.Any(claim => claim.Matches(path)))
            {
                continue;
            }

            var content = await service.ResolveAsync(path.TrimStart('/'), context.RequestAborted);
            if (content is null)
            {
                continue;
            }

            context.Response.ContentType = content.ContentType;
            context.Response.ContentLength = content.Bytes.Length;
            if (!isHead)
            {
                await context.Response.Body.WriteAsync(content.Bytes, context.RequestAborted);
            }

            return;
        }

        await _next(context);
    }
}
