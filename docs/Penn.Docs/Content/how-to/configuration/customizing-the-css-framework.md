---
title: "Customizing the CSS Framework"
description: "Use CustomCssFrameworkSettings for advanced MonorailCSS theming, configure ContentPaths for JS class scanning, inject raw CSS with ExtraStyles, and understand the CssClassCollector's runtime vs startup behavior"
uid: "penn.how-to.customizing-the-css-framework"
order: 40
---

## Beat 1: Start with the default MonorailCSS theme

The reader examines Beacon's documentation site running with the default MonorailCSS configuration. They identify three things that need to change: code block backgrounds are too light for the brand, alert colors need adjustment, and a planned JavaScript theme-toggle widget uses utility classes that MonorailCSS will not see in HTML.

### What to show
- The `T:Penn.MonorailCss.MonorailCssOptions` class with its four properties: `P:Penn.MonorailCss.MonorailCssOptions.ColorScheme`, `P:Penn.MonorailCss.MonorailCssOptions.CustomCssFrameworkSettings`, `P:Penn.MonorailCss.MonorailCssOptions.ExtraStyles`, `P:Penn.MonorailCss.MonorailCssOptions.ContentPaths`
- The default `ColorScheme` value -- a `T:Penn.MonorailCss.NamedColorScheme` with `PrimaryColorName = ColorNames.Blue`, `AccentColorName = ColorNames.Purple`, `TertiaryOneColorName = ColorNames.Cyan`, `TertiaryTwoColorName = ColorNames.Pink`, `BaseColorName = ColorNames.Slate`
- How DocSite passes options through: in `M:Penn.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Penn.DocSite.DocSiteOptions})`, `DocSiteOptions.ColorScheme` is forwarded to `MonorailCssOptions.ColorScheme`, and `DocSiteOptions.ExtraStyles` to `MonorailCssOptions.ExtraStyles`
- The generated `/styles.css` endpoint registered by `M:Penn.MonorailCss.MonorailServiceExtensions.UseMonorailCss(Microsoft.AspNetCore.Builder.WebApplication,System.String)`

### Key points
- `MonorailCssOptions` controls the entire CSS generation pipeline -- color scheme, design token overrides, extra raw CSS, and file scanning
- The `CustomCssFrameworkSettings` callback defaults to the identity function (`settings => settings`) -- it only modifies the framework when you provide a custom callback
- `ExtraStyles` defaults to `string.Empty` -- when set, it is prepended to the generated stylesheet
- `ContentPaths` defaults to an empty array -- most sites do not need it because the `T:Penn.MonorailCss.CssClassCollectorProcessor` discovers classes at runtime from HTML and JSON responses

---

## Beat 2: Override the color scheme with NamedColorScheme

The reader replaces the default color scheme with a `NamedColorScheme` that maps Beacon's brand colors (Teal primary, Cyan accent) to Penn's five semantic color roles. After running, the entire site palette shifts.

### What to show
- Configure `MonorailCssOptions` through `DocSiteOptions`:
  ```csharp
  ColorScheme = new NamedColorScheme
  {
      PrimaryColorName = ColorNames.Teal,
      AccentColorName = ColorNames.Cyan,
      TertiaryOneColorName = ColorNames.Sky,
      TertiaryTwoColorName = ColorNames.Emerald,
      BaseColorName = ColorNames.Slate,
  },
  ```
- Show `T:Penn.MonorailCss.NamedColorScheme` and its `M:Penn.MonorailCss.NamedColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)` method -- it calls `theme.MapColorPalette(name, "primary")` for each semantic role
- Contrast with `T:Penn.MonorailCss.AlgorithmicColorScheme` which generates palettes from a hue via `T:Penn.MonorailCss.ColorPaletteGenerator` -- show `M:Penn.MonorailCss.ColorPaletteGenerator.GenerateFromHue(System.Double)` that produces OKLCH color values
- Show the `T:Penn.MonorailCss.IColorScheme` interface: `M:Penn.MonorailCss.IColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)` returns a modified `Theme`

