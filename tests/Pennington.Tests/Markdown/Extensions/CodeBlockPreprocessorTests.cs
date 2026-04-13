using Markdig;
using Markdig.Renderers;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Highlighting;
using Pennington.Markdown;
using Pennington.Markdown.Extensions;

namespace Pennington.Tests.Markdown.Extensions;

public class CodeBlockPreprocessorTests
{
    private static string RenderMarkdown(
        string markdown,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors = null)
    {
        var highlightingService = new HighlightingService([]);
        using var sp = new ServiceCollection().BuildServiceProvider();
        var pipeline = MarkdownPipelineFactory.CreateWithExtensions(
            sp,
            highlightingService,
            preprocessors: preprocessors);

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);

        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    [Fact]
    public void Preprocessor_InterceptsMatchingCodeBlock()
    {
        var preprocessor = new TestPreprocessor("test:special",
            new CodeBlockPreprocessResult(
                "<pre><code><span class=\"preprocessed\">custom output</span></code></pre>",
                "test"));

        var markdown = "```test:special\nsome code\n```";

        var result = RenderMarkdown(markdown, [preprocessor]);

        result.ShouldContain("preprocessed");
        result.ShouldContain("custom output");
    }

    [Fact]
    public void Preprocessor_ReturnsNull_PassesThrough()
    {
        var preprocessor = new TestPreprocessor("test:special", null);

        var markdown = "```csharp\nvar x = 1;\n```";

        var result = RenderMarkdown(markdown, [preprocessor]);

        // Normal highlighting should run (plain text fallback since no highlighters registered)
        result.ShouldContain("var x = 1;");
        result.ShouldNotContain("preprocessed");
    }

    [Fact]
    public void HigherPriorityPreprocessor_RunsFirst()
    {
        var lowPriority = new TestPreprocessor("test:special",
            new CodeBlockPreprocessResult(
                "<pre><code>low priority</code></pre>", "test"),
            priority: 10);

        var highPriority = new TestPreprocessor("test:special",
            new CodeBlockPreprocessResult(
                "<pre><code>high priority</code></pre>", "test"),
            priority: 100);

        var markdown = "```test:special\nsome code\n```";

        var result = RenderMarkdown(markdown, [lowPriority, highPriority]);

        result.ShouldContain("high priority");
        result.ShouldNotContain("low priority");
    }

    [Fact]
    public void SkipTransform_PreventsCodeTransformer()
    {
        // CodeTransformer wraps lines in <span class="line"> elements.
        // With SkipTransform=true, the output should be used as-is.
        var preprocessor = new TestPreprocessor("test:skip",
            new CodeBlockPreprocessResult(
                "<pre><code>raw output line</code></pre>",
                "test",
                SkipTransform: true));

        var markdown = "```test:skip\nsome code\n```";

        var result = RenderMarkdown(markdown, [preprocessor]);

        result.ShouldContain("raw output line");
        // With SkipTransform, CodeTransformer is not called, so no <span class="line"> wrapping
        result.ShouldNotContain("<span class=\"line\">");
    }

    private class TestPreprocessor(
        string matchLanguageId,
        CodeBlockPreprocessResult? result,
        int priority = 50) : ICodeBlockPreprocessor
    {
        public int Priority => priority;

        public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
        {
            return languageId == matchLanguageId ? result : null;
        }
    }
}
