namespace Pennington.DocSite;

using System.Reflection;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Routing;

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
}
