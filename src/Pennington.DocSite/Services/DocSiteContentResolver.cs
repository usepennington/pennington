namespace Pennington.DocSite.Services;

using System.Collections.Immutable;
using Content;
using FrontMatter;
using Infrastructure;
using Localization;
using Navigation;
using Pipeline;
using Routing;

/// <summary>
/// The DocSite's per-request content facade. Resolves a page by URL (delegating the
/// discover → parse → render step to the core <see cref="IPageResolver"/>) and adds the
/// DocSite-specific concerns around it: locale detection with fallback to the default locale,
/// the <see cref="ResolvedContent"/> view-model, navigation/TOC, alternate languages, and area
/// scoping. Distinct from <see cref="IPageResolver"/>, which is the locale-naive single-page
/// primitive shared with bare hosts.
/// </summary>
public sealed class DocSiteContentResolver
{
    private readonly IEnumerable<IContentService> _services;
    private readonly IPageResolver _pageResolver;
    private readonly NavigationBuilder _navBuilder;
    private readonly LocalizationOptions _localization;
    private readonly DocSiteOptions _docSiteOptions;
    private readonly IContentParser? _parser;
    private readonly IContentRenderer? _renderer;

    /// <summary>Creates a new resolver with the supplied content services, page resolver, options, and pipeline primitives.</summary>
    public DocSiteContentResolver(
        IEnumerable<IContentService> services,
        IPageResolver pageResolver,
        NavigationBuilder navBuilder,
        LocalizationOptions localization,
        DocSiteOptions docSiteOptions,
        IContentParser? parser = null,
        IContentRenderer? renderer = null)
    {
        _services = services;
        _pageResolver = pageResolver;
        _navBuilder = navBuilder;
        _localization = localization;
        _docSiteOptions = docSiteOptions;
        _parser = parser;
        _renderer = renderer;
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
        {
            url = "/";
        }

        // Try exact URL first (finds locale-specific markdown, fallback entries, or exact matches).
        // IPageResolver runs discover → parse (per-source typed) → render and returns the rendered page.
        var rendered = await _pageResolver.ResolveAsync(new UrlPath(url));
        var locale = _localization.GetLocaleFromUrl(url);
        var isFallback = rendered?.Route.IsFallback ?? false;
        var requestedLocale = isFallback ? locale : null;
        if (isFallback)
        {
            locale = _localization.DefaultLocale;
        }

        // Runtime fallback: strip locale prefix and try the content-relative path
        if (rendered == null
            && _localization.IsMultiLocale
            && !string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            var contentPath = _localization.StripLocalePrefix(url, locale);
            rendered = await _pageResolver.ResolveAsync(new UrlPath(contentPath));
            if (rendered != null)
            {
                isFallback = true;
                requestedLocale = locale;
            }
        }

        if (rendered == null)
        {
            return null;
        }

        return new ResolvedContent(
            Route: rendered.Route,
            Title: rendered.Metadata.Title,
            Description: rendered.Metadata.Description,
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
    /// Resolves the site's not-found body from a content-root <c>404.md</c>, rendered through the
    /// full markdown pipeline. Returns null when no <c>404.md</c> exists (the catch-all then tries
    /// a <c>NotFound</c> component, then the built-in message) or when the host registered no
    /// markdown parser. The file is reserved out of discovery
    /// (<see cref="MarkdownContentServiceOptions.ReserveNotFoundPage"/>), so it is never a routable
    /// page. One body serves every locale: the static build emits a single root <c>404.html</c>,
    /// which is all any static host serves for an unknown URL.
    /// </summary>
    public async Task<ResolvedContent?> GetNotFoundContentAsync()
    {
        if (_parser is null || _renderer is null)
        {
            return null;
        }

        // The DocSite's primary markdown source is rooted at "/" (the doc tree); its
        // AbsoluteContentRoot is where 404.md lives. It's registered first, so its dispatch
        // key is MarkdownFormat.Key (per-source keys come from MarkdownFormat.SourceKey — if
        // the doc source were ever registered after another markdown source, capture the
        // source's real key instead of assuming the default).
        var source = _services.OfType<IMarkdownContentSource>()
            .FirstOrDefault(s => s.BasePageUrl.Value == "/");
        if (source is null)
        {
            return null;
        }

        var path = Path.Combine(
            source.AbsoluteContentRoot, MarkdownContentService<DocSiteFrontMatter>.NotFoundPageFileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/404"),
            OutputFile = new FilePath("404.html"),
        };
        var discovered = new DiscoveredItem(route, new FileSource(new FilePath(path), MarkdownFormat.Key));

        var parsed = await _parser.ParseAsync(discovered);
        if (parsed.Value is not ParsedItem parsedItem)
        {
            return null;
        }

        var rendered = await _renderer.RenderAsync(parsedItem);
        if (rendered.Value is not RenderedItem renderedItem)
        {
            return null;
        }

        return new ResolvedContent(
            Route: renderedItem.Route,
            Title: renderedItem.Metadata.Title,
            Description: renderedItem.Metadata.Description,
            Html: renderedItem.Content.Html,
            Outline: renderedItem.Content.Outline,
            Metadata: renderedItem.Metadata);
    }

    /// <summary>
    /// Get navigation info for a URL, filtered by locale.
    /// </summary>
    public async Task<NavigationInfo?> GetNavigationInfoAsync(string url)
    {
        url = "/" + url.Trim('/');
        var locale = _localization.IsMultiLocale ? _localization.GetLocaleFromUrl(url) : null;

        var tocItems = await _services.CollectTocEntriesAsync();
        return await _navBuilder.BuildNavigationInfoAsync(tocItems.ToList(), new UrlPath(url), locale);
    }

    /// <summary>
    /// Get all TOC items, optionally filtered by locale.
    /// </summary>
    public async Task<IReadOnlyList<ContentTocItem>> GetTocItemsAsync(string? locale = null)
    {
        var items = await _services.CollectTocEntriesAsync();

        if (locale != null)
        {
            return items
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
        {
            return Task.FromResult(ImmutableList<AlternateLanguagePage>.Empty);
        }

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
        if (_docSiteOptions.Areas.Count == 0)
        {
            return null;
        }

        var segments = url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var firstSegment = segments.Length > 0 ? segments[0] : null;
        if (firstSegment == null)
        {
            return null;
        }

        // For multi-locale URLs, the first segment is the locale — check the second segment
        if (_localization.IsMultiLocale
            && _localization.Locales.ContainsKey(firstSegment))
        {
            firstSegment = segments.Length > 1 ? segments[1] : null;
            if (firstSegment == null)
            {
                return null;
            }
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
        if (area == null)
        {
            return items;
        }

        // Strip the leading locale segment before matching the area slug — for a
        // non-default locale the hierarchy is [locale, area, …], so matching the
        // raw HierarchyParts[0] never hits and the localized area TOC comes back
        // empty. Mirrors NavigationBuilder.FilterByLocale.
        return items
            .Select(i => i.Locale != null
                    && locale != null
                    && i.HierarchyParts.Length > 0
                    && string.Equals(i.HierarchyParts[0], locale, StringComparison.OrdinalIgnoreCase)
                ? i with { HierarchyParts = i.HierarchyParts[1..] }
                : i)
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

        var tocItems = await GetTocItemsForAreaAsync(locale, area);
        return await _navBuilder.BuildNavigationInfoAsync(tocItems.ToList(), new UrlPath(url), locale);
    }
}

/// <summary>Rendered content plus the surrounding locale/fallback metadata needed to render a page.</summary>
/// <param name="Route">Canonical route for the resolved page.</param>
/// <param name="Title">Page title from front matter.</param>
/// <param name="Description">Page description from front matter.</param>
/// <param name="Html">Rendered HTML body.</param>
/// <param name="Outline">Headings extracted from the body for the on-page outline.</param>
/// <param name="Metadata">Parsed front matter for the page.</param>
/// <param name="Locale">Locale used to render the page (may differ from the requested locale when falling back).</param>
/// <param name="IsFallback">True when content from the default locale was served because the requested locale had no match.</param>
/// <param name="RequestedLocale">Locale the user originally requested, when different from <paramref name="Locale"/>.</param>
/// <param name="FallbackRequestedDisplayName">Display name for the requested locale, shown in the fallback notice.</param>
/// <param name="FallbackDefaultDisplayName">Display name for the locale actually served, shown in the fallback notice.</param>
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
