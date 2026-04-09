namespace Penn.IntegrationTests.Examples;

using Microsoft.Playwright;
using Penn.IntegrationTests.Infrastructure;

public class AlexBlogExamplePlaywrightTests : IClassFixture<AlexBlogExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly AlexBlogExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public AlexBlogExamplePlaywrightTests(AlexBlogExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Alex's Dev Blog");
    }

    [Fact]
    public async Task Homepage_ShowsHeroContent()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Hi, I'm Alex");
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
    [InlineData("/blog/building-a-cli-part-1/", "Building a CLI Tool, Part 1")]
    [InlineData("/blog/building-a-cli-part-2/", "Building a CLI Tool, Part 2")]
    [InlineData("/blog/why-i-switched-to-linux/", "Why I Switched to Linux")]
    public async Task BlogPosts_RenderSuccessfully(string url, string expectedContent)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }

    [Fact]
    public async Task BlogPost_ShowsTags()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/building-a-cli-part-1/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("dotnet");
        body.ShouldContain("cli");
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
        await _page.GotoAsync($"{_fixture.BaseUrl}/tags/dotnet");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("dotnet");
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
