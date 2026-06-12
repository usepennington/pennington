namespace Pennington.Content;

using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Pipeline;
using Routing;

/// <summary>
/// Surfaces every servable file under the content root as a <see cref="ContentToCopy"/>, so the
/// static build mirrors the whole-content-root static mount that
/// <see cref="Pennington.Infrastructure.PenningtonExtensions.UsePennington"/> serves at runtime.
/// <para>
/// Files placed in the content root but outside any markdown source's <c>ContentPath</c> (the
/// documented home for shared, absolute-URL assets) are served live but were never copied by the
/// build. Routing them through <see cref="GetContentToCopyAsync"/> closes that gap on one code
/// path: the build copy and both link auditors already consume
/// <see cref="ContentServiceExtensions.CollectContentToCopyAsync"/>, so dev and build cannot diverge.
/// </para>
/// <para>
/// Carries no routes, navigation, or search entries — it is an asset-copy source only. Register it
/// after the markdown sources so their (prefix-aware) outputs win the output-path dedup in the build.
/// </para>
/// </summary>
public sealed class ContentRootAssetService : IContentService
{
    // Source files are rendered into HTML pages, never copied verbatim — matching
    // MarkdownContentService.GetContentToCopyAsync. The content-type gate below is not enough on its
    // own: .NET 11's default provider maps .md to text/markdown, so without this set raw markdown
    // would be copied into the published output.
    private static readonly HashSet<string> SourceExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".md", ".mdx", ".razor", ".yml", ".yaml" };

    private readonly string _contentRoot;
    private readonly IFileSystem _fileSystem;
    private readonly IContentTypeProvider _contentTypeProvider;

    /// <summary>Creates the service over <paramref name="contentRootPath"/>; the content-type gate defaults to ASP.NET's standard extension map.</summary>
    public ContentRootAssetService(
        string contentRootPath,
        IFileSystem fileSystem,
        IContentTypeProvider? contentTypeProvider = null)
    {
        _fileSystem = fileSystem;
        _contentRoot = fileSystem.Path.GetFullPath(contentRootPath);
        _contentTypeProvider = contentTypeProvider ?? new FileExtensionContentTypeProvider();
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;

    /// <inheritdoc/>
    public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        if (!_fileSystem.Directory.Exists(_contentRoot))
        {
            return Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        }

        var builder = ImmutableList.CreateBuilder<ContentToCopy>();
        foreach (var file in _fileSystem.Directory.EnumerateFiles(_contentRoot, "*.*", SearchOption.AllDirectories))
        {
            // Output path == content-root-relative path, matching the URL the runtime mount serves it
            // at (RequestPath ""). When a markdown source already copies this file under its BasePageUrl
            // prefix, the build's output-path dedup keeps the source's copy.
            var relativePath = _fileSystem.Path.GetRelativePath(_contentRoot, file).Replace('\\', '/');

            // The runtime mount is new PhysicalFileProvider(root), whose default ExclusionFilters.Sensitive
            // 404s dot-prefixed segments (.well-known/, .git/, …). Skip them so the copy set matches what
            // the mount actually serves.
            if (relativePath.Split('/').Any(segment => segment.StartsWith('.')))
            {
                continue;
            }

            if (SourceExtensions.Contains(_fileSystem.Path.GetExtension(file)))
            {
                continue;
            }

            // llms-header.txt is an input to the generated llms.txt front door (read by
            // LlmsTxtService), not a publishable asset — same class as the .yml sidecars
            // above. The runtime mount still serves it (harmless), but it must not be
            // copied into the published output.
            if (relativePath.Equals("llms-header.txt", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Mirror the runtime content-root mount, which serves with ServeUnknownFileTypes = false:
            // only files whose extension maps to a known content type are reachable, so an unmapped
            // extension is a 404 at runtime and must not be copied either.
            if (!_contentTypeProvider.TryGetContentType(file, out _))
            {
                continue;
            }

            builder.Add(new ContentToCopy(new FilePath(file), new FilePath(relativePath)));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
        Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
        Task.FromResult(ImmutableList<CrossReference>.Empty);
}
