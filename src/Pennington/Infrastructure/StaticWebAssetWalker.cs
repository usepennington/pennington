namespace Pennington.Infrastructure;

using Microsoft.Extensions.FileProviders;

/// <summary>
/// Walks an <see cref="IFileProvider"/> tree yielding every physical file once, deduped by relative
/// path. <c>WebRootFileProvider</c> is a <see cref="CompositeFileProvider"/> whose children (physical
/// wwwroot + RCL manifest providers) can each expose the same logical path; first-wins dedup keeps
/// the static build copy and the link auditor's known-asset set reading the same files, so a
/// wwwroot/RCL asset the build copies is exactly the asset the auditor trusts.
/// </summary>
internal static class StaticWebAssetWalker
{
    /// <summary>A physical file discovered in the provider tree, with its forward-slash path relative to the provider root.</summary>
    internal readonly record struct WebAsset(string RelativePath, string PhysicalPath);

    /// <summary>Yields each physical file under <paramref name="provider"/> once, deduped by relative path (first-wins).</summary>
    internal static IEnumerable<WebAsset> Walk(IFileProvider provider) =>
        WalkCore(provider, "", new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    private static IEnumerable<WebAsset> WalkCore(IFileProvider provider, string subpath, HashSet<string> visited)
    {
        foreach (var item in provider.GetDirectoryContents(subpath))
        {
            var relativePath = string.IsNullOrEmpty(subpath) ? item.Name : $"{subpath}/{item.Name}";

            if (!visited.Add(relativePath))
            {
                continue;
            }

            if (item.IsDirectory)
            {
                foreach (var nested in WalkCore(provider, relativePath, visited))
                {
                    yield return nested;
                }
            }
            else if (item.PhysicalPath != null)
            {
                yield return new WebAsset(relativePath, item.PhysicalPath);
            }
        }
    }
}
