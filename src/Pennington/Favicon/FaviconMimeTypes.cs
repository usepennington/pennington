namespace Pennington.Favicon;

/// <summary>Maps icon file extensions to their <c>&lt;link&gt;</c> <c>type</c> MIME value.</summary>
internal static class FaviconMimeTypes
{
    /// <summary>
    /// Infers the MIME type from an icon href's extension, or returns <c>null</c> when the extension is
    /// unknown or absent (e.g. a <c>manifest</c> href), in which case the <c>type</c> attribute is omitted.
    /// </summary>
    public static string? InferFromHref(string href) => Extension(href) switch
    {
        ".ico" => "image/x-icon",
        ".png" => "image/png",
        ".svg" => "image/svg+xml",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => null,
    };

    private static string Extension(string href)
    {
        // Strip query/fragment so "/favicon.ico?v=2" still resolves to ".ico".
        var path = href;
        var cut = path.IndexOfAny(['?', '#']);
        if (cut >= 0)
        {
            path = path[..cut];
        }

        var dot = path.LastIndexOf('.');
        return dot < 0 ? "" : path[dot..].ToLowerInvariant();
    }
}
