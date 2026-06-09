namespace Pennington.Infrastructure;

using System.Collections.Immutable;
using System.Reflection;
using FrontMatter;
using Highlighting;
using LlmsTxt;
using Localization;
using Markdig;
using Markdown.Extensions.Tabs;
using Pipeline;
using Routing;
using Search;
using SocialCards;
using StandardSite;

/// <summary>Main configuration options for the Pennington content engine.</summary>
public sealed class PenningtonOptions
{
    /// <summary>Site title shown in the browser tab, OpenGraph tags, and RSS feed.</summary>
    public string SiteTitle { get; set; } = "";

    /// <summary>Default site description used for meta tags when a page supplies none.</summary>
    public string SiteDescription { get; set; } = "";

    /// <summary>Absolute base URL used to generate canonical, OpenGraph, and feed links.</summary>
    public string? CanonicalBaseUrl { get; set; }

    /// <summary>
    /// Site-level author fallback for auto-emitted JSON-LD, surfaced as
    /// <see cref="StructuredData.StructuredDataContext.FallbackAuthorName"/> when a record's front
    /// matter describes structured data but names no author of its own. BlogSite forwards its
    /// <c>BlogSiteOptions.AuthorName</c> here.
    /// </summary>
    public string? StructuredDataAuthorName { get; set; }

    /// <summary>Root filesystem directory containing site content.</summary>
    public FilePath ContentRootPath { get; set; } = new("Content");

    /// <summary>Code-highlighting configuration.</summary>
    public HighlightingOptions Highlighting { get; } = new();

    /// <summary>Front-matter parser configuration (strict-mode toggle).</summary>
    public FrontMatterParserOptions FrontMatter { get; } = new();

    /// <summary>Localization configuration, including locales and defaults.</summary>
    public LocalizationOptions Localization { get; } = new();

    /// <summary>Translation string configuration for localized UI.</summary>
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
        where TFrontMatter : IFrontMatter
    {
        var options = new MarkdownContentOptions { FrontMatterType = typeof(TFrontMatter) };
        configure(options);
        _markdownSources.Add(options);
        return options;
    }

    /// <summary>Markdown content sources registered via <see cref="AddMarkdownContent{TFrontMatter}"/>.</summary>
    public IReadOnlyList<MarkdownContentOptions> MarkdownSources => _markdownSources;

    private readonly List<ContentFormatOptions> _contentFormats = [];

    /// <summary>
    /// Register a content source for a custom file format. The format's <see cref="IContentParser"/> and
    /// <see cref="IContentRenderer"/> are supplied via <see cref="ContentFormatOptions.UseParser{T}"/> /
    /// <see cref="ContentFormatOptions.UseRenderer{T}"/> and the pipeline routes to them by
    /// <paramref name="format"/>. The format's files are discovered, parsed, and rendered through the same
    /// pipeline as markdown.
    /// </summary>
    public ContentFormatOptions AddContentFormat<TFrontMatter>(string format, Action<ContentFormatOptions> configure)
        where TFrontMatter : IFrontMatter
    {
        var options = new ContentFormatOptions { Format = format, FrontMatterType = typeof(TFrontMatter) };
        configure(options);
        _contentFormats.Add(options);
        return options;
    }

    /// <summary>Content formats registered via <see cref="AddContentFormat{TFrontMatter}"/>.</summary>
    public IReadOnlyList<ContentFormatOptions> ContentFormats => _contentFormats;

    private LlmsTxtOptions? _llmsTxtOptions;

    /// <summary>Enable llms.txt generation for this site.</summary>
    public LlmsTxtOptions AddLlmsTxt(Action<LlmsTxtOptions>? configure = null)
    {
        _llmsTxtOptions = new LlmsTxtOptions();
        configure?.Invoke(_llmsTxtOptions);
        return _llmsTxtOptions;
    }

    /// <summary>llms.txt options registered via <see cref="AddLlmsTxt"/>, or <c>null</c> when not enabled.</summary>
    public LlmsTxtOptions? LlmsTxt => _llmsTxtOptions;

