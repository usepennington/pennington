namespace ExtensibilityLabExample;

using System.Collections.Immutable;
using System.Text;
using Pennington.Artifacts;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Demonstrates the artifact tier — <see cref="IArtifactContentService"/> for byte outputs
/// (robots, search-index sidecars, social-image generators) that are not routed pages, not in
/// navigation, and not xref targets. <see cref="Claims"/> declares the URL territory,
/// <see cref="ResolveAsync"/> produces the bytes (served live in dev by the artifact router),
/// and <see cref="DiscoverAsync"/> enumerates the routes the static build writes — one byte
/// path for both surfaces.
/// <para>
/// Backs how-to <c>/how-to/extensibility/emit-generated-artifacts</c>.
/// </para>
/// </summary>
public sealed class RobotsTxtContentService : IArtifactContentService
{
    private const string Body = """
        User-agent: *
        Allow: /
        Sitemap: /sitemap.xml
        """;

    /// <summary>The one URL this service owns.</summary>
    public ImmutableList<ArtifactClaim> Claims { get; } =
        [new ArtifactClaim("robots", new ExactClaim(new UrlPath("/robots.txt")), "robots.txt")];

    /// <summary>
    /// Produces the robots.txt bytes — for a live dev request and for the static build alike.
    /// Returning null declines the request so it falls through to content routing.
    /// </summary>
    public Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
        => Task.FromResult(relativePath.Equals("robots.txt", StringComparison.OrdinalIgnoreCase)
            ? new ArtifactContent(Encoding.UTF8.GetBytes(Body), "text/plain; charset=utf-8")
            : null);

    /// <summary>Enumerates the single robots.txt route for the static build.</summary>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        yield return new DiscoveredItem(
            new ContentRoute
            {
                CanonicalPath = new UrlPath("/robots.txt"),
                OutputFile = new FilePath("robots.txt"),
            },
            new GeneratedSource("text/plain"));
    }
}
