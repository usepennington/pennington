---
title: "DocSiteOptions"
description: "The options record that configures a DocSite template host, from site chrome to areas to Pennington escape hatches."
uid: reference.options.docsite-options
order: 401020
sectionLabel: Configuration Options
tags: [docsite, options, configuration, reference]
---

> **In this page.** _Every property on `DocSiteOptions` — site title and description, color scheme, display/body fonts, header/footer content, GitHub URL, social image, solution path, content areas, and the Pennington/MonorailCSS escape-hatch callbacks._
>
> **Not in this page.** _`PenningtonOptions` (the base engine options) — see the preceding page [PenningtonOptions](xref:reference.options.pennington-options)._

## Summary

_**One sentence: what it is.** `DocSiteOptions` is the options record passed to `AddDocSite` that configures the documentation-site template: site chrome, typography, color scheme, content areas, and escape-hatch callbacks for customizing the underlying `PenningtonOptions` and `MonorailCssOptions`._
_**One sentence: where it lives.** Namespace `Pennington.DocSite`, file `src/Pennington.DocSite/DocSiteOptions.cs`, consumed by `DocSiteServiceExtensions.AddDocSite(IServiceCollection, Func<DocSiteOptions>)`._

## Declaration

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

_One sentence: a `public record` with two required init-only properties (`SiteTitle`, `Description`) and the rest optional. Every property is `init`-only — configure once in the factory passed to `AddDocSite` and treat as immutable afterward._

## Properties

_Alphabetical. Two required properties (`SiteTitle`, `Description`) are marked in the Default column. Nested type references link out to their own reference pages; do not duplicate their surface here._

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | _One sentence: raw HTML string injected into every page's `<head>` element, rendered as `MarkupString`._ |
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | _One sentence: extra assemblies scanned for Razor `@page` components so custom pages outside `Pennington.DocSite` participate in routing._ |
| `Areas` | `IReadOnlyList<ContentArea>` | `[]` | _One sentence: content areas shown in the DocSite area selector; each `ContentArea.Slug` must match a top-level directory under `ContentRootPath`, and an empty or single-element list suppresses the selector. See [ContentArea](#contentarea) below._ |
| `BodyFontFamily` | `string?` | `null` | _One sentence: CSS `font-family` value applied to body text; pair with `FontPreloads` and `ExtraStyles` for self-hosted faces._ |
| `CanonicalBaseUrl` | `string?` | `null` | _One sentence: absolute canonical origin used for social meta tags, sitemap entries, RSS links, and structured-data URLs._ |
| `ColorScheme` | `IColorScheme?` | `null` | _One sentence: MonorailCSS color scheme forwarded to `MonorailCssOptions.ColorScheme`; accepts `NamedColorScheme` or `AlgorithmicColorScheme`._ |
| `ConfigureLocalization` | `Action<LocalizationOptions>?` | `null` | _One sentence: callback invoked against the underlying `LocalizationOptions` to register locales and set `DefaultLocale`._ |
| `ConfigurePennington` | `Action<PenningtonOptions>?` | `null` | _One sentence: escape-hatch callback run against the underlying `PenningtonOptions` after DocSite's defaults are applied, used to register extra `AddMarkdownContent<T>` sources, highlighters, or islands without dropping to bare `AddPennington`._ |
| `ContentRootPath` | `FilePath` | `new("Content")` | _One sentence: root directory scanned for markdown content; each `ContentArea.Slug` resolves as a subfolder of this path._ |
| `CustomCssFrameworkSettings` | `Func<CssFrameworkSettings, CssFrameworkSettings>?` | `null` | _One sentence: callback that transforms the MonorailCSS framework settings after the DocSite theme has been applied, mirroring `MonorailCssOptions.CustomCssFrameworkSettings`._ |
| `Description` **(required)** | `string` | — | _One sentence: site-wide description used as the default `<meta name="description">` and in structured data when a page does not supply its own._ |
| `DisplayFontFamily` | `string?` | `null` | _One sentence: CSS `font-family` value applied to heading/display text._ |
| `ExtraStyles` | `string?` | `null` | _One sentence: raw CSS emitted above the generated MonorailCSS stylesheet, forwarded to `MonorailCssOptions.ExtraStyles`; typical use is `@font-face` rules paired with `FontPreloads`._ |
| `FooterContent` | `string?` | `null` | _One sentence: raw HTML rendered as the page footer via `MarkupString`._ |
| `FontPreloads` | `FontPreload[]` | `[]` | _One sentence: font files emitted as `<link rel="preload">` hints in the `<head>`; each entry carries an `Href` and MIME `Type` (default `font/woff2`)._ |
| `GitHubUrl` | `string?` | `null` | _One sentence: repository URL linked from the header GitHub icon; setting `null` hides the icon._ |
| `HeaderContent` | `string?` | `null` | _One sentence: raw HTML rendered inside the site header region via `MarkupString`, typically the site wordmark or home link._ |
| `HeaderIcon` | `string?` | `null` | _One sentence: raw SVG or HTML markup rendered as the header logo/icon._ |
| `LlmsTxtContentSelector` | `string?` | `null` (effective `"#main-content"`) | _One sentence: CSS selector scoping llms.txt raw-markdown extraction to a page region; empty string indexes the whole body, same conventions as `SearchIndexContentSelector`._ |
| `SearchIndexContentSelector` | `string?` | `null` (effective `"#main-content"`) | _One sentence: CSS selector scoping the search index to a page region; empty string indexes the whole body, custom selector supports replaced layouts._ |
| `SiteTitle` **(required)** | `string` | — | _One sentence: site name shown in the header, `<title>` suffix, RSS/sitemap metadata, and structured data._ |
| `SocialImageUrl` | `string?` | `null` | _One sentence: URL used as the default Open Graph / Twitter card image on pages that do not supply a per-page override._ |
| `SolutionPath` | `string?` | `null` | _One sentence: path to a `.sln` or `.slnx` file enabling Roslyn-backed xmldocid code fences; requires the `Pennington.Roslyn` package and its `AddPenningtonRoslyn` registration._ |

### `ContentArea`

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

_One sentence: record describing a top-level section of the documentation site with `Title`, `Slug`, and optional `Icon`._
_One sentence: `Slug` doubles as the URL prefix and the top-level directory name under `ContentRootPath`, so `new ContentArea("Guides", "guides")` maps `/guides/…` to `Content/guides/…`._

### `FontPreload`

```csharp:xmldocid
T:Pennington.Infrastructure.FontPreload
```

_One sentence: record describing a font file to preload via `<link rel="preload">`, with `Href` and `Type` (default `font/woff2`)._

## Example

_One sentence: the `BuildDocSiteOptions` helper in `DocSiteKitchenSinkExample` wires every optional surface in one place — site metadata, color scheme, fonts, header/footer, localization callback, and areas — so each `init` property shows up next to its sibling in one readable block._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

_One sentence of context: this is the factory passed to `AddDocSite` in the kitchen-sink example's `Program.cs`._

## See also

- Related reference: [PenningtonOptions](xref:reference.options.pennington-options) — the base engine options `DocSiteOptions` configures through `ConfigurePennington`.
- Related reference: [MonorailCssOptions](xref:reference.options.monorail-css-options) — the `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` target surface.
- Related reference: [LocalizationOptions](xref:reference.options.localization-options) — the type mutated by the `ConfigureLocalization` callback.
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) — task-oriented walkthrough of `AdditionalHtmlHeadContent`, `ExtraStyles`, slots, and `AdditionalRoutingAssemblies`.
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) — the positioning rationale and the three surfaces DocSite caps.
