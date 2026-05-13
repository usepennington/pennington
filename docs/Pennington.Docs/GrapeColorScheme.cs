using System.Collections.Immutable;
using MonorailCss.Theme;
using Pennington.MonorailCss;

namespace Pennington.Docs;

/// <summary>
/// Hand-tuned OKLCH palette scoped to the Pennington docs site.
/// Registers four brand palettes (grape, english, windmill, dogs)
/// and four syntax-highlight accents (accent-one through accent-four), then
/// maps grape/windmill/english onto the primary/accent/base slots.
/// </summary>
internal sealed class GrapeColorScheme : IColorScheme
{
    private static readonly ImmutableDictionary<string, string> Grape = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 301.039)",
        ["100"] = "oklch(94% 0.022 301.039)",
        ["200"] = "oklch(89% 0.041 301.039)",
        ["300"] = "oklch(82% 0.075 301.039)",
        ["400"] = "oklch(72% 0.123 301.039)",
        ["500"] = "oklch(64% 0.159 301.039)",
        ["600"] = "oklch(56% 0.167 301.039)",
        ["700"] = "oklch(49% 0.153 301.039)",
        ["800"] = "oklch(43% 0.124 301.039)",
        ["900"] = "oklch(38% 0.099 301.039)",
        ["950"] = "oklch(27% 0.067 301.039)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> English = new Dictionary<string, string>
    {
        ["50"]  = "oklch(98.5% 0.004 301.039)",
        ["100"] = "oklch(97% 0.005 301.039)",
        ["200"] = "oklch(92.5% 0.009 301.039)",
        ["300"] = "oklch(87% 0.018 301.039)",
        ["400"] = "oklch(71% 0.032 301.039)",
        ["500"] = "oklch(55% 0.050 301.039)",
        ["600"] = "oklch(44% 0.045 301.039)",
        ["700"] = "oklch(37% 0.042 301.039)",
        ["800"] = "oklch(27% 0.036 301.039)",
        ["900"] = "oklch(21% 0.031 301.039)",
        ["950"] = "oklch(14% 0.025 301.039)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Windmill = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 271.039)",
        ["100"] = "oklch(94% 0.022 271.039)",
        ["200"] = "oklch(89% 0.041 271.039)",
        ["300"] = "oklch(82% 0.075 271.039)",
        ["400"] = "oklch(72% 0.123 271.039)",
        ["500"] = "oklch(64% 0.159 271.039)",
        ["600"] = "oklch(56% 0.167 271.039)",
        ["700"] = "oklch(49% 0.153 271.039)",
        ["800"] = "oklch(43% 0.124 271.039)",
        ["900"] = "oklch(38% 0.099 271.039)",
        ["950"] = "oklch(27% 0.067 271.039)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Dogs = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 331.039)",
        ["100"] = "oklch(94% 0.022 331.039)",
        ["200"] = "oklch(89% 0.041 331.039)",
        ["300"] = "oklch(82% 0.075 331.039)",
        ["400"] = "oklch(72% 0.123 331.039)",
        ["500"] = "oklch(64% 0.159 331.039)",
        ["600"] = "oklch(56% 0.167 331.039)",
        ["700"] = "oklch(49% 0.153 331.039)",
        ["800"] = "oklch(43% 0.124 331.039)",
        ["900"] = "oklch(38% 0.099 331.039)",
        ["950"] = "oklch(27% 0.067 331.039)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> AccentOne = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 11.054)",
        ["100"] = "oklch(94% 0.023 11.054)",
        ["200"] = "oklch(89% 0.043 11.054)",
        ["300"] = "oklch(82% 0.078 11.054)",
        ["400"] = "oklch(72% 0.127 11.054)",
        ["500"] = "oklch(64% 0.165 11.054)",
        ["600"] = "oklch(56% 0.173 11.054)",
        ["700"] = "oklch(49% 0.158 11.054)",
        ["800"] = "oklch(43% 0.129 11.054)",
        ["900"] = "oklch(38% 0.102 11.054)",
        ["950"] = "oklch(27% 0.069 11.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> AccentTwo = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 101.054)",
        ["100"] = "oklch(94% 0.023 101.054)",
        ["200"] = "oklch(89% 0.043 101.054)",
        ["300"] = "oklch(82% 0.078 101.054)",
        ["400"] = "oklch(72% 0.127 101.054)",
        ["500"] = "oklch(64% 0.165 101.054)",
        ["600"] = "oklch(56% 0.173 101.054)",
        ["700"] = "oklch(49% 0.158 101.054)",
        ["800"] = "oklch(43% 0.129 101.054)",
        ["900"] = "oklch(38% 0.102 101.054)",
        ["950"] = "oklch(27% 0.069 101.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> AccentThree = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 191.054)",
        ["100"] = "oklch(94% 0.023 191.054)",
        ["200"] = "oklch(89% 0.043 191.054)",
        ["300"] = "oklch(82% 0.078 191.054)",
        ["400"] = "oklch(72% 0.127 191.054)",
        ["500"] = "oklch(64% 0.165 191.054)",
        ["600"] = "oklch(56% 0.173 191.054)",
        ["700"] = "oklch(49% 0.158 191.054)",
        ["800"] = "oklch(43% 0.129 191.054)",
        ["900"] = "oklch(38% 0.102 191.054)",
        ["950"] = "oklch(27% 0.069 191.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> AccentFour = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.010 251.054)",
        ["100"] = "oklch(94% 0.023 251.054)",
        ["200"] = "oklch(89% 0.043 251.054)",
        ["300"] = "oklch(82% 0.078 251.054)",
        ["400"] = "oklch(72% 0.127 251.054)",
        ["500"] = "oklch(64% 0.165 251.054)",
        ["600"] = "oklch(56% 0.173 251.054)",
        ["700"] = "oklch(49% 0.158 251.054)",
        ["800"] = "oklch(43% 0.129 251.054)",
        ["900"] = "oklch(38% 0.102 251.054)",
        ["950"] = "oklch(27% 0.069 251.054)",
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme) =>
        theme.AddColorPalette("grape", Grape)
             .AddColorPalette("english", English)
             .AddColorPalette("windmill", Windmill)
             .AddColorPalette("dogs", Dogs)
             .AddColorPalette("accent-one", AccentOne)
             .AddColorPalette("accent-two", AccentTwo)
             .AddColorPalette("accent-three", AccentThree)
             .AddColorPalette("accent-four", AccentFour)
             .MapColorPalette("grape", "primary")
             .MapColorPalette("windmill", "accent")
             .MapColorPalette("mauve", "base");
}
