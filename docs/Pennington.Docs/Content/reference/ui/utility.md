---
title: "Utility components"
description: "The three Pennington.UI utility components — LanguageSwitcher, StructuredData, and FallbackNotice — their parameters and a one-line use-when row each."
uid: reference.ui.utility
order: 3
sectionLabel: UI Components
tags: [ui, components, localization, structured-data]
---

`Pennington.UI` ships three utility Razor components — `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — that handle locale selection, JSON-LD head injection, and fallback-locale notification respectively. All three live in namespace `Pennington.UI.Components` and are made available via the `@using Pennington.UI.Components` import.

## `LanguageSwitcher`

Renders a `<details>`-backed dropdown of alternate-language links pre-wired for SPA reload via `data-spa-reload`. Hides itself when fewer than two locales are available. Auto-computes the list from `LocaleContext` and `LocalizationOptions` when `AlternateLanguages` is null or empty.

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

```razor
@if (LocalizationOptions.IsMultiLocale)
{
    <LanguageSwitcher AlternateLanguages="_langSwitcherItems" />
}
```

## `StructuredData`

Emits one `<script type="application/ld+json">` per supplied entity into the document `<head>` via `<HeadContent>`. Accepts any sequence of `JsonLdEntity` — including user-defined subclasses — and serializes each with `JsonLdSerializer`. Null entries in the sequence are skipped.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Entities` | `IEnumerable<JsonLdEntity>?` | `null` | Sequence of schema.org entities to emit. Each is serialized by `JsonLdSerializer.Serialize` and rendered as its own `<script type="application/ld+json">` block. |

### Example

```razor
@if (!string.IsNullOrEmpty(Options.CanonicalBaseUrl))
{
    <StructuredData Entities="@entities" />
}

@code {
    private IEnumerable<JsonLdEntity> entities = [
        new JsonLdArticle { Headline = "...", Url = "..." },
        new JsonLdBreadcrumbList { Items = [...] },
    ];
}
```

To define a schema.org type Pennington doesn't ship, see [Add a custom schema.org JSON-LD type](xref:how-to.rich-content.structured-data-custom-types).

## `FallbackNotice`

Renders an inline amber notice banner above the article region when the requested locale has no translation and the page is being served from the default locale. Renders nothing when `RequestedLocale` is null or empty.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `RequestedLocale` | `string?` | `null` | Locale code the visitor requested; when non-null and non-empty, the notice renders and displays this value as the unavailable locale. |
| `DefaultLocale` | `string?` | `null` | Locale code the page is served in; displayed in the notice as the locale the visitor sees instead. |

### Example

```razor
<FallbackNotice RequestedLocale="@Article.FallbackRequestedLocale"
                DefaultLocale="@LocalizationOptions.DefaultLocale" />
```

## See also

- Related reference: [Navigation components](xref:reference.ui.navigation) — sibling `Pennington.UI` reference page for `TableOfContentsNavigation` and `OutlineNavigation`.
- Related reference: [Content components](xref:reference.ui.content) — sibling `Pennington.UI` reference page for `Card`, `Badge`, `CodeBlock`, and the rest of the content-authoring surface.
- How-to: [Add a custom schema.org JSON-LD type](xref:how-to.rich-content.structured-data-custom-types) — define a new `JsonLdEntity` subclass and emit it through `StructuredData`.
- How-to: [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale) — tutorial that wires `LanguageSwitcher` and `FallbackNotice` end-to-end via `AddDocSite`.
