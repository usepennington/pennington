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
        CanonicalPath = new UrlPath(path),
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
        entries[0].Url.Value.ShouldBe("https://example.com/about");
        entries[1].Url.Value.ShouldBe("https://example.com/contact");
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
        entries[0].Url.Value.ShouldBe("https://example.com/published");
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
}
