using MonorailCss;
using MonorailCss.Theme;

namespace Pennington.MonorailCss;

/// <summary>
/// A color reference that provides IntelliSense discoverability for known Tailwind colors
/// while still accepting arbitrary custom color names via implicit string conversion.
/// </summary>
/// <param name="Value">Underlying color name (Tailwind palette key or custom identifier).</param>
public readonly record struct ColorName(string Value)
{
    /// <summary>Converts a plain string to a <see cref="ColorName"/>.</summary>
    public static implicit operator ColorName(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Red</summary>
    public static ColorName Red => new(ColorNames.Red);
    /// <summary>Orange</summary>
    public static ColorName Orange => new(ColorNames.Orange);
    /// <summary>Amber</summary>
    public static ColorName Amber => new(ColorNames.Amber);
    /// <summary>Yellow</summary>
    public static ColorName Yellow => new(ColorNames.Yellow);
    /// <summary>Lime</summary>
    public static ColorName Lime => new(ColorNames.Lime);
    /// <summary>Green</summary>
    public static ColorName Green => new(ColorNames.Green);
    /// <summary>Emerald</summary>
    public static ColorName Emerald => new(ColorNames.Emerald);
    /// <summary>Teal</summary>
    public static ColorName Teal => new(ColorNames.Teal);
    /// <summary>Cyan</summary>
    public static ColorName Cyan => new(ColorNames.Cyan);
    /// <summary>Sky</summary>
    public static ColorName Sky => new(ColorNames.Sky);
    /// <summary>Blue</summary>
    public static ColorName Blue => new(ColorNames.Blue);
    /// <summary>Indigo</summary>
    public static ColorName Indigo => new(ColorNames.Indigo);
    /// <summary>Violet</summary>
    public static ColorName Violet => new(ColorNames.Violet);
    /// <summary>Purple</summary>
    public static ColorName Purple => new(ColorNames.Purple);
    /// <summary>Fuchsia</summary>
    public static ColorName Fuchsia => new(ColorNames.Fuchsia);
    /// <summary>Pink</summary>
    public static ColorName Pink => new(ColorNames.Pink);
    /// <summary>Rose</summary>
    public static ColorName Rose => new(ColorNames.Rose);
    /// <summary>Slate</summary>
    public static ColorName Slate => new(ColorNames.Slate);
    /// <summary>Gray</summary>
    public static ColorName Gray => new(ColorNames.Gray);
    /// <summary>Zinc</summary>
    public static ColorName Zinc => new(ColorNames.Zinc);
    /// <summary>Neutral</summary>
    public static ColorName Neutral => new(ColorNames.Neutral);
    /// <summary>Stone</summary>
    public static ColorName Stone => new(ColorNames.Stone);
    /// <summary>Mauve</summary>
    public static ColorName Mauve => new(ColorNames.Mauve);
    /// <summary>Olive</summary>
    public static ColorName Olive => new(ColorNames.Olive);
    /// <summary>Mist</summary>
    public static ColorName Mist => new(ColorNames.Mist);
    /// <summary>Taupe</summary>
    public static ColorName Taupe => new(ColorNames.Taupe);
    /// <summary>Black</summary>
    public static ColorName Black => new(ColorNames.Black);
    /// <summary>White</summary>
    public static ColorName White => new(ColorNames.White);
}

/// <summary>
/// Options for configuring the Monorail CSS framework integration.
/// </summary>
public class MonorailCssOptions
{
    /// <summary>
    /// Gets or sets the color scheme for the site.
    /// The default is a NamedColorScheme with Blue (primary), Purple (accent), and Slate (base).
    /// </summary>
    public IColorScheme ColorScheme { get; init; } = new NamedColorScheme
    {
        PrimaryColorName = ColorName.Blue,
        AccentColorName = ColorName.Purple,
        BaseColorName = ColorName.Slate
    };

    /// <summary>
    /// Gets or sets the syntax-highlight color theme.
    /// Controls the Tailwind palettes used by <c>.hljs-*</c> token classes,
    /// independent of the site's brand <see cref="ColorScheme"/>.
    /// </summary>
    public SyntaxTheme SyntaxTheme { get; init; } = SyntaxTheme.Default;

    /// <summary>
    /// Gets or sets a function to customize the CSS framework settings.
    /// This allows for advanced customization of the MonorailCSS framework.
    /// </summary>
    public Func<CssFrameworkSettings, CssFrameworkSettings> CustomCssFrameworkSettings { get; init; } =
        settings => settings;

    /// <summary>
    /// Gets or sets any extra CSS styles to be included in the generated stylesheet.
    /// </summary>
    public string ExtraStyles { get; init; } = string.Empty;
}

/// <summary>
/// Defines how color schemes are applied to the MonorailCSS theme.
/// </summary>
public interface IColorScheme
{
    /// <summary>
    /// Applies the color scheme to the given theme.
    /// </summary>
    /// <param name="theme">The theme to apply colors to</param>
    Theme ApplyToTheme(Theme theme);
}

/// <summary>
/// A color scheme that generates palettes algorithmically from hue values.
/// </summary>
public class AlgorithmicColorScheme : IColorScheme
{
    /// <summary>
    /// Gets or sets the primary hue value (0-360).
    /// </summary>
    public required int PrimaryHue { get; init; }

    /// <summary>
    /// Gets or sets the base color name from the MonorailCSS color palette.
    /// The default value is "Gray".
    /// </summary>
    public ColorName BaseColorName { get; init; } = ColorName.Gray;

    /// <summary>
    /// Gets or sets the function that generates the accent hue from the primary hue.
    /// Defaults to the complementary hue (primary + 180Â°).
    /// </summary>
    public Func<int, int> ColorSchemeGenerator { get; init; } = primary => primary + 180;

    /// <summary>
    /// Gets or sets additional color mappings beyond the core slots.
    /// Key is the target slot name (e.g., "info", "warning"), value is the source color.
    /// </summary>
    public Dictionary<string, ColorName> AdditionalMappings { get; init; } = [];

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme)
    {
        var primary = ColorPaletteGenerator.GenerateFromHue(PrimaryHue);
        var accentHue = ColorSchemeGenerator(PrimaryHue);
        var accent = ColorPaletteGenerator.GenerateFromHue(accentHue);

        theme = theme.AddColorPalette("primary", primary)
             .AddColorPalette("accent", accent)
             .MapColorPalette(BaseColorName.Value, "base");

        foreach (var (slot, color) in AdditionalMappings)
            theme = theme.MapColorPalette(color.Value, slot);

        return theme;
    }
}

/// <summary>
/// A color scheme that uses named Tailwind colors.
/// </summary>
public class NamedColorScheme : IColorScheme
{
    /// <summary>
    /// Gets or sets the color name to map to "primary".
    /// </summary>
    public required ColorName PrimaryColorName { get; init; }

    /// <summary>
    /// Gets or sets the color name to map to "accent".
    /// </summary>
    public required ColorName AccentColorName { get; init; }

    /// <summary>
    /// Gets or sets the color name to map to "base".
    /// </summary>
    public required ColorName BaseColorName { get; init; }

    /// <summary>
    /// Gets or sets additional color mappings beyond the core slots.
    /// Key is the target slot name (e.g., "info", "warning"), value is the source color.
    /// </summary>
    public Dictionary<string, ColorName> AdditionalMappings { get; init; } = [];

    /// <inheritdoc />
    public Theme ApplyToTheme(Theme theme)
    {
        theme = theme.MapColorPalette(PrimaryColorName.Value, "primary")
             .MapColorPalette(AccentColorName.Value, "accent")
             .MapColorPalette(BaseColorName.Value, "base");

        foreach (var (slot, color) in AdditionalMappings)
            theme = theme.MapColorPalette(color.Value, slot);

        return theme;
    }
}
