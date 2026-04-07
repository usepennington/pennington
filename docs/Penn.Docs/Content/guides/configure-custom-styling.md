---
title: "Configure Custom Styling"
description: "Customize MonorailCSS colors, themes, fonts, and styles in your Penn site"
uid: "penn.guides.configure-custom-styling"
order: 2050
---

Penn uses [MonorailCSS](https://github.com/nickyoungblood/monorailcss) for its utility-first CSS. MonorailCSS is Tailwind-compatible, runs entirely at runtime in .NET, and requires no Node.js toolchain. It scans your rendered HTML to discover which utility classes are in use, then generates only those styles. No build step, no `node_modules`, no purge configuration.

Penn does not require MonorailCSS. You can use Tailwind, plain CSS, or any other approach. However, Penn's built-in UI components (`Penn.UI`) emit Tailwind-compatible utility classes, so a compatible CSS framework must be available for them to render correctly.

> [!NOTE]
> For the full API reference, see [MonorailCSS Configuration](xref:penn.reference.monorail-css-configuration).

## Registering MonorailCSS

Install the package:

```bash
dotnet add package Penn.MonorailCss
```

Register services and middleware in `Program.cs`:

```csharp
builder.Services.AddMonorailCss();

var app = builder.Build();
app.UsePenn();
app.UseMonorailCss();
```

`AddMonorailCss()` registers the `CssClassCollector`, `MonorailCssService`, and the response processor that scans HTML for class names. `UseMonorailCss()` maps a `GET /styles.css` endpoint that returns the generated stylesheet.

Link the stylesheet in your layout:

```razor
@inject LinkService LinkService

<link rel="stylesheet" href="@LinkService.GetLink("styles.css")" />
```

If you use `Penn.DocSite`, this wiring is handled automatically by `AddDocSite()` and `UseDocSite()`. See [Using the DocSite Package](xref:penn.guides.using-docsite) for details.

## Understanding the Color System

Penn maps colors to five semantic roles. Each role produces a full Tailwind-compatible scale from `50` (lightest) through `950` (darkest):

| Role | Purpose | Default |
|------|---------|---------|
| `primary` | Brand color. Links, buttons, emphasis. | Blue |
| `accent` | Complementary color. Secondary actions, highlights. | Purple |
| `tertiary-one` | Syntax highlighting (strings, numbers, literals). | Cyan |
| `tertiary-two` | Syntax highlighting (variables, attributes). | Pink |
| `base` | Neutral. Backgrounds, text, borders. | Slate |

Use these roles in your templates with standard Tailwind utility syntax:

```razor
<div class="bg-primary-600 text-primary-50">
<p class="text-base-800 dark:text-base-200">
<button class="bg-accent-500 hover:bg-accent-600">
```

The scale values follow Tailwind conventions: `50` and `100` are near-white, `500` is the mid-tone, and `900`/`950` are near-black.

## Named Color Scheme

The simplest way to customize colors is to assign existing Tailwind palette names to each role:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Emerald,
        AccentColorName = ColorNames.Teal,
        TertiaryOneColorName = ColorNames.Sky,
        TertiaryTwoColorName = ColorNames.Violet,
        BaseColorName = ColorNames.Zinc
    }
});
```

This maps the full 50-950 range of each Tailwind palette directly to the corresponding semantic role. All standard Tailwind color names are available:

- **Neutrals**: `Gray`, `Slate`, `Zinc`, `Neutral`, `Stone`
- **Colors**: `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Cyan`, `Sky`, `Blue`, `Indigo`, `Violet`, `Purple`, `Fuchsia`, `Pink`, `Rose`

## Algorithmic Color Scheme

For palette generation from a single value, use `AlgorithmicColorScheme`. It takes a hue (0-360 degrees) and generates a full 50-950 OKLCH color palette using `ColorPaletteGenerator.GenerateFromHue()`:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 230,
        BaseColorName = ColorNames.Zinc
    }
});
```

The `PrimaryHue` value maps to the color wheel: 0 is red, 120 is green, 240 is blue.

