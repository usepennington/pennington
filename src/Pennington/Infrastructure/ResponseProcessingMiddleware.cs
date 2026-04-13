namespace Pennington.Infrastructure;

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Diagnostics;

/// <summary>Captures response body once and runs all registered IResponseProcessors in order.</summary>
public sealed class ResponseProcessingMiddleware(RequestDelegate next)
{
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
                    body = await processor.ProcessAsync(body, context);

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
        if (diagnosticContext is not { HasAny: true }) return;

        foreach (var diag in diagnosticContext.Diagnostics)
        {
            var value = diag.Source is not null
                ? $"{diag.Severity}|{diag.Message}|{diag.Source}"
                : $"{diag.Severity}|{diag.Message}";
            context.Response.Headers.Append("X-Pennington-Diagnostic", value);
        }
    }
}