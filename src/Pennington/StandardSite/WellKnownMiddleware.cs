namespace Pennington.StandardSite;

using System.Text;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Serves the Standard Site verification well-known files in dev mode (the build emitter bakes them
/// for static output). Mirrors the search-artifact middleware: matches the fixed paths, pins
/// <c>text/plain</c>, and otherwise passes through. No-ops when the options are unconfigured.
/// </summary>
internal sealed class WellKnownMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes the middleware.</summary>
    public WellKnownMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Serves a matching well-known file, otherwise calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context, StandardSiteOptions options)
    {
        if (options.IsConfigured)
        {
            var path = context.Request.Path.Value ?? "";
            var publicationPath = "/.well-known/site.standard.publication" + options.PublicationPath.TrimEnd('/');

            if (string.Equals(path, publicationPath, StringComparison.OrdinalIgnoreCase))
            {
                await WriteTextAsync(context, AtUri.Build(options.Did, "site.standard.publication", options.PublicationRkey));
                return;
            }

            if (options.EmitAtprotoDid
                && string.Equals(path, "/.well-known/atproto-did", StringComparison.OrdinalIgnoreCase))
            {
                await WriteTextAsync(context, options.Did);
                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteTextAsync(HttpContext context, string body)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body));
    }
}
