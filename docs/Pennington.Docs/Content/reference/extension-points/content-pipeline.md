---
title: "Content pipeline interfaces"
description: "The five pipeline contracts — IContentService, IContentParser, IContentRenderer, IContentPipeline — together with the ContentItem and ContentSource union types and every case."
sectionLabel: "Extension Points"
order: 10
tags: [pipeline, content-service, unions, extension-points]
uid: reference.extension-points.content-pipeline
---

> **In this page.** `IContentService`, `IContentParser`, `IContentRenderer`, `IContentPipeline`, and the `ContentItem`/`ContentSource` union types with every case.
>
> **Not in this page.** Implementation examples — see the how-to [Implement a custom `IContentService`](/how-to/extensibility/custom-content-service).

## Summary

_**One sentence: what it is.** The four interfaces and two union types that make up Pennington's content processing pipeline — services discover content, parsers extract front matter and markdown, renderers produce HTML, and the pipeline orchestrates all three stages through `ContentItem` cases that terminate in a `BuildReport`._
_**One sentence: where it lives.** Namespace `Pennington.Pipeline` (`src/Pennington/Pipeline/`) for the parser/renderer/pipeline contracts and the union types; namespace `Pennington.Content` (`src/Pennington/Content/`) for `IContentService`._

## Overview

_Six-row table keyed by type. Columns: **Type**, **Namespace**, **Kind**, **Purpose**. One-sentence purposes only — this is the landing index for the five interfaces and two union types bundled on this page. Alphabetical within each kind grouping (interfaces, then unions)._

| Type | Namespace | Kind | Purpose |
|---|---|---|---|
| `IContentService` | `Pennington.Content` | interface | Discovers content and supplies navigation, copy-through files, generated files, and cross-references. |
| `IContentParser` | `Pennington.Pipeline` | interface | Transforms a `DiscoveredItem` into a `ParsedItem` by extracting `IFrontMatter` metadata and the raw markdown body. |
| `IContentPipeline` | `Pennington.Pipeline` | interface | Orchestrates the four pipeline stages (discover → parse → render → generate) and returns a `BuildReport`. |
| `IContentRenderer` | `Pennington.Pipeline` | interface | Transforms a `ParsedItem` into a `RenderedItem` by running the Markdig pipeline and collecting outline/tags/xrefs/search/social metadata. |
| `ContentItem` | `Pennington.Pipeline` | union | The pipeline payload type; every stage produces and consumes `ContentItem` so failures can propagate as a single case rather than via exceptions. |
| `ContentSource` | `Pennington.Pipeline` | union | Discriminates the origin of a `DiscoveredItem` across four kinds of backing content (markdown file, Razor page, redirect stub, programmatic generator). |

## `IContentService`

```csharp:xmldocid
T:Pennington.Content.IContentService
```

_The adapter contract that plugs a content source into the pipeline; every registered `IContentService` is consulted during discovery and for the auxiliary outputs (static copies, generated files, TOC entries, search/llms indexing, xref registration). Built-in implementations: `MarkdownContentService<TFrontMatter>`, `RazorPageContentService`, `LlmsTxtContentService`, `BlogSiteContentService`._

### Members

_Alphabetical after the required `DiscoverAsync` entry point. `GetIndexableEntriesAsync` is the only member with a default implementation (it returns `GetContentTocEntriesAsync`)._

