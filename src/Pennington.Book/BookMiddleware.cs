namespace Pennington.Book;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Serves book artifacts in dev mode from the same in-memory <see cref="BookArtifactService"/> the
/// build-time emitter writes from. Runs as middleware (like the search and llms.txt sidecars) so the
/// <c>{locale}/{slug}</c> routes aren't claimed by content routes. Two surfaces:
/// <c>/pdf/**.pdf</c> renders and returns the PDF on demand; <c>/book-preview/...</c> returns the
/// composed HTML so paged.js paginates it live in the browser for print-CSS iteration.
/// </summary>
internal sealed class BookMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes the middleware.</summary>
    public BookMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Serves a matching book PDF or preview, otherwise calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context, BookArtifactService service)
    {
        var path = context.Request.Path.Value ?? "";

        if (path.StartsWith("/pdf/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await service.GetPdfAsync(path.TrimStart('/'));
            if (bytes is not null)
            {
                context.Response.ContentType = "application/pdf";
                await context.Response.Body.WriteAsync(bytes);
                return;
            }
        }
        else if (path.StartsWith("/book-preview/", StringComparison.OrdinalIgnoreCase))
        {
            var html = await service.GetPreviewHtmlAsync(path);
            if (html is not null)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
                return;
            }
        }

        await _next(context);
    }
}
