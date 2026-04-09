namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class LocalizationTutorialExamplePlaywrightTests : IClassFixture<LocalizationTutorialExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly LocalizationTutorialExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public LocalizationTutorialExamplePlaywrightTests(LocalizationTutorialExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task EnglishHomepage_ShowsWelcome()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Welcome");
    }

    [Fact]
    public async Task SpanishHomepage_Loads()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/es/");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task SpanishGettingStarted_ShowsTranslatedContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/es/getting-started/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Primeros Pasos");
    }

    [Fact]
    public async Task SpanishConfiguration_ShowsFallback()
    {
        // Spanish configuration doesn't exist — should fall back to English content
        await _page.GotoAsync($"{_fixture.BaseUrl}/es/configuration/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        // Should show English content since Spanish version doesn't exist
        body.ShouldContain("Configuration");
    }

    [Fact]
    public async Task EnglishPages_RenderSuccessfully()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/getting-started/");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
