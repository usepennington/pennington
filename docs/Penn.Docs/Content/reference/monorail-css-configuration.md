---
title: "MonorailCSS Configuration"
description: "Reference for configuring MonorailCSS color schemes, fonts, extra styles, and CSS class collection in Penn"
uid: "penn.reference.monorail-css-configuration"
order: 4020
---

[MonorailCSS](https://github.com/monorailcss/MonorailCss.Framework) gives Penn sites a Tailwind-like utility CSS framework without requiring a Node.js build step. The `Penn.MonorailCss` package handles colour palette generation, syntax highlighting themes, and stylesheet output — all from C# configuration. No `tailwind.config.js` required, for better or worse.

## Registration

```csharp
// In your service configuration:
services.AddMonorailCss(sp => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Blue,
        AccentColorName = ColorNames.Purple,
        TertiaryOneColorName = ColorNames.Cyan,
        TertiaryTwoColorName = ColorNames.Pink,
        BaseColorName = ColorNames.Slate
    }
});

// In your middleware pipeline:
app.UseMonorailCss();           // serves at /styles.css by default
app.UseMonorailCss("/css/app.css");  // or at a custom path
```

`AddMonorailCss` registers:

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `MonorailCssOptions` | Singleton/Transient | Configuration options |
| `CssClassCollector` | Singleton | Collects CSS classes from rendered HTML |
| `MonorailCssService` | Transient | Generates the stylesheet from collected classes |
| `CssClassCollectorProcessor` | Singleton | `IResponseProcessor` that scans HTML responses |

`UseMonorailCss` maps a GET endpoint that returns the generated stylesheet and, if `ContentPaths` are configured, scans those files for CSS class usage at startup.

## MonorailCssOptions

```csharp:xmldocid
T:Penn.MonorailCss.MonorailCssOptions
```

### ColorScheme

- **Type**: `IColorScheme`
- **Default**: `NamedColorScheme` with Blue / Purple / Cyan / Pink / Slate

Penn themes use five semantic colour roles:

| Role | Usage |
|------|-------|
| `primary` | Main theme colour — links, headings, active states |
| `accent` | Complementary colour — function names in syntax highlighting, hover states |
| `tertiary-one` | Strings and numbers in syntax highlighting |
| `tertiary-two` | Variables and attributes in syntax highlighting |
| `base` | Neutral greys — body text, borders, backgrounds |

Two implementations of `IColorScheme` ship with Penn.

### ExtraStyles

- **Type**: `string`
- **Default**: `""`

Raw CSS prepended to the generated stylesheet. Useful for `@font-face` declarations, CSS custom properties, or styles that don't map to utility classes:

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    ExtraStyles = """
        @font-face {
            font-family: 'Berkeley Mono';
            src: url('/fonts/BerkeleyMono-Regular.woff2') format('woff2');
            font-weight: 400;
            font-display: swap;
        }

        :root {
            --font-mono: 'Berkeley Mono', ui-monospace, monospace;
        }
        """
});
```

### ContentPaths

- **Type**: `string[]`
- **Default**: `[]`

File paths relative to the web root that should be scanned for CSS class usage at startup. This solves the same problem as Tailwind's `content` configuration — if CSS classes only appear in client-side JavaScript or other non-HTML files, the collector won't see them during HTML rendering.

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["js/spa-engine.js", "js/search.js"]
});
```

The scanner extracts potential class names broadly (splitting on whitespace and delimiters). False positives are harmless — MonorailCSS ignores tokens it doesn't recognise.

### CustomCssFrameworkSettings

- **Type**: `Func<CssFrameworkSettings, CssFrameworkSettings>`
- **Default**: `settings => settings`

