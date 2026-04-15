namespace Pennington.IntegrationTests.DocsSite;

using System.Text;
using System.Text.Json;
using Infrastructure;
using LlmsTxt;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Regression tests for the bug where llms.txt and search-index.json were
/// generated from pre-render markdown instead of the post-pipeline HTML.
/// Hits a real Kestrel instance because the endpoints self-fetch pages via
/// HttpClient, which TestServer (WebApplicationFactory) cannot service.
/// </summary>
public class LlmsTxtAndSearchEndpointTests : IClassFixture<DocsRealServerFixture>
{
    private readonly DocsRealServerFixture _fixture;

    public LlmsTxtAndSearchEndpointTests(DocsRealServerFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task LlmsTxt_ReturnsIndexWithEntries()
    {
        var response = await _fixture.Client.GetAsync("/llms.txt", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("# ");                // site title heading
        content.ShouldContain("_llms/");            // at least one stripped-md entry reference
        content.ShouldContain("](_llms/");          // markdown-link to stripped md
    }

    [Fact]
    public async Task LlmsTxt_MarkdownFiles_ContainPostPipelineContent()
    {
        // _llms/*.md files are emitted at static-build time, not served as live
        // endpoints, so we fetch them directly from the service.
        var llmsService = _fixture.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        files.Count.ShouldBeGreaterThan(0);

        // Every file should have non-trivial content — proves the CSS selector
        // matched and HtmlToMarkdownConverter produced something useful.
        foreach (var file in files)
        {
            var text = Encoding.UTF8.GetString(file.Content);
            text.ShouldNotBeNullOrWhiteSpace();
            text.Length.ShouldBeGreaterThan(50);
        }
    }

    [Fact]
    public async Task SearchIndex_ReturnsJsonArrayOfDocuments()
    {
        var response = await _fixture.Client.GetAsync("/search-index-en.json", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().ShouldBeGreaterThan(0);

        // Every document should have the required fields populated.
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            element.GetProperty("title").GetString().ShouldNotBeNullOrEmpty();
            element.GetProperty("url").GetString().ShouldNotBeNullOrEmpty();
            element.TryGetProperty("body", out _).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SearchIndex_DocumentsHaveNonEmptyBodyText()
    {
        // Verifies the selector resolved and StripHtml produced meaningful text.
        // Before this fix, bodies came from StripHtml(IContentRenderer.Render(...)),
        // which produced empty bodies for Razor pages and missed code-block text
        // that only materializes after the full pipeline runs.
        var response = await _fixture.Client.GetAsync("/search-index-en.json", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);

        var nonEmpty = doc.RootElement.EnumerateArray()
            .Count(el => !string.IsNullOrWhiteSpace(el.GetProperty("body").GetString()));

        nonEmpty.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task LlmsTxt_MarkdownFiles_RewriteInternalLinksToStrippedMarkdown()
    {
        // Links inside a stripped _llms/*.md that point to another indexed page
        // should be rewritten to _llms/{path}.md so an LLM following them stays
        // inside the stripped-markdown corpus instead of jumping back into
        // HTML with all the chrome.
        var llmsService = _fixture.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        // Count rewritten internal links across all files. Form: "_llms/…md".
        var linkRegex = new System.Text.RegularExpressions.Regex(@"\[[^\]]*\]\((_llms/[^)]+\.md[^)]*)\)");

        var rewrittenInternalLinks = 0;
        foreach (var file in files)
        {
            var text = Encoding.UTF8.GetString(file.Content);
            rewrittenInternalLinks += linkRegex.Matches(text).Count;
        }

        rewrittenInternalLinks.ShouldBeGreaterThan(0,
            "at least one stripped markdown file should contain a rewritten _llms/*.md link");
    }
}