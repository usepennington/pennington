---
title: "TranslationOptions"
description: "In-memory per-locale string registry populated on PenningtonOptions.Translations and read by IStringLocalizer."
sectionLabel: "Configuration Options"
order: 401060
tags: [options, localization, translations, istringlocalizer]
uid: reference.options.translations
---

`TranslationOptions` is the in-memory per-locale key/value registry that backs Pennington's `IStringLocalizer` for UI string translations. It is declared in `Pennington.Localization`, exposed as `PenningtonOptions.Translations`, and populated inside the `AddPennington`, `AddDocSite`, or `AddBlogSite` configuration callback.

> **Note:** To enable multiple locales at the routing layer, see <xref:reference.options.localization-options>.

Sealed class backed by a `Dictionary<string, Dictionary<string, string>>` keyed by locale code (case-insensitive); locale buckets are created lazily on first `Add`.

## Methods

### `Add(string locale, string key, string value)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.String,System.String)
```

Registers a single translation under the given locale code and key, creating the locale bucket on first use and overwriting any prior value at the same key. Locale codes must match the codes registered on `LocalizationOptions.Locales`; keys are free-form and resolved at request time by `IStringLocalizer`.

### `Add(string locale, Dictionary<string, string> entries)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.Collections.Generic.Dictionary{System.String,System.String})
```

Bulk overload that applies the per-key `Add` for every entry in the supplied dictionary against a single locale. Existing keys under that locale are overwritten; keys absent from the dictionary are left untouched.

## Consuming translations

Registered entries are read at request time through `Microsoft.Extensions.Localization.IStringLocalizer`, which Pennington fulfils with an internal `PenningtonStringLocalizer` that maps `CultureInfo.CurrentUICulture` to a locale code, looks up the key, and falls back to `LocalizationOptions.DefaultLocale` then to the key name when no match is found.

## Example

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.TranslationRegistration.Register(Pennington.Localization.TranslationOptions)
```

## See also

- How-to: [Enable multiple locales](xref:how-to.configuration.localization)
- Related reference: [`LocalizationOptions`](xref:reference.options.localization-options)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Background: [Locale-aware URLs and content fallback](xref:explanation.localization.urls-and-fallback)
