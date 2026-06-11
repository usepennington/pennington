namespace Pennington.Book.Tests;

using System.Collections.Immutable;
using System.IO.Abstractions;
using Markdig;
using Microsoft.Extensions.FileProviders;
using Pennington.Book;
using Pennington.Book.Composition;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Markdown;
using Pennington.Navigation;
using Pennington.Routing;
using Testably.Abstractions.Testing;
using static Pennington.Book.Tests.BookTestBuilders;

public sealed class BookComposerTests
{
    private static BookComposer Composer(
        string siteTitle = "Test Site",
        TranslationOptions? translations = null,
        PenningtonOptions? penn = null)
        => new(
            MarkdownPipelineFactory.CreateDefault(),
            new CanonicalBaseUrl(new UrlPath("https://example.com")),
            penn ?? new PenningtonOptions { SiteTitle = siteTitle },
            translations ?? new TranslationOptions(),
            new LocalizationOptions());

    // A pipeline with Markdig's alert blocks so round-tripped `> [!CHECKPOINT]` produces a real
    // (renderer-untitled) alert box, exercising the composer's title backfill.
    private static BookComposer AlertComposer()
        => new(
            new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAlertBlocks().Build(),
            new CanonicalBaseUrl(new UrlPath("https://example.com")),
            new PenningtonOptions { SiteTitle = "Test Site" },
            new TranslationOptions(),
            new LocalizationOptions());

