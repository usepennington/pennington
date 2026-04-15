---
title: "Utility components"
description: "The three Pennington.UI utility components — LanguageSwitcher, StructuredData, and FallbackNotice — their parameters and a one-line use-when row each."
uid: reference.ui.utility
order: 404030
sectionLabel: UI Components
tags: [ui, components, localization, structured-data]
---

`Pennington.UI` ships three utility Razor components — `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — that handle locale selection, JSON-LD head injection, and fallback-locale notification respectively. All three live in namespace `Pennington.UI.Components` and are made available via the `@using Pennington.UI.Components` import.

## `LanguageSwitcher`

```razor:path
src/Pennington.UI/Components/LanguageSwitcher.razor
```

Renders a `<details>`-backed dropdown of alternate-language links pre-wired for SPA reload via the `data-spa-reload` attribute; hides itself when fewer than two locales are available, and auto-computes the list from `LocaleContext` and `LocalizationOptions` when `AlternateLanguages` is null or empty.

| Field | Value |
|---|---|
| Renders | `<details>` dropdown of alternate-language `<a>` links in the page chrome. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `AlternateLanguages` | `IReadOnlyList<AlternateLanguageItem>?` | `null` | Explicit list of alternate-language items; when null or empty, the component auto-computes the list from the injected `LocaleContext` and `LocalizationOptions`. |

### `AlternateLanguageItem`

Nested record type that callers supply when constructing an explicit `AlternateLanguages` list; `DocSite`'s `MainLayout` builds one instance per locale per-request from `ContentResolver.GetAlternateLanguagesAsync`.

| Name | Type | Description |
|---|---|---|
| `Locale` | `string` | Locale code written to the `data-locale` attribute on the rendered `<a>`. |
| `DisplayName` | `string` | Visible label used in the dropdown row and as the currently-selected summary text. |
| `Url` | `string` | `href` written on the anchor; typically a locale-prefixed canonical path. |
| `IsCurrentLocale` | `bool` | When `true`, the row renders with current-locale styling (`font-semibold` and the primary accent color). |

### Example

The DocSite `MainLayout` shows the production wiring: guard on `LocalizationOptions.IsMultiLocale`, then pass the pre-computed `_langSwitcherItems` list.

```razor:path
src/Pennington.DocSite/Components/Layout/MainLayout.razor
```

> **Note:** In a DocSite host `LanguageSwitcher` is rendered automatically; the above is the reference wiring for replaced layouts or bare `AddPennington` hosts.

## `StructuredData`

```razor:path
src/Pennington.UI/Components/StructuredData.razor
```

Emits up to three `<script type="application/ld+json">` tags into the document `<head>` via `<HeadContent>`, one each for `JsonLdArticle`, `JsonLdBreadcrumbList`, and `JsonLdWebSite`; each payload is serialized with `JsonLdSerializer` and rendered only when the corresponding parameter is non-null.

| Field | Value |
|---|---|
| Renders | Zero to three `<script type="application/ld+json">` tags injected into `<head>`. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Article` | `JsonLdArticle?` | `null` | Schema.org `Article` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeArticle`. |
| `Breadcrumbs` | `JsonLdBreadcrumbList?` | `null` | Schema.org `BreadcrumbList` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeBreadcrumbList`. |
| `WebSite` | `JsonLdWebSite?` | `null` | Schema.org `WebSite` payload emitted when non-null, serialized by `JsonLdSerializer.SerializeWebSite`; typically rendered once on the home page. |

### Example

The DocSite `Pages` component emits `StructuredData` gated on `CanonicalBaseUrl` being set — article and breadcrumb on content pages, website on the home page.

```razor:path
src/Pennington.DocSite/Components/Layout/Pages.razor
```

> **Note:** The JSON-LD payload record types are documented at <xref:reference.structured-data.types>.

## `FallbackNotice`

```razor:path
src/Pennington.UI/Components/FallbackNotice.razor
```

Renders an inline amber notice banner above the article region when the requested locale has no translation and the page is being served from the default locale; renders nothing when `RequestedLocale` is null or empty.

| Field | Value |
|---|---|
| Renders | Amber-styled notice `<div>` above the article when a fallback is active; nothing otherwise. |

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `RequestedLocale` | `string?` | `null` | Locale code the visitor requested; when non-null and non-empty, the notice renders and displays this value as the unavailable locale. |
| `DefaultLocale` | `string?` | `null` | Locale code the page is served in; displayed in the notice as the locale the visitor sees instead. |

### Example

`DocSiteArticle` places `FallbackNotice` above the article header whenever a non-empty `FallbackRequestedLocale` is supplied by the content resolver.

```razor:path
src/Pennington.DocSite/Slots/Components/DocSiteArticle.razor
```

> **Note:** Fallback detection is owned by `ContentResolver`; `FallbackNotice` is a pure presentation surface.

## See also

- Related reference: [Navigation components](xref:reference.ui.navigation) — sibling `Pennington.UI` reference page for `TableOfContentsNavigation` and `OutlineNavigation`.
- Related reference: [Content components](xref:reference.ui.content) — sibling `Pennington.UI` reference page for `Card`, `Badge`, `CodeBlock`, and the rest of the content-authoring surface.
- Related reference: [JSON-LD schema types](xref:reference.structured-data.types) — the record types (`JsonLdArticle`, `JsonLdBreadcrumbList`, `JsonLdWebSite`) that `StructuredData` serializes.
- How-to: [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale) — tutorial that wires `LanguageSwitcher` and `FallbackNotice` end-to-end via `AddDocSite`.
