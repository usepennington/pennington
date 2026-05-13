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
    /// <summary>
    /// Builds the stylesheet from scratch on every call: a fresh <see cref="CssFramework"/>
    /// processes the current class set from <see cref="IClassRegistry.GetClasses"/>.
    /// This deliberately skips <see cref="IClassRegistry.Css"/> (which caches against the
    /// framework that was baked into <c>MonorailDiscoveryOptions.Framework</c> at startup).
    /// Combined with transient lifetimes on <see cref="MonorailCssOptions"/> and this service
    /// (see <c>MonorailServiceExtensions.AddMonorailCss</c>), dotnet-watch hot-reload edits to
    /// <see cref="MonorailCssOptions.ColorScheme"/>, prose customizations, and
    /// <see cref="MonorailCssOptions.CustomCssFrameworkSettings"/> flow into the served
    /// stylesheet on the next request without a process restart.
    /// Pennington is a static content engine — the build is one-shot and the dev server
    /// is the only other consumer, so per-call rebuild is the right tradeoff.
    /// </summary>
    public string GetStyleSheet()
    {
        var framework = BuildFramework(options);
        var css = framework.Process(classRegistry.GetClasses());

        return $"""
                {ContentVisibilityRules}

                {options.ExtraStyles}

                {css}
                """;
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
