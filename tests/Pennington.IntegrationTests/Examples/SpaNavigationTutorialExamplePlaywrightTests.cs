namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class SpaNavigationTutorialExamplePlaywrightTests : IClassFixture<SpaNavigationTutorialExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly SpaNavigationTutorialExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public SpaNavigationTutorialExamplePlaywrightTests(SpaNavigationTutorialExamplePlaywrightFixture fixture)
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
        body.ShouldContain("SPA Navigation Tutorial");
    }

    [Theory]
    [InlineData("/introduction", "Introduction")]
    [InlineData("/configuration", "Configuration")]
    [InlineData("/lifecycle", "Lifecycle Events")]
    [InlineData("/advanced", "Advanced Topics")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedTitle)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain(expectedTitle);
    }

    [Fact]
    public async Task Layout_HasSpaIslandAttributes()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/introduction");

        var articleIsland = _page.Locator("[data-spa-island='article']");
        var count = await articleIsland.CountAsync();
        count.ShouldBeGreaterThan(0);

        var navIsland = _page.Locator("[data-spa-island='nav']");
        var navCount = await navIsland.CountAsync();
        navCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Layout_HasSkeletonTemplate()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/introduction");

        var skeleton = _page.Locator("template[data-spa-skeleton-for='article']");
        var count = await skeleton.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsJson()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/_spa-data/introduction.json");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task Sidebar_ShowsNavigationLinks()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/introduction");

        var nav = _page.Locator("nav").First;
        var navContent = await nav.TextContentAsync();
        navContent.ShouldNotBeNull();
        navContent.ShouldContain("Introduction");
        navContent.ShouldContain("Configuration");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