### The ColorSchemeGenerator Function

By default, accent and tertiary hues are derived from the primary hue using complementary and split-complementary offsets:

- Accent: `primary + 180` (complementary)
- Tertiary one: `primary + 90`
- Tertiary two: `primary - 90`

Override this with the `ColorSchemeGenerator` property:

```csharp
ColorScheme = new AlgorithmicColorScheme
{
    PrimaryHue = 205,
    BaseColorName = ColorNames.Slate,
    ColorSchemeGenerator = primary => (
        primary + 155,  // Accent hue
        primary + 45,   // Tertiary one hue
        primary - 30    // Tertiary two hue
    )
}
```

The function receives the primary hue as input and returns a tuple of `(accentHue, tertiaryOneHue, tertiaryTwoHue)`. Hue values outside 0-360 are normalized automatically.

### How Palette Generation Works

`ColorPaletteGenerator` builds each shade using the OKLCH color space. It applies per-shade lightness and chroma values calibrated to match Tailwind v4 palettes, with hue-specific adjustments:

- Yellows and greens receive lightness boosts in mid-tones (shades 300-600) to avoid appearing muddy.
- Blues shift slightly toward violet in darker shades for vibrancy.
- Chroma follows a Gaussian distribution, peaking at shades 500-600.

The base color (`BaseColorName`) always uses an existing Tailwind neutral palette rather than generating one algorithmically.

## Dark/Light Theme Switching

Penn supports dark mode through Tailwind's `dark:` variant prefix, controlled by a `dark` class on `<html>`.

### Preventing Flash of Unstyled Content (FOUC)

Add this inline script in the `<head>` of your HTML, before any stylesheets:

```html
<script>
    const isDarkMode = localStorage.theme === "dark" ||
        (!("theme" in localStorage) &&
         window.matchMedia("(prefers-color-scheme: dark)").matches);
    document.documentElement.classList.toggle("dark", isDarkMode);
    document.documentElement.dataset.theme = isDarkMode ? "dark" : "light";
</script>
```

This script must be inline in `<head>`, not in an external file. If it loads after the page renders, users see a flash of the wrong theme.

### Theme Toggle Button

Add a button with the `data-theme-toggle` attribute. Penn's `ThemeManager` (in `scripts.js`) finds all elements with this attribute at initialization and wires up click handlers automatically:

```razor
<button aria-label="Toggle Dark Mode" data-theme-toggle>
    <svg class="dark:hidden" viewBox="0 0 24 24"><!-- sun icon --></svg>
    <svg class="hidden dark:block" viewBox="0 0 24 24"><!-- moon icon --></svg>
</button>
```

Clicking the button toggles the `dark` class on `document.documentElement` and persists the preference to `localStorage.theme`.

### Using Dark Mode Classes

All MonorailCSS utilities support the `dark:` prefix:

```razor
<div class="bg-white dark:bg-base-900 text-base-900 dark:text-base-100">
<button class="bg-primary-600 hover:bg-primary-700
               dark:bg-primary-500 dark:hover:bg-primary-400">
```

## Custom Fonts

