namespace Pennington.IntegrationTests.DocsSite;

using Infrastructure;

[Collection(DocsTestServerCollection.Name)]
public class DocsHttpTests
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
        await response.ShouldReturnSuccessWithContent("Pennington");
    }

    [Fact(Skip = "Docs content restructuring in progress")]
    public async Task ContentPage_ReturnsSuccess_WithTitle()
    {
        var response = await _client.GetAsync("/getting-started/creating-first-site/", TestContext.Current.CancellationToken);
        await response.ShouldReturnSuccessWithContent("Creating Your First Site");
    }

    [Fact(Skip = "Docs content restructuring in progress")]
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
        var response = await _client.GetAsync("/_content/Pennington.UI/scripts.js", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SpaEngineJs_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/_content/Pennington.UI/spa-engine.js", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ContentPage_EmitsSpaRegionMarkup()
    {
        // SPA navigation parses the rendered page and swaps elements marked
        // data-spa-region. The DocSite layout must emit at least the content
        // region for that contract to work.
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("data-spa-region=\"content\"");
    }

    [Fact]
    public async Task StaticAsset_Favicon_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/favicon.ico", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonExistentPage_Returns404_WithNotFoundBody()
    {
        // DocSite pages signal Pennington.NotFound when ContentResolver returns
        // null; NotFoundStatusProcessor flips the response to 404 after every
        // other rewriter has run, so the body still ships with localized chrome.
        var response = await _client.GetAsync("/this-does-not-exist/", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
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
        content.ShouldContain("""rel="preload" href="/fonts/lexend.woff2" as="font" type="font/woff2" crossorigin""");
        content.ShouldContain("""rel="preload" href="/fonts/noto-sans.woff2" as="font" type="font/woff2" crossorigin""");
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

    [Theory(Skip = "Docs content restructuring in progress")]
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
