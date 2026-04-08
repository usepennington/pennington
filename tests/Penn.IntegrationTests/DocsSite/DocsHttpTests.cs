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
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);
        await response.ShouldReturnSuccessWithContent("Penn");
    }

    [Fact]
    public async Task ContentPage_ReturnsSuccess_WithTitle()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        await response.ShouldReturnSuccessWithContent("Creating Your First Site");
    }

    [Fact]
    public async Task ContentPage_RendersMarkdown_AsHtml()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        // Should contain rendered markdown headings
        content.ShouldContain("<h2");
        // Should contain prose class (markdown styling)
        content.ShouldContain("prose");
    }

    [Fact]
    public async Task ContentPage_HasNavigationTree()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        // Navigation sidebar should have links to other pages
        content.ShouldContain("Getting Started");
    }

    [Fact]
    public async Task StylesCss_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/styles.css", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ScriptsJs_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/_content/Penn.UI/scripts.js", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SpaEngineJs_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/_content/Penn.UI/spa-engine.js", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsJsonForContentPage()
    {
        var response = await _client.GetAsync("/_spa-data/getting-started/creating-first-site.json", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("Creating Your First Site");
        content.ShouldContain("islands");
        content.ShouldContain("content");
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsIslandHtmlWithArticle()
    {
        var response = await _client.GetAsync("/_spa-data/getting-started/creating-first-site.json", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        // The "content" island should contain the rendered article with prose styling.
        // Angle brackets are JSON-encoded (\u003C), so check for the class name and tag.
        content.ShouldContain("prose");
        content.ShouldContain("header");
    }

    [Fact]
    public async Task SpaDataEndpoint_ReturnsReloadForNonExistentPage()
    {
        var response = await _client.GetAsync("/_spa-data/does-not-exist.json", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("reload");
    }

    [Fact]
    public async Task StaticAsset_Favicon_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/favicon.ico", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonExistentPage_Returns200_WithNotFound()
    {
        // Blazor SSR returns 200 with "Page not found" in body (not a 404)
        var response = await _client.GetAsync("/this-does-not-exist/", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("Page not found");
    }

    [Fact]
    public async Task Layout_HasSkipToContentLink()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""href="#main-content""");
        content.ShouldContain("Skip to main content");
    }

    [Fact]
    public async Task Layout_HasSkipToNavigationLink()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""href="#nav-sidebar""");
        content.ShouldContain("Skip to navigation");
    }

    [Fact]
    public async Task Layout_ContentArticleHasMainContentId()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""id="main-content""");
    }

    [Fact]
    public async Task Layout_SidebarNavHasAriaLabel()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""aria-label="Sidebar""");
    }

    [Fact]
    public async Task Layout_HeaderNavHasAriaLabel()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""aria-label="Site""");
    }

    [Fact]
    public async Task StylesCss_ContainsSkipLinkUtilities()
    {
        var response = await _client.GetAsync("/styles.css", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("sr-only");
    }

    [Fact]
    public async Task Homepage_HasFontPreloadHints()
    {
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("""rel="preload" href="fonts/lexend.woff2" as="font" type="font/woff2" crossorigin""");
        content.ShouldContain("""rel="preload" href="fonts/noto-sans.woff2" as="font" type="font/woff2" crossorigin""");
    }

    [Fact]
    public async Task FontPreloadHints_AppearBeforeStylesheet()
    {
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var preloadIndex = content.IndexOf("""rel="preload""");
        var stylesheetIndex = content.IndexOf("""rel="stylesheet""");
        preloadIndex.ShouldBeGreaterThan(-1);
        stylesheetIndex.ShouldBeGreaterThan(-1);
        preloadIndex.ShouldBeLessThan(stylesheetIndex);
    }

    [Theory]
    [InlineData("/getting-started/creating-first-site/")]
    [InlineData("/getting-started/deploying-to-github-pages/")]
    [InlineData("/guides/markdown-extensions/")]
    [InlineData("/reference/front-matter-properties/")]
    [InlineData("/under-the-hood/syntax-highlighting-system/")]
    public async Task AllSections_ReturnSuccess(string url)
    {
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("prose");
    }
}
