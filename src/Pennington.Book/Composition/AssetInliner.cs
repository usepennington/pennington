namespace Pennington.Book.Composition;

using System.IO.Abstractions;
using Content;
using Microsoft.Extensions.FileProviders;
using Routing;

/// <summary>
/// Resolves <c>&lt;img src&gt;</c> values to self-contained <c>data:</c> URIs so the composed book
/// renders offline (Chromium navigates via <c>SetContentAsync</c> with no server to fetch from).
/// Internal sources resolve first from the content-copy map (a content source's own static assets),
/// then from the host web root (wwwroot + RCL static web assets). External <c>http(s)</c> images are
/// left untouched — an offline Chromium simply omits them — and unresolved internal sources are
/// rewritten to an absolute URL so they at least point somewhere real.
/// </summary>
public sealed class AssetInliner
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileProvider _webRoot;
    private readonly CanonicalBaseUrl _canonicalBase;

    /// <summary>Creates an inliner over the given file system, web-root provider, and canonical base.</summary>
    public AssetInliner(IFileSystem fileSystem, IFileProvider webRoot, CanonicalBaseUrl canonicalBase)
    {
        _fileSystem = fileSystem;
        _webRoot = webRoot;
        _canonicalBase = canonicalBase;
    }

    /// <summary>
    /// Builds the <c>output-path → source-file</c> map from every content service's static-copy
    /// declarations. First registration wins on a duplicate output path, mirroring the build's copy pass.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, string>> BuildContentMapAsync(IEnumerable<IContentService> services)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in await services.CollectContentToCopyAsync())
        {
            var key = NormalizeKey(item.OutputPath.Value);
            map.TryAdd(key, item.SourcePath.Value);
        }

        return map;
    }

    /// <summary>
    /// Resolves <paramref name="src"/> to a <c>data:</c> URI when the bytes can be found, an absolute
    /// URL when an internal source is unresolved, or <paramref name="src"/> unchanged for external images.
    /// </summary>
    public string Resolve(string src, IReadOnlyDictionary<string, string> contentMap)
    {
        if (string.IsNullOrWhiteSpace(src) || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return src;
        }

        if (src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("//"))
        {
            return src;
        }

        // Strip query/fragment, then URL-decode to match on-disk paths.
        var pathPart = src;
        var cut = pathPart.IndexOfAny(['?', '#']);
        if (cut >= 0)
        {
            pathPart = pathPart[..cut];
        }

        var key = NormalizeKey(Uri.UnescapeDataString(pathPart));

        var bytes = TryReadFromContentMap(key, contentMap) ?? TryReadFromWebRoot(key);
        if (bytes is not null)
        {
            return $"data:{MimeForExtension(key)};base64,{Convert.ToBase64String(bytes)}";
        }

        // Unresolved internal source: absolutize so it at least targets the live site.
        return _canonicalBase.Combine(new UrlPath("/" + key)).Value;
    }

    private byte[]? TryReadFromContentMap(string key, IReadOnlyDictionary<string, string> contentMap)
    {
        if (!contentMap.TryGetValue(key, out var source) || !_fileSystem.File.Exists(source))
        {
            return null;
        }

        try
        {
            return _fileSystem.File.ReadAllBytes(source);
        }
        catch
        {
            return null;
        }
    }

    private byte[]? TryReadFromWebRoot(string key)
    {
        var file = _webRoot.GetFileInfo(key);
        if (!file.Exists || file.IsDirectory)
        {
            return null;
        }

        try
        {
            using var stream = file.CreateReadStream();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeKey(string path)
        => path.Replace('\\', '/').TrimStart('/');

    private static string MimeForExtension(string path)
    {
        var dot = path.LastIndexOf('.');
        var ext = dot >= 0 ? path[(dot + 1)..].ToLowerInvariant() : "";
        return ext switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "svg" => "image/svg+xml",
            "webp" => "image/webp",
            "avif" => "image/avif",
            "ico" => "image/x-icon",
            "bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
    }
}
