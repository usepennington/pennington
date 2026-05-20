namespace Pennington.Infrastructure;

using System.Text;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Captures response body once and runs all registered IResponseProcessors in order.</summary>
public sealed class ResponseProcessingMiddleware(RequestDelegate next)
{
    /// <summary>Captures the response body, runs applicable processors in order, then writes the final payload.</summary>
    public async Task InvokeAsync(HttpContext context, IEnumerable<IResponseProcessor> processors)
    {
        var originalBodyStream = context.Response.Body;
        try
        {
            await using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await next(context);

            var applicable = processors
                .Where(p => p.ShouldProcess(context))
                .OrderBy(p => p.Order)
                .ToList();

            if (applicable.Count > 0)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(memoryStream).ReadToEndAsync();

                foreach (var processor in applicable)
                {
                    body = await processor.ProcessAsync(body, context);
                }

                // Write diagnostic headers after processors (which may add diagnostics)
                // but before the body is sent to the client
                WriteDiagnosticHeaders(context);

                var bytes = Encoding.UTF8.GetBytes(body);
                context.Response.ContentLength = null;
                await originalBodyStream.WriteAsync(bytes);
            }
            else
            {
                WriteDiagnosticHeaders(context);
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static void WriteDiagnosticHeaders(HttpContext context)
    {
        var diagnosticContext = context.RequestServices?.GetService<DiagnosticContext>();
        if (diagnosticContext is not { HasAny: true })
        {
            return;
        }

        foreach (var diag in diagnosticContext.Diagnostics)
        {
            // HTTP headers must be ASCII; percent-encode so non-ASCII values
            // (locale display names like "Español", content titles with accents)
            // survive instead of crashing Kestrel's header writer.
            var severity = Uri.EscapeDataString(diag.Severity.ToString());
            var message = Uri.EscapeDataString(diag.Message);
            var value = diag.Source is not null
                ? $"{severity}|{message}|{Uri.EscapeDataString(diag.Source)}"
                : $"{severity}|{message}";
            context.Response.Headers.Append("X-Pennington-Diagnostic", value);
        }
    }
}