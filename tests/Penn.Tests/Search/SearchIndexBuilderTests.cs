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
        doc.Url.ShouldBe("/docs/intro/");
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

    [Fact]
    public void Build_DecodesHtmlEntities()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/entities"),
            Metadata: new TestFrontMatter { Title = "Entities" },
            Content: MakeContent("<p>Tom &amp; Jerry &lt;3 &gt; others &quot;said&quot; he&#39;s&nbsp;right</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldBe("Tom & Jerry <3 > others \"said\" he's right");
    }

    [Fact]
    public void Build_StripsNestedHtml()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/nested"),
            Metadata: new TestFrontMatter { Title = "Nested" },
            Content: MakeContent("<div><p>Outer <strong>bold <em>italic</em></strong> text</p></div>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldBe("Outer bold italic text");
    }

    [Fact]
    public void Build_CollapsesWhitespace()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/whitespace"),
            Metadata: new TestFrontMatter { Title = "Whitespace" },
            Content: MakeContent("<p>Line one</p>\n\n<p>Line   two</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldNotContain("\n");
        doc.Body.ShouldNotContain("  "); // no double spaces
    }

    [Fact]
    public void Build_UsesCustomPriority()
    {
        var highPriorityBuilder = new SearchIndexBuilder(defaultPriority: 10);
        var item = new RenderedItem(
            Route: MakeRoute("/api/auth"),
            Metadata: new TestFrontMatter { Title = "Auth API" },
            Content: MakeContent("<p>Authentication</p>")
        );

        var doc = highPriorityBuilder.Build(item);

        doc.ShouldNotBeNull();
        doc.Priority.ShouldBe(10);
    }

    [Fact]
    public void Build_PassesLocaleFromRoute()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/docs/intro", "fr"),
            Metadata: new TestFrontMatter { Title = "Introduction" },
            Content: MakeContent("<p>Bienvenue</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Locale.ShouldBe("fr");
    }

    [Fact]
    public void Build_DefaultLocale_ReturnsEmptyString()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/docs/intro"),
            Metadata: new TestFrontMatter { Title = "Intro" },
            Content: MakeContent("<p>Welcome</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Locale.ShouldBe("");
    }

    // Front matter that only implements IFrontMatter — no capability interfaces
    private record MinimalFrontMatter(string Title) : IFrontMatter;

    [Fact]
    public void Build_NonDraftableFrontMatter_IsIncluded()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/page"),
            Metadata: new MinimalFrontMatter("Minimal"),
            Content: MakeContent("<p>Content</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Title.ShouldBe("Minimal");
        doc.Section.ShouldBeNull();
    }

    [Fact]
    public void Build_NonDraftableFrontMatter_SectionIsNull()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/basic"),
            Metadata: new MinimalFrontMatter("Basic"),
            Content: MakeContent("<p>No section</p>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Section.ShouldBeNull();
    }

    [Fact]
    public void Build_HtmlWithCodeBlocks_StripsCodeTags()
    {
        var item = new RenderedItem(
            Route: MakeRoute("/code"),
            Metadata: new TestFrontMatter { Title = "Code" },
            Content: MakeContent("<p>Use this:</p><pre><code class=\"language-csharp\">var x = 42;</code></pre>")
        );

        var doc = _builder.Build(item);

        doc.ShouldNotBeNull();
        doc.Body.ShouldContain("Use this:");
        doc.Body.ShouldContain("var x = 42;");
        doc.Body.ShouldNotContain("<code");
        doc.Body.ShouldNotContain("<pre>");
    }
}
