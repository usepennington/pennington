---
title: "Configure Custom Styling"
description: "Customize MonorailCSS colors, themes, and integrate with existing CSS frameworks"
uid: "docs.guides.configure-custom-styling"
order: 2050
---

This guide shows you how to customize the visual appearance of your MyLittleContentEngine site using MonorailCSS.
MonorailCSS is a [TailwindCSS](https://tailwindcss.com/) compatible utility-first CSS framework that aims for syntax
compatibility with Tailwind
while providing enhanced customization capabilities. You'll learn to modify color palettes, implement theme switching,
and override default styling. It isn't required for MyLittleContentEngine, but the `MyLittleContentEngine.UI` package
makes assumptions that it's configured, or at least TailwindCSS is configured compatibly.

> [!NOTE]
> For complete MonorailCSS configuration options and component styles, see [Monorail CSS Configuration](xref:docs.reference.monorail-css-configuration).

## Prerequisites

Before customizing styling, ensure you have MonorailCSS configured in your application:

```bash
dotnet add package MyLittleContentEngine.MonorailCss
```

Register the services and middleware in your `Program.cs`:

```csharp
builder.Services.AddMonorailCss();

var app = builder.Build();
app.UseMonorailCss();
```

Then link the generated stylesheet in your layout using `LinkService`, which ensures the path is correct across
all deployment environments:

```razor
@inject LinkService LinkService

<link rel="stylesheet" href="@LinkService.GetLink("styles.css")" />
```

> [!NOTE]
> MonorailCSS works by scanning your rendered HTML at runtime to discover which CSS classes are in use, then
> generating only those styles. This keeps the stylesheet lean without a separate build step. For more detail,
> see [Monorail CSS Configuration](xref:docs.reference.monorail-css-configuration).

## Understanding MonorailCSS Colors

MonorailCSS uses a TailwindCSS-compatible color system with numbered scales (50-950) and semantic color names. The
framework generates a complete color palette from a primary hue, base colors, and automatically calculated accent and
tertiary colors.

### Color Scale Structure

Like TailwindCSS, MonorailCSS uses a numerical color scale:

- `50` - Lightest shade
- `100-400` - Light shades
- `500` - Base/medium shade
- `600-800` - Dark shades
- `900-950` - Darkest shades

## Customizing Color Palettes

MonorailCSS provides two approaches for defining color schemes: named color palettes (using predefined Tailwind colors) or algorithmic generation (generating palettes from hue values).

### Named Color Scheme

The simplest approach uses named Tailwind colors for all five color roles:

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

### Algorithmic Color Scheme

For more control, generate palettes from hue values (0-360 degrees):

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 230,                        // Blue-ish hue
        BaseColorName = ColorNames.Zinc,         // Base neutral palette
        ColorSchemeGenerator = primary => (      // Optional customization
            primary + 180,  // Accent hue (complementary)
            primary + 90,   // Tertiary one hue
            primary - 90    // Tertiary two hue
        )
    }
});
```

### Available Color Names

MonorailCSS supports all TailwindCSS-compatible color names:

- **Neutrals**: `Gray`, `Slate`, `Zinc`, `Neutral`, `Stone`
- **Colors**: `Red`, `Orange`, `Amber`, `Yellow`, `Lime`, `Green`, `Emerald`, `Teal`, `Cyan`, `Sky`, `Blue`, `Indigo`, `Violet`, `Purple`, `Fuchsia`, `Pink`, `Rose`

All [colors defined by Tailwind CSS](https://tailwindcss.com/docs/colors) as of version 4.0 are available.

### Generated Color Palettes

When you configure a primary hue, MonorailCSS automatically generates:

- **primary-{50-950}** - Your brand's primary color scale
- **base-{50-950}** - Neutral colors for backgrounds, text, borders
- **accent-{50-950}** - Complementary colors for highlights
- **tertiary-one-{50-950}** - Additional accent colors
- **tertiary-two-{50-950}** - Additional accent colors

All colors follow TailwindCSS naming conventions, so you can use them like:

```razor
<div class="bg-primary-600 text-primary-50 border-primary-700">
<p class="text-base-800 dark:text-base-200">
<button class="bg-accent-500 hover:bg-accent-600">
```

All colors supported by TailwindCSS are available, but it's recommended to stick to `base`, `primary`, and `accent`
for your primary design system colors, as these are the ones that'll be used by the MonorailCSS components.

## Implementing Dark/Light Theme Switching

MyLittleContentEngine includes built-in theme switching functionality that requires specific HTML markup and JavaScript
integration.

### HTML Markup

Add a theme toggle button with the `data-theme-toggle` attribute:

```razor
<button aria-label="Toggle Dark Mode" data-theme-toggle>
    <!-- Sun icon for light mode -->
    <svg class="dark:hidden" viewBox="0 0 24 24">
        <!-- sun icon path -->
    </svg>
    
    <!-- Moon icon for dark mode -->
    <svg class="hidden dark:block" viewBox="0 0 24 24">
        <!-- moon icon path -->
    </svg>