    /// <summary>
    /// Social-card (OpenGraph / Twitter image) generation. Set to enable per-page card discovery,
    /// the on-demand rendering endpoint, and the meta-tag wiring; null disables the feature.
    /// Templates forward their own option into this.
    /// </summary>
    public SocialCardOptions? SocialCards { get; set; }

    /// <summary>
    /// Standard Site (AT Protocol long-form publishing) integration. Set to emit the verification
    /// well-known files and per-page <c>site.standard.*</c> head links; null disables the feature.
    /// Templates forward their own option into this.
    /// </summary>
    public StandardSiteOptions? StandardSite { get; set; }

    /// <summary>Configuration for the search index.</summary>
    public SearchIndexOptions SearchIndex { get; } = new();

    /// <summary>
    /// Configuration for the shared <see cref="ISiteProjection"/> consumed by
    /// every corpus aggregator (search index, llms.txt, build-time link audit).
    /// Set <see cref="SiteProjectionOptions.ContentSelector"/> here when the
    /// layout wraps content in chrome that should be stripped before indexing
    /// or extracting markdown.
    /// </summary>
    public SiteProjectionOptions SiteProjection { get; } = new();

    /// <summary>
    /// Extra assemblies to scan for routable <c>@page</c> Razor components. The
    /// entry assembly is always scanned, so a bare host need only set this to add
    /// components defined in other assemblies.
    /// </summary>
    public Assembly[] AdditionalRoutingAssemblies { get; set; } = [];

    /// <summary>
    /// When true (the default), <see cref="PenningtonExtensions.UsePennington"/>
    /// maps the <c>/sitemap.xml</c> endpoint. Set to false to suppress the
    /// endpoint when the host environment supplies its own sitemap. Template
    /// extensions like <c>AddBlogSite</c> forward their own toggle into this
    /// flag.
    /// </summary>
    public bool MapSitemap { get; set; } = true;
}

/// <summary>Options for a markdown content source.</summary>
public sealed class MarkdownContentOptions
{
    /// <summary>Filesystem path to the directory containing markdown files.</summary>
    public string ContentPath { get; set; } = "Content";

    /// <summary>URL prefix prepended to routes generated from this source.</summary>
    public string BasePageUrl { get; set; } = "/";

    /// <summary>Default section label applied when front matter does not specify one.</summary>
    public string? SectionLabel { get; set; }

    /// <summary>
    /// Relative subpaths (from <see cref="ContentPath"/>) to skip during discovery
    /// and content copying. Set this on a broad catch-all source to carve out a
    /// subtree that is owned by a more specific markdown source registered nearby.
    /// See <c>MarkdownContentServiceOptions.ExcludePaths</c> for matching semantics.
    /// </summary>
    public ImmutableArray<string> ExcludePaths { get; set; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// When true, a content-root <c>404.md</c> is reserved as the not-found body: skipped during
    /// discovery so it never becomes a routable page or a nav/sitemap/search/llms entry. Host
    /// templates render it on demand as the 404 page. See
    /// <c>MarkdownContentServiceOptions.ReserveNotFoundPage</c>.
    /// </summary>
    public bool ReserveNotFoundPage { get; set; }

    internal Type? FrontMatterType { get; set; }
}

/// <summary>Options for a custom content-format source registered via <see cref="PenningtonOptions.AddContentFormat{TFrontMatter}"/>.</summary>
public sealed class ContentFormatOptions
{
    /// <summary>Filesystem path to the directory containing the format's source files.</summary>
    public string ContentPath { get; set; } = "Content";

    /// <summary>URL prefix prepended to routes generated from this source.</summary>
    public string BasePageUrl { get; set; } = "/";

    /// <summary>Glob pattern used to enumerate source files (for example <c>*.cook</c>).</summary>
    public string FilePattern { get; set; } = "*.*";

    /// <summary>Default section label applied when front matter does not specify one.</summary>
    public string? SectionLabel { get; set; }

    /// <summary>Relative subpaths (from <see cref="ContentPath"/>) to skip during discovery.</summary>
    public ImmutableArray<string> ExcludePaths { get; set; } = ImmutableArray<string>.Empty;

    internal string Format { get; set; } = "";
    internal Type? FrontMatterType { get; set; }
    internal Type? ParserType { get; set; }
    internal Type? RendererType { get; set; }

