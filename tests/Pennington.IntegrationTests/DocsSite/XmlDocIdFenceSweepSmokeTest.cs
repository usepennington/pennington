namespace Pennington.IntegrationTests.DocsSite;

using Infrastructure;

/// <summary>
/// Belt-and-suspenders smoke test for the 2026-04-13 fence-syntax migration:
/// confirms a sweep-converted page's <c>csharp:xmldocid</c> fence actually resolves and
/// renders non-empty source instead of the silent empty block the old attribute form produced.
/// </summary>
public class XmlDocIdFenceSweepSmokeTest : IClassFixture<DocsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public XmlDocIdFenceSweepSmokeTest(DocsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(Skip = "reference/options/pennington-options page is in draft; un-skip when it's published")]
    public async Task PenningtonOptionsPage_DeclarationFence_Renders_Real_Source()
    {
        var response = await _client.GetAsync("/reference/options/pennington-options/", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.ShouldNotContain("Page not found");

        // A working xmldocid fence renders classified source between <pre><code...> tags.
        // The broken attribute form used to render <pre><code class="...highlighted"></code></pre> — empty.
        content.ShouldContain("PenningtonOptions");
        content.ShouldContain("SiteTitle");
        content.ShouldContain("ContentRootPath");
    }
}