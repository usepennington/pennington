---
title: "DocSiteOptions"
description: "Every property on DocSiteOptions (title, description, color scheme, fonts, header/footer, GitHub URL, social image, solution path, areas, etc.)."
section: "options"
order: 20
tags: []
uid: reference.options.docsite-options
isDraft: true
search: false
llms: false
---

> **In this page.** Every property on `DocSiteOptions` (title, description, color scheme, fonts, header/footer, GitHub URL, social image, solution path, areas, etc.).
>
> **Not in this page.** `PenningtonOptions` (the base) — see the preceding page.

## Summary

The options record that configures a documentation site built with `AddDocSite` / `UseDocSite` / `RunDocSiteAsync`.
Namespace `Pennington.DocSite`, declared in `src/Pennington.DocSite/DocSiteOptions.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | Raw HTML appended to the document `<head>`. |
| `AdditionalRoutingAssemblies` | `System.Reflection.Assembly[]` | `[]` | Extra assemblies scanned for Razor component routes. |
| `Areas` | `IReadOnlyList<ContentArea>` | `[]` | Content areas; each `Slug` must match a top-level directory under `ContentRootPath`. No selector is shown when empty or single-area. |
| `BodyFontFamily` | `string?` | `null` | CSS `font-family` applied to body text. |
| `CanonicalBaseUrl` | `string?` | `null` | Canonical absolute base URL used for social metadata, sitemap, and RSS. |
| `ColorScheme` | `Pennington.MonorailCss.IColorScheme?` | `null` | Color palette driver (e.g., `AlgorithmicColorScheme`, `NamedColorScheme`). |
| `ConfigureLocalization` | `Action<Pennington.Infrastructure.LocalizationOptions>?` | `null` | Delegate invoked to configure locales and the default locale. |
| `ContentRootPath` | `Pennington.Routing.FilePath` | `new("Content")` | Root directory that holds the site's markdown content. |
| `Description` | `string` (required) | — | Site description used in metadata and default social tags. |
| `DisplayFontFamily` | `string?` | `null` | CSS `font-family` applied to headings and display text. |
| `ExtraStyles` | `string?` | `null` | Raw CSS appended to the generated stylesheet. |
| `FontPreloads` | `Pennington.Infrastructure.FontPreload[]` | `[]` | Font files emitted as `<link rel="preload">` entries. |
| `FooterContent` | `string?` | `null` | Raw HTML rendered in the site footer. |
| `GitHubUrl` | `string?` | `null` | Repository URL surfaced in the header. |
| `HeaderContent` | `string?` | `null` | Raw HTML rendered beside the site title in the header. |
| `HeaderIcon` | `string?` | `null` | Raw SVG markup rendered as the header icon. |
| `SiteTitle` | `string` (required) | — | Site title shown in the header and document title. |
| `SocialImageUrl` | `string?` | `null` | Default Open Graph / Twitter card image URL. |
| `SolutionPath` | `string?` | `null` | Path to a `.sln` / `.slnx` for Roslyn integration; requires the `Pennington.Roslyn` package. |

## Related types

### `ContentArea`

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

Record describing a top-level content area. `Slug` must match a directory name under `ContentRootPath`.

### `FontPreload`

```csharp:xmldocid
T:Pennington.Infrastructure.FontPreload
```

Record describing a font asset to preload; `Type` defaults to `font/woff2`.

## Example

```csharp:xmldocid,bodyonly
F:Program.<Main>$
```

Minimal `DocSiteOptions` wiring from `examples/BeaconDocsExample/Program.cs`.

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- Related reference: [`MonorailCssOptions`](/reference/options/monorail-css-options)
