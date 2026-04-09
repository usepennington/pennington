---
title: "Configuring DocSite"
description: "Configure every DocSiteOptions property: content areas, Roslyn integration, header and footer content, social images, fonts, additional routing assemblies, and localization"
uid: "penn.how-to.configuring-docsite"
order: 10
---

## Beat 1: Start with the minimum viable DocSite

The reader creates a bare `Program.cs` with only the two required properties. Running the site shows what the defaults provide: the default Blue/Purple/Cyan/Pink/Slate color scheme, system fonts, no header icon, and a single flat content area.

### What to show
- The `AddDocSite` extension method registration: `M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})`
- The two `required` properties being set: `P:Pennington.DocSite.DocSiteOptions.SiteTitle` and `P:Pennington.DocSite.DocSiteOptions.Description`
- The full middleware chain: `M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)` and `M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`
- A minimal `Program.cs` with `builder.Services.AddDocSite(() => new DocSiteOptions { SiteTitle = "Beacon", Description = "HTTP Monitoring for .NET" });`, then `app.UseDocSite();` and `await app.RunDocSiteAsync(args);`
- One simple `Content/index.md` file with `DocSiteFrontMatter` YAML (title, order)

### Key points
- `SiteTitle` and `Description` are the only `required` properties on `T:Pennington.DocSite.DocSiteOptions`
- `P:Pennington.DocSite.DocSiteOptions.ContentRootPath` defaults to `"Content"` (a `T:Pennington.Routing.FilePath`)
- The default `P:Pennington.DocSite.DocSiteOptions.ColorScheme` is `null`, which falls back to a `T:Pennington.MonorailCss.NamedColorScheme` with Blue primary, Purple accent, Cyan/Pink tertiaries, and Slate base
- `P:Pennington.DocSite.DocSiteOptions.Areas` defaults to an empty list, meaning no area selector UI is shown

---

## Beat 2: Add identity and branding

The reader sets the properties that establish Beacon's identity: a canonical URL for SEO, GitHub link, social preview image, and header/footer content. After running, the header shows a radar icon, version badge, and GitHub link; the footer shows a copyright line.

### What to show
- Set `P:Pennington.DocSite.DocSiteOptions.CanonicalBaseUrl` to `"https://beacon-docs.example.com/"`
- Set `P:Pennington.DocSite.DocSiteOptions.GitHubUrl` to `"https://github.com/example/beacon"`
- Set `P:Pennington.DocSite.DocSiteOptions.SocialImageUrl` to `"/social.png"`
- Set `P:Pennington.DocSite.DocSiteOptions.HeaderIcon` to an inline SVG string (a radar/signal icon, ~5 lines of SVG markup)
- Set `P:Pennington.DocSite.DocSiteOptions.HeaderContent` to `"<span class='badge'>v3.2</span>"`
- Set `P:Pennington.DocSite.DocSiteOptions.FooterContent` to `"&copy; 2026 Beacon Contributors"`

### Key points
- `HeaderIcon` accepts raw HTML/SVG markup -- it is rendered inline in the site header next to the title
- `HeaderContent` is arbitrary HTML rendered in the header area (useful for version badges, status indicators)
- `FooterContent` is arbitrary HTML rendered in the site footer
- `CanonicalBaseUrl` is used for RSS feeds, sitemaps, and `og:url` meta tags -- it should include the trailing slash
- `SocialImageUrl` is used for `og:image` meta tags on all pages
- `GitHubUrl` renders a GitHub icon link in the header

---

## Beat 3: Configure typography

The reader replaces system fonts with custom web fonts. Two `FontPreload` entries prevent flash of unstyled text, and `@font-face` declarations go in `ExtraStyles`. After running, the fonts load without FOUT.

### What to show
- Set `P:Pennington.DocSite.DocSiteOptions.DisplayFontFamily` to `"Inter, sans-serif"`
- Set `P:Pennington.DocSite.DocSiteOptions.BodyFontFamily` to `"'Source Sans 3', sans-serif"`
- Set `P:Pennington.DocSite.DocSiteOptions.FontPreloads` to an array of `T:Pennington.Infrastructure.FontPreload` records:
  ```csharp
  FontPreloads = [
      new FontPreload("fonts/inter.woff2"),
      new FontPreload("fonts/source-sans.woff2"),
  ],
  ```
- Show the `T:Pennington.Infrastructure.FontPreload` record signature: `FontPreload(string Href, string Type = "font/woff2")`
- Set `P:Pennington.DocSite.DocSiteOptions.ExtraStyles` with `@font-face` declarations for both fonts
- Set `P:Pennington.DocSite.DocSiteOptions.AdditionalHtmlHeadContent` to inject any extra `<meta>` or `<link>` tags needed

