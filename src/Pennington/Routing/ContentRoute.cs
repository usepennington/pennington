namespace Pennington.Routing;

/// <summary>Describes the canonical location of a piece of content, its output file, and originating locale.</summary>
public sealed record ContentRoute
{
    /// <summary>Canonical URL path (leading slash, trailing slash for directories).</summary>
    public required UrlPath CanonicalPath { get; init; }

    /// <summary>Relative output file path written during static generation.</summary>
    public required FilePath OutputFile { get; init; }

    /// <summary>Originating source file on disk, if any.</summary>
    public FilePath? SourceFile { get; init; }

    /// <summary>Locale code for this route; empty for the default locale.</summary>
    public string Locale { get; init; } = "";

    /// <summary>True when this route serves default-locale content as a fallback for a missing translation.</summary>
    public bool IsFallback { get; init; }

    /// <summary>Composes the canonical path with a base URL path.</summary>
    public UrlPath WithBaseUrl(UrlPath baseUrl) => baseUrl / CanonicalPath;

    /// <summary>
    /// Compose the canonical path with the site's canonical base URL.
    /// </summary>
    /// <remarks>
    /// When <paramref name="canonicalBase"/> is a plain path (e.g. <c>/</c> or
    /// <c>/preview/</c>), we use <see cref="UrlPath"/>'s path-composition
    /// operator directly. When it is an absolute URL (has a scheme), we
    /// compose with string concatenation instead — <see cref="UrlPath"/>'s
    /// <c>/</c> operator is path-only and forces a leading slash on the root
    /// case, which would turn <c>https://site.com</c> + <c>/</c> into
    /// <c>/https://site.com</c>.
    /// </remarks>
    public UrlPath AbsoluteUrl(UrlPath canonicalBase)
    {
        var baseVal = canonicalBase.Value;
        if (Uri.TryCreate(baseVal, UriKind.Absolute, out _))
        {
            var trimmedBase = baseVal.TrimEnd('/');
            var path = CanonicalPath.Value;
            if (string.IsNullOrEmpty(path) || path == "/")
                return new UrlPath(trimmedBase + "/");
            if (!path.StartsWith('/')) path = "/" + path;
            return new UrlPath(trimmedBase + path);
        }
        return canonicalBase / CanonicalPath;
    }

    /// <summary>True when this route belongs to the default (unprefixed) locale.</summary>
    public bool IsDefaultLocale => string.IsNullOrEmpty(Locale);
}