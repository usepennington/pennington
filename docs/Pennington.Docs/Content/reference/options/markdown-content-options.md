---
title: "MarkdownContentOptions<T>"
description: "Per-source configuration record passed to PenningtonOptions.AddMarkdownContent<TFrontMatter> — content root, URL prefix, section label, and excluded subpaths."
sectionLabel: "Configuration Options"
order: 401040
tags: [options, markdown, content-sources, configuration]
uid: reference.options.markdown-content-options
---

`MarkdownContentOptions` is the per-source options record supplied to `PenningtonOptions.AddMarkdownContent<TFrontMatter>`, describing one markdown content root. It is declared in namespace `Pennington.Infrastructure`; the internal sibling `MarkdownContentServiceOptions` in `Pennington.Content` extends it with `FilePattern`, `Locale`, and `SearchPriority` when the public options flow into `MarkdownContentService<T>`.

## Declaration

```csharp:xmldocid
T:Pennington.Infrastructure.MarkdownContentOptions
```

Each call to `AddMarkdownContent<TFrontMatter>` constructs one instance, stamps the generic argument onto the internal `FrontMatterType`, and appends it to `PenningtonOptions.MarkdownSources`.

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `BasePageUrl` | `string` | `"/"` | URL prefix under which every file discovered at `ContentPath` is published, combined with the relative file path minus its extension to form each page's canonical route. |
| `ContentPath` | `string` | `"Content"` | Filesystem directory, relative to the app's content root, scanned for markdown files during discovery and asset copying. |
| `ExcludePaths` | `ImmutableArray<string>` | `ImmutableArray<string>.Empty` | Forward-slash relative subpaths (from `ContentPath`) whose subtrees are skipped during discovery; matching is case-insensitive and segment-based, so `"a/b"` excludes `a/b` and everything beneath it but not `a/bcd`. |
| `SectionLabel` | `string?` | `null` | Default navigation section heading applied to pages from this source when their front matter does not supply an `ISectionable.Section` override. |

### Interaction with multiple sources

Each `AddMarkdownContent<T>` call appends one entry to `PenningtonOptions.MarkdownSources`; `MarkdownSourceOverlapDetector` emits a startup warning when two sources' `ContentPath` values nest without `ExcludePaths` carving the inner tree out of the broader one. `BasePageUrl` values are not checked for URL-space collisions — two sources with disjoint `ContentPath`s may still publish to overlapping URLs if their prefixes are configured that way. The per-source `FrontMatterType`, set via the generic argument on `AddMarkdownContent<TFrontMatter>`, determines which `IFrontMatter` implementation parses each file, so two sources with different front-matter types can coexist on the same host.

## Example

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

Registers a single markdown source with `DocFrontMatter`, `ContentPath = "Content"`, and `BasePageUrl = "/"`; chained calls add more sources with distinct paths and prefixes.

## See also

- How-to: [Use multiple content sources](xref:how-to.configuration.multiple-sources)
- Related reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)
