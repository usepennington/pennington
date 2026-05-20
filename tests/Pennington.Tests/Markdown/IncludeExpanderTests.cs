using Pennington.Markdown;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Markdown;

public class IncludeExpanderTests
{
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
    public void Expand_NoDirective_ReturnsInputUnchanged()
    {
        var fs = CreateFs(("/content/page.md", ""));

        var result = IncludeExpander.Expand("# Title\n\nBody.", new FilePath("/content/page.md"), fs);

        result.ShouldBe("# Title\n\nBody.");
    }

    [Fact]
    public void Expand_BlockInclude_SplicesReferencedFile()
    {
        var fs = CreateFs(
            ("/content/page.md", "Before.\n\n[!INCLUDE [shared](partials/note.md)]\n\nAfter."),
            ("/content/partials/note.md", "Shared **note** body."));

        var result = IncludeExpander.Expand(
            "Before.\n\n[!INCLUDE [shared](partials/note.md)]\n\nAfter.",
            new FilePath("/content/page.md"), fs);

        result.ShouldBe("Before.\n\nShared **note** body.\n\nAfter.");
    }

    [Fact]
    public void Expand_InlineInclude_SplicesWithinSentence()
    {
        var fs = CreateFs(("/content/snippet.md", "the latest release"));

        var result = IncludeExpander.Expand(
            "Install [!INCLUDE [v](snippet.md)] today.",
            new FilePath("/content/page.md"), fs);

        result.ShouldBe("Install the latest release today.");
    }

    [Fact]
    public void Expand_NestedIncludes_ExpandsRecursively()
    {
        var fs = CreateFs(
            ("/content/outer.md", "[!INCLUDE [mid](mid.md)]"),
            ("/content/mid.md", "mid + [!INCLUDE [inner](inner.md)]"),
            ("/content/inner.md", "leaf"));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [mid](mid.md)]", new FilePath("/content/outer.md"), fs);

        result.ShouldBe("mid + leaf");
    }

    [Fact]
    public void Expand_RelativePathResolution_ResolvesAgainstSourceFile()
    {
        var fs = CreateFs(("/content/shared/intro.md", "intro text"));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [i](../shared/intro.md)]",
            new FilePath("/content/guides/page.md"), fs);

        result.ShouldBe("intro text");
    }

    [Fact]
    public void Expand_MissingFile_EmitsComment()
    {
        var fs = CreateFs(("/content/page.md", ""));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [x](missing.md)]", new FilePath("/content/page.md"), fs);

        result.ShouldBe("<!-- Pennington: include not found: missing.md -->");
    }

    [Fact]
    public void Expand_DirectCycle_BreaksWithComment()
    {
        var fs = CreateFs(
            ("/content/a.md", "[!INCLUDE [b](b.md)]"),
            ("/content/b.md", "[!INCLUDE [a](a.md)]"));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [b](b.md)]", new FilePath("/content/a.md"), fs);

        // The re-entered directive (b.md, already on the stack) collapses to a comment.
        result.ShouldBe("<!-- Pennington: include cycle broken: b.md -->");
    }

    [Fact]
    public void Expand_IncludedFileFrontMatter_IsStripped()
    {
        var fs = CreateFs(
            ("/content/partial.md", "---\ntitle: Partial\n---\nVisible body."));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [p](partial.md)]", new FilePath("/content/page.md"), fs);

        result.ShouldBe("Visible body.");
    }

    [Fact]
    public void Expand_AbsoluteUrl_IsSkipped()
    {
        var fs = CreateFs(("/content/page.md", ""));

        var result = IncludeExpander.Expand(
            "[!INCLUDE [u](https://example.com/x.md)]", new FilePath("/content/page.md"), fs);

        result.ShouldContain("include skipped");
    }

    [Fact]
    public void Expand_DirectiveInFencedCodeBlock_LeftVerbatim()
    {
        var fs = CreateFs(("/content/partial.md", "spliced body"));

        var markdown =
            "Real one:\n\n[!INCLUDE [p](partial.md)]\n\n" +
            "Documented syntax:\n\n```markdown\n[!INCLUDE [p](partial.md)]\n```\n";

        var result = IncludeExpander.Expand(markdown, new FilePath("/content/page.md"), fs);

        result.ShouldContain("Real one:\n\nspliced body");
        // The fenced occurrence survives so the syntax can be documented.
        result.ShouldContain("```markdown\n[!INCLUDE [p](partial.md)]\n```");
    }

    [Fact]
    public void Expand_DirectiveIsCaseInsensitive()
    {
        var fs = CreateFs(("/content/p.md", "body"));

        var result = IncludeExpander.Expand(
            "[!include [p](p.md)]", new FilePath("/content/page.md"), fs);

        result.ShouldBe("body");
    }
}