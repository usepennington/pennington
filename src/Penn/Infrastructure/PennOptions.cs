namespace Penn.Infrastructure;

using System.Reflection;
using Penn.Highlighting;
using Penn.Islands;
using Penn.Localization;

/// <summary>Main configuration options for the Penn content engine.</summary>
public sealed class PennOptions
{
    public string SiteTitle { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string? CanonicalBaseUrl { get; set; }
    public string ContentRootPath { get; set; } = "Content";

    public HighlightingOptions Highlighting { get; } = new();
    public IslandsOptions Islands { get; } = new();
    public LocalizationOptions Localization { get; } = new();

    private readonly List<MarkdownContentOptions> _markdownSources = [];

    /// <summary>Register a markdown content source with a specific front matter type.</summary>
    public MarkdownContentOptions AddMarkdownContent<TFrontMatter>(Action<MarkdownContentOptions> configure)
        where TFrontMatter : Penn.FrontMatter.IFrontMatter
    {
        var options = new MarkdownContentOptions { FrontMatterType = typeof(TFrontMatter) };
        configure(options);
        _markdownSources.Add(options);
        return options;
    }

    public IReadOnlyList<MarkdownContentOptions> MarkdownSources => _markdownSources;

    /// <summary>Assemblies to scan for @page Razor components.</summary>
    public Assembly[] AdditionalRoutingAssemblies { get; set; } = [];
}

/// <summary>Options for a markdown content source.</summary>
public sealed class MarkdownContentOptions
{
    public string ContentPath { get; set; } = "Content";
    public string BasePageUrl { get; set; } = "/";
    public string? Section { get; set; }
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
}