</button>
```

### JavaScript Integration

The theme switching is handled automatically by the included JavaScript. The `ThemeManager` class:

1. **Finds theme toggle buttons** - Searches for elements with `[data-theme-toggle]`
2. **Binds click events** - Automatically wires up the theme switching functionality
3. **Manages theme state** - Toggles the `dark` class on `document.documentElement`
4. **Persists preference** - Saves theme choice to `localStorage`

### Dark Mode with TailwindCSS Syntax

MonorailCSS supports TailwindCSS dark mode syntax using the `dark:` prefix:

```razor
<!-- TailwindCSS-compatible dark mode classes -->
<div class="bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100">
<p class="text-primary-700 dark:text-primary-300">
<button class="bg-primary-600 hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-400">
```

### Setting the Default Dark/Light Theme

By default, your pages will display in light mode unless you've set the `dark` class on the `<html>` element.

You can make this more dynamic by adding a small script in the `<head>` section of your HTML to check the user's
preference:

```html
<script>
    // Placed in <head> to run before the page renders, preventing a flash of unstyled content (FOUC).
    const isDarkMode = localStorage.theme === "dark" || (!("theme" in localStorage) && window.matchMedia("(prefers-color-scheme: dark)").matches);
    document.documentElement.classList.toggle("dark", isDarkMode);
    document.documentElement.dataset.theme = isDarkMode ? "dark" : "light";
</script>
```

This script checks the user's preference and applies the dark theme if they've set it
or if their system preference is for dark mode. You don't want to do this in the scripts.js file,
as it won't be executed until the page has loaded, which will cause a flash of unstyled content (FOUC).
Instead, you can place this script in the `<head>` section of your HTML to ensure it runs before the page is rendered.

## Custom CSS Framework Settings

You can override MonorailCSS defaults to customize fonts, typography, and other design system elements:

### Custom Fonts

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

### Custom CSS Classes with TailwindCSS Utilities

You can add custom CSS rules using TailwindCSS-compatible utility classes:

```csharp
CustomCssFrameworkSettings = defaultSettings => defaultSettings with
{
    Applies = new Dictionary<string, string>
    {
        // Custom component styles using TailwindCSS syntax
        { ".my-custom-card", "bg-base-100 border border-base-300 rounded-lg p-4 shadow-sm dark:bg-base-900 dark:border-base-700" },
        
        // Button variants
        { ".btn-primary", "bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md transition-colors dark:bg-primary-500 dark:hover:bg-primary-400" },
        
        // Override existing styles
        { ".prose code", "bg-primary-100 text-primary-800 px-2 py-1 rounded dark:bg-primary-900 dark:text-primary-200" }
    }
}
```

The `Applies` dictionary lets you define reusable component classes using the full range of TailwindCSS utilities that
MonorailCSS supports. This is especially useful when integrating with JavaScript frameworks. These apply elements are
used, for example, to style the syntax highlighting in the code blocks.

## Complete Example

Here's a complete example showing advanced styling customization:

```csharp
// Program.cs
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    // Custom brand colors using algorithmic generation
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 205,  // Brand blue
        BaseColorName = ColorNames.Slate,
        ColorSchemeGenerator = primary => (
            primary + 155,  // Green accent
            primary + 45,   // Purple tertiary
            primary - 30    // Orange tertiary
        )
    },

    // Advanced framework customization
    CustomCssFrameworkSettings = defaultSettings => defaultSettings with
    {
        DesignSystem = defaultSettings.DesignSystem with
        {
            // Custom fonts
            FontFamilies = defaultSettings.DesignSystem.FontFamilies
                .Add("brand", new FontFamilyDefinition("Inter, sans-serif"))
                .SetItem("mono", new FontFamilyDefinition("'JetBrains Mono', monospace"))
        },

        // Custom component styles using TailwindCSS utilities
        Applies = new Dictionary<string, string>
        {
            { ".hero-section", "bg-gradient-to-br from-primary-50 to-accent-100 dark:from-primary-950 dark:to-accent-900" },
            { ".card-elevated", "bg-base-50 shadow-lg border border-base-200 rounded-xl p-6 dark:bg-base-900 dark:border-base-700" },
            { ".btn-outline", "border-2 border-primary-500 text-primary-600 hover:bg-primary-500 hover:text-white transition-colors px-4 py-2 rounded-lg" }
        }
    }
});
```

## Troubleshooting

* **No Styling Applied:** Ensure you've added the `<link>` tag using `LinkService.GetLink("styles.css")` as shown
  in the Prerequisites section above.

* **Theme not switching:** Ensure your button has the `data-theme-toggle` attribute and the JavaScript is loaded.
