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
}