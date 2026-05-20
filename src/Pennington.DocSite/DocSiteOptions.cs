namespace Pennington.DocSite;

using System.Reflection;
using Infrastructure;
using MonorailCss;
using Routing;

/// <summary>
/// Options record passed to <see cref="DocSiteServiceExtensions.AddDocSite"/> that configures
/// the DocSite template: site chrome, typography, color scheme, content areas, and escape-hatch
/// callbacks for the underlying <see cref="PenningtonOptions"/> and
/// <see cref="MonorailCssOptions"/>.
/// </summary>
public record DocSiteOptions
{
    /// <summary>Title displayed in the site chrome and used as a default for OpenGraph tags.</summary>
    public required string SiteTitle { get; init; }

    /// <summary>Short description used in the meta description tag and default OpenGraph description.</summary>
    public required string Description { get; init; }

    /// <summary>Color scheme driving the MonorailCSS theme. Defaults to the built-in DocSite palette when null.</summary>
    public IColorScheme? ColorScheme { get; init; }

    /// <summary>
    /// Syntax-highlight color palette used by <c>.hljs-*</c> token classes.
    /// Defaults to <see cref="SyntaxTheme.Default"/> when null.
    /// Values may reference custom palette names registered via <see cref="ColorScheme"/>.
    /// </summary>
    public SyntaxTheme? SyntaxTheme { get; init; }

    /// <summary>Absolute base URL used when emitting canonical links, sitemap entries, and absolute feed URLs.</summary>
    public string? CanonicalBaseUrl { get; init; }

    /// <summary>Root folder (relative to the content project) that holds the markdown and razor content tree.</summary>
    public FilePath ContentRootPath { get; init; } = new("Content");

    /// <summary>Optional image URL rendered as the header logo/icon.</summary>
    public string? HeaderIcon { get; init; }

    /// <summary>Markdown or HTML inserted into the site header alongside the icon and title.</summary>
    public string? HeaderContent { get; init; }

    /// <summary>Markdown or HTML inserted into the site footer.</summary>
    public string? FooterContent { get; init; }

    /// <summary>URL to the project's GitHub repository. When set, a GitHub link is shown in the header.</summary>
    public string? GitHubUrl { get; init; }

    /// <summary>Default social-share image URL used when a page does not specify its own.</summary>
    public string? SocialImageUrl { get; init; }

    /// <summary>CSS font-family stack used for display type (headings and hero copy).</summary>
    public string? DisplayFontFamily { get; init; }

    /// <summary>CSS font-family stack used for body copy.</summary>
    public string? BodyFontFamily { get; init; }

    /// <summary>CSS font-family stack used for monospaced contexts (code blocks, inline code, kbd).</summary>
    public string? MonoFontFamily { get; init; }

    /// <summary>Additional CSS appended to the generated stylesheet.</summary>
    public string? ExtraStyles { get; init; }

    /// <summary>Additional raw HTML appended to the document <c>&lt;head&gt;</c> (for analytics, meta tags, etc.).</summary>
    public string? AdditionalHtmlHeadContent { get; init; }

    /// <summary>Fonts to preload via <c>&lt;link rel="preload"&gt;</c> for faster first paint.</summary>
    public FontPreload[] FontPreloads { get; init; } = [];

    /// <summary>Additional assemblies scanned for Razor components so out-of-project pages participate in routing.</summary>
    public Assembly[] AdditionalRoutingAssemblies { get; init; } = [];

    /// <summary>Configure localization options (locales, default locale).</summary>
    public Action<LocalizationOptions>? ConfigureLocalization { get; init; }

    /// <summary>
    /// Content areas for the documentation site.
    /// When empty or containing a single area, no area selector is shown.
    /// Each area's slug must match a top-level directory name under ContentRootPath.
    /// </summary>
    public IReadOnlyList<ContentArea> Areas { get; init; } = [];

    /// <summary>
    /// Escape hatch for additional content wiring: callback invoked against the
    /// underlying <see cref="PenningtonOptions"/> after DocSite's own defaults
    /// are applied. Use to register extra <see cref="PenningtonOptions.AddMarkdownContent{T}"/>
    /// sources, add highlighters, register islands, etc., without dropping to
    /// bare <see cref="PenningtonExtensions.AddPennington"/>.
    /// </summary>
    public Action<PenningtonOptions>? ConfigurePennington { get; init; }

    /// <summary>
    /// Override the CSS selector used to scope the search index to a page region.
    /// Default is <c>#main-content</c> — the element wrapping the article in the
    /// stock DocSite layout. Set to an empty string to index the whole page
    /// body, or to a custom selector when you've replaced the layout.
    /// </summary>
    public string? SearchIndexContentSelector { get; init; }

    /// <summary>
    /// Override the CSS selector used to scope HTML-to-markdown extraction for the
    /// LLM-channel HTTP-fetch fallback (used for Razor pages and other non-markdown
    /// content that don't have a markdown source to render). Markdown content uses
    /// the rendition channel directly and ignores this. Default is <c>#main-content</c>;
    /// override when the layout has been replaced. Set to an empty string to extract
    /// the full body.
    /// </summary>
    public string? LlmsTxtContentSelector { get; init; }

    /// <summary>
    /// Callback to further customize the MonorailCSS framework settings after
    /// the DocSite theme has been applied. Mirrors
    /// <see cref="MonorailCssOptions.CustomCssFrameworkSettings"/>.
    /// </summary>
    public Func<global::MonorailCss.CssFrameworkSettings, global::MonorailCss.CssFrameworkSettings>? CustomCssFrameworkSettings { get; init; }

    /// <summary>
    /// Wraps the baseline <see cref="global::MonorailCss.Theme.ProseCustomization"/>. Forwarded to
    /// <see cref="MonorailCssOptions.ExtendProseCustomization"/>.
    /// </summary>
    public Func<global::MonorailCss.Theme.ProseCustomization, global::MonorailCss.Theme.ProseCustomization>? ExtendProseCustomization { get; init; }
}