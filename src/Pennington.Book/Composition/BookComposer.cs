namespace Pennington.Book.Composition;

using System.Collections.Immutable;
using System.Globalization;
using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Composes one self-contained book document from a navigation tree and the projected pages under it.
/// <para>
/// Each page is normalized exactly like llms.txt — its post-pipeline HTML is converted to markdown
/// (<see cref="HtmlToMarkdownConverter"/>) and re-rendered through the shared
/// <see cref="MarkdownPipeline"/>, which re-highlights fences and normalizes alerts/tabs regardless of
/// which content service produced the page. The result is wrapped into chapter/page sections, prefixed
/// with a cover and a page-numbered table of contents, and emitted with the print stylesheet and the
/// paged.js polyfill inlined so the document renders offline (no server, no external assets).
/// </para>
/// </summary>
public sealed class BookComposer
{
    /// <summary>Translation key for the chapter eyebrow label; the format argument is the chapter number.</summary>
    internal const string ChapterLabelKey = "pennington.book.chapter";

    /// <summary>Translation key for the table-of-contents heading.</summary>
    internal const string ContentsKey = "pennington.book.contents";

    private const string DefaultChapterLabel = "Chapter {0}";
    private const string DefaultContents = "Contents";

    private static readonly string BookCss = LoadResource("book.css");
    private static readonly string BookBwCss = LoadResource("book-bw.css");
    private static readonly string PagedPolyfill = LoadResource("paged.polyfill.js");

    private readonly MarkdownPipeline _pipeline;
    private readonly CanonicalBaseUrl _canonicalBase;
    private readonly PenningtonOptions _penn;
    private readonly TranslationOptions _translations;
    private readonly LocalizationOptions _localization;
    private readonly HtmlParser _parser = new();

    /// <summary>Creates a composer that re-renders through <paramref name="pipeline"/>, absolutizes out-of-book links against <paramref name="canonicalBase"/>, titles the cover from <paramref name="penn"/>, and localizes book chrome strings through <paramref name="translations"/> for the locale resolved against <paramref name="localization"/>.</summary>
    public BookComposer(
        MarkdownPipeline pipeline,
        CanonicalBaseUrl canonicalBase,
        PenningtonOptions penn,
        TranslationOptions translations,
        LocalizationOptions localization)
    {
        _pipeline = pipeline;
        _canonicalBase = canonicalBase;
        _penn = penn;
        _translations = translations;
        _localization = localization;
    }

    /// <summary>
    /// Composes the full HTML document for <paramref name="book"/> from <paramref name="tree"/> (already
    /// scoped to the book) and <paramref name="pageByPath"/> (the projected pages keyed by trimmed
    /// canonical path). A tree wrapped in the book's own index node is unwrapped first
    /// (<see cref="BookScoping.UnwrapBookRoot"/>) so the index's children become the chapters.
    /// When <paramref name="monochrome"/> is set, a grayscale override stylesheet is
    /// appended after the built-in one; <paramref name="additionalCss"/> is appended last (so it still
    /// wins), and <paramref name="resolveImageSrc"/> inlines image sources to <c>data:</c> URIs.
    /// <paramref name="stamp"/> supplies provenance (version, date, locale) for the cover version line
    /// and the colophon page; when null both are omitted and the document language falls back to the
    /// default locale.
    /// </summary>
    public string Compose(
        BookDefinition book,
        ImmutableList<NavigationTreeItem> tree,
        IReadOnlyDictionary<string, RenderedPage> pageByPath,
        string? additionalCss,
        Func<string, string> resolveImageSrc,
        bool monochrome = false,
        BookStamp? stamp = null)
    {
        // A per-area book scopes to a single root — the area index — which would otherwise become
        // the book's only chapter. Unwrap it: its children are the chapters, its own page content
        // (when it has any) an unnumbered introduction. An intro without content is dropped
        // entirely, so links to it absolutize to the live site instead of a dead anchor.
        var (intro, chapters) = BookScoping.UnwrapBookRoot(tree, book);
        if (intro is not null && !HasComposableContent(intro, pageByPath))
        {
            intro = null;
        }

        // Assign a unique slug per node; both the TOC anchors and the section ids reference it.
        var slugByNode = new Dictionary<NavigationTreeItem, string>(ReferenceEqualityComparer.Instance);
        var usedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AssignSlugs(intro is null ? chapters : [intro, .. chapters], slugByNode, usedSlugs);

        // Canonical path -> slug for in-book links; everything else is external or out-of-book.
        var inBookSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (node, slug) in slugByNode)
        {
            var path = NormalizePath(node.Route.CanonicalPath.Value);
            if (!string.IsNullOrEmpty(path))
            {
                inBookSlugs[path] = slug;
            }
        }

