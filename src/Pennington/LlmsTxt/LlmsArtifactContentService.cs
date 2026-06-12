namespace Pennington.LlmsTxt;

using System.Collections.Immutable;
using System.Text;
using Artifacts;
using Pipeline;
using Routing;

/// <summary>
/// Artifact-tier façade over <see cref="LlmsTxtService"/>: one service owning every llms URL —
/// the root <c>/llms.txt</c> front door (and optional <c>/llms-full.txt</c>), the per-subtree
/// <c>{prefix}/llms.txt</c> indexes (a mid-path territory only a <see cref="SuffixClaim"/> can
/// express — endpoint routing would let a content route's <c>{slug}</c> segment capture them),
/// and the per-page <c>{OutputDirectory}/{path}.md</c> sidecars. Serves dev requests through the
/// artifact router and enumerates the same files for the static build. Transient so each
/// resolution captures the current file-watched service.
/// </summary>
public sealed class LlmsArtifactContentService : IArtifactContentService
{
    private const string TextContentType = "text/plain; charset=utf-8";
    private const string MarkdownContentType = "text/markdown; charset=utf-8";

    private readonly LlmsTxtService _service;
    private readonly LlmsTxtOptions _options;
    private readonly ImmutableList<ArtifactClaim> _claims;

    /// <summary>Creates the façade; claims derive from the options alone.</summary>
    public LlmsArtifactContentService(LlmsTxtService service, LlmsTxtOptions options)
    {
        _service = service;
        _options = options;

        var sidecarPrefix = "/" + options.OutputDirectory.Trim('/') + "/";
        var builder = ImmutableList.CreateBuilder<ArtifactClaim>();
        builder.Add(new ArtifactClaim("llms", new ExactClaim(new UrlPath("/llms.txt")), "llms.txt front door"));
        if (options.GenerateFullFile)
        {
            builder.Add(new ArtifactClaim("llms", new ExactClaim(new UrlPath("/llms-full.txt")), "concatenated llms-full.txt"));
        }

        builder.Add(new ArtifactClaim("llms", new SuffixClaim("/llms.txt"), "per-subtree llms.txt indexes"));
        builder.Add(new ArtifactClaim("llms", new PrefixClaim(new UrlPath(sidecarPrefix), ".md"), "per-page markdown sidecars"));
        _claims = builder.ToImmutable();
    }

    /// <inheritdoc/>
    public ImmutableList<ArtifactClaim> Claims => _claims;

    /// <inheritdoc/>
    public async Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (relativePath.Equals("llms.txt", StringComparison.OrdinalIgnoreCase))
        {
            return new ArtifactContent(Encoding.UTF8.GetBytes(await _service.GetLlmsTxtAsync()), TextContentType);
        }

        if (_options.GenerateFullFile && relativePath.Equals("llms-full.txt", StringComparison.OrdinalIgnoreCase))
        {
            var full = await _service.GetLlmsFullTxtAsync();
            return full is null ? null : new ArtifactContent(Encoding.UTF8.GetBytes(full), TextContentType);
        }

        if (relativePath.EndsWith("/llms.txt", StringComparison.OrdinalIgnoreCase))
        {
            var match = Find(await _service.GetSubtreeFilesAsync(), relativePath);
            return match is null ? null : new ArtifactContent(match.Content, TextContentType);
        }

        if (relativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            var match = Find(await _service.GetMarkdownFilesAsync(), relativePath);
            return match is null ? null : new ArtifactContent(match.Content, MarkdownContentType);
        }

        return null;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        yield return Item("llms.txt", "text/plain");

        foreach (var file in await _service.GetMarkdownFilesAsync())
        {
            yield return Item(file.OutputPath.Value, "text/markdown");
        }

        foreach (var file in await _service.GetSubtreeFilesAsync())
        {
            yield return Item(file.OutputPath.Value, "text/plain");
        }

        if (_options.GenerateFullFile)
        {
            yield return Item("llms-full.txt", "text/plain");
        }
    }

    private static LlmsTxtService.MarkdownFile? Find(ImmutableList<LlmsTxtService.MarkdownFile> files, string relativePath)
        => files.FirstOrDefault(f => string.Equals(f.OutputPath.Value, relativePath, StringComparison.OrdinalIgnoreCase));

    private static DiscoveredItem Item(string path, string contentType)
        => new(
            new ContentRoute
            {
                CanonicalPath = new UrlPath("/" + path),
                OutputFile = new FilePath(path),
            },
            new GeneratedSource(contentType));
}
