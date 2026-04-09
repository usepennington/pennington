---
title: "Styling Your Site with MonorailCSS"
description: "Add Penn.MonorailCss to a Penn site and configure color schemes, dark mode, custom fonts, and extra styles"
uid: "penn.tutorials.styling-with-monorailcss"
order: 20
---

## Beat 1: Starting Point — MonorailCSS Registration

Establish the baseline. If the reader is using DocSite or BlogSite, MonorailCSS is already wired in. If they are building a custom Penn app, they need two lines. Show both paths so no one is left behind.

### What to show
- Code reference: `M:Penn.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Penn.MonorailCss.MonorailCssOptions})` — show the extension method signature; explain it registers `CssClassCollector`, `MonorailCssService`, and a `CssClassCollectorProcessor`
- Code reference: `M:Penn.MonorailCss.MonorailServiceExtensions.UseMonorailCss(Microsoft.AspNetCore.Builder.WebApplication,System.String)` — show the extension method; explain it maps a GET endpoint at `/styles.css` (configurable) that returns the generated stylesheet
- Reference how DocSite wires these in automatically: `:path src/Penn.DocSite/DocSiteServiceExtensions.cs` (lines 48-59 for AddMonorailCss, line 85 for UseMonorailCss)
- For custom apps, show the minimal two-line setup:
  ```csharp
  builder.Services.AddMonorailCss();
  app.UseMonorailCss();
  ```

### Key points
- MonorailCSS is a utility-first CSS framework similar to Tailwind — it scans rendered HTML for class names and generates only the CSS actually used
- The `/styles.css` endpoint is dynamically generated, not a static file
- The `CssClassCollectorProcessor` runs as an `IResponseProcessor`, collecting class names from every HTML response

## Beat 2: Apply a NamedColorScheme

MonorailCSS maps five semantic color roles to full shade palettes ranging from 50 (lightest) to 950 (darkest). The **primary** role appears in links and active navigation items, **accent** shows up in badges and highlights, and **base** controls page backgrounds and body text. Two additional tertiary roles feed syntax highlighting in code blocks.

In this step the reader picks named Tailwind palettes for each role and wires them into the color scheme.

### What to show
- Code reference: `T:Penn.MonorailCss.NamedColorScheme` — show the class definition with its required properties
- Code reference: `P:Penn.DocSite.DocSiteOptions.ColorScheme` — show that DocSiteOptions accepts an `IColorScheme?` which flows into MonorailCss
- Show the default color scheme from `MonorailCssOptions`: `:path src/Penn.MonorailCss/MonorailCssOptions.cs` (lines 16-23) — Blue primary, Purple accent, Cyan tertiary-one, Pink tertiary-two, Slate base
- Show the code the reader adds to their `DocSiteOptions` (or `MonorailCssOptions` directly):
  ```csharp
  ColorScheme = new NamedColorScheme
  {
      PrimaryColorName = ColorNames.Amber,
      AccentColorName = ColorNames.Teal,
      TertiaryOneColorName = ColorNames.Cyan,
      TertiaryTwoColorName = ColorNames.Rose,
      BaseColorName = ColorNames.Stone,
  }
  ```
- Note that `ColorNames` comes from the `MonorailCss` NuGet package (`MonorailCss.Theme.ColorNames`) and contains standard Tailwind color names: Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose, Slate, Gray, Zinc, Neutral, Stone

### Key points
- `NamedColorScheme` maps existing Tailwind palettes by name — the colors are pre-defined, you just choose which role each one fills
- The `required` modifier on each property means the compiler enforces that all five roles are set
- After changing the color scheme, run the site and observe: links change from blue to amber, badges shift to teal, the page background takes on a warm stone tone

### Visual verification
- Point out specific elements where each role appears: primary in sidebar active links and header links, accent in tag badges and search highlight, base in page background (`bg-base-100`/`bg-base-950`) and body text (`text-base-900`/`text-base-50`)

## Beat 3: Switch to an AlgorithmicColorScheme

Show the alternative approach: generating all five palettes from a single hue value using OKLCH color science.

### What to show
- Code reference: `T:Penn.MonorailCss.AlgorithmicColorScheme` — show the full class definition with `PrimaryHue` (required), `BaseColorName` (defaults to Gray), and `ColorSchemeGenerator` (the function that derives accent and tertiary hues)
- Show the `ApplyToTheme` method: `:path src/Penn.MonorailCss/MonorailCssOptions.cs` (lines 81-94) — explain that it calls `ColorPaletteGenerator.GenerateFromHue()` for each role
- Show the default `ColorSchemeGenerator` function: `primary => (primary + 180, primary + 90, primary - 90)` — complementary accent (opposite on the color wheel), and two triadic tertiaries
- Show the replacement code:
  ```csharp
  ColorScheme = new AlgorithmicColorScheme
  {
      PrimaryHue = 25,          // Warm orange
      BaseColorName = "Stone",
  }
  ```
- Explain that hue 25 produces: primary at 25 (orange), accent at 205 (blue — complementary), tertiary-one at 115 (green), tertiary-two at 295 (purple)

