using System.Collections.Immutable;
using MonorailCss.Theme;

namespace Pennington.Docs;

/// <summary>
/// Pennington-brand styling layered on top of the neutral MonorailCSS defaults shipped
/// by <c>Pennington.MonorailCss</c>: pseudo-element @apply blocks (H2 left gradient bar,
/// custom list bullet dot) plus prose flair (animated link underline, primary-tinted
/// blockquote, pre inset shadow, non-pre inline-code chip, H2 padding for the gradient bar).
/// Wired in <c>Program.cs</c> via <c>DocSiteOptions.CustomCssFrameworkSettings</c> and
/// <c>DocSiteOptions.ExtendProseCustomization</c>.
/// </summary>
internal static class BrandStyling
{
    /// <summary>Brand @apply blocks for pseudo-element rules MonorailCSS's prose customization can't emit.</summary>
    public static readonly ImmutableDictionary<string, string> Applies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // H2 left gradient bar — main vertical landmark between sections.
            {
                ".prose :where(h2):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))",
                "before:content-[''] before:absolute before:left-0 before:top-0 before:bottom-0 before:w-[4px] before:rounded-sm before:bg-gradient-to-b before:from-primary-500 before:to-primary-700 dark:before:from-primary-300 dark:before:to-primary-500"
            },
            // List bullet dot — replaces the native bullet that the neutral baseline would emit.
            {
                ".prose :where(ul > li):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))",
                "before:content-[''] before:absolute before:left-[0.35rem] before:top-[0.7em] before:w-[5px] before:h-[5px] before:rounded-full before:bg-primary-500/55 dark:before:bg-primary-300/55"
            },
        });

    /// <summary>Overlays brand prose rules onto a neutral baseline. Brand wins where selectors overlap.</summary>
    public static ProseCustomization Extend(ProseCustomization baseline) => new()
    {
        Customization = theme =>
        {
            string Color(string color, string shade) =>
                theme.ResolveValue(shade, [$"--color-{color}"]) ?? "#000000";

            string Mix(string color, string opacity) =>
                $"color-mix(in srgb, {color} {opacity}, transparent)";

            var baseRules = baseline.Customization(theme);

            var brand = new Dictionary<string, ProseElementRules>
            {
                ["DEFAULT"] = Rules(
                    // Animated underline — color animates independently of the text color
                    // because text-decoration-color can transition where border-bottom can't.
                    Rule("a",
                        ("color", Color("primary", "700")),
                        ("font-weight", "500"),
                        ("text-decoration-line", "underline"),
                        ("text-decoration-thickness", "1px"),
                        ("text-underline-offset", "3px"),
                        ("text-decoration-color", Mix(Color("primary", "700"), "35%")),
                        ("transition", "color .15s, text-decoration-color .15s")),
                    Rule("a:hover",
                        ("text-decoration-color", Mix(Color("primary", "700"), "90%"))),
                    // Hide native bullets so the bullet-dot pseudo carries the marker.
                    Rule("ul",
                        ("list-style", "none"),
                        ("padding-left", "0")),
                    Rule("ul > li",
                        ("position", "relative"),
                        ("padding-left", "1.4rem")),
                    // Room for the H2 gradient bar pseudo on the left.
                    Rule("h2",
                        ("position", "relative"),
                        ("padding-left", "0.85rem"),
                        ("scroll-margin-top", "5rem")),
                    Rule("blockquote",
                        ("border-left-width", "4px"),
                        ("padding-left", "1rem"),
                        ("border-color", Color("primary", "700"))),
                    Rule("pre",
                        ("background-color", Mix(Color("base", "200"), "50%")),
                        ("box-shadow", "inset 0 0 0 1px oklch(87.1% .006 286.286)"),
                        ("border-radius", "0.4rem")),
                    Rule(":not(pre) > code",
                        ("font-family", "var(--font-mono)"),
                        ("padding", "0.12em 0.42em"),
                        ("border-radius", "5px"),
                        ("background-color", Color("base", "100")),
                        ("border", $"1px solid {Color("base", "200")}"),
                        ("color", Color("base", "700")),
                        ("white-space", "nowrap"))),
                ["sm"] = Rules(
                    Rule("ul",
                        ("list-style", "none"),
                        ("padding-left", "0")),
                    Rule("ul > li",
                        ("position", "relative"),
                        ("padding-left", "1.4rem"))),
                ["invert"] = Rules(
                    Rule("a",
                        ("color", Color("primary", "300")),
                        ("text-decoration-color", Mix(Color("primary", "300"), "35%"))),
                    Rule("a:hover",
                        ("text-decoration-color", Mix(Color("primary", "300"), "90%"))),
                    Rule("pre",
                        ("font-weight", "300"),
                        ("background-color", Mix(Color("base", "800"), "75%")),
                        ("box-shadow", "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)")),
                    Rule(":not(pre) > code",
                        ("background-color", Color("base", "800")),
                        ("border-color", Color("base", "700")),
                        ("color", Color("base", "200")))),
            };

            return Merge(baseRules, brand);
        },
    };

    // Per-section overlay: brand selectors replace baseline selectors; new sections from brand are added.
    private static ImmutableDictionary<string, ProseElementRules> Merge(
        ImmutableDictionary<string, ProseElementRules> baseline,
        Dictionary<string, ProseElementRules> brand)
    {
        var result = baseline;
        foreach (var (section, brandSection) in brand)
        {
            if (baseline.TryGetValue(section, out var baseSection))
            {
                var brandSelectors = brandSection.Rules.Select(r => r.Selector).ToHashSet();
                var keptBase = baseSection.Rules.Where(r => !brandSelectors.Contains(r.Selector));
                result = result.SetItem(section, new ProseElementRules
                {
                    Rules = [.. keptBase, .. brandSection.Rules],
                });
            }
            else
            {
                result = result.SetItem(section, brandSection);
            }
        }
        return result;
    }

    private static ProseElementRule Rule(string selector, params (string Property, string Value)[] declarations) =>
        new()
        {
            Selector = selector,
            Declarations =
                [.. declarations.Select(d => new ProseDeclaration { Property = d.Property, Value = d.Value })],
        };

    private static ProseElementRules Rules(params ProseElementRule[] rules) => new() { Rules = [.. rules], };
}