    private static BookStamp Stamp(string? version = "1.2.3", string? locale = null)
        => new(version, new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero), locale);

    private static BookDefinition Book(string? subtitle = null)
        => new("My Book", "/") { Subtitle = subtitle };

    private static ImmutableList<NavigationTreeItem> Tree(params NavigationTreeItem[] nodes) => [.. nodes];

    [Fact]
    public void Cover_leads_with_the_site_title_and_uses_the_book_title_as_subtitle()
    {
        var html = Composer(siteTitle: "Pennington").Compose(
            Book(subtitle: "A subtitle"),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s);

        var cover = Parse(html).QuerySelector("section.book-cover")!;
        cover.QuerySelector(".book-cover-title")!.TextContent.ShouldBe("Pennington");   // big site title
        cover.QuerySelector(".book-cover-subtitle")!.TextContent.ShouldBe("My Book");    // section as subtitle
        cover.QuerySelector(".book-cover-tagline")!.TextContent.ShouldBe("A subtitle");
        cover.QuerySelectorAll("h1,h2,h3,h4,h5,h6").Length.ShouldBe(0);                   // divs keep the PDF outline clean
    }

    [Fact]
    public void Whole_site_book_omits_the_redundant_section_subtitle()
    {
        // The whole-site book's title IS the site title, so there is no separate section to show.
        var html = Composer(siteTitle: "Pennington").Compose(
            new BookDefinition("Pennington", "/") { Subtitle = "A content engine for .NET" },
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s);

        var cover = Parse(html).QuerySelector("section.book-cover")!;
        cover.QuerySelector(".book-cover-title")!.TextContent.ShouldBe("Pennington");
        cover.QuerySelector(".book-cover-subtitle").ShouldBeNull();
        cover.QuerySelector(".book-cover-tagline")!.TextContent.ShouldBe("A content engine for .NET");
    }

    [Fact]
    public void Top_level_nodes_are_chapters_and_nested_nodes_are_pages()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("Chapter", "/c/", Node("Child", "/c/child/"))),
            PageMap(Page("/c/", "<p>c</p>"), Page("/c/child/", "<p>child</p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        var chapter = doc.QuerySelector("section.book-chapter#p--c")!;
        chapter.QuerySelector("h1")!.TextContent.ShouldBe("Chapter");

        var child = doc.QuerySelector("section.book-page#p--c-child")!;
        child.QuerySelector("h2")!.TextContent.ShouldBe("Child");
    }

    [Fact]
    public void Drops_the_source_h1_and_emits_the_nav_title()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("Nav Title", "/a/")),
            PageMap(Page("/a/", "<h2>Real Section</h2><p>body</p>", title: "Original H1")),
            additionalCss: null,
            s => s);

        var section = Parse(html).QuerySelector("section#p--a")!;
        section.QuerySelector("h1")!.TextContent.ShouldBe("Nav Title");
        section.TextContent.ShouldNotContain("Original H1");
        section.QuerySelector("h2")!.TextContent.ShouldBe("Real Section");
    }

    [Fact]
    public void Demotes_content_headings_by_nav_depth()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("Chapter", "/c/", Node("Child", "/c/child/"))),
            PageMap(Page("/c/", "<p/>"), Page("/c/child/", "<h2>Sub</h2>")),
            additionalCss: null,
            s => s);

        var child = Parse(html).QuerySelector("section#p--c-child")!;
        child.QuerySelector("h2")!.TextContent.ShouldBe("Child");   // page title at depth 1
        child.QuerySelector("h3")!.TextContent.ShouldBe("Sub");     // content h2 demoted by 1
    }

    [Fact]
    public void Caps_heading_demotion_at_h6()
    {
        var deep = Node("A", "/a/",
            Node("B", "/a/b/",
                Node("C", "/a/b/c/",
                    Node("D", "/a/b/c/d/",
                        Node("E", "/a/b/c/d/e/")))));

        var html = Composer().Compose(
            Book(),
            Tree(deep),
            PageMap(Page("/a/b/c/d/e/", "<h2>X</h2><h3>Y</h3>")),
            additionalCss: null,
            s => s);

        var e = Parse(html).QuerySelector("section#p--a-b-c-d-e")!;
        e.QuerySelector("h5")!.TextContent.ShouldBe("E");   // page title at depth 4
        e.QuerySelectorAll("h6").Length.ShouldBe(2);        // 2+4 and 3+4 both clamp to 6
    }

    [Fact]
    public void Rewrites_in_book_page_fragment_and_out_of_book_links()
    {
        var x = Page("/x/", """
            <p>
            <a href="/x/y/">to y</a>
            <a href="/other/">out</a>
            <a href="/x/y/#install">frag</a>
            </p>
            """);

        var html = Composer().Compose(
            Book(),
            Tree(Node("X", "/x/", Node("Y", "/x/y/"))),
            PageMap(x, Page("/x/y/", "<p>y body</p>")),
            additionalCss: null,
            s => s);

        var hrefs = Parse(html).QuerySelector("section#p--x")!
            .QuerySelectorAll("a")
            .Select(a => a.GetAttribute("href"))
            .ToList();

        hrefs.ShouldContain("#p--x-y");
        hrefs.ShouldContain("#x-y--install");
        hrefs.ShouldContain("https://example.com/other/");
    }

    [Fact]
    public void Table_of_contents_is_nested_with_section_anchors()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("Chapter", "/c/", Node("Child", "/c/child/"))),
            PageMap(Page("/c/", "<p/>"), Page("/c/child/", "<p/>")),
            additionalCss: null,
            s => s);

        var toc = Parse(html).QuerySelector("nav.book-toc")!;
        var top = toc.QuerySelector("ol > li > a")!;
        top.GetAttribute("href").ShouldBe("#p--c");
        top.QuerySelector(".toc-title")!.TextContent.ShouldBe("Chapter");

        var nested = toc.QuerySelector("ol > li > ol > li > a")!;
        nested.GetAttribute("href").ShouldBe("#p--c-child");
    }

    [Fact]
    public void Prefixes_content_ids_with_the_page_slug()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<h2>Install Steps</h2>")),
            additionalCss: null,
            s => s);

        var section = Parse(html).QuerySelector("section#p--a")!;
        section.QuerySelector("#a--install-steps").ShouldNotBeNull();
    }

    [Fact]
    public void Inlines_images_through_the_resolver()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory(@"C:\site\img");
        fs.File.WriteAllBytes(@"C:\site\img\p.png", new byte[] { 1, 2, 3 });
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["img/p.png"] = @"C:\site\img\p.png",
        };
        var inliner = new AssetInliner(fs, new NullFileProvider(), new CanonicalBaseUrl(new UrlPath("https://example.com")));

        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p><img src=\"/img/p.png\" alt=\"x\"></p>")),
            additionalCss: null,
            s => inliner.Resolve(s, map));

        var img = Parse(html).QuerySelector("section#p--a img")!;
        img.GetAttribute("src").ShouldStartWith("data:image/png;base64,");
    }

    [Fact]
    public void Neutralizes_code_fence_directives_so_resolved_code_is_not_reprocessed()
    {
        // A tree-sitter :symbol fence renders to a wrapper carrying data-language="csharp:symbol"
        // and already-resolved code. Recovering that directive verbatim would make the re-render
        // re-run the :symbol preprocessor over the resolved lines. The directive must be trimmed to
        // the base language so the recovered fence is a plain ```csharp block.
        var page = Page("/a/",
            "<div class=\"code-highlight-wrapper\" data-language=\"csharp:symbol\">"
            + "<pre><code><span class=\"line\">var x = 1;</span>\n<span class=\"line\">var y = 2;</span></code></pre>"
            + "</div>");

        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(page),
            additionalCss: null,
            s => s);

        var section = Parse(html).QuerySelector("section#p--a")!;
        section.InnerHtml.ShouldNotContain(":symbol");
        section.TextContent.ShouldContain("var x = 1;");
        section.QuerySelector("pre").ShouldNotBeNull();
    }

    [Fact]
    public void Backfills_a_label_for_a_checkpoint_alert_the_renderer_leaves_untitled()
    {
        // The <Checkpoint> component round-trips through `> [!CHECKPOINT]`, which the renderer emits
        // as a checkpoint box with an empty title slot. The composer must give it a "Checkpoint" label.
        var page = Page("/a/",
            "<div class=\"markdown-alert markdown-alert-checkpoint not-prose\">"
            + "<p class=\"markdown-alert-title\">Checkpoint</p>"
            + "<ul><li>It works</li></ul></div>");

        var html = AlertComposer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(page),
            additionalCss: null,
            s => s);

        var alert = Parse(html).QuerySelector("section#p--a .markdown-alert-checkpoint")!;
        alert.QuerySelector(".markdown-alert-title")!.TextContent.ShouldBe("Checkpoint");
        alert.TextContent.ShouldContain("It works");
    }

    [Fact]
    public void Inlines_paged_js_with_config_before_the_polyfill_and_appends_css()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: ".custom{color:red}",
            s => s);

        html.ShouldContain(".custom{color:red}");
        html.ShouldContain("window.PagedConfig");
        html.ShouldContain("__pagedDone");
        html.IndexOf("PagedConfig", StringComparison.Ordinal)
            .ShouldBeLessThan(html.IndexOf("Paged.js v0.4.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Monochrome_appends_the_grayscale_override_after_book_css_and_before_additional_css()
    {
        // Without the flag the color palette stands and the override is absent.
        var color = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s);

        color.ShouldContain("--book-accent: #2b6cb0");   // book.css color accent
        color.ShouldNotContain("--book-accent: #333");   // grayscale override not emitted

        var mono = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: ".custom{color:red}",
            s => s,
            monochrome: true);

        mono.ShouldContain("--book-accent: #333");        // grayscale override emitted
        // Cascade order: book.css, then the grayscale override, then the caller's CSS (which still wins).
        mono.IndexOf("--book-accent: #2b6cb0", StringComparison.Ordinal)
            .ShouldBeLessThan(mono.IndexOf("--book-accent: #333", StringComparison.Ordinal));
        mono.IndexOf("--book-accent: #333", StringComparison.Ordinal)
            .ShouldBeLessThan(mono.IndexOf(".custom{color:red}", StringComparison.Ordinal));
    }

    [Fact]
    public void No_stamp_omits_the_colophon_and_version_line_and_defaults_lang_to_en()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        doc.QuerySelector(".book-colophon").ShouldBeNull();
        doc.QuerySelector(".book-cover-version").ShouldBeNull();
        doc.DocumentElement.GetAttribute("lang").ShouldBe("en");
    }

    [Fact]
    public void Stamp_adds_a_cover_version_line_and_a_colophon_between_the_cover_and_the_toc()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp());

        var doc = Parse(html);
        doc.QuerySelector(".book-cover-version")!.TextContent.ShouldBe("Version 1.2.3");

        // Front matter reads cover → colophon → TOC, with the first chapter directly after.
        doc.Body!.Children.Select(c => c.ClassName)
            .ShouldBe(["book-cover", "book-colophon", "book-toc", "book-chapter"]);
    }

    [Fact]
    public void Colophon_lists_version_date_url_and_the_pennington_credit()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp());

        var colophon = Parse(html).QuerySelector("section.book-colophon")!;
        var text = colophon.TextContent;
        text.ShouldContain("Version 1.2.3");
        text.ShouldContain("June 2026");
        text.ShouldContain("https://example.com");
        // The core version changes per commit, so assert only the credit itself.
        text.ShouldContain("Produced with Pennington");
        colophon.QuerySelectorAll("h1,h2,h3,h4,h5,h6").Length.ShouldBe(0);   // divs keep the PDF outline clean
    }

    [Fact]
    public void Colophon_includes_the_author_copyright_only_when_configured()
    {
        var penn = new PenningtonOptions { SiteTitle = "Test Site", StructuredDataAuthorName = "Phil Scott" };
        var withAuthor = Composer(penn: penn).Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp());

        Parse(withAuthor).QuerySelector(".book-colophon")!.TextContent.ShouldContain("© 2026 Phil Scott");

        var withoutAuthor = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp());

        Parse(withoutAuthor).QuerySelector(".book-colophon")!.TextContent.ShouldNotContain("©");
    }

    [Fact]
    public void Null_version_keeps_the_colophon_but_drops_the_version_lines()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp(version: null));

        var doc = Parse(html);
        doc.QuerySelector(".book-cover-version").ShouldBeNull();
        var colophon = doc.QuerySelector(".book-colophon")!;
        colophon.ShouldNotBeNull();
        colophon.TextContent.ShouldNotContain("Version");
    }

    [Fact]
    public void Chapter_labels_are_numbered_in_document_order_and_only_on_chapters()
    {
        var html = Composer().Compose(
            Book(),
            Tree(
                Node("One", "/one/", Node("Sub", "/one/sub/")),
                Node("Two", "/two/")),
            PageMap(Page("/one/", "<p/>"), Page("/one/sub/", "<p/>"), Page("/two/", "<p/>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        doc.QuerySelectorAll("section.book-chapter > .book-chapter-label")
            .Select(l => l.TextContent)
            .ShouldBe(["Chapter 1", "Chapter 2"]);
        doc.QuerySelectorAll("section.book-page .book-chapter-label").Length.ShouldBe(0);

        // The eyebrow precedes the chapter heading.
        Parse(html).QuerySelector("section#p--one")!.FirstElementChild!.ClassName.ShouldBe("book-chapter-label");
    }

    [Fact]
    public void Chapter_label_and_contents_heading_come_from_translations()
    {
        var translations = new TranslationOptions();
        translations.Add("fr", BookComposer.ChapterLabelKey, "Chapitre {0}");
        translations.Add("fr", BookComposer.ContentsKey, "Sommaire");

        var html = Composer(translations: translations).Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp(locale: "fr"));

        var doc = Parse(html);
        doc.QuerySelector(".book-chapter-label")!.TextContent.ShouldBe("Chapitre 1");
        doc.QuerySelector(".book-toc-heading")!.TextContent.ShouldBe("Sommaire");

        // Without registered translations the English defaults stand.
        var english = Parse(Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s));
        english.QuerySelector(".book-chapter-label")!.TextContent.ShouldBe("Chapter 1");
        english.QuerySelector(".book-toc-heading")!.TextContent.ShouldBe("Contents");
    }

    [Fact]
    public void Colophon_date_is_formatted_for_the_stamp_locale()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp(locale: "fr"));

        var text = Parse(html).QuerySelector(".book-colophon")!.TextContent;
        text.ShouldContain("juin");
        text.ShouldContain("2026");
    }

    [Fact]
    public void Html_lang_attribute_comes_from_the_stamp_locale()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("A", "/a/")),
            PageMap(Page("/a/", "<p>hi</p>")),
            additionalCss: null,
            s => s,
            stamp: Stamp(locale: "fr"));

        Parse(html).DocumentElement.GetAttribute("lang").ShouldBe("fr");
    }

    [Fact]
    public void Unwraps_a_single_area_root_into_chapters_with_an_unnumbered_introduction()
    {
        // A per-area book scopes to one root node — the area index — wrapping everything else.
        var html = Composer().Compose(
            new BookDefinition("Getting Started", "/tutorials/"),
            Tree(Node("Tutorials", "/tutorials/",
                Node("Getting Started", "/tutorials/getting-started/"),
                Node("Docsite", "/tutorials/docsite/"))),
            PageMap(
                Page("/tutorials/", "<p>welcome</p>"),
                Page("/tutorials/getting-started/", "<p>gs</p>"),
                Page("/tutorials/docsite/", "<p>ds</p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);

        // The area's sections are the chapters, each promoted to h1 with its own label.
        var chapters = doc.QuerySelectorAll("section.book-chapter").ToList();
        chapters.Select(c => c.Id).ShouldBe(["p--tutorials", "p--tutorials-getting-started", "p--tutorials-docsite"]);
        doc.QuerySelectorAll(".book-chapter-label").Select(l => l.TextContent).ShouldBe(["Chapter 1", "Chapter 2"]);
        doc.QuerySelector("section#p--tutorials-getting-started > h1")!.TextContent.ShouldBe("Getting Started");

        // The area landing becomes an unnumbered introduction with its own content, no eyebrow.
        var introSection = doc.QuerySelector("section#p--tutorials")!;
        introSection.QuerySelector(".book-chapter-label").ShouldBeNull();
        introSection.TextContent.ShouldContain("welcome");

        // TOC: intro entry first and unnumbered, chapters numbered from 1.
        var entries = doc.QuerySelectorAll("nav.book-toc > ol > li > a").ToList();
        entries.Select(a => a.QuerySelector(".toc-title")!.TextContent)
            .ShouldBe(["Tutorials", "Getting Started", "Docsite"]);
        entries[0].QuerySelector(".toc-chapter-number").ShouldBeNull();
        entries[1].QuerySelector(".toc-chapter-number")!.TextContent.ShouldBe("1");
        entries[2].QuerySelector(".toc-chapter-number")!.TextContent.ShouldBe("2");
    }

    [Fact]
    public void Unwrap_drops_an_introduction_without_content_and_absolutizes_links_to_it()
    {
        // No page for /tutorials/ itself: the chapters stand alone and a link to the area
        // landing leaves the book instead of pointing at a dead anchor.
        var html = Composer().Compose(
            new BookDefinition("Getting Started", "/tutorials/"),
            Tree(Node("Tutorials", "/tutorials/",
                Node("Getting Started", "/tutorials/getting-started/"))),
            PageMap(Page("/tutorials/getting-started/", "<p><a href=\"/tutorials/\">up</a></p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        doc.QuerySelector("section#p--tutorials").ShouldBeNull();
        doc.QuerySelectorAll("nav.book-toc .toc-title").Select(t => t.TextContent)
            .ShouldBe(["Getting Started"]);
        doc.QuerySelector(".book-chapter-label")!.TextContent.ShouldBe("Chapter 1");
        doc.QuerySelector("section#p--tutorials-getting-started a")!
            .GetAttribute("href").ShouldBe("https://example.com/tutorials/");
    }

    [Fact]
    public void Unwraps_a_single_section_root_without_a_page_of_its_own()
    {
        // The common per-area shape: the wrapper is a bare folder section (no route), and the
        // chapters beneath it are sections too.
        var html = Composer().Compose(
            new BookDefinition("Getting Started", "/tutorials/"),
            Tree(Section("Tutorials",
                Section("Getting Started", Node("First site", "/tutorials/getting-started/first-site/")),
                Section("Docsite", Node("Scaffold", "/tutorials/docsite/scaffold/")))),
            PageMap(
                Page("/tutorials/getting-started/first-site/", "<p>fs</p>"),
                Page("/tutorials/docsite/scaffold/", "<p>sc</p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        doc.QuerySelectorAll(".book-chapter-label").Select(l => l.TextContent).ShouldBe(["Chapter 1", "Chapter 2"]);
        doc.QuerySelectorAll("section.book-chapter > h1").Select(h => h.TextContent)
            .ShouldBe(["Getting Started", "Docsite"]);

        // The pageless wrapper leaves no trace: no section, no TOC entry.
        doc.QuerySelectorAll("nav.book-toc > ol > li > a .toc-title").Select(t => t.TextContent)
            .ShouldBe(["Getting Started", "Docsite"]);
        doc.Body!.TextContent.ShouldNotContain("Tutorials");
    }

    [Fact]
    public void Does_not_unwrap_a_childless_root_that_matches_the_prefix()
    {
        var html = Composer().Compose(
            new BookDefinition("Getting Started", "/tutorials/"),
            Tree(Node("Tutorials", "/tutorials/")),
            PageMap(Page("/tutorials/", "<p>welcome</p>")),
            additionalCss: null,
            s => s);

        var doc = Parse(html);
        var chapter = doc.QuerySelector("section.book-chapter#p--tutorials")!;
        chapter.QuerySelector(".book-chapter-label")!.TextContent.ShouldBe("Chapter 1");
        doc.QuerySelector("nav.book-toc .toc-chapter-number")!.TextContent.ShouldBe("1");
    }

    [Fact]
    public void Toc_numbers_top_level_entries_only()
    {
        var html = Composer().Compose(
            Book(),
            Tree(Node("Chapter", "/c/", Node("Child", "/c/child/"))),
            PageMap(Page("/c/", "<p/>"), Page("/c/child/", "<p/>")),
            additionalCss: null,
            s => s);

        var toc = Parse(html).QuerySelector("nav.book-toc")!;
        toc.QuerySelector("ol > li > a .toc-chapter-number")!.TextContent.ShouldBe("1");
        toc.QuerySelector("ol ol .toc-chapter-number").ShouldBeNull();
    }
}
