namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class UserInterfaceExamplePlaywrightTests : IClassFixture<UserInterfaceExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly UserInterfaceExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public UserInterfaceExamplePlaywrightTests(UserInterfaceExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_RendersContent()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("CloudFlow");
    }

    [Fact]
    public async Task GettingStartedPage_LoadsAndRenders()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/getting-started");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Getting Started");
        body.ShouldContain("CloudFlow");
    }

    [Fact]
    public async Task ConfigurationPage_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/configuration");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Configuration");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