| Name | Signature | Description |
|---|---|---|
| `DiscoverAsync` | `IAsyncEnumerable<DiscoveredItem> DiscoverAsync()` | Yields one `DiscoveredItem` per piece of content this service owns; the stream feeds `ContentPipeline.DiscoverAsync`. |
| `GetContentToCopyAsync` | `Task<ImmutableList<ContentToCopy>>` | Returns static files to copy verbatim into the output tree (images, downloads, non-markdown assets). |
| `GetContentToCreateAsync` | `Task<ImmutableList<ContentToCreate>>` | Returns dynamically generated files to write into the output tree (e.g., the search index JSON, llms.txt sidecar files). |
| `GetContentTocEntriesAsync` | `Task<ImmutableList<ContentTocItem>>` | Returns navigation entries consumed by `NavigationBuilder.BuildTree` to assemble sidebars, breadcrumbs, and prev/next links. |
| `GetIndexableEntriesAsync` | `Task<ImmutableList<ContentTocItem>>` (default) | Returns entries that feed `SearchIndexBuilder` and `LlmsTxtService`; defaults to `GetContentTocEntriesAsync` and should be overridden when "shown in navigation" diverges from "discoverable via search" (as `RazorPageContentService` does). |
| `GetCrossReferencesAsync` | `Task<ImmutableList<CrossReference>>` | Returns `uid → URL` entries registered with `XrefResolver` for `<xref:uid>` and `[text](xref:uid)` resolution. |
| `DefaultSectionLabel` | `string { get; }` | Fallback section label applied to pages from this service when their front matter supplies no `SectionLabel`. |
| `SearchPriority` | `int { get; }` | Priority value merged into the per-locale search index so results from different services can be ranked against each other. |

## `IContentParser`

```csharp:xmldocid
T:Pennington.Pipeline.IContentParser
```

_The single-method parse contract invoked by `ContentPipeline.ParseAsync` for every `DiscoveredItem`. Implementations are responsible for catching their own exceptions and returning a `FailedItem`; the pipeline's wrapper catch block is a safety net, not the primary error path._

### Members

| Name | Signature | Description |
|---|---|---|
| `ParseAsync` | `Task<ContentItem> ParseAsync(DiscoveredItem item)` | Returns `ParsedItem(Route, Metadata, RawMarkdown)` on success or `FailedItem(Route, ContentError)` on failure; the return type is the `ContentItem` union so both outcomes pass through the pipeline uniformly. |

## `IContentRenderer`

```csharp:xmldocid
T:Pennington.Pipeline.IContentRenderer
```

_The single-method render contract invoked by `ContentPipeline.RenderAsync` for every `ParsedItem`. Produces `RenderedContent` (HTML plus outline, tags, cross-references, optional search document, optional social metadata)._

### Members

| Name | Signature | Description |
|---|---|---|
| `RenderAsync` | `Task<ContentItem> RenderAsync(ParsedItem item)` | Returns `RenderedItem(Route, Metadata, RenderedContent)` on success or `FailedItem(Route, ContentError)` on failure; the return type is the `ContentItem` union so both outcomes propagate without throwing. |

## `IContentPipeline`

```csharp:xmldocid
T:Pennington.Pipeline.IContentPipeline
```

_The four-stage orchestrator; the shipped `ContentPipeline` implementation fans in every registered `IContentService` at `DiscoverAsync`, delegates parsing and rendering to `IContentParser` / `IContentRenderer`, and closes with `GenerateAsync`. Items already at a later stage pass through each transform unchanged, and `FailedItem` is never re-processed._

### Members

_In pipeline order: discover → parse → render → generate._

| Name | Signature | Description |
|---|---|---|
| `DiscoverAsync` | `IAsyncEnumerable<ContentItem> DiscoverAsync()` | Entry stage; yields a `DiscoveredItem` for every content item across every registered `IContentService`. |
| `ParseAsync` | `IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items)` | Transform stage; delegates each `DiscoveredItem` to `IContentParser.ParseAsync` and emits the resulting `ParsedItem` or `FailedItem`; `ParsedItem`, `RenderedItem`, and `FailedItem` inputs pass through unchanged. |
| `RenderAsync` | `IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items)` | Transform stage; delegates each `ParsedItem` to `IContentRenderer.RenderAsync` and emits the resulting `RenderedItem` or `FailedItem`; `RenderedItem` and `FailedItem` inputs pass through unchanged. |
| `GenerateAsync` | `Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items, OutputOptions options)` | Exit stage; pattern-matches each union case, writes non-draft `RenderedItem`s to `OutputOptions.OutputDirectory`, records `FailedItem`s as errors, and returns the accumulated `BuildReport`. |