### Key points
- `AlgorithmicColorScheme` is useful when you want a harmonious palette from a brand color without manually picking five palettes
- The `PrimaryHue` is a value from 0 to 360 on the OKLCH hue wheel
- The `ColorSchemeGenerator` can be overridden to use different harmony rules (e.g., analogous instead of complementary)
- `BaseColorName` still uses a named Tailwind palette because neutrals are harder to generate algorithmically
- Click the dark mode toggle to verify — dark mode works automatically with any `IColorScheme`. A small inline script in `<head>` applies the saved theme preference before the body renders, preventing a flash of the wrong theme.

## Beat 4: Add a Custom Font

Walk through adding a custom font with proper preloading to avoid flash of unstyled text.

### What to show
- Code reference: `T:Penn.Infrastructure.FontPreload` — show the record definition: `FontPreload(string Href, string Type = "font/woff2")`
- Code reference: `P:Penn.DocSite.DocSiteOptions.FontPreloads` — show the property (array of `FontPreload`)
- Code reference: `P:Penn.DocSite.DocSiteOptions.DisplayFontFamily` and `P:Penn.DocSite.DocSiteOptions.BodyFontFamily` — show these DocSite-specific properties
- Code reference: `P:Penn.DocSite.DocSiteOptions.ExtraStyles` — show the property for injecting raw CSS
- Show the App.razor font preload loop: `:path src/Penn.DocSite/Components/App.razor` (line 17) — the `@foreach` that emits `<link rel="preload">` tags
- Show the configuration code:
  ```csharp
  ExtraStyles = """
      @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
      :root { --font-display: 'Inter', sans-serif; }
  """,
  FontPreloads = [new FontPreload("/fonts/inter-var.woff2", "font/woff2")]
  ```

### Key points
- `FontPreload` emits `<link rel="preload" as="font" crossorigin>` tags in the HTML head — the browser fetches the font file early, before CSS parsing discovers it
- The `Type` parameter defaults to `"font/woff2"` — override it only for non-woff2 formats
- `ExtraStyles` is raw CSS appended to the generated MonorailCSS stylesheet — use it for `@import`, `@font-face` declarations, and CSS custom property overrides
- For self-hosted fonts, place the `.woff2` file in `wwwroot/fonts/` and reference it from both `FontPreloads` and an `@font-face` rule in `ExtraStyles`

## Beat 5: Use Utility Classes in Razor Components

Show the reader how to use MonorailCSS utility classes directly in Razor markup, and explain how class discovery works.

### What to show
- Show a small Razor snippet using utility classes:
  ```razor
  <div class="text-primary-700 dark:text-primary-300 font-bold bg-base-50 dark:bg-base-900 rounded-lg p-4">
      Custom styled content
  </div>
  ```
- Code reference: `T:Penn.MonorailCss.MonorailCssOptions` — show the `ContentPaths` property (lines 38-43): explain that for classes used only in JavaScript files (not HTML), you can list those files in `ContentPaths` so they are scanned at startup
- Reference `M:Penn.MonorailCss.MonorailServiceExtensions.ScanContentFiles(Penn.MonorailCss.CssClassCollector,Microsoft.Extensions.FileProviders.IFileProvider,System.String[])` — the method that extracts potential class names from non-HTML files
- Show how MonorailCSS detects classes: the `CssClassCollectorProcessor` runs as an `IResponseProcessor`, scanning every HTML response for class attributes and extracting tokens

### Key points
- Any Tailwind-style utility class used in Razor markup is automatically detected and included in the generated CSS — no configuration needed for HTML content
- Classes used only in JavaScript or other non-HTML files need to be listed in `ContentPaths` so they are found during startup scanning
- False positives are harmless — MonorailCSS ignores tokens it does not recognize as valid utility classes
- The color roles (`primary`, `accent`, `base`, etc.) work as prefixes: `text-primary-600`, `bg-accent-100`, `border-base-300`

### Visual verification
- Run the site with `dotnet run` and walk through this checklist:
  - Links and active nav items use the primary color (Amber if following the tutorial)
  - Tag badges and highlights use the accent color (Teal)
  - Page background is warm-toned (Stone base palette)
  - Code blocks use tertiary colors for syntax highlighting
  - Click the dark mode toggle — all colors invert to their dark variants
  - Headings use the custom font (Inter if following the tutorial)
  - No flash of unstyled text on initial page load (font preload working)
- The generated stylesheet at `/styles.css` contains only the classes actually used in the site — inspect it to see the output
- Changing the color scheme is a single-line configuration change; every component in the site adapts automatically because they all use the semantic role names, not hard-coded colors

## Beat 6: What's Next

The reader has transformed the site's visual identity: a color scheme controls every surface and text color, a custom font gives the typography a distinct feel, and utility classes let Razor components participate in the same design system.

- For advanced customization — overriding `MonorailCssOptions.CustomCssFrameworkSettings`, adding `ContentPaths` for non-HTML files, and working with CSS layer directives — see [Customizing the CSS Framework](xref:penn.how-to.customizing-the-css-framework).
- For other DocSite-specific options like navigation, site metadata, and layout settings, see [Configuring DocSite](xref:penn.how-to.configuring-docsite).
