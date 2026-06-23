namespace Pennington.IntegrationTests.DocsSite;

using AngleSharp.Html.Parser;
using Infrastructure;

/// <summary>
/// Verifies the area-home behaviour in the DocSite sidebar: an area whose folder has an
/// <c>index.md</c> home page gets a pill that navigates straight there (<c>data-area-direct</c>),
/// and the redundant home entry is dropped from that area's table of contents. Areas without a
/// home page keep the preview-on-click behaviour and a full TOC. The docs site's <c>showcase</c>
/// area has a home page; <c>how-to</c> does not.
/// </summary>
public class AreaHomeNavigationTests : IClassFixture<DocsWebApplicationFactory>
{
    // Any content page renders every area's pill and (server-side) every area's TOC container.
    private const string AnyPage = "/explanation/core/content-pipeline/";

    private readonly HttpClient _client;

    public AreaHomeNavigationTests(DocsWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task AreaWithHomePage_PillNavigatesDirectly_AndHomeIsDroppedFromToc()
    {
        var document = await Load(AnyPage);

        var pill = document.QuerySelector("[data-area-nav] a[data-area='showcase']");
        pill.ShouldNotBeNull();

        // The pill links to the area home and is flagged so the click handler lets the
        // href navigate instead of just previewing the TOC.
        pill.GetAttribute("data-area-direct").ShouldBe("true");
        pill.GetAttribute("href").ShouldContain("showcase");

        // The home entry is the only showcase page, so dropping it leaves an empty TOC —
        // no link back to the home the pill already points at.
        var tocLinks = document.QuerySelectorAll("[data-area-toc='showcase'] a");
        tocLinks.Length.ShouldBe(0);
    }

    [Fact]
    public async Task AreaWithoutHomePage_KeepsTocPreview()
    {
        var document = await Load(AnyPage);

        var pill = document.QuerySelector("[data-area-nav] a[data-area='how-to']");
        pill.ShouldNotBeNull();

        // No index.md home: the pill is not a direct-nav pill, and the area keeps a full TOC.
        pill.GetAttribute("data-area-direct").ShouldBeNull();

        var tocLinks = document.QuerySelectorAll("[data-area-toc='how-to'] a");
        tocLinks.Length.ShouldBeGreaterThan(0);
    }

    private async Task<AngleSharp.Dom.IDocument> Load(string path)
    {
        var html = await _client.GetStringAsync(path, TestContext.Current.CancellationToken);
        return await new HtmlParser().ParseDocumentAsync(html, TestContext.Current.CancellationToken);
    }
}
