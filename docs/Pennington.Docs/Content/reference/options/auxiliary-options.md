---
title: "Auxiliary options classes"
description: "The remaining option classes on PenningtonOptions with properties, defaults, and what each controls."
section: "options"
order: 60
tags: []
uid: reference.options.auxiliary-options
isDraft: true
search: false
llms: false
---

> **In this page.** The remaining option classes on `PenningtonOptions` with properties, defaults, and what each controls.
>
> **Not in this page.** MonorailCSS options (separate page).

## Summary

Five auxiliary options classes configure highlighting, islands, search indexing, llms.txt generation, and static build output.
`HighlightingOptions` and `IslandsOptions` live in `Pennington.Infrastructure` nested under `PenningtonOptions`; `SearchIndexOptions` in `Pennington.Search`; `LlmsTxtOptions` in `Pennington.LlmsTxt`; `OutputOptions` in `Pennington.Generation`.

## `HighlightingOptions`

```csharp:xmldocid
T:Pennington.Infrastructure.HighlightingOptions
```

| Name | Type | Default | Description |
|---|---|---|---|
| `AddHighlighter<T>()` | `void` method where `T : ICodeHighlighter, new()` | — | Instantiates `T` and appends it to the registered highlighters. |
| `AddHighlighter(ICodeHighlighter)` | `void` method | — | Appends the supplied highlighter instance to the registered highlighters. |
| `Highlighters` | `IReadOnlyList<ICodeHighlighter>` | empty list | Registered highlighter instances, in registration order. |

## `IslandsOptions`

```csharp:xmldocid
T:Pennington.Infrastructure.IslandsOptions
```

| Name | Type | Default | Description |
|---|---|---|---|
| `Register<T>(string name)` | `void` method where `T : IIslandRenderer` | — | Registers renderer type `T` under the given island name. |
| `RegisteredIslands` | `IReadOnlyDictionary<string, Type>` | empty dictionary | Map of island name to renderer type. |

## `SearchIndexOptions`

```csharp:xmldocid
T:Pennington.Search.SearchIndexOptions
```

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | CSS selector for the main content element in rendered HTML; when null, the entire `<body>` is indexed. |
| `DefaultPriority` | `int` | `5` | Priority assigned to indexed documents. |

## `LlmsTxtOptions`

```csharp:xmldocid
T:Pennington.LlmsTxt.LlmsTxtOptions
```

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string?` | `null` | CSS selector for the main content element converted to markdown; when null, the entire `<body>` is used. |
| `GenerateFullFile` | `bool` | `false` | When true, also emits `llms-full.txt` with all content concatenated. |
| `OutputDirectory` | `string` | `"_llms"` | Output directory for raw markdown files, relative to site root. |

## `OutputOptions`

```csharp:xmldocid
T:Pennington.Generation.OutputOptions
```

| Name | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `UrlPath` | `new UrlPath("/")` | Base URL prepended to generated links. |
| `CleanOutput` | `bool` | `true` | When true, clears `OutputDirectory` before writing. |
| `OutputDirectory` | `FilePath` | required (no default) | Filesystem directory the static build writes to. |
| `FromArgs(string[] args)` | `static OutputOptions` method | — | Parses the `build [baseUrl] [output]` CLI shape; returns a no-op `OutputOptions` when `args[0]` is not `"build"`. |

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Related reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
