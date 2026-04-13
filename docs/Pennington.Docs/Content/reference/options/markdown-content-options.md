---
title: MarkdownContentOptions<T>
description: "`ContentPath`, `BasePageUrl`, `Section`, `ExcludePaths`, and their interaction with multi-source setups."
section: options
order: 40
tags: []
uid: reference.options.markdown-content-options
isDraft: true
search: false
llms: false
---

> **In this page.** `ContentPath`, `BasePageUrl`, `Section`, `ExcludePaths`, and their interaction with multi-source setups.
>
> **Not in this page.** The content-pipeline interfaces that consume the options.

## Summary

- One sentence — what it is: the options record passed to `PenningtonOptions.AddMarkdownContent<TFrontMatter>(...)` describing a single markdown content source.
- One sentence — where it lives: class `Pennington.Infrastructure.MarkdownContentOptions` in `src/Pennington/Infrastructure/PenningtonOptions.cs`; the `<T>` in the page title refers to the generic parameter on `AddMarkdownContent<TFrontMatter>` that binds this options instance to a front-matter type.

## Declaration

- xmldocid fence: `T:Pennington.Infrastructure.MarkdownContentOptions`

```csharp:xmldocid
T:Pennington.Infrastructure.MarkdownContentOptions
```

## Properties

Alphabetical. Verified against `src/Pennington/Infrastructure/PenningtonOptions.cs` (class `MarkdownContentOptions`, lines 58–73).

| Name | Type | Default | Description |
|---|---|---|---|
| `BasePageUrl` | `string` | `"/"` | URL prefix applied to every page produced by this source. |
| `ContentPath` | `string` | `"Content"` | Filesystem path (relative to `ContentRootPath`) that this source scans for markdown files. |
| `ExcludePaths` | `ImmutableArray<string>` | `ImmutableArray<string>.Empty` | Relative subpaths under `ContentPath` to skip during discovery and content copying; set on a catch-all source to carve out a subtree owned by a more specific source. |
| `Section` | `string?` | `null` | Default value for `ISectionable.Section` applied to pages from this source when front matter does not set one. |

Not listed: `FrontMatterType` is `internal` (set by `AddMarkdownContent<TFrontMatter>`); omitted from the public surface.

## Registration

- Entry point — `PenningtonOptions.AddMarkdownContent<TFrontMatter>(Action<MarkdownContentOptions> configure)`.
- `TFrontMatter` constraint — `Pennington.FrontMatter.IFrontMatter`.
- Return value — the configured `MarkdownContentOptions`; also appended to `PenningtonOptions.MarkdownSources`.

xmldocid fence:

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})
```

## Multi-source interaction

Lookup table for how the four properties combine when multiple `AddMarkdownContent<T>` calls are registered.

| Property | Effect across multiple sources |
|---|---|
| `ContentPath` | Each source scans its own tree independently; overlapping trees are detected by `MarkdownSourceOverlapDetector` and surfaced as diagnostics. |
| `BasePageUrl` | Each source prefixes its page URLs with its own value; two sources with the same `BasePageUrl` produce colliding routes. |
| `Section` | Applied per source; pages carry the source's default `Section` unless their front matter overrides it. |
| `ExcludePaths` | Matching semantics defined on `MarkdownContentServiceOptions.ExcludePaths`; intended use is to carve a subtree out of a broad catch-all source so a narrower sibling source can own it. |

Related type — `MarkdownContentServiceOptions` in `src/Pennington/Content/MarkdownContentServiceOptions.cs` is the internal per-service shape (`FilePattern`, `Locale`, `SearchPriority`) derived from these public options; documented on its own reference page.

## Example

- Source — `examples/MultipleContentSourceExample/Program.cs` (three `AddMarkdownContent<T>` calls: catch-all with `ExcludePaths`, blog subtree, docs subtree).

```csharp:xmldocid,bodyonly
M:MultipleContentSourceExample.Program.<Main>$(System.String[])
```

## See also

- How-to: [Use multiple content sources](/how-to/configuration/multiple-sources)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Related reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
