using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

/// <summary>
/// DOM-level tests for <see cref="WordBreakHtmlRewriter"/>, driven through the
/// real <see cref="HtmlResponseRewritingProcessor"/> so the rewriter parses,
/// mutates, and serializes exactly as it does in the response pipeline. Ported
/// from the former third-party WordbreakMiddleware integration tests.
/// </summary>
public class WordBreakHtmlRewriterTests
{
    private static Task<string> Rewrite(string html, Action<WordBreakOptions>? configure = null)
    {
        var options = new WordBreakOptions { MinimumCharacters = 10 };
        configure?.Invoke(options);

        var processor = new HtmlResponseRewritingProcessor([new WordBreakHtmlRewriter(options)]);
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return processor.ProcessAsync(html, ctx);
    }

    [Fact]
    public async Task DoesNotProcessPlainBodyText()
    {
        var content = await Rewrite("<html><body>Short HttpClient text System.Net.Http.HttpClient</body></html>");

        content.ShouldContain("System.Net.Http.HttpClient");
        content.ShouldNotContain("<wbr>");
    }

    [Fact]
    public async Task ProcessesTextInHeadings()
    {
        var content = await Rewrite("<html><body><h1>The System.Net.Http.HttpClient class</h1></body></html>");

        content.ShouldContain("System.<wbr>Net.<wbr>Http.<wbr>Http<wbr>Client");
    }

    [Fact]
    public async Task ProcessesNamespacedIdentifiersInHeadings()
    {
        var content = await Rewrite("<html><body><h2>Use System.Net.Http.HttpClientHandler for configuration</h2></body></html>");

        content.ShouldContain("System.<wbr>Net.<wbr>Http.<wbr>Http<wbr>Client<wbr>Handler");
    }

    [Fact]
    public async Task SkipsScriptContent()
    {
        var html = """
            <html><body>
                <script>var System.Net.Http = 'test';</script>
                <h3>System.Net.Http.HttpClient</h3>
            </body></html>
            """;
        var content = await Rewrite(html);

        content.ShouldContain("var System.Net.Http = 'test'");
        content.ShouldContain("<h3>System.<wbr>Net.<wbr>Http.<wbr>Http<wbr>Client</h3>");
    }

    [Fact]
    public async Task SkipsCodeContentOutsideSelector()
    {
        var html = """
            <html><body>
                <code>System.Net.Http</code>
                <h5>System.Net.Http outside code</h5>
            </body></html>
            """;
        var content = await Rewrite(html);

        content.ShouldContain("<code>System.Net.Http</code>");
        content.ShouldContain("<h5>System.<wbr>Net.<wbr>Http outside code</h5>");
    }

    [Fact]
    public async Task ProcessesHeadingsButNotParagraphsWithDefaultSelector()
    {
        var html = """
            <html><body>
                <h1>System.Net.Http</h1>
                <h2>Microsoft.Extensions.DependencyInjection</h2>
                <h3>System.IO.FileSystem</h3>
                <p>System.Net.Http in paragraph</p>
            </body></html>
            """;
        var content = await Rewrite(html);

        content.ShouldContain("<h1>System.<wbr>Net.<wbr>Http</h1>");
        content.ShouldContain("<h2>Microsoft.<wbr>Extensions.<wbr>Dependency<wbr>Injection</h2>");
        content.ShouldContain("<h3>System.<wbr>IO.<wbr>File<wbr>System</h3>");
        content.ShouldContain("<p>System.Net.Http in paragraph</p>"); // default selector excludes <p>
    }

    [Fact]
    public async Task ProcessesTextBreakClass()
    {
        var html = "<html><body><code class=\"text-break\">MyLittleContentEngine.Services.Content.TableOfContents.ContentTocItem</code></body></html>";
        var content = await Rewrite(html);

        content.ShouldContain("My<wbr>Little<wbr>Content<wbr>Engine.<wbr>Services.<wbr>Content.<wbr>Table<wbr>Of<wbr>Contents.<wbr>Content<wbr>Toc<wbr>Item");
    }

    [Fact]
    public async Task PreservesSurroundingHtmlStructure()
    {
        var html = """
            <html><body>
                <div class="container">
                    <h2 id="test">System.Net.Http text</h2>
                </div>
            </body></html>
            """;
        var content = await Rewrite(html);

        content.ShouldContain("class=\"container\"");
        content.ShouldContain("id=\"test\"");
        content.ShouldContain("System.<wbr>Net.<wbr>Http");
    }

    [Fact]
    public async Task RespectsMinimumCharactersSetting()
    {
        var html = "<html><body><h1>System.Net.Http and Microsoft.Extensions.DependencyInjection.ServiceCollection</h1></body></html>";
        var content = await Rewrite(html, o => o.MinimumCharacters = 30);

        // System.Net.Http is 15 chars, below the threshold.
        content.ShouldContain("System.Net.Http and");
        content.ShouldNotContain("System.<wbr>Net.<wbr>Http");

        // The long namespace breaks at dots, but each segment stays under 30 — no uppercase breaks.
        content.ShouldContain("Microsoft.<wbr>Extensions.<wbr>DependencyInjection.<wbr>ServiceCollection");
    }

    [Fact]
    public async Task UsesCustomWordBreakCharacters()
    {
        var content = await Rewrite(
            "<html><body><h1>System.Net.Http</h1></body></html>",
            o => o.WordBreakCharacters = "­");

        content.ShouldContain("System.­Net.­Http");
        content.ShouldNotContain("<wbr>");
    }

    [Fact]
    public async Task BreaksOnUppercaseInLongSegments()
    {
        var html = "<html><body><h1>MyLittleContentEngine.IntegrationTests.ExampleProjects.MultipleContentSourceExampleWebApplicationFactory</h1></body></html>";
        var content = await Rewrite(html);

        content.ShouldContain("My<wbr>Little<wbr>Content<wbr>Engine.<wbr>Integration<wbr>Tests.<wbr>Example<wbr>Projects.<wbr>Multiple<wbr>Content<wbr>Source<wbr>Example<wbr>Web<wbr>Application<wbr>Factory");
    }

    [Fact]
    public async Task DoesNotBreakConsecutiveUppercaseLetters()
    {
        var content = await Rewrite("<h1>System.IO.XMLHttpRequestFactory</h1>");

        content.ShouldContain("System.<wbr>IO.<wbr>");
        content.ShouldContain("XMLHttp<wbr>Request<wbr>Factory");
        content.ShouldNotContain("X<wbr>M<wbr>L");
        content.ShouldNotContain("I<wbr>O");
    }
}
