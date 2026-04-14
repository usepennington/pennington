---
title: "Utility components"
description: "The three Pennington.UI utility components — LanguageSwitcher, StructuredData, and FallbackNotice — their parameters and a one-line use-when row each."
uid: reference.ui.utility
order: 30
sectionLabel: UI Components
tags: [ui, components, localization, structured-data]
---

> **In this page.** _One sentence pulled from `docs-toc.md`: `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — parameters and a one-line "use when" row for each._
>
> **Not in this page.** _One sentence pulled from `docs-toc.md`: authoring your own Razor components — see the tutorial [Author a custom Razor component for markdown](/tutorials/beyond-basics/custom-razor-component) and the extensibility how-to._

## Summary

_**One sentence: what it is.** The three non-content, non-navigation Razor components shipped in `Pennington.UI`: a locale selector, a `<head>`-injecting JSON-LD emitter, and an inline notice shown when a page falls back to the default locale._
_**One sentence: where it lives.** Namespace `Pennington.UI.Components`, files `src/Pennington.UI/Components/LanguageSwitcher.razor`, `StructuredData.razor`, and `FallbackNotice.razor`; all three are surfaced by the `Pennington.UI` `@using Pennington.UI.Components` import in consumers._

## `LanguageSwitcher`

```csharp:xmldocid
T:Pennington.UI.Components.LanguageSwitcher
```

_One sentence: renders a `<details>`-backed dropdown of alternate-language links, pre-wired for SPA reload via the `data-spa-reload` attribute; hides itself when fewer than two locales are available. Falls back to `LocaleContext` + `LocalizationOptions.GetAlternateLanguages` when `AlternateLanguages` is null or empty._

| Field | Value |
|---|---|
| Renders | `<details>` dropdown of alternate-language `<a>` links in the page chrome. |
| Use when | The site has more than one locale registered and the layout needs a language picker. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `AlternateLanguages` | `IReadOnlyList<AlternateLanguageItem>?` | `null` | _One sentence: explicit list of alternate-language items; when null or empty, the component auto-computes the list from the injected `LocaleContext` and `LocalizationOptions`._ |

### `AlternateLanguageItem`

_One sentence: nested record type exposed on the component for callers that need to pass an explicit list — `DocSite`'s `MainLayout` builds one per-request from `ContentResolver.GetAlternateLanguagesAsync`._

| Name | Type | Description |
|---|---|---|
| `Locale` | `string` | _One sentence: locale code written to the `data-locale` attribute on the rendered `<a>`._ |
| `DisplayName` | `string` | _One sentence: visible label used in the dropdown row and as the currently-selected summary text._ |
| `Url` | `string` | _One sentence: `href` written on the anchor; typically a locale-prefixed canonical path._ |
| `IsCurrentLocale` | `bool` | _One sentence: when `true`, the row renders with the current-locale styling (`font-semibold` and the primary accent color)._ |

### Example

_One sentence: the DocSite `MainLayout` shows the production wiring — guard on `LocalizationOptions.IsMultiLocale`, then pass the pre-computed `_langSwitcherItems` list._

```razor:path
src/Pennington.DocSite/Components/Layout/MainLayout.razor
```

_One sentence of context: in a DocSite host `LanguageSwitcher` is rendered for you; the above is the reference wiring for replaced layouts or bare `AddPennington` hosts._

## `StructuredData`

```csharp:xmldocid
T:Pennington.UI.Components.StructuredData
```

_One sentence: emits up to three `<script type="application/ld+json">` tags into the document `<head>` via `<HeadContent>`, one each for `JsonLdArticle`, `JsonLdBreadcrumbList`, and `JsonLdWebSite`. Each payload is serialized with `JsonLdSerializer` and only rendered when the corresponding parameter is non-null._

| Field | Value |
|---|---|
| Renders | Zero to three `<script type="application/ld+json">` tags injected into `<head>`. |
| Use when | A page needs schema.org JSON-LD for SEO — typically article, breadcrumb, or site-level metadata. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Article` | `JsonLdArticle?` | `null` | _One sentence: schema.org `Article` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeArticle`._ |
| `Breadcrumbs` | `JsonLdBreadcrumbList?` | `null` | _One sentence: schema.org `BreadcrumbList` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeBreadcrumbList`._ |
| `WebSite` | `JsonLdWebSite?` | `null` | _One sentence: schema.org `WebSite` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeWebSite`; typically rendered once on the home page._ |

### Example

_One sentence: the DocSite `Pages` component emits `StructuredData` gated on `CanonicalBaseUrl` being set — article + breadcrumb on content pages, website on the home page._

```razor:path
src/Pennington.DocSite/Components/Layout/Pages.razor
```

_One sentence of context: the JSON-LD payload records themselves are documented on the schema types reference page._

## `FallbackNotice`

```csharp:xmldocid
T:Pennington.UI.Components.FallbackNotice
```

_One sentence: renders an inline amber notice banner above the article region when the requested locale has no translation and the page is being served from the default locale. Renders nothing when `RequestedLocale` is null or empty._

| Field | Value |
|---|---|
| Renders | Amber-styled notice `<div>` above the article when a fallback is active; nothing otherwise. |
| Use when | A page is served in the default locale because the requested locale is missing a translation. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `RequestedLocale` | `string?` | `null` | _One sentence: locale code the visitor asked for; when non-null/non-empty, the notice renders and this value is shown as the "not yet available" locale._ |
| `DefaultLocale` | `string?` | `null` | _One sentence: locale code the page is actually being served in; shown in the notice as the locale the visitor is seeing instead._ |

### Example

_One sentence: `DocSiteArticle` places `FallbackNotice` above the article header whenever a non-empty `FallbackRequestedLocale` is supplied by the content resolver._

```razor:path
src/Pennington.DocSite/Slots/Components/DocSiteArticle.razor
```

_One sentence of context: fallback detection itself is owned by `ContentResolver` — the component is a pure presentation surface._

## See also

- Related reference: [Navigation components](/reference/ui/navigation) — sibling `Pennington.UI` reference page for `TableOfContentsNavigation` and `OutlineNavigation`.
- Related reference: [Content components](/reference/ui/content) — sibling `Pennington.UI` reference page for `Card`, `Badge`, `CodeBlock`, and the rest of the content-authoring surface.
- Related reference: [JSON-LD schema types](/reference/structured-data/types) — the record types (`JsonLdArticle`, `JsonLdBreadcrumbList`, `JsonLdWebSite`) that `StructuredData` serializes.
- How-to: [Add a second locale to your site](/tutorials/beyond-basics/add-a-locale) — tutorial that wires `LanguageSwitcher` and `FallbackNotice` end-to-end via `AddDocSite`.
