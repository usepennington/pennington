using MonorailCss.Theme;

namespace Pennington.MonorailCss;

/// <summary>
/// A curated, named color theme: one seed hue grows the algorithmic <c>primary</c> and
/// <c>accent</c> brand palettes plus a coordinating OKLCH syntax-highlight palette, while
/// <c>base</c> is the stock MonorailCss neutral whose undertone sits nearest the hue (see
/// <see cref="NeutralForHue"/>). Assign <see cref="MonorailCssOptions.ColorScheme"/> to the theme
/// and <see cref="MonorailCssOptions.SyntaxTheme"/> to its <see cref="SyntaxTheme"/>.
/// </summary>
/// <remarks>
/// The <c>primary</c>/<c>accent</c> roles come from
/// <see cref="ColorPaletteGenerator.ApplyAlgorithmicColorScheme"/> (its generated base is replaced
/// by the named neutral); the syntax tokens are foreground palettes at a tetradic spread off the primary hue
/// (<c>+0/+60/+150/+240°</c>), mirroring the layout you can preview and tune at
/// <see href="https://monorailcss.github.io/color-scheme-gen/"/>. The built-in catalog
/// (<see cref="Ember"/> … <see cref="Graphite"/>, enumerated by <see cref="All"/>) walks the
/// hue wheel; construct your own with any <see cref="PrimaryHue"/> / <see cref="Chroma"/> /
/// <see cref="Coordinating"/> seed.
/// </remarks>
public sealed record ColorTheme : IColorScheme
{
    // Syntax token hues, relative to the primary hue — the playground's tetradic spread.
    // Keyword sits on the primary hue; comment tracks the neutral base instead of an accent.
    private const double KeywordHueOffset = 0;
    private const double StringHueOffset = 60;
    private const double VariableHueOffset = 150;
    private const double FunctionHueOffset = 240;

    // Floor for the syntax palettes so token colors stay legible even when the brand seed is
    // near-gray (e.g. Graphite). A no-op for every catalog theme with Chroma >= this value.
    private const double SyntaxChromaFloor = 0.11;

    /// <summary>Display name of the theme.</summary>
    public required string Name { get; init; }

    /// <summary>Primary hue in degrees (0–360); the seed every palette is grown from.</summary>
    public required double PrimaryHue { get; init; }

    /// <summary>
    /// Seed chroma for the brand foreground palettes. Typical range 0.05 (muted) to 0.18 (vivid);
    /// the 500 stop of the generated primary lands on this value.
    /// </summary>
    public double Chroma { get; init; } = 0.15;

    /// <summary>How coordinating accent hues are picked relative to <see cref="PrimaryHue"/>.</summary>
    public CoordinatingScheme Coordinating { get; init; } = CoordinatingScheme.Complementary;

    /// <summary>
    /// Optional override for the <c>base</c> neutral. Leave it <see langword="null"/> (the default)
    /// to auto-pick the MonorailCss neutral whose undertone is nearest <see cref="PrimaryHue"/> via
    /// <see cref="NeutralForHue"/>. Set it to a specific family (e.g. <see cref="ColorName.Zinc"/>, or
    /// <see cref="ColorName.Neutral"/> for crisp untinted grays) to force that one instead. Comments —
    /// which track <c>base</c> — follow this choice too.
    /// </summary>
    public ColorName? BaseColorName { get; init; }

