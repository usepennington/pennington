namespace Pennington.StandardSite;

using System.Collections.Immutable;
using System.Text;
using Artifacts;
using Pipeline;
using Routing;

/// <summary>
/// Artifact-tier service for the Standard Site verification well-known files: the publication
/// AT-URI at <c>/.well-known/site.standard.publication[{path}]</c> and, when enabled, the bare
/// DID at <c>/.well-known/atproto-did</c>. Bytes derive from options alone — the trivial proof
/// that the artifact tier does not require the site projection. Claims nothing when the options
/// are incompletely configured (fail-safe). Transient so it captures current options.
/// </summary>
public sealed class WellKnownArtifactService : IArtifactContentService
{
    private const string ContentType = "text/plain; charset=utf-8";

    private readonly StandardSiteOptions _options;
    private readonly string _publicationPath;
    private readonly ImmutableList<ArtifactClaim> _claims;

    /// <summary>Creates the service; claims derive from the options alone.</summary>
    public WellKnownArtifactService(StandardSiteOptions options)
    {
        _options = options;
        _publicationPath = ".well-known/site.standard.publication" + options.PublicationPath.TrimEnd('/');

        if (!options.IsConfigured)
        {
            _claims = [];
            return;
        }

        var builder = ImmutableList.CreateBuilder<ArtifactClaim>();
        builder.Add(new ArtifactClaim("standard-site", new ExactClaim(new UrlPath("/" + _publicationPath)), "Standard Site publication AT-URI"));
        if (options.EmitAtprotoDid)
        {
            builder.Add(new ArtifactClaim("standard-site", new ExactClaim(new UrlPath("/.well-known/atproto-did")), "AT Protocol DID"));
        }

        _claims = builder.ToImmutable();
    }

    /// <inheritdoc/>
    public ImmutableList<ArtifactClaim> Claims => _claims;

    /// <inheritdoc/>
    public Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            return Task.FromResult<ArtifactContent?>(null);
        }

        if (string.Equals(relativePath, _publicationPath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ArtifactContent?>(Text(AtUri.Build(_options.Did, "site.standard.publication", _options.PublicationRkey)));
        }

        if (_options.EmitAtprotoDid
            && string.Equals(relativePath, ".well-known/atproto-did", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ArtifactContent?>(Text(_options.Did));
        }

        return Task.FromResult<ArtifactContent?>(null);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        if (!_options.IsConfigured)
        {
            yield break;
        }

        yield return Item(_publicationPath);
        if (_options.EmitAtprotoDid)
        {
            yield return Item(".well-known/atproto-did");
        }
    }

    private static ArtifactContent Text(string body)
        => new(Encoding.UTF8.GetBytes(body), ContentType);

    private static DiscoveredItem Item(string path)
        => new(
            new ContentRoute
            {
                CanonicalPath = new UrlPath("/" + path),
                OutputFile = new FilePath(path),
            },
            new GeneratedSource("text/plain"));
}
