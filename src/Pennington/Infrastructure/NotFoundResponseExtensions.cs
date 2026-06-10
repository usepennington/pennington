namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Lets a content-resolving page signal that the requested route is missing. The marker is read
/// by <see cref="NotFoundStatusProcessor"/>, which flips the status to 404 after the rendered 404
/// body (with its layout and chrome) has been produced — so pages set the marker instead of
/// writing <see cref="HttpResponse.StatusCode"/> directly.
/// </summary>
public static class NotFoundResponseExtensions
{
    /// <summary>Marks the current request as having resolved to a missing route.</summary>
    public static void MarkNotFound(this HttpContext context)
        => context.Items[NotFoundStatusProcessor.NotFoundKey] = true;

    /// <summary>Returns <c>true</c> when <see cref="MarkNotFound"/> has been called for this request.</summary>
    public static bool IsMarkedNotFound(this HttpContext context)
        => context.Items[NotFoundStatusProcessor.NotFoundKey] is true;
}
