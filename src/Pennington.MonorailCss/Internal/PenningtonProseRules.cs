using System.Collections.Immutable;
using MonorailCss.Theme;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Pennington's default <see cref="ProseCustomization"/> — link, blockquote, code, and pre styling
/// shared by light and inverted prose.
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
                    Rule("a",
                        ("font-weight", "700"),
                        ("text-decoration", "none")),
                    Rule("a:not(:has(> code))",
                        ("border-bottom-width", "1px"),
                        ("border-bottom-color", Mix(Color("primary", "500"), "75%"))),
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
                        ("padding", "3px 8px"),
                        ("box-shadow", "inset 0 0 0 1px oklch(87.1% .006 286.286)"),
                        ("border-radius", "0.4rem"),
                        ("background-color", Mix(Color("base", "200"), "50%")),
                        ("color", Color("base", "700")),
                        ("word-break", "break-word"))),
                ["base"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.8em"))),
                ["sm"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.8em"))),
                ["invert"] = Rules(
                    Rule("pre",
                        ("font-weight", "300"),
                        ("background-color", Mix(Color("base", "800"), "75%")),
                        ("box-shadow", "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)")),
                    Rule(":not(pre) > code",
                        ("background-color", Mix(Color("base", "800"), "75%")),
                        ("color", Color("base", "200")),
                        ("box-shadow", "inset 0 0 0 1px oklab(100% 0 5.96046e-8/.1)"))),
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
