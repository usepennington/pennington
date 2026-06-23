namespace Pennington.DocSite;

using System.Reflection;
using Microsoft.AspNetCore.Components;
using Favicon;
using Infrastructure;
using Localization;
using MonorailCss;
using Pennington.UI;
using Routing;
using SocialCards;
using StandardSite;

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
    public required string SiteDescription { get; init; }

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

    /// <summary>
    /// The header brand area (logo + title). Assign a raw HTML string or a
    /// <see cref="RenderFragment"/> — both convert implicitly. When set, it replaces the default
    /// icon and <see cref="SiteTitle"/> link outright, giving total control over that region; when
    /// null, the default document icon and title link render. Strings are emitted as raw HTML, not markdown.
    /// </summary>
    public MarkupContent? HeaderContent { get; init; }

    /// <summary>
    /// Content rendered in the site footer. Assign a raw HTML string or a
    /// <see cref="RenderFragment"/> — both convert implicitly. Strings are emitted as raw HTML, not markdown.
    /// </summary>
    public MarkupContent? FooterContent { get; init; }

    /// <summary>URL to the project's GitHub repository. When set, a GitHub link is shown in the header.</summary>
    public string? GitHubUrl { get; init; }

    /// <summary>Default social-share image URL used when a page does not specify its own.</summary>
    public string? SocialImageUrl { get; init; }

    /// <summary>
    /// Enables generated per-page social cards. When set, each page emits an <c>og:image</c>/
    /// <c>twitter:image</c> pointing at an on-demand-rendered card (and the site-wide
    /// <see cref="SocialImageUrl"/> default steps aside). The host supplies the drawing via
    /// <see cref="SocialCardOptions.Render"/>.
    /// </summary>
    public SocialCardOptions? SocialCards { get; init; }

    /// <summary>
    /// Standard Site (AT Protocol) integration. Forwarded to <see cref="PenningtonOptions.StandardSite"/>.
    /// </summary>
    public StandardSiteOptions? StandardSite { get; init; }

    /// <summary>
    /// Favicon / icon links. Forwarded to <see cref="PenningtonOptions.Favicons"/>.
    /// </summary>
    public FaviconOptions? Favicons { get; init; }

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
    /// Override the CSS selector used to scope the shared site projection to a
    /// page region. The same selector drives the search index, llms.txt sidecars,
    /// and build-time link audit, so chrome (navigation, footers) is stripped once.
    /// Default is <c>#main-content</c> — the element wrapping the article in the
    /// stock DocSite layout. Set to an empty string to project the whole page
    /// body, or to a custom selector when you've replaced the layout.
    /// </summary>
    public string? ContentSelector { get; init; }

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