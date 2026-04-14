---
title: "HighlightingOptions, IslandsOptions, SearchIndexOptions, LlmsTxtOptions, OutputOptions"
description: "Catalog of the remaining nested option bags on PenningtonOptions plus the build-time OutputOptions, each with properties, defaults, and registration methods."
uid: reference.options.auxiliary-options
order: 70
sectionLabel: Configuration Options
tags: [options, configuration, reference]
---

> **In this page.** _One sentence. Five option classes reached from `PenningtonOptions` or the `build` CLI — `HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions` — with every public property, default, and registration method._
>
> **Not in this page.** _One sentence. MonorailCSS options live on their own page — see [`MonorailCssOptions`](/reference/options/monorail-css-options)._

## Summary

_**One sentence: what this page is.** Lookup table for the five smaller option classes bundled together because each is too narrow for its own page — four are nested bags exposed on `PenningtonOptions`, the fifth is the build-time record that `RunOrBuildAsync` constructs from CLI args._
_**One sentence: where they live.** `HighlightingOptions` and `IslandsOptions` are declared in `src/Pennington/Infrastructure/PenningtonOptions.cs`; `SearchIndexOptions` in `src/Pennington/Search/SearchIndexOptions.cs`; `LlmsTxtOptions` in `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`; `OutputOptions` in `src/Pennington/Generation/OutputOptions.cs`._

## Overview

