namespace DocSiteKitchenSinkExample;

using Pennington.DocSite;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.MonorailCss;

/// <summary>
/// Small, focused configuration helpers called from <c>Program.cs</c>.
/// Each method configures exactly one surface so how-to pages can embed
/// the method body with <c>csharp:xmldocid,bodyonly</c>.
/// </summary>
internal static class ServiceConfiguration
{
    /// <summary>
    /// Two content areas mapped to <c>Content/main/</c> and <c>Content/api/</c>.
    /// Each area's <see cref="ContentArea.Slug"/> drives both the URL prefix
    /// and the top-level folder under the shared <c>ContentRootPath</c>.
    /// Demonstrates the DocSite-level "multiple content roots" pattern —
    /// one markdown pipeline, two logical trees separated by folder.
    /// </summary>
    public static IReadOnlyList<ContentArea> BuildAreas() =>
    [
        new ContentArea("Main", "main"),
        new ContentArea("API", "api"),
    ];

    /// <summary>
    /// Two locales — English at the URL root, French under <c>/fr/</c>.
    /// Non-default locale content lives under <c>Content/fr/</c>; the default
    /// locale owns <c>Content/</c> directly.
    /// </summary>
    public static void ConfigureLocalization(LocalizationOptions loc)
    {
        loc.DefaultLocale = "en";
        loc.AddLocale("en", new LocaleInfo("English"));
        loc.AddLocale("fr", new LocaleInfo("Français", HtmlLang: "fr"));
    }

    /// <summary>
    /// Algorithmic color scheme seeded from a single primary hue.
    /// The generator function derives the accent and two tertiary hues so
    /// the whole palette moves with one number.
    /// </summary>
    public static AlgorithmicColorScheme BuildColorScheme() =>
        new()
        {
            PrimaryHue = 220,
            ColorSchemeGenerator = primary => (primary + 140, primary + 60, primary - 40),
            BaseColorName = ColorName.Zinc,
        };

    /// <summary>
    /// Font preloads for Display + Body faces. The filenames are resolved
    /// against <c>wwwroot/fonts/</c> at request time; missing files don't
    /// break the host (the preload hints just 404).
    /// </summary>
    public static FontPreload[] BuildFontPreloads() =>
    [
        new FontPreload("/fonts/display.woff2"),
        new FontPreload("/fonts/body.woff2"),
    ];

    /// <summary>
    /// Minimal <c>@font-face</c> rules plus one site-wide CSS tweak appended
    /// to MonorailCSS output. The <c>ExtraStyles</c> string is emitted verbatim
    /// above the generated utility stylesheet.
    /// </summary>
    public static string BuildExtraStyles() => """
        @font-face {
            font-family: 'DocSiteKitchenSinkDisplay';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(/fonts/display.woff2) format('woff2');
        }
        @font-face {
            font-family: 'DocSiteKitchenSinkBody';
            font-style: normal;
            font-weight: 100 900;
            font-display: swap;
            src: url(/fonts/body.woff2) format('woff2');
        }
        article .feature-callout-demo { letter-spacing: 0.01em; }
        """;

    /// <summary>
    /// Footer HTML injected below the article region. Raw HTML string
    /// rendered via <c>MarkupString</c> — keep it minimal.
    /// </summary>
    public static string BuildFooter() => """
        <footer class="mt-16 py-8 text-center text-sm text-base-500">
            Built with Pennington DocSite. This kitchen sink backs 18 how-to pages.
        </footer>
        """;

    /// <summary>
    /// Builds the final <see cref="DocSiteOptions"/> used by <c>AddDocSite</c>.
    /// Every configurable surface named in the how-to index is wired here so
    /// each how-to can fence into one helper method above.
    /// </summary>
    public static DocSiteOptions BuildDocSiteOptions() => new()
    {
        SiteTitle = "Kitchen Sink Docs",
        Description = "A wide-surface DocSite example that backs eighteen how-to pages.",
        GitHubUrl = "https://github.com/usepennington/pennington",
        CanonicalBaseUrl = "https://example.com/",
        HeaderContent = """<a href="/" class="font-bold">Kitchen Sink Docs</a>""",
        FooterContent = BuildFooter(),
        ColorScheme = BuildColorScheme(),
        DisplayFontFamily = "'DocSiteKitchenSinkDisplay', system-ui, sans-serif",
        BodyFontFamily = "'DocSiteKitchenSinkBody', system-ui, sans-serif",
        FontPreloads = BuildFontPreloads(),
        ExtraStyles = BuildExtraStyles(),
        ConfigureLocalization = ConfigureLocalization,
        Areas = BuildAreas(),
    };
}