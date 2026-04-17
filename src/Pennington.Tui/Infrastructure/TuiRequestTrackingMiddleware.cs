namespace Pennington.Tui.Infrastructure;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Records every HTTP request (method, path, query, status, duration) into the
/// shared <c>BoundedSequenceLog&lt;RequestEntry&gt;</c> as it completes. The TUI's
/// Main tab streams these rows into its Requests panel. Registered first in the
/// pipeline via <see cref="TuiStartupFilter"/> so nothing is missed.
/// </summary>
internal sealed class TuiRequestTrackingMiddleware(RequestDelegate next, BoundedSequenceLog<RequestEntry> log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var method = context.Request.Method;
        var path = context.Request.Path.HasValue ? context.Request.Path.Value! : "";
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value! : "";
        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            var status = context.Response.StatusCode;
            var completed = DateTimeOffset.Now;
            log.Append(seq => new RequestEntry(seq, completed, method, path, query, status, elapsed));
        }
    }
}
