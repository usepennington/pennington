namespace Pennington.Tui.Infrastructure;

using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;
using Pennington.Diagnostics;

/// <summary>
/// Snapshots the scoped <see cref="DiagnosticContext"/> at the end of each HTML
/// response and records it against the request path in <see cref="PageDiagnosticsCollector"/>.
/// Static assets (CSS, JS, images) don't run through the content pipeline so they
/// have empty diagnostic contexts anyway; filtering on <c>text/html</c> keeps the
/// Diagnostics tab from filling up with static-asset rows.
/// </summary>
internal sealed class TuiDiagnosticsCaptureMiddleware(RequestDelegate next, PageDiagnosticsCollector collector)
{
    public async Task InvokeAsync(HttpContext context, DiagnosticContext diagnostics)
    {
        try
        {
            await next(context);
        }
        finally
        {
            var contentType = context.Response.ContentType;
            if (contentType is not null && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var path = context.Request.Path.Value ?? "/";
                collector.Record(path, DateTimeOffset.Now, [.. diagnostics.Diagnostics]);
            }
        }
    }
}
