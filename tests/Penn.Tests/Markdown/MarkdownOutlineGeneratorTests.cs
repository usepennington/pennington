using Markdig;
using Markdig.Syntax;
using Penn.Markdown;

namespace Penn.Tests.Markdown;

public class MarkdownOutlineGeneratorTests
{
    private static MarkdownPipeline Pipeline => MarkdownPipelineFactory.CreateDefault();

    private static MarkdownDocument ParseDocument(string markdown)
        => Markdig.Markdown.Parse(markdown, Pipeline);

    [Fact]
    public void GenerateOutline_MultipleHeadingLevels_ExtractsAll()
    {
        var doc = ParseDocument("## Overview\n\nText.\n\n### Details\n\nMore text.\n\n## Conclusion\n\nFinal.");

        var outline = MarkdownOutlineGenerator.GenerateOutline(doc);

        outline.Length.ShouldBe(3);
        outline[0].Text.ShouldBe("Overview");
        outline[0].Level.ShouldBe(2);
        outline[0].Id.ShouldNotBeNullOrWhiteSpace();
        outline[1].Text.ShouldBe("Details");
        outline[1].Level.ShouldBe(3);
        outline[2].Text.ShouldBe("Conclusion");
        outline[2].Level.ShouldBe(2);
    }

    [Fact]
    public void GenerateOutline_HeadingWithoutAutoId_IsSkipped()
    {
        // Use a pipeline without auto-identifiers to verify headings without IDs are skipped
        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
        var doc = Markdig.Markdown.Parse("# No ID Heading\n\nContent.", pipeline);

        var outline = MarkdownOutlineGenerator.GenerateOutline(doc);

        outline.ShouldBeEmpty();
    }

    [Fact]
    public void GenerateOutline_EmptyDocument_ReturnsEmptyArray()
    {
        var doc = ParseDocument("");

        var outline = MarkdownOutlineGenerator.GenerateOutline(doc);

        outline.ShouldBeEmpty();
    }

    [Fact]
    public void GenerateOutline_InlineFormatting_ExtractsPlainText()
    {
        var doc = ParseDocument("## Hello **bold** and `code` world");

        var outline = MarkdownOutlineGenerator.GenerateOutline(doc);

        outline.Length.ShouldBe(1);
        outline[0].Text.ShouldBe("Hello bold and code world");
        outline[0].Level.ShouldBe(2);
    }

    [Fact]
    public void GenerateOutline_FlatNotHierarchical()
    {
        var doc = ParseDocument("## Parent\n\n### Child\n\n#### Grandchild");

        var outline = MarkdownOutlineGenerator.GenerateOutline(doc);

        // All entries are flat — no nesting
        outline.Length.ShouldBe(3);
        outline[0].Level.ShouldBe(2);
        outline[1].Level.ShouldBe(3);
        outline[2].Level.ShouldBe(4);
    }
}
