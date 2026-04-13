---
title: "Utility components"
description: "LanguageSwitcher, StructuredData, and FallbackNotice — parameters and render behavior."
section: "ui"
order: 30
tags: []
uid: reference.ui.utility
isDraft: true
search: false
llms: false
---

> **In this page.** `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — parameters and render behavior.
>
> **Not in this page.** Authoring your own Razor components.

## Summary

- One sentence: what these are — three cross-cutting Razor components shipped by `Pennington.UI` that plug locale switching, JSON-LD emission, and fallback messaging into a page.
- One sentence: where they live — namespace `Pennington.UI.Components`, project `src/Pennington.UI/`, assembly `Pennington.UI`.

## Scope

- Covers: `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — parameters and render behavior.
- Does not cover: authoring your own Razor components.

## `LanguageSwitcher`

- Render condition: emits nothing when fewer than two language entries are resolved.
- Declaration fence: ```razor file="src/Pennington.UI/Components/LanguageSwitcher.razor"```
- Injected services (context, not parameters):
  - `LocaleContext` (scoped)
  - `LocalizationOptions`
- Parameter table:

  | Name | Type | Default | Description |
  |---|---|---|---|
  | `AlternateLanguages` | `IReadOnlyList<LanguageSwitcher.AlternateLanguageItem>?` | `null` | Explicit alternate language entries. When `null` or empty, the component computes entries from `LocaleContext.ContentPath` via `LocalizationOptions.GetAlternateLanguages`. |

- Nested record:
  - `LanguageSwitcher.AlternateLanguageItem(string Locale, string DisplayName, string Url, bool IsCurrentLocale)`
## `StructuredData`

- Render target: emits `<script type="application/ld+json">` elements into `<head>` via `HeadContent`.
- Declaration fence: ```razor file="src/Pennington.UI/Components/StructuredData.razor"```
- Parameter table:

  | Name | Type | Default | Description |
  |---|---|---|---|
  | `Article` | `JsonLdArticle?` | `null` | When non-null, serialized via `JsonLdSerializer.SerializeArticle` and emitted as a JSON-LD `<script>` tag. |
  | `Breadcrumbs` | `JsonLdBreadcrumbList?` | `null` | When non-null, serialized via `JsonLdSerializer.SerializeBreadcrumbList` and emitted as a JSON-LD `<script>` tag. |
  | `WebSite` | `JsonLdWebSite?` | `null` | When non-null, serialized via `JsonLdSerializer.SerializeWebSite` and emitted as a JSON-LD `<script>` tag. |

- Render behavior (stated, not recommended): each parameter is independent — a `<script>` block is emitted only for parameters that are non-null.

## `FallbackNotice`

- Render condition: used when a requested locale has no translation and the default-locale page is being served instead.
- Declaration fence: ```razor file="src/Pennington.UI/Components/FallbackNotice.razor"```
- Parameter table:

  | Name | Type | Default | Description |
  |---|---|---|---|
  | `RequestedLocale` | `string?` | `null` | Locale code the visitor asked for. Rendering is suppressed when this is `null` or empty. |
  | `DefaultLocale` | `string?` | `null` | Locale code of the page actually being shown; displayed inside the notice text. |

- Render behavior (stated, not recommended): nothing is emitted unless `RequestedLocale` is a non-empty string.

## See also

- Related reference: [Navigation components](/reference/ui/navigation)
- Related reference: [Content components](/reference/ui/content)
- Related reference: [Localization options](/reference/options/localization)
- Related reference: [Structured data types](/reference/structured-data/types)
