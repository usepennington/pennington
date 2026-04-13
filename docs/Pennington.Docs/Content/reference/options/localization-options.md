---
title: LocalizationOptions
description: DefaultLocale, Locales, IsMultiLocale, and the URL helpers GetLocaleFromUrl, StripLocalePrefix, BuildLocaleUrl, GetAlternateLanguages.
section: options
order: 50
tags: []
uid: reference.options.localization-options
isDraft: true
search: false
llms: false
---

> **In this page.** `DefaultLocale`, `Locales`, `IsMultiLocale`, and the URL helpers `GetLocaleFromUrl`, `StripLocalePrefix`, `BuildLocaleUrl`, `GetAlternateLanguages`.
>
> **Not in this page.** Authoring translated content (see How-Tos).

## Summary

The options class that configures locale registry and URL math for Pennington's localization pipeline.
Namespace `Pennington.Infrastructure`; nested in `src/Pennington/Infrastructure/PenningtonOptions.cs` and exposed as `PenningtonOptions.Localization`.

## Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.LocalizationOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `DefaultLocale` | `string` | `"en"` | Locale code used when no known non-default prefix is present in the URL. |
| `IsMultiLocale` | `bool` | `false` | `true` when more than one locale has been registered via `AddLocale`. |
| `Locales` | `IReadOnlyDictionary<string, LocaleInfo>` | empty | Registered locale code to `LocaleInfo` map. |

## Methods

### `AddLocale(string, LocaleInfo)`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)
```

Registers a locale with an explicit `LocaleInfo`. Returns `void`. Overwrites any existing entry for `code`.

### `AddLocale(string, string)`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,System.String)
```

Registers a locale, constructing `LocaleInfo` from `displayName`. Returns `void`. Overwrites any existing entry for `code`.

### `GetLocaleFromUrl`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.GetLocaleFromUrl(System.String)
```

Returns `string`. Extracts the locale code from a URL path by matching the first segment against `Locales`. Returns `DefaultLocale` when the site is not multi-locale, when the first segment is empty, or when it is not a registered non-default locale.

### `StripLocalePrefix`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.StripLocalePrefix(System.String,System.String)
```

Returns `string`. Strips the `locale` prefix from `url`, returning the content-relative path with a leading `/`. Returns the URL unchanged when `locale` equals `DefaultLocale`. Returns `/` when the URL is exactly the locale segment.

### `BuildLocaleUrl`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.BuildLocaleUrl(System.String,System.String)
```

Returns `string`. Builds a full URL for `contentPath` in `locale`, omitting the locale prefix when `locale` equals `DefaultLocale`. Always emits a trailing slash.

### `GetAlternateLanguages`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.GetAlternateLanguages(System.String)
```

Returns `IReadOnlyList<AlternateLanguage>`. Produces one `AlternateLanguage` record per registered locale for `url`. Returns an empty list when `IsMultiLocale` is `false`. Pure URL math — does not check whether the target content exists. Normalizes `/index` to `/` and rewrites the 404-generator sentinel path to `/`.

## Supporting types

### `AlternateLanguage`

```csharp:xmldocid
T:Pennington.Infrastructure.AlternateLanguage
```

| Name | Type | Description |
|---|---|---|
| `Locale` | `string` | Locale code. |
| `DisplayName` | `string` | Human-readable locale name from `LocaleInfo.DisplayName`. |
| `HtmlLang` | `string` | Value for `hreflang` / `lang` attributes; falls back to `Locale` when `LocaleInfo.HtmlLang` is `null`. |
| `Url` | `string` | Locale-prefixed URL built via `BuildLocaleUrl`. |
| `IsCurrentLocale` | `bool` | `true` when this record matches the active locale. Default `false`. |

### `LocaleInfo`

```csharp:xmldocid
T:Pennington.Localization.LocaleInfo
```

| Name | Type | Default | Description |
|---|---|---|---|
| `DisplayName` | `string` | — | Human-readable locale name. |
| `Direction` | `string` | `"ltr"` | Text direction (`"ltr"` or `"rtl"`). |
| `HtmlLang` | `string?` | `null` | BCP 47 tag for `hreflang` / `lang` attributes; `null` falls back to the locale code. |

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- How-to: [Configure localization](/how-to/configuration/localization)
- Tutorial: [Add a locale](/tutorials/beyond-basics/add-a-locale)
