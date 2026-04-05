using Penn.Pipeline;
using Penn.FrontMatter;
using Penn.Routing;
using System.Collections.Immutable;

namespace Penn.Tests.Pipeline;

public class ContentItemTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static ContentSource MakeSource() =>
        new ContentSource(new MarkdownFileSource("content/page.md"));

    private static RenderedContent MakeRenderedContent() => new(
        Html: "<p>test</p>",
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        SearchDocument: null,
        Social: null
    );

    private static IFrontMatter MakeMetadata(string title = "Test Page") =>
        new DocFrontMatter { Title = title };

    [Fact]
    public void ExhaustivePatternMatch_AllFourCases()
    {
        ContentItem discovered = new ContentItem(new DiscoveredItem(MakeRoute("/a"), MakeSource()));
        ContentItem parsed = new ContentItem(new ParsedItem(MakeRoute("/b"), MakeMetadata(), "# Hi"));
        ContentItem rendered = new ContentItem(new RenderedItem(MakeRoute("/c"), MakeMetadata(), MakeRenderedContent()));
        ContentItem failed = new ContentItem(new FailedItem(MakeRoute("/d"), new ContentError("fail")));

        Describe(discovered).ShouldBe("Discovered: /a/");
        Describe(parsed).ShouldBe("Parsed: /b/");
        Describe(rendered).ShouldBe("Rendered: /c/");
        Describe(failed).ShouldBe("Failed: /d/");
    }

    private static string Describe(ContentItem item) => item switch
    {
        DiscoveredItem d => $"Discovered: {d.Route.CanonicalPath}",
        ParsedItem p     => $"Parsed: {p.Route.CanonicalPath}",
        RenderedItem r   => $"Rendered: {r.Route.CanonicalPath}",
        FailedItem f     => $"Failed: {f.Route.CanonicalPath}",
        _ => throw new InvalidOperationException("Unknown ContentItem case")
    };

    [Fact]
    public void FailedItem_ErrorMessage_PreservedThroughUnion()
    {
        var error = new ContentError("Parse error at line 42", new InvalidOperationException("bad syntax"));
        var failed = new FailedItem(MakeRoute("/broken"), error);
        var item = new ContentItem(failed);

        var recovered = item switch
        {
            FailedItem f => f,
            _ => null
        };

        recovered.ShouldNotBeNull();
        recovered.Error.Message.ShouldBe("Parse error at line 42");
        recovered.Error.Exception.ShouldNotBeNull();
        recovered.Error.Exception.Message.ShouldBe("bad syntax");
    }

    [Fact]
    public void DiscoveredItem_CarriesContentSourceCorrectly()
    {
        var source = new ContentSource(new MarkdownFileSource("docs/intro.md"));
        var discovered = new DiscoveredItem(MakeRoute(), source);
        var item = new ContentItem(discovered);

        var result = item switch
        {
            DiscoveredItem d => d.Source,
            _ => default
        };

        (result is MarkdownFileSource).ShouldBeTrue();
        var md = result switch
        {
            MarkdownFileSource m => m,
            _ => null
        };
        md.ShouldNotBeNull();
        md.Path.Value.ShouldBe("docs/intro.md");
    }

    [Fact]
    public void ParsedItem_CarriesFrontMatterAndRawMarkdown()
    {
        var metadata = new DocFrontMatter { Title = "My Article" };
        var rawMarkdown = "# My Article\n\nSome content here.";
        var parsed = new ParsedItem(MakeRoute(), metadata, rawMarkdown);
        var item = new ContentItem(parsed);

        var result = item switch
        {
            ParsedItem p => p,
            _ => null
        };

        result.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("My Article");
        result.RawMarkdown.ShouldBe("# My Article\n\nSome content here.");
    }

    [Fact]
    public void RenderedItem_CarriesRenderedContent()
    {
        var content = MakeRenderedContent();
        var rendered = new RenderedItem(MakeRoute(), MakeMetadata(), content);
        var item = new ContentItem(rendered);

        var result = item switch
        {
            RenderedItem r => r,
            _ => null
        };

        result.ShouldNotBeNull();
        result.Content.Html.ShouldBe("<p>test</p>");
        result.Content.Outline.ShouldBeEmpty();
        result.Content.Tags.ShouldBeEmpty();
        result.Content.CrossReferences.ShouldBeEmpty();
        result.Content.SearchDocument.ShouldBeNull();
        result.Content.Social.ShouldBeNull();
    }
}