Override font families through the `CustomCssFrameworkSettings` callback:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    CustomCssFrameworkSettings = defaultSettings => defaultSettings with
    {
        DesignSystem = defaultSettings.DesignSystem with
        {
            FontFamilies = defaultSettings.DesignSystem.FontFamilies
                .Add("display", new FontFamilyDefinition("Lexend, sans-serif"))
                .SetItem("mono", new FontFamilyDefinition("""
                    "Cascadia Code", ui-monospace, SFMono-Regular,
                    Menlo, Monaco, Consolas, "Liberation Mono",
                    "Courier New", monospace
                    """))
        }
    }
});
```

The `CustomCssFrameworkSettings` function receives the fully configured `CssFrameworkSettings` (with Penn's color scheme, prose customization, and component styles already applied) and returns a modified version. Use `with` expressions to override individual properties while preserving everything else.

Use `.Add()` to introduce new font family keys (e.g., `font-display`) and `.SetItem()` to replace existing ones (e.g., override the default `mono` stack).

## Custom CSS Classes with Applies

The `Applies` dictionary maps CSS selectors to Tailwind utility class strings. MonorailCSS expands these into real CSS rules:

```csharp
CustomCssFrameworkSettings = defaultSettings => defaultSettings with
{
    Applies = defaultSettings.Applies
        .Add(".my-card", "bg-base-100 border border-base-300 rounded-lg p-4 shadow-sm dark:bg-base-900 dark:border-base-700")
        .Add(".btn-primary", "bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md transition-colors")
}
```

> [!WARNING]
> Merge your custom applies with `defaultSettings.Applies` as shown above. If you create a new dictionary instead, you replace Penn's default applies for code blocks, tabs, alerts, syntax highlighting, and search. Those defaults are what style the built-in components.

To replace the defaults entirely (if you are providing your own component styles):

```csharp
Applies = new Dictionary<string, string>
{
    { ".my-selector", "bg-white p-4 rounded" }
}.ToImmutableDictionary()
```

## Extra Styles

For raw CSS that does not fit the utility model, use `ExtraStyles`. This content is prepended to the generated stylesheet:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ExtraStyles = """
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');

        .custom-gradient {
            background: linear-gradient(135deg, var(--color-primary-500), var(--color-accent-500));
        }
        """
});
```

Use `ExtraStyles` for font imports, CSS custom properties, keyframe animations, and any rules that require standard CSS syntax.

## Content Paths

MonorailCSS discovers utility classes by scanning server-rendered HTML responses. Classes that appear only in client-side JavaScript files are not visible to this process. The `ContentPaths` property tells MonorailCSS to scan additional files at startup:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["wwwroot/js/app.js", "wwwroot/js/components.js"]
});
```

Paths are relative to the web root. MonorailCSS extracts potential class names using both `class="..."` attribute parsing and broad token splitting. False positives are harmless -- MonorailCSS ignores tokens it does not recognize as utility classes.

## Complete Example

This example combines an algorithmic color scheme, custom fonts, extra styles, and content path scanning:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 205,
        BaseColorName = ColorNames.Slate,
        ColorSchemeGenerator = primary => (
            primary + 155,  // Green accent
            primary + 45,   // Purple tertiary
            primary - 30    // Orange tertiary
        )
    },

    CustomCssFrameworkSettings = defaultSettings => defaultSettings with
    {
        DesignSystem = defaultSettings.DesignSystem with
        {
            FontFamilies = defaultSettings.DesignSystem.FontFamilies
                .Add("brand", new FontFamilyDefinition("Inter, sans-serif"))
                .SetItem("mono", new FontFamilyDefinition("'JetBrains Mono', monospace"))
        },
        Applies = defaultSettings.Applies
            .Add(".btn-primary", "bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md")
    },

    ExtraStyles = """
        @import url('https://fonts.googleapis.com/css2?family=Inter&family=JetBrains+Mono&display=swap');
        """,

    ContentPaths = ["js/search-ui.js"]
});
```

## Troubleshooting

**No styles appear.** Verify that the `<link>` tag uses `LinkService.GetLink("styles.css")` and that `UseMonorailCss()` is called in the middleware pipeline.

**Theme toggle does not work.** Check that the toggle button has the `data-theme-toggle` attribute and that Penn's `scripts.js` is loaded on the page.

**Classes from JavaScript files are missing.** Add the file paths to `ContentPaths`. Paths are relative to `wwwroot`.

**Algorithmic colors look wrong.** Verify your hue value. Hue 0 is red, 60 is yellow, 120 is green, 180 is cyan, 240 is blue, 300 is magenta.

**Custom applies removed built-in styles.** Make sure you are merging with `defaultSettings.Applies` using `.Add()` rather than creating a new dictionary. See the [Custom CSS Classes with Applies](#custom-css-classes-with-applies) section.

**FOUC on page load.** The inline theme detection script must be in `<head>` before any `<link>` or `<style>` elements. If it is in an external file or at the end of `<body>`, the browser renders the page before the script runs.
