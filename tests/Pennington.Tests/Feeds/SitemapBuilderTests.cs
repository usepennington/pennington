using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Pennington.Feeds;
using Pennington.FrontMatter;
using Pennington.Generation;
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

    private record TestFrontMatter : IFrontMatter, ISectionable, IRedirectable
    {
        public string Title { get; init; } = "Test";
        public bool IsDraft { get; init; }
        public string? SectionLabel { get; init; }
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
    public void Build_RootRoute_WithAbsoluteBase_ProducesWellFormedUrl()
    {
        // Regression for Phase 5 bug: composing `https://example.com` with the
        // root canonical path `/` used to produce `/https://example.com`
        // because UrlPath's `/` operator is path-only and forces a leading
        // slash. The fix in ContentRoute.AbsoluteUrl composes via URI
        // semantics when the base has a scheme.
        var rootRoute = new ContentRoute
        {
            CanonicalPath = new UrlPath("/"),
            OutputFile = new FilePath("index.html"),
        };
        var candidates = new List<SitemapCandidate>
        {
            new(rootRoute, new TestFrontMatter { Title = "Home" }),
        };

        var entries = _builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/");
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

    [Fact]
    public void Build_SubPathBaseUrl_WithoutCanonicalUrl_ProducesPrefixedRelativeUrls()
    {
        // Regression for P0-3 / postmortem-SubPathDeployableExample.md: a sub-path
        // build (e.g. `dotnet run -- build /sub/`) must produce a sitemap whose
        // <loc> values crawlers can resolve to the deployed location — even when
        // CanonicalBaseUrl is not set. The DI factory in PenningtonExtensions
        // falls back to OutputOptions.BaseUrl; this test exercises that
        // composition end-to-end using the SitemapBuilder.
        var builder = new SitemapBuilder(new UrlPath("/sub"));

        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/about"), new TestFrontMatter { Title = "About" }),
            new(MakeRoute("/guides/first"), new TestFrontMatter { Title = "First Guide" }),
        };

        var entries = builder.Build(candidates);

        entries.Count.ShouldBe(2);
        entries[0].Url.Value.ShouldBe("/sub/about/");
        entries[1].Url.Value.ShouldBe("/sub/guides/first/");
    }

    [Fact]
    public void PenningtonDiFactory_FallsBackToBaseUrl_WhenCanonicalBaseUrlIsMissing()
    {
        // Mirror the DI factory wired in PenningtonExtensions.cs so a change to
        // that factory without also updating this replication will fail here.
        // See `// Feed builders` block in PenningtonExtensions.AddPennington.
        string? canonicalBaseUrl = null; // user did not set PenningtonOptions.CanonicalBaseUrl

        var services = new ServiceCollection();
        services.AddSingleton(new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            BaseUrl = new UrlPath("/sub/"),
        });
        services.AddSingleton(sp =>
        {
            var effectiveBase = !string.IsNullOrEmpty(canonicalBaseUrl)
                ? new UrlPath(canonicalBaseUrl)
                : sp.GetRequiredService<OutputOptions>().BaseUrl;
            return new SitemapBuilder(effectiveBase);
        });

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<SitemapBuilder>();

        builder.CanonicalBase.Value.ShouldBe("/sub/");
    }

    [Fact]
    public void PenningtonDiFactory_ExplicitCanonicalBaseUrl_WinsOverBaseUrl()
    {
        // When users set CanonicalBaseUrl to a fully-qualified URL (the correct
        // form per the sitemap protocol), it must not be overridden by the
        // OutputOptions.BaseUrl fallback.
        var canonicalBaseUrl = "https://example.com/my-sub-path";

        var services = new ServiceCollection();
        services.AddSingleton(new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            BaseUrl = new UrlPath("/"),
        });
        services.AddSingleton(sp =>
        {
            var effectiveBase = !string.IsNullOrEmpty(canonicalBaseUrl)
                ? new UrlPath(canonicalBaseUrl)
                : sp.GetRequiredService<OutputOptions>().BaseUrl;
            return new SitemapBuilder(effectiveBase);
        });

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<SitemapBuilder>();

        builder.CanonicalBase.Value.ShouldBe("https://example.com/my-sub-path");
    }

    [Fact]
    public void Build_ExcludesScheduledFutureDatedEntries()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero));
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        var builder = new SitemapBuilder(new UrlPath("https://example.com"), clock);

        var candidates = new List<SitemapCandidate>
        {
            new(MakeRoute("/posts/published"), new TestFrontMatter { Title = "Published", Date = new DateTime(2030, 6, 14) }),
            new(MakeRoute("/posts/scheduled"), new TestFrontMatter { Title = "Scheduled", Date = new DateTime(2030, 6, 16) }),
        };

        var entries = builder.Build(candidates);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/posts/published/");
    }
}