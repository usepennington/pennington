using Pennington.FrontMatter;
using Pennington.Markdown;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Markdown;

public class MarkdownContentParserTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        foreach (var (path, content) in files)
        {
            var dir = fs.Path.GetDirectoryName(path);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(path, content);
        }
        return fs;
    }

    [Fact]
    public async Task ParseAsync_MarkdownFileWithFrontMatter_ReturnsParsedItem()
    {
        var fs = CreateFs(("/content/hello.md",
            "---\ntitle: Hello World\n---\n# Hello\n\nSome content."));

        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/content/hello.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/hello"), source);

        var result = await parser.ParseAsync(discovered);

        var parsed = result.ShouldBeCase<ParsedItem>();
        parsed.Metadata.Title.ShouldBe("Hello World");
        parsed.RawMarkdown.ShouldContain("# Hello");
        parsed.RawMarkdown.ShouldContain("Some content.");
    }

    [Fact]
    public async Task ParseAsync_MissingFile_ReturnsFailedItem()
    {
        var fs = new MockFileSystem();
        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/nonexistent/path/file.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/missing"), source);

        var result = await parser.ParseAsync(discovered);

        var failed = result.ShouldBeCase<FailedItem>();
        failed.Error.Message.ShouldContain("Failed to parse");
        failed.Error.Exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ParseAsync_NonMarkdownSource_ReturnsFailedItem()
    {
        var fs = new MockFileSystem();
        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new RazorPageSource("SomePage"));
        var discovered = new DiscoveredItem(MakeRoute("/razor"), source);

        var result = await parser.ParseAsync(discovered);

        var failed = result.ShouldBeCase<FailedItem>();
        failed.Error.Message.ShouldContain("Unsupported content source type");
    }

    [Fact]
    public async Task ParseAsync_FileWithNoFrontMatter_ReturnsParsedItemWithDefaultMetadata()
    {
        var fs = CreateFs(("/content/plain.md",
            "# Just Markdown\n\nNo front matter here."));

        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/content/plain.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/plain"), source);

        var result = await parser.ParseAsync(discovered);

        var parsed = result.ShouldBeCase<ParsedItem>();
        parsed.Metadata.Title.ShouldBe("");
        parsed.RawMarkdown.ShouldContain("# Just Markdown");
        parsed.RawMarkdown.ShouldContain("No front matter here.");
    }

    [Fact]
    public async Task ParseAsync_DiscoveredItemCarriesCachedBody_ServesCacheWithoutReadingDisk()
    {
        // Simulates a file being rewritten mid-edit: on disk it is momentarily empty/truncated,
        // but the discovering service already cached a complete parse and carries it on the item.
        // The parser must serve the cache, not the half-written file — otherwise the page goes blank.
        var fs = CreateFs(("/content/hello.md", ""));

        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/content/hello.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/hello"), source)
        {
            Metadata = new DocFrontMatter { Title = "Cached Title" },
            RawBody = "# Cached body\n\nComplete content.",
        };

        var result = await parser.ParseAsync(discovered);

        var parsed = result.ShouldBeCase<ParsedItem>();
        parsed.Metadata.Title.ShouldBe("Cached Title");
        parsed.RawMarkdown.ShouldBe("# Cached body\n\nComplete content.");
    }

    [Fact]
    public async Task ParseAsync_CachedBodyWithoutMetadata_FallsBackToDisk()
    {
        // RawBody alone (no carried Metadata) isn't enough to short-circuit — the parser still
        // reads and parses the file so the front-matter type is produced correctly.
        var fs = CreateFs(("/content/hello.md",
            "---\ntitle: From Disk\n---\n# From disk"));

        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/content/hello.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/hello"), source) { RawBody = "stale" };

        var result = await parser.ParseAsync(discovered);

        var parsed = result.ShouldBeCase<ParsedItem>();
        parsed.Metadata.Title.ShouldBe("From Disk");
        parsed.RawMarkdown.ShouldContain("# From disk");
    }

    [Fact]
    public async Task ParseAsync_FileWithWindowsLineEndings_ParsesCorrectly()
    {
        var fs = CreateFs(("/content/windows.md",
            "---\r\ntitle: Windows File\r\n---\r\n# Hello\r\n\r\nWindows line endings."));

        var parser = new MarkdownContentParser<DocFrontMatter>(new FrontMatterParser(), fs);
        var source = new ContentSource(new FileSource(new FilePath("/content/windows.md"), "markdown"));
        var discovered = new DiscoveredItem(MakeRoute("/windows"), source);

        var result = await parser.ParseAsync(discovered);

        var parsed = result.ShouldBeCase<ParsedItem>();
        parsed.Metadata.Title.ShouldBe("Windows File");
    }
}