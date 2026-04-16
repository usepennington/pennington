namespace Pennington.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.RegularExpressions;
using FrontMatter;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Pipeline;
using Routing;

/// <summary>
/// Discovers @page Razor components for the content pipeline.
/// Scans configured assemblies for types inheriting ComponentBase with
/// non-parameterized [RouteAttribute] routes. Optionally loads metadata
/// from sidecar .razor.metadata.yml files placed alongside the component.
/// </summary>
public sealed partial class RazorPageContentService : IContentService
{
    private readonly Assembly[] _assemblies;
    private readonly IFileSystem _fileSystem;
    private readonly FrontMatterParser _frontMatterParser;
    private readonly ILogger<RazorPageContentService> _logger;
    private readonly List<(string Template, string TypeName)> _missingTrailingSlashPages = [];
    private readonly Lazy<Dictionary<string, string>> _razorFileCache;
    private readonly Lazy<List<ComponentWithMetadata>> _componentMetadataCache;

    private sealed record ComponentWithMetadata(Type Component, List<ContentRoute> Routes, DocFrontMatter? Metadata);

    /// <summary>
    /// Initializes the service with the assemblies to scan for routable Razor components.
    /// </summary>
    public RazorPageContentService(
        Assembly[] assemblies,
        IFileSystem fileSystem,
        FrontMatterParser frontMatterParser,
        ILogger<RazorPageContentService> logger)
    {
        _assemblies = assemblies;
        _fileSystem = fileSystem;
        _frontMatterParser = frontMatterParser;
        _logger = logger;
        _razorFileCache = new Lazy<Dictionary<string, string>>(BuildRazorFileCache);
        _componentMetadataCache = new Lazy<List<ComponentWithMetadata>>(BuildComponentMetadataCache);
    }

