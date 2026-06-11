namespace Pennington.Book;

/// <summary>
/// One book carved out of the site's table of contents. A book covers every page whose
/// canonical path falls under <see cref="RoutePrefix"/>; the whole TOC when the prefix is <c>/</c>.
/// </summary>
/// <param name="Title">Book title, shown on the cover and the download link.</param>
/// <param name="RoutePrefix">Canonical route prefix the book covers (for example <c>/tutorials/</c>, or <c>/</c> for the whole site).</param>
public record BookDefinition(string Title, string RoutePrefix)
{
    /// <summary>Optional slug for the output file (<c>pdf/{slug}.pdf</c>); defaults to the route prefix flattened to a slug.</summary>
    public string? Slug { get; init; }

    /// <summary>Optional cover subtitle; defaults to the site description for the whole-site book.</summary>
    public string? Subtitle { get; init; }

    /// <summary>Effective output slug — <see cref="Slug"/> when set, otherwise derived from <see cref="RoutePrefix"/>.</summary>
    public string EffectiveSlug =>
        !string.IsNullOrWhiteSpace(Slug) ? Slug! : SlugFromPrefix(RoutePrefix);

    /// <summary>Route prefix normalized to a leading + trailing slash (<c>/tutorials/</c>); <c>/</c> for the whole site.</summary>
    public string NormalizedRoutePrefix
    {
        get
        {
            var trimmed = RoutePrefix.Trim('/');
            return trimmed.Length == 0 ? "/" : $"/{trimmed}/";
        }
    }

    private static string SlugFromPrefix(string prefix)
    {
        var slug = prefix.Trim('/').Replace('/', '-');
        return string.IsNullOrEmpty(slug) ? "book" : slug;
    }
}

/// <summary>
/// Configuration for <c>AddPenningtonBook</c>: which books to carve out of the TOC and how to
/// launch Chromium. When <see cref="Books"/> is empty a single book covers the whole site.
/// </summary>
public sealed class BookOptions
{
    /// <summary>Explicit books. Empty (the default) means one whole-site book titled from the site title.</summary>
    public List<BookDefinition> Books { get; } = [];

    /// <summary>Path to a Chromium/Chrome executable. When null, PuppeteerSharp downloads a private Chromium on first use.</summary>
    public string? ChromiumExecutablePath { get; set; }

    /// <summary>Extra command-line arguments passed to Chromium (for example <c>--no-sandbox</c> in CI).</summary>
    public string[] AdditionalChromiumArgs { get; set; } = [];

    /// <summary>Extra CSS appended after the built-in print stylesheet, for per-site print tweaks.</summary>
    public string? AdditionalCss { get; set; }

    /// <summary>When true, appends a grayscale override stylesheet so books print without color — neutral grays for links, alerts, code, and the syntax palette. <see cref="AdditionalCss"/> still wins, since it is appended last.</summary>
    public bool Monochrome { get; set; }

    /// <summary>
    /// Resolves the effective book list: the configured <see cref="Books"/>, or a single
    /// whole-site book synthesized from <paramref name="penn"/> when none are configured.
    /// </summary>
    internal IReadOnlyList<BookDefinition> ResolveBooks(Infrastructure.PenningtonOptions penn)
    {
        if (Books.Count > 0)
        {
            return Books;
        }

        var title = string.IsNullOrWhiteSpace(penn.SiteTitle) ? "Documentation" : penn.SiteTitle;
        var subtitle = string.IsNullOrWhiteSpace(penn.SiteDescription) ? null : penn.SiteDescription;
        return [new BookDefinition(title, "/") { Slug = "book", Subtitle = subtitle }];
    }
}

/// <summary>URL/path math for book artifacts, mirroring the per-locale search shard layout.</summary>
internal static class BookRoutes
{
    /// <summary>True when <paramref name="locale"/> is the default locale (no URL prefix) or unspecified.</summary>
    public static bool IsDefaultLocale(string? locale, string defaultLocale)
        => string.IsNullOrEmpty(locale)
            || string.Equals(locale, defaultLocale, StringComparison.OrdinalIgnoreCase);

    /// <summary>Site-relative PDF path: <c>pdf/{slug}.pdf</c> (default locale) or <c>pdf/{locale}/{slug}.pdf</c>.</summary>
    public static string PdfPath(string slug, string? locale, string defaultLocale)
        => IsDefaultLocale(locale, defaultLocale) ? $"pdf/{slug}.pdf" : $"pdf/{locale}/{slug}.pdf";

    /// <summary>Live HTML preview path: <c>book-preview/{slug}/</c> or <c>book-preview/{locale}/{slug}/</c>.</summary>
    public static string PreviewPath(string slug, string? locale, string defaultLocale)
        => IsDefaultLocale(locale, defaultLocale) ? $"book-preview/{slug}/" : $"book-preview/{locale}/{slug}/";
}
