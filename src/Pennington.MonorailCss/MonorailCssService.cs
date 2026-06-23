using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
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
/// <param name="logger">Emits per-request build timing at <see cref="LogLevel.Trace"/>.</param>
public sealed class MonorailCssService(
    MonorailCssOptions options,
    IClassRegistry classRegistry,
    ILogger<MonorailCssService> logger)
{
    // The framework — utility registry, parser, 11-stage pipeline, and a per-token compile cache —
    // is expensive to construct and depends only on the resolved CssFrameworkSettings, never on the
    // class set. So it is built once and reused across requests, keyed by a structural fingerprint of
    // those settings. Recomputing the fingerprint each request from the freshly-resolved (transient)
    // options means any change to the rendered theme/applies/prose — a dotnet-watch hot reload, a
    // restart, even a config-driven options factory — yields a new fingerprint and a rebuild, while
    // unrelated content/.razor/.cs edits reuse the cached framework AND its warm per-token compile
    // cache. volatile for safe publication; the build race on a miss is benign (two equal frameworks
    // built under concurrency, last publish wins), and Process + its LruCache are documented
    // thread-safe for the shared read path.
    private sealed record FrameworkCache(string Fingerprint, CssFramework Framework);

    private static volatile FrameworkCache? _cache;

    /// <summary>
    /// Builds the stylesheet by running the current class set from
    /// <see cref="IClassRegistry.GetClasses"/> through a cached <see cref="CssFramework"/>. This
    /// deliberately skips <see cref="IClassRegistry.Css"/> (which caches against the framework that
    /// was baked into <c>MonorailDiscoveryOptions.Framework</c> at startup).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The framework is cached process-wide and reused across requests rather than rebuilt each call:
    /// constructing it (utility registry + pipeline) is the dominant per-hit cost, and it carries a
    /// warm per-token compile cache a fresh instance would discard. The cache key is a structural
    /// fingerprint of the resolved <see cref="CssFrameworkSettings"/>
    /// (<see cref="ComputeSettingsFingerprint"/>), recomputed every call from the freshly-resolved
    /// (transient) <see cref="MonorailCssOptions"/>. So edits to
    /// <see cref="MonorailCssOptions.ColorScheme"/>, fonts, <see cref="MonorailCssOptions.SyntaxTheme"/>,
    /// prose, and other framework settings flow into the next <c>/styles.css</c> — under
    /// <c>dotnet watch</c> or otherwise — while edits that leave those settings unchanged (content,
    /// markup, unrelated code) reuse the cached framework. The class set is pulled fresh on every
    /// call, so content edits flow through independently of the cache.
    /// </para>
    /// <para>
    /// At <see cref="LogLevel.Trace"/> the build is timed and logged — framework (built or cached),
    /// the class-set snapshot pull (with count), and CSS generation — so <c>dotnet watch</c> shows
    /// where each <c>/styles.css</c> hit spends its time. Off by default; under the docs site's
    /// <c>"Pennington": "Trace"</c> filter it surfaces automatically.
    /// </para>
    /// </remarks>
    public string GetStyleSheet()
    {
        var traceEnabled = logger.IsEnabled(LogLevel.Trace);
        var startedAt = traceEnabled ? Stopwatch.GetTimestamp() : 0L;

        var settings = BuildSettings(options);
        var fingerprint = ComputeSettingsFingerprint(settings);

        var cached = _cache;
        bool rebuilt;
        CssFramework framework;
        if (cached is not null && cached.Fingerprint == fingerprint)
        {
            framework = cached.Framework;
            rebuilt = false;
        }
        else
        {
            framework = new CssFramework(settings);
            _cache = new FrameworkCache(fingerprint, framework);
            rebuilt = true;
        }

        var frameworkReadyAt = traceEnabled ? Stopwatch.GetTimestamp() : 0L;

        var classes = classRegistry.GetClasses();
        var classesPulledAt = traceEnabled ? Stopwatch.GetTimestamp() : 0L;

        var css = framework.Process(classes);

        if (traceEnabled)
        {
            var generatedAt = Stopwatch.GetTimestamp();
            logger.LogTrace(
                "styles.css built in {TotalMs:0.0}ms (framework {FrameworkState} {FrameworkMs:0.0}ms, class discovery {DiscoveryMs:0.0}ms / {ClassCount} classes, CSS generation {GenerationMs:0.0}ms)",
                Stopwatch.GetElapsedTime(startedAt, generatedAt).TotalMilliseconds,
                rebuilt ? "rebuilt" : "cached",
                Stopwatch.GetElapsedTime(startedAt, frameworkReadyAt).TotalMilliseconds,
                Stopwatch.GetElapsedTime(frameworkReadyAt, classesPulledAt).TotalMilliseconds,
                classes.Count,
                Stopwatch.GetElapsedTime(classesPulledAt, generatedAt).TotalMilliseconds);
        }

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
    /// Builds a Tailwind-aware class merge delegate from Pennington's options:
    /// <c>(baseClasses, overrideClasses) =&gt; merged</c>, dropping utilities in
    /// <paramref name="options"/>-rendered <c>baseClasses</c> that conflict with later
    /// <c>overrideClasses</c>. Backed by <see cref="CssFramework.Merge(string)"/> over the same framework
    /// the site renders with, so the semantic palette and custom utilities define the conflicts.
    /// Consumed by the Pennington.UI style registry's consumer-override layer
    /// (<c>AddPenningtonStyles</c>).
    /// </summary>
    /// <param name="options">MonorailCSS options whose framework defines the merge conflicts.</param>
    public static Func<string, string, string> CreateClassMerger(MonorailCssOptions options)
    {
        // ClassMerger is documented thread-safe with its own LRU cache, so one instance is
        // shared across every overridden slot and every resolve.
        var framework = BuildFramework(options);
        return (baseClasses, overrideClasses) => framework.Merge(baseClasses, overrideClasses);
    }

    /// <summary>
    /// Builds a fully-configured <see cref="CssFramework"/> from Pennington's options.
    /// Used during DI registration to seed <see cref="MonorailDiscoveryOptions.Framework"/>;
    /// the discovery pipeline may then rebuild the framework when it processes source CSS.
    /// </summary>
    /// <param name="options">MonorailCSS options to build the framework from.</param>
    internal static CssFramework BuildFramework(MonorailCssOptions options) =>
        new(BuildSettings(options));

    /// <summary>
    /// Resolves Pennington's options into the <see cref="CssFrameworkSettings"/> a
    /// <see cref="CssFramework"/> is constructed from: the color scheme applied to the default theme,
    /// the syntax-theme applies, Pennington's scrollbar/card utilities, and the prose customization,
    /// then the consumer's <see cref="MonorailCssOptions.CustomCssFrameworkSettings"/> hook over the
    /// top. Cheap relative to constructing the framework; <see cref="GetStyleSheet"/> fingerprints the
    /// result to decide whether the cached framework can be reused.
    /// </summary>
    /// <param name="options">MonorailCSS options to resolve.</param>
    internal static CssFrameworkSettings BuildSettings(MonorailCssOptions options)
    {
        var theme = options.ColorScheme.ApplyToTheme(Theme.CreateWithDefaults());

        var settings = new CssFrameworkSettings
        {
            Theme = theme,
            Applies = PenningtonApplies.All(options.SyntaxTheme),
            CustomUtilities = PenningtonApplies.ScrollbarUtilities.AddRange(PenningtonApplies.CardUtilities),
            ProseCustomization = options.ExtendProseCustomization(PenningtonProseRules.Default),
        };

        return options.CustomCssFrameworkSettings(settings);
    }

    /// <summary>
    /// Computes a deterministic, process-local fingerprint of everything in <paramref name="settings"/>
    /// that affects generated CSS — the cache key for the constructed <see cref="CssFramework"/>.
    /// Dictionaries are sorted (their enumeration order isn't guaranteed); the prose customization is a
    /// delegate, so its <em>resolved</em> rules are hashed via <see cref="ProseCustomization.GetRules"/>
    /// rather than the opaque function. Custom utilities/variants are compared by count — Pennington
    /// supplies a fixed set, so only a consumer adding or removing them via
    /// <see cref="MonorailCssOptions.CustomCssFrameworkSettings"/> moves the count. The result is only
    /// ever compared against another fingerprint from the same process (the cache is in-memory), so
    /// the canonical string is used directly.
    /// </summary>
    /// <param name="settings">The resolved framework settings to fingerprint.</param>
    internal static string ComputeSettingsFingerprint(CssFrameworkSettings settings)
    {
        var sb = new StringBuilder();

        AppendSorted(sb, "theme", settings.Theme.Values);
        sb.Append("prefix=").Append(settings.Theme.Prefix).Append('\n');
        AppendSorted(sb, "applies", settings.Applies);
        AppendSorted(sb, "keyframes", settings.Keyframes);
        sb.Append("variants=")
            .AppendJoin(',', settings.Variants.OrderBy(v => v, StringComparer.Ordinal))
            .Append('\n');
        sb.Append("flags=").Append(settings.Important).Append(',')
            .Append(settings.IncludePreflight).Append(',').Append(settings.ColorEmission).Append('\n');
        sb.Append("custom=").Append(settings.CustomUtilities.Count).Append(',')
            .Append(settings.CustomVariants.Count).Append('\n');

        if (settings.ProseCustomization is { } prose)
        {
            foreach (var (modifier, rules) in
                     prose.GetRules(settings.Theme).OrderBy(e => e.Key, StringComparer.Ordinal))
            {
                sb.Append("prose[").Append(modifier).Append("]=");
                foreach (var rule in rules.Rules)
                {
                    sb.Append(rule.Selector).Append('{')
                        .Append(rule.UseWhereWrapper).Append(';').Append(rule.ExcludeClass).Append(';');
                    foreach (var declaration in rule.Declarations)
                    {
                        sb.Append(declaration.Property).Append(':').Append(declaration.Value);
                        if (declaration.Important)
                        {
                            sb.Append('!');
                        }

                        sb.Append(';');
                    }

                    sb.Append('}');
                }

                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    private static void AppendSorted(StringBuilder sb, string label, ImmutableDictionary<string, string> map)
    {
        sb.Append(label).Append('=');
        foreach (var (key, value) in map.OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            sb.Append(key).Append(':').Append(value).Append(';');
        }

        sb.Append('\n');
    }
}