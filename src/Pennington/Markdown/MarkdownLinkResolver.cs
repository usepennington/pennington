namespace Pennington.Markdown;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Content;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Resolves author-written relative links inside markdown bodies to absolute
/// canonical URLs. Handles three cases:
/// <list type="bullet">
/// <item><c>../how-to/foo.md</c> — rewrites to the canonical URL of the target
/// markdown file and strips the <c>.md</c> suffix.</item>
/// <item><c>sample-post</c> (no extension) — treated as a sibling markdown
/// reference, resolved against the source file's directory and looked up by
/// trying common extensions.</item>
/// <item><c>./image.png</c> or other non-markdown relative assets — resolved
/// against the source file's directory and emitted as an absolute URL relative
/// to the owning content source's base URL.</item>
/// </list>
/// The index is built lazily from all registered <see cref="IContentService"/>
/// instances. When managed by <see cref="FileWatchDependencyFactory{T}"/>, the
/// instance is recreated on file changes so the index stays fresh.
/// </summary>
public sealed class MarkdownLinkResolver : IFileWatchAware
{
    private static readonly string[] MarkdownExtensions = [".md", ".markdown", ".mdx"];

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    private readonly AsyncLazy<IndexData> _indexLazy;

    /// <summary>Creates the resolver; the link index is built lazily on first resolution.</summary>
    public MarkdownLinkResolver(IEnumerable<IContentService> contentServices)
    {
        _indexLazy = new AsyncLazy<IndexData>(() => BuildIndexAsync(contentServices));
    }

    /// <summary>
    /// Resolve a markdown-author-written href against the source file that
    /// contains it. Returns the rewritten href, or <c>null</c> if the href is
    /// external / absolute / unresolvable and should be left untouched.
    /// </summary>
    public async ValueTask<string?> ResolveAsync(FilePath sourceFile, string href)
    {
        if (string.IsNullOrEmpty(href))
        {
            return null;
        }

        if (!IsRewritableHref(href))
        {
            return null;
        }

        var index = await _indexLazy;
        return ResolveInternal(index, sourceFile, href);
    }

    /// <summary>Synchronous helper for tests: assumes the index is already warm.</summary>
    internal string? ResolveFromPrebuilt(IndexData index, FilePath sourceFile, string href)
    {
        if (string.IsNullOrEmpty(href))
        {
            return null;
        }

        if (!IsRewritableHref(href))
        {
            return null;
        }

        return ResolveInternal(index, sourceFile, href);
    }

    private static string? ResolveInternal(IndexData index, FilePath sourceFile, string href)
    {
        // Split off fragment/query tail so we can append it back after rewriting.
        var splitAt = IndexOfAny(href, '#', '?');
        var pathPart = splitAt >= 0 ? href[..splitAt] : href;
        var tail = splitAt >= 0 ? href[splitAt..] : string.Empty;

        if (pathPart.Length == 0)
        {
            // Pure fragment/query; leave untouched.
            return null;
        }

        var sourceDir = Path.GetDirectoryName(Path.GetFullPath(sourceFile.Value));
        if (string.IsNullOrEmpty(sourceDir))
        {
            return null;
        }

        // Resolve the relative path against the source directory.
        string resolved;
        try
        {
            resolved = Path.GetFullPath(Path.Combine(sourceDir, pathPart));
        }
        catch
        {
            return null;
        }

        // 1. Markdown link lookup: try as-is, then with each known markdown extension.
        var key = NormalizePath(resolved);
        if (index.MarkdownByPath.TryGetValue(key, out var canonicalUrl))
        {
            return canonicalUrl + tail;
        }

        foreach (var ext in MarkdownExtensions)
        {
            if (index.MarkdownByPath.TryGetValue(NormalizePath(resolved + ext), out canonicalUrl))
            {
                return canonicalUrl + tail;
            }
        }

        // If the href explicitly ended in .md/.markdown/.mdx but the lookup missed,
        // also try stripping the extension and seeing if the bare path maps — handles
        // the "source filename matches but index stored without extension" edge case.
        var resolvedExt = Path.GetExtension(resolved);
        if (resolvedExt.Length > 0 && IsMarkdownExtension(resolvedExt))
        {
            var stripped = resolved[..^resolvedExt.Length];
            if (index.MarkdownByPath.TryGetValue(NormalizePath(stripped), out canonicalUrl))
            {
                return canonicalUrl + tail;
            }
        }

        // 2. Asset fallback: resolve against the owning content source and emit
        //    an absolute URL rooted at that source's base URL. This handles
        //    `./image.png` from a markdown file inside a content directory.
        if (TryResolveAsAsset(index, resolved, out var assetUrl))
        {
            return assetUrl + tail;
        }

        return null;
    }

