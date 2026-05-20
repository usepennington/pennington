namespace Pennington.Routing;

/// <summary>
/// Singleton wrapper for the site's effective canonical base URL, used by
/// generators (sitemap, RSS, llms.txt) to produce absolute links.
/// Resolved from <c>PenningtonOptions.CanonicalBaseUrl</c> when set, otherwise
/// from <c>OutputOptions.BaseUrl</c>.
/// </summary>
/// <param name="Value">The underlying canonical base (an origin like
/// <c>https://site.com</c>, or a path like <c>/</c> or <c>/sub/</c>).</param>
public sealed record CanonicalBaseUrl(UrlPath Value)
{
    /// <summary>Combines the base with a site-relative path; see <see cref="UrlComposer.Combine"/>.</summary>
    public UrlPath Combine(UrlPath relative) => UrlComposer.Combine(Value, relative);
}