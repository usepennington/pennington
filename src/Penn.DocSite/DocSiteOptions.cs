namespace Penn.DocSite;

using System.Reflection;
using Penn.Infrastructure;
using Penn.MonorailCss;
using Penn.Routing;

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
    public Assembly[] AdditionalRoutingAssemblies { get; init; } = [];

    /// <summary>Path to .sln or .slnx for Roslyn integration. Requires Penn.Roslyn package.</summary>
    public string? SolutionPath { get; init; }

    /// <summary>Configure localization options (locales, default locale).</summary>
    public Action<LocalizationOptions>? ConfigureLocalization { get; init; }
}