Allows deep customisation of the underlying MonorailCSS framework. You receive the fully-configured settings (including all built-in component styles) and return a modified copy:

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    CustomCssFrameworkSettings = settings => settings with
    {
        Applies = settings.Applies
            .Add(".my-custom-card", "bg-white shadow-lg rounded-xl p-6 dark:bg-base-900")
            .SetItem(".hljs-keyword", "text-accent-700 dark:text-accent-300")
    }
});
```

## NamedColorScheme

Maps named Tailwind colour palettes to Penn's semantic roles. Each shade (50 through 950) of the source palette becomes the corresponding shade of the role.

```csharp
new NamedColorScheme
{
    PrimaryColorName = ColorNames.Indigo,
    AccentColorName = ColorNames.Violet,
    TertiaryOneColorName = ColorNames.Teal,
    TertiaryTwoColorName = ColorNames.Rose,
    BaseColorName = ColorNames.Zinc
}
```

**Available colour names:** `Blue`, `Purple`, `Cyan`, `Pink`, `Slate`, `Gray`, `Zinc`, `Neutral`, `Stone`, `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Sky`, `Indigo`, `Violet`, `Fuchsia`, `Rose`

## AlgorithmicColorScheme

Generates full colour palettes from a single hue value using OKLCH colour space. You provide a hue (0-360 degrees) and Penn generates primary, accent, and tertiary palettes algorithmically.

```csharp:xmldocid
T:Penn.MonorailCss.AlgorithmicColorScheme
```

### How It Works

The `ColorPaletteGenerator` creates an 11-shade palette (50 through 950) for each hue using OKLCH:

- **Lightness** follows a curve from 97.1% (shade 50) down to 25.8% (shade 950), with per-hue adjustments — yellows and greens get boosted lightness in the middle range to avoid muddy mid-tones.
- **Chroma** follows a Gaussian distribution that peaks around shades 500-600 (the most saturated swatches), with per-hue multipliers — yellows get reduced chroma to prevent oversaturation.
- **Hue** shifts subtly across the palette for more vibrant darks. Blues shift toward violet, reds toward orange, and yellows shift strongly toward orange in darker shades. This matches how Tailwind v4's palettes behave.

### Default Colour Scheme Generation

By default, the accent and tertiary hues are derived from the primary hue:

| Role | Default Hue | Relationship |
|------|-------------|-------------|
| Primary | Your input | — |
| Accent | Primary + 180 | Complementary |
| Tertiary One | Primary + 90 | Right angle |
| Tertiary Two | Primary - 90 | Left angle |

### Custom Colour Scheme Generator

Override `ColorSchemeGenerator` to use a different strategy:

```csharp
new AlgorithmicColorScheme
{
    PrimaryHue = 220,  // Blue
    ColorSchemeGenerator = primary => (
        primary + 120,  // Triadic accent (green-ish)
        primary + 60,   // Analogous tertiary one
        primary - 30    // Analogous tertiary two
    )
}
```

The function receives the primary hue and returns a tuple of `(accentHue, tertiaryOneHue, tertiaryTwoHue)`.

### Example: Full Configuration

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 262,                    // Purple
        BaseColorName = ColorNames.Slate,    // Neutral base
        ColorSchemeGenerator = primary => (
            primary - 120,   // Warm accent
            primary + 60,    // Cool tertiary
            primary - 60     // Warm tertiary
        )
    },
    ExtraStyles = """
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
        :root { --font-sans: 'Inter', system-ui, sans-serif; }
        """,
    ContentPaths = ["js/app.js"]
});
```

## Built-in Component Styles

`MonorailCssService` registers utility-class rules for several built-in component types via the `Applies` dictionary. These are generated automatically and can be overridden with `CustomCssFrameworkSettings`.

### Code Blocks

Styles for the code highlighting wrapper, line highlighting (`.highlight`, `.diff-add`, `.diff-remove`), focus/blur effects, word highlights with optional messages, and light/dark mode variants.

### Tab Components

Styles for `.tab-container`, `.tab-list`, `.tab-button`, and `.tab-panel` — used by Penn's markdown tab extension.

### Markdown Alerts

GitHub-flavoured alert blocks (`.markdown-alert-note`, `.markdown-alert-tip`, `.markdown-alert-caution`, `.markdown-alert-warning`, `.markdown-alert-important`) each with their own colour scheme.

### Syntax Highlighting

A complete highlight.js theme using the semantic colour roles:

| Token Type | Colour Role |
|-----------|-------------|
| Comments | Base (muted, italic) |
| Keywords, selectors | Primary |
| Strings, numbers, regex | Tertiary One |
| Functions, titles | Accent |
| Variables, attributes | Tertiary Two |
| Operators, punctuation | Base |

### Prose Customisation

MonorailCSS's prose plugin is configured with custom rules for links (no underline, coloured bottom border), blockquotes (primary-coloured left border), code blocks (subtle background with inset shadow), and inline code (rounded pill with base background). Dark mode inverts with appropriate contrast adjustments.

## Overriding Built-in Styles

Use `CustomCssFrameworkSettings` to modify or replace any built-in style:

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    CustomCssFrameworkSettings = settings => settings with
    {
        // Change the deletion highlight colour
        Applies = settings.Applies
            .SetItem(".hljs-deletion", "text-amber-700 dark:text-amber-300")
    }
});
```