    /// <summary>Sets the <see cref="IContentParser"/> type that parses this format's files (resolved from DI).</summary>
    public ContentFormatOptions UseParser<TParser>() where TParser : class, IContentParser
    {
        ParserType = typeof(TParser);
        return this;
    }

    /// <summary>Sets the <see cref="IContentRenderer"/> type that renders this format's parsed items (resolved from DI).</summary>
    public ContentFormatOptions UseRenderer<TRenderer>() where TRenderer : class, IContentRenderer
    {
        RendererType = typeof(TRenderer);
        return this;
    }
}

/// <summary>Options for code highlighting configuration.</summary>
public sealed class HighlightingOptions
{
    private readonly List<ICodeHighlighter> _highlighters = [];

    /// <summary>Registers a highlighter type, constructed with its parameterless constructor.</summary>
    public void AddHighlighter<T>() where T : ICodeHighlighter, new()
        => _highlighters.Add(new T());

    /// <summary>Registers a pre-built highlighter instance.</summary>
    public void AddHighlighter(ICodeHighlighter highlighter)
        => _highlighters.Add(highlighter);

    /// <summary>Highlighters registered via <see cref="AddHighlighter(ICodeHighlighter)"/> or the generic overload.</summary>
    public IReadOnlyList<ICodeHighlighter> Highlighters => _highlighters;
}

/// <summary>Options for localization.</summary>
public sealed class LocalizationOptions
{
    /// <summary>Locale code used when no URL locale prefix is present.</summary>
    public string DefaultLocale { get; set; } = "en";
    private readonly Dictionary<string, LocaleInfo> _locales = [];

    /// <summary>Registers a locale with the supplied metadata.</summary>
    public void AddLocale(string code, LocaleInfo info)
        => _locales[code] = info;

    /// <summary>Registers a locale with just a display name.</summary>
    public void AddLocale(string code, string displayName)
        => _locales[code] = new LocaleInfo(displayName);

    /// <summary>Configured locales keyed by locale code.</summary>
    public IReadOnlyDictionary<string, LocaleInfo> Locales => _locales;

    /// <summary>True when more than one locale is configured.</summary>
    public bool IsMultiLocale => _locales.Count > 1;

    /// <summary>
    /// Extracts the locale code from a URL path.
    /// Returns the default locale when the first segment is not a known non-default locale.
    /// </summary>
    public string GetLocaleFromUrl(string url)
    {
        if (!IsMultiLocale)
        {
            return DefaultLocale;
        }

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
        {
            return url;
        }

        var trimmed = url.TrimStart('/');
        var prefix = locale + "/";

        if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return "/" + trimmed[prefix.Length..];
        }

        // URL is just the locale with no trailing path
        if (string.Equals(trimmed, locale, StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        return url;
    }

    /// <summary>
    /// Builds a full URL for a content path in a specific locale.
    /// </summary>
    public string BuildLocaleUrl(string contentPath, string locale)
    {
        var path = contentPath.Trim('/');

        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrEmpty(path) ? "/" : $"/{path}/";
        }

        return string.IsNullOrEmpty(path) ? $"/{locale}/" : $"/{locale}/{path}/";
    }

    /// <summary>
    /// Gets alternate language versions for a page URL across all configured locales.
    /// Pure URL math — does not check if content exists (fallback handles that).
    /// </summary>
    public IReadOnlyList<AlternateLanguage> GetAlternateLanguages(string url)
    {
        if (!IsMultiLocale)
        {
            return [];
        }

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
        {
            url = "/";
        }

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
/// <param name="Locale">Locale code (e.g. <c>en</c>, <c>fr</c>, <c>pt-BR</c>).</param>
/// <param name="DisplayName">User-visible language name.</param>
/// <param name="HtmlLang">Value to emit in <c>hreflang</c> and <c>lang</c> attributes.</param>
/// <param name="Url">URL of the page in this locale.</param>
/// <param name="IsCurrentLocale">True when this entry represents the current request locale.</param>
public record AlternateLanguage(
    string Locale,
    string DisplayName,
    string HtmlLang,
    string Url,
    bool IsCurrentLocale = false
);