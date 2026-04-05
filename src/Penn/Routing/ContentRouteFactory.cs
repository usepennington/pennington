namespace Penn.Routing;

public static class ContentRouteFactory
{
    /// <summary>
    /// Markdown: file path to route.
    /// Converts a markdown file path relative to contentRoot into a URL.
    /// Example: sourceFile="Content/Docs/getting-started.md", contentRoot="Content/Docs", basePageUrl="/docs"
    ///   results in CanonicalPath="/docs/getting-started/", OutputFile="docs/getting-started/index.html"
    /// </summary>
    public static ContentRoute FromMarkdownFile(FilePath sourceFile, FilePath contentRoot, UrlPath basePageUrl, string locale = "")
    {
        // Get relative path from content root, remove extension
        var relative = GetRelativePath(contentRoot.Value, sourceFile.Value);
        var withoutExt = RemoveExtension(relative);

        // Handle index files: "index" becomes "" (just use base URL)
        var urlSegment = withoutExt.Equals("index", StringComparison.OrdinalIgnoreCase) ? "" : withoutExt;

        // Build canonical path with locale prefix
        var basePath = basePageUrl.RemoveTrailingSlash();
        UrlPath canonicalPath;
        if (!string.IsNullOrEmpty(locale))
        {
            canonicalPath = string.IsNullOrEmpty(urlSegment)
                ? new UrlPath($"/{locale}{basePath.Value}")
                : new UrlPath($"/{locale}{basePath.Value}/{NormalizeSegment(urlSegment)}");
        }
        else
        {
            var baseValue = basePath.Value == "/" ? "" : basePath.Value;
            canonicalPath = string.IsNullOrEmpty(urlSegment)
                ? basePath.Value == "" || basePath.Value == "/" ? new UrlPath("/") : basePath
                : new UrlPath($"{baseValue}/{NormalizeSegment(urlSegment)}");
        }

        canonicalPath = canonicalPath.EnsureLeadingSlash();

        return new ContentRoute
        {
            CanonicalPath = canonicalPath.EnsureTrailingSlash(),
            OutputFile = BuildOutputFile(canonicalPath),
            SourceFile = sourceFile,
            Locale = locale
        };
    }

    /// <summary>Razor: @page directive to route.</summary>
    public static ContentRoute FromRazorPage(string pageRoute, string locale = "")
    {
        var path = new UrlPath(pageRoute).EnsureLeadingSlash().RemoveTrailingSlash();
        if (!string.IsNullOrEmpty(locale))
            path = new UrlPath($"/{locale}{path.Value}");

        return new ContentRoute
        {
            CanonicalPath = path.EnsureTrailingSlash(),
            OutputFile = BuildOutputFile(path),
            Locale = locale
        };
    }

    /// <summary>Programmatic: explicit URL to route.</summary>
    public static ContentRoute FromUrl(UrlPath url, string locale = "")
    {
        var path = url.EnsureLeadingSlash().RemoveTrailingSlash();
        if (!string.IsNullOrEmpty(locale))
            path = new UrlPath($"/{locale}{path.Value}");

        return new ContentRoute
        {
            CanonicalPath = path.EnsureTrailingSlash(),
            OutputFile = BuildOutputFile(path),
            Locale = locale
        };
    }

    /// <summary>Custom: for non-markdown content services.</summary>
    public static ContentRoute FromCustom(UrlPath url, FilePath? sourceFile = null, string locale = "")
    {
        var path = url.EnsureLeadingSlash().RemoveTrailingSlash();
        if (!string.IsNullOrEmpty(locale))
            path = new UrlPath($"/{locale}{path.Value}");

        return new ContentRoute
        {
            CanonicalPath = path.EnsureTrailingSlash(),
            OutputFile = BuildOutputFile(path),
            SourceFile = sourceFile,
            Locale = locale
        };
    }

    /// <summary>Redirect: source URL to route (output is redirect HTML).</summary>
    public static ContentRoute ForRedirect(UrlPath sourceUrl)
    {
        var path = sourceUrl.EnsureLeadingSlash().RemoveTrailingSlash();

        return new ContentRoute
        {
            CanonicalPath = path.EnsureTrailingSlash(),
            OutputFile = BuildOutputFile(path)
        };
    }

    // Helper: get relative path, normalize to forward slashes
    private static string GetRelativePath(string basePath, string fullPath)
    {
        var normalizedBase = basePath.Replace('\\', '/').TrimEnd('/');
        var normalizedFull = fullPath.Replace('\\', '/');

        if (normalizedFull.StartsWith(normalizedBase + "/", StringComparison.OrdinalIgnoreCase))
            return normalizedFull[(normalizedBase.Length + 1)..];
        if (normalizedFull.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            return normalizedFull[normalizedBase.Length..].TrimStart('/');

        return normalizedFull;
    }

    private static string RemoveExtension(string path)
    {
        var lastDot = path.LastIndexOf('.');
        return lastDot > 0 ? path[..lastDot] : path;
    }

    private static FilePath BuildOutputFile(UrlPath path)
    {
        var outputPath = path.RemoveTrailingSlash().RemoveLeadingSlash().Value;
        return string.IsNullOrEmpty(outputPath)
            ? new FilePath("index.html")
            : new FilePath($"{outputPath}/index.html");
    }

    // Normalize URL segment: lowercase, replace backslashes
    private static string NormalizeSegment(string segment)
        => segment.Replace('\\', '/').ToLowerInvariant();
}
