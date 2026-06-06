using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Head;
using Pennington.Infrastructure;
using Pennington.Routing;
using Pennington.StructuredData;

namespace Pennington.Tests.StructuredData;

/// <summary>
/// DOM-level tests for <see cref="StructuredDataHeadContributor"/>, driven through the real
/// <see cref="HeadCompositionHtmlRewriter"/> + <see cref="HtmlResponseRewritingProcessor"/> so the
/// contributor resolves the record, builds the canonical URL, and injects the JSON-LD exactly as it
/// does in the response pipeline.
/// </summary>
public class StructuredDataHeadContributorTests
{
    // A self-contained JsonLdEntity subclass so the test needn't depend on a template's concrete types.
    private sealed record TestJobPosting : JsonLdEntity
    {
        [JsonPropertyName("@type")]
        public override string Type => "JobPosting";

        [JsonPropertyName("title")]
        public required string Title { get; init; }

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("author")]
        public string? Author { get; init; }
    }

    private sealed record JobFrontMatter : IFrontMatter, IHasStructuredData
    {
        public string Title { get; init; } = "";

        public IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context)
        {
            yield return new TestJobPosting
            {
                Title = Title,
                Url = context.CanonicalUrl,
                Author = string.IsNullOrEmpty(context.FallbackAuthorName) ? null : context.FallbackAuthorName,
            };
        }
    }

    private sealed record PlainFrontMatter : IFrontMatter
    {
        public string Title { get; init; } = "";
    }

    private static ContentRecord Record(string url, IFrontMatter fm) =>
        new(ContentRouteFactory.FromUrl(new UrlPath(url)), fm);

    private static async Task<string> Render(
        string requestPath,
        ContentRecordRegistry registry,
        string? canonicalBaseUrl = "https://example.com",
        string? authorName = null)
    {
        var options = new PenningtonOptions
        {
            CanonicalBaseUrl = canonicalBaseUrl,
            StructuredDataAuthorName = authorName,
        };
        var processor = new HtmlResponseRewritingProcessor(
            [new HeadCompositionHtmlRewriter([new StructuredDataHeadContributor(options)], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = requestPath;
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync("<html><head></head><body></body></html>", ctx);
    }

    [Fact]
    public async Task EmitsJsonLd_ForRecordWithStructuredData()
    {
        var registry = new ContentRecordRegistry([Record("/jobs/senior/", new JobFrontMatter { Title = "Senior Engineer" })]);

        var html = await Render("/jobs/senior/", registry);

        html.ShouldContain("application/ld+json");
        html.ShouldContain("\"title\":\"Senior Engineer\"");
        html.ShouldContain("https://example.com/jobs/senior/");
    }

    [Fact]
    public async Task EmitsNothing_WhenNoRecordMatchesTheRoute()
    {
        var registry = new ContentRecordRegistry([Record("/jobs/senior/", new JobFrontMatter { Title = "Senior Engineer" })]);

        var html = await Render("/jobs/junior/", registry);

        html.ShouldNotContain("application/ld+json");
    }

    [Fact]
    public async Task EmitsNothing_WhenRecordHasNoStructuredData()
    {
        var registry = new ContentRecordRegistry([Record("/about/", new PlainFrontMatter { Title = "About" })]);

        var html = await Render("/about/", registry);

        html.ShouldNotContain("application/ld+json");
    }

    [Fact]
    public async Task EmitsNothing_WhenCanonicalBaseUrlIsUnset()
    {
        var registry = new ContentRecordRegistry([Record("/jobs/senior/", new JobFrontMatter { Title = "Senior Engineer" })]);

        var html = await Render("/jobs/senior/", registry, canonicalBaseUrl: null);

        html.ShouldNotContain("application/ld+json");
    }

    [Fact]
    public async Task UsesSiteAuthorFallback_WhenRecordNamesNone()
    {
        var registry = new ContentRecordRegistry([Record("/jobs/senior/", new JobFrontMatter { Title = "Senior Engineer" })]);

        var html = await Render("/jobs/senior/", registry, authorName: "Acme Editorial");

        html.ShouldContain("\"author\":\"Acme Editorial\"");
    }
}
