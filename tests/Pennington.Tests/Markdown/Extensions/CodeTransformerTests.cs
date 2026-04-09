using Pennington.Markdown.Extensions;

namespace Pennington.Tests.Markdown.Extensions;

public class CodeTransformerTests
{
    [Fact]
    public void Transform_WrapsLinesInSpanLineElements()
    {
        var html = "<pre><code>line one\nline two\nline three</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("<span class=\"line\">");
        result.ShouldContain("line one");
        result.ShouldContain("line two");
        result.ShouldContain("line three");
    }

    [Fact]
    public void Transform_AppliesHighlightDirective()
    {
        var html = "<pre><code>normal line\nhighlighted line // [!code highlight]</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("highlight");
        result.ShouldContain("has-highlighted");
        // The directive comment should be removed from the output
        result.ShouldNotContain("[!code highlight]");
    }

    [Fact]
    public void Transform_AppliesDiffAddRemove()
    {
        var html = "<pre><code>added line // [!code ++]\nremoved line // [!code --]</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("diff-add");
        result.ShouldContain("diff-remove");
        result.ShouldContain("has-diff");
        result.ShouldNotContain("[!code ++]");
        result.ShouldNotContain("[!code --]");
    }

    [Fact]
    public void Transform_RemovesDirectiveCommentsFromOutput()
    {
        var html = "<pre><code>var x = 1; // [!code highlight]\nvar y = 2; # [!code focus]</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldNotContain("[!code highlight]");
        result.ShouldNotContain("[!code focus]");
    }

    [Fact]
    public void Transform_EmptyInput_ReturnsInput()
    {
        var result = CodeTransformerAccessor.Transform("");

        result.ShouldBe("");
    }

    [Fact]
    public void Transform_NoDirectives_StillWrapsInLines()
    {
        var html = "<pre><code>line one\nline two</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("<span class=\"line\">");
    }
}

/// <summary>
/// Accessor to invoke the internal CodeTransformer from tests.
/// </summary>
internal static class CodeTransformerAccessor
{
    public static string Transform(string highlightedHtml)
    {
        // Use reflection to access the internal static method
        var type = typeof(Pennington.Markdown.Extensions.CodeHighlightRenderOptions).Assembly
            .GetType("Pennington.Markdown.Extensions.CodeTransformer")!;
        var method = type.GetMethod("Transform",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
        return (string)method.Invoke(null, [highlightedHtml])!;
    }
}