### Key points
- `NamedColorScheme` maps existing Tailwind color palette names to Penn's semantic roles -- no color generation involved, just remapping
- `AlgorithmicColorScheme` generates full palettes algorithmically: `P:Penn.MonorailCss.AlgorithmicColorScheme.PrimaryHue` (0-360) is the input, and `P:Penn.MonorailCss.AlgorithmicColorScheme.ColorSchemeGenerator` computes accent and tertiary hues (default: complementary + split-complementary)
- Penn uses five semantic color roles everywhere: `primary` (links, highlights, headings), `accent` (functions, tabs), `tertiary-one` (strings, numbers in code), `tertiary-two` (variables, attributes in code), `base` (backgrounds, borders, text)
- You can implement `T:Penn.MonorailCss.IColorScheme` yourself for full control -- the interface has a single method

---

## Beat 3: Customize component styles with CustomCssFrameworkSettings

The reader writes the `CustomCssFrameworkSettings` callback to override code block backgrounds and alert accent colors at the design-token level. This is a theme-level override, not a CSS hack.

### What to show
- Set `P:Penn.MonorailCss.MonorailCssOptions.CustomCssFrameworkSettings` -- the property type is `Func<CssFrameworkSettings, CssFrameworkSettings>`:
  ```csharp
  CustomCssFrameworkSettings = settings => settings with
  {
      Applies = settings.Applies
          .SetItem(".code-highlight-wrapper .standalone-code-container",
              "bg-slate-50 border border-slate-300 shadow-xs rounded-xl overflow-x-auto dark:bg-slate-900 dark:border-slate-700")
          .SetItem(".markdown-alert-note",
              "fill-teal-700 dark:fill-teal-500 bg-teal-100/75 border-teal-500/20 dark:border-teal-500/30 dark:bg-teal-900/25 text-teal-800 dark:text-teal-200"),
  },
  ```
- Show how `T:Penn.MonorailCss.MonorailCssService` uses the callback: in `M:Penn.MonorailCss.MonorailCssService.GetStyleSheet`, the service builds a `CssFrameworkSettings` with default `Applies` (code blocks, tabs, alerts, hljs, search modal), then passes it through `options.CustomCssFrameworkSettings(cssFrameworkSettings)` before creating the `CssFramework`
- Reference the built-in applies dictionaries to show what keys are available for override: the code block keys (`.code-highlight-wrapper .standalone-code-container`, `.code-highlight-wrapper pre`, etc.), alert keys (`.markdown-alert-note`, `.markdown-alert-tip`, `.markdown-alert-caution`, `.markdown-alert-warning`, `.markdown-alert-important`), and hljs syntax highlighting keys (`.hljs-keyword`, `.hljs-string`, `.hljs-function`, etc.)

### Key points
- `CssFrameworkSettings.Applies` is an `ImmutableDictionary<string, string>` where keys are CSS selectors and values are space-separated MonorailCSS utility class strings
- Use `.SetItem()` to override a specific selector's utility classes while preserving all other defaults
- The `ProseCustomization` property on `CssFrameworkSettings` controls how `prose` typography styling renders (link colors, code backgrounds, blockquote borders) -- it can also be overridden via the `with` expression
- The callback receives the fully-assembled settings (including color-scheme-applied theme), so overrides see the correct palette

---

## Beat 4: Add raw CSS with ExtraStyles

The reader adds a `@layer brand` declaration with custom CSS properties. They learn that `ExtraStyles` is prepended to the generated stylesheet and how CSS layer ordering works with MonorailCSS.

### What to show
- Set `P:Penn.MonorailCss.MonorailCssOptions.ExtraStyles` (through `P:Penn.DocSite.DocSiteOptions.ExtraStyles`):
  ```csharp
  ExtraStyles = """
      @layer brand {
          :root {
              --brand-gradient: linear-gradient(135deg, var(--color-teal-500), var(--color-cyan-400));
              --brand-radius: 0.75rem;
          }
          .brand-hero { background: var(--brand-gradient); border-radius: var(--brand-radius); }
      }
      """,
  ```
- Show how `M:Penn.MonorailCss.MonorailCssService.GetStyleSheet` prepends `ExtraStyles` before the generated utility stylesheet:
  ```
  {options.ExtraStyles}
  
  {styleSheet}
  ```

### Key points
- `ExtraStyles` is prepended (not appended) to the generated stylesheet -- this means `@layer` directives and `@font-face` rules appear before the utility classes
- CSS `@layer` ordering gives you control over specificity: a `@layer brand` block has lower specificity than unlayered utility classes, but the order ensures custom properties are available everywhere
- This is the correct place for `@font-face` declarations, CSS custom properties, `@import` statements, and any hand-written CSS that complements the utility framework
- The raw CSS is not processed by MonorailCSS -- it is included verbatim

