using AngleSharp;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class HtmlToMarkdownConverterTests
{
    private static readonly IBrowsingContext Context = BrowsingContext.New(Configuration.Default);

    private static string ConvertFragment(string innerHtml, Func<string, string>? rewriteHref = null)
    {
        var doc = Context.OpenAsync(req => req.Content($"<html><body>{innerHtml}</body></html>")).Result;
        return HtmlToMarkdownConverter.Convert(doc.Body!, rewriteHref);
    }

    [Fact]
    public void Headings_EmitHashPrefixes()
    {
        var md = ConvertFragment("<h1>One</h1><h2>Two</h2><h3>Three</h3>");

        md.ShouldContain("# One");
        md.ShouldContain("## Two");
        md.ShouldContain("### Three");
    }

    [Fact]
    public void Paragraph_EmitsTextWithBlankLine()
    {
        var md = ConvertFragment("<p>Hello world</p><p>Second paragraph</p>");

        md.ShouldContain("Hello world");
        md.ShouldContain("Second paragraph");
        // Paragraphs separated by blank line
        md.ShouldContain("Hello world\n\nSecond paragraph");
    }

    [Fact]
    public void UnorderedList_EmitsDashBullets()
    {
        var md = ConvertFragment("<ul><li>Alpha</li><li>Beta</li><li>Gamma</li></ul>");

        md.ShouldContain("- Alpha");
        md.ShouldContain("- Beta");
        md.ShouldContain("- Gamma");
    }

    [Fact]
    public void OrderedList_EmitsNumberedItems()
    {
        var md = ConvertFragment("<ol><li>First</li><li>Second</li><li>Third</li></ol>");

        md.ShouldContain("1. First");
        md.ShouldContain("2. Second");
        md.ShouldContain("3. Third");
    }

    [Fact]
    public void Link_EmitsMarkdownLink()
    {
        var md = ConvertFragment("<p>See <a href=\"/docs\">the docs</a> for details.</p>");

        md.ShouldContain("[the docs](/docs)");
    }

    [Fact]
    public void AnchorOnlyLink_EmitsPlainText()
    {
        var md = ConvertFragment("<p>Go to <a href=\"#section\">this section</a>.</p>");

        md.ShouldContain("this section");
        md.ShouldNotContain("](#section)");
    }

    [Fact]
    public void InlineCode_EmitsBacktickPair()
    {
        var md = ConvertFragment("<p>Run <code>dotnet build</code> first.</p>");

        md.ShouldContain("`dotnet build`");
    }

    [Fact]
    public void FencedCodeBlock_EmitsLanguageFromClass()
    {
        var md = ConvertFragment(
            "<pre><code class=\"language-csharp\">var x = 42;</code></pre>");

        md.ShouldContain("```csharp");
        md.ShouldContain("var x = 42;");
        md.ShouldContain("```");
    }

    [Fact]
    public void FencedCodeBlock_NoLanguage_EmitsBarefenced()
    {
        var md = ConvertFragment("<pre><code>plain text</code></pre>");

        md.ShouldContain("```\nplain text");
    }

    [Fact]
    public void BoldAndItalic_EmitsAsterisks()
    {
        var md = ConvertFragment("<p><strong>bold</strong> and <em>italic</em></p>");

        md.ShouldContain("**bold**");
        md.ShouldContain("*italic*");
    }

    [Fact]
    public void Image_EmitsMarkdownImage()
    {
        var md = ConvertFragment("<p><img src=\"/logo.png\" alt=\"Logo\"></p>");

        md.ShouldContain("![Logo](/logo.png)");
    }

    [Fact]
    public void Blockquote_PrefixesLinesWithGt()
    {
        var md = ConvertFragment("<blockquote><p>Quoted text</p></blockquote>");

        md.ShouldContain("> Quoted text");
    }

    [Fact]
    public void HorizontalRule_EmitsTripleDash()
    {
        var md = ConvertFragment("<p>Before</p><hr><p>After</p>");

        md.ShouldContain("---");
    }

    [Fact]
    public void ScriptAndStyleTags_AreIgnored()
    {
        var md = ConvertFragment(
            "<p>Real content</p><script>alert('x')</script><style>.foo{}</style>");

        md.ShouldContain("Real content");
        md.ShouldNotContain("alert");
        md.ShouldNotContain(".foo");
    }

    [Fact]
    public void NestedStructure_RendersAllLevels()
    {
        var md = ConvertFragment(
            "<article><h1>Title</h1><p>Intro with <a href=\"/link\">link</a>.</p>" +
            "<ul><li>Item with <code>inline</code></li></ul></article>");

        md.ShouldContain("# Title");
        md.ShouldContain("[link](/link)");
        md.ShouldContain("- Item with `inline`");
    }

    [Fact]
    public void Output_EndsWithSingleNewline()
    {
        var md = ConvertFragment("<p>content</p>");

        md.ShouldEndWith("\n");
        md.ShouldNotEndWith("\n\n");
    }

    [Fact]
    public void ConsecutiveBlankLines_Collapsed()
    {
        var md = ConvertFragment("<p>A</p><p>B</p><p>C</p>");

        md.ShouldNotContain("\n\n\n");
    }

    [Fact]
    public void Link_WithRewriter_UsesRewrittenHref()
    {
        var md = ConvertFragment(
            "<p><a href=\"/docs/foo/\">Foo</a></p>",
            href => href == "/docs/foo/" ? "_llms/docs/foo.md" : href);

        md.ShouldContain("[Foo](_llms/docs/foo.md)");
        md.ShouldNotContain("[Foo](/docs/foo/)");
    }

    [Fact]
    public void Link_WithRewriter_PassesOriginalHrefIncludingQueryAndFragment()
    {
        string? captured = null;
        ConvertFragment(
            "<p><a href=\"/docs/bar/?q=1#sec\">Bar</a></p>",
            href => { captured = href; return href; });

        captured.ShouldBe("/docs/bar/?q=1#sec");
    }

    [Fact]
    public void Link_WithoutRewriter_EmitsOriginalHref()
    {
        var md = ConvertFragment("<p><a href=\"/docs/baz/\">Baz</a></p>");

        md.ShouldContain("[Baz](/docs/baz/)");
    }

    [Fact]
    public void Link_AnchorOnly_NotPassedToRewriter()
    {
        var rewriterCalled = false;
        var md = ConvertFragment(
            "<p><a href=\"#section\">Section</a></p>",
            href => { rewriterCalled = true; return href; });

        rewriterCalled.ShouldBeFalse();
        md.ShouldContain("Section");
        md.ShouldNotContain("](#section)");
    }

    [Fact]
    public void HumansOnlyElement_IsStrippedWithSubtree()
    {
        var md = ConvertFragment(
            "<p>Before</p>" +
            "<div class=\"humans-only\"><p>Interactive widget</p><p>More widget</p></div>" +
            "<p>After</p>");

        md.ShouldContain("Before");
        md.ShouldContain("After");
        md.ShouldNotContain("Interactive widget");
        md.ShouldNotContain("More widget");
    }

    [Fact]
    public void RobotsOnlyElement_IsKept()
    {
        var md = ConvertFragment(
            "<p>Before</p>" +
            "<div class=\"robots-only\"><p>Full signature detail</p></div>" +
            "<p>After</p>");

        md.ShouldContain("Before");
        md.ShouldContain("Full signature detail");
        md.ShouldContain("After");
    }

    [Fact]
    public void HumansOnlyClass_AlongsideOtherClasses_StillStrips()
    {
        var md = ConvertFragment(
            "<p>Visible</p>" +
            "<div class=\"prose humans-only mt-4\"><p>Hidden from llms</p></div>");

        md.ShouldContain("Visible");
        md.ShouldNotContain("Hidden from llms");
    }

    [Fact]
    public void CodeHighlightWrapper_RecoversLanguageFromDataAttribute()
    {
        // Pennington's syntax-highlighted code blocks lose the original Markdig fence
        // info-string when TextMate tokenizes — the language is preserved on the wrapper
        // div as data-language so the converter can put it back on the markdown fence.
        var md = ConvertFragment(
            "<div class=\"code-highlight-wrapper not-prose\" data-language=\"csharp\">" +
                "<div class=\"standalone-code-container\">" +
                    "<div class=\"standalone-code-highlight\">" +
                        "<pre><code><span class=\"line\"><span class=\"hljs-keyword\">var</span> x = <span class=\"hljs-number\">42</span>;</span></code></pre>" +
                    "</div>" +
                "</div>" +
            "</div>");

        md.ShouldContain("```csharp\n");
        md.ShouldContain("var x = 42;");
        md.ShouldContain("\n```");
        md.ShouldNotContain("hljs");
        md.ShouldNotContain("class=");
    }

    [Fact]
    public void CodeHighlightWrapper_StripsLineSpansToTextContent()
    {
        var md = ConvertFragment(
            "<div class=\"code-highlight-wrapper\" data-language=\"python\">" +
                "<pre><code><span class=\"line\">def foo():</span>\n<span class=\"line\">    return 42</span></code></pre>" +
            "</div>");

        md.ShouldContain("```python\n");
        md.ShouldContain("def foo():");
        md.ShouldContain("    return 42");
    }

    [Fact]
    public void CodeHighlightWrapper_NoLanguageData_EmitsBareFence()
    {
        var md = ConvertFragment(
            "<div class=\"code-highlight-wrapper\">" +
                "<pre><code>plain</code></pre>" +
            "</div>");

        md.ShouldContain("```\nplain");
    }

    [Fact]
    public void MarkdownAlert_EmitsGfmAdmonition()
    {
        var md = ConvertFragment(
            "<div class=\"markdown-alert markdown-alert-warning\">" +
                "<p class=\"markdown-alert-title\">Warning</p>" +
                "<p>Something dangerous.</p>" +
            "</div>");

        md.ShouldContain("> [!WARNING]");
        md.ShouldContain("> Something dangerous.");
        // The title paragraph "Warning" is encoded in the marker line, not duplicated as body.
        md.IndexOf("Warning\n").ShouldBe(-1);
    }

    [Fact]
    public void MarkdownAlert_PreservesMultilineBody()
    {
        var md = ConvertFragment(
            "<div class=\"markdown-alert markdown-alert-note\">" +
                "<p class=\"markdown-alert-title\">Note</p>" +
                "<p>First sentence.</p>" +
                "<p>Second sentence.</p>" +
            "</div>");

        md.ShouldContain("> [!NOTE]");
        md.ShouldContain("> First sentence.");
        md.ShouldContain("> Second sentence.");
    }

    [Fact]
    public void TabbedCodeBlock_EmitsAllTabsAsH3Sections()
    {
        // Multi-language tabs (e.g. C# / F# / VB) — emit every variant so an LLM sees
        // each option, never just the first tab.
        var md = ConvertFragment(
            "<div class=\"not-prose\">" +
                "<div class=\"tab-container\">" +
                    "<div role=\"tablist\">" +
                        "<button role=\"tab\" id=\"btn-1-0\">C#</button>" +
                        "<button role=\"tab\" id=\"btn-1-1\">F#</button>" +
                    "</div>" +
                    "<div aria-labelledby=\"btn-1-0\" class=\"tab-panel\">" +
                        "<div class=\"code-highlight-wrapper\" data-language=\"csharp\"><pre><code>var x = 1;</code></pre></div>" +
                    "</div>" +
                    "<div aria-labelledby=\"btn-1-1\" class=\"tab-panel\">" +
                        "<div class=\"code-highlight-wrapper\" data-language=\"fsharp\"><pre><code>let x = 1</code></pre></div>" +
                    "</div>" +
                "</div>" +
            "</div>");

        md.ShouldContain("### C#");
        md.ShouldContain("### F#");
        md.ShouldContain("```csharp");
        md.ShouldContain("var x = 1;");
        md.ShouldContain("```fsharp");
        md.ShouldContain("let x = 1");
        // Verify ordering: C# section appears before F# section.
        md.IndexOf("### C#").ShouldBeLessThan(md.IndexOf("### F#"));
    }

    [Fact]
    public void FencedCodeBlock_ContentWithTripleBackticks_EscalatesFenceLength()
    {
        // Content that already contains ``` would close a 3-backtick fence early.
        // The emitter must use a longer fence (one more than the longest backtick run).
        var md = ConvertFragment(
            "<pre><code class=\"language-md\">Here is a fence:\n```\ncode\n```</code></pre>");

        md.ShouldContain("````md\n");
        md.ShouldContain("\n````\n");
        md.ShouldContain("```\ncode\n```");
    }

    [Fact]
    public void CodeHighlightWrapper_ContentWithTripleBackticks_EscalatesFenceLength()
    {
        var md = ConvertFragment(
            "<div class=\"code-highlight-wrapper\" data-language=\"md\">" +
                "<pre><code>Example:\n```\nnested\n```</code></pre>" +
            "</div>");

        md.ShouldContain("````md\n");
        md.ShouldContain("\n````\n");
        md.ShouldContain("```\nnested\n```");
    }

    [Fact]
    public void InlineCode_ContentWithBacktick_UsesDoubleBackticksWithPadding()
    {
        var md = ConvertFragment("<p>The token <code>`raw`</code> stays.</p>");

        // Content contains backticks at both ends, so emitter must escalate to ``
        // and pad with spaces inside the delimiters.
        md.ShouldContain("`` `raw` ``");
    }

    [Fact]
    public void InlineCode_ContentStartingWithBacktick_PadsWithSpace()
    {
        var md = ConvertFragment("<p>See <code>`leading</code>.</p>");

        md.ShouldContain("`` `leading ``");
    }
}