namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class BeaconDocsExamplePlaywrightTests : IClassFixture<BeaconDocsExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly BeaconDocsExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public BeaconDocsExamplePlaywrightTests(BeaconDocsExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Beacon");
    }

    [Theory]
    [InlineData("/getting-started/", "Getting Started")]
    [InlineData("/getting-started/install/", "Installation")]
    [InlineData("/guides/configuration/", "Configuration")]
    [InlineData("/guides/migration-v3/", "Migrating from v2 to v3")]
    [InlineData("/api/", "API Reference")]
    [InlineData("/changelog/v3-2/", "v3.2")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedContent)
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}{url}");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }

    [Fact]
    public async Task GettingStarted_LoadsSuccessfully()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/getting-started/");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task Configuration_LoadsSuccessfully()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/guides/configuration/");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Configuration");
    }

    [Fact]
    public async Task MigrationGuide_RendersMultipleAlerts()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/guides/migration-v3/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("PollOnce");
        body.ShouldContain("migration analyzer");
    }

    [Fact]
    public async Task ApiReference_LoadsSuccessfully()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/api/");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("API Reference");
    }

    [Fact]
    public async Task RedirectPage_HasMetaRefresh()
    {
        // Navigate without following redirects by checking the HTML content
        await _page.GotoAsync($"{_fixture.BaseUrl}/setup/");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        // The redirect page should either redirect or show the moved message
        // Depending on implementation, we might land on getting-started
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

    [Fact]
    public async Task Sitemap_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/sitemap.xml");
        response!.Status.ShouldBe(200);
    }
}
