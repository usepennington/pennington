namespace Pennington.Infrastructure;

using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Routing;

/// <summary>
/// Dev-mode response processor that runs <see cref="LinkVerificationService"/> on the
/// HTML being served and pushes broken-link findings into the request-scoped
/// <see cref="DiagnosticContext"/> so the overlay renders them. Replaces the corpus-wide
/// <see cref="Generation.LinkAuditor"/> pass in dev — that one issues one HTTP self-fetch
/// per page and floods the log on large sites. The build-mode auditor still runs the
/// full pass for the build report.
/// </summary>
public sealed class PageLinkAuditProcessor : IResponseProcessor
{
    private readonly bool _isDevMode = !PenningtonBuildMode.IsHeadlessOneShot;

    /// <summary>Order picked so this runs before <see cref="DiagnosticOverlayProcessor"/> (Order=30) reads the context.</summary>
    public int Order => 24;

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
    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var verifierHost = context.RequestServices.GetService<PageLinkVerifier>();
        var diagnosticContext = context.RequestServices.GetService<DiagnosticContext>();
        if (verifierHost is null || diagnosticContext is null)
        {
            return responseBody;
        }

        var verifier = await verifierHost.GetVerifierAsync();

        var requestPath = context.Request.Path.Value ?? "/";
        var canonicalPath = new UrlPath(requestPath);
        var sourceRoute = new ContentRoute
        {
            CanonicalPath = canonicalPath,
            OutputFile = new FilePath(requestPath.TrimStart('/')),
        };

        foreach (var result in verifier.VerifyLinks(sourceRoute, responseBody))
        {
            if (result.Value is not BrokenLinkResult broken)
            {
                continue;
            }

            diagnosticContext.AddWarning(
                $"Broken link to {broken.Url} ({broken.Reason})",
                $"content.links/{broken.Type}/{broken.Url}");
        }

        return responseBody;
    }
}
