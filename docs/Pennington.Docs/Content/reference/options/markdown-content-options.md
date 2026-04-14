---
title: "MarkdownContentOptions<T>"
description: "Per-source configuration record passed to PenningtonOptions.AddMarkdownContent<TFrontMatter> — content root, URL prefix, section label, and excluded subpaths."
sectionLabel: "Configuration Options"
order: 40
tags: [options, markdown, content-sources, configuration]
uid: reference.options.markdown-content-options
---

> **In this page.** _One sentence. The four public properties on `MarkdownContentOptions` — `ContentPath`, `BasePageUrl`, `SectionLabel`, `ExcludePaths` — with types and defaults, plus how they combine when more than one markdown source is registered._
>
> **Not in this page.** _One sentence. The content-pipeline interfaces (`IContentService`, `IContentParser`, `IContentRenderer`) that consume these options — those are documented on their own reference pages under `/reference/core/`._

## Summary

_**One sentence: what it is.** The per-source options record supplied to `PenningtonOptions.AddMarkdownContent<TFrontMatter>`, describing one markdown content root._
_**One sentence: where it lives.** Declared in namespace `Pennington.Infrastructure` at `src/Pennington/Infrastructure/PenningtonOptions.cs`; the internal sibling `MarkdownContentServiceOptions` in `Pennington.Content` adds `FilePattern`, `Locale`, and `SearchPriority` when the public options flow into `MarkdownContentService<T>`._

## Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.MarkdownContentOptions
```

_One sentence. Each call to `AddMarkdownContent<TFrontMatter>` constructs one instance, stamps the generic argument onto the internal `FrontMatterType`, and appends it to `PenningtonOptions.MarkdownSources`._

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `BasePageUrl` | `string` | `"/"` | _One sentence. URL prefix under which every file discovered at `ContentPath` is published — combined with the relative file path minus extension to form each page's canonical route._ |
| `ContentPath` | `string` | `"Content"` | _One sentence. Filesystem directory (relative to the app's content root) scanned for markdown files during discovery and asset copying._ |
| `ExcludePaths` | `ImmutableArray<string>` | `ImmutableArray<string>.Empty` | _One to two sentences. Forward-slash relative subpaths (from `ContentPath`) whose subtrees are skipped during discovery and content copying; matching is case-insensitive and segment-based, so `"a/b"` excludes `a/b` and everything beneath it but not `a/bcd`. Set on a broad catch-all source when a narrower source registered nearby owns a carved-out subtree._ |
| `SectionLabel` | `string?` | `null` | _One sentence. Default navigation section heading applied to pages from this source when their front matter does not supply an `ISectionable.Section` override._ |

### Interaction with multiple sources

_Two to four sentences. Each `AddMarkdownContent<T>` call adds one more entry to `PenningtonOptions.MarkdownSources`; `MarkdownSourceOverlapDetector` emits a startup warning when two sources' `ContentPath` values nest without `ExcludePaths` carving the inner tree out of the broader one. `BasePageUrl` values are not checked for URL-space collisions — two sources whose `ContentPath`s are disjoint may still publish to overlapping URLs if their `BasePageUrl` prefixes are configured that way. The per-source `FrontMatterType` (set via the generic argument on `AddMarkdownContent<TFrontMatter>`) determines which `IFrontMatter` implementation parses each file, so two sources with different front-matter types can coexist on the same host._

## Example

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

_One sentence. Registers a single markdown source with `DocFrontMatter`, `ContentPath = "Content"`, and `BasePageUrl = "/"` — the minimal shape; chained calls add more sources with distinct paths and prefixes._

## See also

- How-to: [Use multiple content sources](xref:how-to.configuration.multiple-sources)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
