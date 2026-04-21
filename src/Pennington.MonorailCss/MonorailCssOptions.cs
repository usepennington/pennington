using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Theme;

namespace Pennington.MonorailCss;

/// <summary>
/// A color reference that provides IntelliSense discoverability for known Tailwind colors
/// while still accepting arbitrary custom color names via implicit string conversion.
/// </summary>
/// <param name="Value">Underlying color name (Tailwind palette key or custom identifier).</param>
public readonly record struct ColorName(string Value)
{
    /// <summary>Converts a plain string to a <see cref="ColorName"/>.</summary>
    public static implicit operator ColorName(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Red</summary>
    public static ColorName Red => new(ColorNames.Red);
    /// <summary>Orange</summary>
    public static ColorName Orange => new(ColorNames.Orange);
    /// <summary>Amber</summary>
    public static ColorName Amber => new(ColorNames.Amber);
    /// <summary>Yellow</summary>
    public static ColorName Yellow => new(ColorNames.Yellow);
    /// <summary>Lime</summary>
    public static ColorName Lime => new(ColorNames.Lime);
    /// <summary>Green</summary>
    public static ColorName Green => new(ColorNames.Green);
    /// <summary>Emerald</summary>
    public static ColorName Emerald => new(ColorNames.Emerald);
    /// <summary>Teal</summary>
    public static ColorName Teal => new(ColorNames.Teal);
    /// <summary>Cyan</summary>
    public static ColorName Cyan => new(ColorNames.Cyan);
    /// <summary>Sky</summary>
    public static ColorName Sky => new(ColorNames.Sky);
    /// <summary>Blue</summary>
    public static ColorName Blue => new(ColorNames.Blue);
    /// <summary>Indigo</summary>
    public static ColorName Indigo => new(ColorNames.Indigo);
    /// <summary>Violet</summary>
    public static ColorName Violet => new(ColorNames.Violet);
    /// <summary>Purple</summary>
    public static ColorName Purple => new(ColorNames.Purple);
    /// <summary>Fuchsia</summary>
    public static ColorName Fuchsia => new(ColorNames.Fuchsia);
    /// <summary>Pink</summary>
    public static ColorName Pink => new(ColorNames.Pink);
    /// <summary>Rose</summary>
    public static ColorName Rose => new(ColorNames.Rose);
    /// <summary>Slate</summary>
    public static ColorName Slate => new(ColorNames.Slate);
    /// <summary>Gray</summary>
    public static ColorName Gray => new(ColorNames.Gray);
    /// <summary>Zinc</summary>
    public static ColorName Zinc => new(ColorNames.Zinc);
    /// <summary>Neutral</summary>
    public static ColorName Neutral => new(ColorNames.Neutral);
    /// <summary>Stone</summary>
    public static ColorName Stone => new(ColorNames.Stone);
    /// <summary>Mauve</summary>
    public static ColorName Mauve => new(ColorNames.Mauve);
    /// <summary>Olive</summary>
    public static ColorName Olive => new(ColorNames.Olive);
    /// <summary>Mist</summary>
    public static ColorName Mist => new(ColorNames.Mist);
    /// <summary>Taupe</summary>
    public static ColorName Taupe => new(ColorNames.Taupe);
    /// <summary>Black</summary>
    public static ColorName Black => new(ColorNames.Black);
    /// <summary>White</summary>
    public static ColorName White => new(ColorNames.White);
}

/// <summary>
/// Options for configuring the Monorail CSS framework integration.
/// </summary>
public class MonorailCssOptions
{
    /// <summary>
    /// Gets or sets the color scheme for the site.
    /// The default is a NamedColorScheme with Blue (primary), Purple (accent), and Slate (base).
    /// </summary>
    public IColorScheme ColorScheme { get; init; } = new NamedColorScheme
    {
        PrimaryColorName = ColorName.Blue,
        AccentColorName = ColorName.Purple,
        BaseColorName = ColorName.Slate
    };

    /// <summary>
    /// Gets or sets the syntax-highlight color theme.
    /// Controls the Tailwind palettes used by <c>.hljs-*</c> token classes,
    /// independent of the site's brand <see cref="ColorScheme"/>.
    /// </summary>
    public SyntaxTheme SyntaxTheme { get; init; } = SyntaxTheme.Default;

    /// <summary>
    /// Gets or sets a function to customize the CSS framework settings.
    /// This allows for advanced customization of the MonorailCSS framework.
    /// </summary>
    public Func<CssFrameworkSettings, CssFrameworkSettings> CustomCssFrameworkSettings { get; init; } =
        settings => settings;

    /// <summary>
    /// Gets or sets any extra CSS styles to be included in the generated stylesheet.
    /// </summary>
    public string ExtraStyles { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets file paths (relative to the web root) to scan for CSS class usage at startup.
    /// Similar to Tailwind's <c>content</c> configuration — ensures classes used only in
    /// client-side JS or other non-HTML files are included in the generated stylesheet.
    /// </summary>
    public string[] ContentPaths { get; init; } = [];
}

/// <summary>
/// Defines how color schemes are applied to the MonorailCSS theme.
/// </summary>
public interface IColorScheme
{
    /// <summary>
    /// Applies the color scheme to the given theme.
    /// </summary>
    /// <param name="theme">The theme to apply colors to</param>
    Theme ApplyToTheme(Theme theme);
}

/// <summary>
/// A color scheme that generates palettes algorithmically from hue values.
/// </summary>
public class AlgorithmicColorScheme : IColorScheme
{
    /// <summary>
    /// Gets or sets the primary hue value (0-360).
    /// </summary>
    public required int PrimaryHue { get; init; }

    /// <summary>
    /// Gets or sets the base color name from the MonorailCSS color palette.
    /// The default value is "Gray".
    /// </summary>
    public ColorName BaseColorName { get; init; } = ColorName.Gray;

    /// <summary>
    /// Gets or sets the function that generates the accent hue from the primary hue.
    /// Defaults to the complementary hue (primary + 180°).
    /// </summary>
    public Func<int, int> ColorSchemeGenerator { get; init; } = primary => primary + 180;

    /// <summary>
    /// Gets or sets additional color mappings beyond the core slots.
    /// Key is the target slot name (e.g., "info", "warning"), value is the source color.
    /// </summary>
    public Dictionary<string, ColorName> AdditionalMappings { get; init; } = [];

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme)
    {
        var primary = ColorPaletteGenerator.GenerateFromHue(PrimaryHue);
        var accentHue = ColorSchemeGenerator(PrimaryHue);
        var accent = ColorPaletteGenerator.GenerateFromHue(accentHue);

        theme = theme.AddColorPalette("primary", primary)
             .AddColorPalette("accent", accent)
             .MapColorPalette(BaseColorName.Value, "base");

        foreach (var (slot, color) in AdditionalMappings)
            theme = theme.MapColorPalette(color.Value, slot);

        return theme;
    }
}

/// <summary>
/// A color scheme that uses named Tailwind colors.
/// </summary>
public class NamedColorScheme : IColorScheme
{
    /// <summary>
    /// Gets or sets the color name to map to "primary".
    /// </summary>
    public required ColorName PrimaryColorName { get; init; }

    /// <summary>
    /// Gets or sets the color name to map to "accent".
    /// </summary>
    public required ColorName AccentColorName { get; init; }

    /// <summary>
    /// Gets or sets the color name to map to "base".
    /// </summary>
    public required ColorName BaseColorName { get; init; }

    /// <summary>
    /// Gets or sets additional color mappings beyond the core slots.
    /// Key is the target slot name (e.g., "info", "warning"), value is the source color.
    /// </summary>
    public Dictionary<string, ColorName> AdditionalMappings { get; init; } = [];

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme)
    {
        theme = theme.MapColorPalette(PrimaryColorName.Value, "primary")
             .MapColorPalette(AccentColorName.Value, "accent")
             .MapColorPalette(BaseColorName.Value, "base");

        foreach (var (slot, color) in AdditionalMappings)
            theme = theme.MapColorPalette(color.Value, slot);

        return theme;
    }
}

