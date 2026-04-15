---
title: "PenningtonOptions"
description: "Top-level configuration surface passed to AddPennington — site metadata, content sources, and nested subsystem option bags."
sectionLabel: "Configuration Options"
order: 401010
tags: [options, configuration, infrastructure]
uid: reference.options.pennington-options
---

The top-level options class supplied to `services.AddPennington(Action<PenningtonOptions>)`. Declared in namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`.

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Extra assemblies scanned for `@page` Razor components beyond the entry assembly. |
| `CanonicalBaseUrl` | `string?` | `null` | Absolute origin used to build canonical links, sitemap URLs, and RSS feed self-links. |
| `ConfigureMarkdownPipeline` | `Action<MarkdownPipelineBuilder, IServiceProvider>?` | `null` | Hook that runs after built-in Markdig extensions are registered, allowing additional Markdig extensions to be appended with access to the resolved `IServiceProvider`. |
| `ContentRootPath` | `string` | `"Content"` | Root directory (relative to the app content root) containing markdown sources and static assets. |
| `Highlighting` | `HighlightingOptions` | `new()` | Code highlighter registry — pointer-only; see [`HighlightingOptions`](xref:reference.options.auxiliary-options). |
| `Islands` | `IslandsOptions` | `new()` | Interactive island registration — pointer-only; see [`IslandsOptions`](xref:reference.options.auxiliary-options). |
| `LlmsTxt` | `LlmsTxtOptions?` | `null` (set by `AddLlmsTxt`) | `llms.txt` generation options — pointer-only; see [`LlmsTxtOptions`](xref:reference.options.auxiliary-options). |
| `Localization` | `LocalizationOptions` | `new()` | Locale registry and URL math — pointer-only; see [`LocalizationOptions`](xref:reference.options.localization-options). |
| `MarkdownSources` | `IReadOnlyList<MarkdownContentOptions>` | `[]` | Read-only view of the markdown sources registered via `AddMarkdownContent<TFrontMatter>`. |
| `SearchIndex` | `SearchIndexOptions` | `new()` | Client-side search index configuration — pointer-only; see [`SearchIndexOptions`](xref:reference.options.auxiliary-options). |
| `SiteDescription` | `string` | `""` | Human-readable site description emitted into feed metadata and `<meta>` tags. |
| `SiteTitle` | `string` | `""` | Human-readable site title emitted into feed metadata and layout chrome. |
| `Translations` | `TranslationOptions` | `new()` | String-resource registry backing `IStringLocalizer` — pointer-only; see [`TranslationOptions`](xref:reference.options.translations). |

### `AddMarkdownContent<TFrontMatter>`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})
```

Registers a markdown content source whose files are parsed with the supplied `IFrontMatter` implementation; returns the created `MarkdownContentOptions` for further configuration. See [`MarkdownContentOptions`](xref:reference.options.markdown-content-options).

### `AddLlmsTxt`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonOptions.AddLlmsTxt(System.Action{Pennington.LlmsTxt.LlmsTxtOptions})
```

Enables `llms.txt` and stripped-markdown generation and returns the backing `LlmsTxtOptions`; subsequent access is available via the `LlmsTxt` property.

## Example

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage2.Run(System.String[])
```

Shape of a minimal `AddPennington` call: set top-level metadata, then register one markdown source.

## See also

- How-to: [Register a markdown content source](xref:how-to.configuration.multiple-sources)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Related reference: [`MarkdownContentOptions`](xref:reference.options.markdown-content-options)
