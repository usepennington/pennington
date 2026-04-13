namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Infrastructure;

public class SearchExamplePlaywrightTests : IClassFixture<SearchExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly SearchExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public SearchExamplePlaywrightTests(SearchExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_Loads()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Random");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }
}