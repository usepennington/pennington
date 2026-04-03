using Penn.FrontMatter;
using Penn.Markdown;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Markdown;

public class MarkdownContentParserTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private readonly MarkdownContentParser<DocFrontMatter> _parser = new(new FrontMatterParser());

    [Fact]
    public async Task ParseAsync_MarkdownFileWithFrontMatter_ReturnsParsedItem()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile,
                "---\ntitle: Hello World\n---\n# Hello\n\nSome content.",
                TestContext.Current.CancellationToken);

            var source = new ContentSource(new MarkdownFileSource(new FilePath(tempFile)));
            var discovered = new DiscoveredItem(MakeRoute("/hello"), source);

            var result = await _parser.ParseAsync(discovered);

            (result is ParsedItem).ShouldBeTrue();
            var parsed = result switch { ParsedItem p => p, _ => null };
            parsed.ShouldNotBeNull();
            parsed.Metadata.Title.ShouldBe("Hello World");
            parsed.RawMarkdown.ShouldContain("# Hello");
            parsed.RawMarkdown.ShouldContain("Some content.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseAsync_MissingFile_ReturnsFailedItem()
    {
        var source = new ContentSource(new MarkdownFileSource(new FilePath("/nonexistent/path/file.md")));
        var discovered = new DiscoveredItem(MakeRoute("/missing"), source);

        var result = await _parser.ParseAsync(discovered);

        (result is FailedItem).ShouldBeTrue();
        var failed = result switch { FailedItem f => f, _ => null };
        failed.ShouldNotBeNull();
        failed.Error.Message.ShouldContain("Failed to parse");
        failed.Error.Exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ParseAsync_NonMarkdownSource_ReturnsFailedItem()
    {
        var source = new ContentSource(new RazorPageSource("SomePage"));
        var discovered = new DiscoveredItem(MakeRoute("/razor"), source);

        var result = await _parser.ParseAsync(discovered);

        (result is FailedItem).ShouldBeTrue();
        var failed = result switch { FailedItem f => f, _ => null };
        failed.ShouldNotBeNull();
        failed.Error.Message.ShouldContain("Unsupported content source type");
    }

    [Fact]
    public async Task ParseAsync_FileWithNoFrontMatter_ReturnsParsedItemWithDefaultMetadata()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile,
                "# Just Markdown\n\nNo front matter here.",
                TestContext.Current.CancellationToken);

            var source = new ContentSource(new MarkdownFileSource(new FilePath(tempFile)));
            var discovered = new DiscoveredItem(MakeRoute("/plain"), source);

            var result = await _parser.ParseAsync(discovered);

            (result is ParsedItem).ShouldBeTrue();
            var parsed = result switch { ParsedItem p => p, _ => null };
            parsed.ShouldNotBeNull();
            parsed.Metadata.Title.ShouldBe("");
            parsed.RawMarkdown.ShouldContain("# Just Markdown");
            parsed.RawMarkdown.ShouldContain("No front matter here.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
