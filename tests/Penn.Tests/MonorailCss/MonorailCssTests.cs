using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.MonorailCss;

namespace Pennington.Tests.MonorailCss;

public class MonorailCssTests
{
    [Fact]
    public void CssClassCollector_CollectsClasses()
    {
        var collector = new CssClassCollector();

        collector.BeginProcessing();
        try
        {
            collector.AddClasses("/test", ["unique-bg-red-500", "unique-text-white", "unique-p-4"]);
        }
        finally
        {
            collector.EndProcessing();
        }

        var classes = collector.GetClasses();
        classes.ShouldContain("unique-bg-red-500");
        classes.ShouldContain("unique-text-white");
        classes.ShouldContain("unique-p-4");
    }

    [Fact]
    public async Task CssClassCollector_IsThreadSafe()
    {
        var collector = new CssClassCollector();
        var exceptions = new List<Exception>();

        // Use unique prefixes so we can verify our specific classes were added
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            try
            {
                collector.BeginProcessing();
                try
                {
                    collector.AddClasses($"/page-{i}", [$"threadsafe-{i}-a", $"threadsafe-{i}-b"]);
                }
                finally
                {
                    collector.EndProcessing();
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        exceptions.ShouldBeEmpty();

        var classes = collector.GetClasses();
        // Verify all 20 unique classes from this test were added
        for (var i = 0; i < 10; i++)
        {
            classes.ShouldContain($"threadsafe-{i}-a");
            classes.ShouldContain($"threadsafe-{i}-b");
        }
    }

    [Fact]
    public async Task CssClassCollectorProcessor_ExtractsClassesFromHtml()
    {
        var collector = new CssClassCollector();
        var logger = NullLogger<CssClassCollectorProcessor>.Instance;
        var processor = new CssClassCollectorProcessor(collector, logger);
        var responseProcessor = (Pennington.Infrastructure.IResponseProcessor)processor;

        var html = """<div class="extract-bg-blue-500 extract-text-white extract-p-4"><span class="extract-font-bold">Hello</span></div>""";

        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Response.ContentType = "text/html";

        responseProcessor.ShouldProcess(context).ShouldBeTrue();

        await responseProcessor.ProcessAsync(html, context);

        var classes = collector.GetClasses();
        classes.ShouldContain("extract-bg-blue-500");
        classes.ShouldContain("extract-text-white");
        classes.ShouldContain("extract-p-4");
        classes.ShouldContain("extract-font-bold");
    }
}
