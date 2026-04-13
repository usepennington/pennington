namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>Processes HTTP response bodies for a specific concern.</summary>
public interface IResponseProcessor
{
    int Order { get; }
    bool ShouldProcess(HttpContext context);
    Task<string> ProcessAsync(string responseBody, HttpContext context);
}