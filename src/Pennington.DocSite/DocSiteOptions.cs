namespace Pennington.DocSite;

using System.Reflection;
using Infrastructure;
using MonorailCss;
using Routing;

/// <summary>
/// Configuration for a documentation site.
/// </summary>
public record DocSiteOptions
{
    public required string SiteTitle { get; init; }
    public required string Description { get; init; }
    public IColorScheme? ColorScheme { get; init; }
    public string? CanonicalBaseUrl { get; init; }
    public FilePath ContentRootPath { get; init; } = new("Content");
    public string? HeaderIcon { get; init; }
    public string? HeaderContent { get; init; }
    public string? FooterContent { get; init; }
    public string? GitHubUrl { get; init; }
    public string? SocialImageUrl { get; init; }
    public string? DisplayFontFamily { get; init; }
    public string? BodyFontFamily { get; init; }
    public string? ExtraStyles { get; init; }
    public string? AdditionalHtmlHeadContent { get; init; }
    public FontPreload[] FontPreloads { get; init; } = [];
    public Assembly[] AdditionalRoutingAssemblies { get; init; } = [];

    /// <summary>Path to .sln or .slnx for Roslyn integration. Requires Pennington.Roslyn package.</summary>
    public string? SolutionPath { get; init; }

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
    /// bare <see cref="Infrastructure.PenningtonExtensions.AddPennington"/>.
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
    /// Override the CSS selector used to scope llms.txt raw-markdown extraction.
    /// Default is <c>#main-content</c>. Same conventions as
    /// <see cref="SearchIndexContentSelector"/>.
    /// </summary>
    public string? LlmsTxtContentSelector { get; init; }

    /// <summary>
    /// Callback to further customize the MonorailCSS framework settings after
    /// the DocSite theme has been applied. Mirrors
    /// <see cref="Pennington.MonorailCss.MonorailCssOptions.CustomCssFrameworkSettings"/>.
    /// </summary>
    public Func<global::MonorailCss.CssFrameworkSettings, global::MonorailCss.CssFrameworkSettings>? CustomCssFrameworkSettings { get; init; }
}