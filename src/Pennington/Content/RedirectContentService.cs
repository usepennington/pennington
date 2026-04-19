namespace Pennington.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using FrontMatter;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;
using Routing;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Holds the unified redirect map used by <see cref="PenningtonRedirectMiddleware"/>
/// at dev time and by the static build crawler's 301 handling at publish time.
/// The map is the union of two sources:
/// <list type="bullet">
///   <item>A <c>_redirects.yml</c> file at the top of <see cref="PenningtonOptions.ContentRootPath"/>.</item>
///   <item>Every page whose front matter sets <c>redirectUrl:</c> (see <see cref="IRedirectable"/>).
///     <see cref="MarkdownContentService{TFrontMatter}.DiscoverAsync"/> emits those as
///     <see cref="RedirectSource"/> items; this service collects them by scanning
///     registered <see cref="IContentService"/>s on first access.</item>
/// </list>
/// Both sources flow through one middleware so dev and static build behave identically
/// — a request to the redirect source URL returns HTTP 301 with a meta-refresh body,
/// and <see cref="Generation.OutputGenerationService"/> writes that body to disk.
/// </summary>
public sealed class RedirectContentService : IContentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PenningtonOptions _pennOptions;
    private readonly IFileSystem _fileSystem;
    private readonly AsyncLazy<ImmutableDictionary<string, string>> _mappingsLazy;

    /// <summary>
    /// Initializes the service and prepares lazy loading of the unified redirect map.
    /// </summary>
    public RedirectContentService(
        IServiceProvider serviceProvider,
        PenningtonOptions pennOptions,
        IFileSystem fileSystem)
    {
        _serviceProvider = serviceProvider;
        _pennOptions = pennOptions;
        _fileSystem = fileSystem;
        _mappingsLazy = new AsyncLazy<ImmutableDictionary<string, string>>(LoadMappingsAsync);
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;

    /// <summary>
    /// Returns the merged source-URL → target-URL map
    /// (<c>_redirects.yml</c> entries plus front-matter redirects).
    /// First call is eager — it enumerates every registered content service's
    /// <c>DiscoverAsync()</c> to pick up <see cref="RedirectSource"/> items.
    /// </summary>
    public Task<ImmutableDictionary<string, string>> GetRedirectMappingsAsync() => _mappingsLazy.Value;

    /// <summary>
    /// Yields one <see cref="DiscoveredItem"/> per <c>_redirects.yml</c> entry so the
    /// static build crawler hits the source URL, gets the middleware's 301 response,
    /// and writes a meta-refresh HTML file at that path. Per-page redirects already
    /// appear via their owning content service's <see cref="DiscoverAsync"/>, so they
    /// are not re-emitted here.
    /// </summary>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var yamlEntries = await LoadYamlAsync();
        foreach (var (source, target) in yamlEntries)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath(source));
            yield return new DiscoveredItem(route, new RedirectSource(new UrlPath(target)));
        }
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
        Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
        Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
        Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
        Task.FromResult(ImmutableList<CrossReference>.Empty);

    private async Task<ImmutableDictionary<string, string>> LoadMappingsAsync()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);

        // YAML first so per-page front matter can override on collision.
        foreach (var (source, target) in await LoadYamlAsync())
        {
            builder[NormalizeSource(source)] = target;
        }

        // Ask every other content service for its redirect sources. The default
        // IContentService.GetRedirectSourcesAsync returns empty without doing any
        // work, so services that have no redirects don't pay discovery costs here
        // (the old shape iterated each service's DiscoverAsync which, for example,
        // forced the Roslyn workspace to load for the auto-generated API reference).
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider.GetServices<IContentService>();
        foreach (var service in services)
        {
            if (ReferenceEquals(service, this)) continue;

            foreach (var item in await service.GetRedirectSourcesAsync())
            {
                if (item.Source.Value is RedirectSource redirect)
                {
                    builder[NormalizeSource(item.Route.CanonicalPath.Value)] = redirect.TargetUrl.Value;
                }
            }
        }

        return builder.ToImmutable();
    }

    private async Task<ImmutableDictionary<string, string>> LoadYamlAsync()
    {
        var root = _pennOptions.ContentRootPath;
        var absoluteRoot = _fileSystem.Path.GetFullPath(root);
        var redirectsFile = _fileSystem.Path.Combine(absoluteRoot, "_redirects.yml");

        if (!_fileSystem.File.Exists(redirectsFile))
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        var yaml = await _fileSystem.File.ReadAllTextAsync(redirectsFile);
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<RedirectsConfig>(yaml);
            if (config?.Redirects is null || config.Redirects.Count == 0)
            {
                return ImmutableDictionary<string, string>.Empty;
            }

            var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (source, target) in config.Redirects)
            {
                if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target)) continue;
                builder[NormalizeSource(source)] = target;
            }
            return builder.ToImmutable();
        }
        catch (YamlException)
        {
            return ImmutableDictionary<string, string>.Empty;
        }
    }

    private static string NormalizeSource(string source)
    {
        var trimmed = source.TrimEnd('/');
        return trimmed.StartsWith('/') ? trimmed : "/" + trimmed;
    }

    private sealed class RedirectsConfig
    {
        public Dictionary<string, string>? Redirects { get; set; }
    }
}