---

## Beat 5: Fix missing JavaScript classes with ContentPaths

The reader adds a JavaScript file that toggles utility classes on DOM elements. Running the site reveals that these classes produce no styles because the `CssClassCollector` never sees them. Adding the JS file to `ContentPaths` fixes the problem.

### What to show
- Create `wwwroot/js/theme-toggle.js` with ~15 lines toggling classes like `bg-slate-800`, `text-white`, `border-teal-500` on a container element
- Run the site and observe the missing styles
- Explain the `T:Penn.MonorailCss.CssClassCollector` dual strategy:
  1. **Runtime scanning** via `T:Penn.MonorailCss.CssClassCollectorProcessor` -- an `T:Penn.Infrastructure.IResponseProcessor` that extracts CSS class names from HTML and JSON responses using regex. Show `M:Penn.MonorailCss.CssClassCollectorProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)` (it observes but never modifies responses)
  2. **Startup file scanning** via `P:Penn.MonorailCss.MonorailCssOptions.ContentPaths` -- files scanned at startup by `M:Penn.MonorailCss.MonorailServiceExtensions.ScanContentFiles(Penn.MonorailCss.CssClassCollector,Microsoft.Extensions.FileProviders.IFileProvider,System.String[])`. Show the two extraction strategies in `M:Penn.MonorailCss.MonorailServiceExtensions.ExtractPotentialClasses(System.String)`: HTML class attribute regex and broad token splitting
- Set `P:Penn.MonorailCss.MonorailCssOptions.ContentPaths` to `["js/theme-toggle.js"]`
- Note: `ContentPaths` are relative to the web root (`wwwroot/`), not the project root
- Show `M:Penn.MonorailCss.CssClassCollector.AddClasses(System.String,System.Collections.Generic.IEnumerable{System.String})` and `M:Penn.MonorailCss.CssClassCollector.GetClasses` -- the thread-safe collector that accumulates classes across requests

### Key points
- The `CssClassCollectorProcessor` only sees classes in rendered HTML and JSON responses -- classes referenced only in JavaScript, CSS-in-JS, or data attributes are invisible to it
- `ContentPaths` solves this the same way Tailwind's `content` configuration does: scan specified files at startup and register all potential class names
- `ExtractPotentialClasses` uses two strategies: (1) regex matching on `class="..."` attributes, (2) broad token splitting that catches classes in JS string constants -- false positives are harmless because MonorailCSS ignores tokens it does not recognize
- `CssClassCollector` is thread-safe (uses `ReaderWriterLockSlim`) -- classes accumulate across requests and are never cleared at runtime; stale classes are harmless and removed on the next static build
- `P:Penn.MonorailCss.CssClassCollector.ShouldProcess(System.String)` always returns `true` in the current implementation

---

## Beat 6: Verify the generated stylesheet

The reader inspects `/styles.css` to confirm that all customizations are present: brand colors from the `NamedColorScheme`, overridden code block and alert styles from `CustomCssFrameworkSettings`, the `@layer brand` block from `ExtraStyles`, and the JS-referenced utility classes from `ContentPaths`.

### What to show
- Navigate to `/styles.css` in the browser
- Point out the structure: `ExtraStyles` content appears first (the `@layer brand` block), followed by the MonorailCSS-generated utility classes
- Verify the overridden `.code-highlight-wrapper .standalone-code-container` selector uses the slate-based classes
- Verify the `.markdown-alert-note` selector uses teal-based classes
- Search for `bg-slate-800`, `text-white`, `border-teal-500` to confirm the JS-referenced classes generated styles
- Show the `/styles.css` endpoint mapping in `M:Penn.MonorailCss.MonorailServiceExtensions.UseMonorailCss(Microsoft.AspNetCore.Builder.WebApplication,System.String)`: `app.MapGet(path, (MonorailCssService cssService) => Results.Content(cssService.GetStyleSheet(), "text/css"))`

### Key points
- The stylesheet is generated on every request during development (the `CssClassCollector` grows as new pages are visited) and at build time for static output
- The default stylesheet endpoint is `/styles.css` but can be changed via the `path` parameter of `UseMonorailCss`
- During static site generation, the build visits all pages first (populating the collector), then serves the final stylesheet with all discovered classes
- The `MonorailCssService` is registered as transient, so each request gets a fresh instance that reads the current state of the singleton `CssClassCollector`
