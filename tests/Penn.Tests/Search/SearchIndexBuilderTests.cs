using System.Collections.Immutable;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;
using Penn.Search;

namespace Penn.Tests.Search;

public class SearchIndexBuilderTests
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

    private readonly SearchIndexBuilder _builder = new();

    [Fact]
    public void Build_ReturnsDocument_WithTitleBodyUrlAndLocale()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/docs/intro", "en"),
            Metadata: new TestFrontMatter { Title = "Introduction" },
            Content: MakeContent("<p>Welcome to the docs</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Title.ShouldBe("Introduction");
        doc.Body.ShouldBe("Welcome to the docs");
        doc.Url.Value.ShouldBe("/docs/intro");
        doc.Locale.ShouldBe("en");
        doc.Priority.ShouldBe(5);
    }

    [Fact]
    public void Build_StripsHtmlFromBody()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/page"),
            Metadata: new TestFrontMatter { Title = "Page" },
            Content: MakeContent("<p>Hello <strong>world</strong></p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldBe("Hello world");
    }

    [Fact]
    public void Build_SkipsDraftItems_ReturnsNull()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/draft"),
            Metadata: new TestFrontMatter { Title = "Draft Post", IsDraft = true },
            Content: MakeContent()
        );

        var doc = _builder.Build(item);

        doc.ShouldBeNull();
    }

    [Fact]
    public void Build_IncludesSectionFromISectionable()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/api/auth"),
            Metadata: new TestFrontMatter { Title = "Auth", Section = "api" },
            Content: MakeContent()
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Section.ShouldBe("api");
    }

    [Fact]
    public void Build_HandlesEmptyHtml()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/empty"),
            Metadata: new TestFrontMatter { Title = "Empty" },
            Content: MakeContent("")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldBe("");
    }
}