    // 500-stop OKLCH undertone hue of every named neutral MonorailCss ships, in hue order. Pure
    // `neutral` is excluded — it has no hue, so it is never auto-selected (request it explicitly via
    // BaseColorName). NeutralForHue snaps the seed to the family whose undertone sits nearest, so a
    // warm seed lands on stone/taupe, a cool one on slate/gray, a magenta one on mauve, and so on.
    private static readonly (ColorName Name, double Hue)[] NeutralHues =
    [
        (ColorName.Taupe, 43.1),
        (ColorName.Stone, 58.1),
        (ColorName.Olive, 107.3),
        (ColorName.Mist, 213.5),
        (ColorName.Slate, 257.4),
        (ColorName.Gray, 264.4),
        (ColorName.Zinc, 285.9),
        (ColorName.Mauve, 322.5),
    ];

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme)
    {
        theme = theme.ApplyAlgorithmicColorScheme(PrimaryHue, Chroma, Coordinating);

        // Replace the generated base with a stock MonorailCss neutral — the requested family, or the
        // one whose undertone sits nearest the seed hue — so the surface grays coordinate with the
        // brand. The algorithmic primary/accent palettes are untouched.
        var neutral = BaseColorName ?? NeutralForHue(PrimaryHue);
        theme = theme.MapColorPalette(neutral.Value, "base");

        var syntaxChroma = Math.Max(Chroma, SyntaxChromaFloor);
        return theme
            .AddColorPalette("syntax-keyword", ColorPaletteGenerator.GenerateForeground(PrimaryHue + KeywordHueOffset, syntaxChroma))
            .AddColorPalette("syntax-string", ColorPaletteGenerator.GenerateForeground(PrimaryHue + StringHueOffset, syntaxChroma))
            .AddColorPalette("syntax-variable", ColorPaletteGenerator.GenerateForeground(PrimaryHue + VariableHueOffset, syntaxChroma))
            .AddColorPalette("syntax-function", ColorPaletteGenerator.GenerateForeground(PrimaryHue + FunctionHueOffset, syntaxChroma));
    }

    /// <summary>
    /// Picks the MonorailCss neutral palette whose undertone hue sits nearest <paramref name="hue"/>
    /// (measured around the color wheel), giving <c>base</c> grays that coordinate with the brand.
    /// The pure, hueless <see cref="ColorName.Neutral"/> is never auto-selected — request it explicitly
    /// via <see cref="BaseColorName"/> when you want untinted grays.
    /// </summary>
    /// <param name="hue">Seed hue in degrees; normalized into 0–360 before comparison.</param>
    public static ColorName NeutralForHue(double hue)
    {
        var h = ((hue % 360) + 360) % 360;
        return NeutralHues.MinBy(n => HueDistance(h, n.Hue)).Name;
    }

    // Shortest angular distance between two hues, in degrees (0–180).
    private static double HueDistance(double a, double b)
    {
        var d = Math.Abs(a - b) % 360;
        return Math.Min(d, 360 - d);
    }

    /// <summary>
    /// The syntax-highlight theme paired with this color theme. Keyword/string/variable/function
    /// map onto the generated <c>syntax-*</c> accent palettes (registered by
    /// <see cref="ApplyToTheme"/>); comments track the neutral <c>base</c> palette.
    /// </summary>
    public SyntaxTheme SyntaxTheme => new()
    {
        Keyword = new ColorName("syntax-keyword"),
        String = new ColorName("syntax-string"),
        Variable = new ColorName("syntax-variable"),
        Function = new ColorName("syntax-function"),
        Comment = new ColorName("base"),
    };

    /// <summary>Warm red-orange with cyan rhythm. Split-complementary.</summary>
    public static ColorTheme Ember { get; } = new()
    {
        Name = "Ember", PrimaryHue = 30, Chroma = 0.14, Coordinating = CoordinatingScheme.SplitComplementary,
    };

    /// <summary>Golden amber with a blue complement.</summary>
    public static ColorTheme Marigold { get; } = new()
    {
        Name = "Marigold", PrimaryHue = 70, Chroma = 0.13, Coordinating = CoordinatingScheme.Complementary,
    };

    /// <summary>Yellow-green citrus, triadic for a punchy three-color set.</summary>
    public static ColorTheme Citron { get; } = new()
    {
        Name = "Citron", PrimaryHue = 115, Chroma = 0.13, Coordinating = CoordinatingScheme.Triadic,
    };

    /// <summary>Leafy green with neighbouring analogous accents.</summary>
    public static ColorTheme Fern { get; } = new()
    {
        Name = "Fern", PrimaryHue = 150, Chroma = 0.12, Coordinating = CoordinatingScheme.Analogous,
    };

    /// <summary>Deep teal, split-complementary toward warm coral.</summary>
    public static ColorTheme Lagoon { get; } = new()
    {
        Name = "Lagoon", PrimaryHue = 188, Chroma = 0.11, Coordinating = CoordinatingScheme.SplitComplementary,
    };

    /// <summary>Bright cyan-blue with an orange complement.</summary>
    public static ColorTheme Aqua { get; } = new()
    {
        Name = "Aqua", PrimaryHue = 210, Chroma = 0.12, Coordinating = CoordinatingScheme.Complementary,
    };

    /// <summary>Classic azure blue with a warm complement.</summary>
    public static ColorTheme Azure { get; } = new()
    {
        Name = "Azure", PrimaryHue = 242, Chroma = 0.14, Coordinating = CoordinatingScheme.Complementary,
    };

    /// <summary>Deep indigo, triadic across the wheel.</summary>
    public static ColorTheme Indigo { get; } = new()
    {
        Name = "Indigo", PrimaryHue = 270, Chroma = 0.15, Coordinating = CoordinatingScheme.Triadic,
    };

    /// <summary>Violet iris, split-complementary toward yellow-green.</summary>
    public static ColorTheme Iris { get; } = new()
    {
        Name = "Iris", PrimaryHue = 295, Chroma = 0.15, Coordinating = CoordinatingScheme.SplitComplementary,
    };

    /// <summary>Magenta orchid with a green complement.</summary>
    public static ColorTheme Orchid { get; } = new()
    {
        Name = "Orchid", PrimaryHue = 325, Chroma = 0.15, Coordinating = CoordinatingScheme.Complementary,
    };

    /// <summary>Pink-red rose with analogous warmth.</summary>
    public static ColorTheme Rose { get; } = new()
    {
        Name = "Rose", PrimaryHue = 0, Chroma = 0.14, Coordinating = CoordinatingScheme.Analogous,
    };

    /// <summary>Restrained blue-gray for a quiet, professional brand; syntax stays legible via the chroma floor.</summary>
    public static ColorTheme Graphite { get; } = new()
    {
        Name = "Graphite", PrimaryHue = 250, Chroma = 0.05, Coordinating = CoordinatingScheme.Complementary,
    };

    /// <summary>The full curated catalog, in hue-wheel order.</summary>
    public static IReadOnlyList<ColorTheme> All { get; } =
    [
        Ember, Marigold, Citron, Fern, Lagoon, Aqua, Azure, Indigo, Iris, Orchid, Rose, Graphite,
    ];
}
