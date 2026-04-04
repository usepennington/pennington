namespace Penn.IntegrationTests.Examples;

using Microsoft.Playwright;
using Penn.IntegrationTests.Infrastructure;

public class MinimalExamplePlaywrightTests : IClassFixture<MinimalExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly MinimalExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public MinimalExamplePlaywrightTests(MinimalExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_ShowsWelcomeTitle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Welcome");
        heading.ShouldContain("My Little Content Engine");
    }

    [Fact]
    public async Task Homepage_ListsContentPages()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var links = _page.Locator("ul li a");
        var count = await links.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ContentPage_RendersTitle()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/page-one");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Page One");
    }

    [Fact]
    public async Task ContentPage_RendersMarkdownBody()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/page-two");

        var prose = _page.Locator(".prose");
        await Assertions.Expect(prose).ToBeVisibleAsync();

        var content = await prose.TextContentAsync();
        content.ShouldNotBeNull();
        content.ShouldContain("Advanced Content Management");
    }

    [Fact]
    public async Task ContentPage_HasNavigationLinks()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/page-one");

        var links = _page.Locator(".prose a");
        var count = await links.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task NonExistentPage_ShowsNotFound()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/does-not-exist");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Not found");
    }

    [Fact]
    public async Task SubFolder_SamplePost_RendersContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/sample-post");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Sample Post with Images");

        // Should render markdown image references
        var images = _page.Locator(".prose img");
        var count = await images.CountAsync();
        count.ShouldBeGreaterThan(0);
    }
}
