using System.Collections.Immutable;
using Penn.Feeds;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Feeds;

public class RssFeedBuilderTests
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

    private readonly RssFeedBuilder _builder = new(new UrlPath("https://example.com"));

    [Fact]
    public void Build_CreatesFeedItemsFromDatedItems()
    {
        var date = new DateTime(2026, 3, 15);
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/blog/post-1"),
                Metadata: new TestFrontMatter { Title = "Post One", Date = date },
                Content: MakeContent()
            ),
        };

        var feed = _builder.Build(items);

        feed.Count.ShouldBe(1);
        feed[0].Title.ShouldBe("Post One");
        feed[0].Url.Value.ShouldBe("https://example.com/blog/post-1");
        feed[0].PublishDate.ShouldBe(date);
    }

    [Fact]
    public void Build_SortedByDateDescending()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/blog/old"),
                Metadata: new TestFrontMatter { Title = "Old Post", Date = new DateTime(2025, 1, 1) },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/blog/new"),
                Metadata: new TestFrontMatter { Title = "New Post", Date = new DateTime(2026, 6, 1) },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/blog/mid"),
                Metadata: new TestFrontMatter { Title = "Mid Post", Date = new DateTime(2026, 3, 1) },
                Content: MakeContent()
            ),
        };

        var feed = _builder.Build(items);

        feed.Count.ShouldBe(3);
        feed[0].Title.ShouldBe("New Post");
        feed[1].Title.ShouldBe("Mid Post");
        feed[2].Title.ShouldBe("Old Post");
    }

    [Fact]
    public void Build_ExcludesDrafts()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/blog/published"),
                Metadata: new TestFrontMatter { Title = "Published", Date = new DateTime(2026, 3, 1) },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/blog/draft"),
                Metadata: new TestFrontMatter { Title = "Draft", IsDraft = true, Date = new DateTime(2026, 4, 1) },
                Content: MakeContent()
            ),
        };

        var feed = _builder.Build(items);

        feed.Count.ShouldBe(1);
        feed[0].Title.ShouldBe("Published");
    }

    [Fact]
    public void Build_ExcludesUndatedItems()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/blog/dated"),
                Metadata: new TestFrontMatter { Title = "Dated", Date = new DateTime(2026, 3, 1) },
                Content: MakeContent()
            ),
            new(
                Route: MakeRoute("/about"),
                Metadata: new TestFrontMatter { Title = "About", Date = null },
                Content: MakeContent()
            ),
        };

        var feed = _builder.Build(items);

        feed.Count.ShouldBe(1);
        feed[0].Title.ShouldBe("Dated");
    }

    [Fact]
    public void Build_IncludesDescriptionFromIDescribable()
    {
        var items = new List<RenderedItem>
        {
            new(
                Route: MakeRoute("/blog/post"),
                Metadata: new TestFrontMatter
                {
                    Title = "Post",
                    Date = new DateTime(2026, 3, 1),
                    Description = "A great article about testing"
                },
                Content: MakeContent()
            ),
        };

        var feed = _builder.Build(items);

        feed.Count.ShouldBe(1);
        feed[0].Description.ShouldBe("A great article about testing");
    }
}
