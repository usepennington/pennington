using Pennington.Feeds;
using Pennington.FrontMatter;
using Pennington.Routing;

namespace Pennington.Tests.Feeds;

public class SitemapBuilderTests
{
    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale
    };

    private record TestFrontMatter : IFrontMatter, IDraftable, ISectionable, IDateable, IDescribable, IRedirectable
    {
        public string Title { get; init; } = "Test";
        public bool IsDraft { get; init; }
        public string? Section { get; init; }
        public DateTime? Date { get; init; }
        public string? Description { get; init; }
        public string? RedirectUrl { get; init; }
    }

    private readonly SitemapBuilder _builder = new(new UrlPath("https://example.com"));

    [Fact]
    public void Build_CreatesEntriesWithAbsoluteUrls()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/about"), new TestFrontMatter { Title = "About" }),
            new(MakeRoute("/contact"), new TestFrontMatter { Title = "Contact" }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(2);
        entries[0].Url.Value.ShouldBe("https://example.com/about/");
        entries[1].Url.Value.ShouldBe("https://example.com/contact/");
    }

    [Fact]
    public void Build_ExcludesDrafts()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/published"), new TestFrontMatter { Title = "Published" }),
            new(MakeRoute("/draft"), new TestFrontMatter { Title = "Draft", IsDraft = true }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/published/");
    }

    [Fact]
    public void Build_ExcludesRedirects()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/canonical"), new TestFrontMatter { Title = "Canonical" }),
            new(MakeRoute("/legacy"), new TestFrontMatter { Title = "Legacy", RedirectUrl = "/canonical/" }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/canonical/");
    }

    [Fact]
    public void Build_UsesDateFromIDateable()
    {
        var date = new DateTime(2026, 3, 15);
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/post"), new TestFrontMatter { Title = "Post", Date = date }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].LastModified.ShouldBe(date);
    }

    // Front matter with no capabilities
    private record MinimalFrontMatter(string Title) : IFrontMatter;

    [Fact]
    public void Build_NonDraftableFrontMatter_Included()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/page"), new MinimalFrontMatter("Minimal Page")),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/page/");
    }

    [Fact]
    public void Build_NonDateable_LastModifiedIsNull()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/page"), new MinimalFrontMatter("No Date")),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].LastModified.ShouldBeNull();
    }

    [Fact]
    public void Build_NullMetadata_IncludedWithNoLastModified()
    {
        // Simulates a programmatic-content candidate that has no front matter.
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/generated"), Metadata: null),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/generated/");
        entries[0].LastModified.ShouldBeNull();
    }

    [Fact]
    public void Build_LocaleInRoute_PreservedInUrl()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/fr/docs/intro", "fr"), new TestFrontMatter { Title = "Introduction" }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/fr/docs/intro/");
    }

    [Fact]
    public void Build_MultiplePagesWithDrafts_OnlyNonDraftsIncluded()
    {
        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/getting-started"), new TestFrontMatter { Title = "Getting Started" }),
            new(MakeRoute("/wip"), new TestFrontMatter { Title = "WIP", IsDraft = true }),
            new(MakeRoute("/about"), new MinimalFrontMatter("About")),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(2);
        var urls = entries.Select(e => e.Url.Value).ToList();
        urls.ShouldContain("https://example.com/getting-started/");
        urls.ShouldContain("https://example.com/about/");
    }
}
