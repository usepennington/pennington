using AngleSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

namespace Pennington.Tests.MonorailCss;

public class MonorailCssTests
{
    [Fact]
    public async Task CssClassCollectorProcessor_ExtractsClassesFromMultiLineAttribute()
    {
        const string html = """
            <html><body>
            <main class="prose prose-invert max-w-full
                         prose-headings:font-display prose-headings:font-normal prose-headings:text-warm-50 prose-headings:scroll-m-20
                         prose-h2:text-[28px] prose-h2:tracking-[-0.018em] prose-h2:mt-14 prose-h2:mb-4
                         prose-h3:font-sans prose-h3:font-semibold prose-h3:text-[17px] prose-h3:mt-8 prose-h3:mb-3
                         prose-p:text-warm-100 prose-p:text-[15.5px] prose-p:leading-[1.65] prose-p:max-w-[700px] prose-p:text-pretty
                         prose-a:text-orange-500 prose-a:no-underline hover:prose-a:underline
                         prose-strong:text-warm-50
                         prose-code:font-mono prose-code:text-orange-500 prose-code:text-[0.9em] prose-code:before:content-none prose-code:after:content-none
                         prose-pre:bg-warm-900 prose-pre:border prose-pre:border-warm-800 prose-pre:rounded-xl prose-pre:p-0 prose-pre:font-mono prose-pre:text-[13px]
                         prose-li:text-warm-100 prose-li:marker:text-warm-500
                         prose-blockquote:border-l-2 prose-blockquote:border-orange-500 prose-blockquote:text-warm-100 prose-blockquote:not-italic
                         prose-hr:border-warm-800
                         prose-table:bg-warm-900 prose-table:border prose-table:border-warm-800 prose-table:rounded-xl prose-table:overflow-hidden prose-table:font-mono prose-table:text-[13.5px]
                         prose-th:bg-warm-800 prose-th:font-sans prose-th:font-medium prose-th:text-[11px] prose-th:uppercase prose-th:tracking-[0.08em] prose-th:text-warm-300 prose-th:px-[18px] prose-th:py-3 prose-th:text-left
                         prose-td:px-[18px] prose-td:py-3.5 prose-td:text-warm-50 prose-td:border-warm-800">
                content
            </main>
            </body></html>
            """;

        var collector = new CssClassCollector();
        var processor = new CssClassCollectorProcessor(collector, NullLogger<CssClassCollectorProcessor>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/multi-line-test";
        context.Response.ContentType = "text/html";

        await ((IResponseProcessor)processor).ProcessAsync(html, context);

        var classes = collector.GetClasses();

        // Sanity: the simple classes from the first line.
        classes.ShouldContain("prose");
        classes.ShouldContain("prose-invert");
        classes.ShouldContain("max-w-full");

        // Classes that appear only after a newline must also be picked up.
        classes.ShouldContain("prose-headings:font-display");
        classes.ShouldContain("prose-h2:text-[28px]");
        classes.ShouldContain("prose-td:border-warm-800");
        classes.ShouldContain("hover:prose-a:underline");
        classes.ShouldContain("prose-h2:tracking-[-0.018em]");
    }

    [Fact]
    public async Task CssClassCollectorProcessor_ExtractsClassesAfterAngleSharpRoundTrip()
    {
        const string html = """
            <html><body>
            <main class="prose prose-invert max-w-full
                         prose-headings:font-display
                         prose-h2:text-[28px] prose-h2:tracking-[-0.018em]
                         prose-td:px-[18px] prose-td:py-3.5
                         hover:prose-a:underline">
                content
            </main>
            </body></html>
            """;

        var browsingContext = BrowsingContext.New(Configuration.Default);
        var document = await browsingContext.OpenAsync(req => req.Content(html), TestContext.Current.CancellationToken);
        var roundTripped = document.ToHtml();

        var collector = new CssClassCollector();
        var processor = new CssClassCollectorProcessor(collector, NullLogger<CssClassCollectorProcessor>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/round-trip-test";
        context.Response.ContentType = "text/html";

        await ((IResponseProcessor)processor).ProcessAsync(roundTripped, context);

        var classes = collector.GetClasses();
        classes.ShouldContain("prose-headings:font-display");
        classes.ShouldContain("prose-h2:text-[28px]");
        classes.ShouldContain("prose-h2:tracking-[-0.018em]");
        classes.ShouldContain("prose-td:px-[18px]");
        classes.ShouldContain("prose-td:py-3.5");
        classes.ShouldContain("hover:prose-a:underline");
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

}