    private static bool TryResolveAsAsset(IndexData index, string resolvedAbsolutePath, out string url)
    {
        // Find the content source root that contains this path. Use the longest match
        // in case roots are nested. Match case-insensitively (Windows filesystems
        // are case-insensitive) but emit the URL using the original casing to
        // preserve case-sensitive web serving.
        var resolvedForward = resolvedAbsolutePath.Replace('\\', '/');

        ContentRootInfo? best = null;
        var bestLen = -1;

        foreach (var root in index.ContentRoots)
        {
            var rootForward = root.AbsoluteRoot.Replace('\\', '/');
            if (!rootForward.EndsWith('/'))
            {
                rootForward += "/";
            }

            if (resolvedForward.StartsWith(rootForward, StringComparison.OrdinalIgnoreCase)
                && rootForward.Length > bestLen)
            {
                best = root;
                bestLen = rootForward.Length;
            }
        }

        if (best is null)
        {
            url = string.Empty;
            return false;
        }

        var relative = resolvedForward[bestLen..];
        var basePart = best.BasePageUrl.Value.TrimEnd('/');
        if (!basePart.StartsWith('/'))
        {
            basePart = "/" + basePart;
        }

        if (basePart == "/")
        {
            basePart = string.Empty;
        }

        url = $"{basePart}/{relative}";
        return true;
    }

    private static bool IsRewritableHref(string href)
    {
        // Absolute URLs / external schemes / fragments.
        if (href.StartsWith('/'))
        {
            return false;
        }

        if (href.StartsWith('#'))
        {
            return false;
        }

        if (href.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        var colon = href.IndexOf(':');
        if (colon > 0)
        {
            var scheme = href[..colon];
            if (HasSchemeLikeShape(scheme))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasSchemeLikeShape(ReadOnlySpan<char> scheme)
    {
        if (scheme.Length == 0)
        {
            return false;
        }

        if (!char.IsLetter(scheme[0]))
        {
            return false;
        }

        foreach (var c in scheme)
        {
            if (!char.IsLetterOrDigit(c) && c != '+' && c != '-' && c != '.')
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsMarkdownExtension(string ext)
    {
        foreach (var known in MarkdownExtensions)
        {
            if (string.Equals(ext, known, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static int IndexOfAny(string s, char a, char b)
    {
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == a || c == b)
            {
                return i;
            }
        }
        return -1;
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').ToLowerInvariant();

    private static async Task<IndexData> BuildIndexAsync(IEnumerable<IContentService> services)
    {
        var markdownBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        var roots = new List<ContentRootInfo>();

        foreach (var service in services.OfType<IMarkdownContentSource>())
        {
            roots.Add(new ContentRootInfo(
                AbsoluteRoot: Path.GetFullPath(service.AbsoluteContentRoot),
                BasePageUrl: service.BasePageUrl));
        }

        await foreach (var item in services.DiscoverAllAsync())
        {
            if (item.Source is not MarkdownFileSource markdown)
            {
                continue;
            }

            var sourcePath = Path.GetFullPath(markdown.Path.Value);
            var canonical = item.Route.CanonicalPath.Value;

            // Store three keys so lookups can match regardless of whether the
            // author wrote the extension explicitly or relied on bare-name lookup.
            markdownBuilder[NormalizePath(sourcePath)] = canonical;

            var ext = Path.GetExtension(sourcePath);
            if (ext.Length > 0)
            {
                var withoutExt = sourcePath[..^ext.Length];
                markdownBuilder[NormalizePath(withoutExt)] = canonical;
            }
        }

        return new IndexData(markdownBuilder.ToImmutable(), roots);
    }

    internal sealed record ContentRootInfo(string AbsoluteRoot, UrlPath BasePageUrl);

    internal sealed record IndexData(
        ImmutableDictionary<string, string> MarkdownByPath,
        IReadOnlyList<ContentRootInfo> ContentRoots);
}