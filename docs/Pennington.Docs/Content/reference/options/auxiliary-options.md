---
title: "HighlightingOptions, IslandsOptions, SearchIndexOptions, LlmsTxtOptions, OutputOptions"
description: "Catalog of the remaining nested option bags on PenningtonOptions plus the build-time OutputOptions, each with properties, defaults, and registration methods."
uid: reference.options.auxiliary-options
order: 401070
sectionLabel: Configuration Options
tags: [options, configuration, reference]
---

Four nested bags exposed on `PenningtonOptions` and one build-time record constructed by `RunOrBuildAsync` from CLI args. `HighlightingOptions` and `IslandsOptions` are declared in `src/Pennington/Infrastructure/PenningtonOptions.cs`; `SearchIndexOptions` in `src/Pennington/Search/SearchIndexOptions.cs`; `LlmsTxtOptions` in `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`; `OutputOptions` in `src/Pennington/Generation/OutputOptions.cs`. MonorailCSS options are documented separately at [`MonorailCssOptions`](xref:reference.options.monorail-css-options).

## Overview

| Class | Reached via | One-sentence purpose |
|---|---|---|
| [`HighlightingOptions`](#highlightingoptions) | `PenningtonOptions.Highlighting` | Registry of `ICodeHighlighter` implementations consulted in registration order, with `PlainTextHighlighter` as fallback. |
| [`IslandsOptions`](#islandsoptions) | `PenningtonOptions.Islands` | Name-to-`IIslandRenderer`-type map used by the SPA pipeline to resolve `data-spa-island="name"` markers. |
| [`SearchIndexOptions`](#searchindexoptions) | `PenningtonOptions.SearchIndex` | Per-site search index tuning: a CSS selector scoping indexing to a page region and a default priority for indexed documents. |
| [`LlmsTxtOptions`](#llmstxtoptions) | `PenningtonOptions.AddLlmsTxt(...)` | Opt-in llms.txt generation; configure output directory, full-file toggle, and the CSS selector driving markdown extraction. |
| [`OutputOptions`](#outputoptions) | `OutputOptions.FromArgs(args)` inside `RunOrBuildAsync` | Build-time record describing output directory, base URL rewritten into links, and whether the target directory is cleared before writing. |

## `HighlightingOptions`

### Summary

The nested option bag exposed as `PenningtonOptions.Highlighting` that holds `ICodeHighlighter` instances consulted by `HighlightingService` in registration order. Declared in namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`; highlighter contracts live in `Pennington.Highlighting`.

### Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.HighlightingOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Highlighters` | `IReadOnlyList<ICodeHighlighter>` | empty | Read-only view of highlighter instances registered via `AddHighlighter`, in registration order. |

### Methods

#### `AddHighlighter<T>()`

```csharp:xmldocid
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter``1
```

Appends a new instance of `T` — which must implement `ICodeHighlighter` and expose a parameterless constructor — to `Highlighters`. For highlighters that require DI-resolved dependencies, use the instance overload instead.

#### `AddHighlighter(ICodeHighlighter)`

```csharp:xmldocid
M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter(Pennington.Highlighting.ICodeHighlighter)
```

Appends an already-constructed highlighter instance to `Highlighters`. Use when the highlighter requires constructor arguments the options surface cannot provide generically.

### Example

```csharp
builder.Services.AddPennington(penn =>
{
    penn.Highlighting.AddHighlighter<MyAsciiDocHighlighter>();
    penn.Highlighting.AddHighlighter(new ShellHighlighter());
    // ...
});
```

### See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)

## `IslandsOptions`

### Summary

The nested option bag exposed as `PenningtonOptions.Islands` that maps island names to `IIslandRenderer` types for the SPA-style partial rendering pipeline. Declared in namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`; `IIslandRenderer` contract and SPA wiring live in `Pennington.Islands`.

### Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.IslandsOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `RegisteredIslands` | `IReadOnlyDictionary<string, Type>` | empty | Read-only view of the name-to-renderer-type map, keyed by the string passed to `Register<T>`. |

### Methods

#### `Register<T>(string name)`

```csharp:xmldocid
M:Pennington.Infrastructure.IslandsOptions.Register``1(System.String)
```

Associates `name` with a renderer type `T` that must implement `IIslandRenderer`; the SPA pipeline resolves `data-spa-island="name"` markers to this type via DI. Re-registering the same `name` replaces the prior mapping.

### Example

```csharp
builder.Services.AddPennington(penn =>
{
    penn.Islands.Register<ChartIslandRenderer>("chart");
    // ...
});
```

### See also

- How-to: [Register an island renderer](xref:how-to.extensibility.island-renderer)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)

## `SearchIndexOptions`

### Summary

The per-site search-index tuning bag exposed as `PenningtonOptions.SearchIndex`, consumed by `SearchIndexBuilder` when it projects post-pipeline HTML into the per-locale index files served at `/search-index-{locale}.json`. Declared in namespace `Pennington.Search` at `src/Pennington/Search/SearchIndexOptions.cs`.

### Declaration

```csharp:xmldocid
T:Pennington.Search.SearchIndexOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | CSS selector identifying the main content element inside each rendered page (for example `#main-content`, `article`, `main`); the matched element's text is indexed. When `null`, the entire `<body>` is used. |
| `DefaultPriority` | `int` | `5` | Default ranking priority assigned to documents that do not set one explicitly via front matter or content-service metadata. |

### Note

Under `AddDocSite`, `ContentSelector` is pinned to `#main-content` via `DocSiteOptions.SearchIndexContentSelector` and cannot be set through this class directly. Use `search: false` front matter to opt a page out; drop to bare `AddPennington` when a custom selector is required.

### See also

- How-to: [Configure search indexing](xref:how-to.configuration.search)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)

## `LlmsTxtOptions`

### Summary

The opt-in configuration bag for `llms.txt` generation, materialized only after `PenningtonOptions.AddLlmsTxt(...)` is called and thereafter accessible via `PenningtonOptions.LlmsTxt`. Declared in namespace `Pennington.LlmsTxt` at `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`.

### Declaration

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | CSS selector identifying the main content element whose HTML is converted to markdown for the sidecar output; when `null`, the entire `<body>` is converted. |
| `GenerateFullFile` | `bool` | `false` | When `true`, emits `llms-full.txt` concatenating every eligible page's stripped markdown into a single document. |
| `OutputDirectory` | `string` | `"_llms"` | Site-root-relative directory under which per-page stripped-markdown files are written; the `llms.txt` index itself is served at `/llms.txt`. |

### Note

Registration is through `PenningtonOptions.AddLlmsTxt(Action<LlmsTxtOptions>)`, not by assigning the nested property directly; `PenningtonOptions.LlmsTxt` is `null` until `AddLlmsTxt` has been called. Under `AddDocSite`, `ContentSelector` is pinned via `DocSiteOptions.LlmsTxtContentSelector`.

### See also

- How-to: [Generate an llms.txt](xref:how-to.configuration.llms-txt)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)

## `OutputOptions`

### Summary

The build-time record passed to `OutputGenerationService.GenerateAsync` describing where the crawler writes static output, the base URL it rewrites into emitted links, and whether it clears the target directory first. Declared in namespace `Pennington.Generation` at `src/Pennington/Generation/OutputOptions.cs`; constructed inside `RunOrBuildAsync` via the static `FromArgs` factory, never wired through DI.

### Declaration

```csharp:xmldocid
T:Pennington.Generation.OutputOptions
```

Init-only record with one `required` property (`OutputDirectory`) and two optional ones; in practice every instance comes from `FromArgs`.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `UrlPath` | `new("/")` | URL prefix forwarded to `BaseUrlHtmlRewriter` so generated pages, assets, and anchors deploy under a sub-path; populated from the `--base-url` flag or second positional `build` arg. |
| `CleanOutput` | `bool` | `true` | When `true`, the generator clears `OutputDirectory` before writing new files. |
| `OutputDirectory` **(required)** | `FilePath` | — | Filesystem directory the static crawler writes to; populated from the `--output` flag or third positional `build` arg, defaulting to `output` when `FromArgs` synthesizes a value. |

### Methods

#### `FromArgs(string[] args)`

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

Parses a `build`-shaped argv into an `OutputOptions`. Returns a no-op default (`OutputDirectory = "output"`, `BaseUrl = "/"`) when `args[0]` is not the literal `"build"`, so dev runs, xunit invocations, and `dotnet watch` do not misread positional args. When `args[0]` is `"build"`, it accepts `--base-url` and `--output` (as `--flag value` or `--flag=value`) with positional fallback; named flags take precedence when both are present.

### See also

- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
