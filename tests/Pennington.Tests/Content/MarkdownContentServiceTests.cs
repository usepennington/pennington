using System.Collections.Immutable;
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
        LocalizationOptions? localization = null)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = basePageUrl ?? new UrlPath("/docs"),
            Section = section
        };

        return new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, new FileWatcher(fs), localization ?? DefaultLocalization);
    }

    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        foreach (var (path, content) in files)
        {
            var fullPath = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(fullPath);
            if (dir != null) fs.Directory.CreateDirectory(dir);
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
            items.Add(item);

        items.Count.ShouldBe(2);
        foreach (var item in items)
            (item.Source is MarkdownFileSource).ShouldBeTrue();
    }

    [Fact]
    public async Task DiscoverAsync_BuildsCorrectRoutes()
    {
        var fs = CreateFs(
            ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"));
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
            items.Add(item);

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
            items.Add(item);

        items.Count.ShouldBe(1);
        (items[0].Source is MarkdownFileSource).ShouldBeTrue();
        var path = items[0].Source switch
        {
            MarkdownFileSource m => m.Path.Value,
            _ => ""
        };
        path.ShouldEndWith("intro.md");
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
            items.Add(item);

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

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromOptions()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: A Page\n---\n# A Page"));
        var service = CreateTestService(fs, section: "Guides");

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Section.ShouldBe("Guides");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromFrontMatter()
    {
        var fs = CreateFs(
            ("page.md", "---\ntitle: A Page\nsection: OverriddenSection\n---\n# A Page"));
        var service = CreateTestService(fs, section: "DefaultSection");

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Section.ShouldBe("OverriddenSection");
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
        toCopy[0].OutputPath.Value.ShouldBe("images/logo.png");
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
        outputPaths.ShouldContain("image.jpg");
        outputPaths.ShouldContain("script.js");
    }

    [Fact]
    public async Task EmptyDirectory_ReturnsEmptyResults()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var service = CreateTestService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
            items.Add(item);

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
            Section = "Test"
        };
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, new FileWatcher(fs), DefaultLocalization);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
            items.Add(item);

        items.ShouldBeEmpty();

        var toc = await service.GetContentTocEntriesAsync();
        toc.ShouldBe(ImmutableList<ContentTocItem>.Empty);

        var xrefs = await service.GetCrossReferencesAsync();
        xrefs.ShouldBe(ImmutableList<CrossReference>.Empty);

        var toCopy = await service.GetContentToCopyAsync();
        toCopy.ShouldBe(ImmutableList<ContentToCopy>.Empty);
    }

    [Fact]
    public void DefaultSection_ReturnsEmptyWhenNull()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        var service = CreateTestService(fs, section: null);
        service.DefaultSection.ShouldBe("");
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
            items.Add(item);

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
            items.Add(item);

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
            Section = section
        };

        return new MarkdownContentService<BlogFrontMatter>(options, new FrontMatterParser(), fs, new FileWatcher(fs), DefaultLocalization);
    }

    [Fact]
    public async Task BlogFrontMatter_DiscoverAsync_FindsPosts()
    {
        var fs = CreateFs(
            ("hello-world.md", "---\ntitle: Hello World\ndate: 2026-03-15\ntags:\n  - intro\n---\n# Hello World"));
        var service = CreateBlogService(fs);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
            items.Add(item);

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
            items.Add(item);

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
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), fs, new FileWatcher(fs), DefaultLocalization);

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
            if (dir != null) fs.Directory.CreateDirectory(dir);
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
            items.Add(item);

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
            items.Add(item);

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
            items.Add(item);

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
            items.Add(item);

        items.Count.ShouldBe(1);
        // With single locale, original behavior: locale comes from options.Locale (empty by default)
    }
}
