---
title: Content pipeline interfaces
description: IContentService, IContentParser, IContentRenderer, IContentPipeline, and the ContentItem / ContentSource union types with every case.
section: extension-points
order: 10
tags: []
uid: reference.extension-points.content-pipeline
isDraft: true
search: false
llms: false
---

> **In this page.** `IContentService`, `IContentParser`, `IContentRenderer`, `IContentPipeline`, and the `ContentItem`/`ContentSource` union types with every case.
>
> **Not in this page.** Implementation examples — see How-Tos.

## Summary

The interface family that shapes Pennington's four-stage discover/parse/render/generate pipeline, plus the two union types that carry items and source discriminators through it.
Namespaces `Pennington.Pipeline` (pipeline + unions) and `Pennington.Content` (`IContentService`); defined under `src/Pennington/Pipeline/` and `src/Pennington/Content/`.

## `IContentService`

Source-adapter contract. Each registered implementation feeds `DiscoveredItem`s into stage 1 of the pipeline and supplies side-channel data (files to copy, dynamic files to create, TOC entries, cross-references).

```csharp:xmldocid
T:Pennington.Content.IContentService
```

### Members

| Signature | Returns | Description |
|---|---|---|
| `IAsyncEnumerable<DiscoveredItem> DiscoverAsync()` | stream of `DiscoveredItem` | Enumerates every content item this service is responsible for. |
| `Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()` | `ImmutableList<ContentToCopy>` | Static files to copy verbatim to the output tree (images, downloads). |
| `Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()` | `ImmutableList<ContentToCreate>` | Dynamically generated files (e.g., search index sidecars). |
| `Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()` | `ImmutableList<ContentTocItem>` | Navigation entries for the table of contents. |
| `Task<ImmutableList<ContentTocItem>> GetIndexableEntriesAsync()` | `ImmutableList<ContentTocItem>` | Entries for the search index and llms.txt. Default member — returns `GetContentTocEntriesAsync()`. |
| `Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()` | `ImmutableList<CrossReference>` | Uid → URL entries for xref resolution. |
| `string DefaultSection { get; }` | `string` | Fallback section name applied to pages that do not declare one. |
| `int SearchPriority { get; }` | `int` | Default priority assigned to this service's search-index documents. |

## `IContentParser`

Parse stage. Invoked by `ContentPipeline.ParseAsync` on every `DiscoveredItem`.

```csharp:xmldocid
T:Pennington.Pipeline.IContentParser
```

### Members

| Signature | Returns | Description |
|---|---|---|
| `Task<ContentItem> ParseAsync(DiscoveredItem item)` | `ContentItem` (normally `ParsedItem`; `FailedItem` on failure) | Extracts YAML front matter and raw markdown body from the discovered source. |

## `IContentRenderer`

Render stage. Invoked by `ContentPipeline.RenderAsync` on every `ParsedItem`.

```csharp:xmldocid
T:Pennington.Pipeline.IContentRenderer
```

### Members

| Signature | Returns | Description |
|---|---|---|
| `Task<ContentItem> RenderAsync(ParsedItem item)` | `ContentItem` (normally `RenderedItem`; `FailedItem` on failure) | Runs the Markdig pipeline, producing HTML plus outline, tags, cross-references, search document, and social metadata. |

## `IContentPipeline`

The four-stage orchestrator; default implementation is `ContentPipeline` in the same namespace.

```csharp:xmldocid
T:Pennington.Pipeline.IContentPipeline
```

### Members

| Signature | Returns | Description |
|---|---|---|
| `IAsyncEnumerable<ContentItem> DiscoverAsync()` | stream of `DiscoveredItem` | Stage 1. Fans in `IContentService.DiscoverAsync()` across every registered service. |
| `IAsyncEnumerable<ContentItem> ParseAsync(IAsyncEnumerable<ContentItem> items)` | stream of `ContentItem` | Stage 2. Delegates each `DiscoveredItem` to `IContentParser`; `FailedItem`s pass through; other cases pass through unchanged. |
| `IAsyncEnumerable<ContentItem> RenderAsync(IAsyncEnumerable<ContentItem> items)` | stream of `ContentItem` | Stage 3. Delegates each `ParsedItem` to `IContentRenderer`; `FailedItem`s pass through; other cases pass through unchanged. |
| `Task<BuildReport> GenerateAsync(IAsyncEnumerable<ContentItem> items, OutputOptions options)` | `BuildReport` | Stage 4. Pattern-matches each case, skipping drafts, emitting errors for `FailedItem`, warning on items that did not reach `RenderedItem`. |

## `ContentItem` union

Discriminated union carrying an item through the pipeline. The compiler enforces exhaustive matching over exactly these four cases; `ContentItem.Route` is exposed on the union so call sites do not re-dispatch to read the route.

```csharp:xmldocid
T:Pennington.Pipeline.ContentItem
```

### Cases

| Case | Payload | Emitted by |
|---|---|---|
| `DiscoveredItem` | `ContentRoute Route`, `ContentSource Source` | `IContentService.DiscoverAsync` (fanned in by `ContentPipeline.DiscoverAsync`). |
| `ParsedItem` | `ContentRoute Route`, `IFrontMatter Metadata`, `string RawMarkdown` | `IContentParser.ParseAsync` on success (via `ContentPipeline.ParseAsync`). |
| `RenderedItem` | `ContentRoute Route`, `IFrontMatter Metadata`, `RenderedContent Content` | `IContentRenderer.RenderAsync` on success (via `ContentPipeline.RenderAsync`). |
| `FailedItem` | `ContentRoute Route`, `ContentError Error` | `ContentPipeline.ParseAsync` / `ContentPipeline.RenderAsync` catch blocks, or any `IContentParser` / `IContentRenderer` that returns one directly. |

## `ContentSource` union

Discriminates the origin that produced a `DiscoveredItem`. Consumed by downstream services that need to branch on source kind (for example, `SitemapService` skips `RedirectSource`, `SpaNavigationContentService` skips `RazorPageSource` and `RedirectSource`).

```csharp:xmldocid
T:Pennington.Pipeline.ContentSource
```

### Cases

| Case | Payload | Emitted by |
|---|---|---|
| `MarkdownFileSource` | `FilePath Path` | `MarkdownContentService<TFrontMatter>.DiscoverAsync` (`src/Pennington/Content/MarkdownContentService.cs`). |
| `RazorPageSource` | `string ComponentType` (assembly-qualified name) | `RazorPageContentService.DiscoverAsync` (`src/Pennington/Content/RazorPageContentService.cs`); `BlogSiteContentService.DiscoverAsync` (`src/Pennington.BlogSite/Services/BlogSiteContentService.cs`) for tag pages. |
| `RedirectSource` | `UrlPath TargetUrl` | `SpaNavigationContentService.DiscoverAsync` (`src/Pennington/Islands/SpaNavigationContentService.cs`) for SPA JSON routes; also produced by `ContentRouteFactory.ForRedirect` call sites that build explicit redirects. |
| `ProgrammaticSource` | `IProgrammaticContentGenerator Generator` | Custom `IContentService` implementations whose items synthesize their own front matter + body (see `examples/ForgePortalExample/ReleaseNotesContentService.cs`, `examples/SearchExample/Services/RandomContentService.cs`). |

## See also

- How-to: [Add a custom content service](/how-to/extensibility/custom-content-service)
- Related reference: [`IFrontMatter` and capability defaults](/reference/front-matter/ifrontmatter)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