## `ContentItem` union

```csharp:xmldocid
T:Pennington.Pipeline.ContentItem
```

_The pipeline payload union: exactly four cases. The union exposes a shared `Route` property that pattern-matches every case so call sites can read the route without a `switch` each time. Exhaustive matching is compiler-enforced._

### Cases

_One row per case. Alphabetical by case name after the stage-ordered pair (`DiscoveredItem`, `ParsedItem`, `RenderedItem`) is followed by the terminal `FailedItem`._

| Case | Fields | Produced by | Description |
|---|---|---|---|
| `DiscoveredItem` | `ContentRoute Route`, `ContentSource Source` | `IContentService.DiscoverAsync` | Pre-parse item identifying where content lives and what kind of source backs it; carries no metadata or body yet. |
| `ParsedItem` | `ContentRoute Route`, `IFrontMatter Metadata`, `string RawMarkdown` | `IContentParser.ParseAsync` | Post-YAML item with typed front-matter metadata and the raw markdown body ready for the Markdig pipeline. |
| `RenderedItem` | `ContentRoute Route`, `IFrontMatter Metadata`, `RenderedContent Content` | `IContentRenderer.RenderAsync` | Post-render item carrying final HTML plus `Outline`, `Tags`, `CrossReferences`, optional `SearchDocument`, and optional `Social` metadata. |
| `FailedItem` | `ContentRoute Route`, `ContentError Error` | parser or renderer catch blocks | Terminal error case that propagates through `ParseAsync` and `RenderAsync` untouched and becomes a `BuildReport` error in `GenerateAsync`. |

### Shared members

| Name | Type | Description |
|---|---|---|
| `Route` | `ContentRoute` | Union-level property that pattern-matches every case and returns the route; throws `InvalidOperationException` on an uninitialized union value. |

## `ContentSource` union

```csharp:xmldocid
T:Pennington.Pipeline.ContentSource
```

_Discriminates the origin of a `DiscoveredItem` across exactly four cases. `IContentParser` implementations pattern-match on this union to decide how to read the item — e.g., the Markdown parser matches `MarkdownFileSource` to read from disk, and `RedirectSource` short-circuits to a meta-refresh stub._

### Cases

_One row per case. Alphabetical._

| Case | Fields | Description |
|---|---|---|
| `MarkdownFileSource` | `FilePath Path` | Points at a `.md` file on disk; the `FilePath` is consumed by `MarkdownContentService` and read by the default `IContentParser`. |
| `ProgrammaticSource` | `IProgrammaticContentGenerator Generator` | Wraps a generator that returns a `ProgrammaticContent` union (`TextProgrammaticContent` or `BinaryProgrammaticContent`) at parse time; used for feeds, sidecar files, and any runtime-synthesized page body. |
| `RazorPageSource` | `string ComponentType` | Names the Razor component type responsible for rendering the page; `RazorPageContentService` emits this case and the parser/renderer delegate to Blazor SSR. |
| `RedirectSource` | `UrlPath TargetUrl` | Carries the destination URL for a meta-refresh stub; items with this source are rendered as `<meta http-equiv="refresh">` pages with `<meta name="robots" content="noindex">`. |

## Example

_One minimal example pulled from `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs` — the canonical non-markdown `IContentService` implementation, shown here at the type level so a reader recognizes the full interface surface in one place. Implementation walkthrough lives in the how-to._

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.ReleaseNotesContentService
```

_Reference shape for a custom `IContentService` that yields `DiscoveredItem`s from a non-markdown source (JSON release notes) and supplies the remaining `IContentService` members._

## See also

- How-to: [Implement a custom `IContentService`](/how-to/extensibility/custom-content-service)
- Related reference: [Routing types](/reference/extension-points/routing)
- Related reference: [Markdown content options](/reference/options/markdown-content-options)
- Background: [The content pipeline and union types](/explanation/core/content-pipeline)
