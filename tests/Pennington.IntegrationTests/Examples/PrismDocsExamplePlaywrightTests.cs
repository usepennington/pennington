namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Infrastructure;

public class PrismDocsExamplePlaywrightTests : IClassFixture<PrismDocsExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly PrismDocsExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public PrismDocsExamplePlaywrightTests(PrismDocsExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Prism");
    }

    [Theory]
    [InlineData("/guides/enum-generator/", "Enum Generator")]
    [InlineData("/guides/migration-v2/", "Migrating to Prism v2")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedContent)
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}{url}");
        response!.Status.ShouldBe(200);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }

    [Fact]
    public async Task EnumGenerator_RendersCodeBlocks()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/guides/enum-generator/");

        var codeBlocks = _page.Locator("pre code");
        var count = await codeBlocks.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task MigrationGuide_RendersCodeBlocks()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/guides/migration-v2/");

        var codeBlocks = _page.Locator("pre code");
        var count = await codeBlocks.CountAsync();
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
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/search-index-en.json");
        response!.Status.ShouldBe(200);
    }
}