/// <summary>
/// Generates CSS stylesheets using MonorailCSS with collected utility classes.
/// </summary>
/// <param name="options">MonorailCSS configuration options.</param>
/// <param name="cssClassCollector">Collector containing discovered CSS class names.</param>
public class MonorailCssService(MonorailCssOptions options, CssClassCollector cssClassCollector)
{
    /// <summary>
    /// Processes collected CSS classes and returns the generated stylesheet.
    /// </summary>
    public string GetStyleSheet()
    {
        // we are only scanning razor files, not the generated files. if you use
        // code like bg-{color}-400 in the razor as a variable, that's not going to be detected.
        var cssClassValues = cssClassCollector.GetClasses();
        var cssFramework = GetCssFramework();

        var styleSheet = cssFramework.Process(cssClassValues);

        return $"""
                {ContentVisibilityRules}

                {options.ExtraStyles}

                {styleSheet}
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

    private CssFramework GetCssFramework()
    {
        var theme = Theme.CreateWithDefaults();
        theme = options.ColorScheme.ApplyToTheme(theme);

        var cssFrameworkSettings = new CssFrameworkSettings()
        {
            Theme = theme,

            Applies = ImmutableDictionary<string, string>.Empty
                .AddRange(CodeBlockApplies())
                .AddRange(TabApplies())
                .AddRange(MarkdownAlertApplies())
                .AddRange(HljsApplies(options.SyntaxTheme))
                .AddRange(SearchModalApplies()),

            ProseCustomization = GetCustomProseSettings()
        };

        var optionsCustomCssFrameworkSettings = options.CustomCssFrameworkSettings(cssFrameworkSettings);
        return new CssFramework(optionsCustomCssFrameworkSettings);
    }

    private static ProseCustomization GetCustomProseSettings()
    {
        var proseCustomization = new ProseCustomization
        {
            Customization = theme =>
            {
                // Helper to get color values from theme
                string GetColorValue(string color, string shade) =>
                    theme.ResolveValue(shade, [$"--color-{color}"]) ?? "#000000";

                // Helper to add opacity to color (simplified - you might need a more robust implementation)
                string WithOpacity(string color, string opacity) =>
                    $"color-mix(in srgb, {color} {opacity}, transparent)";

                return new Dictionary<string, ProseElementRules>
                {
                    ["DEFAULT"] = new()
                    {
                        Rules = new List<ProseElementRule>
                        {
                            new() { Selector = "a", Declarations = new List<ProseDeclaration> {
                                    new() { Property = "font-weight", Value = "700" },
                                    new() { Property = "text-decoration", Value = "none" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = "a:not(:has(> code))",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "border-bottom-width", Value = "1px" },
                                    new() { Property = "border-bottom-color", Value = WithOpacity(GetColorValue("primary", "500"), "75%")
                                    }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = "blockquote",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "border-left-width", Value = "4px" },
                                    new() { Property = "padding-left", Value = "1rem" },
                                    new() { Property = "border-color", Value = GetColorValue("primary", "700") }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = "pre",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "background-color", Value = WithOpacity(GetColorValue("base", "200"), "50%") },
                                    new() { Property = "box-shadow", Value = "inset 0 0 0 1px oklch(87.1% .006 286.286)" },
                                    new() { Property = "border-radius", Value = "0.4rem" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = "code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-weight", Value = "400" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = ":not(pre) > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "padding", Value = "3px 8px" },
                                    new() { Property = "box-shadow", Value = "inset 0 0 0 1px oklch(87.1% .006 286.286)" },
                                    new() { Property = "border-radius", Value = "0.4rem" },
                                    new() { Property = "background-color", Value = WithOpacity(GetColorValue("base", "200"), "50%") },
                                    new() { Property = "color", Value = GetColorValue("base", "700") },
                                    new() { Property = "word-break", Value = "break-word" }
                                }.ToImmutableList()
                            }
                        }.ToImmutableList()
                    },
                    ["base"] = new()
                    {
                        Rules = new List<ProseElementRule>
                        {
                            new()
                            {
                                Selector = "pre > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-size", Value = "inherit" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = ":not(pre) > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-size", Value = "0.8em" }
                                }.ToImmutableList()
                            }
                        }.ToImmutableList()
                    },
                    ["sm"] = new()
                    {
                        Rules = new List<ProseElementRule>
                        {
                            new()
                            {
                                Selector = "pre > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-size", Value = "inherit" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = ":not(pre) > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-size", Value = "0.8em" }
                                }.ToImmutableList()
                            }
                        }.ToImmutableList()
                    },
                    ["invert"] = new()
                    {
                        Rules = new List<ProseElementRule>
                        {
                            new()
                            {
                                Selector = "pre",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "font-weight", Value = "300" },
                                    new() { Property = "background-color", Value = WithOpacity(GetColorValue("base", "800"), "75%") },
                                    new() { Property = "box-shadow", Value = "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)" }
                                }.ToImmutableList()
                            },
                            new()
                            {
                                Selector = ":not(pre) > code",
                                Declarations = new List<ProseDeclaration>
                                {
                                    new() { Property = "background-color", Value = WithOpacity(GetColorValue("base", "800"), "75%") },
                                    new() { Property = "color", Value = GetColorValue("base", "200") },
                                    new() { Property = "box-shadow", Value = "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)" }
                                }.ToImmutableList()
                            }
                        }.ToImmutableList()
                    }
                }.ToImmutableDictionary();
            }
        };
        return proseCustomization;
    }

    private static ImmutableDictionary<string, string> CodeBlockApplies()
    {
        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                {
                    ".code-highlight-wrapper .standalone-code-container",
                    "bg-white/50 border border-base-300/75 shadow-xs rounded rounded-xl overflow-x-auto dark:bg-black/20 dark:border-base-700/50"
                },
                {
                    ".code-highlight-wrapper pre ",
                    "py-2 md:py-3 px-4 md:px-8  overflow-x-auto  font-mono text-xs md:text-sm  leading-relaxed w-full dark:scheme-dark"
                },
                {
                    ".code-highlight-wrapper .standalone-code-highlight pre",
                    "text-base-900/90 dark:text-base-100/90"
                },
                {
                    ".code-highlight-wrapper pre code",
                    "font-mono inline-block min-w-full"
                },

                // Code transformation line containers
                {
                    ".code-highlight-wrapper .line",
                    "inline-block transition-all duration-300 lg:py-[1px] px-8 -mx-8 w-[calc(100%+4rem)] relative"
                },
                {
                    ".code-highlight-wrapper pre.has-focused .line",
                    "blur-[0.05rem] opacity-75"
                },

                {
                    ".code-highlight-wrapper pre.has-focused:hover .line",
                    "blur-[0] opacity-100"
                },

                // Line highlighting
                {
                    ".code-highlight-wrapper .line.highlight",
                    "bg-primary-700/20 dark:bg-primary-500/20"
                },

                // Diff notation
                {
                    ".code-highlight-wrapper .line.diff-add",
                    "bg-emerald-600/20 dark:bg-emerald-900/20 before:font-bold before:content-['+'] before:hidden md:before:block before:text-sm before:absolute before:left-1 before:green:text-green-500 before:text-green-700"
                },
                {
                    ".code-highlight-wrapper .line.diff-remove",
                    "bg-red-600/20 dark:bg-red-900/20 before:font-bold  before:content-['-'] before:hidden md:before:block before:text-sm before:absolute before:left-1 before:dark:text-red-500 before:text-red-700"
                },

                {
                    ".code-highlight-wrapper .line.diff-remove > *",
                    "opacity-50 contrast-50"
                },

                // Focus and blur
                {
                    ".code-highlight-wrapper pre.has-focused  .line.focused",
                    "blur-[0] opacity-100"
                },

                // Error and warning states
                {
                    ".code-highlight-wrapper .line.error",
                    "bg-red-300/50 dark:bg-red-500/20"
                },
                {
                    ".code-highlight-wrapper .line.warning",
                    "bg-amber-300/50 dark:bg-amber-400/20"
                },

                // Word highlighting
                {
                    ".code-highlight-wrapper .word-highlight",
                    "border border-primary-600 dark:border-primary-300/25 rounded px-0.5 py-0 bg-primary-100/25 dark:bg-primary-500/10"
                },
                {
                    ".code-highlight-wrapper .word-highlight-with-message",
                    "border border-b border-primary-600 dark:border-primary-300/25 rounded px-1 py-1 bg-primary-100/25 dark:bg-primary-500/10 relative "
                },
                {
                    ".code-highlight-wrapper .word-highlight-wrapper",
                    "relative inline-block"
                },
                {
                    ".code-highlight-wrapper .word-highlight-message",
                    "font-sans font-semilight tracking-loose absolute top-full left-0 mt-3 px-2 py-1 text-xs text-base-800 bg-base-200/25 dark:bg-primary-700/20 dark:text-primary-200 rounded border border-primary-500/50 whitespace-nowrap z-10 select-none pointer-events-none"
                },

                {
                    ".code-highlight-wrapper .word-highlight-message::selection",
                    "bg-transparent"
                },
                {
                    ".code-highlight-wrapper .line:has(.word-highlight-wrapper)",
                    "mb-12"
                },
            });
    }

    private static ImmutableDictionary<string, string> TabApplies()
    {
        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                {
                    ".tab-container",
                    "flex flex-col bg-base-100 border border-base-300/75 shadow-xs rounded rounded-xl overflow-x-auto dark:bg-base-950/25 dark:border-base-700/50"
                },
                {
                    ".tab-list",
                    "flex flex-row flex-wrap px-4 pt-1 bg-base-200/90 gap-x-2 lg:gap-x-4 dark:bg-base-800/50"
                },
                {
                    ".tab-button",
                    "whitespace-nowrap border-b border-transparent py-2 text-xs text-base-900/90 font-medium transition-colors hover:text-accent-500 disabled:pointer-events-none disabled:opacity-50 data-[selected=true]:text-accent-700 data-[selected=true]:border-accent-700 dark:text-base-100/90 dark:hover:text-accent-300 dark:data-[selected=true]:text-accent-400 dark:data-[selected=true]:border-accent-400"
                },
                {
                    ".tab-panel",
                    "hidden data-[selected=true]:block py-3 "
                },
            });
    }

    private static ImmutableDictionary<string, string> MarkdownAlertApplies()
    {
        const string alertFormatString =
            "fill-{0}-700 dark:fill-{0}-500 bg-{0}-100/75 border-{0}-500/20 dark:border-{0}-500/30 dark:bg-{0}-900/25 text-{0}-800 dark:text-{0}-200";

        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                // Markdig Alert Styles
                { ".markdown-alert", "my-6 px-4 flex flex-row gap-2.5 rounded-2xl border text-sm items-center" },
                { ".markdown-alert a", "underline" },
                { ".markdown-alert-note", string.Format(alertFormatString, "emerald") },
                { ".markdown-alert-tip", string.Format(alertFormatString, "blue") },
                { ".markdown-alert-caution", string.Format(alertFormatString, "amber") },
                { ".markdown-alert-warning", string.Format(alertFormatString, "rose") },
                { ".markdown-alert-important", string.Format(alertFormatString, "sky") },
                { ".markdown-alert-title", "text-[0px]" },
                { ".markdown-alert svg", "h-4 w-4 mt-0.5" },
            });
    }

    private static ImmutableDictionary<string, string> HljsApplies(SyntaxTheme syntax)
    {
        string Token(ColorName c) => $"text-{c.Value}-800 dark:text-{c.Value}-300";
        string Soft(ColorName c) => $"text-{c.Value}-700 dark:text-{c.Value}-300";

        var keyword = Token(syntax.Keyword);
        var @string = Token(syntax.String);
        var variable = Token(syntax.Variable);
        var function = Token(syntax.Function);

        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                // Base highlight.js styles
                { ".hljs", "text-base-900 dark:text-base-200" },

                // Comments
                { ".hljs-comment", $"text-{syntax.Comment.Value}-600 italic dark:text-{syntax.Comment.Value}-400" },
                { ".hljs-quote", $"text-{syntax.Comment.Value}-800/50 italic dark:text-{syntax.Comment.Value}-300" },

                // Keywords and control flow
                { ".hljs-keyword", keyword },
                { ".hljs-selector-tag", Soft(syntax.Keyword) },
                { ".hljs-literal", keyword },
                { ".hljs-type", "text-base-700 dark:text-base-300" },

                // Strings and characters
                { ".hljs-string", @string },
                { ".hljs-number", @string },
                { ".hljs-regexp", @string },

                // Functions and methods
                { ".hljs-function", function },
                { ".hljs-title", function },
                { ".hljs-params", function },

                // Variables and identifiers
                { ".hljs-variable", variable },
                { ".hljs-name", variable },
                { ".hljs-attr", variable },
                { ".hljs-symbol", variable },

                // Operators and punctuation
                { ".hljs-operator", "text-base-800 dark:text-base-300" },
                { ".hljs-punctuation", "text-base-800 dark:text-base-300" },

                // Special elements
                { ".hljs-built_in", Soft(syntax.Function) },
                { ".hljs-class", keyword },
                { ".hljs-meta", "text-base-800 dark:text-base-300" },
                { ".hljs-tag", keyword },
                { ".hljs-attribute", variable },
                { ".hljs-addition", "text-green-800 dark:text-green-300" },
                { ".hljs-deletion", "text-red-800 dark:text-red-300" },
                { ".hljs-link", "text-blue-800 dark:text-blue-300" },
            });
    }

    private static ImmutableDictionary<string, string> SearchModalApplies()
    {
        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                // Modal backdrop and container
                { ".search-modal-backdrop", "fixed inset-0 bg-base-950/50 backdrop-blur z-50 p-4 md:p-16" },
                {
                    ".search-modal-content",
                    " top-16 mx-auto w-full mt-8 max-w-2xl bg-base-100 dark:bg-base-900 rounded-lg shadow-xl border border-base-200 dark:border-base-700"
                },

                // Modal header and input
                { ".search-modal-header", "p-4 border-b border-base-200 dark:border-base-700" },
                { ".search-modal-input-container", "relative" },
                {
                    ".search-modal-input",
                    "w-full px-4 py-2 pl-10 bg-base-50 dark:bg-base-800 border border-base-300 dark:border-base-600 rounded-md text-base-900 dark:text-base-100 placeholder-base-500 dark:placeholder-base-400 focus:outline-none focus:ring-1 focus:ring-primary-500/50 focus:border-primary-500"
                },
                { ".search-modal-icon", "absolute left-3 top-2.5 h-4 w-4 text-base-400 dark:text-base-500" },

                // Results container
                { ".search-modal-results", "max-h-96 overflow-y-auto px-4 dark:scheme-dark" },

                // Status messages
                { ".search-modal-placeholder", "text-center text-base-600 dark:text-base-400 py-4" },
                { ".search-modal-loading", "text-center text-base-600 dark:text-base-400 py-4" },
                { ".search-modal-no-results", "text-center text-base-600 dark:text-base-400 py-4" },
                { ".search-modal-error", "text-center text-red-600 dark:text-red-400 py-4" },

                // Search result items
                { ".search-result-item", "border-b border-base-200 dark:border-base-800 py-4 last:border-b-0" },
                {
                    ".search-result-link",
                    "block hover:bg-base-50 dark:hover:bg-base-800 rounded-md p-2 -m-2 transition-colors"
                },
                { ".search-result-header", "flex items-start justify-between mb-1" },
                { ".search-result-title", "text-sm font-medium text-primary-700 dark:text-primary-400 flex-1" },
                { ".search-result-score", "text-xs text-base-500 dark:text-base-500 ml-2" },
                { ".search-result-description", "text-sm text-base-600 dark:text-base-400 mb-2" },
                { ".search-result-snippet", "text-xs text-base-700 dark:text-base-500" },
                { ".search-result-url", "text-xs text-base-500 dark:text-base-500 mt-2" },

                // Search highlighting
                { ".search-result-title .search-highlight", "text-primary-500 dark:text-primary-100 bg-inherit" },
                { ".search-highlight", "text-base-500 dark:text-base-50 bg-inherit" },
            });
    }
}