---
title: "MonorailCSS Configuration"
description: "Complete reference for configuring MonorailCSS options, color schemes, and styling in MyLittleContentEngine"
uid: "docs.reference.monorail-css-configuration"
order: 4020
---

[MonorailCSS](https://github.com/monorailcss/MonorailCss.Framework) provides a Tailwind-like CSS framework specifically designed for MyLittleContentEngine. This reference covers all configuration options, built-in styles, and customization capabilities.

## MonorailCssOptions

The `MonorailCssOptions` class provides the primary configuration interface for customizing the CSS framework:

```csharp
public class MonorailCssOptions
{
    public IColorScheme ColorScheme { get; init; } = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Blue,
        AccentColorName = ColorNames.Purple,
        TertiaryOneColorName = ColorNames.Cyan,
        TertiaryTwoColorName = ColorNames.Pink,
        BaseColorName = ColorNames.Slate
    };
    public Func<CssFrameworkSettings, CssFrameworkSettings> CustomCssFrameworkSettings { get; init; } =
        settings => settings;
}
```

### Configuration Properties

#### ColorScheme
- **Type**: `IColorScheme`
- **Default**: `NamedColorScheme` with Blue, Purple, Cyan, Pink, and Slate
- **Purpose**: Defines the color palette for your site theme

There are two built-in color scheme implementations:

##### NamedColorScheme
Uses named Tailwind color palettes. Requires specifying all five color roles:

**Example Usage:**
```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Blue,      // Main theme color
        AccentColorName = ColorNames.Purple,     // Complementary accent
        TertiaryOneColorName = ColorNames.Cyan,  // Syntax highlighting
        TertiaryTwoColorName = ColorNames.Pink,  // Syntax highlighting
        BaseColorName = ColorNames.Slate         // Neutral colors
    }
});
```

**Available Color Names:**
`Blue`, `Purple`, `Cyan`, `Pink`, `Slate`, `Gray`, `Zinc`, `Neutral`, `Stone`, `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Sky`, `Indigo`, `Violet`, `Fuchsia`, `Rose`

##### AlgorithmicColorScheme
Generates color palettes algorithmically from hue values (0-360):

**Example Usage:**
```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 200,                        // Cyan/teal theme
        BaseColorName = ColorNames.Slate,        // Neutral base
        ColorSchemeGenerator = primary => (      // Optional customization
            primary + 120,  // Triadic accent
            primary + 60,   // Analogous tertiary one
            primary - 60    // Analogous tertiary two
        )
    }
});
```

**Default Color Generation:**
- **Accent**: Primary + 180° (complementary color)
- **Tertiary One**: Primary + 90° (for syntax highlighting)
- **Tertiary Two**: Primary - 90° (for syntax highlighting)

#### CustomCssFrameworkSettings
- **Type**: `Func<CssFrameworkSettings, CssFrameworkSettings>`
- **Default**: `settings => settings` (no modification)
- **Purpose**: Allows deep customization of the underlying CSS framework

**Example Usage:**
```csharp
builder.Services.AddMonorailCss(options => new MonorailCssOptions
{
    CustomCssFrameworkSettings = settings => settings with
    {
        // Add custom utility classes or modify existing ones
        Applies = settings.Applies.Add(".my-custom-class", "bg-primary-500 text-white p-4")
    }
});
```

## Built-in Component Styles

MonorailCSS includes pre-configured styles for common components. You can modify these using the `CustomCssFrameworkSettings` option.

```csharp
builder.Services.AddMonorailCss(_ =>
{
    return new MonorailCssOptions
    {
        CustomCssFrameworkSettings = defaultSettings => defaultSettings with
        {
            Applies = defaultSettings.Applies.SetItem(".hljs-deletion", "text-amber-700 dark:text-amber-300")
        }
    };
});
```

### Tab Components
```csharp:xmldocid,bodyonly
M:MyLittleContentEngine.MonorailCss.MonorailCssService.TabApplies
```

### Code Highlighting
```csharp:xmldocid,bodyonly
M:MyLittleContentEngine.MonorailCss.MonorailCssService.CodeBlockApplies
```

### Markdown Alert Blocks
```csharp:xmldocid,bodyonly
M:MyLittleContentEngine.MonorailCss.MonorailCssService.MarkdownAlertApplies
```

## Syntax Highlighting

MonorailCSS provides a complete syntax highlighting theme using the generated color palettes:

### Color Mapping
- **Comments**: Base colors with reduced opacity, italic
- **Keywords**: Primary color palette
- **Strings/Numbers**: Tertiary-one color palette
- **Functions**: Accent color palette
- **Variables**: Tertiary-two color palette
- **Operators**: Base colors

### Configuration
```csharp:xmldocid,bodyonly
M:MyLittleContentEngine.MonorailCss.MonorailCssService.HljsApplies
```
