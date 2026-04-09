namespace Pennington.DocSite.Services;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Diagnostics;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Resolves content pages by URL for the DocSite.
/// Locale-aware: detects locale from URL, searches locale-specific content first,
/// then falls back to the default locale. Works with all content sources (markdown,
/// Razor pages, custom services).
/// </summary>
public sealed class ContentResolver
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly NavigationBuilder _navBuilder;
    private readonly DiagnosticContext _diagnostics;
    private readonly LocalizationOptions _localization;
    private readonly DocSiteOptions _docSiteOptions;

    public ContentResolver(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        NavigationBuilder navBuilder,
        DiagnosticContext diagnostics,
        LocalizationOptions localization,
        DocSiteOptions docSiteOptions)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _navBuilder = navBuilder;
        _diagnostics = diagnostics;
        _localization = localization;
        _docSiteOptions = docSiteOptions;
    }

    /// <summary>
    /// Get rendered content for a URL. Returns null if not found.
    /// Handles locale detection and fallback to default locale.
    /// </summary>
    public async Task<ResolvedContent?> GetContentByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        // Normalize bare "/index" to "/" so it matches the homepage route
        if (url.Equals("/index", StringComparison.OrdinalIgnoreCase))
            url = "/";

        // Try exact URL first (finds locale-specific markdown, fallback entries, or exact matches)
        var found = await FindDiscoveredItem(url);
        var locale = _localization.GetLocaleFromUrl(url);
        var isFallback = found?.Route.IsFallback ?? false;
        string? requestedLocale = isFallback ? locale : null;
        if (isFallback) locale = _localization.DefaultLocale;

        // Runtime fallback: strip locale prefix and try the content-relative path
        if (found == null
            && _localization.IsMultiLocale
            && !string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            var contentPath = _localization.StripLocalePrefix(url, locale);
            found = await FindDiscoveredItem(contentPath);
            if (found != null)
            {
                isFallback = true;
                requestedLocale = locale;
            }
        }

        if (found == null) return null;

        // Parse
        var parseResult = await ParseItem(found);
        if (parseResult == null) return null;

        // Render
        var renderResult = await _renderer.RenderAsync(parseResult);
        if (renderResult is FailedItem failed)
        {
            _diagnostics.AddError(failed.Error.Message, "ContentResolver");
            return null;
        }
        if (renderResult is not RenderedItem rendered) return null;

        return new ResolvedContent(
            Route: rendered.Route,
            Title: rendered.Metadata.Title,
            Description: (rendered.Metadata as IDescribable)?.Description,
            Html: rendered.Content.Html,
            Outline: rendered.Content.Outline,
            Metadata: rendered.Metadata,
            Locale: locale,
            IsFallback: isFallback,
            RequestedLocale: requestedLocale,
            FallbackRequestedDisplayName: isFallback
                ? (_localization.Locales.TryGetValue(requestedLocale ?? "", out var reqInfo) ? reqInfo.DisplayName : requestedLocale)
                : null,
            FallbackDefaultDisplayName: isFallback
                ? (_localization.Locales.TryGetValue(locale, out var defInfo) ? defInfo.DisplayName : locale)
                : null
        );
    }

    /// <summary>
    /// Get navigation info for a URL, filtered by locale.
    /// </summary>
    public async Task<NavigationInfo?> GetNavigationInfoAsync(string url)
    {
        url = "/" + url.Trim('/');
        var locale = _localization.IsMultiLocale ? _localization.GetLocaleFromUrl(url) : null;

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(url),
            OutputFile = new FilePath($"{url.TrimStart('/')}/index.html"),
        };

        var tocItems = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        return _navBuilder.BuildNavigationInfo(tocItems, route, locale);
    }

    /// <summary>
    /// Get all TOC items, optionally filtered by locale.
    /// </summary>
    public async Task<IReadOnlyList<ContentTocItem>> GetTocItemsAsync(string? locale = null)
    {
        var items = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            items.AddRange(await service.GetContentTocEntriesAsync());
        }

        if (locale != null)
        {
            items = items
                .Where(i => i.Locale == null
                    || string.Equals(i.Locale, locale, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return items;
    }

    /// <summary>
    /// Get alternate language versions for a page URL.
    /// Always includes all configured locales — fallback resolution handles missing translations.
    /// Delegates URL math to <see cref="LocalizationOptions.GetAlternateLanguages"/> and
    /// wraps results with <see cref="ContentRoute"/> for DocSite consumption.
    /// </summary>
    public Task<ImmutableList<AlternateLanguagePage>> GetAlternateLanguagesAsync(string url)
    {
        if (!_localization.IsMultiLocale)
            return Task.FromResult(ImmutableList<AlternateLanguagePage>.Empty);

        var alternates = _localization.GetAlternateLanguages(url);
        var builder = ImmutableList.CreateBuilder<AlternateLanguagePage>();

        foreach (var alt in alternates)
        {
            builder.Add(new AlternateLanguagePage(
                Locale: alt.Locale,
                DisplayName: alt.DisplayName,
                Route: new ContentRoute
                {
                    CanonicalPath = new UrlPath(alt.Url),
                    OutputFile = new FilePath($"{alt.Url.Trim('/')}/index.html"),
                    Locale = alt.Locale,
                },
                IsCurrentLocale: alt.IsCurrentLocale));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <summary>
    /// Resolves which content area the given URL belongs to, based on the first path segment
    /// matching a configured area slug. Returns null if no area matches.
    /// </summary>
    public ContentArea? ResolveCurrentArea(string url)
    {
        if (_docSiteOptions.Areas.Count == 0) return null;

        var segments = url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var firstSegment = segments.Length > 0 ? segments[0] : null;
        if (firstSegment == null) return null;

        // For multi-locale URLs, the first segment is the locale — check the second segment
        if (_localization.IsMultiLocale
            && _localization.Locales.ContainsKey(firstSegment))
        {
            firstSegment = segments.Length > 1 ? segments[1] : null;
            if (firstSegment == null) return null;
        }

        return _docSiteOptions.Areas.FirstOrDefault(a =>
            string.Equals(a.Slug, firstSegment, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get TOC items scoped to a specific area. Filters by area slug matching
    /// HierarchyParts[0] and strips the area prefix, mirroring the locale-stripping
    /// pattern in NavigationBuilder.
    /// </summary>
    public async Task<IReadOnlyList<ContentTocItem>> GetTocItemsForAreaAsync(
        string? locale, ContentArea? area)
    {
        var items = await GetTocItemsAsync(locale);
        if (area == null) return items;

        return items
            .Where(i => i.HierarchyParts.Length > 0
                && string.Equals(i.HierarchyParts[0], area.Slug,
                    StringComparison.OrdinalIgnoreCase))
            .Select(i => i with { HierarchyParts = i.HierarchyParts[1..] })
            .ToList();
    }

    /// <summary>
    /// Get navigation info (prev/next/breadcrumbs) scoped to an area.
    /// When area is null, falls back to the full-site navigation.
    /// </summary>
    public async Task<NavigationInfo?> GetNavigationInfoForAreaAsync(
        string url, ContentArea? area)
    {
        url = "/" + url.Trim('/');
        var locale = _localization.IsMultiLocale ? _localization.GetLocaleFromUrl(url) : null;

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(url),
            OutputFile = new FilePath($"{url.TrimStart('/')}/index.html"),
        };

        var tocItems = await GetTocItemsForAreaAsync(locale, area);
        return _navBuilder.BuildNavigationInfo(tocItems.ToList(), route, locale);
    }

    private async Task<DiscoveredItem?> FindDiscoveredItem(string url)
    {
        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (item.Route.CanonicalPath.Matches(new UrlPath(url)))
                    return item;
            }
        }
        return null;
    }

    private async Task<ParsedItem?> ParseItem(DiscoveredItem item)
    {
        if (item.Source is not MarkdownFileSource source) return null;

        var content = await File.ReadAllTextAsync(source.Path.Value);
        var result = _parser.Parse<DocSiteFrontMatter>(content);
        var metadata = result.Metadata ?? new DocSiteFrontMatter();

        return new ParsedItem(item.Route, metadata, result.Body);
    }

}

public record ResolvedContent(
    ContentRoute Route,
    string Title,
    string? Description,
    string Html,
    OutlineEntry[] Outline,
    IFrontMatter Metadata,
    string Locale = "",
    bool IsFallback = false,
    string? RequestedLocale = null,
    string? FallbackRequestedDisplayName = null,
    string? FallbackDefaultDisplayName = null
);
