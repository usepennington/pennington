---
title: "TranslationOptions"
description: "TranslationOptions.Add(locale, key, value) and Add(locale, dictionary) overloads, plus the runtime access methods that back PenningtonOptions.Translations."
section: "options"
order: 60
tags: []
uid: reference.options.translations
isDraft: true
search: false
llms: false
---

> **In this page.** The `TranslationOptions.Add(locale, key, value)` and `Add(locale, dictionary)` overloads, plus the internal runtime access methods that back `PenningtonOptions.Translations`.
>
> **Not in this page.** Enabling multiple locales at the routing layer (see `LocalizationOptions`) or authoring translated page content (see the Localization how-to).

## Summary

- The options object that holds UI-string translations keyed by locale and key.
- Namespace `Pennington.Localization`; exposed on `PenningtonOptions.Translations` and read at render time via the standard `IStringLocalizer` / `IStringLocalizerFactory` DI surfaces.

## TranslationOptions

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

- `Get(string locale, string key)` and `GetAll(string locale)` are `internal` — consumed by the framework's `IStringLocalizer` implementation. They are listed here for completeness but are not part of the public surface.
- Translations are stored in a `Dictionary<string, Dictionary<string, string>>` keyed on locale code, with case-insensitive comparers on both levels.

## Consumption

Consuming code injects `IStringLocalizer` or `IStringLocalizer<T>` from DI and resolves keys via the indexer. Lookups fall back from the active locale to `LocalizationOptions.DefaultLocale`, then to the key itself. `AddPennington` wires the implementation.

## See also

- Related reference: [`LocalizationOptions`](/reference/options/localization-options) — locale registry and URL math.
- How-to: [Enable multiple locales](/how-to/configuration/localization) — populating `TranslationOptions` from `Program.cs`.
- Background: [Locale-aware URLs and content fallback](/explanation/localization/urls-and-fallback) — where string localization fits in the broader locale pipeline.