        var rewriteHref = BuildHrefRewriter(inBookSlugs);

        // The cover leads with the site title; the book's own title is the section/area beneath it.
        // A whole-site book takes its title from the site, so the two coincide and there is no subtitle.
        var siteTitle = string.IsNullOrWhiteSpace(_penn.SiteTitle) ? book.Title : _penn.SiteTitle;
        var documentTitle = string.Equals(book.Title, siteTitle, StringComparison.Ordinal)
            ? siteTitle
            : $"{siteTitle} — {book.Title}";

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html>\n<html lang=\"").Append(Encode(ResolveLang(stamp?.Locale))).Append("\">\n<head>\n<meta charset=\"utf-8\">\n");
        sb.Append("<title>").Append(Encode(documentTitle)).Append("</title>\n");
        sb.Append("<style>\n").Append(BookCss);
        if (monochrome)
        {
            sb.Append('\n').Append(BookBwCss);
        }

        if (!string.IsNullOrWhiteSpace(additionalCss))
        {
            sb.Append('\n').Append(additionalCss);
        }

        sb.Append("\n</style>\n");
        // PagedConfig must precede the polyfill so the auto-run picks up the readiness callback.
        sb.Append("<script>window.PagedConfig = { auto: true, after: () => { window.__pagedDone = true; } };</script>\n");
        sb.Append("<script>\n").Append(PagedPolyfill).Append("\n</script>\n");
        sb.Append("</head>\n<body>\n");

        AppendCover(sb, siteTitle, book, stamp?.Version);

        // Front matter reads cover → colophon → TOC; the print stylesheet styles all of it
        // with the roman-folio `front` named page.
        if (stamp is not null)
        {
            AppendColophon(sb, stamp);
        }

        AppendToc(sb, intro, chapters, slugByNode, stamp?.Locale);

        if (intro is not null)
        {
            RenderNode(sb, intro, depth: 0, slugByNode, pageByPath, rewriteHref, resolveImageSrc);
        }

        var chapterTemplate = Translate(stamp?.Locale, ChapterLabelKey, DefaultChapterLabel);
        var chapterNumber = 0;
        foreach (var node in chapters)
        {
            var chapterLabel = string.Format(CultureInfo.InvariantCulture, chapterTemplate, ++chapterNumber);
            RenderNode(sb, node, depth: 0, slugByNode, pageByPath, rewriteHref, resolveImageSrc, chapterLabel);
        }

        sb.Append("</body>\n</html>\n");
        return sb.ToString();
    }

    private static void AssignSlugs(
        ImmutableList<NavigationTreeItem> nodes,
        Dictionary<NavigationTreeItem, string> slugByNode,
        HashSet<string> used)
    {
        foreach (var node in nodes)
        {
            var basis = NormalizePath(node.Route.CanonicalPath.Value);
            var slug = !string.IsNullOrEmpty(basis) ? basis.Replace('/', '-') : Slugify(node.Title);
            if (string.IsNullOrEmpty(slug))
            {
                slug = "section";
            }

            var unique = slug;
            var n = 2;
            while (!used.Add(unique))
            {
                unique = $"{slug}-{n++}";
            }

            slugByNode[node] = unique;
            AssignSlugs(node.Children, slugByNode, used);
        }
    }

    private void RenderNode(
        StringBuilder sb,
        NavigationTreeItem node,
        int depth,
        Dictionary<NavigationTreeItem, string> slugByNode,
        IReadOnlyDictionary<string, RenderedPage> pageByPath,
        Func<string, string> rewriteHref,
        Func<string, string> resolveImageSrc,
        string? chapterLabel = null)
    {
        var level = Math.Min(depth + 1, 6);
        var slug = slugByNode[node];
        var sectionClass = depth == 0 ? "book-chapter" : "book-page";

        sb.Append("<section class=\"").Append(sectionClass).Append("\" id=\"p--").Append(slug).Append("\">\n");

        // The eyebrow is a div (not part of the heading), so the PDF outline and the running header
        // string-set keep the plain chapter title.
        if (chapterLabel is not null)
        {
            sb.Append("<div class=\"book-chapter-label\">").Append(Encode(chapterLabel)).Append("</div>\n");
        }

        sb.Append("<h").Append(level).Append('>').Append(Encode(node.Title)).Append("</h").Append(level).Append(">\n");

        var key = NormalizePath(node.Route.CanonicalPath.Value);
        if (!string.IsNullOrEmpty(key)
            && pageByPath.TryGetValue(key, out var page)
            && page.Content is not null
            && !string.IsNullOrEmpty(page.Html))
        {
            var content = ProcessPageContent(page, depth, slug, rewriteHref, resolveImageSrc);
            if (!string.IsNullOrEmpty(content))
            {
                sb.Append(content).Append('\n');
            }
        }

        sb.Append("</section>\n");

        foreach (var child in node.Children)
        {
            RenderNode(sb, child, depth + 1, slugByNode, pageByPath, rewriteHref, resolveImageSrc);
        }
    }

    private string ProcessPageContent(
        RenderedPage page,
        int demoteBy,
        string slug,
        Func<string, string> rewriteHref,
        Func<string, string> resolveImageSrc)
    {
        // Re-parse into our own document so we never mutate the projection's shared Content element.
        var sourceBody = _parser.ParseDocument(page.Html).Body;
        if (sourceBody is null)
        {
            return "";
        }

        // Strip article chrome: the pager <nav>s and the leading <h1> (re-emitted from the nav title).
        foreach (var nav in sourceBody.QuerySelectorAll("nav"))
        {
            nav.Remove();
        }

        sourceBody.QuerySelector("h1")?.Remove();

        // A code-fence preprocessor directive (tree-sitter `:symbol`/`:symbol-diff`, `:xmldocid`, …)
        // survives on the wrapper's data-language even though the wrapper holds already-resolved code.
        // Recovering it verbatim would re-emit a directive fence that the re-render re-processes —
        // turning each resolved code line into a "file not found" error. Keep only the base highlight
        // language so the recovered fence re-highlights cleanly. (Core deliberately keeps the full
        // directive for its own consumers; this trim is local to the book's re-render.)
        foreach (var wrapper in sourceBody.QuerySelectorAll("[data-language]"))
        {
            var language = wrapper.GetAttribute("data-language");
            var colon = language?.IndexOf(':') ?? -1;
            if (colon >= 0)
            {
                wrapper.SetAttribute("data-language", language![..colon]);
            }
        }

        var markdown = HtmlToMarkdownConverter.Convert(sourceBody, rewriteHref);
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "";
        }

        var rendered = Markdown.ToHtml(markdown, _pipeline);
        var content = _parser.ParseDocument(rendered).Body;
        if (content is null)
        {
            return "";
        }

        LabelUntitledAlerts(content);

        if (demoteBy > 0)
        {
            foreach (var heading in content.QuerySelectorAll("h1,h2,h3,h4,h5,h6"))
            {
                var level = heading.NodeName[1] - '0';
                var newLevel = Math.Min(level + demoteBy, 6);
                if (newLevel != level)
                {
                    DemoteHeading(heading, newLevel);
                }
            }
        }

        // Prefix every id so anchors are unique across the book and intra-book fragment links resolve.
        foreach (var element in content.QuerySelectorAll("[id]"))
        {
            element.Id = $"{slug}--{element.Id}";
        }

        foreach (var img in content.QuerySelectorAll("img[src]"))
        {
            img.SetAttribute("src", resolveImageSrc(img.GetAttribute("src") ?? ""));
        }

        return content.InnerHtml;
    }

    /// <summary>
    /// Gives a label to any alert the renderer left untitled. Markdig titles its five built-in kinds
    /// (note/tip/important/warning/caution) with an octicon, but emits an empty title slot for a custom
    /// kind like <c>checkpoint</c> — which the round-trip produces from the <c>&lt;Checkpoint&gt;</c>
    /// component. The label is derived from the flavor class so the box reads correctly in print.
    /// </summary>
    private static void LabelUntitledAlerts(IElement content)
    {
        foreach (var alert in content.QuerySelectorAll(".markdown-alert"))
        {
            var flavor = AlertFlavor(alert);
            if (flavor is null)
            {
                continue;
            }

            var existing = alert.QuerySelector(".markdown-alert-title");
            if (existing is not null && !string.IsNullOrWhiteSpace(existing.TextContent))
            {
                continue;
            }

            var label = char.ToUpperInvariant(flavor[0]) + flavor[1..];

            // Markdig emits the (empty) title slot as the first <p>; relabel it, else prepend one.
            if (alert.FirstElementChild is { NodeName: "P" } slot && string.IsNullOrWhiteSpace(slot.TextContent))
            {
                slot.ClassName = "markdown-alert-title";
                slot.TextContent = label;
            }
            else
            {
                var title = alert.Owner!.CreateElement("p");
                title.ClassName = "markdown-alert-title";
                title.TextContent = label;
                alert.Prepend(title);
            }
        }
    }

    private static string? AlertFlavor(IElement alert)
    {
        const string prefix = "markdown-alert-";
        foreach (var cls in alert.ClassList)
        {
            if (cls.Length > prefix.Length && cls.StartsWith(prefix, StringComparison.Ordinal))
            {
                return cls[prefix.Length..];
            }
        }

        return null;
    }

    private static void DemoteHeading(IElement heading, int newLevel)
    {
        var replacement = heading.Owner!.CreateElement("h" + newLevel);
        replacement.InnerHtml = heading.InnerHtml;
        foreach (var attr in heading.Attributes)
        {
            replacement.SetAttribute(attr.Name, attr.Value);
        }

        heading.Parent?.ReplaceChild(replacement, heading);
    }

    private Func<string, string> BuildHrefRewriter(Dictionary<string, string> inBookSlugs)
    {
        return href =>
        {
            if (string.IsNullOrEmpty(href)
                || href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("//")
                || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                return href;
            }

            var hashIdx = href.IndexOf('#');
            var fragment = hashIdx >= 0 ? href[(hashIdx + 1)..] : "";
            var beforeHash = hashIdx >= 0 ? href[..hashIdx] : href;
            var queryIdx = beforeHash.IndexOf('?');
            var pathPart = queryIdx >= 0 ? beforeHash[..queryIdx] : beforeHash;

            var key = NormalizePath(pathPart);
            if (!string.IsNullOrEmpty(key) && inBookSlugs.TryGetValue(key, out var targetSlug))
            {
                return !string.IsNullOrEmpty(fragment)
                    ? $"#{targetSlug}--{fragment}"
                    : $"#p--{targetSlug}";
            }

            // Out-of-book internal link: absolutize so it targets the live site.
            if (pathPart.StartsWith('/'))
            {
                var absolute = _canonicalBase.Combine(new UrlPath(pathPart)).Value;
                return string.IsNullOrEmpty(fragment) ? absolute : $"{absolute}#{fragment}";
            }

            return href;
        };
    }

    private static void AppendCover(StringBuilder sb, string siteTitle, BookDefinition book, string? version)
    {
        sb.Append("<section class=\"book-cover\">\n");
        sb.Append("<div class=\"book-cover-rule\"></div>\n");
        sb.Append("<div class=\"book-cover-title\">").Append(Encode(siteTitle)).Append("</div>\n");

        // The section/area name reads as a subtitle when it is distinct from the site title.
        if (!string.IsNullOrWhiteSpace(book.Title) && !string.Equals(book.Title, siteTitle, StringComparison.Ordinal))
        {
            sb.Append("<div class=\"book-cover-subtitle\">").Append(Encode(book.Title)).Append("</div>\n");
        }

        if (!string.IsNullOrWhiteSpace(book.Subtitle))
        {
            sb.Append("<div class=\"book-cover-tagline\">").Append(Encode(book.Subtitle)).Append("</div>\n");
        }

        if (!string.IsNullOrWhiteSpace(version))
        {
            sb.Append("<div class=\"book-cover-version\">").Append(Encode($"Version {version}")).Append("</div>\n");
        }

        sb.Append("</section>\n");
    }

    /// <summary>
    /// Emits the colophon — the small-print provenance page on the back of the cover: version,
    /// generation month + year, the canonical site URL, an author copyright when one is configured,
    /// and the Pennington credit. All divs, so the PDF outline stays clean.
    /// </summary>
    private void AppendColophon(StringBuilder sb, BookStamp stamp)
    {
        sb.Append("<section class=\"book-colophon\">\n");

        if (!string.IsNullOrWhiteSpace(stamp.Version))
        {
            AppendColophonLine(sb, $"Version {stamp.Version}");
        }

        var culture = ResolveCulture(stamp.Locale ?? _localization.DefaultLocale);
        AppendColophonLine(sb, $"Generated {stamp.GeneratedAt.ToString("Y", culture)}");
        AppendColophonLine(sb, _canonicalBase.Value.Value);

        if (!string.IsNullOrWhiteSpace(_penn.StructuredDataAuthorName))
        {
            AppendColophonLine(sb, $"© {stamp.GeneratedAt.Year} {_penn.StructuredDataAuthorName}");
        }

        var pennington = BookVersion.Pennington();
        AppendColophonLine(sb, pennington is null ? "Produced with Pennington" : $"Produced with Pennington {pennington}");

        sb.Append("</section>\n");
    }

    private static void AppendColophonLine(StringBuilder sb, string text)
        => sb.Append("<div class=\"book-colophon-line\">").Append(Encode(text)).Append("</div>\n");

    private void AppendToc(
        StringBuilder sb,
        NavigationTreeItem? intro,
        ImmutableList<NavigationTreeItem> chapters,
        Dictionary<NavigationTreeItem, string> slugByNode,
        string? locale)
    {
        sb.Append("<nav class=\"book-toc\">\n");
        sb.Append("<div class=\"book-toc-heading\">").Append(Encode(Translate(locale, ContentsKey, DefaultContents))).Append("</div>\n");
        sb.Append("<ol>\n");

        // The unwrapped area landing reads as an unnumbered introduction entry.
        if (intro is not null)
        {
            AppendTocEntry(sb, intro, slugByNode, number: null);
        }

        // Chapter numbers only on the top level, mirroring the chapter eyebrow labels.
        var number = 0;
        foreach (var node in chapters)
        {
            AppendTocEntry(sb, node, slugByNode, ++number);
        }

        sb.Append("</ol>\n");
        sb.Append("</nav>\n");
    }

    private static void AppendTocEntry(
        StringBuilder sb,
        NavigationTreeItem node,
        Dictionary<NavigationTreeItem, string> slugByNode,
        int? number)
    {
        var slug = slugByNode[node];
        sb.Append("<li><a href=\"#p--").Append(slug).Append("\">");
        if (number is not null)
        {
            sb.Append("<span class=\"toc-chapter-number\">").Append(number).Append("</span>");
        }

        sb.Append("<span class=\"toc-title\">").Append(Encode(node.Title)).Append("</span></a>");
        if (node.Children.Count > 0)
        {
            sb.Append('\n');
            AppendTocLevel(sb, node.Children, slugByNode);
        }

        sb.Append("</li>\n");
    }

    private static void AppendTocLevel(
        StringBuilder sb,
        ImmutableList<NavigationTreeItem> nodes,
        Dictionary<NavigationTreeItem, string> slugByNode)
    {
        sb.Append("<ol>\n");
        foreach (var node in nodes)
        {
            AppendTocEntry(sb, node, slugByNode, number: null);
        }

        sb.Append("</ol>\n");
    }

    /// <summary>True when <paramref name="node"/> maps to a projected page with a body to compose — the same test <see cref="RenderNode"/> applies before emitting page content.</summary>
    private static bool HasComposableContent(NavigationTreeItem node, IReadOnlyDictionary<string, RenderedPage> pageByPath)
    {
        var key = NormalizePath(node.Route.CanonicalPath.Value);
        return !string.IsNullOrEmpty(key)
            && pageByPath.TryGetValue(key, out var page)
            && page.Content is not null
            && !string.IsNullOrEmpty(page.Html);
    }

    /// <summary>Resolves a book chrome string: the locale's translation, then the default locale's, then <paramref name="fallback"/> — the same chain as <see cref="BookCatalog"/>.</summary>
    private string Translate(string? locale, string key, string fallback)
        => _translations.Get(locale ?? _localization.DefaultLocale, key)
            ?? _translations.Get(_localization.DefaultLocale, key)
            ?? fallback;

    /// <summary>The <c>lang</c> attribute value for <paramref name="locale"/>: the locale's configured <see cref="LocaleInfo.HtmlLang"/> when present, else the locale code itself.</summary>
    private string ResolveLang(string? locale)
    {
        var code = locale ?? _localization.DefaultLocale;
        return _localization.Locales.TryGetValue(code, out var info) ? info.HtmlLang ?? code : code;
    }

    private static CultureInfo ResolveCulture(string locale)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(locale);

            // On Linux/macOS, ICU accepts any string without throwing, synthesizing a culture for
            // unknown tags. Reject those (same guard as PenningtonUrlRequestCultureProvider).
            if (culture.LCID == 4096
                && (culture.TwoLetterISOLanguageName.Length > 3
                    || culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal)))
            {
                return CultureInfo.InvariantCulture;
            }

            return culture;
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string NormalizePath(string canonicalPath) => canonicalPath.Trim('/');

    private static string Slugify(string text)
    {
        var sb = new StringBuilder(text.Length);
        var prevDash = false;
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                prevDash = false;
            }
            else if (!prevDash && sb.Length > 0)
            {
                sb.Append('-');
                prevDash = true;
            }
        }

        return sb.ToString().Trim('-');
    }

    private static string LoadResource(string fileName)
    {
        var assembly = typeof(BookComposer).Assembly;
        var name = Array.Find(
            assembly.GetManifestResourceNames(),
            n => n.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded resource '{fileName}' not found in {assembly.GetName().Name}.");
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
