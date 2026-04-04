---
title: "Configure Custom Styling"
description: "Customize MonorailCSS colors, themes, fonts, and extra styles in your Penn site"
uid: "penn.guides.configure-custom-styling"
order: 2050
---

This guide covers customizing the visual appearance of your Penn site using MonorailCSS. MonorailCSS is a [TailwindCSS](https://tailwindcss.com/)-compatible utility-first CSS framework that aims for syntax compatibility with Tailwind while doing everything at runtime. No build step. No `node_modules`. No existential dread about your JavaScript toolchain.

Penn doesn't require MonorailCSS, but `Penn.MonorailCss` exists for a reason, and that reason is that Penn's default components assume Tailwind-compatible utility classes are available. Use MonorailCSS, use Tailwind, or write your own CSS. Penn is opinionated but not possessive.

> [!NOTE]
> For the full MonorailCSS configuration reference, see [Monorail CSS Configuration](xref:penn.reference.monorail-css-configuration).

## Prerequisites

Install the package:

```bash
dotnet add package Penn.MonorailCss
```

Register the services and middleware in `Program.cs`:

```csharp
builder.Services.AddMonorailCss();

var app = builder.Build();
app.UsePenn();
app.UseMonorailCss();
```

Link the generated stylesheet in your layout:

```razor
@inject LinkService LinkService

<link rel="stylesheet" href="@LinkService.GetLink("styles.css")" />
```

> [!NOTE]
> MonorailCSS works by scanning your rendered HTML at runtime to discover which CSS classes are in use, then generating only those styles. No purging step, no content configuration, no surprise missing classes in production. The tradeoff is runtime cost, which is negligible for content sites.

## Understanding MonorailCSS Colors

MonorailCSS uses a TailwindCSS-compatible color system with numbered scales (50-950) and semantic color roles. Penn's color system revolves around five roles:

- **primary** -- Your brand color. Used for links, buttons, and emphasis.
- **accent** -- Complementary color for highlights and secondary actions.
- **tertiary-one** and **tertiary-two** -- Additional accent colors, used primarily in syntax highlighting.
- **base** -- Neutral colors for backgrounds, text, and borders.

### Color Scale Structure

Like Tailwind, each color has a numerical scale:

- `50` -- Lightest shade
- `100-400` -- Light shades
- `500` -- Base/medium shade
- `600-800` -- Dark shades
- `900-950` -- Darkest shades

## Customizing Color Palettes

MonorailCSS provides two approaches, because sometimes you know exactly what color you want, and sometimes you just want "blue-ish."

### Named Color Scheme

The simplest approach. Pick Tailwind color names for each role:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
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
```

This maps existing Tailwind palettes to Penn's semantic roles. If you like Tailwind's blue, you get Tailwind's blue. All of it. Every shade.

### Algorithmic Color Scheme

For more control, generate palettes from a hue value (0-360 degrees). `AlgorithmicColorScheme` uses `ColorPaletteGenerator.GenerateFromHue()` to create a full 50-950 palette from a single number:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 230,                        // Blue-ish
        BaseColorName = ColorNames.Zinc,         // Neutral palette
        ColorSchemeGenerator = primary => (
            primary + 180,  // Accent hue (complementary)
            primary + 90,   // Tertiary one hue
            primary - 90    // Tertiary two hue
        )
    }
});
```

The `ColorSchemeGenerator` function takes the primary hue and returns a tuple of `(accentHue, tertiaryOneHue, tertiaryTwoHue)`. The default generates complementary and split-complementary colors. Override it if your color theory opinions differ from Penn's. They probably should.

### Available Color Names

All [TailwindCSS colors](https://tailwindcss.com/docs/colors) are available:

- **Neutrals**: `Gray`, `Slate`, `Zinc`, `Neutral`, `Stone`
- **Colors**: `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Cyan`, `Sky`, `Blue`, `Indigo`, `Violet`, `Purple`, `Fuchsia`, `Pink`, `Rose`

### Using Colors in Templates

All five roles are available as utility classes:

```razor
<div class="bg-primary-600 text-primary-50 border-primary-700">
<p class="text-base-800 dark:text-base-200">
<button class="bg-accent-500 hover:bg-accent-600">
```

Stick to `base`, `primary`, and `accent` for your primary design system. The tertiary colors are there for syntax highlighting and the occasional flourish. Restraint is appreciated.

## Implementing Dark/Light Theme Switching

Penn supports dark mode through Tailwind's `dark:` variant prefix, controlled by a `dark` class on `<html>`.

### Preventing Flash of Unstyled Content

Add this script in the `<head>` of your HTML, before any stylesheets load:

```html
<script>
    const isDarkMode = localStorage.theme === "dark" ||
        (!("theme" in localStorage) &&
         window.matchMedia("(prefers-color-scheme: dark)").matches);
    document.documentElement.classList.toggle("dark", isDarkMode);
    document.documentElement.dataset.theme = isDarkMode ? "dark" : "light";
</script>
```

This must be in `<head>`, not in an external script. Loading it later causes a flash of light-mode content that makes your dark-mode users wince. Penn has seen this happen. Penn remembers.

### Theme Toggle Button

Add a button with the `data-theme-toggle` attribute. Penn's JavaScript finds elements with this attribute and wires up the click handler automatically:

```razor
<button aria-label="Toggle Dark Mode" data-theme-toggle>
    <svg class="dark:hidden" viewBox="0 0 24 24"><!-- sun icon --></svg>
    <svg class="hidden dark:block" viewBox="0 0 24 24"><!-- moon icon --></svg>
</button>
```

The `ThemeManager` class toggles the `dark` class on `document.documentElement` and persists the preference to `localStorage`.

### Using Dark Mode Classes

MonorailCSS supports the full TailwindCSS `dark:` prefix syntax:

```razor
<div class="bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100">
<p class="text-primary-700 dark:text-primary-300">
<button class="bg-primary-600 hover:bg-primary-700
               dark:bg-primary-500 dark:hover:bg-primary-400">
```

## Custom Fonts

Override the font families in the design system:

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

The `CustomCssFrameworkSettings` function receives the default `CssFrameworkSettings` (already configured with Penn's color scheme, prose customization, and component styles) and returns a modified version. Use `with` expressions to override only what you need.

## Custom CSS Classes with Applies

The `Applies` dictionary lets you define reusable component classes using Tailwind utility syntax:

```csharp
CustomCssFrameworkSettings = defaultSettings => defaultSettings with
{
    Applies = new Dictionary<string, string>
    {
        { ".my-card", "bg-base-100 border border-base-300 rounded-lg p-4 shadow-sm dark:bg-base-900 dark:border-base-700" },
        { ".btn-primary", "bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md transition-colors" },
        { ".prose code", "bg-primary-100 text-primary-800 px-2 py-1 rounded dark:bg-primary-900 dark:text-primary-200" }
    }
}
```

> [!WARNING]
> The `Applies` dictionary in the above example replaces the default applies entirely. If you want to keep Penn's default code block, tab, and alert styles, merge your custom applies with `defaultSettings.Applies` instead of creating a new dictionary. Penn's defaults are opinionated, but they're also the thing keeping your syntax highlighting from looking terrible.

## Extra Styles

For raw CSS that doesn't fit the utility model, use `ExtraStyles`:

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

Extra styles are prepended to the generated stylesheet, so they're available alongside utility classes.

## Content Paths for CSS Discovery

If you have CSS classes used only in client-side JavaScript or other non-HTML files, tell MonorailCSS where to find them:

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["wwwroot/js/app.js", "wwwroot/js/components.js"]
});
```

This is the Tailwind "content" problem -- classes referenced only in JS won't appear in server-rendered HTML, so MonorailCSS wouldn't normally discover them. `ContentPaths` solves this by scanning the specified files at startup.

## Complete Example

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
        }
    },

    ExtraStyles = """
        @import url('https://fonts.googleapis.com/css2?family=Inter&family=JetBrains+Mono&display=swap');
        """
});
```

## Troubleshooting

- **No styling applied**: Ensure the `<link>` tag uses `LinkService.GetLink("styles.css")` and that `UseMonorailCss()` is called after `UsePenn()` in the middleware pipeline.
- **Theme not switching**: Verify your button has the `data-theme-toggle` attribute and the JavaScript is loaded.
- **Missing classes**: If classes from JS files are missing, add their paths to `ContentPaths`.
- **Colors look wrong**: Double-check your hue values. Hue 0 is red, 120 is green, 240 is blue. Common knowledge, yet commonly forgotten.
