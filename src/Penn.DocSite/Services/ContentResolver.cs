namespace Penn.DocSite.Services;

using System.Collections.Immutable;
using Penn.Content;
using Penn.Diagnostics;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Localization;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Routing;

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

    public ContentResolver(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        NavigationBuilder navBuilder,
        DiagnosticContext diagnostics,
        LocalizationOptions localization)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _navBuilder = navBuilder;
        _diagnostics = diagnostics;
        _localization = localization;
    }

    /// <summary>
    /// Get rendered content for a URL. Returns null if not found.
    /// Handles locale detection and fallback to default locale.
    /// </summary>
    public async Task<ResolvedContent?> GetContentByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        // Try exact URL first (finds locale-specific markdown or exact matches)
        var found = await FindDiscoveredItem(url);
        var locale = _localization.GetLocaleFromUrl(url);
        var isFallback = false;
        string? requestedLocale = null;

        // Fallback: strip locale prefix and try the content-relative path
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
                locale = _localization.DefaultLocale;
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
            RequestedLocale: requestedLocale
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
    /// </summary>
    public async Task<ImmutableList<AlternateLanguagePage>> GetAlternateLanguagesAsync(string url)
    {
        if (!_localization.IsMultiLocale)
            return ImmutableList<AlternateLanguagePage>.Empty;

        url = "/" + url.Trim('/');
        var locale = _localization.GetLocaleFromUrl(url);
        var contentPath = _localization.StripLocalePrefix(url, locale);

        // Collect all TOC entries across all services
        var allItems = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            allItems.AddRange(await service.GetContentTocEntriesAsync());
        }

        var builder = ImmutableList.CreateBuilder<AlternateLanguagePage>();

        foreach (var (localeCode, localeInfo) in _localization.Locales)
        {
            // Check if this locale has the page (or it's locale-agnostic)
            var exists = allItems.Any(item =>
            {
                // Locale-agnostic items (Razor pages, etc.) are available in all locales
                if (item.Locale == null) return NormalizeUrl(item.Route.CanonicalPath.Value) == NormalizeUrl(contentPath);

                if (!string.Equals(item.Locale, localeCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var itemContentPath = _localization.StripLocalePrefix(item.Route.CanonicalPath.Value, localeCode);
                return NormalizeUrl(itemContentPath) == NormalizeUrl(contentPath);
            });

            if (!exists) continue;

            var localeUrl = _localization.BuildLocaleUrl(contentPath.Trim('/'), localeCode);
            builder.Add(new AlternateLanguagePage(
                Locale: localeCode,
                DisplayName: localeInfo.DisplayName,
                Route: new ContentRoute
                {
                    CanonicalPath = new UrlPath(localeUrl),
                    OutputFile = new FilePath($"{localeUrl.Trim('/')}/index.html"),
                    Locale = localeCode
                },
                IsCurrentLocale: string.Equals(localeCode, locale, StringComparison.OrdinalIgnoreCase)
            ));
        }

        return builder.ToImmutable();
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

    private static string NormalizeUrl(string url) => url.Trim('/').ToLowerInvariant();
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
    string? RequestedLocale = null
);
