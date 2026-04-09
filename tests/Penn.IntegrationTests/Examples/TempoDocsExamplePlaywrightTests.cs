namespace Penn.IntegrationTests.Examples;

using Microsoft.Playwright;
using Penn.IntegrationTests.Infrastructure;

public class TempoDocsExamplePlaywrightTests : IClassFixture<TempoDocsExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly TempoDocsExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public TempoDocsExamplePlaywrightTests(TempoDocsExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Tempo");
    }

    [Theory]
    [InlineData("/getting-started/", "Getting Started")]
    [InlineData("/configuration/", "Configuration")]
    [InlineData("/api-reference/", "API Reference")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedTitle)
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}{url}");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedTitle);
    }

    [Fact]
    public async Task GettingStarted_RendersNoteAlert()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/getting-started/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Tempo requires .NET 9");
    }

    [Fact]
    public async Task Configuration_RendersWarningAlert()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/configuration/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("thread pool starvation");
    }

    [Fact]
    public async Task ApiReference_RendersTable()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/api-reference/");

        var tables = _page.Locator("table");
        var count = await tables.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task SearchIndex_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/search-index.json");
        response!.Status.ShouldBe(200);
    }
}