### Key points
- `FontPreload` generates `<link rel="preload" as="font" type="font/woff2" href="..." crossorigin>` in the HTML head
- The `Type` parameter defaults to `"font/woff2"` but can be changed for other font formats
- `ExtraStyles` is prepended to the generated MonorailCSS stylesheet -- it is the right place for `@font-face` rules, CSS custom properties, and `@layer` directives
- `AdditionalHtmlHeadContent` is raw HTML injected into `<head>` -- useful for third-party scripts, analytics, or extra meta tags
- `DisplayFontFamily` typically applies to headings; `BodyFontFamily` applies to body text (the exact mapping depends on the layout components)

---

## Beat 4: Set the color scheme

The reader switches from the default named-color scheme to an `AlgorithmicColorScheme` that generates the entire palette from a single hue value. Beacon's brand is teal (hue 195), with Zinc as the neutral base. After running, the entire site palette shifts.

### What to show
- Set `P:Pennington.DocSite.DocSiteOptions.ColorScheme` to an `T:Pennington.MonorailCss.AlgorithmicColorScheme`:
  ```csharp
  ColorScheme = new AlgorithmicColorScheme
  {
      PrimaryHue = 195,
      BaseColorName = ColorNames.Zinc,
  },
  ```
- Show the `T:Pennington.MonorailCss.AlgorithmicColorScheme` class with its properties: `P:Pennington.MonorailCss.AlgorithmicColorScheme.PrimaryHue`, `P:Pennington.MonorailCss.AlgorithmicColorScheme.BaseColorName`, `P:Pennington.MonorailCss.AlgorithmicColorScheme.ColorSchemeGenerator`
- Contrast with `T:Pennington.MonorailCss.NamedColorScheme` and its five required properties: `P:Pennington.MonorailCss.NamedColorScheme.PrimaryColorName`, `P:Pennington.MonorailCss.NamedColorScheme.AccentColorName`, `P:Pennington.MonorailCss.NamedColorScheme.TertiaryOneColorName`, `P:Pennington.MonorailCss.NamedColorScheme.TertiaryTwoColorName`, `P:Pennington.MonorailCss.NamedColorScheme.BaseColorName`
- Show the `T:Pennington.MonorailCss.IColorScheme` interface and its `M:Pennington.MonorailCss.IColorScheme.ApplyToTheme(MonorailCss.Theme.Theme)` method

### Key points
- `AlgorithmicColorScheme` uses `T:Pennington.MonorailCss.ColorPaletteGenerator` to produce OKLCH color palettes from a hue value (0-360)
- The default `ColorSchemeGenerator` computes accent as hue+180, tertiary-one as hue+90, tertiary-two as hue-90 (a complementary+split-complementary strategy)
- `NamedColorScheme` maps existing Tailwind color palette names (e.g., `ColorNames.Blue`, `ColorNames.Teal`) to Pennington's five semantic roles: primary, accent, tertiary-one, tertiary-two, base
- Both implement `IColorScheme`, so you can create a fully custom implementation if neither built-in option fits
- The five semantic color roles (primary, accent, tertiary-one, tertiary-two, base) are used throughout Pennington's generated CSS for code blocks, alerts, links, and navigation

---

## Beat 5: Organize content areas

The reader adds four `ContentArea` entries to group content into tabbed sections. Each area maps to a top-level directory under `ContentRootPath` and gets its own sidebar navigation tree.

### What to show
- Set `P:Pennington.DocSite.DocSiteOptions.Areas` with four `T:Pennington.DocSite.ContentArea` entries:
  ```csharp
  Areas = [
      new ContentArea("Getting Started", "getting-started"),
      new ContentArea("Guides", "guides", Icon: "<svg>...</svg>"),
      new ContentArea("API Reference", "api"),
      new ContentArea("Changelog", "changelog"),
  ],
  ```
- Show the `T:Pennington.DocSite.ContentArea` record: `ContentArea(string Title, string Slug, string? Icon = null)`
- Create matching directory structure under `Content/`: `getting-started/`, `guides/`, `api/`, `changelog/`
- Each directory contains 1-2 markdown files with `T:Pennington.DocSite.DocSiteFrontMatter` YAML demonstrating `P:Pennington.DocSite.DocSiteFrontMatter.Order` and `P:Pennington.DocSite.DocSiteFrontMatter.Section`

