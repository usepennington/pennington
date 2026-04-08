namespace YogaStudioExample.Models;

using System.Collections.Immutable;
using MonorailCss.Theme;
using Penn.MonorailCss;

public class YogaColorScheme : IColorScheme
{
    public Theme ApplyToTheme(Theme theme)
    {
        // Sage green "Amulet" — generated from UIHue (#7C9A72)
        var primary = new Dictionary<string, string>
        {
            ["50"] = "#f1fcef", ["100"] = "#e5f8df", ["200"] = "#d0f1c6",
            ["300"] = "#b1e7a5", ["400"] = "#96d282", ["500"] = "#8cbb79",
            ["600"] = "#7c9a72", ["700"] = "#5f7b59", ["800"] = "#466043",
            ["900"] = "#324e31", ["950"] = "#122b15",
        }.ToImmutableDictionary();

        // Terracotta "Twine" — generated from UIHue (#C4956A)
        var accent = new Dictionary<string, string>
        {
            ["50"] = "#fff7ed", ["100"] = "#ebdfd1", ["200"] = "#d6bda4",
            ["300"] = "#c4956a", ["400"] = "#cc6f28", ["500"] = "#d65500",
            ["600"] = "#d03b00", ["700"] = "#af2a00", ["800"] = "#8e260c",
            ["900"] = "#762612", ["950"] = "#441208",
        }.ToImmutableDictionary();

        // Warm stone "Bronco" — generated from UIHue (#A89F91)
        var baseColors = new Dictionary<string, string>
        {
            ["50"] = "#fafaf9", ["100"] = "#f4f5f2", ["200"] = "#e9e4df",
            ["300"] = "#d8d2c9", ["400"] = "#a89f91", ["500"] = "#797163",
            ["600"] = "#565347", ["700"] = "#444037", ["800"] = "#2a2420",
            ["900"] = "#1c1915", ["950"] = "#0c0a09",
        }.ToImmutableDictionary();

        return theme
            .AddColorPalette("primary", primary)
            .AddColorPalette("accent", accent)
            .AddColorPalette("tertiary-one", primary)
            .AddColorPalette("tertiary-two", accent)
            .AddColorPalette("base", baseColors);
    }
}
