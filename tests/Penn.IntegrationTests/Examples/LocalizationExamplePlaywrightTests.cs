namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class LocalizationExamplePlaywrightTests : IClassFixture<LocalizationExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly LocalizationExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public LocalizationExamplePlaywrightTests(LocalizationExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task EnglishHomepage_Loads()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Tavern");
    }

    [Fact]
    public async Task PigLatinHomepage_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/pl/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Averntay");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
