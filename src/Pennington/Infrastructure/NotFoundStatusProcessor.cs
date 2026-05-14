namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Flips <see cref="HttpResponse.StatusCode"/> to 404 just before the response
/// body is written, when a content-resolving page has signalled the request
/// landed on a missing route via <c>HttpContext.Items["Pennington.NotFound"]</c>.
/// <para>
/// Pages set the marker (instead of setting <c>StatusCode = 404</c> directly)
/// so the rest of the response-processor pipeline still runs — every other
/// processor in the chain gates on a 2xx status. Doing the status flip last
/// keeps the rendered 404 body (with localized chrome, layout, structured
/// data) intact while still surfacing a real 404 to crawlers, link checkers,
/// and HTTP clients.
/// </para>
/// </summary>
public sealed class NotFoundStatusProcessor : IResponseProcessor
{
    /// <summary>Marker key set on <see cref="HttpContext.Items"/> by pages that resolve a 404.</summary>
    public const string NotFoundKey = "Pennington.NotFound";

    // Runs after everything else — body has been fully rewritten, status flip
    // happens just before the middleware writes the buffered response.
    /// <inheritdoc/>
    public int Order => int.MaxValue;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
        => context.Items[NotFoundKey] is true;

    /// <inheritdoc/>
    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        context.Response.StatusCode = 404;
        return Task.FromResult(responseBody);
    }
}
