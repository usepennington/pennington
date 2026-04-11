using Pennington.FrontMatter;
using Pennington.Markdown;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Markdown;

public class MarkdownContentRendererTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
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

        var rendered = result.ShouldBeCase<RenderedItem>();
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

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Outline.Length.ShouldBe(2);
        rendered.Content.Outline[0].Text.ShouldBe("Getting Started");
        rendered.Content.Outline[0].Level.ShouldBe(2);
        rendered.Content.Outline[0].Id.ShouldNotBeNullOrWhiteSpace();
        rendered.Content.Outline[1].Text.ShouldBe("Installation");
        rendered.Content.Outline[1].Level.ShouldBe(3);
        rendered.Content.Outline[1].Id.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RenderAsync_FencedCodeBlock_ProducesPreCodeElements()
    {
        var markdown = "```csharp\nvar x = 42;\n```";
        var parsed = new ParsedItem(MakeRoute("/code"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("<pre>");
        rendered.Content.Html.ShouldContain("<code");
        rendered.Content.Html.ShouldContain("var x = 42;");
    }

    [Fact]
    public async Task RenderAsync_Links_ProducesAnchorTags()
    {
        var markdown = "Visit [Pennington docs](/docs/getting-started) for more info.";
        var parsed = new ParsedItem(MakeRoute("/links"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("<a");
        rendered.Content.Html.ShouldContain("href=\"/docs/getting-started\"");
        rendered.Content.Html.ShouldContain("Pennington docs");
    }

    [Fact]
    public async Task RenderAsync_Images_ProducesImgTags()
    {
        var markdown = "![Logo](/images/logo.png)";
        var parsed = new ParsedItem(MakeRoute("/images"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("<img");
        rendered.Content.Html.ShouldContain("src=\"/images/logo.png\"");
        rendered.Content.Html.ShouldContain("alt=\"Logo\"");
    }

    [Fact]
    public async Task RenderAsync_MultipleHeadingLevels_AllAppearInOutline()
    {
        var markdown = "## Overview\n\nText.\n\n## Installation\n\n### Prerequisites\n\n### Steps\n\n## Usage";
        var parsed = new ParsedItem(MakeRoute("/multi"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Outline.Length.ShouldBe(5);
        rendered.Content.Outline.Count(e => e.Level == 2).ShouldBe(3);
        rendered.Content.Outline.Count(e => e.Level == 3).ShouldBe(2);
    }

    [Fact]
    public async Task RenderAsync_ComplexDocument_LinksImagesCodeAndHeadings()
    {
        var markdown = """
            ## Getting Started

            Install [Pennington](https://github.com/example/pennington) by running:

            ```bash
            dotnet add package Pennington
            ```

            ![Architecture](/images/architecture.png)

            ### Configuration

            | Option | Default |
            |--------|---------|
            | Title  | "Site"  |
            """;
        var parsed = new ParsedItem(MakeRoute("/complex"), MakeMetadata(), markdown);

        var result = await _renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("<a");
        rendered.Content.Html.ShouldContain("<pre>");
        rendered.Content.Html.ShouldContain("<img");
        rendered.Content.Html.ShouldContain("<table");
        rendered.Content.Outline.Length.ShouldBeGreaterThanOrEqualTo(2);
    }

}
