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
    public void Transform_StripsHtmlBlockCommentShellAroundDirective()
    {
        var html = "<pre><code>&lt;LangVersion&gt;preview&lt;/LangVersion&gt; &lt;!-- [!code ++] --&gt;</code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("diff-add");
        result.ShouldNotContain("[!code ++]");
        result.ShouldNotContain("&lt;!--");
        result.ShouldNotContain("--&gt;");
    }

    [Fact]
    public void Transform_PreservesTrailingPunctuationTokenWhenRemovingDirective()
    {
        // A trailing `;` is a real punctuation token, not an orphaned comment marker.
        // Mirrors the highlighter's span structure for `return a + b; // [!code highlight]`.
        var html = "<pre><code>    <span class=\"hljs-keyword\">return</span> " +
                   "<span class=\"hljs-variable\">a</span> <span class=\"hljs-operator\">+</span> " +
                   "<span class=\"hljs-variable\">b</span><span class=\"hljs-punctuation\">;</span> " +
                   "<span class=\"hljs-comment\">// [!code highlight]</span></code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("has-highlighted");
        result.ShouldNotContain("[!code highlight]");
        result.ShouldContain(";"); // the statement terminator must survive
    }

    [Fact]
    public void Transform_PreservesWhitespaceBetweenSameClassTokensWhenRemovingDirective()
    {
        // `public` and `int` are both hljs-keyword separated by a space. Merging same-class
        // spans must not drop the space (regression: `public int` collapsed to `publicint`).
        var html = "<pre><code><span class=\"hljs-keyword\">public</span> " +
                   "<span class=\"hljs-keyword\">int</span> <span class=\"hljs-title\">Multiply</span> " +
                   "<span class=\"hljs-comment\">// [!code ++]</span></code></pre>";

        var result = CodeTransformerAccessor.Transform(html);

        result.ShouldContain("has-diff");
        result.ShouldNotContain("[!code ++]");
        result.ShouldNotContain("publicint");
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