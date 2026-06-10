namespace Pennington.SocialCards;

using FrontMatter;
using Routing;

/// <summary>
/// Host configuration for social-card (OpenGraph / Twitter image) generation. Pennington owns the
/// discovery, routing, and meta-tag wiring; the host owns the drawing via <see cref="Render"/> and
/// brings its own image library (ImageSharp, SkiaSharp, Playwright, ...). Set this on
/// <see cref="Infrastructure.PenningtonOptions.SocialCards"/> (templates forward it from their own
/// options) to enable the feature; leaving it null disables it.
/// </summary>
public sealed record SocialCardOptions
{
    /// <summary>
    /// Host renderer invoked once per page to produce the card image bytes. Receives the request's
    /// <see cref="IServiceProvider"/> so a renderer can resolve registered services (font caches,
    /// theming options, ...). Return <c>null</c> to skip a page (its card route then serves 404 and
    /// is omitted from the build).
    /// </summary>
    public required Func<SocialCardRequest, IServiceProvider, CancellationToken, Task<byte[]?>> Render { get; init; }

    /// <summary>URL prefix under which card routes are published (e.g. <c>/social-cards/blog/post.png</c>).</summary>
    public string BaseUrl { get; init; } = "/social-cards";

    /// <summary>Card width in pixels passed to <see cref="Render"/>. Defaults to the 1200x630 OpenGraph standard.</summary>
    public int Width { get; init; } = 1200;

    /// <summary>Card height in pixels passed to <see cref="Render"/>. Defaults to the 1200x630 OpenGraph standard.</summary>
    public int Height { get; init; } = 630;

    /// <summary>MIME type served for, and meta-tagged on, the generated card.</summary>
    public string ContentType { get; init; } = "image/png";
}

/// <summary>
/// Everything <see cref="SocialCardOptions.Render"/> needs to draw a card for one page: the page's
/// resolved metadata plus the site identity and the card's own absolute URL.
/// </summary>
/// <param name="Title">Page title.</param>
/// <param name="Description">Page description, when the front matter supplies one.</param>
/// <param name="Date">Publication date, when set.</param>
/// <param name="CanonicalPath">Canonical path of the page the card represents.</param>
/// <param name="CardUrl">The card's own URL — absolute when a canonical base URL is configured, else root-relative.</param>
/// <param name="Locale">Locale code of the page, or <c>null</c> for the default locale.</param>
/// <param name="SiteTitle">Site title, for branding the card.</param>
/// <param name="SiteDescription">Site description, available for fallback copy.</param>
/// <param name="Metadata">The page's full typed front matter, so a renderer can read tags, author, and other capabilities.</param>
/// <param name="Width">Requested card width in pixels.</param>
/// <param name="Height">Requested card height in pixels.</param>
public sealed record SocialCardRequest(
    string Title,
    string? Description,
    DateTime? Date,
    UrlPath CanonicalPath,
    string CardUrl,
    string? Locale,
    string SiteTitle,
    string? SiteDescription,
    IFrontMatter Metadata,
    int Width,
    int Height);
