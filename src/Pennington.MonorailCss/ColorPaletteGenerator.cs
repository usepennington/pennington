using System.Collections.Immutable;

namespace Pennington.MonorailCss;

/// <summary>
/// Generates OKLCH color palettes from hue values for use with MonorailCSS themes.
/// </summary>
public static class ColorPaletteGenerator
{
    // Chroma values following Gaussian distribution (peaks at 500-600 range)
    // Based on Tailwind v4 actual red palette analysis
    private static readonly double[] ChromaLevels =
    [
        0.013, 0.032, 0.062, 0.114, 0.191, 0.237, 0.245, 0.213, 0.177, 0.141, 0.092
    ];

    private static readonly double[] LightnessLevels =
    [
        97.10, 93.60, 88.50, 80.80, 70.40, 63.70, 57.70, 50.50, 44.40, 39.60, 25.80
    ];

    // Keys for the palette
    private static readonly string[] PaletteKeys =
    [
        "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950"
    ];

    /// <summary>
    /// Generates a primary and accent color palette based on a hue value in degrees (0-360)
    /// </summary>
    public static ImmutableDictionary<string, string> GenerateFromHue(double hue)
    {
        // Normalize the hue to 0-360 range
        hue = (hue % 360 + 360) % 360;

        // Generate a primary palette (with the exact hue)
        return GeneratePaletteFromHue(hue);
    }

    /// <summary>
    /// Generates a palette from a specific hue value
    /// </summary>
    private static ImmutableDictionary<string, string> GeneratePaletteFromHue(double hue)
    {
        var palette = new Dictionary<string, string>();

        // Generate colors for each step in the palette
        for (var i = 0; i < PaletteKeys.Length; i++)
        {
            // Apply per-hue lightness adjustment (yellows/greens are lighter)
            var adjustedLightnessPercent = LightnessLevels[i] + GetLightnessAdjustment(hue, i);
            var lightness = adjustedLightnessPercent / 100.0; // Convert to 0-1 range for OKLCH

            // Apply per-hue chroma adjustment (yellows have lower chroma)
            var chroma = ChromaLevels[i] * GetChromaMultiplier(hue);

            // Apply hue rotation for more vibrant colors in dark shades
            var adjustedHue = hue + CalculateHueShift(hue, i);

            // Apply smoothing at extremes to prevent over-desaturation
            var adjustedChroma = chroma * GetSmoothingFactor(i);

            // Create the OKLCH color
            var oklchColor = $"oklch({lightness:F3} {adjustedChroma:F3} {adjustedHue:F3})";
            palette.Add(PaletteKeys[i], oklchColor);
        }

        return palette.ToImmutableDictionary();
    }

    /// <summary>
    /// Calculates hue shift based on color range and palette position
    /// Yellows shift toward orange, blues toward violet in darker shades
    /// </summary>
    private static double CalculateHueShift(double baseHue, int paletteIndex)
    {
        // Normalize hue to 0-360
        var normalizedHue = (baseHue % 360 + 360) % 360;

        // No shift for lightest shade (50)
        if (paletteIndex == 0) return 0.0;

        // Yellows and golds (45-110) shift strongly toward orange/red in dark shades
        // Uses a gentler curve - balanced shift progression
        if (normalizedHue >= 45 && normalizedHue <= 110)
        {
            // Curve calibrated to match Tailwind: ~30% at shade 500, 100% at shade 950
            var yellowIntensity = Math.Pow(paletteIndex / 10.0, 1.8);
            return -48.0 * yellowIntensity;
        }

        // Calculate shift intensity using a curve that peaks around shade 700
        // Parabolic curve: starts small, builds through 500, peaks at 700, slightly decreases
        double shiftIntensity;
        if (paletteIndex <= 7) // Shades 100-700
        {
            // Quadratic growth from 0 to 1
            shiftIntensity = Math.Pow(paletteIndex / 7.0, 1.5);
        }
        else // Shades 800-950
        {
            // Slight decrease after peak
            shiftIntensity = 1.0 - (paletteIndex - 7) * 0.05;
        }

        // Blues (200-260) shift toward violet (increase hue)
        if (normalizedHue >= 200 && normalizedHue <= 260)
        {
            return 15.0 * shiftIntensity;
        }

        // Reds (330-360 or 0-30) shift toward orange-red for vibrant darker shades
        if (normalizedHue >= 330 || normalizedHue <= 30)
        {
            return 14.0 * shiftIntensity;
        }

        // Cyans/teals (150-200) shift slightly toward blue
        if (normalizedHue >= 150 && normalizedHue < 200)
        {
            return 8.0 * shiftIntensity;
        }

        // Other hues: minimal adjustment
        return 0.0;
    }

