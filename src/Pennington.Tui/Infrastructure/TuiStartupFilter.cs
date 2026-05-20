namespace Pennington.Tui.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// Inserts <see cref="TuiRequestTrackingMiddleware"/> at the very start of the request
/// pipeline so it observes every request, including ones short-circuited by later
/// middleware (404s, static files, redirects).
/// </summary>
internal sealed class TuiStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.UseMiddleware<TuiRequestTrackingMiddleware>();
            app.UseMiddleware<TuiDiagnosticsCaptureMiddleware>();
            next(app);
        };
}