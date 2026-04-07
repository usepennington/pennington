---
title: "MonorailCSS Configuration"
description: "Reference for configuring MonorailCSS color schemes, fonts, extra styles, and CSS class collection in Penn"
uid: "penn.reference.monorail-css-configuration"
order: 4020
---

[MonorailCSS](https://github.com/monorailcss/MonorailCss.Framework) gives Penn sites a Tailwind-like utility CSS framework without requiring a Node.js build step. The `Penn.MonorailCss` package handles colour palette generation, syntax highlighting themes, and stylesheet output from C# configuration.

For a guided walkthrough, see [Configure Custom Styling](xref:penn.guides.configure-custom-styling). For production build considerations, see [Optimizing CSS and JavaScript](xref:penn.guides.optimizing-css-and-javascript).

## Registration

Register services with `AddMonorailCss`, then activate the stylesheet endpoint with `UseMonorailCss`:

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

When called without a factory delegate, `AddMonorailCss` registers a default `MonorailCssOptions` instance as a singleton. When called with a factory, the options are registered as transient so they can resolve other services.

### Services Registered

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `MonorailCssOptions` | Singleton (no factory) or Transient (with factory) | Configuration options |
| `CssClassCollector` | Singleton | Collects CSS class names from rendered HTML responses |
| `MonorailCssService` | Transient | Generates the final stylesheet from collected classes and options |
| `CssClassCollectorProcessor` | Singleton | `IResponseProcessor` that scans HTML and JSON responses for `class` attributes |

`UseMonorailCss` does two things:

1. Maps a GET endpoint at the specified path (default `/styles.css`) that returns the generated stylesheet with `text/css` content type.
2. If `ContentPaths` are configured, scans those files for CSS class usage at startup.

CSS class collection from rendered pages happens automatically via `CssClassCollectorProcessor`, which is registered as an `IResponseProcessor` and runs inside the unified `ResponseProcessingMiddleware`.

## MonorailCssOptions

```csharp:xmldocid
T:Penn.MonorailCss.MonorailCssOptions
```

### ColorScheme

- **Type**: `IColorScheme`
- **Default**: `NamedColorScheme` with Blue / Purple / Cyan / Pink / Slate

Penn themes use five semantic colour roles. All built-in component styles reference these roles, so changing the colour scheme automatically updates syntax highlighting, alerts, code blocks, and prose styling.

| Role | CSS prefix | Usage |
|------|-----------|-------|
| `primary` | `primary-{shade}` | Main theme colour — links, headings, active states, highlighted code lines |
| `accent` | `accent-{shade}` | Complementary colour — function names in syntax highlighting, tab active states |
| `tertiary-one` | `tertiary-one-{shade}` | Strings, numbers, and regex in syntax highlighting |
| `tertiary-two` | `tertiary-two-{shade}` | Variables, attributes, and symbols in syntax highlighting |
| `base` | `base-{shade}` | Neutral greys — body text, borders, backgrounds, comments |

Two implementations of `IColorScheme` ship with Penn: `NamedColorScheme` and `AlgorithmicColorScheme`.

### ExtraStyles

- **Type**: `string`
- **Default**: `""`

Raw CSS prepended to the generated stylesheet. Use this for `@font-face` declarations, CSS custom properties, or styles that do not map to utility classes:

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

File paths relative to the web root that are scanned for CSS class usage at startup. This solves the same problem as Tailwind's `content` configuration: if CSS classes only appear in client-side JavaScript or other non-HTML files, the `CssClassCollectorProcessor` will not encounter them during HTML response processing.

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["js/spa-engine.js", "js/search.js"]
});
```

The scanner uses two extraction strategies. First, it matches `class="..."` attributes. Second, it splits the entire file on delimiter characters and treats every token as a potential class. False positives are harmless because MonorailCSS ignores tokens it does not recognise as utility classes.

### CustomCssFrameworkSettings

- **Type**: `Func<CssFrameworkSettings, CssFrameworkSettings>`
- **Default**: `settings => settings` (identity, no changes)

Provides access to the fully-configured `CssFrameworkSettings` object, including the theme, all built-in component applies, and prose customisation. You receive the settings after Penn has applied all defaults and return a modified copy. See the [Overriding Built-in Styles](#overriding-built-in-styles) section for usage patterns.

## NamedColorScheme

Maps named Tailwind colour palettes to Penn's five semantic roles. Each shade (50 through 950) of the source palette becomes the corresponding shade of the target role.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `PrimaryColorName` | `string` | Yes | Colour name mapped to `primary` |
| `AccentColorName` | `string` | Yes | Colour name mapped to `accent` |
| `TertiaryOneColorName` | `string` | Yes | Colour name mapped to `tertiary-one` |
| `TertiaryTwoColorName` | `string` | Yes | Colour name mapped to `tertiary-two` |
| `BaseColorName` | `string` | Yes | Colour name mapped to `base` |

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

**Available colour names** (from `ColorNames`): `Blue`, `Purple`, `Cyan`, `Pink`, `Slate`, `Gray`, `Zinc`, `Neutral`, `Stone`, `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Sky`, `Indigo`, `Violet`, `Fuchsia`, `Rose`.

## AlgorithmicColorScheme

Generates full colour palettes from a single hue value using OKLCH colour space. You provide a primary hue (0-360 degrees) and Penn generates primary, accent, and tertiary palettes algorithmically.

```csharp:xmldocid
T:Penn.MonorailCss.AlgorithmicColorScheme
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `PrimaryHue` | `int` | Yes | -- | Primary hue in degrees (0-360) |
| `BaseColorName` | `string` | No | `"Gray"` | Named Tailwind palette for the base/neutral role |
| `ColorSchemeGenerator` | `Func<int, (int, int, int)>` | No | Complementary + right/left angles | Function that derives accent and tertiary hues from the primary hue |

