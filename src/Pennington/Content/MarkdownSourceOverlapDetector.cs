namespace Pennington.Content;

using System.Collections.Immutable;

/// <summary>
/// Detects overlapping markdown content sources — pairs where one source's
/// <see cref="IMarkdownContentSource.AbsoluteContentRoot"/> is a strict descendant
/// directory of another's, AND the outer source does not opt out of that subtree
/// via <see cref="IMarkdownContentSource.ExcludePaths"/>.
///
/// Overlap without an explicit carve-out is almost always a misconfiguration: the
/// outer (catch-all) source and the inner (specialized) source both emit routes
/// for the same canonical URLs, producing duplicate TOC entries in every page's
/// sidebar and file-lock races when two pipelines write the same output file.
/// The engine emits warnings rather than silently deduping so users can see and
/// fix the misconfig.
/// </summary>
public static class MarkdownSourceOverlapDetector
{
    /// <summary>
    /// Returns one warning per unresolved overlap. Empty when everything is
    /// either disjoint or explicitly carved out.
    /// </summary>
    public static ImmutableArray<string> DetectOverlaps(
        IEnumerable<IMarkdownContentSource> sources)
    {
        var list = sources.ToList();
        if (list.Count < 2) return ImmutableArray<string>.Empty;

        var warnings = ImmutableArray.CreateBuilder<string>();

        for (var i = 0; i < list.Count; i++)
        {
            var outer = list[i];
            var outerRoot = NormalizeDirectory(outer.AbsoluteContentRoot);
            if (outerRoot.Length == 0) continue;

            for (var j = 0; j < list.Count; j++)
            {
                if (i == j) continue;
                var inner = list[j];
                var innerRoot = NormalizeDirectory(inner.AbsoluteContentRoot);
                if (innerRoot.Length == 0) continue;

                // inner must be a *strict* descendant of outer
                if (innerRoot.Length <= outerRoot.Length) continue;
                if (!innerRoot.StartsWith(outerRoot, StringComparison.OrdinalIgnoreCase)) continue;
                if (innerRoot[outerRoot.Length] != '/') continue;

                var relative = innerRoot[(outerRoot.Length + 1)..];
                if (IsExcluded(outer.ExcludePaths, relative)) continue;

                warnings.Add(
                    $"Markdown content source rooted at '{outer.AbsoluteContentRoot}' " +
                    $"overlaps a more specific source rooted at '{inner.AbsoluteContentRoot}'. " +
                    $"Both will discover files under '{relative}', producing duplicate TOC " +
                    $"entries and output-file races. Add `ExcludePaths = [\"{relative}\"]` to " +
                    $"the outer source's options so the inner source owns that subtree.");
            }
        }

        return warnings.ToImmutable();
    }

    /// <summary>
    /// Canonicalizes a directory path for prefix comparison: forward-slash separators,
    /// no trailing slash, lowercase for case-insensitive file systems. Returns empty
    /// for null/whitespace.
    /// </summary>
    private static string NormalizeDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        var normalized = path.Replace('\\', '/').TrimEnd('/').ToLowerInvariant();
        return normalized;
    }

    /// <summary>
    /// Mirrors <c>MarkdownContentService.IsRelativePathExcluded</c> — segment-based
    /// prefix matching. Kept inline here so the detector doesn't depend on the
    /// generic service type.
    /// </summary>
    private static bool IsExcluded(ImmutableArray<string> excludePaths, string relative)
    {
        if (excludePaths.IsDefaultOrEmpty) return false;
        var normalized = relative.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        foreach (var excluded in excludePaths)
        {
            if (normalized.Length == excluded.Length
                && normalized.Equals(excluded, StringComparison.Ordinal))
                return true;
            if (normalized.Length > excluded.Length
                && normalized.StartsWith(excluded, StringComparison.Ordinal)
                && normalized[excluded.Length] == '/')
                return true;
        }
        return false;
    }
}