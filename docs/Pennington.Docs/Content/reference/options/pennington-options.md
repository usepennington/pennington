---
title: PenningtonOptions
description: Every property on PenningtonOptions — SiteTitle, SiteDescription, CanonicalBaseUrl, ContentRootPath, Highlighting, Islands, Localization, Translations, SearchIndex, LlmsTxt, AdditionalRoutingAssemblies — with types and defaults.
section: options
order: 10
tags: []
uid: reference.options.pennington-options
isDraft: true
search: false
llms: false
---

> **In this page.** Every property on `PenningtonOptions` — `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, `Highlighting`, `Islands`, `Localization`, `Translations`, `SearchIndex`, `LlmsTxt`, `AdditionalRoutingAssemblies` — with types and defaults.
>
> **Not in this page.** Task recipes that use these options (see How-Tos).

## Summary

The top-level configuration class passed to `services.AddPennington(...)`.
Namespace `Pennington.Infrastructure`; defined in `src/Pennington/Infrastructure/PenningtonOptions.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.PenningtonOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Assemblies to scan for `@page` Razor components. |
| `CanonicalBaseUrl` | `string?` | `null` | Canonical absolute base URL used for feeds, sitemap, and structured data. |
| `ContentRootPath` | `string` | `"Content"` | Root filesystem path under which content sources are discovered. |
| `Highlighting` | `HighlightingOptions` | new instance | Registry of `ICodeHighlighter` implementations. See [Highlighting](#highlighting). |
| `Islands` | `IslandsOptions` | new instance | Registry of named `IIslandRenderer` types. See [Islands](#islands). |
| `LlmsTxt` | `LlmsTxtOptions?` | `null` | Populated when `AddLlmsTxt(...)` is called; otherwise disabled. See [LlmsTxt](#llmstxt). |
| `Localization` | `LocalizationOptions` | new instance | Locale registry and URL math. See [Localization](#localization). |
| `MarkdownSources` | `IReadOnlyList<MarkdownContentOptions>` | empty | Markdown content sources registered via `AddMarkdownContent<TFrontMatter>(...)`. |
| `SearchIndex` | `SearchIndexOptions` | new instance | Configuration for the per-locale search index. See [SearchIndex](#searchindex). |
| `SiteDescription` | `string` | `""` | Site description used in feeds, structured data, and social metadata. |
| `SiteTitle` | `string` | `""` | Site title used in feeds, structured data, and default page titles. |
| `Translations` | `TranslationOptions` | new instance | In-memory per-locale key/value translation dictionary. |

## Methods

### `AddMarkdownContent<TFrontMatter>`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})
```

Registers a markdown content source with a specific `IFrontMatter` type. Returns the configured `MarkdownContentOptions`; also appended to `MarkdownSources`.

### `AddLlmsTxt`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonOptions.AddLlmsTxt(System.Action{Pennington.LlmsTxt.LlmsTxtOptions})
```

Enables llms.txt generation and returns the configured `LlmsTxtOptions`. Sets the `LlmsTxt` property.

## Highlighting

Type: `HighlightingOptions` (nested in `PenningtonOptions.cs`).

| Name | Type | Default | Description |
|---|---|---|---|
| `Highlighters` | `IReadOnlyList<ICodeHighlighter>` | empty | Registered highlighters in registration order. |

### Methods

| Name | Signature | Description |
|---|---|---|
| `AddHighlighter<T>` | `void AddHighlighter<T>() where T : ICodeHighlighter, new()` | Instantiates and registers `T`. |
| `AddHighlighter` | `void AddHighlighter(ICodeHighlighter highlighter)` | Registers an existing instance. |

## Islands

Type: `IslandsOptions` (nested in `PenningtonOptions.cs`).

| Name | Type | Default | Description |
|---|---|---|---|
| `RegisteredIslands` | `IReadOnlyDictionary<string, Type>` | empty | Island name to `IIslandRenderer` type map. |

### Methods

| Name | Signature | Description |
|---|---|---|
| `Register<T>` | `void Register<T>(string name) where T : IIslandRenderer` | Registers type `T` under `name`. |

## Localization

Type: `LocalizationOptions` (nested in `PenningtonOptions.cs`).

| Name | Type | Default | Description |
|---|---|---|---|
| `DefaultLocale` | `string` | `"en"` | Locale code used when no prefix is present in the URL. |
| `IsMultiLocale` | `bool` | `false` | `true` when more than one locale is configured. |
| `Locales` | `IReadOnlyDictionary<string, LocaleInfo>` | empty | Registered locale code to `LocaleInfo` map. |

### Methods

| Name | Signature | Description |
|---|---|---|
| `AddLocale` | `void AddLocale(string code, LocaleInfo info)` | Registers a locale with an explicit `LocaleInfo`. |
| `AddLocale` | `void AddLocale(string code, string displayName)` | Registers a locale, constructing `LocaleInfo` from `displayName`. |
| `GetLocaleFromUrl` | `string GetLocaleFromUrl(string url)` | Returns the locale code for a URL, or `DefaultLocale` when no known non-default prefix matches. |
| `StripLocalePrefix` | `string StripLocalePrefix(string url, string locale)` | Returns the URL with the locale prefix removed; unchanged for the default locale. |
| `BuildLocaleUrl` | `string BuildLocaleUrl(string contentPath, string locale)` | Builds a full URL for a content path in the specified locale. |
| `GetAlternateLanguages` | `IReadOnlyList<AlternateLanguage> GetAlternateLanguages(string url)` | Returns `AlternateLanguage` records for every configured locale. |

## Translations

Type: `TranslationOptions` — see `src/Pennington/Localization/TranslationOptions.cs`.

In-memory per-locale key/value dictionary backing `PenningtonStringLocalizer`. Documented on its own reference page.

## SearchIndex

Type: `SearchIndexOptions` — see `src/Pennington/Search/SearchIndexOptions.cs`.

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | (see source) | (see source) | CSS selector identifying the body region to index. |
| `DefaultPriority` | (see source) | (see source) | Default priority applied when a source does not set one. |

Full definition documented on its own reference page.

## LlmsTxt

Type: `LlmsTxtOptions?` — see `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`. `null` until `AddLlmsTxt(...)` is called.

| Name | Type | Default | Description |
|---|---|---|---|
| `OutputDirectory` | `string` | `"_llms"` | Directory under the output root for per-page markdown sidecars. |
| `GenerateFullFile` | `bool` | (see source) | Whether to emit the concatenated `llms-full.txt`. |
| `ContentSelector` | (see source) | (see source) | CSS selector identifying the body region to strip. |

Full definition documented on its own reference page.

## MarkdownSources

Type: `IReadOnlyList<MarkdownContentOptions>`. Populated via `AddMarkdownContent<TFrontMatter>(...)`.

### `MarkdownContentOptions`

| Name | Type | Default | Description |
|---|---|---|---|
| `BasePageUrl` | `string` | `"/"` | URL prefix applied to pages from this source. |
| `ContentPath` | `string` | `"Content"` | Filesystem path for the source's markdown files. |
| `ExcludePaths` | `ImmutableArray<string>` | empty | Relative subpaths (from `ContentPath`) to skip during discovery and content copying. |
| `Section` | `string?` | `null` | Default `Section` applied to pages from this source. |

Full definition documented on its own reference page.

## See also

- Related reference: [`DocSiteOptions`](/reference/options/doc-site-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blog-site-options)
- Related reference: [`MarkdownContentOptions`](/reference/options/markdown-content-options)
