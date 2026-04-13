namespace Pennington.Islands;

/// <summary>Converts between URLs and SPA data slugs.</summary>
public static class SpaSlug
{
    /// <summary>Convert a URL path to a slug. "/" -> "index", "/docs/intro" -> "docs/intro"</summary>
    public static string ToSlug(string url)
    {
        var trimmed = url.Trim('/');
        return string.IsNullOrEmpty(trimmed) ? "index" : trimmed;
    }

    /// <summary>Convert a slug back to a URL path. "index" -> "/", "docs/intro" -> "/docs/intro"</summary>
    public static string ToUrl(string slug)
    {
        if (string.IsNullOrEmpty(slug) || slug == "index") return "/";
        return "/" + slug.TrimStart('/');
    }
}