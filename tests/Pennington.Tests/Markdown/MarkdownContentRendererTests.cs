using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Markdown;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

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
    public async Task RenderAsync_WithResolver_RewritesRelativeMdLink()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.Directory.CreateDirectory("/content/tutorials");
        fs.Directory.CreateDirectory("/content/how-to");
        fs.File.WriteAllText("/content/tutorials/intro.md", "---\ntitle: Intro\n---\nbody");
        fs.File.WriteAllText("/content/how-to/panels.md", "---\ntitle: Panels\n---\nbody");

        var service = new MarkdownContentService<DocFrontMatter>(
            new MarkdownContentServiceOptions
            {
                ContentPath = new FilePath("/content"),
                BasePageUrl = new UrlPath("/console"),
            },
            new FrontMatterParser(), fs, new FileWatcher(fs), new LocalizationOptions());

        var resolver = new MarkdownLinkResolver([service]);
        var renderer = new MarkdownContentRenderer(linkResolver: resolver);

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/console/tutorials/intro/"),
            OutputFile = new FilePath("console/tutorials/intro/index.html"),
            SourceFile = new FilePath("/content/tutorials/intro.md"),
        };
        var parsed = new ParsedItem(route, MakeMetadata(),
            "See [panels](../how-to/panels.md) for details.");

        var result = await renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("href=\"/console/how-to/panels/\"");
        rendered.Content.Html.ShouldNotContain(".md\"");
    }

    [Fact]
    public async Task RenderAsync_WithResolver_RewritesRelativeImageSrc()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.Directory.CreateDirectory("/content/getting-started");
        fs.File.WriteAllText("/content/getting-started/index.md", "---\ntitle: GS\n---\nbody");

        var service = new MarkdownContentService<DocFrontMatter>(
            new MarkdownContentServiceOptions
            {
                ContentPath = new FilePath("/content"),
                BasePageUrl = new UrlPath("/"),
            },
            new FrontMatterParser(), fs, new FileWatcher(fs), new LocalizationOptions());

        var resolver = new MarkdownLinkResolver([service]);
        var renderer = new MarkdownContentRenderer(linkResolver: resolver);

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/getting-started/"),
            OutputFile = new FilePath("getting-started/index.html"),
            SourceFile = new FilePath("/content/getting-started/index.md"),
        };
        var parsed = new ParsedItem(route, MakeMetadata(),
            "![Arch](./beacon-arch.png)");

        var result = await renderer.RenderAsync(parsed);

        var rendered = result.ShouldBeCase<RenderedItem>();
        rendered.Content.Html.ShouldContain("src=\"/getting-started/beacon-arch.png\"");
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
