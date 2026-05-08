using System.Collections.Immutable;
using MonorailCss.Theme;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Pennington's default <see cref="ProseCustomization"/> — link, list, heading, divider, and
/// code styling shared by light and inverted prose. The H2 accent bar, bullet dots, link
/// underline, and inline-code chip are part of the doc-site article aesthetic and apply
/// wherever the <c>prose</c> utility is in scope.
/// </summary>
internal static class PenningtonProseRules
{
    public static ProseCustomization Default { get; } = new()
    {
        Customization = theme =>
        {
            string Color(string color, string shade) =>
                theme.ResolveValue(shade, [$"--color-{color}"]) ?? "#000000";

            string Mix(string color, string opacity) =>
                $"color-mix(in srgb, {color} {opacity}, transparent)";

            return new Dictionary<string, ProseElementRules>
            {
                ["DEFAULT"] = Rules(
                    Rule("p",
                        ("text-wrap", "pretty")),

                    // Links — text-decoration-based underline lets the underline color animate
                    // independently of the text color, which the previous border-bottom couldn't.
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

                    Rule("strong",
                        ("color", Color("base", "900")),
                        ("font-weight", "600")),

                    Rule("ul",
                        ("list-style", "none"),
                        ("padding-left", "0")),
                    Rule("ul > li",
                        ("position", "relative"),
                        ("padding-left", "1.4rem")),

                    Rule("hr",
                        ("border", "0"),
                        ("border-top", $"1px solid {Color("base", "200")}"),
                        ("margin", "3rem 0")),

                    // H2 carries a 3px primary gradient bar on the left for visual rhythm
                    // between sections — the doc article's main vertical landmark. The bar
                    // itself is a ::before pseudo-element registered separately in
                    // PenningtonApplies because MonorailCSS's prose customization doesn't
                    // emit pseudo-element rules.
                    Rule("h2",
                        ("position", "relative"),
                        ("padding-left", "0.85rem"),
                        ("scroll-margin-top", "5rem")),

                    Rule("h3",
                        ("scroll-margin-top", "5rem")),

                    Rule("blockquote",
                        ("border-left-width", "4px"),
                        ("padding-left", "1rem"),
                        ("border-color", Color("primary", "700"))),

                    Rule("pre",
                        ("background-color", Mix(Color("base", "200"), "50%")),
                        ("box-shadow", "inset 0 0 0 1px oklch(87.1% .006 286.286)"),
                        ("border-radius", "0.4rem")),
                    Rule("code",
                        ("font-weight", "400")),
                    Rule(":not(pre) > code",
                        ("font-family", "var(--font-mono)"),
                        ("padding", "0.12em 0.42em"),
                        ("border-radius", "5px"),
                        ("background-color", Color("base", "100")),
                        ("border", $"1px solid {Color("base", "200")}"),
                        ("color", Color("base", "700")),
                        ("white-space", "nowrap"))),

                ["base"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.86em"))),

                ["sm"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.86em"))),

                ["invert"] = Rules(
                    Rule("a",
                        ("color", Color("primary", "300")),
                        ("text-decoration-color", Mix(Color("primary", "300"), "35%"))),
                    Rule("a:hover",
                        ("text-decoration-color", Mix(Color("primary", "300"), "90%"))),
                    Rule("strong",
                        ("color", Color("base", "50"))),
                    Rule("hr",
                        ("border-top-color", Color("base", "800"))),
                    Rule("pre",
                        ("font-weight", "300"),
                        ("background-color", Mix(Color("base", "800"), "75%")),
                        ("box-shadow", "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)")),
                    Rule(":not(pre) > code",
                        ("background-color", Color("base", "800")),
                        ("border-color", Color("base", "700")),
                        ("color", Color("base", "200")))),
            }.ToImmutableDictionary();
        },
    };

    private static ProseElementRule Rule(string selector, params (string Property, string Value)[] declarations) => new()
    {
        Selector = selector,
        Declarations = [.. declarations.Select(d => new ProseDeclaration { Property = d.Property, Value = d.Value })],
    };

    private static ProseElementRules Rules(params ProseElementRule[] rules) => new()
    {
        Rules = [.. rules],
    };
}
