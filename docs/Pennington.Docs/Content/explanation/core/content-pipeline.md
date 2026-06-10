---
title: "The content pipeline and union types"
description: "Why Pennington models content as a four-case union that flows Discovered to Parsed to Rendered — with Failed as a peer case rather than an exception."
uid: explanation.core.content-pipeline
order: 1
sectionLabel: "Core Architecture"
tags: [pipeline, unions, architecture]
---

Why does Pennington model its content flow as a union of four records instead of a single `ContentItem` class with a status field or a polymorphic base class?

## Context

A content engine has to move each page through at least four distinct phases — discovery, parsing, rendering, and emission — and each phase legitimately knows different things about the item. A discovered file has a source location and a route, but no parsed front matter or markdown body. A parsed item has structured metadata and text, but no HTML. A rendered item has HTML and an outline, ready to write to disk. These are not the same data at different points in time; they are genuinely different shapes.

The conventional escape routes both have a cost. A single `ContentItem` class with nullable `Metadata`, `Html`, and `Error` fields invites "is it safe to touch this yet?" checks at every call site, and the compiler has no way to enforce which combination of fields is populated at which stage. A traditional inheritance hierarchy — `ContentItem` → `ParsedItem` → `RenderedItem` — puts discovery-stage code and render-stage code in a subtyping relationship that does not reflect how they are actually used, and forces the failure case into either a parallel branch that breaks `is`-checks downstream or a nullable error field on every subclass. [C# 15 discriminated unions](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/union) offer a third path: the compiler tracks which case you hold, pattern matching is exhaustive, and each case carries exactly the fields that exist at that stage.

## How it works

### The union shape

`ContentItem` is a union of four record cases — `DiscoveredItem` (route + source), `ParsedItem` (adds metadata and raw markdown), `RenderedItem` (adds HTML and outline), and `FailedItem` (route + error). Each case is a plain record holding only the fields that make sense at its stage — no base class, no status enum, no nullable placeholders. The union itself exposes a single `Route` property that all four cases share, because "every content item, even a failed one, belongs to a route" is a genuine invariant. Call sites that need the rendered HTML must pattern-match and will get a compile error if they forget a case.

`Route` is the one projection lifted onto the union, and that narrowness is deliberate: every additional lifted property would need a sensible value for all four cases — which is exactly the nullable-field trap the union is meant to avoid. For full member lists, see <xref:reference.api.i-content-service>.

### `ContentSource` discriminates where an item came from

A `DiscoveredItem` pairs a `ContentRoute` with a second union, `ContentSource`, which records where the item came from. The reason this is a separate union rather than a field on `DiscoveredItem` is that discovery is itself a pluggable step — different sources carry different data (a file path, a Razor component type, a target URL) without forcing later stages to care. Once an item has been parsed, its source has already done its job: the parser and renderer work entirely against the resolved front matter and content text, and `ContentSource` disappears from the picture. For why this second union is shaped the way it is — its cases, and the `.Value` read that works across both target frameworks — see <xref:explanation.core.content-source>.

### Stage transitions replace the item

Each stage in the pipeline works by replacing the incoming union case with the next one. `ParseAsync` pulls a stream of `ContentItem` values and, for each `DiscoveredItem`, hands its content to the registered `IContentParser`. When the parser succeeds, the `DiscoveredItem` is replaced by a `ParsedItem` carrying the resolved front matter and text. `RenderAsync` does the same thing one level further: each `ParsedItem` is handed to the `IContentRenderer`, and on success a `RenderedItem` takes its place, now carrying the HTML output and a navigation outline. The final stage, `GenerateAsync`, pattern-matches on the full union to write output files and accumulate the build report.

The replacement invariant is what gives the pipeline its composability. A `RenderedItem` flowing into `ParseAsync` is already past that stage, so `ParseAsync` passes it through unchanged. A `ParsedItem` flowing into `RenderAsync` gets rendered; a `RenderedItem` in the same stream passes through. This means you can hand the pipeline a partially-processed stream — one that mixes discovered and already-parsed items — and it will do the right thing for each. There is no need to coordinate which stage ran last; the case type carries that information.

```csharp:symbol,bodyonly
src/Pennington/Pipeline/ContentPipeline.cs > ContentPipeline.ParseAsync
```

The implementation has three explicit branches: a `FailedItem` passes through without touching the parser, a `DiscoveredItem` is handed to `IContentParser.ParseAsync` inside a try/catch that demotes any exception to a `FailedItem`, and a `ParsedItem` or `RenderedItem` passes through unchanged because the work for that stage is already done. There is also a guard for `RedirectSource` and `EndpointSource`: items whose source is a redirect or an endpoint (`sitemap.xml`, `llms.txt`) skip the parser entirely, because neither has a body to parse — a redirect is served by middleware at request time rather than written as an HTML file, and an endpoint is produced by a live HTTP endpoint.

### `FailedItem` as a peer case, not an exception

Exceptions thrown inside `IContentParser.ParseAsync` or `IContentRenderer.RenderAsync` are caught at the pipeline boundary and rewritten as a `FailedItem` carrying the route and a `ContentError` that describes what went wrong. From that point on, the failed item rides the same async stream as the successful ones. Downstream stages check `is FailedItem` and short-circuit without touching the error or trying to make sense of absent fields.

This is what `GenerateAsync` relies on. Because `FailedItem` is a case in the union, the exhaustive pattern match there routes it to `BuildReportBuilder.AddError` — every parse or render exception ends up as a named entry in the build report rather than an unhandled exception that aborts the crawl. One broken markdown file does not prevent the other four hundred from rendering. Because failure is just another data case, a failed item and a successful one are the same kind of value in the stream, so the final aggregation step handles them the same way.

```csharp:symbol
src/Pennington/Pipeline/ContentItem.cs > FailedItem
```

Treating failure as a data case is what gives the exhaustive match in `GenerateAsync` something to act on. If `FailedItem` were an exception that escaped the pipeline, there would be no case to match — and no way for the compiler to confirm that every outcome was handled.

## Further reading

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service) — the catalog of `IContentService`, `IContentParser`, `IContentRenderer`, and every case record with members.
- How-to: [Implement a custom `IContentService`](xref:how-to.content-services.custom-content-service) — how to plug a new source into the discovery stage.
