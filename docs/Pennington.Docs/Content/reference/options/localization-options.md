---
title: "LocalizationOptions"
description: "Locale registry and URL math for multi-locale Pennington sites — default locale, registered locales, and the URL helpers that drive routing and language switchers."
sectionLabel: "Configuration Options"
order: 401050
tags: [options, localization, routing, urls]
uid: reference.options.localization-options
---

The options bag that registers locales and computes locale-prefixed URLs for a Pennington site. Exposed as `PenningtonOptions.Localization`; declared in namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.LocalizationOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `DefaultLocale` | `string` | `"en"` | Locale code that serves at the URL root with no prefix; all other registered locales are served under `/<code>/`. |
| `IsMultiLocale` | `bool` | `false` | Returns `true` when more than one locale has been registered; the middleware pipeline short-circuits locale detection and URL rewriting when this is `false`. |
| `Locales` | `IReadOnlyDictionary<string, LocaleInfo>` | empty | Read-only view of registered locales keyed by locale code, populated exclusively through the `AddLocale` overloads. |

## Methods

### `AddLocale(string, LocaleInfo)`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)
```

Registers a locale with explicit `LocaleInfo` metadata (`DisplayName`, `Direction`, `HtmlLang`). Overwrites any existing entry for the same code. Callers observe the registration via the `Locales` dictionary.

### `AddLocale(string, string)`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,System.String)
```

Convenience overload that constructs a `LocaleInfo` from a display name using default direction (`"ltr"`) and no explicit `HtmlLang`. Overwrites any existing entry for the same code.

### `GetLocaleFromUrl`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.GetLocaleFromUrl(System.String)
```

Extracts the locale code from the first path segment of `url`, returning that code only when it matches a registered non-default locale. Returns `DefaultLocale` in every other case, including single-locale configurations and default-locale URLs that carry no prefix.

### `StripLocalePrefix`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.StripLocalePrefix(System.String,System.String)
```

Removes the `/<locale>/` prefix from `url` and returns the content-relative path. Returns the URL unchanged when `locale` equals `DefaultLocale` (default-locale URLs carry no prefix); a bare `/<locale>` with no trailing segment collapses to `"/"`.

### `BuildLocaleUrl`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.BuildLocaleUrl(System.String,System.String)
```

Builds a canonical site URL for `contentPath` under the given `locale`, returning `/<path>/` for the default locale and `/<locale>/<path>/` otherwise. Empty content paths collapse to the locale landing page (`/` or `/<locale>/`).

### `GetAlternateLanguages`

```csharp:xmldocid
M:Pennington.Infrastructure.LocalizationOptions.GetAlternateLanguages(System.String)
```

Returns one `AlternateLanguage` entry per registered locale for the same content path, used by language switchers and hreflang emitters. Returns an empty list when `IsMultiLocale` is `false`; performs pure URL math and does not verify whether the target content exists.

## Example

```csharp:xmldocid,bodyonly
M:BeyondLocaleExample.Stage2.Run(System.String[])
```

Demonstrates a `ConfigureLocalization` action that sets `DefaultLocale` and registers two locales via `AddLocale`.

## See also

- How-to: [Enable multiple locales](xref:how-to.configuration.localization)
- Related reference: [`TranslationOptions`](xref:reference.options.translations)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Background: [Locale-aware URLs and content fallback](xref:explanation.localization.urls-and-fallback)
