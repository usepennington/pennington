namespace Pennington.IntegrationTests.DocsSite;

using AngleSharp.Html.Parser;
using Infrastructure;

/// <summary>
/// Verifies the DocSite chrome renders the expected component looks now that styling lives on the
/// components themselves (inline defaults + a <c>Variant</c> param) rather than in a style
/// registry: the sidebar uses <c>TableOfContentsNavigation</c>'s Pill variant, and the outline
/// marker carries <c>OutlineNavigation</c>'s default classes.
/// </summary>
public class SidebarChromeTests : IClassFixture<DocsWebApplicationFactory>
{
    private const string ChromePage = "/explanation/core/content-pipeline/";

    private readonly HttpClient _client;

    public SidebarChromeTests(DocsWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task SidebarChildLinks_RenderThePillVariant()
    {
        var document = await Load(ChromePage);

        // Child-level links live in the nested list and carry the current-page state attribute.
        var links = document.QuerySelectorAll("#nav-sidebar ul ul a[data-current]");
        links.Length.ShouldBeGreaterThan(0);

        foreach (var link in links)
        {
            var classes = link.ClassList.ToArray();
            // Pill variant markers...
            classes.ShouldContain("rounded-md");
            classes.ShouldContain("gap-1.5");
            classes.ShouldContain("data-[current=true]:bg-primary-500/8");
            // ...and none of the Rail variant's left-border treatment.
            classes.ShouldNotContain("border-l");
        }
    }

    [Fact]
    public async Task OutlineMarker_RendersDefaultClasses()
    {
        var document = await Load(ChromePage);

        var marker = document.QuerySelector("[data-role='page-outline-highlighter']");
        marker.ShouldNotBeNull();

        var classes = marker.ClassList.ToArray();
        // OutlineNavigation default marker look...
        classes.ShouldContain("w-[2px]");
        classes.ShouldContain("rounded-sm");
        classes.ShouldContain("bg-primary-600");
        classes.ShouldContain("dark:bg-primary-300");
        // ...plus the functional classes the outline script depends on, kept hardcoded.
        classes.ShouldContain("absolute");
        classes.ShouldContain("opacity-0");
    }

    private async Task<AngleSharp.Dom.IDocument> Load(string path)
    {
        var html = await _client.GetStringAsync(path, TestContext.Current.CancellationToken);
        return await new HtmlParser().ParseDocumentAsync(html, TestContext.Current.CancellationToken);
    }
}
