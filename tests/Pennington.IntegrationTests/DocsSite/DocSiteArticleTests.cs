namespace Pennington.IntegrationTests.DocsSite;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pennington.DocSite.Slots.Components;

/// <summary>
/// <see cref="DocSiteArticle"/> accepts its body either as the markdown pipeline's HTML string or as
/// child components, and both land inside the same prose wrapper. The component path is what a
/// generated reference section (one page per namespace, built from a model) uses to keep the header,
/// pager, and typography of a markdown page without first flattening its content to a string.
/// </summary>
public sealed class DocSiteArticleTests
{
    [Fact]
    public async Task HtmlContent_RendersInsideProseWrapper()
    {
        var html = await RenderAsync(new Dictionary<string, object?>
        {
            [nameof(DocSiteArticle.Title)] = "Strings",
            [nameof(DocSiteArticle.HtmlContent)] = "<h2 id=\"functions\">Functions</h2>",
        });

        html.ShouldContain("<h1");
        html.ShouldContain(">Strings</h1>");
        ProseBody(html).ShouldContain("<h2 id=\"functions\">Functions</h2>");
    }

    [Fact]
    public async Task ChildContent_RendersInsideProseWrapper_AndWinsOverHtmlContent()
    {
        RenderFragment body = builder =>
        {
            builder.OpenElement(0, "h2");
            builder.AddAttribute(1, "id", "types");
            builder.AddContent(2, "Types");
            builder.CloseElement();
        };

        var html = await RenderAsync(new Dictionary<string, object?>
        {
            [nameof(DocSiteArticle.Title)] = "Strings",
            [nameof(DocSiteArticle.HtmlContent)] = "<p>ignored</p>",
            [nameof(DocSiteArticle.ChildContent)] = body,
            [nameof(DocSiteArticle.NextPageName)] = "std.io",
            [nameof(DocSiteArticle.NextPageHref)] = "/stdlib/io/",
        });

        ProseBody(html).ShouldContain("<h2 id=\"types\">Types</h2>");
        html.ShouldNotContain("ignored");

        // The pager still renders around component-authored content.
        html.ShouldContain("href=\"/stdlib/io/\"");
        html.ShouldContain("std.io");
    }

    private static async Task<string> RenderAsync(Dictionary<string, object?> parameters)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        await using var sp = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(sp, sp.GetRequiredService<ILoggerFactory>());

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<DocSiteArticle>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    /// <summary>The inner HTML of the <c>&lt;main class="prose …"&gt;</c> wrapper.</summary>
    private static string ProseBody(string html)
    {
        var start = html.IndexOf("<main class=\"prose", StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, "prose wrapper missing");
        var end = html.IndexOf("</main>", start, StringComparison.Ordinal);
        return html[start..end];
    }
}