    /// <summary>
    /// Razor @page directives that were missing a trailing slash.
    /// Populated after <see cref="DiscoverAsync"/> runs.
    /// </summary>
    public IReadOnlyList<(string Template, string TypeName)> MissingTrailingSlashPages => _missingTrailingSlashPages;

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 5;

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var entry in _componentMetadataCache.Value)
        {
            if (entry.Metadata is { IsDraft: true })
                continue;

            foreach (var route in entry.Routes)
            {
                ContentSource source = new RazorPageSource(
                    entry.Component.AssemblyQualifiedName ?? entry.Component.FullName ?? entry.Component.Name);
                yield return new DiscoveredItem(route, source);
            }
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var entry in _componentMetadataCache.Value)
        {
            if (entry.Metadata is null) continue;
            if (entry.Metadata is { IsDraft: true }) continue;

            var route = entry.Routes[0];
            var hierarchyParts = route.CanonicalPath.Value
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            var order = entry.Metadata is IOrderable orderable ? orderable.Order : int.MaxValue;
            var sectionLabel = entry.Metadata is ISectionable sectionable ? sectionable.SectionLabel : null;

            builder.Add(new ContentTocItem(
                Title: entry.Metadata.Title,
                Route: route,
                Order: order,
                HierarchyParts: hierarchyParts,
                SectionLabel: sectionLabel ?? DefaultSectionLabel,
                Locale: null
            ));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetIndexableEntriesAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var entry in _componentMetadataCache.Value)
        {
            if (entry.Metadata is { IsDraft: true })
                continue;

            var route = entry.Routes[0];
            var hierarchyParts = route.CanonicalPath.Value
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            var title = entry.Metadata?.Title;
            if (string.IsNullOrWhiteSpace(title))
                title = AutoTitle(entry.Component.Name);

            var order = entry.Metadata is IOrderable orderable ? orderable.Order : int.MaxValue;
            var sectionLabel = entry.Metadata is ISectionable sectionable ? sectionable.SectionLabel : null;
            var excludeFromSearch = entry.Metadata is { Search: false };
            var excludeFromLlms = entry.Metadata is { Llms: false };

            builder.Add(new ContentTocItem(
                Title: title,
                Route: route,
                Order: order,
                HierarchyParts: hierarchyParts,
                SectionLabel: sectionLabel ?? DefaultSectionLabel,
                Locale: null
            )
            {
                ExcludeFromSearch = excludeFromSearch,
                ExcludeFromLlms = excludeFromLlms,
            });
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var entry in _componentMetadataCache.Value)
        {
            if (entry.Metadata is { Uid: { } uid } && !string.IsNullOrEmpty(uid))
            {
                builder.Add(new CrossReference(uid, entry.Metadata.Title, entry.Routes[0]));
            }
        }

        return Task.FromResult(builder.ToImmutable());
    }

    private List<ComponentWithMetadata> BuildComponentMetadataCache()
    {
        var components = new List<ComponentWithMetadata>();

        foreach (var assembly in _assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(ComponentBase).IsAssignableFrom(type) || type.IsAbstract)
                        continue;

                    var routes = new List<ContentRoute>();
                    foreach (var attr in type.GetCustomAttributes<RouteAttribute>())
                    {
                        var template = attr.Template;

                        if (template.Contains('{'))
                            continue;

                        if (template != "/" && !template.EndsWith('/'))
                            _missingTrailingSlashPages.Add((template, type.FullName ?? type.Name));

                        routes.Add(ContentRouteFactory.FromRazorPage(template));
                    }

                    if (routes.Count == 0)
                        continue;

                    var metadata = TryLoadMetadata(type);
                    components.Add(new ComponentWithMetadata(type, routes, metadata));
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned (e.g., dynamic assemblies)
            }
        }

        _logger.LogDebug("Built component metadata cache with {Count} components", components.Count);
        return components;
    }

    private DocFrontMatter? TryLoadMetadata(Type component)
    {
        var sidecarPath = GetSidecarFilePath(component);
        if (sidecarPath is null || !_fileSystem.File.Exists(sidecarPath))
            return null;

        try
        {
            var yamlContent = _fileSystem.File.ReadAllText(sidecarPath);
            if (string.IsNullOrWhiteSpace(yamlContent))
                return null;

            var metadata = _frontMatterParser.DeserializeYaml<DocFrontMatter>(yamlContent);
            _logger.LogDebug("Loaded metadata for component {ComponentName} from {SidecarPath}",
                component.Name, sidecarPath);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse metadata from sidecar file {SidecarPath} for component {ComponentName}",
                sidecarPath, component.Name);
            return null;
        }
    }

    private string? GetSidecarFilePath(Type component)
    {
        var componentName = component.Name;
        var razorFileName = $"{componentName}.razor";
        var componentPath = _razorFileCache.Value.GetValueOrDefault(componentName);

        if (componentPath is null)
            return null;

        var componentDirectory = _fileSystem.Path.GetDirectoryName(componentPath);
        if (string.IsNullOrEmpty(componentDirectory))
            return null;

        var metadataPath = _fileSystem.Path.Combine(componentDirectory, $"{razorFileName}.metadata.yml");
        return _fileSystem.File.Exists(metadataPath) ? metadataPath : null;
    }

    private Dictionary<string, string> BuildRazorFileCache()
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var projectRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in _assemblies)
        {
            var assemblyLocation = assembly.Location;
            if (string.IsNullOrEmpty(assemblyLocation))
                continue;

            var assemblyDir = _fileSystem.Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(assemblyDir))
                continue;

            var projectRoot = FindProjectRoot(assemblyDir);
            if (projectRoot is not null && _fileSystem.Directory.Exists(projectRoot))
                projectRoots.Add(projectRoot);
        }

        foreach (var projectRoot in projectRoots)
        {
            try
            {
                var razorFiles = _fileSystem.Directory.GetFiles(projectRoot, "*.razor",
                    new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true });

                foreach (var filePath in razorFiles)
                {
                    if (ShouldExcludeFile(filePath))
                        continue;

                    var componentName = _fileSystem.Path.GetFileNameWithoutExtension(filePath);
                    if (!cache.ContainsKey(componentName))
                    {
                        cache[componentName] = filePath;
                    }
                    else
                    {
                        _logger.LogDebug("Duplicate component name: {ComponentName}. Using first match, ignoring: {DuplicatePath}",
                            componentName, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error scanning for Razor files in {ProjectRoot}", projectRoot);
            }
        }

        _logger.LogDebug("Built Razor file cache with {Count} components", cache.Count);
        return cache;
    }

    private string? FindProjectRoot(string startDirectory)
    {
        var currentDir = startDirectory;

        while (!string.IsNullOrEmpty(currentDir))
        {
            try
            {
                if (_fileSystem.Directory.Exists(currentDir) &&
                    _fileSystem.Directory.GetFiles(currentDir, "*.csproj").Length > 0)
                {
                    return currentDir;
                }

                var parentDir = _fileSystem.Directory.GetParent(currentDir)?.FullName;
                if (parentDir == currentDir)
                    break;
                currentDir = parentDir;
            }
            catch (DirectoryNotFoundException)
            {
                break;
            }
        }

        return null;
    }

    private static bool ShouldExcludeFile(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');
        return normalizedPath.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
               normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
               normalizedPath.Contains("/node_modules/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Derive a human-readable title from a PascalCase Razor component name.
    /// "ClassDetail" → "Class Detail", "FAQPage" → "FAQ Page".
    /// </summary>
    internal static string AutoTitle(string componentName) =>
        AutoTitleRegex().Replace(componentName, " ");

    [GeneratedRegex(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex AutoTitleRegex();
}