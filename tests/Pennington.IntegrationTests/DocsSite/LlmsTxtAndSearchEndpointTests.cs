namespace Pennington.IntegrationTests.DocsSite;

using System.Text;
using System.Text.Json;
using AngleSharp;
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
        // Without a CanonicalBaseUrl configured, the fixture falls back to
        // OutputOptions.BaseUrl ("/"), so links are root-relative. An LLM that
        // fetches /llms.txt can resolve these against the origin.
        content.ShouldContain("](/_llms/");
        content.ShouldNotContain("](_llms/");       // bare relative form has no origin — unusable for LLMs
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
    public async Task SearchIndex_ExcludesCodeBlockText_ByDefault()
    {
        // Pick any indexed doc whose rendered page has a <pre> block, then verify
        // a distinctive token from inside that <pre> does not appear in the search
        // body for the same URL. Content-agnostic — walks pages until one qualifies.
        var json = await _fixture.Client.GetStringAsync("/search-index-en.json", TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);

        var context = BrowsingContext.New(Configuration.Default);
        string? codeOnlyToken = null;
        string? matchedBody = null;

        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            var url = entry.GetProperty("url").GetString();
            if (string.IsNullOrEmpty(url)) continue;

            var html = await _fixture.Client.GetStringAsync(url, TestContext.Current.CancellationToken);
            var page = await context.OpenAsync(req => req.Content(html), TestContext.Current.CancellationToken);
            var pre = page.QuerySelector("#main-content pre") ?? page.QuerySelector("pre");
            if (pre is null) continue;

            // Find a token that appears inside <pre> but NOT anywhere else on the page.
            var preText = pre.TextContent;
            pre.Remove();
            var pageTextWithoutPre = page.Body?.TextContent ?? "";

            var candidate = preText
                .Split([' ', '\n', '\r', '\t', '(', ')', '{', '}', '[', ']', ';', ','], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(t => t.Length >= 6 && !pageTextWithoutPre.Contains(t, StringComparison.Ordinal));
            if (candidate is null) continue;

            codeOnlyToken = candidate;
            matchedBody = entry.GetProperty("body").GetString();
            break;
        }

        codeOnlyToken.ShouldNotBeNull("expected at least one docs page with a <pre> block containing a code-only token");
        matchedBody.ShouldNotBeNull();
        matchedBody!.Contains(codeOnlyToken!, StringComparison.Ordinal).ShouldBeFalse(
            $"token '{codeOnlyToken}' appears only in <pre> on the source page, so it must not leak into the search body");
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

        // Count rewritten internal links across all files. Form: "/_llms/…md"
        // (root-relative; fixture has no canonical origin configured).
        var linkRegex = new System.Text.RegularExpressions.Regex(@"\[[^\]]*\]\((/_llms/[^)]+\.md[^)]*)\)");

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