namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class RoslynIntegrationExamplePlaywrightTests : IClassFixture<RoslynIntegrationExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly RoslynIntegrationExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public RoslynIntegrationExamplePlaywrightTests(RoslynIntegrationExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Welcome");
        body.ShouldContain("My Little Content Engine");
    }

    [Fact]
    public async Task RoslynIntegrationDemoPage_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/roslyn-integration-demo");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Roslyn Integration Demo");
    }

    [Fact]
    public async Task SubFolderPage_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/page-one");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Page One");
    }

    [Fact]
    public async Task SubFolderPageTwo_Loads()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/sub-folder/page-two");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}
