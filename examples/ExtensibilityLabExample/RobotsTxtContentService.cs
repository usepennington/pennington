namespace ExtensibilityLabExample;

using System.Collections.Immutable;
using System.Text;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Demonstrates the "emit artifacts only" <see cref="IContentService"/> shape —
/// every discovery member returns empty, and <see cref="GetContentToCreateAsync"/>
/// writes a single <c>robots.txt</c> to the output root. Useful for services
/// whose job is to produce a byte artifact (robots, search-index sidecars,
/// social-image generators) rather than contribute routed pages.
/// <para>
/// Backs how-to <c>/how-to/extensibility/emit-generated-artifacts</c>.
/// </para>
/// </summary>
public sealed class RobotsTxtContentService : IContentService
{
    private const string Body = """
        User-agent: *
        Allow: /
        Sitemap: /sitemap.xml
        """;

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        yield break;
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <summary>
    /// Emits a single <c>robots.txt</c> at the site root. The generator runs
    /// only when output is written, so it can depend on late-stage state.
    /// </summary>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
    {
        var artifact = new ContentToCreate(
            new FilePath("robots.txt"),
            () => Task.FromResult(Encoding.UTF8.GetBytes(Body)),
            "text/plain");

        return Task.FromResult(ImmutableList.Create(artifact));
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);
}