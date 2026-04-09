namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Diagnostics;

/// <summary>
/// Response processor that resolves xref: cross-reference links in HTML responses.
/// Delegates to <see cref="XrefResolvingService"/> for the actual resolution logic.
/// </summary>
public sealed class XrefResolvingProcessor : IResponseProcessor
{
    private readonly XrefResolvingService _service;

    public XrefResolvingProcessor(XrefResolvingService service)
    {
        _service = service;
    }

    public int Order => -10;

    public bool ShouldProcess(HttpContext context)
    {
        var contentType = context.Response.ContentType ?? "";
        return context.Response.StatusCode is >= 200 and < 300
            && contentType.Contains("text/html");
    }

    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var diagnostics = context.RequestServices.GetRequiredService<DiagnosticContext>();
        return await _service.ResolveAsync(responseBody, diagnostics);
    }
}
