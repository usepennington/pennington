namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class RecipeExamplePlaywrightTests : IClassFixture<RecipeExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly RecipeExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public RecipeExamplePlaywrightTests(RecipeExamplePlaywrightFixture fixture)
        => _fixture = fixture;

    public async ValueTask InitializeAsync()
        => _page = await _fixture.Browser.NewPageAsync();

    public async ValueTask DisposeAsync()
        => await _page.CloseAsync();

    [Fact]
    public async Task Homepage_ShowsRecipeCollectionTitle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Recipe Collection");
    }

    [Fact]
    public async Task Homepage_ListsRecipeCards()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var cards = _page.Locator("article");
        var count = await cards.CountAsync();
        count.ShouldBe(7);
    }

    [Fact]
    public async Task Homepage_ShowsRecipeCount()
    {
        await _page.GotoAsync(_fixture.BaseUrl);

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("7");
        body.ShouldContain("recipes");
    }

    [Fact]
    public async Task RecipePage_ShowsTitle()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/chili");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain("Chili");
    }

    [Fact]
    public async Task RecipePage_ShowsIngredients()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/chili");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Ingredients");
        body.ShouldContain("ground beef");
    }

    [Fact]
    public async Task RecipePage_ShowsInstructions()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/chili");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Instructions");
    }

    [Fact]
    public async Task RecipePage_ShowsDescription()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/beer-cheese");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("pub-style beer cheese dip");
    }

    [Fact]
    public async Task RecipePage_ShowsServings()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/cajun-chicken-pasta");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Servings");
        body.ShouldContain("4");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task NonExistentRecipe_ShowsLoading()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/recipes/does-not-exist");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Loading recipe");
    }

    [Theory]
    [InlineData("/recipes/chili", "Chili")]
    [InlineData("/recipes/beer-cheese", "Beer Cheese")]
    [InlineData("/recipes/bacon-wrapped-jalapenos", "Bacon Wrapped Jalapenos")]
    [InlineData("/recipes/cajun-chicken-pasta", "Cajun Chicken Pasta")]
    [InlineData("/recipes/chicken-piccata", "Chicken Paccata")]
    [InlineData("/recipes/chex-mix", "Bourbon Bacon Chex Mix")]
    [InlineData("/recipes/zuppa-toscana", "Lazy Zuppa Toscana")]
    public async Task AllRecipes_RenderSuccessfully(string url, string expectedTitle)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain(expectedTitle);
    }
}
