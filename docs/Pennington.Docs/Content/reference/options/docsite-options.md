---
title: "DocSiteOptions"
description: "The options record that configures a DocSite template host, from site chrome to areas to Pennington escape hatches."
uid: reference.options.docsite-options
order: 401020
sectionLabel: Configuration Options
tags: [docsite, options, configuration, reference]
---

`DocSiteOptions` is the options record passed to `AddDocSite` that configures the documentation-site template: site chrome, typography, color scheme, content areas, and escape-hatch callbacks for the underlying `PenningtonOptions` and `MonorailCssOptions`. Defined in namespace `Pennington.DocSite` and consumed by `DocSiteServiceExtensions.AddDocSite(IServiceCollection, Func<DocSiteOptions>)`.

## Declaration

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

A `public record` with two required `init`-only properties (`SiteTitle`, `Description`); all remaining properties are optional `init`-only and default to `null` or empty collections.

## Properties

Properties are listed alphabetically. The two required properties (`SiteTitle`, `Description`) are marked with **(required)** in the Name column; all others are optional.

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | Raw HTML string injected into every page's `<head>` element, rendered as `MarkupString`. |
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Extra assemblies scanned for Razor `@page` components so custom pages outside `Pennington.DocSite` participate in routing. |
| `Areas` | `IReadOnlyList<ContentArea>` | `[]` | Content areas shown in the DocSite area selector; each `ContentArea.Slug` must match a top-level directory under `ContentRootPath`, and an empty or single-element list suppresses the selector. See [ContentArea](#contentarea) below. |
| `BodyFontFamily` | `string?` | `null` | CSS `font-family` value applied to body text. |
| `CanonicalBaseUrl` | `string?` | `null` | Absolute canonical origin used for social meta tags, sitemap entries, RSS links, and structured-data URLs. |
| `ColorScheme` | `IColorScheme?` | `null` | MonorailCSS color scheme forwarded to `MonorailCssOptions.ColorScheme`; accepts `NamedColorScheme` or `AlgorithmicColorScheme`. |
| `ConfigureLocalization` | `Action<LocalizationOptions>?` | `null` | Callback invoked against the underlying `LocalizationOptions` to register locales and set `DefaultLocale`. |
| `ConfigurePennington` | `Action<PenningtonOptions>?` | `null` | Escape-hatch callback run against the underlying `PenningtonOptions` after DocSite's defaults are applied, allowing registration of extra `AddMarkdownContent<T>` sources, highlighters, or islands without dropping to bare `AddPennington`. |
| `ContentRootPath` | `FilePath` | `new("Content")` | Root directory scanned for markdown content; each `ContentArea.Slug` resolves as a subfolder of this path. |
| `CustomCssFrameworkSettings` | `Func<CssFrameworkSettings, CssFrameworkSettings>?` | `null` | Callback that transforms the MonorailCSS framework settings after the DocSite theme has been applied, mirroring `MonorailCssOptions.CustomCssFrameworkSettings`. |
| `Description` **(required)** | `string` | — | Site-wide description used as the default `<meta name="description">` and in structured data when a page does not supply its own. |
| `DisplayFontFamily` | `string?` | `null` | CSS `font-family` value applied to heading and display text. |
| `ExtraStyles` | `string?` | `null` | Raw CSS emitted above the generated MonorailCSS stylesheet, forwarded to `MonorailCssOptions.ExtraStyles`. |
| `FooterContent` | `string?` | `null` | Raw HTML rendered as the page footer via `MarkupString`. |
| `FontPreloads` | `FontPreload[]` | `[]` | Font files emitted as `<link rel="preload">` hints in the `<head>`; each entry carries an `Href` and MIME `Type` (default `font/woff2`). |
| `GitHubUrl` | `string?` | `null` | Repository URL linked from the header GitHub icon; `null` hides the icon. |
| `HeaderContent` | `string?` | `null` | Raw HTML rendered inside the site header region via `MarkupString`. |
| `HeaderIcon` | `string?` | `null` | Raw SVG or HTML markup rendered as the header logo/icon. |
| `LlmsTxtContentSelector` | `string?` | `null` (effective `"#main-content"`) | CSS selector scoping llms.txt raw-markdown extraction to a page region; an empty string indexes the whole body. |
| `SearchIndexContentSelector` | `string?` | `null` (effective `"#main-content"`) | CSS selector scoping the search index to a page region; an empty string indexes the whole body. |
| `SiteTitle` **(required)** | `string` | — | Site name shown in the header, `<title>` suffix, RSS/sitemap metadata, and structured data. |
| `SocialImageUrl` | `string?` | `null` | URL used as the default Open Graph and Twitter card image on pages that do not supply a per-page override. |
| `SolutionPath` | `string?` | `null` | Path to a `.sln` or `.slnx` file enabling Roslyn-backed xmldocid code fences; requires the `Pennington.Roslyn` package and `AddPenningtonRoslyn` registration. |

### `ContentArea`

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

Record describing a top-level section of the documentation site. `Slug` doubles as the URL prefix and the top-level directory name under `ContentRootPath` — `new ContentArea("Guides", "guides")` maps `/guides/…` to `Content/guides/…`.

### `FontPreload`

```csharp:xmldocid
T:Pennington.Infrastructure.FontPreload
```

Record describing a font file to preload via `<link rel="preload">`, with `Href` and `Type` (defaults to `font/woff2`).

## Example

The `BuildDocSiteOptions` helper in `DocSiteKitchenSinkExample` configures every optional surface in one factory method, covering site metadata, color scheme, fonts, header/footer, localization, and areas.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

This factory is passed to `AddDocSite` in the kitchen-sink example's `Program.cs`.

## See also

- Related reference: [PenningtonOptions](xref:reference.options.pennington-options) — the base engine options `DocSiteOptions` configures through `ConfigurePennington`.
- Related reference: [MonorailCssOptions](xref:reference.options.monorail-css-options) — the `ColorScheme`, `ExtraStyles`, and `CustomCssFrameworkSettings` target surface.
- Related reference: [LocalizationOptions](xref:reference.options.localization-options) — the type mutated by the `ConfigureLocalization` callback.
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) — task-oriented walkthrough of `AdditionalHtmlHeadContent`, `ExtraStyles`, slots, and `AdditionalRoutingAssemblies`.
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) — the positioning rationale and the three surfaces DocSite caps.
