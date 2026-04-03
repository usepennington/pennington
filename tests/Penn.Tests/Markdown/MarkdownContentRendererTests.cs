using Penn.FrontMatter;
using Penn.Markdown;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Markdown;

public class MarkdownContentRendererTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static IFrontMatter MakeMetadata(string title = "Test") =>
        new DocFrontMatter { Title = title };

    private readonly MarkdownContentRenderer _renderer = new();

    [Fact]
    public async Task RenderAsync_SimpleMarkdown_ProducesHtml()
    {
        var parsed = new ParsedItem(MakeRoute("/hello"), MakeMetadata(), "# Hello\n\nWorld");

        var result = await _renderer.RenderAsync(parsed);

        (result is RenderedItem).ShouldBeTrue();
        var rendered = result switch { RenderedItem r => r, _ => null };
        rendered.ShouldNotBeNull();
        rendered.Content.Html.ShouldContain("<h1");
        rendered.Content.Html.ShouldContain("Hello");
        rendered.Content.Html.ShouldContain("<p>World</p>");
    }

    [Fact]
    public async Task RenderAsync_HeadingsExtractedToOutline()
    {
        var markdown = "## Getting Started\n\nIntro text.\n\n### Installation\n\nInstall steps.";
        var parsed = new ParsedItem(MakeRoute("/docs"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        (result is RenderedItem).ShouldBeTrue();
        var rendered = result switch { RenderedItem r => r, _ => null };
        rendered.ShouldNotBeNull();
        rendered.Content.Outline.Length.ShouldBe(2);
        rendered.Content.Outline[0].Text.ShouldBe("Getting Started");
        rendered.Content.Outline[0].Level.ShouldBe(2);
        rendered.Content.Outline[0].Id.ShouldNotBeNullOrWhiteSpace();
        rendered.Content.Outline[1].Text.ShouldBe("Installation");
        rendered.Content.Outline[1].Level.ShouldBe(3);
        rendered.Content.Outline[1].Id.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RenderAsync_EmptyMarkdown_ProducesEmptyHtml()
    {
        var parsed = new ParsedItem(MakeRoute("/empty"), MakeMetadata(), "");

        var result = await _renderer.RenderAsync(parsed);

        (result is RenderedItem).ShouldBeTrue();
        var rendered = result switch { RenderedItem r => r, _ => null };
        rendered.ShouldNotBeNull();
        rendered.Content.Html.Trim().ShouldBeEmpty();
        rendered.Content.Outline.ShouldBeEmpty();
    }

    [Fact]
    public async Task RenderAsync_PreservesMetadataAndRoute()
    {
        var route = MakeRoute("/preserve");
        var metadata = MakeMetadata("Preserved Title");
        var parsed = new ParsedItem(route, metadata, "Some content");

        var result = await _renderer.RenderAsync(parsed);

        (result is RenderedItem).ShouldBeTrue();
        var rendered = result switch { RenderedItem r => r, _ => null };
        rendered.ShouldNotBeNull();
        rendered.Route.ShouldBe(route);
        rendered.Metadata.Title.ShouldBe("Preserved Title");
    }
}
