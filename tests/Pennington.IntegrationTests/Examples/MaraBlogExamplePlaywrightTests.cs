namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Infrastructure;

public class MaraBlogExamplePlaywrightTests : IClassFixture<MaraBlogExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly MaraBlogExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public MaraBlogExamplePlaywrightTests(MaraBlogExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_ShowsSiteTitle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Mara Writes Code");
    }

    [Fact]
    public async Task Homepage_ShowsHeroContent()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Performance engineer");
    }

    [Fact]
    public async Task Homepage_ListsBlogPosts()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var articles = _page.Locator("article");
        var count = await articles.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("/blog/allocation-traps/", "Allocation Traps")]
    [InlineData("/blog/span-patterns/", "Span")]
    [InlineData("/blog/config-pitfalls/", "Configuration Pitfalls")]
    public async Task BlogPosts_RenderSuccessfully(string url, string expectedContent)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }

    [Fact]
    public async Task SeriesPosts_BelongToSeries()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/allocation-traps/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        // The post content mentions the series topic
        body.ShouldContain("Allocation Traps");
    }

    [Fact]
    public async Task TagsPage_ListsTags()
    {
        // BlogSite generates tag pages - try both /topics and /tags
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/topics");
        if (response!.Status != 200)
        {
            response = await _page.GotoAsync($"{_fixture.BaseUrl}/tags");
        }
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task TagPage_FiltersByTag()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/topics/performance");
        if (response!.Status != 200)
        {
            response = await _page.GotoAsync($"{_fixture.BaseUrl}/tags/performance");
        }
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task ArchivePage_ShowsAllPosts()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/archive");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("All Posts");
    }

    [Fact]
    public async Task Sitemap_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/sitemap.xml");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}