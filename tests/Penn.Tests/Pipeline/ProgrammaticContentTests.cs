using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Pipeline;

public class ProgrammaticContentTests
{
    [Fact]
    public void PatternMatch_TextCase()
    {
        var pc = new ProgrammaticContent(new TextProgrammaticContent(null, "raw content"));
        var result = pc switch
        {
            TextProgrammaticContent t => t.RawContent,
            BinaryProgrammaticContent _ => "binary",
        };
        result.ShouldBe("raw content");
    }

    [Fact]
    public void PatternMatch_BinaryCase()
    {
        var pc = new ProgrammaticContent(new BinaryProgrammaticContent(() => Task.FromResult(new byte[] { 0xFF }), "image/png"));
        var result = pc switch
        {
            TextProgrammaticContent _ => "text",
            BinaryProgrammaticContent b => b.ContentType,
        };
        result.ShouldBe("image/png");
    }

    [Fact]
    public void TextProgrammaticContent_DefaultContentType()
    {
        var text = new TextProgrammaticContent(null, "content");
        text.ContentType.ShouldBe("text/html");
    }

    [Fact]
    public void TextProgrammaticContent_CustomContentType()
    {
        var text = new TextProgrammaticContent(null, "# Heading", "text/markdown");
        text.ContentType.ShouldBe("text/markdown");
    }

    [Fact]
    public async Task StubGenerator_ReturnsTextContent()
    {
        var generator = new StubGenerator();
        var route = new ContentRoute
        {
            CanonicalPath = "/test/",
            OutputFile = "test/index.html"
        };

        var result = await generator.GenerateAsync(route);
        var text = result switch
        {
            TextProgrammaticContent t => t,
            _ => null
        };

        text.ShouldNotBeNull();
        text!.RawContent.ShouldBe("test");
        text.Metadata.ShouldBeNull();
    }

    private class StubGenerator : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
            => Task.FromResult(new ProgrammaticContent(new TextProgrammaticContent(null, "test")));
    }
}
