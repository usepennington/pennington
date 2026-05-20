using Pennington.Infrastructure;

namespace Pennington.Routing;

/// <summary>
/// Composes a canonical base URL with a site-relative path, yielding either a
/// fully-qualified URL (when the base has an http(s) scheme) or a root-relative
/// path (when the base is path-only like <c>/</c> or <c>/sub/</c>).
/// </summary>
public static class UrlComposer
{
    /// <summary>
    /// Combines <paramref name="canonicalBase"/> with <paramref name="relative"/> to
    /// produce an absolute URL when the base has an http(s) scheme, or a normalized
    /// root-relative path otherwise.
    /// </summary>
    /// <remarks>
    /// <see cref="UrlPath"/>'s <c>/</c> operator is path-only and forces a leading
    /// slash on the root case, which would turn <c>https://site.com</c> + <c>/</c>
    /// into <c>/https://site.com</c>. This helper handles the scheme case explicitly.
    /// </remarks>
    public static UrlPath Combine(UrlPath canonicalBase, UrlPath relative)
    {
        var baseVal = canonicalBase.Value;
        // AllowImplicitFilePaths = false rejects bare paths like `/preview/`,
        // which the default Uri parser accepts as a file URI on Linux but not
        // on Windows — a subtle cross-platform trap.
        var options = new UriParseOptions { UriKind = UriKind.Absolute, AllowImplicitFilePaths = false };
        if (Uri.TryCreate(baseVal, options, out var uri) && uri!.Scheme is "http" or "https")
        {
            var trimmedBase = baseVal.TrimEnd('/');
            var path = relative.Value;
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return new UrlPath(trimmedBase + "/");
            }

            if (!path.StartsWith('/'))
            {
                path = "/" + path;
            }

            return new UrlPath(trimmedBase + path);
        }
        return canonicalBase / relative;
    }
}