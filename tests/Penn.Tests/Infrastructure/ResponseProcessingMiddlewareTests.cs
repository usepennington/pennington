using System.Text;
using Microsoft.AspNetCore.Http;
using Penn.Infrastructure;

namespace Penn.Tests.Infrastructure;

public class ResponseProcessingMiddlewareTests
{
    [Fact]
    public async Task Middleware_PassesThrough_WhenNoProcessors()
    {
        var expectedBody = "<html><body>Hello</body></html>";
        var middleware = new ResponseProcessingMiddleware(async context =>
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(expectedBody);
        });

        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context, []);

        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody).ReadToEndAsync(TestContext.Current.CancellationToken);
        body.ShouldBe(expectedBody);
    }

    [Fact]
    public async Task Middleware_RunsApplicableProcessor()
    {
        var originalBody = "<html><body>Hello</body></html>";
        var middleware = new ResponseProcessingMiddleware(async context =>
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(originalBody);
        });

        var processor = new AppendingProcessor("<!-- appended -->");
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context, [processor]);

        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody).ReadToEndAsync(TestContext.Current.CancellationToken);
        body.ShouldBe("<html><body>Hello</body></html><!-- appended -->");
    }

    [Fact]
    public async Task Middleware_SkipsProcessor_WhenShouldProcessIsFalse()
    {
        var expectedBody = "<html><body>Hello</body></html>";
        var middleware = new ResponseProcessingMiddleware(async context =>
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(expectedBody);
        });

        var processor = new NeverProcessor();
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context, [processor]);

        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody).ReadToEndAsync(TestContext.Current.CancellationToken);
        body.ShouldBe(expectedBody);
    }

    /// <summary>A processor that always applies and appends text to the body.</summary>
    private sealed class AppendingProcessor(string suffix) : IResponseProcessor
    {
        public int Order => 0;
        public bool ShouldProcess(HttpContext context) => true;
        public Task<string> ProcessAsync(string responseBody, HttpContext context)
            => Task.FromResult(responseBody + suffix);
    }

    /// <summary>A processor that never applies.</summary>
    private sealed class NeverProcessor : IResponseProcessor
    {
        public int Order => 0;
        public bool ShouldProcess(HttpContext context) => false;
        public Task<string> ProcessAsync(string responseBody, HttpContext context)
            => Task.FromResult(responseBody + "SHOULD NOT APPEAR");
    }
}