### OKLCH Palette Generation

The `ColorPaletteGenerator` creates an 11-shade palette (50 through 950) for each hue using the OKLCH colour space:

- **Lightness** follows a curve from 97.1% (shade 50) down to 25.8% (shade 950). Per-hue adjustments boost yellows and greens in the middle range to avoid muddy mid-tones.
- **Chroma** follows a Gaussian distribution peaking around shades 500-600 (the most saturated swatches). Per-hue multipliers reduce chroma for yellows to prevent oversaturation.
- **Hue** shifts subtly across the palette for more vibrant darks. Blues shift toward violet, reds toward orange, and yellows shift strongly toward orange in darker shades. This matches how Tailwind v4's built-in palettes behave.

Smoothing factors at the extremes (shades 50, 100, and 950) reduce chroma slightly to prevent over-desaturation at the edges of the palette.

### Default Colour Scheme Generation

By default, `ColorSchemeGenerator` derives the accent and tertiary hues using geometric relationships:

| Role | Default Hue | Relationship to Primary |
|------|-------------|------------------------|
| Primary | Input hue | -- |
| Accent | Primary + 180 | Complementary |
| Tertiary One | Primary + 90 | Right angle |
| Tertiary Two | Primary - 90 | Left angle |

### Custom Colour Scheme Generator

Override `ColorSchemeGenerator` to use a different colour harmony:

```csharp
new AlgorithmicColorScheme
{
    PrimaryHue = 220,  // Blue
    ColorSchemeGenerator = primary => (
        primary + 120,  // Triadic accent
        primary + 60,   // Analogous tertiary one
        primary - 30    // Analogous tertiary two
    )
}
```

The function receives the primary hue and returns a tuple of `(accentHue, tertiaryOneHue, tertiaryTwoHue)`. All hue values are normalised to the 0-360 range internally.

### Full Configuration Example

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

`MonorailCssService` registers utility-class rules for several built-in components via the `Applies` dictionary on `CssFrameworkSettings`. These rules are generated automatically and use the semantic colour roles, so they adapt to your chosen colour scheme. All of them can be overridden with `CustomCssFrameworkSettings`.

### Code Blocks

Styles for the code highlighting wrapper (`.code-highlight-wrapper`) and its children:

- **Container**: `.standalone-code-container` -- white/dark background, border, rounded corners, overflow handling.
- **Pre element**: padding, monospace font, responsive text size, light/dark text colours.
- **Line containers**: `.line` -- inline-block with transitions for highlight and focus effects. Lines span the full width with negative margin for edge-to-edge highlighting.
- **Line highlighting**: `.line.highlight` uses `primary` with opacity for highlighted lines.
- **Diff notation**: `.line.diff-add` (green background, `+` pseudo-element) and `.line.diff-remove` (red background, `-` pseudo-element, reduced opacity on children).
- **Focus/blur**: `.has-focused .line` applies a subtle blur and reduced opacity; `.line.focused` restores clarity. Hovering the code block also restores all lines.
- **Error and warning**: `.line.error` (red) and `.line.warning` (amber) backgrounds.
- **Word highlights**: `.word-highlight` for inline highlighting with primary-coloured border; `.word-highlight-with-message` and `.word-highlight-message` for annotated highlights with a positioned tooltip.

### Tab Components

Styles for Penn's markdown tab extension:

