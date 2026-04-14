namespace Pennington.Infrastructure;

using System.Collections.Immutable;
using System.Reflection;
using Markdig;
using Highlighting;
using Islands;
using LlmsTxt;
using Localization;
using Markdown.Extensions.Tabs;
using Search;

/// <summary>Main configuration options for the Pennington content engine.</summary>
public sealed class PenningtonOptions
{
    public string SiteTitle { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string? CanonicalBaseUrl { get; set; }
    public string ContentRootPath { get; set; } = "Content";

    public HighlightingOptions Highlighting { get; } = new();
    public IslandsOptions Islands { get; } = new();
    public LocalizationOptions Localization { get; } = new();
    public TranslationOptions Translations { get; } = new();

    /// <summary>
    /// Customize the Markdig pipeline after Pennington's built-in extensions (including Mdazor) are added.
    /// Runs with the resolved <see cref="IServiceProvider"/> so extensions requiring DI can be wired up.
    /// </summary>
    public Action<MarkdownPipelineBuilder, IServiceProvider>? ConfigureMarkdownPipeline { get; set; }

    /// <summary>
    /// Override the CSS class names emitted by the tabbed-code-block renderer.
    /// When set, the returned <see cref="TabbedCodeBlockRenderOptions"/> replaces the
    /// <see cref="TabbedCodeBlockRenderOptions.Default"/> shape on the pipeline's
    /// single registration of the tabbed extension.
    /// </summary>
    public Func<TabbedCodeBlockRenderOptions>? TabbedCodeBlockOptions { get; set; }

    private readonly List<MarkdownContentOptions> _markdownSources = [];

    /// <summary>Register a markdown content source with a specific front matter type.</summary>
    public MarkdownContentOptions AddMarkdownContent<TFrontMatter>(Action<MarkdownContentOptions> configure)
        where TFrontMatter : FrontMatter.IFrontMatter
    {
        var options = new MarkdownContentOptions { FrontMatterType = typeof(TFrontMatter) };
        configure(options);
        _markdownSources.Add(options);
        return options;
    }

    public IReadOnlyList<MarkdownContentOptions> MarkdownSources => _markdownSources;

    private LlmsTxtOptions? _llmsTxtOptions;

    /// <summary>Enable llms.txt generation for this site.</summary>
    public LlmsTxtOptions AddLlmsTxt(Action<LlmsTxtOptions>? configure = null)
    {
        _llmsTxtOptions = new LlmsTxtOptions();
        configure?.Invoke(_llmsTxtOptions);
        return _llmsTxtOptions;
    }

    public LlmsTxtOptions? LlmsTxt => _llmsTxtOptions;

    /// <summary>Configuration for the search index.</summary>
    public SearchIndexOptions SearchIndex { get; } = new();

    /// <summary>Assemblies to scan for @page Razor components.</summary>
    public Assembly[] AdditionalRoutingAssemblies { get; set; } = [];
}

/// <summary>Options for a markdown content source.</summary>
public sealed class MarkdownContentOptions
{
    public string ContentPath { get; set; } = "Content";
    public string BasePageUrl { get; set; } = "/";
    public string? SectionLabel { get; set; }

    /// <summary>
    /// Relative subpaths (from <see cref="ContentPath"/>) to skip during discovery
    /// and content copying. Set this on a broad catch-all source to carve out a
    /// subtree that is owned by a more specific markdown source registered nearby.
    /// See <c>MarkdownContentServiceOptions.ExcludePaths</c> for matching semantics.
    /// </summary>
    public ImmutableArray<string> ExcludePaths { get; set; } = ImmutableArray<string>.Empty;

    internal Type? FrontMatterType { get; set; }
}

/// <summary>Options for code highlighting configuration.</summary>
public sealed class HighlightingOptions
{
    private readonly List<ICodeHighlighter> _highlighters = [];

    public void AddHighlighter<T>() where T : ICodeHighlighter, new()
        => _highlighters.Add(new T());

    public void AddHighlighter(ICodeHighlighter highlighter)
        => _highlighters.Add(highlighter);

    public IReadOnlyList<ICodeHighlighter> Highlighters => _highlighters;
}

/// <summary>Options for island registration.</summary>
public sealed class IslandsOptions
{
    private readonly Dictionary<string, Type> _islands = [];

