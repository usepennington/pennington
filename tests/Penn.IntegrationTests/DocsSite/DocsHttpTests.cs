namespace Penn.IntegrationTests.DocsSite;

using Penn.IntegrationTests.Infrastructure;

public class DocsHttpTests : IClassFixture<DocsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DocsHttpTests(DocsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Homepage_ReturnsSuccess_WithHeroContent()
    {
        var response = await _client.GetAsync("/");
        await response.ShouldReturnSuccessWithContent("Penn");
    }

    [Fact]
    public async Task ContentPage_ReturnsSuccess_WithTitle()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/");
        await response.ShouldReturnSuccessWithContent("Creating Your First Site");
    }

    [Fact]
    public async Task ContentPage_RendersMarkdown_AsHtml()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/");
        var content = await response.Content.ReadAsStringAsync();
        // Should contain rendered markdown headings
        content.ShouldContain("<h2");
        // Should contain prose class (markdown styling)
        content.ShouldContain("prose");
    }

    [Fact]
    public async Task ContentPage_HasNavigationTree()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/");
        var content = await response.Content.ReadAsStringAsync();
        // Navigation sidebar should have links to other pages
        content.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task StylesCss_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/styles.css");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ScriptsJs_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/_content/Penn.UI/scripts.js");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SpaEngineJs_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/_content/Penn.UI/spa-engine.js");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsJsonForContentPage()
    {
        var response = await _client.GetAsync("/_spa-data/getting-started/creating-first-site.json");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Creating Your First Site");
        content.ShouldContain("islands");
        content.ShouldContain("content");
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsIslandHtmlWithArticle()
    {
        var response = await _client.GetAsync("/_spa-data/getting-started/creating-first-site.json");
        var content = await response.Content.ReadAsStringAsync();
        // The "content" island should contain the rendered article with prose styling.
        // Angle brackets are JSON-encoded (\u003C), so check for the class name and tag.
        content.ShouldContain("prose");
        content.ShouldContain("header");
    }

    [Fact]
    public async Task SpaDataEndpoint_Returns404ForNonExistentPage()
    {
        var response = await _client.GetAsync("/_spa-data/does-not-exist.json");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StaticAsset_Favicon_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/favicon.ico");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonExistentPage_Returns200_WithNotFound()
    {
        // Blazor SSR returns 200 with "Page not found" in body (not a 404)
        var response = await _client.GetAsync("/this-does-not-exist/");
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Page not found");
    }

    [Theory]
    [InlineData("/getting-started/creating-first-site/")]
    [InlineData("/getting-started/deploying-to-github-pages/")]
    [InlineData("/guides/markdown-extensions/")]
    [InlineData("/reference/front-matter-properties/")]
    [InlineData("/under-the-hood/syntax-highlighting-system/")]
    public async Task AllSections_ReturnSuccess(string url)
    {
        var response = await _client.GetAsync(url);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("prose");
    }
}
