using MonorailCss;
using MonorailCss.Discovery;
using MonorailCss.Theme;
using Pennington.MonorailCss.Internal;

namespace Pennington.MonorailCss;

/// <summary>
/// Generates CSS stylesheets using MonorailCSS with collected utility classes.
/// </summary>
/// <param name="options">MonorailCSS configuration options.</param>
/// <param name="engine">Configured framework wrapper shared with the discovery pipeline.</param>
/// <param name="classRegistry">Live snapshot of classes discovered by the runtime scanner.</param>
public class MonorailCssService(
    MonorailCssOptions options,
    MonorailCssEngine engine,
    IClassRegistry classRegistry)
{
    private string? _cachedStyleSheet;
    private string? _cachedRegistryVersion;
    private readonly Lock _cacheLock = new();

    /// <summary>
    /// Processes the discovered CSS classes and returns the generated stylesheet. The result
    /// is cached until the discovery registry's version token changes, so repeated GETs of
    /// <c>/styles.css</c> are served from memory.
    /// </summary>
    public string GetStyleSheet()
    {
        var version = classRegistry.Version;

        lock (_cacheLock)
        {
            if (_cachedStyleSheet is not null && _cachedRegistryVersion == version)
            {
                return _cachedStyleSheet;
            }
        }

        var classes = classRegistry.GetClasses();
        var styleSheet = engine.Framework.Process(classes);

        var result = $"""
                {ContentVisibilityRules}

                {options.ExtraStyles}

                {styleSheet}
                """;

        lock (_cacheLock)
        {
            _cachedStyleSheet = result;
            _cachedRegistryVersion = version;
        }

        return result;
    }

    // Paired content-visibility classes consumed by the llms.txt and search pipelines.
    // .humans-only has no browser-side effect — it's a marker the extractor honors.
    // .robots-only hides from browsers via display:none; the markup is still emitted,
    // so automated extraction keeps it.
    private const string ContentVisibilityRules = """
        /* Pennington content-visibility markers. */
        /* .humans-only — visible in the browser; stripped from llms.txt extraction. */
        /* .robots-only — hidden in the browser; kept in llms.txt extraction. */
        .robots-only { display: none; }
        """;

    /// <summary>
    /// Builds a fully-configured <see cref="CssFramework"/> from Pennington's options.
    /// Called once during DI registration so the framework can be reused by both the
    /// discovery pipeline (for candidate validation) and the stylesheet endpoint
    /// (for CSS generation), keeping the theme consistent across both.
    /// </summary>
    public static CssFramework BuildFramework(MonorailCssOptions options)
    {
        var theme = options.ColorScheme.ApplyToTheme(Theme.CreateWithDefaults());

        var settings = new CssFrameworkSettings
        {
            Theme = theme,
            Applies = PenningtonApplies.All(options.SyntaxTheme),
            CustomUtilities = PenningtonApplies.ScrollbarUtilities,
            ProseCustomization = PenningtonProseRules.Default,
        };

        return new CssFramework(options.CustomCssFrameworkSettings(settings));
    }
}
