namespace Penn.Islands;

using Penn.Routing;

/// <summary>Context available during island rendering.</summary>
public record RenderContext(
    UrlPath BaseUrl,
    string SiteTitle,
    string? Locale
);