    /// <summary>
    /// Applies smoothing factor at extreme ends to prevent full desaturation
    /// </summary>
    private static double GetSmoothingFactor(int paletteIndex)
    {
        // Reduce chroma slightly at the lightest shade (50)
        if (paletteIndex == 0) return 0.8;

        // Slight reduction at 100
        if (paletteIndex == 1) return 0.9;

        // Full chroma in middle range (200-900)
        if (paletteIndex >= 2 && paletteIndex <= 9) return 1.0;

        // Slight reduction at darkest shade (950)
        if (paletteIndex == 10) return 0.85;

        return 1.0;
    }

    /// <summary>
    /// Calculates lightness adjustment based on hue to match Tailwind v4's per-color curves
    /// Yellows and greens are lighter in middle shades, blues slightly lighter in dark shades
    /// </summary>
    private static double GetLightnessAdjustment(double hue, int paletteIndex)
    {
        // Normalize hue to 0-360
        var normalizedHue = (hue % 360 + 360) % 360;

        // Yellows and golds (45-110) - significantly lighter in middle ranges
        if (normalizedHue >= 45 && normalizedHue <= 110)
        {
            return paletteIndex switch
            {
                0 => 1.6,   // 50: +1.6%
                1 => 0.0,   // 100: no adjustment
                2 => 0.0,   // 200: no adjustment
                3 => 7.0,   // 300: +7%
                4 => 12.0,  // 400: +12%
                5 => 15.8,  // 500: +15.8%
                6 => 10.0,  // 600: +10%
                7 => 7.0,   // 700: +7%
                8 => 5.0,   // 800: +5%
                9 => 3.5,   // 900: +3.5%
                10 => 2.8,  // 950: +2.8%
                _ => 0.0
            };
        }

        // Greens (110-170) - moderately lighter in middle ranges
        if (normalizedHue >= 110 && normalizedHue <= 170)
        {
            return paletteIndex switch
            {
                0 => 1.1,   // 50: +1.1%
                1 => 0.0,   // 100: no adjustment
                2 => 0.0,   // 200: no adjustment
                3 => 4.5,   // 300: +4.5%
                4 => 7.5,   // 400: +7.5%
                5 => 8.6,   // 500: +8.6%
                6 => 7.0,   // 600: +7%
                7 => 5.0,   // 700: +5%
                8 => 3.0,   // 800: +3%
                9 => 1.5,   // 900: +1.5%
                10 => 0.8,  // 950: +0.8%
                _ => 0.0
            };
        }

        // Blues (200-260) - slightly lighter only in darkest shade
        if (normalizedHue >= 200 && normalizedHue <= 260)
        {
            return paletteIndex switch
            {
                0 => -0.1,  // 50: -0.1%
                10 => 2.4,  // 950: +2.4%
                _ => 0.0
            };
        }

        // All other hues (reds, magentas, cyans, etc.) use base curve
        return 0.0;
    }

    /// <summary>
    /// Calculates chroma adjustment based on hue to match Tailwind v4's per-color curves
    /// Yellows have lower chroma than reds
    /// </summary>
    private static double GetChromaMultiplier(double hue)
    {
        // Normalize hue to 0-360
        var normalizedHue = (hue % 360 + 360) % 360;

        // Yellows and golds (45-110) have lower chroma than reds
        if (normalizedHue >= 45 && normalizedHue <= 110)
        {
            return 0.78; // 22% reduction to match Tailwind yellow
        }

        // All other hues use base chroma curve
        return 1.0;
    }
}
