using System.Collections.Immutable;
using MonorailCss.Theme;
using Pennington.MonorailCss;

namespace Pennington.Docs;

/// <summary>
/// Hand-tuned OKLCH palette scoped to the Pennington docs site.
/// Registers four brand palettes (snugglepuss, pewter, aventurine, winchester)
/// and four syntax-highlight accents (accent-one through accent-four), then
/// maps snugglepuss/winchester/pewter onto the primary/accent/base slots.
/// </summary>
internal sealed class SnugglepussColorScheme : IColorScheme
{
    private static readonly ImmutableDictionary<string, string> Snugglepuss = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.006 311.054)",
        ["100"] = "oklch(94% 0.014 311.054)",
        ["200"] = "oklch(89% 0.026 311.054)",
        ["300"] = "oklch(82% 0.046 311.054)",
        ["400"] = "oklch(72% 0.076 311.054)",
        ["500"] = "oklch(64% 0.099 311.054)",
        ["600"] = "oklch(56% 0.104 311.054)",
        ["700"] = "oklch(49% 0.095 311.054)",
        ["800"] = "oklch(43% 0.077 311.054)",
        ["900"] = "oklch(38% 0.061 311.054)",
        ["950"] = "oklch(27% 0.042 311.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Pewter = new Dictionary<string, string>
    {
        ["50"]  = "oklch(98.5% 0.003 311.054)",
        ["100"] = "oklch(97% 0.004 311.054)",
        ["200"] = "oklch(92.5% 0.007 311.054)",
        ["300"] = "oklch(87% 0.014 311.054)",
        ["400"] = "oklch(71% 0.026 311.054)",
        ["500"] = "oklch(55% 0.040 311.054)",
        ["600"] = "oklch(44% 0.036 311.054)",
        ["700"] = "oklch(37% 0.034 311.054)",
        ["800"] = "oklch(27% 0.029 311.054)",
        ["900"] = "oklch(21% 0.025 311.054)",
        ["950"] = "oklch(14% 0.020 311.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Aventurine = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.006 101.054)",
        ["100"] = "oklch(94% 0.014 101.054)",
        ["200"] = "oklch(89% 0.026 101.054)",
        ["300"] = "oklch(82% 0.046 101.054)",
        ["400"] = "oklch(72% 0.076 101.054)",
        ["500"] = "oklch(64% 0.099 101.054)",
        ["600"] = "oklch(56% 0.104 101.054)",
        ["700"] = "oklch(49% 0.095 101.054)",
        ["800"] = "oklch(43% 0.077 101.054)",
        ["900"] = "oklch(38% 0.061 101.054)",
        ["950"] = "oklch(27% 0.042 101.054)",
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Winchester = new Dictionary<string, string>
    {
        ["50"]  = "oklch(97% 0.006 161.054)",
        ["100"] = "oklch(94% 0.014 161.054)",
        ["200"] = "oklch(89% 0.026 161.054)",
        ["300"] = "oklch(82% 0.046 161.054)",
        ["400"] = "oklch(72% 0.076 161.054)",
        ["500"] = "oklch(64% 0.099 161.054)",
        ["600"] = "oklch(56% 0.104 161.054)",
        ["700"] = "oklch(49% 0.095 161.054)",
        ["800"] = "oklch(43% 0.077 161.054)",
        ["900"] = "oklch(38% 0.061 161.054)",
        ["950"] = "oklch(27% 0.042 161.054)",
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
        theme.AddColorPalette("snugglepuss", Snugglepuss)
             .AddColorPalette("pewter", Pewter)
             .AddColorPalette("aventurine", Aventurine)
             .AddColorPalette("winchester", Winchester)
             .AddColorPalette("accent-one", AccentOne)
             .AddColorPalette("accent-two", AccentTwo)
             .AddColorPalette("accent-three", AccentThree)
             .AddColorPalette("accent-four", AccentFour)
             .MapColorPalette("snugglepuss", "primary")
             .MapColorPalette("winchester", "accent")
             .MapColorPalette(ColorNames.Zinc, "base");
}