| Class | Reached via | One-sentence purpose |
|---|---|---|
| [`HighlightingOptions`](#highlightingoptions) | `PenningtonOptions.Highlighting` | _One sentence: registry of `ICodeHighlighter` implementations consulted in registration order with `PlainTextHighlighter` as fallback._ |
| [`IslandsOptions`](#islandsoptions) | `PenningtonOptions.Islands` | _One sentence: name → `IIslandRenderer` type map used by the SPA pipeline to resolve `data-spa-island="name"` markers to a renderer._ |
| [`SearchIndexOptions`](#searchindexoptions) | `PenningtonOptions.SearchIndex` | _One sentence: per-site search index tuning — content selector that scopes indexing to a page region, plus default priority assigned to indexed documents._ |
| [`LlmsTxtOptions`](#llmstxtoptions) | `PenningtonOptions.AddLlmsTxt(...)` | _One sentence: opt-in llms.txt generation — enable with `AddLlmsTxt`, then configure output directory, full-file toggle, and the selector driving markdown extraction._ |
| [`OutputOptions`](#outputoptions) | `OutputOptions.FromArgs(args)` inside `RunOrBuildAsync` | _One sentence: build-time record describing where the static crawler writes output, the base URL it rewrites into links, and whether it wipes the target directory first._ |

## `HighlightingOptions`

### Summary

_**One sentence: what it is.** The nested option bag exposed as `PenningtonOptions.Highlighting` that holds the registered `ICodeHighlighter` instances consulted by `HighlightingService` in registration order._
_**One sentence: where it lives.** Namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`; the highlighter contracts themselves live in `Pennington.Highlighting`._

### Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.HighlightingOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Highlighters` | `IReadOnlyList<ICodeHighlighter>` | empty | _One sentence: read-only view of the highlighter instances registered via `AddHighlighter`, in the order they were added._ |

### Methods

#### `AddHighlighter<T>()`

```csharp:xmldocid
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter``1
```

_Two sentences: appends a new instance of `T` — which must implement `ICodeHighlighter` and expose a parameterless constructor — to `Highlighters`. Intended for stateless highlighters; for highlighters that require DI-resolved dependencies, use the instance overload instead._

#### `AddHighlighter(ICodeHighlighter)`

```csharp:xmldocid
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter(Pennington.Highlighting.ICodeHighlighter)
```

_Two sentences: appends an already-constructed highlighter instance to `Highlighters`. Use this when the highlighter needs constructor arguments the options surface cannot provide generically._

### Example

```csharp:xmldocid,bodyonly
M:Program.$Main(System.String[])
```

_One sentence: TODO — pull a focused `penn.Highlighting.AddHighlighter(...)` snippet via xmldocid once `ExtensibilityLabExample/Program.cs` is addressable (currently top-level statements); fall back to `:path` fence of `examples/ExtensibilityLabExample/Program.cs` if the symbol cannot be named._

### See also

- How-to: [Add a custom syntax highlighter](/how-to/extensibility/custom-highlighter)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)

## `IslandsOptions`

### Summary

_**One sentence: what it is.** The nested option bag exposed as `PenningtonOptions.Islands` that maps island names to `IIslandRenderer` types for the SPA-style partial rendering pipeline._
_**One sentence: where it lives.** Namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`; the `IIslandRenderer` contract and SPA wiring live in `Pennington.Islands`._

### Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.IslandsOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `RegisteredIslands` | `IReadOnlyDictionary<string, Type>` | empty | _One sentence: read-only view of the name → renderer type map, keyed by the string passed to `Register<T>` and valued as the `Type` of the island renderer._ |

### Methods

#### `Register<T>(string name)`

```csharp:xmldocid
M:Pennington.Infrastructure.IslandsOptions.Register``1(System.String)
```

_Two to three sentences: associates `name` with a renderer type `T` that must implement `IIslandRenderer`; the SPA pipeline later resolves `data-spa-island="name"` markers to this type via DI. Re-registering the same `name` replaces the prior mapping._

### Example

```csharp:xmldocid,bodyonly
M:Program.$Main(System.String[])
```

_One sentence: TODO — same blocker as `HighlightingOptions` (top-level-statements `Program.cs` in `ExtensibilityLabExample`); the line to pull is `penn.Islands.Register<ChartIslandRenderer>("chart")`. Use a `:path` fence if xmldocid cannot name it._

### See also

- How-to: [Register an island renderer](/how-to/extensibility/island-renderer)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)

## `SearchIndexOptions`

### Summary

_**One sentence: what it is.** The per-site search-index tuning bag exposed as `PenningtonOptions.SearchIndex`, consumed by `SearchIndexBuilder` when it projects post-pipeline HTML into the per-locale index files served at `/search-index-{locale}.json`._
_**One sentence: where it lives.** Namespace `Pennington.Search` at `src/Pennington/Search/SearchIndexOptions.cs`._

### Declaration

```csharp:xmldocid
T:Pennington.Search.SearchIndexOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | _One to two sentences: CSS selector identifying the main content element inside each rendered page (e.g. `#main-content`, `article`, `main`); the matched element's text becomes the indexed body. When `null`, the entire `<body>` is used._ |
| `DefaultPriority` | `int` | `5` | _One sentence: default ranking priority assigned to documents whose source does not set one explicitly via front-matter or content-service metadata._ |

### Note

_One to two sentences: under `AddDocSite` this selector is pinned to `#main-content` via `DocSiteOptions.SearchIndexContentSelector` and is not reached through this class directly — teach `search: false` front matter as the DocSite-idiomatic opt-out and drop to bare `AddPennington` when a custom selector is required._

### See also

- How-to: [Configure search indexing](/how-to/configuration/search)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Background: [When is DocSite the right starting point?](/explanation/core/docsite-positioning)

## `LlmsTxtOptions`

### Summary

_**One sentence: what it is.** The opt-in configuration bag for `llms.txt` generation, materialized only after `PenningtonOptions.AddLlmsTxt(...)` is called and thereafter accessible via `PenningtonOptions.LlmsTxt`._
_**One sentence: where it lives.** Namespace `Pennington.LlmsTxt` at `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`; enable via the `AddLlmsTxt` method on `PenningtonOptions`._

### Declaration

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | _One to two sentences: CSS selector identifying the main content element whose HTML is converted to markdown for the stripped-markdown sidecar output; when `null`, the entire `<body>` is converted._ |
| `GenerateFullFile` | `bool` | `false` | _One sentence: when `true`, emits an additional `llms-full.txt` concatenating every eligible page's stripped markdown into a single document._ |
| `OutputDirectory` | `string` | `"_llms"` | _One sentence: site-root-relative directory under which the per-page stripped-markdown files are written (the `llms.txt` index itself is served at `/llms.txt`)._ |

### Note

_One to two sentences: registration is through `PenningtonOptions.AddLlmsTxt(Action<LlmsTxtOptions>)`, not by assigning the nested property; the `LlmsTxt` property on `PenningtonOptions` is `null` until `AddLlmsTxt` has been called. Under `AddDocSite`, the selector is pinned via `DocSiteOptions.LlmsTxtContentSelector` — mirror the DocSite search story._

### See also

- How-to: [Generate an llms.txt](/how-to/configuration/llms-txt)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Background: [When is DocSite the right starting point?](/explanation/core/docsite-positioning)

## `OutputOptions`

### Summary

_**One sentence: what it is.** The build-time record passed to `OutputGenerationService.GenerateAsync` describing where the crawler writes static output, the base URL it rewrites into emitted links, and whether it wipes the target directory first._
_**One sentence: where it lives.** Namespace `Pennington.Generation` at `src/Pennington/Generation/OutputOptions.cs`; constructed inside `RunOrBuildAsync` via the static `FromArgs(args)` factory, never wired through DI._

### Declaration

```csharp:xmldocid
T:Pennington.Generation.OutputOptions
```

_One sentence: init-only record with one `required` property (`OutputDirectory`) and two optional ones; every instance comes from `FromArgs` in practice._

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `UrlPath` | `new("/")` | _One to two sentences: URL prefix forwarded to `BaseUrlHtmlRewriter` so generated pages, assets, and anchors deploy under a sub-path; the second positional `build` arg or `--base-url` flag populates it._ |
| `CleanOutput` | `bool` | `true` | _One sentence: when `true`, the generator clears `OutputDirectory` before writing new files so stale outputs are not retained between builds._ |
| `OutputDirectory` **(required)** | `FilePath` | — | _One sentence: filesystem directory the static crawler writes to; the third positional `build` arg or `--output` flag populates it, defaulting to `output` when `FromArgs` has to synthesize a value._ |

### Methods

#### `FromArgs(string[] args)`

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

_Three to four sentences: parses a `build`-shaped argv into an `OutputOptions`. Returns a no-op default (`OutputDirectory = "output"`, `BaseUrl = "/"`) whenever `args[0]` is not the literal `"build"`, so dev runs, xunit invocations, and `dotnet watch` do not misread positional args. When `args[0]` is `"build"`, it accepts `--base-url` and `--output` (as `--flag value` or `--flag=value`) with positional fallback, and named flags win when both appear._

### See also

- How-to: [Build a static site](/how-to/deployment/static-build)
- How-to: [Host under a sub-path (base URL)](/how-to/deployment/base-url)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
