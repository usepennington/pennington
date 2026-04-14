---
title: ContentPipeline
description: The discovery/parse/render pipeline every content source flows through.
sectionLabel: Core API
order: 20
---

# ContentPipeline

`ContentPipeline` is the assembly line `IContentService` implementations
feed into. Each source yields `DiscoveredItem`s, the pipeline parses them
into `ParsedItem`s via `IContentParser`, and finally renders them into
`RenderedItem`s via `IContentRenderer`.

## The three stages

1. **Discover** — each registered `IContentService` walks its source (disk,
   Razor pages, a JSON feed, whatever) and emits `DiscoveredItem` unions.
2. **Parse** — the matching `IContentParser` reads the item, separates
   front matter from body, and produces a `ParsedItem`.
3. **Render** — `IContentRenderer` turns the parsed body into HTML (plus
   an outline of headings and any diagnostics).

Custom parsers and renderers plug into the same pipeline — see the
*Extensions* section in the sidebar for how to write one.
