using MonorailCss;
using MonorailCss.Discovery;
using MonorailCss.Theme;
using Pennington.MonorailCss.Internal;

namespace Pennington.MonorailCss;

/// <summary>
/// Wraps the discovery-pipeline output with Pennington's stylesheet prefix
/// (<see cref="ContentVisibilityRules"/> + <see cref="MonorailCssOptions.ExtraStyles"/>)
/// and serves the result on every <c>/styles.css</c> hit.
/// </summary>
/// <param name="options">MonorailCSS configuration options.</param>
/// <param name="classRegistry">Live snapshot of classes + generated CSS from the runtime scanner.</param>
public class MonorailCssService(
    MonorailCssOptions options,
    IClassRegistry classRegistry)
{
    private string? _cachedStyleSheet;
    private string? _cachedRegistryVersion;
    private readonly Lock _cacheLock = new();

    /// <summary>
    /// Returns the registry's generated CSS wrapped with Pennington's prefix. The result is
    /// cached until the discovery registry's version token changes, so repeated GETs of
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

        var result = $"""
                {ContentVisibilityRules}

                {options.ExtraStyles}

                {classRegistry.Css}
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
    /// Used during DI registration to seed <see cref="MonorailDiscoveryOptions.Framework"/>;
    /// the discovery pipeline may then rebuild the framework when it processes source CSS.
    /// </summary>
    public static CssFramework BuildFramework(MonorailCssOptions options)
    {
        var theme = options.ColorScheme.ApplyToTheme(Theme.CreateWithDefaults());

        var settings = new CssFrameworkSettings
        {
            Theme = theme,
            Applies = PenningtonApplies.All(options.SyntaxTheme),
            CustomUtilities = PenningtonApplies.ScrollbarUtilities,
            ProseCustomization = options.ExtendProseCustomization(PenningtonProseRules.Default),
        };

        return new CssFramework(options.CustomCssFrameworkSettings(settings));
    }
}
