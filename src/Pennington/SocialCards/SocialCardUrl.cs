namespace Pennington.SocialCards;

using Routing;

/// <summary>
/// The single card-URL convention shared by the discovery service
/// (<see cref="SocialCardContentService"/>), the rendering endpoint
/// (<see cref="SocialCardEndpointExtensions.MapSocialCards"/>), and the templates' meta-tag wiring,
/// so a page's canonical path and its card URL always agree: <c>{BaseUrl}/{canonical-path}.png</c>,
/// with the home page (empty path) reserved as <c>{BaseUrl}/index.png</c>.
/// </summary>
public static class SocialCardUrl
{
    /// <summary>Reserved slug for the home page, whose canonical path trims to the empty string.</summary>
    public const string HomeSlug = "index";

    /// <summary>Root-relative card path for a page, e.g. <c>/social-cards/blog/my-post.png</c>.</summary>
    public static string RelativePath(UrlPath canonicalPath, string baseUrl)
    {
        var trimmed = canonicalPath.Value.Trim('/');
        var slug = string.IsNullOrEmpty(trimmed) ? HomeSlug : trimmed;
        return $"{baseUrl.TrimEnd('/')}/{slug}.png";
    }

    /// <summary>
    /// Card URL for a page — absolute when <paramref name="canonicalBaseUrl"/> is set (OpenGraph
    /// crawlers require an absolute <c>og:image</c>), otherwise the root-relative path.
    /// </summary>
    public static string For(UrlPath canonicalPath, string baseUrl, string? canonicalBaseUrl)
    {
        var relative = RelativePath(canonicalPath, baseUrl);
        return string.IsNullOrEmpty(canonicalBaseUrl)
            ? relative
            : $"{canonicalBaseUrl.TrimEnd('/')}{relative}";
    }

    /// <summary>
    /// Reverses <see cref="RelativePath"/>: maps the catch-all slug captured after <c>BaseUrl</c>
    /// (e.g. <c>blog/my-post.png</c>) back to a <see cref="Content.ContentRecordRegistry"/> key
    /// (<c>blog/my-post</c>); the home slug maps to the empty key.
    /// </summary>
    public static string SlugToRecordKey(string slug)
    {
        var key = slug.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? slug[..^4] : slug;
        key = key.Trim('/');
        return string.Equals(key, HomeSlug, StringComparison.OrdinalIgnoreCase) ? "" : key;
    }
}
