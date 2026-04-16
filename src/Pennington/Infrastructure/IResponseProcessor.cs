namespace Pennington.Infrastructure;

using Microsoft.AspNetCore.Http;

/// <summary>Processes HTTP response bodies for a specific concern.</summary>
public interface IResponseProcessor
{
    /// <summary>Execution order; lower values run earlier in the response pipeline.</summary>
    int Order { get; }

    /// <summary>Returns true when this processor should run for the current request.</summary>
    bool ShouldProcess(HttpContext context);

    /// <summary>Transforms <paramref name="responseBody"/> and returns the processed body.</summary>
    Task<string> ProcessAsync(string responseBody, HttpContext context);
}