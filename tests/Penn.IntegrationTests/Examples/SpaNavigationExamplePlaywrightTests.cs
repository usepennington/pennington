namespace Penn.IntegrationTests.Examples;

using Microsoft.Playwright;
using Penn.IntegrationTests.Infrastructure;

public class SpaNavigationExamplePlaywrightTests : IClassFixture<SpaNavigationExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly SpaNavigationExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public SpaNavigationExamplePlaywrightTests(SpaNavigationExamplePlaywrightFixture fixture)
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
        body.ShouldContain("My Recipe Book");
    }

    [Fact]
    public async Task RecipePage_LoadsAndShowsContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/pasta-carbonara");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Pasta Carbonara");
    }

    [Fact]
    public async Task RecipePage_ShowsInfoCard()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/pasta-carbonara");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        // The recipe info card should display prep time, cook time, or servings
        body.ShouldContain("4"); // servings
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Theory]
    [InlineData("/pasta-carbonara", "Pasta Carbonara")]
    [InlineData("/chocolate-cake", "Chocolate Cake")]
    [InlineData("/thai-green-curry", "Thai Green Curry")]
    public async Task AllRecipes_RenderSuccessfully(string url, string expectedContent)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedContent);
    }
}
