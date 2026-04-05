namespace Penn.IntegrationTests.Examples;

using Microsoft.Playwright;
using Penn.IntegrationTests.Infrastructure;

public class MultipleContentSourceExamplePlaywrightTests : IClassFixture<MultipleContentSourceExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly MultipleContentSourceExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public MultipleContentSourceExamplePlaywrightTests(MultipleContentSourceExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_LoadsAndShowsContent()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldNotContain("Content not found");
        body.ShouldContain("Daily Life Hub");
    }

    [Fact]
    public async Task BlogPage_LoadsBestPizzaToppings()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/blog/best-pizza-toppings");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldNotContain("Content not found");
        body.ShouldContain("Pizza");
    }

    [Fact]
    public async Task DocsPage_LoadsCoffeeBrewingGuide()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/docs/coffee-brewing-guide");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldNotContain("Content not found");
        body.ShouldContain("Coffee");
    }

    [Fact]
    public async Task AboutPage_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/about");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldNotContain("Content not found");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
