using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Theme;

namespace Pennington.MonorailCss;

/// <summary>
/// Selects accent hues relative to the primary hue.
/// </summary>
public enum CoordinatingScheme
{
    /// <summary>Single accent at +180°.</summary>
    Complementary,

    /// <summary>Two accents at +150° and +210°.</summary>
    SplitComplementary,

    /// <summary>Two accents at +120° and +240°.</summary>
    Triadic,

    /// <summary>Two accents at -30° and +30°.</summary>
    Analogous,
}

/// <summary>
/// Generates Tailwind-style 11-step OKLCH palettes from a seed hue and chroma,
/// plus the coordinating accent and neutral base palettes that go with them.
/// </summary>
public static class ColorPaletteGenerator
{
    private static readonly string[] StepKeys =
    [
        "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950",
    ];

    private static readonly double[] LForeground =
    [
        0.970, 0.940, 0.890, 0.820, 0.720, 0.640, 0.560, 0.490, 0.430, 0.380, 0.270,
    ];

    private static readonly double[] LNeutral =
    [
        0.985, 0.970, 0.925, 0.870, 0.710, 0.550, 0.440, 0.370, 0.270, 0.210, 0.140,
    ];

    private static readonly double[] CForegroundFraction =
    [
        0.06, 0.14, 0.26, 0.47, 0.77, 1.00, 1.05, 0.96, 0.78, 0.62, 0.42,
    ];

    private static readonly double[] CNeutralBase =
    [
        0.002, 0.003, 0.005, 0.010, 0.018, 0.028, 0.024, 0.022, 0.016, 0.012, 0.006,
    ];

    private static readonly double[] CNeutralHeldTail =
    [
        0.002, 0.003, 0.005, 0.010, 0.018, 0.028, 0.026, 0.025, 0.024, 0.023, 0.022,
    ];

    // Step 500 is the anchor: foreground chroma is scaled so the 500 stop lands on the seed chroma,
    // and neutral chroma is scaled so the 500 stop matches its base curve at the seed's intensity.
    private const int AnchorIndex = 5;

    // Blend between the tapered tail (C_NEUTRAL_BASE 600..950) and the held-high tail
    // (C_NEUTRAL_HELD_TAIL). 0.5 splits the difference, keeping dark neutrals with a usable tint.
    private const double DarkChromaRetention = 0.5;

    // Base chroma derives from primary chroma but is clamped — saturated primaries shouldn't
    // produce a 100% tinted base, and near-gray primaries still need a faint hue cast to read.
    private const double BaseChromaFraction = 0.33;
    private const double BaseChromaMin = 0.02;
    private const double BaseChromaMax = 0.05;

    /// <summary>
    /// Wires algorithmic palettes (base + primary + accents) onto the theme.
    /// Primary is a foreground palette at <paramref name="hue"/>/<paramref name="chroma"/>;
    /// base is a desaturated neutral palette at the same hue; accents are foreground palettes
    /// at hues picked by <paramref name="scheme"/>. Schemes with multiple accents register the
    /// first as <c>accent</c> and additional ones as <c>accent-2</c>, <c>accent-3</c>, ….
    /// </summary>
    public static Theme ApplyAlgorithmicColorScheme(this Theme theme, double hue, double chroma, CoordinatingScheme scheme)
    {
        var primary = GenerateForeground(hue, chroma);
        var baseChroma = Math.Clamp(chroma * BaseChromaFraction, BaseChromaMin, BaseChromaMax);
        var basePalette = GenerateNeutral(hue, baseChroma);

        theme = theme.AddColorPalette("primary", primary)
            .AddColorPalette("base", basePalette);

        var offsets = GetOffsets(scheme);
        for (var i = 0; i < offsets.Length; i++)
        {
            var accent = GenerateForeground(hue + offsets[i], chroma);
            var slot = i == 0 ? "accent" : $"accent-{i + 1}";
            theme = theme.AddColorPalette(slot, accent);
        }

        return theme;
    }

    /// <summary>
    /// Generates a vibrant foreground 11-step OKLCH palette. The 500 stop lands on
    /// <paramref name="chroma"/>; other stops scale relative to it via the foreground chroma curve.
    /// </summary>
    public static ImmutableDictionary<string, string> GenerateForeground(double hue, double chroma)
    {
        var h = NormalizeHue(hue);
        var peak = chroma / CForegroundFraction[AnchorIndex];

        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        for (var i = 0; i < StepKeys.Length; i++)
        {
            var c = Math.Max(0, peak * CForegroundFraction[i]);
            builder.Add(StepKeys[i], FormatOklch(LForeground[i], c, h));
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Generates a near-gray neutral 11-step OKLCH palette. Chroma scales by intensity
    /// (<paramref name="chroma"/> / curve at 500) and the dark tail blends between tapered
    /// and held-high shapes via <see cref="DarkChromaRetention"/>.
    /// </summary>
    public static ImmutableDictionary<string, string> GenerateNeutral(double hue, double chroma)
    {
        var h = NormalizeHue(hue);
        var intensity = chroma / CNeutralBase[AnchorIndex];

        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        for (var i = 0; i < StepKeys.Length; i++)
        {
            var shape = i < 6
                ? CNeutralBase[i]
                : CNeutralBase[i] * (1 - DarkChromaRetention) + CNeutralHeldTail[i] * DarkChromaRetention;
            var c = Math.Max(0, shape * intensity);
            builder.Add(StepKeys[i], FormatOklch(LNeutral[i], c, h));
        }

        return builder.ToImmutable();
    }

    private static double[] GetOffsets(CoordinatingScheme scheme) => scheme switch
    {
        CoordinatingScheme.Complementary => [180.0],
        CoordinatingScheme.SplitComplementary => [150.0, 210.0],
        CoordinatingScheme.Triadic => [120.0, 240.0],
        CoordinatingScheme.Analogous => [-30.0, 30.0],
        _ => throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null),
    };

    private static double NormalizeHue(double hue) => ((hue % 360) + 360) % 360;

    private static string FormatOklch(double l, double c, double h) =>
        string.Format(CultureInfo.InvariantCulture, "oklch({0:F3} {1:F3} {2:F3})", l, c, h);
}
