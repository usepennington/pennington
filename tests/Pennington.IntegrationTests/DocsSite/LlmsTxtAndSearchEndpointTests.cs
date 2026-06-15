namespace Pennington.IntegrationTests.DocsSite;

using System.Text;
using System.Text.Json;
using AngleSharp;
using Infrastructure;
using LlmsTxt;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Regression tests for the bug where llms.txt and the search index were
/// generated from pre-render markdown instead of the post-pipeline HTML.
/// Runs against TestServer via <see cref="DocsWebApplicationFactory"/> — the
/// in-process dispatcher routes self-fetches through the same pipeline that
/// serves browser requests, so the assertions below reflect post-pipeline HTML.
/// </summary>
[Collection(DocsTestServerCollection.Name)]
public class LlmsTxtAndSearchEndpointTests
{
    private readonly DocsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LlmsTxtAndSearchEndpointTests(DocsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LlmsTxt_ReturnsIndexWithEntries()
    {
        var response = await _client.GetAsync("/llms.txt", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("# ");                // site title heading
        content.ShouldContain("/_llms/");           // at least one stripped-md entry reference
        // The docs site configures an absolute CanonicalBaseUrl, so sidecar links are
        // emitted as absolute URLs. The assertion below rejects the bare-relative form
        // (no origin, no scheme) which would be unusable for LLMs that fetch /llms.txt
        // and try to resolve links against the origin.
        content.ShouldNotContain("](_llms/");
    }

    [Fact]
    public async Task LlmsTxt_MarkdownFiles_ContainPostPipelineContent()
    {
        // _llms/*.md files are emitted at static-build time, not served as live
        // endpoints, so we fetch them directly from the service.
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
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
    public async Task SearchIndex_EntrypointHasDocumentTable()
    {
        var response = await _client.GetAsync("/search/en/index.json", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
        var docs = doc.RootElement.GetProperty("docs");
        docs.ValueKind.ShouldBe(JsonValueKind.Array);
        docs.GetArrayLength().ShouldBeGreaterThan(0);

        // BM25 stats and at least one shard must be present for the client to query.
        doc.RootElement.GetProperty("n").GetInt32().ShouldBeGreaterThan(0);
        doc.RootElement.GetProperty("shards").GetArrayLength().ShouldBeGreaterThan(0);

        // Every document-table row should have the required fields populated.
        foreach (var element in docs.EnumerateArray())
        {
            element.GetProperty("t").GetString().ShouldNotBeNullOrEmpty();
            element.GetProperty("u").GetString().ShouldNotBeNullOrEmpty();
            // Records are heading-level: every row carries a (possibly empty) breadcrumb trail.
            element.TryGetProperty("c", out var crumbs).ShouldBeTrue();
            crumbs.ValueKind.ShouldBe(JsonValueKind.Array);
        }

        // At least one row deep-links to a heading anchor, proving heading-level indexing.
        docs.EnumerateArray()
            .Any(d => (d.GetProperty("u").GetString() ?? "").Contains('#'))
            .ShouldBeTrue("expected at least one heading-level record with a #anchor URL");
    }

    [Fact]
    public async Task SearchIndex_ApiReferencePages_CarryLowPrefixPriority()
    {
        // AddApiReference registers its /reference/api/ tree at SearchPriority 3 (the default) via
        // SearchIndexOptions.PrefixPriorities, so every generated API page row carries p=3 while
        // same-area prose under /reference/ keeps its (higher) area priority. This proves the
        // prefix-priority override flows all the way into the emitted index.
        var json = await _client.GetStringAsync("/search/en/index.json", TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        var docs = doc.RootElement.GetProperty("docs");

        var apiPriorities = new List<int>();
        var proseReferencePriorities = new List<int>();
        foreach (var entry in docs.EnumerateArray())
        {
            var u = entry.GetProperty("u").GetString() ?? "";
            var p = entry.GetProperty("p").GetInt32();
            if (u.StartsWith("/reference/api/", StringComparison.Ordinal))
            {
                apiPriorities.Add(p);
            }
            else if (u.StartsWith("/reference/", StringComparison.Ordinal))
            {
                proseReferencePriorities.Add(p);
            }
        }

        apiPriorities.ShouldNotBeEmpty("expected indexed /reference/api/ pages");
        apiPriorities.ShouldAllBe(p => p == 3);

        // Same content area (reference), but hand-written prose keeps its area priority — strictly
        // above the API drop, so an article outranks a generated reference page on a tie.
        proseReferencePriorities.ShouldNotBeEmpty("expected indexed /reference/ prose pages");
        proseReferencePriorities.ShouldAllBe(p => p > 3);
    }

    [Fact]
    public async Task SearchFragments_HaveNonEmptyBodyText()
    {
        // Bodies live in per-page fragments (f-{docId}.json), fetched lazily by the
        // client. Verifies the selector resolved and StripHtml produced meaningful
        // text after the full pipeline ran (Razor pages included).
        var json = await _client.GetStringAsync("/search/en/index.json", TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        var count = doc.RootElement.GetProperty("docs").GetArrayLength();

        var nonEmpty = 0;
        for (var i = 0; i < count; i++)
        {
            var frag = await _client.GetStringAsync($"/search/en/f-{i}.json", TestContext.Current.CancellationToken);
            using var fd = JsonDocument.Parse(frag);
            if (!string.IsNullOrWhiteSpace(fd.RootElement.GetProperty("body").GetString()))
            {
                nonEmpty++;
            }
        }

        nonEmpty.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task SearchFragments_ExcludeCodeBlockText_ByDefault()
    {
        // Pick any indexed doc whose rendered page has a <pre> block, then verify
        // a distinctive token from inside that <pre> does not appear in the fragment
        // body for the same doc. The document-table order is the fragment id.
        var json = await _client.GetStringAsync("/search/en/index.json", TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        var docs = doc.RootElement.GetProperty("docs");

        // Records are heading-level, so a page maps to several fragments (its sections). Group
        // doc ids by page URL (strip the #anchor) so we can check every section of a page.
        var sectionIdsByPage = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        var id = -1;
        foreach (var entry in docs.EnumerateArray())
        {
            id++;
            var u = entry.GetProperty("u").GetString();
            if (string.IsNullOrEmpty(u))
            {
                continue;
            }

            var pageUrl = u.Split('#', 2)[0];
            (sectionIdsByPage.TryGetValue(pageUrl, out var ids) ? ids : sectionIdsByPage[pageUrl] = []).Add(id);
        }

        var context = BrowsingContext.New(Configuration.Default);
        string? codeOnlyToken = null;
        var pageSectionBodies = new List<string>();

        foreach (var (pageUrl, ids) in sectionIdsByPage)
        {
            var html = await _client.GetStringAsync(pageUrl, TestContext.Current.CancellationToken);
            var page = await context.OpenAsync(req => req.Content(html), TestContext.Current.CancellationToken);
            var pre = page.QuerySelector("#main-content pre") ?? page.QuerySelector("pre");
            if (pre is null)
            {
                continue;
            }

            // Find a token that appears inside <pre> but NOT anywhere else on the page.
            var preText = pre.TextContent;
            pre.Remove();
            var pageTextWithoutPre = page.Body?.TextContent ?? "";

            var candidate = preText
                .Split([' ', '\n', '\r', '\t', '(', ')', '{', '}', '[', ']', ';', ','], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(t => t.Length >= 6 && !pageTextWithoutPre.Contains(t, StringComparison.Ordinal));
            if (candidate is null)
            {
                continue;
            }

            // Collect the body of every section fragment belonging to this page.
            foreach (var sectionId in ids)
            {
                var frag = await _client.GetStringAsync($"/search/en/f-{sectionId}.json", TestContext.Current.CancellationToken);
                using var fd = JsonDocument.Parse(frag);
                pageSectionBodies.Add(fd.RootElement.GetProperty("body").GetString() ?? "");
            }

            codeOnlyToken = candidate;
            break;
        }

        codeOnlyToken.ShouldNotBeNull("expected at least one docs page with a <pre> block containing a code-only token");
        pageSectionBodies.ShouldNotBeEmpty();
        pageSectionBodies.ShouldAllBe(body => !body.Contains(codeOnlyToken!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task LlmsTxt_FrontDoor_StaysCompact()
    {
        // The /reference/ subtree (declared via _meta.yml) and the /reference/api/
        // subtree (declared programmatically by AddApiReference) split the densest
        // area out of the front door. The threshold accounts for the docs site's
        // absolute CanonicalBaseUrl, which adds ~50 bytes per entry vs. root-relative.
        var content = await _client.GetStringAsync("/llms.txt", TestContext.Current.CancellationToken);
        content.Length.ShouldBeLessThan(32_000,
            $"front door is {content.Length} bytes; the subtree splits should keep it under 32KB");
    }

    [Fact]
    public async Task LlmsTxt_FrontDoor_DoesNotInlineSubtreeLeafSidecars()
    {
        // After the subtree split, the front door only contains a See-also pointer to
        // /reference/llms.txt; individual /reference/.../*.md sidecar links must live
        // inside the subtree file, not the front door.
        var content = await _client.GetStringAsync("/llms.txt", TestContext.Current.CancellationToken);

        content.ShouldNotContain("/_llms/reference/");
        content.ShouldContain("/reference/llms.txt");
    }

    [Fact]
    public async Task LlmsTxt_FrontDoor_EmbedsMetadataBlock()
    {
        var content = await _client.GetStringAsync("/llms.txt", TestContext.Current.CancellationToken);

        // Front-door identity: site origin, self-canonical, generation timestamp,
        // and the Pennington version for tooling that wants to disambiguate.
        content.ShouldContain("site:");
        content.ShouldContain("canonical:");
        content.ShouldContain("generated:");
        content.ShouldContain("penningtonVersion:");

        // The version should match the LlmsTxtService assembly's informational version
        // (MinVer-populated in Directory.Build.props), with MinVer's "+<sha>" build
        // metadata trimmed so the value matches the published NuGet PackageVersion.
        var attr = typeof(LlmsTxtService).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .Cast<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault();
        var raw = attr?.InformationalVersion ?? "unknown";
        var plus = raw.IndexOf('+');
        var expected = plus >= 0 ? raw[..plus] : raw;
        content.ShouldContain($"penningtonVersion: {expected}");
    }

    [Fact]
    public async Task LlmsTxt_FrontDoor_HasMapBlockListingSubtrees()
    {
        var content = await _client.GetStringAsync("/llms.txt", TestContext.Current.CancellationToken);

        // Map block replaces See-also: same purpose (point at subtree splits) but with
        // entry counts and token estimates so a budget-aware client can plan its fetches.
        content.ShouldContain("## Map");
        // The docs site declares /reference/ as a subtree via _meta.yml; the Map points
        // at it with the full canonical URL, not a root-relative path.
        content.ShouldContain("](https://usepennington.github.io/pennington/reference/llms.txt)");
        // The blog is surfaced as a subtree by BlogContentService even though its posts
        // are SearchOnly and absent from the navigation tree.
        content.ShouldContain("](https://usepennington.github.io/pennington/blog/llms.txt)");
        // Entries-and-tokens parenthetical attached to each subtree entry.
        content.ShouldContain("entries,");
        content.ShouldContain("tokens)");
    }

    [Fact]
    public async Task ReferenceSubtree_LlmsTxt_ContainsReferenceEntries()
    {
        // The subtree's llms.txt is emitted as a static-build artifact alongside per-page
        // sidecars; pull it directly from the service rather than the live HTTP endpoint.
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var subtreeFiles = await llmsService.GetSubtreeFilesAsync();

        var refFile = subtreeFiles.FirstOrDefault(f =>
            f.OutputPath.Value.Equals("reference/llms.txt", StringComparison.OrdinalIgnoreCase));

        refFile.ShouldNotBeNull("expected /reference/llms.txt subtree file");
        var text = Encoding.UTF8.GetString(refFile!.Content);
        text.ShouldContain("# Reference");
        // Should contain at least one entry linked to its sidecar markdown.
        text.ShouldContain("/_llms/reference/");
    }

    [Fact]
    public async Task Sidecar_HasRichYamlFrontMatter()
    {
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        files.Count.ShouldBeGreaterThan(0);

        foreach (var file in files)
        {
            var text = Encoding.UTF8.GetString(file.Content);
            var firstLine = text.Split('\n', 2)[0];
            text.StartsWith("---", StringComparison.Ordinal).ShouldBeTrue(
                $"sidecar {file.OutputPath.Value} should start with YAML frontmatter; got: {firstLine}");

            // Required fields per the LLM-output spec: identifies the page,
            // pins it to a canonical URL, and gives a budget-aware client the
            // hash + token estimate it needs.
            text.ShouldContain("title:");
            text.ShouldContain("canonical_url:");
            text.ShouldContain("sidecar_url:");
            text.ShouldContain("content_hash: sha256:");
            text.ShouldContain("tokens:");

            var headerEnd = text.IndexOf("\n---", 4, StringComparison.Ordinal);
            headerEnd.ShouldBeGreaterThan(0, $"sidecar {file.OutputPath.Value} has unterminated YAML header");
        }
    }

    [Fact]
    public async Task DocsPage_BodyCarriesRobotsOnlyCueToLlmsTxt()
    {
        // <link rel="alternate" type="text/markdown"> in <head> is stripped by
        // WebFetch-style extractors. App.razor mirrors the same hint inside
        // <body> as a .robots-only paragraph, which is display:none for humans
        // but flows through the extractor — verified empirically against
        // Claude WebFetch on the deployed API reference pages, where other
        // .robots-only content survives. Both gates share the LlmsTxtOptions
        // DI check, so when one is registered the other must render too.
        var html = await _client.GetStringAsync("/", TestContext.Current.CancellationToken);

        var context = BrowsingContext.New(Configuration.Default);
        var page = await context.OpenAsync(req => req.Content(html), TestContext.Current.CancellationToken);

        var cue = page.QuerySelector("body > p.robots-only");
        cue.ShouldNotBeNull("expected a .robots-only cue paragraph at the top of <body>");
        cue!.TextContent.ShouldContain("/llms.txt");
        cue.QuerySelector("a[href='/llms.txt']").ShouldNotBeNull(
            "the cue should link to /llms.txt so an extractor sees the path verbatim");
    }

    [Fact]
    public async Task LlmsTxt_MarkdownFiles_RewriteInternalLinksToStrippedMarkdown()
    {
        // Links inside a stripped _llms/*.md that point to another indexed page
        // should be rewritten to _llms/{path}.md so an LLM following them stays
        // inside the stripped-markdown corpus instead of jumping back into
        // HTML with all the chrome.
        var llmsService = _factory.Services.GetRequiredService<LlmsTxtService>();
        var files = await llmsService.GetMarkdownFilesAsync();

        // Count rewritten internal links across all files. Match URLs of any form
        // (absolute or root-relative) that point at the _llms sidecar tree.
        var linkRegex = new System.Text.RegularExpressions.Regex(@"\[[^\]]*\]\(([^)]*?/_llms/[^)]+\.md[^)]*)\)");

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