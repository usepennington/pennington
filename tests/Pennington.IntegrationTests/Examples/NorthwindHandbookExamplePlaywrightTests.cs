namespace Pennington.IntegrationTests.Examples;

using Microsoft.Playwright;
using Pennington.IntegrationTests.Infrastructure;

public class NorthwindHandbookExamplePlaywrightTests : IClassFixture<NorthwindHandbookExamplePlaywrightFixture>, IAsyncLifetime
{
    private readonly NorthwindHandbookExamplePlaywrightFixture _fixture;
    private IPage _page = null!;

    public NorthwindHandbookExamplePlaywrightTests(NorthwindHandbookExamplePlaywrightFixture fixture)
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
        body.ShouldContain("Northwind Engineering Handbook");
    }

    [Fact]
    public async Task StylesCss_IsServed()
    {
        var response = await _page.GotoAsync($"{_fixture.BaseUrl}/styles.css");
        response!.Status.ShouldBe(200);
    }

    [Fact]
    public async Task NonExistentPage_ShowsNotFound()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/does-not-exist");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Not found");
    }

    [Theory]
    [InlineData("/development/coding-standards", "Coding Standards")]
    [InlineData("/development/pr-process", "Pull Request Process")]
    [InlineData("/operations/deployment-checklist", "Deployment Checklist")]
    [InlineData("/operations/incident-response", "Incident Response")]
    public async Task ContentPages_RenderSuccessfully(string url, string expectedTitle)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var heading = await _page.Locator("h1").TextContentAsync();
        heading.ShouldNotBeNull();
        heading.ShouldContain(expectedTitle);
    }

    [Fact]
    public async Task CodingStandards_RendersCodeBlock()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/development/coding-standards");

        var codeBlocks = _page.Locator("pre code");
        var count = await codeBlocks.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task IncidentResponse_RendersTable()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/operations/incident-response");

        var tables = _page.Locator("table");
        var count = await tables.CountAsync();
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task DeploymentChecklist_RendersCheckboxes()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/operations/deployment-checklist");

        var prose = _page.Locator(".prose");
        await Assertions.Expect(prose).ToBeVisibleAsync();

        var content = await prose.TextContentAsync();
        content.ShouldNotBeNull();
        content.ShouldContain("All tests pass in CI");
    }

    [Fact]
    public async Task Sidebar_ShowsNavigationSections()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/development/coding-standards");

        var nav = _page.Locator("nav").First;
        var navContent = await nav.TextContentAsync();
        navContent.ShouldNotBeNull();
        navContent.ShouldContain("Development");
        navContent.ShouldContain("Operations");
    }

    [Theory]
    [InlineData("/changelog/v2-1-0", "v2.1.0")]
    [InlineData("/changelog/v2-0-1", "v2.0.1")]
    [InlineData("/changelog/v2-0-0", "v2.0.0")]
    public async Task ChangelogPages_RenderSuccessfully(string url, string expectedVersion)
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}{url}");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain(expectedVersion);
    }

    [Fact]
    public async Task ChangelogV210_ShowsBreakingContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/changelog/v2-1-0");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("Auth token format changed");
    }

    [Fact]
    public async Task ChangelogV201_ShowsBugfixContent()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/changelog/v2-0-1");

        var body = await _page.Locator("body").TextContentAsync();
        body.ShouldNotBeNull();
        body.ShouldContain("CLI crash on empty input");
    }
}