### Key points
- Each `ContentArea.Slug` must match a top-level directory name under `ContentRootPath`
- When `Areas` is empty or contains a single entry, no area selector UI appears
- The `Icon` parameter accepts raw SVG or HTML markup rendered beside the area title in the selector
- Content files use `T:Pennington.DocSite.DocSiteFrontMatter` which implements `T:Pennington.FrontMatter.IOrderable` (for sidebar ordering), `T:Pennington.FrontMatter.ISectionable` (for grouping within an area), `T:Pennington.FrontMatter.IDraftable`, `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.ICrossReferenceable`, `T:Pennington.FrontMatter.IDescribable`, and `T:Pennington.FrontMatter.IRedirectable`
- `DocSiteFrontMatter.Order` defaults to `int.MaxValue`, so pages without an explicit order sort last

---

## Beat 6: Wire up Roslyn, extra assemblies, and localization

The reader configures the three integration-focused properties: `SolutionPath` for Roslyn-powered code highlighting, `AdditionalRoutingAssemblies` for Razor pages in companion projects, and `ConfigureLocalization` for multi-language support.

### What to show
- Set `P:Pennington.DocSite.DocSiteOptions.SolutionPath` to `"../../Beacon.slnx"`
- Set `P:Pennington.DocSite.DocSiteOptions.AdditionalRoutingAssemblies` to `[typeof(DashboardPage).Assembly]`
- Set `P:Pennington.DocSite.DocSiteOptions.ConfigureLocalization` as a callback:
  ```csharp
  ConfigureLocalization = opts =>
  {
      opts.AddLocale("en", "English");
      opts.AddLocale("es", "Espa\u00f1ol");
  },
  ```
- Show `T:Pennington.Infrastructure.LocalizationOptions` with its methods: `M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,System.String)` and `M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)`
- Show `T:Pennington.Localization.LocaleInfo` record: `LocaleInfo(string DisplayName, string Direction = "ltr", string? HtmlLang = null)`

### Key points
- `SolutionPath` points to a `.sln` or `.slnx` file; it requires the optional `Pennington.Roslyn` package to be installed for Roslyn-based code highlighting to work
- `AdditionalRoutingAssemblies` are passed to `MapRazorComponents().AddAdditionalAssemblies()` so that `@page` directives in other assemblies are discovered
- `ConfigureLocalization` receives the `T:Pennington.Infrastructure.LocalizationOptions` instance from `T:Pennington.Infrastructure.PenningtonOptions`; the first locale added is not automatically the default -- set `P:Pennington.Infrastructure.LocalizationOptions.DefaultLocale` explicitly if needed (defaults to `"en"`)
- `LocaleInfo.Direction` supports `"rtl"` for right-to-left languages
- The locale switcher UI appears automatically when `P:Pennington.Infrastructure.LocalizationOptions.IsMultiLocale` returns `true` (more than one locale registered)

---

## Beat 7: Review what AddDocSite registers

A brief walkthrough of the services that `AddDocSite` registers internally, so the reader understands what can be overridden via DI and what runs automatically.

### What to show
- Walk through `M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})` body, calling out each registration:
  1. `T:Pennington.DocSite.DocSiteOptions` as singleton
  2. `M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})` -- core engine (content pipeline, highlighting, markdown, search, feeds, localization)
  3. Inside `AddPennington`: `M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})` with `T:Pennington.DocSite.DocSiteFrontMatter`
  4. `M:Pennington.Infrastructure.PenningtonOptions.AddLlmsTxt(System.Action{Pennington.LlmsTxt.LlmsTxtOptions})` for llms.txt generation
  5. `M:Pennington.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Pennington.MonorailCss.MonorailCssOptions})` -- CSS framework
  6. `M:Pennington.Islands.SpaNavigationExtensions.AddSpaNavigation(Microsoft.Extensions.DependencyInjection.IServiceCollection)` -- SPA navigation data endpoints
  7. `T:Pennington.Islands.ComponentRenderer` as scoped -- renders island components for SPA transitions
  8. `T:Pennington.DocSite.Slots.DocSiteArticleSlotRenderer` as `T:Pennington.Islands.IIslandRenderer` -- article content renderer for SPA
  9. `T:Pennington.DocSite.Services.ContentResolver` as transient
- Walk through `M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)` middleware pipeline order: `UsePenningtonLocaleRouting` -> `UseAntiforgery` -> `UseStaticFiles` -> `MapRazorComponents` -> `UseMonorailCss` -> `UseSpaNavigation` -> `UsePennington`

### Key points
- The reader can override any transient or scoped registration by registering their own implementation after `AddDocSite`
- `AddPennington` internally registers the content pipeline (`T:Pennington.Pipeline.ContentPipeline`), markdown parser, highlighter services, search index, sitemap, and all response processors
- The `UseDocSite` middleware order matters: locale routing must come before Razor component mapping so Blazor sees locale-stripped paths
- `RunDocSiteAsync` delegates to `M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`, which either starts the dev server or runs a static build depending on `args[0]`
