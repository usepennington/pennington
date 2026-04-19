namespace Pennington.Islands;

using Routing;

/// <summary>Context available during island rendering.</summary>
/// <param name="BaseUrl">Base URL the site is published under.</param>
/// <param name="SiteTitle">Configured site title.</param>
/// <param name="Locale">Active request locale, or <c>null</c> when localization is not in use.</param>
public record RenderContext(
    UrlPath BaseUrl,
    string SiteTitle,
    string? Locale
);