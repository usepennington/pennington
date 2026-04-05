using System.Collections.Immutable;
using Penn.Feeds;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Feeds;

public class SitemapBuilderTests
{
    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale
    };

    private static RenderedContent MakeContent(string html = "<p>Hello</p>") => new(
        Html: html,
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        SearchDocument: null,
        Social: null
    );

    private record TestFrontMatter : IFrontMatter, IDraftable, ISectionable, IDateable, IDescribable
    {
        public string Title { get; init; } = "Test";
        public bool IsDraft { get; init; }
        public string? Section { get; init; }
        public DateTime? Date { get; init; }
        public string? Description { get; init; }
    }

    private readonly SitemapBuilder _builder = new(new UrlPath("https://example.com"));

    [Fact]
    public void Build_CreatesEntriesWithAbsoluteUrls()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/about"),
                Metadata: new TestFrontMatter { Title = "About" },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/contact"),
                Metadata: new TestFrontMatter { Title = "Contact" },
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(2);
        entries[0].Url.Value.ShouldBe("https://example.com/about/");
        entries[1].Url.Value.ShouldBe("https://example.com/contact/");
    }

    [Fact]
    public void Build_ExcludesDrafts()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/published"),
                Metadata: new TestFrontMatter { Title = "Published" },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/draft"),
                Metadata: new TestFrontMatter { Title = "Draft", IsDraft = true },
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/published/");
    }

    [Fact]
    public void Build_UsesDateFromIDateable()
    {
        var date = new DateTime(2026, 3, 15);
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/post"),
                Metadata: new TestFrontMatter { Title = "Post", Date = date },
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].LastModified.ShouldBe(date);
    }

    [Fact]
    public void Build_EmptyInput_ReturnsEmptyList()
    {
        var entries = _builder.Build([]);

        entries.ShouldBeEmpty();
    }

    // Front matter with no capabilities
    private record MinimalFrontMatter(string Title) : IFrontMatter;

    [Fact]
    public void Build_NonDraftableFrontMatter_Included()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/page"),
                Metadata: new MinimalFrontMatter("Minimal Page"),
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/page/");
    }

    [Fact]
    public void Build_NonDateable_LastModifiedIsNull()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/page"),
                Metadata: new MinimalFrontMatter("No Date"),
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].LastModified.ShouldBeNull();
    }

    [Fact]
    public void Build_LocaleInRoute_PreservedInUrl()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/fr/docs/intro", "fr"),
                Metadata: new TestFrontMatter { Title = "Introduction" },
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].Url.Value.ShouldBe("https://example.com/fr/docs/intro/");
    }

    [Fact]
    public void Build_MultiplePagesWithDrafts_OnlyNonDraftsIncluded()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/getting-started"),
                Metadata: new TestFrontMatter { Title = "Getting Started" },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/wip"),
                Metadata: new TestFrontMatter { Title = "WIP", IsDraft = true },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/about"),
                Metadata: new MinimalFrontMatter("About"),
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(2);
        var urls = entries.Select(e => e.Url.Value).ToList();
        urls.ShouldContain("https://example.com/getting-started/");
        urls.ShouldContain("https://example.com/about/");
    }

    [Fact]
    public void Build_ChangeFrequencyAndPriority_AreNull()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/page"),
                Metadata: new TestFrontMatter { Title = "Page", Date = new DateTime(2026, 1, 1) },
                Content: MakeContent()
            ),
        };

        var entries = _builder.Build(items);

        entries.Count.ShouldBe(1);
        entries[0].ChangeFrequency.ShouldBeNull();
        entries[0].Priority.ShouldBeNull();
    }
}
