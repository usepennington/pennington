namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class BlogExamplePlaywrightTests : IClassFixture<BlogExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly BlogExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public BlogExamplePlaywrightTests(BlogExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Calvin's Chewing Chronicles");
    }

    [Fact]
    public async Task Homepage_ShowsHeroContent()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Test Hero Title");
    }

    [Fact]
    public async Task Homepage_ListsBlogPosts()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var articles = _page.Locator("article");
        var count = await articles.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task BlogPost_RendersContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/2024/03/chewing-magazine-review");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Chewing Magazine");
    }

    [Fact]
    public async Task BlogPost_ShowsDate()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/2024/03/chewing-magazine-review");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("March");
        body.ShouldContain("2024");
    }

    [Fact]
    public async Task BlogPost_ShowsTags()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/2024/03/chewing-magazine-review");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("reviews");
    }

    [Fact]
    public async Task BlogPost_ShowsRenderedMarkdown()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/2024/05/chewing-data-analytics");

        var prose = _page.Locator(".prose");
        await Assertions.Expect(prose).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ArchivePage_ListsAllPosts()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/archive");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("All Posts");
        body.ShouldContain("7");
    }

    [Fact]
    public async Task TagsPage_ListsTags()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/tags");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("All Tags");
    }

    [Fact]
    public async Task TagPage_FiltersByTag()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/tags/reviews");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("reviews");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Theory]
    [InlineData("/blog/2024/03/chewing-magazine-review", "Chewing Magazine")]
    [InlineData("/blog/2024/03/top-five-gum-brands-analysis", "Top 5 Chewing Gum")]
    [InlineData("/blog/2024/04/gum-chewing-apparel-guide", "Gum-Chewing Apparel")]
    [InlineData("/blog/2024/04/mandibular-fitness-regime", "Mandibular Fitness")]
    [InlineData("/blog/2024/05/tongue-exercises-bigger-bubbles", "Tongue Exercises")]
    [InlineData("/blog/2024/05/bazooka-joe-interview", "Bazooka Joe")]
    [InlineData("/blog/2024/05/chewing-data-analytics", "Data Analytics")]
    public async Task AllBlogPosts_RenderSuccessfully(string url, string expectedContent)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }
}
