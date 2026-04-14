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

Type: `HighlightingOptions`. See [Auxiliary options classes](/reference/options/auxiliary-options) for the full member listing.

## Islands

Type: `IslandsOptions`. See [Auxiliary options classes](/reference/options/auxiliary-options) for the full member listing.

## Localization

Type: `LocalizationOptions`. See [`LocalizationOptions`](/reference/options/localization-options) for the full member listing.

## Translations

Type: `TranslationOptions`. See [`TranslationOptions`](/reference/options/translations) for the full member listing.

## SearchIndex

Type: `SearchIndexOptions`. See [Auxiliary options classes](/reference/options/auxiliary-options) for the full member listing.

## LlmsTxt

Type: `LlmsTxtOptions?`. See [Auxiliary options classes](/reference/options/auxiliary-options) for the full member listing.

## MarkdownSources

Type: `IReadOnlyList<MarkdownContentOptions>`. Populated via `AddMarkdownContent<TFrontMatter>(...)`. See [MarkdownContentOptions<T>](/reference/options/markdown-content-options) for property details.

## See also

- Related reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- Related reference: [`MarkdownContentOptions`](/reference/options/markdown-content-options)
