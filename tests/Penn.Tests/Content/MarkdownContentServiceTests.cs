using System.Collections.Immutable;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Content;

public class MarkdownContentServiceTests : IDisposable
{
    private readonly List<string> _tempDirs = [];

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    private MarkdownContentService<DocFrontMatter> CreateTestService(
        string? section = "Documentation",
        UrlPath? basePageUrl = null,
        params (string RelativePath, string Content)[] files)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "penn_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        _tempDirs.Add(tempDir);

        foreach (var (relativePath, content) in files)
        {
            var fullPath = Path.Combine(tempDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
        }

        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(tempDir),
            BasePageUrl = basePageUrl ?? new UrlPath("/docs"),
            Section = section
        };

        return new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser());
    }

    [Fact]
    public async Task DiscoverAsync_FindsMarkdownFiles()
    {
        var service = CreateTestService(
            files: [
                ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started"),
                ("advanced.md", "---\ntitle: Advanced\n---\n# Advanced")
            ]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);
        foreach (var item in items)
        {
            (item.Source is MarkdownFileSource).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task DiscoverAsync_BuildsCorrectRoutes()
    {
        var service = CreateTestService(
            files: [
                ("getting-started.md", "---\ntitle: Getting Started\n---\n# Getting Started")
            ]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/docs/getting-started");
    }

    [Fact]
    public async Task DiscoverAsync_SourcePointsToFile()
    {
        var service = CreateTestService(
            files: [
                ("intro.md", "---\ntitle: Intro\n---\n# Intro")
            ]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

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
    public async Task GetContentTocEntriesAsync_ReturnsEntries()
    {
        var service = CreateTestService(
            files: [
                ("getting-started.md", "---\ntitle: Getting Started\norder: 1\n---\n# Getting Started"),
                ("advanced.md", "---\ntitle: Advanced Topics\norder: 5\n---\n# Advanced")
            ]);

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
        var service = CreateTestService(
            files: [
                ("published.md", "---\ntitle: Published\nisDraft: false\n---\n# Published"),
                ("draft.md", "---\ntitle: Draft Post\nisDraft: true\n---\n# Draft")
            ]);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Title.ShouldBe("Published");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromOptions()
    {
        var service = CreateTestService(
            section: "Guides",
            files: [
                ("page.md", "---\ntitle: A Page\n---\n# A Page")
            ]);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Section.ShouldBe("Guides");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_UsesSectionFromFrontMatter()
    {
        var service = CreateTestService(
            section: "DefaultSection",
            files: [
                ("page.md", "---\ntitle: A Page\nsection: OverriddenSection\n---\n# A Page")
            ]);

        var entries = await service.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(1);
        entries[0].Section.ShouldBe("OverriddenSection");
    }

    [Fact]
    public async Task GetCrossReferencesAsync_ReturnsXrefs()
    {
        var service = CreateTestService(
            files: [
                ("api.md", "---\ntitle: API Reference\nuid: api-ref\n---\n# API"),
                ("guide.md", "---\ntitle: User Guide\nuid: user-guide\n---\n# Guide")
            ]);

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
        var service = CreateTestService(
            files: [
                ("with-uid.md", "---\ntitle: Has UID\nuid: my-uid\n---\n# Has UID"),
                ("no-uid.md", "---\ntitle: No UID\n---\n# No UID")
            ]);

        var xrefs = await service.GetCrossReferencesAsync();

        xrefs.Count.ShouldBe(1);
        xrefs[0].Uid.ShouldBe("my-uid");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludesMarkdown()
    {
        var service = CreateTestService(
            files: [
                ("page.md", "---\ntitle: Page\n---\n# Page"),
                ("images/logo.png", "fake-png-data"),
                ("data/config.yml", "key: value")
            ]);

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(1);
        toCopy[0].OutputPath.Value.ShouldBe("images/logo.png");
    }

    [Fact]
    public async Task GetContentToCopyAsync_ExcludesAllExcludedExtensions()
    {
        var service = CreateTestService(
            files: [
                ("page.md", "# Markdown"),
                ("page.mdx", "# MDX"),
                ("page.razor", "<div>Razor</div>"),
                ("data.yml", "key: value"),
                ("data.yaml", "key: value"),
                ("image.jpg", "fake-jpg-data"),
                ("script.js", "console.log('hello');")
            ]);

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(2);
        var outputPaths = toCopy.Select(c => c.OutputPath.Value).ToList();
        outputPaths.ShouldContain("image.jpg");
        outputPaths.ShouldContain("script.js");
    }

    [Fact]
    public async Task EmptyDirectory_ReturnsEmptyResults()
    {
        var service = CreateTestService(files: []);

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
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(Path.Combine(Path.GetTempPath(), "penn_nonexistent_" + Guid.NewGuid().ToString("N"))),
            BasePageUrl = new UrlPath("/docs"),
            Section = "Test"
        };

        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser());

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
    public void DefaultSection_ReturnsEmptyWhenNull()
    {
        var service = CreateTestService(section: null, files: []);
        service.DefaultSection.ShouldBe("");
    }

    [Fact]
    public async Task DiscoverAsync_SubdirectoryFiles()
    {
        var service = CreateTestService(
            files: [
                ("guides/setup.md", "---\ntitle: Setup Guide\n---\n# Setup"),
                ("guides/advanced/config.md", "---\ntitle: Config\n---\n# Config")
            ]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(2);

        var routes = items.Select(i => i.Route.CanonicalPath.Value).ToList();
        routes.ShouldContain("/docs/guides/setup");
        routes.ShouldContain("/docs/guides/advanced/config");
    }
}
