namespace Penn.Infrastructure;

using System.Text;
using Microsoft.AspNetCore.Http;

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

                var bytes = Encoding.UTF8.GetBytes(body);
                context.Response.ContentLength = null;
                await originalBodyStream.WriteAsync(bytes);
            }
            else
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