- `.tab-container` -- flex column layout with background, border, and rounded corners.
- `.tab-list` -- horizontal flex row with wrapping and background tint.
- `.tab-button` -- text styling with transparent bottom border; selected state uses `accent` colour for text and border. Dark mode variants included.
- `.tab-panel` -- hidden by default, displayed when `data-selected="true"`.

### Markdown Alerts

GitHub-flavoured alert blocks, each with a distinct colour:

| Alert type | CSS class | Colour |
|-----------|-----------|--------|
| Note | `.markdown-alert-note` | Emerald |
| Tip | `.markdown-alert-tip` | Blue |
| Caution | `.markdown-alert-caution` | Amber |
| Warning | `.markdown-alert-warning` | Rose |
| Important | `.markdown-alert-important` | Sky |

Each alert type receives fill, background, border, and text colours in both light and dark modes. The base `.markdown-alert` class provides shared layout (flex row, gap, rounded corners, border, text size).

### Syntax Highlighting

A complete highlight.js compatible theme using the five semantic colour roles:

| Token type | CSS class(es) | Colour role |
|-----------|---------------|-------------|
| Base text | `.hljs` | Base 900 / 200 |
| Comments | `.hljs-comment` | Base (muted, italic) |
| Quotes | `.hljs-quote` | Base (italic) |
| Keywords, selectors, literals | `.hljs-keyword`, `.hljs-selector-tag`, `.hljs-literal` | Primary |
| Types | `.hljs-type` | Base |
| Strings, numbers, regex | `.hljs-string`, `.hljs-number`, `.hljs-regexp` | Tertiary One |
| Functions, titles, params | `.hljs-function`, `.hljs-title`, `.hljs-params` | Accent |
| Variables, names, attributes, symbols | `.hljs-variable`, `.hljs-name`, `.hljs-attr`, `.hljs-symbol` | Tertiary Two |
| Operators, punctuation | `.hljs-operator`, `.hljs-punctuation` | Base |
| Built-ins | `.hljs-built_in` | Accent |
| Additions | `.hljs-addition` | Green |
| Deletions | `.hljs-deletion` | Red |
| Links | `.hljs-link` | Blue |

All token styles include both light and dark mode variants (e.g., `text-primary-800 dark:text-primary-300`).

### Prose Customisation

MonorailCSS's prose plugin receives custom rules for several elements:

- **Links** (`a`): bold, no underline, 1px bottom border using `primary-500` at 75% opacity.
- **Blockquotes**: 4px left border in `primary-700`.
- **Pre blocks**: subtle `base-200` background at 50% opacity with an inset box shadow and rounded corners.
- **Inline code** (`:not(pre) > code`): padding, rounded pill shape, `base-200` background, `base-700` text colour, `word-break: break-word`.
- **Code font weight**: 400 (regular) for all code elements.
- **Size variants** (`base`, `sm`): `pre > code` inherits font size; inline code uses `0.8em`.
- **Invert variant**: pre blocks and inline code switch to `base-800` background at 75% opacity with lighter text.

### Search Modal

Styles for the built-in search modal including the backdrop, input field, result items, and search term highlighting. These use `base` and `primary` colour roles with dark mode variants.

## Overriding Built-in Styles

Use `CustomCssFrameworkSettings` to modify, replace, or extend any built-in style. The `Applies` property is an `ImmutableDictionary<string, string>` where keys are CSS selectors and values are space-separated utility classes.

### Add vs SetItem

- **`Add`** inserts a new entry. It throws if the key already exists. Use this for new selectors that do not conflict with built-in styles.
- **`SetItem`** inserts or replaces. If the key already exists, the value is overwritten. Use this to override a built-in style.

```csharp
services.AddMonorailCss(_ => new MonorailCssOptions
{
    CustomCssFrameworkSettings = settings => settings with
    {
        Applies = settings.Applies
            // Override an existing built-in style (replaces the value)
            .SetItem(".hljs-keyword", "text-accent-700 dark:text-accent-300")
            // Add a new custom style (throws if selector already exists)
            .Add(".my-custom-card", "bg-white shadow-lg rounded-xl p-6 dark:bg-base-900")
    }
});
```

You can also replace the `Theme` or `ProseCustomization` through the same mechanism:

```csharp
CustomCssFrameworkSettings = settings => settings with
{
    Theme = settings.Theme.AddColorPalette("brand", myCustomPalette),
    Applies = settings.Applies
        .SetItem(".hljs-deletion", "text-amber-700 dark:text-amber-300")
}
```

Because `CustomCssFrameworkSettings` receives the settings after all built-in styles have been applied, you always have access to the full set of defaults and can selectively override only what you need.
