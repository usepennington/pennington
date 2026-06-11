namespace Pennington.Navigation;

/// <summary>
/// A downloadable artifact a site advertises in its chrome (for example a sidebar
/// "Download as PDF" link).
/// </summary>
/// <param name="Label">Display-ready link text, already localized by the provider.</param>
/// <param name="Url">Site-relative URL of the artifact (for example <c>pdf/tutorials.pdf</c>).</param>
/// <param name="RoutePrefix">Canonical route prefix the artifact covers (for example <c>/tutorials/</c>); <c>/</c> for a whole-site artifact.</param>
public record DownloadLink(string Label, string Url, string RoutePrefix);

/// <summary>
/// DI-discovered provider of download links a host's chrome can advertise (the DocSite sidebar
/// renders every registered provider's links beneath the matching area's table of contents).
/// Implementations must be cheap to resolve on the request path — derive purely from configured
/// options, never from the site projection.
/// </summary>
public interface IDownloadLinkProvider
{
    /// <summary>
    /// Returns the links available for <paramref name="locale"/> (or the default locale when
    /// <c>null</c>), each with a locale-appropriate URL.
    /// </summary>
    IReadOnlyList<DownloadLink> GetLinks(string? locale = null);
}
