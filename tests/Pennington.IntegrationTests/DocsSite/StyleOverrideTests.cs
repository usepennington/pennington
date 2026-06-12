namespace Pennington.IntegrationTests.DocsSite;

using AngleSharp.Html.Parser;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Cli;
using Pennington.DocSite;
using Pennington.MonorailCss;
using Pennington.UI.Styling;

/// <summary>
/// Docs host whose <see cref="StyleRegistry"/> registration is replaced with one carrying a
/// consumer override, standing in for a real host setting <c>DocSiteOptions.Styles</c>.
/// </summary>
public class StyleOverrideWebApplicationFactory : DocsWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
            services.AddTransient(_ => StyleRegistry.Create(
                DocSiteStyleSkin.Styles,
                new Dictionary<string, string> { [StyleKeys.TocLinkColor] = "text-base-700" },
                MonorailCssService.CreateClassMerger(new MonorailCssOptions()))));
    }
}

public class StyleOverrideTests : IClassFixture<StyleOverrideWebApplicationFactory>
{
    private readonly StyleOverrideWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StyleOverrideTests(StyleOverrideWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SidebarLinks_CarryMergedOverrideClasses()
    {
        // The 404 body ships with full chrome, so the sidebar renders even if this route
        // moves in a docs restructure — the assertions only need a MainLayout page.
        var html = await _client.GetStringAsync(
            "/explanation/core/content-pipeline/", TestContext.Current.CancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(
            html, TestContext.Current.CancellationToken);

        // Child-level links only (nested list): the override targets toc.link-color, while
        // root-level leaf links and section headers render other slots.
        var links = document.QuerySelectorAll("#nav-sidebar ul ul a[data-current]");
        links.Length.ShouldBeGreaterThan(0);

        foreach (var link in links)
        {
            var classes = link.ClassList.ToArray();
            // The override's color replaced the skin's...
            classes.ShouldContain("text-base-700");
            classes.ShouldNotContain("text-base-500");
            // ...while the skin's non-conflicting classes survived the merge.
            classes.ShouldContain("hover:bg-base-100");
            classes.ShouldContain("rounded-md");
        }
    }

    [Fact]
    public async Task StylesCss_ContainsOverriddenUtility()
    {
        var css = await _client.GetStringAsync("/styles.css", TestContext.Current.CancellationToken);

        css.ShouldContain(".text-base-700");
    }

    [Fact]
    public void DiagStylesCommand_IsRegistered()
    {
        _factory.Services.GetServices<IDiagCommand>().ShouldContain(c => c.Name == "styles");
    }

    [Fact]
    public void DocSiteSkin_UsesOnlyValidStyleKeys()
    {
        Should.NotThrow(() => StyleRegistry.Create(DocSiteStyleSkin.Styles));
    }
}
