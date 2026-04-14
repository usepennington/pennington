---
title: "TranslationOptions"
description: "In-memory per-locale string registry populated on PenningtonOptions.Translations and read by IStringLocalizer."
sectionLabel: "Configuration Options"
order: 60
tags: [options, localization, translations, istringlocalizer]
uid: reference.options.translations
---

> **In this page.** The `TranslationOptions.Add(locale, key, value)` / `Add(locale, dictionary)` overloads and how `PenningtonOptions.Translations` is populated; consuming code reads translations via `IStringLocalizer`, and the specific localizer implementation is internal wiring.
>
> **Not in this page.** Enabling multiple locales at the routing layer — see [`LocalizationOptions`](/reference/options/localization-options).

## Summary

_**One sentence: what it is.** The in-memory per-locale key/value registry that backs Pennington's `IStringLocalizer` for UI string translations._
_**One sentence: where it lives.** Declared in namespace `Pennington.Localization` at `src/Pennington/Localization/TranslationOptions.cs`; the single instance is exposed as `PenningtonOptions.Translations` and is populated inside the `AddPennington` / `AddDocSite` / `AddBlogSite` configuration callback._

## Declaration

```csharp:xmldocid
T:Pennington.Localization.TranslationOptions
```

_One sentence. Sealed class with a private `Dictionary<string, Dictionary<string, string>>` keyed by locale code (case-insensitive); new locale buckets are created lazily on first `Add`._

## Methods

### `Add(string locale, string key, string value)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.String,System.String)
```

_Two sentences. Registers a single translation under the given locale code and key, creating the locale bucket on first use and overwriting any prior value at the same key. Locale codes match the codes registered on `LocalizationOptions.Locales`; keys are free-form and resolved later by `IStringLocalizer`._

### `Add(string locale, Dictionary<string, string> entries)`

```csharp:xmldocid
M:Pennington.Localization.TranslationOptions.Add(System.String,System.Collections.Generic.Dictionary{System.String,System.String})
```

_Two sentences. Bulk overload that calls the per-key `Add` for every entry in the supplied dictionary against a single locale. Existing keys under that locale are overwritten; keys not present in the dictionary are left untouched._

## Consuming translations

_One sentence. Registered entries are read at request time through `Microsoft.Extensions.Localization.IStringLocalizer`, which Pennington fulfils with an internal `PenningtonStringLocalizer` that maps `CultureInfo.CurrentUICulture` to a locale code, looks up the key, and falls back to `LocalizationOptions.DefaultLocale` then to the key name when no match exists — see the localization how-to for the injection pattern._

## Example

_TODO: no `examples/` project currently exercises `TranslationOptions.Add`; the inline snippet in [how-to: Enable multiple locales](/how-to/configuration/localization) is the closest reference. When an example project is added, replace this block with a `csharp:xmldocid,bodyonly` fence pointing at the `Run` body that populates `options.Translations`._

## See also

- How-to: [Enable multiple locales](/how-to/configuration/localization)
- Related reference: [`LocalizationOptions`](/reference/options/localization-options)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Background: [Locale-aware URLs and content fallback](/explanation/localization/urls-and-fallback)
