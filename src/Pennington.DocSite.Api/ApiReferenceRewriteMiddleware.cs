namespace Pennington.DocSite.Api;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Pennington.DocSite;

/// <summary>
/// Rewrites inbound request paths under any configured public
/// <see cref="ApiReferenceRegistration.RoutePrefix"/> onto the internal
/// <c>/_pnn_api/{Name}/...</c> shape that <c>ApiReferencePage.razor</c>
/// matches. Runs before Blazor's router so the component sees a stable
/// route template regardless of how the caller configured the public URL.
/// </summary>
public sealed class ApiReferenceRewriteMiddleware
{
    private const string InternalPrefix = "/_pnn_api/";

    private readonly RequestDelegate _next;
    private readonly ApiReferenceRegistrationRegistry _registry;

    /// <summary>Initializes the middleware.</summary>
    public ApiReferenceRewriteMiddleware(RequestDelegate next, ApiReferenceRegistrationRegistry registry)
    {
        _next = next;
        _registry = registry;
    }

    /// <summary>Handles the rewrite; forwards to the next middleware either way.</summary>
    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path)
            && !path.StartsWith(InternalPrefix, StringComparison.Ordinal)
            && !LooksLikeStaticFile(path))
        {
            foreach (var reg in _registry.Registrations)
            {
                if (!path.StartsWith(reg.RoutePrefix, StringComparison.Ordinal)) continue;

                // Preserve the public path so MainLayout can resolve the active
                // area and stamp TOC selection against the URL the user sees,
                // not the /_pnn_api/... route Blazor actually dispatches on.
                context.Items[DocSiteHttpContextKeys.OriginalPath] = path;

                var remainder = path[reg.RoutePrefix.Length..];
                context.Request.Path = new PathString(
                    remainder.Length == 0
                        ? $"{InternalPrefix}{reg.Name}/"
                        : $"{InternalPrefix}{reg.Name}/{remainder.TrimEnd('/')}");
                break;
            }
        }

        return _next(context);
    }

    /// <summary>True when the last path segment contains a dot, indicating a file (e.g. <c>llms.txt</c>, <c>foo.md</c>) rather than an API reference slug.</summary>
    private static bool LooksLikeStaticFile(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        var lastSegment = lastSlash < 0 ? path : path[(lastSlash + 1)..];
        return lastSegment.Contains('.');
    }
}

/// <summary>
/// Installs <see cref="ApiReferenceRewriteMiddleware"/> at the head of the request
/// pipeline. Registered by <c>AddApiReference</c> so the consumer never needs to
/// call <c>UseApiReferenceRewrite</c> manually — it just works after <c>UseDocSite</c>.
/// </summary>
internal sealed class ApiReferenceStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var registry = app.ApplicationServices.GetService(typeof(ApiReferenceRegistrationRegistry)) as ApiReferenceRegistrationRegistry;
            if (registry is not null && registry.Registrations.Count > 0)
            {
                app.UseMiddleware<ApiReferenceRewriteMiddleware>();
            }
            next(app);
        };
    }
}
