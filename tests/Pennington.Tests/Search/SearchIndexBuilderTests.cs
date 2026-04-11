using Pennington.Content;
using Pennington.Routing;
using Pennington.Search;

namespace Pennington.Tests.Search;

public class SearchIndexBuilderTests
{
    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale
    };

    private static ContentTocItem MakeToc(string title, string path, string? section = null, string locale = "") =>
        new(
            Title: title,
            Route: MakeRoute(path, locale),
            Order: 0,
            HierarchyParts: [],
            Section: section,
            Locale: string.IsNullOrEmpty(locale) ? null : locale
        );

    private readonly SearchIndexBuilder _builder = new();

    [Fact]
    public void Build_ReturnsDocument_WithTitleBodyUrlAndLocale()
    {
        var toc = MakeToc("Introduction", "/docs/intro", locale: "en");

        var doc = _builder.Build(toc, "<p>Welcome to the docs</p>");

        doc.Title.ShouldBe("Introduction");
        doc.Body.ShouldBe("Welcome to the docs");
        doc.Url.ShouldBe("/docs/intro/");
        doc.Locale.ShouldBe("en");
        doc.Priority.ShouldBe(5);
    }

    [Fact]
    public void Build_StripsHtmlFromBody()
    {
        var doc = _builder.Build(MakeToc("Page", "/page"), "<p>Hello <strong>world</strong></p>");

        doc.Body.ShouldBe("Hello world");
    }

    [Fact]
    public void Build_IncludesSectionFromToc()
    {
        var doc = _builder.Build(MakeToc("Auth", "/api/auth", section: "api"), "<p>body</p>");

        doc.Section.ShouldBe("api");
    }

    [Fact]
    public void Build_DecodesHtmlEntities()
    {
        var doc = _builder.Build(
            MakeToc("Entities", "/entities"),
            "<p>Tom &amp; Jerry &lt;3 &gt; others &quot;said&quot; he&#39;s&nbsp;right</p>");

        doc.Body.ShouldBe("Tom & Jerry <3 > others \"said\" he's right");
    }

    [Fact]
    public void Build_StripsNestedHtml()
    {
        var doc = _builder.Build(
            MakeToc("Nested", "/nested"),
            "<div><p>Outer <strong>bold <em>italic</em></strong> text</p></div>");

        doc.Body.ShouldBe("Outer bold italic text");
    }

    [Fact]
    public void Build_CollapsesWhitespace()
    {
        var doc = _builder.Build(
            MakeToc("Whitespace", "/whitespace"),
            "<p>Line one</p>\n\n<p>Line   two</p>");

        doc.Body.ShouldNotContain("\n");
        doc.Body.ShouldNotContain("  "); // no double spaces
    }

    [Fact]
    public void Build_HtmlWithCodeBlocks_StripsCodeTags()
    {
        var doc = _builder.Build(
            MakeToc("Code", "/code"),
            "<p>Use this:</p><pre><code class=\"language-csharp\">var x = 42;</code></pre>");

        doc.Body.ShouldContain("Use this:");
        doc.Body.ShouldContain("var x = 42;");
        doc.Body.ShouldNotContain("<code");
        doc.Body.ShouldNotContain("<pre>");
    }
}
