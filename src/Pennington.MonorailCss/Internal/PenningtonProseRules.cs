using System.Collections.Immutable;
using MonorailCss.Theme;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Pennington's default <see cref="ProseCustomization"/> — a neutral, readable baseline
/// for the <c>prose</c> utility: primary-colored underlined links, native list bullets,
/// scroll-targeted headings, simple horizontal rules, plain blockquote, plain pre.
/// Consumers add brand flair (animated underline, custom bullet dots, inline-code chip,
/// gradient H2 accent bar, etc.) through
/// <see cref="MonorailCssOptions.ExtendProseCustomization"/>.
/// </summary>
internal static class PenningtonProseRules
{
    public static ProseCustomization Default { get; } = new()
    {
        Customization = theme =>
        {
            string Color(string color, string shade) =>
                theme.ResolveValue(shade, [$"--color-{color}"]) ?? "#000000";

            return new Dictionary<string, ProseElementRules>
            {
                ["DEFAULT"] = Rules(
                    Rule("p",
                        ("text-wrap", "pretty")),

                    Rule("a",
                        ("color", Color("primary", "700")),
                        ("font-weight", "500"),
                        ("text-decoration-line", "underline"),
                        ("text-underline-offset", "3px")),
                    Rule("strong",
                        ("color", Color("base", "900")),
                        ("font-weight", "600")),
                    Rule("hr",
                        ("border", "0"),
                        ("border-top", $"1px solid {Color("base", "200")}"),
                        ("margin", "3rem 0")),
                    Rule("h2",
                        ("scroll-margin-top", "5rem")),
                    Rule("h3",
                        ("scroll-margin-top", "5rem")),
                    Rule("blockquote",
                        ("border-left-width", "4px"),
                        ("padding-left", "1rem"),
                        ("border-color", Color("base", "300"))),
                    Rule("pre",
                        ("background-color", Color("base", "100")),
                        ("border-radius", "0.4rem")),
                    Rule("code",
                        ("font-weight", "400"))),
                ["base"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.86em"))),
                ["sm"] = Rules(
                    Rule("pre > code", ("font-size", "inherit")),
                    Rule(":not(pre) > code", ("font-size", "0.86em"))),
                ["invert"] = Rules(
                    Rule("a",
                        ("color", Color("primary", "300"))),
                    Rule("strong",
                        ("color", Color("base", "50"))),
                    Rule("hr",
                        ("border-top-color", Color("base", "800"))),
                    Rule("blockquote",
                        ("border-color", Color("base", "700"))),
                    Rule("pre",
                        ("background-color", Color("base", "800")))),
            }.ToImmutableDictionary();
        },
    };

    private static ProseElementRule Rule(string selector, params (string Property, string Value)[] declarations) =>
        new()
        {
            Selector = selector,
            Declarations =
                [.. declarations.Select(d => new ProseDeclaration { Property = d.Property, Value = d.Value })],
        };

    private static ProseElementRules Rules(params ProseElementRule[] rules) => new() { Rules = [.. rules], };
}
