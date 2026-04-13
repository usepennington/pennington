---
title: "TranslationOptions and PenningtonStringLocalizer"
description: "TranslationOptions.Add overloads, how PenningtonOptions.Translations is populated, and how PenningtonStringLocalizer resolves UI strings against the current LocaleContext with fallback to the default locale."
section: "options"
order: 60
tags: []
uid: reference.options.translations
isDraft: true
search: false
llms: false
---

> **In this page.** The `TranslationOptions.Add(locale, key, value)` / `Add(locale, dictionary)` overloads, how `PenningtonOptions.Translations` is populated, and how `PenningtonStringLocalizer` resolves UI strings against the current `CultureInfo.CurrentUICulture`, mapped back to a Pennington locale code, with fallback to the default locale and then to the key itself.
>
> **Not in this page.** Enabling multiple locales at the routing layer (see `LocalizationOptions`) or authoring translated page content (see the Localization how-to).

## Summary

- The options object that holds UI-string translations keyed by locale and key, plus the `IStringLocalizer` implementation that reads it during rendering.
- Namespace `Pennington.Localization`; exposed on `PenningtonOptions.Translations` and resolved via the standard `IStringLocalizer` / `IStringLocalizerFactory` DI surfaces.

## `TranslationOptions`

### Declaration

```csharp:xmldocid
T:Pennington.Localization.TranslationOptions
```

### Methods

#### `Add(string locale, string key, string value)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.String,System.String)
```

Registers a single translation for `locale` / `key`. Returns `void`. Creates the per-locale dictionary on first use. Key lookup is case-insensitive. Overwrites any existing entry for the same `locale`/`key` pair.

#### `Add(string locale, Dictionary<string, string> entries)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.Collections.Generic.Dictionary{System.String,System.String})
```

Bulk-registers each entry in `entries` for `locale`. Returns `void`. Internally forwards each pair to the single-entry `Add`; existing entries for the same key are overwritten.

### Runtime access (internal)

- `Get(string locale, string key)` and `GetAll(string locale)` are `internal` — consumed by `PenningtonStringLocalizer`. They are listed here for completeness but are not part of the public surface.
- Translations are stored in a `Dictionary<string, Dictionary<string, string>>` keyed on locale code, with case-insensitive comparers on both levels.

## `PenningtonStringLocalizer`

### Declaration

```csharp:xmldocid
T:Pennington.Localization.PenningtonStringLocalizer
```

### Indexers

| Indexer | Behavior |
|---|---|
| `this[string name]` | Resolves `name` via `GetTranslation` (current locale → default locale → `null`); returns a `LocalizedString(name, value ?? name, resourceNotFound: value is null)`. |
| `this[string name, params object[] arguments]` | Same lookup, then `string.Format(CultureInfo.CurrentCulture, value ?? name, arguments)`; `resourceNotFound` is `true` when the key was not registered. |

### Methods

#### `GetAllStrings(bool includeParentCultures)`

Returns all translations for the resolved locale. When `includeParentCultures` is `true` and the resolved locale differs from `LocalizationOptions.DefaultLocale`, default-locale entries missing from the resolved locale are yielded afterwards.

### Locale resolution order

Each lookup calls `ResolveLocale`, which picks from `CultureInfo.CurrentUICulture` in this order and falls back to `LocalizationOptions.DefaultLocale`:

1. The full culture name (e.g., `"en"`, `"en-US"`) is a registered Pennington locale.
2. The culture name matches a registered locale's `LocaleInfo.HtmlLang` (case-insensitive).
3. The culture's parent name (e.g., `"en"` for `"en-US"`) is a registered locale.
4. Otherwise, return `LocalizationOptions.DefaultLocale`.

### Fallback order

`GetTranslation`:

1. Look up `(resolvedLocale, key)` — return the value if present.
2. If the resolved locale is not the default locale, look up `(defaultLocale, key)` — return if present.
3. Otherwise return `null` (indexer then substitutes `name` / `resourceNotFound: true`).

## Wiring (reference-only, not a how-to)

- `PenningtonOptions.Translations` is a `TranslationOptions` instance created by the options object.
- `AddPennington` registers `TranslationOptions`, `PenningtonStringLocalizerFactory`, and `PenningtonStringLocalizer` in DI.
- Consuming code injects `IStringLocalizer` or `IStringLocalizer<T>` and uses the indexers; no direct `TranslationOptions` injection is required.

## See also

- Related reference: [`LocalizationOptions`](/reference/options/localization-options) — locale registry and URL math.
- How-to: [Enable multiple locales](/how-to/configuration/localization) — populating `TranslationOptions` from `Program.cs`.
- Background: [Locale-aware URLs and content fallback](/explanation/localization/urls-and-fallback) — where string localization fits in the broader locale pipeline.
