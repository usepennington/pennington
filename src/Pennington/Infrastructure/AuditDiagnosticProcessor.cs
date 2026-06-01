namespace Pennington.Infrastructure;

using Diagnostics;
using Generation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dev-mode response processor that pulls per-route entries out of <see cref="IAuditCache"/>
/// and pushes them into the request-scoped <see cref="DiagnosticContext"/> so the existing
/// overlay (<see cref="DiagnosticOverlayProcessor"/>) renders them. Disabled during static
/// build because audit results land directly in <see cref="BuildReport"/> there.
/// </summary>
internal sealed class AuditDiagnosticProcessor : IResponseProcessor
{
    private readonly bool _isDevMode = !PenningtonBuildMode.IsHeadlessOneShot;

    /// <summary>Order picked so this runs before <see cref="DiagnosticOverlayProcessor"/> (Order=30) reads the context.</summary>
    public int Order => 25;

    /// <inheritdoc/>
    public bool ShouldProcess(HttpContext context)
    {
        if (!_isDevMode)
        {
            return false;
        }

        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    /// <inheritdoc/>
    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var cache = context.RequestServices.GetService<IAuditCache>();
        var diagnosticContext = context.RequestServices.GetService<DiagnosticContext>();
        if (cache is null || diagnosticContext is null || cache.Diagnostics.Count == 0)
        {
            return Task.FromResult(responseBody);
        }

        var requestPath = context.Request.Path.Value ?? "/";
        foreach (var diag in cache.Diagnostics)
        {
            if (!RouteMatches(diag, requestPath))
            {
                continue;
            }

            diagnosticContext.Add(new Diagnostic(diag.Severity, diag.Message, diag.SourceFile));
        }

        return Task.FromResult(responseBody);
    }

    private static bool RouteMatches(BuildDiagnostic diag, string requestPath)
    {
        // Audit diagnostics without a Route are site-wide and would flood every page.
        // The dev surface is per-page, so untouched-by-route diagnostics stay in the
        // cache for the build report only.
        if (diag.Route is null)
        {
            return false;
        }

        var canonical = diag.Route.CanonicalPath.Value;
        return string.Equals(canonical.TrimEnd('/'), requestPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }
}