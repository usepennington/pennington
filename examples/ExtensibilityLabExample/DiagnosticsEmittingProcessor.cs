namespace ExtensibilityLabExample;

using Microsoft.AspNetCore.Http;
using Pennington.Diagnostics;
using Pennington.Infrastructure;

/// <summary>
/// Consumer-side <see cref="IResponseProcessor"/> that injects
/// <see cref="DiagnosticContext"/> and records a warning when a rendered page
/// is missing a canonical link. Every diagnostic it records flows into the
/// <c>X-Pennington-Diagnostic</c> response header and the dev-mode overlay
/// without further wiring.
/// <para>
/// Backs the example block in reference/diagnostics/request-context.
/// </para>
/// </summary>
public sealed class DiagnosticsEmittingProcessor(DiagnosticContext diagnostics) : IResponseProcessor
{
    public int Order => 50;

    public bool ShouldProcess(HttpContext context)
    {
        if (context.Response.StatusCode is < 200 or >= 300) return false;
        var contentType = context.Response.ContentType;
        return contentType is not null
               && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        if (!responseBody.Contains("rel=\"canonical\"", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.AddWarning(
                "Page is missing a <link rel=\"canonical\"> tag.",
                source: context.Request.Path);
        }
        return Task.FromResult(responseBody);
    }
}