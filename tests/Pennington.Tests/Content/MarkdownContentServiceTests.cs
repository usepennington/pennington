using System.Collections.Immutable;
using System.IO;
using Microsoft.Extensions.Time.Testing;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Content;

public class MarkdownContentServiceTests
{
    private static readonly LocalizationOptions DefaultLocalization = new();

    private MarkdownContentService<DocFrontMatter> CreateTestService(
        MockFileSystem fs,
        string? section = "Documentation",
        UrlPath? basePageUrl = null,
        LocalizationOptions? localization = null,
        System.TimeProvider? clock = null,
        bool reserveNotFound = false)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = basePageUrl ?? new UrlPath("/docs"),
            SectionLabel = section,
            ReserveNotFoundPage = reserveNotFound
        };

        return new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, localization ?? DefaultLocalization, clock);
    }

    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        foreach (var (path, content) in files)
        {
            var fullPath = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(fullPath);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(fullPath, content);
        }
        return fs;
    }

    [Fact]
    public async Task DiscoverAsync_FindsMarkdownFiles()
    {
        var fs = CreateFs(
            ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"),
            ("advanced.md", "---\ntitle: Advanced\n---\n# Advanced"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);
        foreach (var item in items)
        {
            (item.Source is FileSource).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task DiscoverAsync_BuildsCorrectRoutes()
    {
        var fs = CreateFs(
            ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/getting-started/");
    }

    [Fact]
    public async Task DiscoverAsync_SourcePointsToFile()
    {
        var fs = CreateFs(
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        (items[0].Source is FileSource).ShouldBeTrue();
        var path = items[0].Source switch
        {
            FileSource m => m.Path.Value,
            _ => ""
        };
        path.ShouldEndWith("intro.md");
    }

    [Fact]
    public async Task DiscoverAsync_ReservedNotFoundPage_ExcludedWhenFlagSet()
    {
        var fs = CreateFs(
            ("404.md", "---\ntitle: Not Found\n---\nMissing."),
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"));
        var service = CreateTestService(fs, reserveNotFound: true);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/intro/");
        items.ShouldNotContain(i => i.Route.CanonicalPath.Value.Contains("404"));
    }

    [Fact]
    public async Task DiscoverAsync_NotFoundPage_IncludedWhenFlagUnset()
    {
        var fs = CreateFs(
            ("404.md", "---\ntitle: Not Found\n---\nMissing."));
        var service = CreateTestService(fs); // reserveNotFound defaults to false (bare-host behavior)

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/404/");
    }

    [Fact]
    public async Task DiscoverAsync_NestedNotFoundPage_NotReserved()
    {
        var fs = CreateFs(
            ("404.md", "---\ntitle: Not Found\n---\nMissing."),
            ("guides/404.md", "---\ntitle: Guides 404\n---\nNested."));
        var service = CreateTestService(fs, reserveNotFound: true);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        // Only the content-root 404.md is reserved — a nested guides/404.md stays a normal page.
        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/guides/404/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_ReservedNotFoundPage_Excluded()
    {
        var fs = CreateFs(
            ("404.md", "---\ntitle: Not Found\n---\nMissing."),
            ("intro.md", "---\ntitle: Intro\norder: 1\n---\n# Intro"));
        var service = CreateTestService(fs, reserveNotFound: true);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Intro");
    }

    [Fact]
    public async Task DiscoverAsync_MultiLocale_ReservedNotFoundPage_Excluded()
    {
        var fs = CreateMultiLocaleFs(
            ("404.md", "---\ntitle: Not Found\n---\nMissing."),
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"),
            ("fr/404.md", "---\ntitle: Introuvable\n---\nManquant."),
            ("fr/intro.md", "---\ntitle: Intro FR\n---\n# Intro FR"));
        var service = CreateTestService(
            fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale(), reserveNotFound: true);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldNotContain(i => i.Route.CanonicalPath.Value.Contains("404"));
        items.ShouldContain(i => i.Route.CanonicalPath.Value == "/intro/");
        items.ShouldContain(i => i.Route.CanonicalPath.Value == "/fr/intro/");
    }

    [Fact]
    public async Task ParseContentAsync_YieldsTypedMetadataAndBody()
    {
        // Parsed with the service's own DocFrontMatter type, so order/sectionLabel
        // bind correctly — the whole point of routing llms.txt through this instead
        // of a single foreign parser.
        var fs = CreateFs(
            ("guide.md", "---\ntitle: Guide\norder: 42\nsectionLabel: Docs\n---\n# Guide\n\nBody text."));
        var service = CreateTestService(fs);

        var items = new List<ParsedItem>();
        await foreach (var item in service.ParseContentAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        var parsed = items[0];
        parsed.Metadata.Title.ShouldBe("Guide");
        var fm = parsed.Metadata.ShouldBeOfType<DocFrontMatter>();
        fm.Order.ShouldBe(42);
        fm.SectionLabel.ShouldBe("Docs");
        parsed.RawMarkdown.ShouldContain("Body text.");
    }

    [Fact]
    public async Task ParseContentAsync_SkipsDrafts()
    {
        var fs = CreateFs(
            ("published.md", "---\ntitle: Published\n---\n# Published"),
            ("draft.md", "---\ntitle: Draft\nisDraft: true\n---\n# Draft"));
        var service = CreateTestService(fs);

        var titles = new List<string>();
        await foreach (var item in service.ParseContentAsync())
        {
            titles.Add(item.Metadata.Title);
        }

        titles.ShouldBe(["Published"]);
    }

    [Fact]
    public async Task DiscoverAsync_AttachesParsedFrontMatter()
    {
        // DiscoverAsync parses each file with its own TFrontMatter type and
        // attaches the result to the DiscoveredItem, so downstream consumers
        // (SitemapService) read it instead of re-parsing with a possibly
        // mismatched parser.
        var fs = CreateFs(
            ("intro.md", "---\ntitle: Intro\norder: 3\n---\n# Intro"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Metadata.ShouldNotBeNull();
        items[0].Metadata!.Title.ShouldBe("Intro");
    }

    [Fact]
    public async Task DiscoverAsync_SkipsDrafts()
    {
        var fs = CreateFs(
            ("published.md", "---\ntitle: Published\nisDraft: false\n---\n# Published"),
            ("draft.md", "---\ntitle: Draft Post\nisDraft: true\n---\n# Draft"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/published/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_ReturnsEntries()
    {
        var fs = CreateFs(
            ("getting-started.md", "---\ntitle: Getting Started\norder: 1\n---\n# Getting Started"),
            ("advanced.md", "---\ntitle: Advanced Topics\norder: 5\n---\n# Advanced"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);

        var gettingStarted = entries.First(e => e.Title == "Getting Started");
        gettingStarted.Order.ShouldBe(1);
        gettingStarted.HierarchyParts.ShouldContain("docs");
        gettingStarted.HierarchyParts.ShouldContain("getting-started");

        var advanced = entries.First(e => e.Title == "Advanced Topics");
        advanced.Order.ShouldBe(5);
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SkipsDrafts()
    {
        var fs = CreateFs(
            ("published.md", "---\ntitle: Published\nisDraft: false\n---\n# Published"),
            ("draft.md", "---\ntitle: Draft Post\nisDraft: true\n---\n# Draft"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Published");
    }

    private static FakeTimeProvider ClockAt(DateTime localNow)
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(localNow, TimeSpan.Zero));
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        return clock;
    }

    private static MarkdownContentService<BlogFrontMatter> CreateBlogService(MockFileSystem fs, FakeTimeProvider clock)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = new UrlPath("/blog"),
        };
        return new MarkdownContentService<BlogFrontMatter>(options, new FrontMatterParser(), fs, DefaultLocalization, clock);
    }

    [Fact]
    public async Task DiscoverAsync_SkipsFutureDatedPages()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15, 12, 0, 0));
        var fs = CreateFs(
            ("live.md", "---\ntitle: Live\ndate: 2030-06-14\n---\n# Live"),
            ("scheduled.md", "---\ntitle: Scheduled\ndate: 2030-06-16\n---\n# Scheduled"));
        var service = CreateBlogService(fs, clock);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/blog/live/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SkipsFutureDatedPages()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15, 12, 0, 0));
        var fs = CreateFs(
            ("live.md", "---\ntitle: Live\ndate: 2030-06-14\n---\n# Live"),
            ("scheduled.md", "---\ntitle: Scheduled\ndate: 2030-06-16\n---\n# Scheduled"));
        var service = CreateBlogService(fs, clock);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Live");
    }

    [Fact]
    public async Task DiscoverAsync_IncludesScheduledPageAfterClockAdvancesPastIt()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15, 12, 0, 0));
        var fs = CreateFs(
            ("scheduled.md", "---\ntitle: Scheduled\ndate: 2030-06-16\n---\n# Scheduled"));
        var service = CreateBlogService(fs, clock);

        var beforeRelease = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            beforeRelease.Add(item);
        }
        beforeRelease.ShouldBeEmpty();

        clock.Advance(TimeSpan.FromDays(2));

        var afterRelease = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            afterRelease.Add(item);
        }
        afterRelease.Count.ShouldBe(1);
    }

    // Captures: a page with empty title (e.g. YAML typo in frontmatter type
    // that doesn't implement IRedirectable, so `redirectUrl:` is silently
    // ignored and no title remains) must not leak into the TOC. Otherwise
    // it produces a search-index entry with empty title + body, which the
    // validator flags as S.MISSING_FIELD.
    [Fact]
    public async Task GetContentTocEntriesAsync_SkipsEntriesWithEmptyTitle()
    {
        var fs = CreateFs(
            ("real.md", "---\ntitle: Real Page\n---\n# Real"),
            ("orphan.md", "---\nredirectUrl: /elsewhere\n---\n"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Real Page");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SkipsRedirects()
    {
        var fs = CreateFs(
            ("real.md", "---\ntitle: Real Page\n---\n# Real"),
            ("legacy.md", "---\ntitle: Legacy URL\nredirectUrl: /real/\n---\n"));
        var service = CreateRedirectableTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Real Page");
    }

    private record RedirectableFrontMatter : IFrontMatter, IRedirectable
    {
        public string Title { get; init; } = "";
        public bool IsDraft { get; init; }
        public string? RedirectUrl { get; init; }
    }

    private MarkdownContentService<RedirectableFrontMatter> CreateRedirectableTestService(MockFileSystem fs)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = new UrlPath("/docs"),
            SectionLabel = "Documentation"
        };

        return new MarkdownContentService<RedirectableFrontMatter>(
                    options, new FrontMatterParser(), fs, DefaultLocalization);
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromOptions()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: A Page\n---\n# A Page"));
        var service = CreateTestService(fs, section: "Guides");

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].SectionLabel.ShouldBe("Guides");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromFrontMatter()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: A Page\nsectionLabel: OverriddenSection\n---\n# A Page"));
        var service = CreateTestService(fs, section: "DefaultSection");

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].SectionLabel.ShouldBe("OverriddenSection");
    }

    [Fact]
    public async Task GetCrossReferencesAsync_ReturnsXrefs()
    {
        var fs = CreateFs(
            ("api.md", "---\ntitle: API Reference\nuid: api-ref\n---\n# API"),
            ("guide.md", "---\ntitle: User Guide\nuid: user-guide\n---\n# Guide"));
        var service = CreateTestService(fs);

        var xrefs = await service.GetCrossReferencesAsync();

        xrefs.Count.ShouldBe(2);

        var apiRef = xrefs.First(x => x.Uid == "api-ref");
        apiRef.Title.ShouldBe("API Reference");

        var guideRef = xrefs.First(x => x.Uid == "user-guide");
        guideRef.Title.ShouldBe("User Guide");
    }

    [Fact]
    public async Task GetCrossReferencesAsync_SkipsFilesWithoutUid()
    {
        var fs = CreateFs(
            ("with-uid.md", "---\ntitle: Has UID\nuid: my-uid\n---\n# Has UID"),
            ("no-uid.md", "---\ntitle: No UID\n---\n# No UID"));
        var service = CreateTestService(fs);

        var xrefs = await service.GetCrossReferencesAsync();

        xrefs.Count.ShouldBe(1);
        xrefs[0].Uid.ShouldBe("my-uid");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludesMarkdown()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: Page\n---\n# Page"),
            ("images/logo.png", "fake-png-data"));
        var service = CreateTestService(fs);

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(1);
        toCopy[0].OutputPath.Value.ShouldBe("docs/images/logo.png");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludesAllExcludedExtensions()
    {
        var fs = CreateFs(
            ("page.md", "# Markdown"),
            ("page.mdx", "# MDX"),
            ("page.razor", "<div>Razor</div>"),
            ("data.yml", "key: value"),
            ("data.yaml", "key: value"),
            ("image.jpg", "fake-jpg-data"),
            ("script.js", "console.log('hello');"));
        var service = CreateTestService(fs);

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(2);
        var outputPaths = toCopy.Select(c => c.OutputPath.Value).ToList();
        outputPaths.ShouldContain("docs/image.jpg");
        outputPaths.ShouldContain("docs/script.js");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludesRootLlmsHeaderInput()
    {
        // A source mounted at "/" shares the content root, so its asset copy would
        // otherwise publish the llms.txt front-door input verbatim at /llms-header.txt.
        var fs = CreateFs(
            ("llms-header.txt", "# Site"),
            ("notes.txt", "real asset"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"));

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Select(c => c.OutputPath.Value).ShouldBe(["notes.txt"]);
    }

    [Fact]
    public async Task EmptyDirectory_ReturnsEmptyResults()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldBeEmpty();

        var toc = await service.GetContentTocEntriesAsync();
        toc.ShouldBe(ImmutableList<ContentTocItem>.Empty);

        var xrefs = await service.GetCrossReferencesAsync();
        xrefs.ShouldBe(ImmutableList<CrossReference>.Empty);

        var toCopy = await service.GetContentToCopyAsync();
        toCopy.ShouldBe(ImmutableList<ContentToCopy>.Empty);
    }

    [Fact]
    public async Task NonExistentDirectory_ReturnsEmptyResults()
    {
        var fs = new MockFileSystem();
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/nonexistent"),
            BasePageUrl = new UrlPath("/docs"),
            SectionLabel = "Test"
        };
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, DefaultLocalization);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldBeEmpty();

        var toc = await service.GetContentTocEntriesAsync();
        toc.ShouldBe(ImmutableList<ContentTocItem>.Empty);

        var xrefs = await service.GetCrossReferencesAsync();
        xrefs.ShouldBe(ImmutableList<CrossReference>.Empty);

        var toCopy = await service.GetContentToCopyAsync();
        toCopy.ShouldBe(ImmutableList<ContentToCopy>.Empty);
    }

    [Fact]
    public void DefaultSectionLabel_ReturnsEmptyWhenNull()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var service = CreateTestService(fs, section: null);
        service.DefaultSectionLabel.ShouldBe("");
    }

    [Fact]
    public async Task DiscoverAsync_SubdirectoryFiles()
    {
        var fs = CreateFs(
            ("guides/setup.md", "---\ntitle: Setup Guide\n---\n# Setup"),
            ("guides/advanced/config.md", "---\ntitle: Config\n---\n# Config"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldContain("/docs/guides/setup/");
        routes.ShouldContain("/docs/guides/advanced/config/");
    }

    [Fact]
    public async Task DiscoverAsync_IndexMd_MapsToBaseUrl()
    {
        var fs = CreateFs(
            ("index.md", "---\ntitle: Home\n---\n# Welcome"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_OrderableCapability_UsesOrder()
    {
        var fs = CreateFs(
            ("first.md", "---\ntitle: First Page\norder: 1\n---\n# First"),
            ("second.md", "---\ntitle: Second Page\norder: 2\n---\n# Second"),
            ("unordered.md", "---\ntitle: Unordered\n---\n# Unordered"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(3);
        var first = entries.First(e => e.Title == "First Page");
        var second = entries.First(e => e.Title == "Second Page");
        var unordered = entries.First(e => e.Title == "Unordered");

        first.Order.ShouldBe(1);
        second.Order.ShouldBe(2);
        unordered.Order.ShouldBe(int.MaxValue); // default for non-orderable
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_HierarchyParts_ReflectPathStructure()
    {
        var fs = CreateFs(
            ("api/auth/tokens.md", "---\ntitle: Tokens\n---\n# Tokens"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].HierarchyParts.ShouldBe(["docs", "api", "auth", "tokens"]);
    }

    // --- BlogFrontMatter tests ---

    private MarkdownContentService<BlogFrontMatter> CreateBlogService(
        MockFileSystem fs,
        string? section = null,
        UrlPath? basePageUrl = null)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = basePageUrl ?? new UrlPath("/blog"),
            SectionLabel = section
        };

        return new MarkdownContentService<BlogFrontMatter>(options, new FrontMatterParser(), fs, DefaultLocalization);
    }

    [Fact]
    public async Task BlogFrontMatter_DiscoverAsync_FindsPosts()
    {
        var fs = CreateFs(
            ("hello-world.md", "---\ntitle: Hello World\ndate: 2026-03-15\ntags:\n  - intro\n---\n# Hello World"));
        var service = CreateBlogService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/blog/hello-world/");
    }

    [Fact]
    public async Task BlogFrontMatter_DraftsSkippedInToc()
    {
        var fs = CreateFs(
            ("published.md", "---\ntitle: Published Post\ndate: 2026-03-15\nisDraft: false\n---\n# Published"),
            ("draft.md", "---\ntitle: Draft Post\ndate: 2026-04-01\nisDraft: true\n---\n# Draft"));
        var service = CreateBlogService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Published Post");
    }

    [Fact]
    public async Task BlogFrontMatter_CrossReferences_WithUid()
    {
        var fs = CreateFs(
            ("post-with-uid.md", "---\ntitle: Referenced Post\nuid: blog-post-1\ndate: 2026-03-15\n---\n# Post"),
            ("post-no-uid.md", "---\ntitle: No UID Post\ndate: 2026-03-20\n---\n# Another Post"));
        var service = CreateBlogService(fs);

        var xrefs = await service.GetCrossReferencesAsync();

        xrefs.Count.ShouldBe(1);
        xrefs[0].Uid.ShouldBe("blog-post-1");
        xrefs[0].Title.ShouldBe("Referenced Post");
    }

    [Fact]
    public async Task BlogFrontMatter_NonOrderable_DefaultsToMaxValue()
    {
        // BlogFrontMatter doesn't implement IOrderable
        var fs = CreateFs(
            ("post.md", "---\ntitle: A Blog Post\ndate: 2026-03-15\n---\n# Post"));
        var service = CreateBlogService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Order.ShouldBe(int.MaxValue);
    }

    [Fact]
    public async Task MultipleContentPaths_SameContentDirectory()
    {
        var fs = CreateFs(
            ("guide.md", "---\ntitle: User Guide\norder: 1\nuid: guide\n---\n# Guide"),
            ("faq.md", "---\ntitle: FAQ\norder: 2\n---\n# FAQ"));
        var service = CreateTestService(fs);

        // Verify all APIs work together
        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var toc = await service.GetContentTocEntriesAsync();
        var xrefs = await service.GetCrossReferencesAsync();
        var toCopy = await service.GetContentToCopyAsync();

        items.Count.ShouldBe(2);
        toc.Count.ShouldBe(2);
        xrefs.Count.ShouldBe(1); // only guide has uid
        toCopy.ShouldBeEmpty(); // only .md files present
    }

    [Fact]
    public void SearchPriority_ReturnsConfiguredValue()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = new UrlPath("/docs"),
            SearchPriority = 10
        };
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, DefaultLocalization);

        service.SearchPriority.ShouldBe(10);
    }

    // --- Multi-locale tests ---

    private static LocalizationOptions CreateMultiLocale()
    {
        var options = new LocalizationOptions { DefaultLocale = "en" };
        options.AddLocale("en", new LocaleInfo("English"));
        options.AddLocale("fr", new LocaleInfo("Français"));
        return options;
    }

    private static MockFileSystem CreateMultiLocaleFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.Directory.CreateDirectory("/content/fr");
        foreach (var (path, content) in files)
        {
            var fullPath = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(fullPath);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(fullPath, content);
        }
        return fs;
    }

    [Fact]
    public async Task DiscoverAsync_MultiLocale_DiscoversLocaleSubfolders()
    {
        var fs = CreateMultiLocaleFs(
            ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"),
            ("fr/getting-started.md", "---\ntitle: Démarrage\n---\n# Démarrage"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);
        items.ShouldContain(i => i.Route.Locale == "en");
        items.ShouldContain(i => i.Route.Locale == "fr");
    }

    [Fact]
    public async Task DiscoverAsync_MultiLocale_DefaultLocaleExcludesSubfolders()
    {
        var fs = CreateMultiLocaleFs(
            ("page.md", "---\ntitle: Page\n---\n# Page"),
            ("fr/page.md", "---\ntitle: Page FR\n---\n# Page FR"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        // Default locale item should NOT have fr/ prefix in path
        var enItem = items.First(i => i.Route.Locale == "en");
        enItem.Route.CanonicalPath.Value.ShouldNotContain("fr");
    }

    [Fact]
    public async Task DiscoverAsync_MultiLocale_NonDefaultLocaleRoutesHavePrefix()
    {
        var fs = CreateMultiLocaleFs(
            ("about.md", "---\ntitle: About\n---\n# About"),
            ("fr/about.md", "---\ntitle: À propos\n---\n# À propos"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var frItem = items.First(i => i.Route.Locale == "fr");
        frItem.Route.CanonicalPath.Value.ShouldStartWith("/fr/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_MultiLocale_AllLocaleEntriesPresent()
    {
        var fs = CreateMultiLocaleFs(
            ("guide.md", "---\ntitle: Guide\norder: 1\n---\n# Guide"),
            ("fr/guide.md", "---\ntitle: Guide FR\norder: 1\n---\n# Guide FR"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries.ShouldContain(e => e.Locale == "en" && e.Title == "Guide");
        entries.ShouldContain(e => e.Locale == "fr" && e.Title == "Guide FR");
    }

    [Fact]
    public async Task GetContentToCopyAsync_MultiLocale_IncludesLocaleAssets()
    {
        var fs = CreateMultiLocaleFs(
            ("images/logo.png", "fake-png"),
            ("fr/images/logo-fr.png", "fake-png-fr"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(2);
        toCopy.ShouldContain(c => c.OutputPath.Value == "images/logo.png");
        toCopy.ShouldContain(c => c.OutputPath.Value == "fr/images/logo-fr.png");
    }

    [Fact]
    public async Task GetContentToCopyAsync_BasePageUrlPrefix_AssetOutputMatchesRouteUrl()
    {
        // Regression: a post at /blog/2016/10/hello/ that references a sibling
        // image (commits.png) produces HTML pointing at /blog/2016/10/commits.png.
        // The copied asset output path must match that URL or the image 404s on
        // deploy (crawler flags this as a broken link and fails the build).
        var fs = CreateFs(
            ("2016/10/hello.md", "---\ntitle: Hello\n---\n![](commits.png)"),
            ("2016/10/commits.png", "fake-png"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/blog"));

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(1);
        toCopy[0].OutputPath.Value.ShouldBe("blog/2016/10/commits.png");
    }

    [Fact]
    public async Task GetContentToCopyAsync_BasePageUrlPrefix_MultiLocaleAssetOutputMatchesRouteUrl()
    {
        // Non-default locale must include both the locale and BasePageUrl in the
        // asset output path. A post at /fr/blog/post/ referencing sibling img.png
        // produces HTML pointing at /fr/blog/post/img.png.
        var fs = CreateMultiLocaleFs(
            ("post/img.png", "fake-png"),
            ("fr/post/img.png", "fake-png-fr"));
        var service = CreateTestService(
            fs,
            basePageUrl: new UrlPath("/blog"),
            localization: CreateMultiLocale());

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(2);
        toCopy.ShouldContain(c => c.OutputPath.Value == "blog/post/img.png");
        toCopy.ShouldContain(c => c.OutputPath.Value == "fr/blog/post/img.png");
    }

    // --- ExcludePaths tests: the outer catch-all source can carve out a subtree
    // owned by a more specific source registered nearby (Phase 6 of the Pennington
    // examples remediation plan). Without this, two overlapping sources emit
    // duplicate routes for the same canonical URLs and race on the same output files.

    private MarkdownContentService<DocFrontMatter> CreateServiceWithExcludes(
        MockFileSystem fs,
        params string[] excludePaths)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = new UrlPath("/docs"),
            SectionLabel = "Documentation",
            ExcludePaths = [.. excludePaths],
        };
        return new MarkdownContentService<DocFrontMatter>(
            options, new FrontMatterParser(), fs, DefaultLocalization);
    }

    [Fact]
    public async Task DiscoverAsync_ExcludePaths_SkipsSubtree()
    {
        var fs = CreateFs(
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"),
            ("development/setup.md", "---\ntitle: Setup\n---\n# Setup"),
            ("changelog/v1.md", "---\ntitle: v1\n---\n# v1"),
            ("changelog/v2.md", "---\ntitle: v2\n---\n# v2"));
        var service = CreateServiceWithExcludes(fs, "changelog");

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldContain("/docs/intro/");
        routes.ShouldContain("/docs/development/setup/");
        routes.ShouldNotContain(r => r.Contains("changelog", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DiscoverAsync_ExcludePaths_SegmentBoundary_DoesNotCatchPrefixMatches()
    {
        // "change" should not exclude "changelog" (segment-based matching)
        var fs = CreateFs(
            ("changelog/v1.md", "---\ntitle: v1\n---\n# v1"),
            ("change/notes.md", "---\ntitle: Notes\n---\n# Notes"));
        var service = CreateServiceWithExcludes(fs, "change");

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldContain("/docs/changelog/v1/");
        routes.ShouldNotContain("/docs/change/notes/");
    }

    [Fact]
    public async Task DiscoverAsync_ExcludePaths_NormalizesBackslashesAndCase()
    {
        var fs = CreateFs(
            ("Changelog/v1.md", "---\ntitle: v1\n---\n# v1"),
            ("guides/setup.md", "---\ntitle: Setup\n---\n# Setup"));
        // Mixed separator + uppercase in the configured exclude — should still match.
        var service = CreateServiceWithExcludes(fs, "CHANGELOG\\");

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldContain("/docs/guides/setup/");
        routes.ShouldNotContain(r => r.Contains("changelog", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DiscoverAsync_ExcludePaths_EmptyEntriesAreIgnored()
    {
        // Defensive: a bare "" or "/" must not swallow the whole content root.
        var fs = CreateFs(
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"));
        var service = CreateServiceWithExcludes(fs, "", "/", "   ");

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/intro/");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludePaths_SkipsAssetsInExcludedSubtree()
    {
        // Assets under an excluded subpath must also be skipped — otherwise the
        // more specific source would end up re-copying the same files.
        var fs = CreateFs(
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"),
            ("images/logo.png", "png"),
            ("changelog/v1.md", "---\ntitle: v1\n---\n# v1"),
            ("changelog/screenshots/v1.png", "png"));
        var service = CreateServiceWithExcludes(fs, "changelog");

        var toCopy = await service.GetContentToCopyAsync();

        var paths = toCopy.Select(c => c.OutputPath.Value).ToList();
        paths.ShouldContain("docs/images/logo.png");
        paths.ShouldNotContain(p => p.Contains("/changelog/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DiscoverAsync_ExcludePaths_MultiLocale_AppliesToAllLocales()
    {
        // An exclude configured as "changelog" should cover both the default locale
        // and each non-default locale subtree — users shouldn't have to list
        // "changelog" and "fr/changelog" separately.
        var fs = CreateMultiLocaleFs(
            ("intro.md", "---\ntitle: Intro\n---\n# Intro"),
            ("changelog/v1.md", "---\ntitle: v1\n---\n# v1"),
            ("fr/intro.md", "---\ntitle: Intro FR\n---\n# Intro FR"),
            ("fr/changelog/v1.md", "---\ntitle: v1 FR\n---\n# v1 FR"));
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = new UrlPath("/"),
            ExcludePaths = ["changelog"],
        };
        var service = new MarkdownContentService<DocFrontMatter>(
            options, new FrontMatterParser(), fs, CreateMultiLocale());

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldNotContain(r => r.Contains("changelog", StringComparison.Ordinal));
        routes.ShouldContain("/intro/");
        routes.ShouldContain("/fr/intro/");
    }

    [Fact]
    public void ExcludePaths_NormalizedOnOptions_ExposedViaInterface()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var service = CreateServiceWithExcludes(fs, "CHANGELOG", "DRAFT/posts");

        IMarkdownContentSource source = service;
        source.ExcludePaths.ShouldBe(["changelog", "draft/posts"]);
    }

    [Fact]
    public async Task DiscoverAsync_MultiLocale_SingleLocaleOptIn_NoExtraDiscovery()
    {
        // When only one locale is configured, behavior should be unchanged
        var fs = CreateFs(
            ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"));
        var singleLocale = new LocalizationOptions { DefaultLocale = "en" };
        singleLocale.AddLocale("en", "English");
        var service = CreateTestService(fs, localization: singleLocale);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        // With single locale, original behavior: locale comes from options.Locale (empty by default)
    }

    // --- Search / Llms opt-out tests ---

    [Fact]
    public async Task GetContentTocEntriesAsync_SearchFalse_SetsExcludeFromSearch()
    {
        var fs = CreateFs(
            ("visible.md", "---\ntitle: Visible\n---\n# Visible"),
            ("hidden.md", "---\ntitle: Hidden\nsearch: false\n---\n# Hidden"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries.First(e => e.Title == "Visible").ExcludeFromSearch.ShouldBeFalse();
        entries.First(e => e.Title == "Hidden").ExcludeFromSearch.ShouldBeTrue();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_LlmsFalse_SetsExcludeFromLlms()
    {
        var fs = CreateFs(
            ("visible.md", "---\ntitle: Visible\n---\n# Visible"),
            ("hidden.md", "---\ntitle: Hidden\nllms: false\n---\n# Hidden"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries.First(e => e.Title == "Visible").ExcludeFromLlms.ShouldBeFalse();
        entries.First(e => e.Title == "Hidden").ExcludeFromLlms.ShouldBeTrue();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SearchAndLlmsFalse_BothFlagsSet()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: Page\nsearch: false\nllms: false\n---\n# Page"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].ExcludeFromSearch.ShouldBeTrue();
        entries[0].ExcludeFromLlms.ShouldBeTrue();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SearchTrue_DefaultBehavior()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: Page\nsearch: true\nllms: true\n---\n# Page"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].ExcludeFromSearch.ShouldBeFalse();
        entries[0].ExcludeFromLlms.ShouldBeFalse();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SearchFalse_StillInTocForNavigation()
    {
        // search: false should NOT remove the page from TOC (navigation still needs it)
        var fs = CreateFs(
            ("page.md", "---\ntitle: Searchable\n---\n# Searchable"),
            ("no-search.md", "---\ntitle: Not Searchable\nsearch: false\n---\n# Not Searchable"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        // Both pages should be in the TOC
        entries.Count.ShouldBe(2);
        entries.ShouldContain(e => e.Title == "Searchable");
        entries.ShouldContain(e => e.Title == "Not Searchable");
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_ExcludesSearchFalsePages()
    {
        var fs = CreateFs(
            ("visible.md", "---\ntitle: Visible\n---\n# Visible"),
            ("hidden.md", "---\ntitle: Hidden\nsearch: false\n---\n# Hidden"));
        var service = CreateTestService(fs);

        IContentService svc = service;
        var entries = await svc.GetIndexableEntriesAsync();

        // Both pages appear; the hidden one carries ExcludeFromSearch so search/llms consumers can filter.
        entries.Count.ShouldBe(2);
        entries.First(e => e.Title == "Hidden").ExcludeFromSearch.ShouldBeTrue();
    }

    [Fact]
    public async Task DiscoverAsync_LlmsMd_ProducesLlmsOnlySource()
    {
        var fs = CreateFs(
            ("regular.md", "---\ntitle: Regular\n---\n# Regular"),
            ("agent-context.llms.md", "---\ntitle: Agent Context\n---\n# Agent Context"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);
        var regular = items.Single(i => i.Source is FileSource);
        var llmsOnly = items.Single(i => i.Source is LlmsOnlySource);
        regular.Route.CanonicalPath.Value.ShouldBe("/docs/regular/");
        llmsOnly.Route.CanonicalPath.Value.ShouldBe("/docs/agent-context/");
    }

    [Fact]
    public async Task DiscoverAsync_LlmsMd_PreservesActualSourceFile()
    {
        var fs = CreateFs(
            ("agent-context.llms.md", "---\ntitle: Agent Context\n---\n# Agent Context"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        var llmsOnly = items.Single();
        if (llmsOnly.Source is LlmsOnlySource llms)
        {
            llms.Path.Value.ShouldEndWith("agent-context.llms.md");
        }
        else
        {
            throw new ShouldAssertException("Source was not LlmsOnlySource");
        }
        // Route's SourceFile should also point at the real file on disk so diagnostics can locate it.
        llmsOnly.Route.SourceFile?.Value.ShouldEndWith("agent-context.llms.md");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_ExcludesLlmsOnlyFiles()
    {
        var fs = CreateFs(
            ("regular.md", "---\ntitle: Regular\n---\n# Regular"),
            ("agent-context.llms.md", "---\ntitle: Agent Context\n---\n# Agent Context"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Regular");
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_IncludesLlmsOnlyFilesAsSearchExcluded()
    {
        var fs = CreateFs(
            ("regular.md", "---\ntitle: Regular\n---\n# Regular"),
            ("agent-context.llms.md", "---\ntitle: Agent Context\n---\n# Agent Context"));
        var service = CreateTestService(fs);

        IContentService svc = service;
        var entries = await svc.GetIndexableEntriesAsync();

        entries.Count.ShouldBe(2);
        var llmsEntry = entries.Single(e => e.Title == "Agent Context");
        llmsEntry.ExcludeFromSearch.ShouldBeTrue();
        llmsEntry.ExcludeFromLlms.ShouldBeFalse();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SearchOnlyFrontMatter_FlowsToTocItem()
    {
        var fs = CreateFs(
            ("visible.md", "---\ntitle: Visible Page\n---\n# Visible"),
            ("hidden.md", "---\ntitle: Hidden FAQ\nsearchOnly: true\n---\n# Hidden"));
        var service = CreateTestService(fs);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries.Single(e => e.Title == "Visible Page").SearchOnly.ShouldBeFalse();
        entries.Single(e => e.Title == "Hidden FAQ").SearchOnly.ShouldBeTrue();
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_SearchOnlyFrontMatter_StillIndexable()
    {
        // SearchOnly entries flow into the indexable channel as ExcludeFromSearch=false,
        // so SearchIndexService keeps them. NavigationBuilder filters them at render time.
        var fs = CreateFs(
            ("hidden.md", "---\ntitle: Hidden FAQ\nsearchOnly: true\n---\n# Hidden"));
        var service = CreateTestService(fs);

        IContentService svc = service;
        var entries = await svc.GetIndexableEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].SearchOnly.ShouldBeTrue();
        entries[0].ExcludeFromSearch.ShouldBeFalse();
        entries[0].ExcludeFromLlms.ShouldBeFalse();
    }

    // --- Incremental OnFileChanged refresh ---
    // OnFileChanged matches the watcher scope by directory-prefix comparison, so paths
    // must match the absolute form the service resolved at construction. The MockFileSystem
    // root depends on host platform — read AbsoluteContentRoot via IMarkdownContentSource
    // and build change paths off it.
    private static string AbsoluteContentPath(MarkdownContentService<DocFrontMatter> service, MockFileSystem fs, string relativeUnderContent)
    {
        IMarkdownContentSource source = service;
        return fs.Path.Combine(source.AbsoluteContentRoot, relativeUnderContent);
    }

    [Fact]
    public async Task OnFileChanged_Changed_RefreshesOnlyTouchedFile()
    {
        var fs = CreateFs(
            ("page-a.md", "---\ntitle: A v1\n---\n# A"),
            ("page-b.md", "---\ntitle: B v1\n---\n# B"));
        var service = CreateTestService(fs);
        var changedAbsolute = AbsoluteContentPath(service, fs, "page-a.md");

        // Seed the cache.
        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(2);

        // Edit one file on disk.
        fs.File.WriteAllText(changedAbsolute, "---\ntitle: A v2\n---\n# A");
        var response = service.OnFileChanged(new FileChangeNotification(changedAbsolute, WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Refreshed);

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(2);
        after.Single(e => e.Route.CanonicalPath.Value == "/docs/page-a/").Title.ShouldBe("A v2");
        after.Single(e => e.Route.CanonicalPath.Value == "/docs/page-b/").Title.ShouldBe("B v1");
    }

    [Fact]
    public async Task OnFileChanged_Created_AddsNewFileToToc()
    {
        var fs = CreateFs(
            ("existing.md", "---\ntitle: Existing\n---\n# Existing"));
        var service = CreateTestService(fs);
        var newAbsolute = AbsoluteContentPath(service, fs, "new.md");

        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(1);

        fs.File.WriteAllText(newAbsolute, "---\ntitle: New Page\n---\n# New");
        service.OnFileChanged(new FileChangeNotification(newAbsolute, WatcherChangeTypes.Created));

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(2);
        after.ShouldContain(e => e.Title == "New Page");
    }

    [Fact]
    public async Task OnFileChanged_Deleted_RemovesFileFromToc()
    {
        var fs = CreateFs(
            ("keep.md", "---\ntitle: Keep\n---\n# Keep"),
            ("remove.md", "---\ntitle: Remove\n---\n# Remove"));
        var service = CreateTestService(fs);
        var removeAbsolute = AbsoluteContentPath(service, fs, "remove.md");

        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(2);

        fs.File.Delete(removeAbsolute);
        service.OnFileChanged(new FileChangeNotification(removeAbsolute, WatcherChangeTypes.Deleted));

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(1);
        after[0].Title.ShouldBe("Keep");
    }

    [Fact]
    public async Task OnFileChanged_AssetFile_LeavesContentMetadataIntact()
    {
        // An image change under the content tree shouldn't evict the markdown
        // metadata cache. The previous behaviour wholesale-reset both caches
        // on any change inside the scope.
        var fs = CreateFs(
            ("page.md", "---\ntitle: Page\n---\n# Page"),
            ("logo.png", "fake-png"));
        var service = CreateTestService(fs);
        var pageAbsolute = AbsoluteContentPath(service, fs, "page.md");
        var logoAbsolute = AbsoluteContentPath(service, fs, "logo.png");

        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(1);

        // Flip the file on disk in a way that would break the next re-parse, then fire
        // an asset change — if the cache *were* evicted, the next read would pick up
        // garbage. With the asset-filter in OnFileChanged it stays valid.
        fs.File.WriteAllText(pageAbsolute, "::: not yaml :::");
        var response = service.OnFileChanged(new FileChangeNotification(logoAbsolute, WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Ignore);

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(1);
        after[0].Title.ShouldBe("Page");
    }

    [Fact]
    public async Task OnFileChanged_MetaYml_UpdatesFolderMetadataNotContentToc()
    {
        var fs = CreateFs(
            ("guides/setup.md", "---\ntitle: Setup\n---\n# Setup"),
            ("guides/_meta.yml", "title: Guides v1\norder: 1\n"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"));
        var metaAbsolute = AbsoluteContentPath(service, fs, "guides/_meta.yml");
        var setupAbsolute = AbsoluteContentPath(service, fs, "guides/setup.md");

        // Seed both caches so subsequent reads come from the cache, not the filesystem.
        var initialToc = await service.GetContentTocEntriesAsync();
        initialToc.Single().Title.ShouldBe("Setup");
        var initialFolders = await service.GetFolderMetadataAsync();
        initialFolders.Single().Title.ShouldBe("Guides v1");

        // Corrupt the markdown on disk; if the _meta.yml change incorrectly evicted the
        // markdown cache, the next read would pick up garbage. Update the sidecar and notify.
        fs.File.WriteAllText(setupAbsolute, "::: not yaml :::");
        fs.File.WriteAllText(metaAbsolute, "title: Guides v2\norder: 1\n");
        service.OnFileChanged(new FileChangeNotification(metaAbsolute, WatcherChangeTypes.Changed));

        (await service.GetFolderMetadataAsync()).Single().Title.ShouldBe("Guides v2");
        // Content TOC stays as it was — _meta.yml change must not touch the markdown cache.
        var toc = await service.GetContentTocEntriesAsync();
        toc.Single().Title.ShouldBe("Setup");
    }

    [Fact]
    public async Task OnFileChanged_MultiLocale_LocalizedCreated_DropsDefaultLocaleFallback()
    {
        // Default-locale-only setup: en has "about.md", fr does not.
        // The cache should currently surface a fallback en-source route for fr.
        var fs = CreateMultiLocaleFs(
            ("about.md", "---\ntitle: About\n---\n# About"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());
        var frAboutAbsolute = AbsoluteContentPath(service, fs, "fr/about.md");

        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(2);
        initial.ShouldContain(e => e.Locale == "fr"); // fallback for fr

        // Now add a real fr/about.md and fire Created. The fr fallback must drop;
        // fr's entry should reflect the localized file's title.
        fs.File.WriteAllText(frAboutAbsolute, "---\ntitle: À propos\n---\n# À propos");
        service.OnFileChanged(new FileChangeNotification(frAboutAbsolute, WatcherChangeTypes.Created));

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(2);
        var frEntry = after.Single(e => e.Locale == "fr");
        frEntry.Title.ShouldBe("À propos");
    }

    [Fact]
    public async Task OnFileChanged_MultiLocale_LocalizedDeleted_RestoresDefaultLocaleFallback()
    {
        var fs = CreateMultiLocaleFs(
            ("about.md", "---\ntitle: About\n---\n# About"),
            ("fr/about.md", "---\ntitle: À propos\n---\n# À propos"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());
        var frAboutAbsolute = AbsoluteContentPath(service, fs, "fr/about.md");

        var initial = await service.GetContentTocEntriesAsync();
        initial.Count.ShouldBe(2);
        initial.Single(e => e.Locale == "fr").Title.ShouldBe("À propos");

        fs.File.Delete(frAboutAbsolute);
        service.OnFileChanged(new FileChangeNotification(frAboutAbsolute, WatcherChangeTypes.Deleted));

        var after = await service.GetContentTocEntriesAsync();
        after.Count.ShouldBe(2);
        // fr falls back to the en source, so its title is the en title.
        after.Single(e => e.Locale == "fr").Title.ShouldBe("About");
    }

    // --- Body caching on FileEntry ---
    // ParseContentAsync yields from the in-memory cache populated during seeding,
    // so a second enumeration does not re-read the source files. OnFileChanged
    // re-populates the cached body alongside the front matter.

    [Fact]
    public async Task ParseContentAsync_SecondEnumeration_DoesNotReadFiles()
    {
        var fs = CreateFs(
            ("page-a.md", "---\ntitle: A\n---\n# A\n\nBody A."),
            ("page-b.md", "---\ntitle: B\n---\n# B\n\nBody B."));
        var service = CreateTestService(fs);

        // First enumeration seeds the cache.
        await foreach (var _ in service.ParseContentAsync())
        {
        }

        // Delete the files on disk. A cache-backed ParseContentAsync still yields
        // the cached bodies; a disk-backed one would skip the missing files.
        var pageAAbsolute = AbsoluteContentPath(service, fs, "page-a.md");
        var pageBAbsolute = AbsoluteContentPath(service, fs, "page-b.md");
        fs.File.Delete(pageAAbsolute);
        fs.File.Delete(pageBAbsolute);

        var items = new List<ParsedItem>();
        await foreach (var item in service.ParseContentAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);
        items.Single(i => i.Metadata.Title == "A").RawMarkdown.ShouldContain("Body A.");
        items.Single(i => i.Metadata.Title == "B").RawMarkdown.ShouldContain("Body B.");
    }

    [Fact]
    public async Task OnFileChanged_Changed_UpdatesCachedBody()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: Page\n---\n# v1\n\nOriginal body."));
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "page.md");

        // Seed.
        var initial = await ToListAsync(service.ParseContentAsync());
        initial.Single().RawMarkdown.ShouldContain("Original body.");

        // Edit and notify.
        fs.File.WriteAllText(absolute, "---\ntitle: Page\n---\n# v2\n\nUpdated body.");
        service.OnFileChanged(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        var after = await ToListAsync(service.ParseContentAsync());
        after.Single().RawMarkdown.ShouldContain("Updated body.");
        after.Single().RawMarkdown.ShouldNotContain("Original body.");
    }

    [Fact]
    public async Task OnFileChanged_Deleted_DropsRouteFromParseContentAsync()
    {
        var fs = CreateFs(
            ("keep.md", "---\ntitle: Keep\n---\n# Keep\n\nKeep body."),
            ("remove.md", "---\ntitle: Remove\n---\n# Remove\n\nRemove body."));
        var service = CreateTestService(fs);
        var removeAbsolute = AbsoluteContentPath(service, fs, "remove.md");

        var initial = await ToListAsync(service.ParseContentAsync());
        initial.Count.ShouldBe(2);

        fs.File.Delete(removeAbsolute);
        service.OnFileChanged(new FileChangeNotification(removeAbsolute, WatcherChangeTypes.Deleted));

        var after = await ToListAsync(service.ParseContentAsync());
        after.Count.ShouldBe(1);
        after.Single().Metadata.Title.ShouldBe("Keep");
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    [Fact]
    public void GetAffectedRoutes_MarkdownChanged_ReturnsOwnRoute()
    {
        var fs = CreateFs(("getting-started.md", "---\ntitle: GS\n---"));
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "getting-started.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        var routes = impact.AffectedRoutes.ShouldNotBeNull();
        routes.Length.ShouldBe(1);
        routes[0].CanonicalPath.Value.ShouldBe("/docs/getting-started/");
    }

    [Fact]
    public void GetAffectedRoutes_MarkdownCreated_ReturnsOwnRoute()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "new-page.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Created));

        var routes = impact.AffectedRoutes.ShouldNotBeNull();
        routes.Length.ShouldBe(1);
        routes[0].CanonicalPath.Value.ShouldBe("/docs/new-page/");
    }

    [Fact]
    public void GetAffectedRoutes_MarkdownDeleted_StillReturnsRouteFromPath()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "gone.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Deleted));

        var routes = impact.AffectedRoutes.ShouldNotBeNull();
        routes[0].CanonicalPath.Value.ShouldBe("/docs/gone/");
    }

    [Fact]
    public void GetAffectedRoutes_MarkdownRenamed_ReturnsWildcard()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "x.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Renamed));

        impact.AffectedRoutes.ShouldBeNull();
    }

    [Fact]
    public void GetAffectedRoutes_FolderMetaChanged_ReturnsWildcard()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "subdir/_meta.yml");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        impact.AffectedRoutes.ShouldBeNull();
    }

    [Fact]
    public void GetAffectedRoutes_OutOfScopeFile_ReturnsNone()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);

        // Path that isn't under the watch scope at all.
        var impact = service.GetAffectedRoutes(new FileChangeNotification("/elsewhere/some.md", WatcherChangeTypes.Changed));

        impact.AffectedRoutes.ShouldNotBeNull().IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void GetAffectedRoutes_AssetUnderContentTree_ReturnsNone()
    {
        var fs = CreateFs();
        var service = CreateTestService(fs);
        var absolute = AbsoluteContentPath(service, fs, "image.png");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        impact.AffectedRoutes.ShouldNotBeNull().IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void GetAffectedRoutes_MultiLocaleDefaultLocale_IncludesFallbackRoutes()
    {
        var fs = CreateMultiLocaleFs(("about.md", "---\ntitle: About\n---"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());
        var absolute = AbsoluteContentPath(service, fs, "about.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        var routes = impact.AffectedRoutes.ShouldNotBeNull();
        // Own route + fr fallback route (multi-locale config has en + fr).
        routes.Length.ShouldBeGreaterThanOrEqualTo(2);
        routes.ShouldContain(r => r.CanonicalPath.Value == "/about/");
        routes.ShouldContain(r => r.CanonicalPath.Value == "/fr/about/");
    }

    [Fact]
    public void GetAffectedRoutes_MultiLocaleNonDefaultLocale_ReturnsOwnRouteOnly()
    {
        var fs = CreateMultiLocaleFs(("fr/about.md", "---\ntitle: À propos\n---"));
        var service = CreateTestService(fs, basePageUrl: new UrlPath("/"), localization: CreateMultiLocale());
        var absolute = AbsoluteContentPath(service, fs, "fr/about.md");

        var impact = service.GetAffectedRoutes(new FileChangeNotification(absolute, WatcherChangeTypes.Changed));

        var routes = impact.AffectedRoutes.ShouldNotBeNull();
        routes.ShouldContain(r => r.CanonicalPath.Value == "/fr/about/");
    }
}