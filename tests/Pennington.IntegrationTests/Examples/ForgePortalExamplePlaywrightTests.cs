namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class ForgePortalExamplePlaywrightTests : IClassFixture<ForgePortalExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly ForgePortalExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public ForgePortalExamplePlaywrightTests(ForgePortalExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Forge");
    }

    [Theory]
    [InlineData("/docs/getting-started", "Getting Started")]
    [InlineData("/docs/api-keys", "API Keys")]
    [InlineData("/blog/welcome", "Welcome to Forge")]
    [InlineData("/blog/q1-retro", "Q1 Retrospective")]
    [InlineData("/about", "About Forge")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedContent)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }

    [Fact]
    public async Task PipelineCodeBlock_RendersContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/docs/pipeline-config");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Pipeline");
    }

    [Fact]
    public async Task FeedbackWidget_IsInjected()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/docs/getting-started");

        var widget = _page.Locator("#feedback-widget");
        var count = await widget.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Sidebar_ShowsMultipleSections()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/docs/getting-started");

        var nav = _page.Locator("nav").First;
        var navContent = await nav.TextContentAsync();
        navContent.ShouldNotBeNull();
        navContent.ShouldContain("Getting Started");
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
}