    public void Register<T>(string name) where T : IIslandRenderer
        => _islands[name] = typeof(T);

    public IReadOnlyDictionary<string, Type> RegisteredIslands => _islands;
}

/// <summary>Options for localization.</summary>
public sealed class LocalizationOptions
{
    public string DefaultLocale { get; set; } = "en";
    private readonly Dictionary<string, LocaleInfo> _locales = [];

    public void AddLocale(string code, LocaleInfo info)
        => _locales[code] = info;

    public void AddLocale(string code, string displayName)
        => _locales[code] = new LocaleInfo(displayName);

    public IReadOnlyDictionary<string, LocaleInfo> Locales => _locales;

    /// <summary>True when more than one locale is configured.</summary>
    public bool IsMultiLocale => _locales.Count > 1;

    /// <summary>
    /// Extracts the locale code from a URL path.
    /// Returns the default locale when the first segment is not a known non-default locale.
    /// </summary>
    public string GetLocaleFromUrl(string url)
    {
        if (!IsMultiLocale) return DefaultLocale;

        var trimmed = url.TrimStart('/');
        var firstSlash = trimmed.IndexOf('/');
        var firstSegment = firstSlash >= 0 ? trimmed[..firstSlash] : trimmed;

        if (!string.IsNullOrEmpty(firstSegment)
            && _locales.ContainsKey(firstSegment)
            && !string.Equals(firstSegment, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return firstSegment;
        }

        return DefaultLocale;
    }

    /// <summary>
    /// Strips the locale prefix from a URL, returning the content-relative path.
    /// For the default locale (no prefix), returns the URL unchanged.
    /// </summary>
    public string StripLocalePrefix(string url, string locale)
    {
        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
            return url;

        var trimmed = url.TrimStart('/');
        var prefix = locale + "/";

        if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return "/" + trimmed[prefix.Length..];

        // URL is just the locale with no trailing path
        if (string.Equals(trimmed, locale, StringComparison.OrdinalIgnoreCase))
            return "/";

        return url;
    }

    /// <summary>
    /// Builds a full URL for a content path in a specific locale.
    /// </summary>
    public string BuildLocaleUrl(string contentPath, string locale)
    {
        var path = contentPath.Trim('/');

        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrEmpty(path) ? "/" : $"/{path}/";

        return string.IsNullOrEmpty(path) ? $"/{locale}/" : $"/{locale}/{path}/";
    }

    /// <summary>
    /// Gets alternate language versions for a page URL across all configured locales.
    /// Pure URL math — does not check if content exists (fallback handles that).
    /// </summary>
    public IReadOnlyList<AlternateLanguage> GetAlternateLanguages(string url)
    {
        if (!IsMultiLocale) return [];

        // The 404-generation sentinel is not a real content page. Treat it as
        // the locale root so language switcher links resolve to each locale's
        // landing page instead of phantom /<locale>/__pennington-404-generator/.
        if (url.Equals(Generation.OutputGenerationService.NotFoundGeneratorPath, StringComparison.Ordinal)
            || url.Equals(Generation.OutputGenerationService.NotFoundGeneratorPath + "/", StringComparison.Ordinal))
        {
            url = "/";
        }

        url = "/" + url.Trim('/');
        if (url.Equals("/index", StringComparison.OrdinalIgnoreCase))
            url = "/";

        var locale = GetLocaleFromUrl(url);
        var contentPath = StripLocalePrefix(url, locale);

        var result = new List<AlternateLanguage>();
        foreach (var (code, info) in _locales)
        {
            var localeUrl = BuildLocaleUrl(contentPath.Trim('/'), code);
            result.Add(new AlternateLanguage(
                Locale: code,
                DisplayName: info.DisplayName,
                HtmlLang: info.HtmlLang ?? code,
                Url: localeUrl,
                IsCurrentLocale: string.Equals(code, locale, StringComparison.OrdinalIgnoreCase)));
        }

        return result;
    }
}

/// <summary>
/// Represents one language version of a page, used for language switchers
/// and hreflang link tags. Content-route-independent (pure URL math).
/// </summary>
public record AlternateLanguage(
    string Locale,
    string DisplayName,
    string HtmlLang,
    string Url,
    bool IsCurrentLocale = false
);