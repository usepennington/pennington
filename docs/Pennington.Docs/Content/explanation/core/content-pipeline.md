---
title: "The content pipeline and union types"
description: "Why Pennington models content as a four-case union that flows Discovered to Parsed to Rendered — with Failed as a peer case rather than an exception."
uid: explanation.core.content-pipeline
order: 301010
sectionLabel: "Core Architecture"
tags: [pipeline, unions, architecture]
---

Why does Pennington model its content flow as a union of four records instead of a single `ContentItem` class with a status field or a polymorphic base class?

## Context

A content engine has to move each page through at least four distinct phases — discovery, parsing, rendering, and emission — and each phase legitimately knows different things about the item. A discovered file has a source location and a route, but no parsed front matter or markdown body. A parsed item has structured metadata and text, but no HTML. A rendered item has HTML and an outline, ready to write to disk. These are not the same data at different points in time; they are genuinely different shapes.

The conventional escape routes both have a cost. A single `ContentItem` class with nullable `Metadata`, `Html`, and `Error` fields invites "is it safe to touch this yet?" checks at every call site, and the compiler has no way to enforce which combination of fields is populated at which stage. A traditional inheritance hierarchy — `ContentItem` → `ParsedItem` → `RenderedItem` — puts discovery-stage code and render-stage code in a subtyping relationship that does not reflect how they are actually used, and forces the failure case into either a parallel branch that breaks `is`-checks downstream or a nullable error field on every subclass. C# 15 discriminated unions offer a third path: the compiler tracks which case you hold, pattern matching is exhaustive, and each case carries exactly the fields that exist at that stage.

## How it works

### The union shape

`ContentItem` is a union of four record cases: `DiscoveredItem`, `ParsedItem`, `RenderedItem`, and `FailedItem`. Each case is a plain record holding only the fields that make sense at its stage — there is no base class, no status enum, and no nullable placeholder for data that has not arrived yet. The union itself exposes a single `Route` property that all four cases share, because "every content item, even a failed one, belongs to a route" is a genuine invariant. Call sites that only need to know the route never have to pattern-match; call sites that need the rendered HTML must match and will get a compile error if they forget a case.

```csharp:path
src/Pennington/Pipeline/ContentItem.cs
```

The four case records are siblings in the same union — none inherits from another, and there is no `Stage` or `Status` field anywhere. `Route` is the one projection lifted onto the union, and that narrowness is deliberate: every additional lifted property would need a sensible value for all four cases, which is exactly the nullable-field trap the union is meant to avoid.

### `ContentSource` discriminates where an item came from

A `DiscoveredItem` pairs a `ContentRoute` with a second union, `ContentSource`, which records where the item came from: a file on disk, a Razor `@page`, a redirect definition, or a programmatic generator. The reason this is a separate union rather than a field on `DiscoveredItem` is that discovery is itself a pluggable step — different sources need to carry different data (a file path, a Razor component type, a target URL) without forcing later stages to care. Once an item has been parsed, its source has already done its job; the parser and renderer work entirely against the resolved front matter and content text, and `ContentSource` disappears from the picture.

```csharp:path
src/Pennington/Pipeline/ContentSource.cs
```

The four cases cover every origin Pennington ships, and downstream stages — parsers, renderers, the output writer — never pattern-match on `ContentSource`; by the time they run, the source has been replaced by the parsed shape.

### Stage transitions replace the item

Each stage in the pipeline works by replacing the incoming union case with the next one. `ParseAsync` pulls a stream of `ContentItem` values and, for each `DiscoveredItem`, hands its content to the registered `IContentParser`. When the parser succeeds, the `DiscoveredItem` is replaced by a `ParsedItem` carrying the resolved front matter and text. `RenderAsync` does the same thing one level further: each `ParsedItem` is handed to the `IContentRenderer`, and on success a `RenderedItem` takes its place, now carrying the HTML output and a navigation outline. The final stage, `GenerateAsync`, pattern-matches on the full union to write output files and accumulate the build report.

The replacement invariant is what gives the pipeline its composability. A `RenderedItem` flowing into `ParseAsync` is already past that stage, so `ParseAsync` passes it through unchanged. A `ParsedItem` flowing into `RenderAsync` gets rendered; a `RenderedItem` in the same stream passes through. This means you can hand the pipeline a partially-processed stream — one that mixes discovered and already-parsed items — and it will do the right thing for each. There is no need to coordinate which stage ran last; the case type carries that information.

```csharp:xmldocid,bodyonly
M:Pennington.Pipeline.ContentPipeline.ParseAsync(System.Collections.Generic.IAsyncEnumerable{Pennington.Pipeline.ContentItem})
```

The implementation has three explicit branches: a `FailedItem` passes through without touching the parser, a `DiscoveredItem` is handed to `IContentParser.ParseAsync` inside a try/catch that demotes any exception to a `FailedItem`, and a `ParsedItem` or `RenderedItem` passes through unchanged because the work for that stage is already done. There is also a guard for `RedirectSource`: items whose source is a redirect skip the parser entirely, because a redirect has no body to parse and is served by middleware at request time rather than written as an HTML file.

### `FailedItem` as a peer case, not an exception

Exceptions thrown inside `IContentParser.ParseAsync` or `IContentRenderer.RenderAsync` are caught at the pipeline boundary and rewritten as a `FailedItem` carrying the route and a `ContentError` that describes what went wrong. From that point on, the failed item rides the same async stream as the successful ones. Downstream stages check `is FailedItem` and short-circuit without touching the error or trying to make sense of absent fields.

The payoff arrives in `GenerateAsync`. Because `FailedItem` is a peer case in the union, the exhaustive pattern match there routes it to `BuildReportBuilder.AddError` — every parse or render exception ends up as a named entry in the build report rather than an unhandled exception that aborts the crawl. One broken markdown file does not prevent the other four hundred from rendering. This is the concrete benefit of treating failure as data: the "sad path" and the "happy path" have identical shape in the stream, which means the final aggregation step sees them through the same lens.

```csharp:xmldocid
T:Pennington.Pipeline.FailedItem
```

Treating failure as a data case is what makes the exhaustive match in `GenerateAsync` meaningful rather than ceremonial. If `FailedItem` were an exception that escaped the pipeline, there would be no case to match — and no way for the compiler to confirm that every outcome was handled.

## Trade-offs

- **Cost:** The union is a change-amplification point. Adding a fifth stage or a new terminal case means every `switch` on `ContentItem` across the codebase must be updated — the compiler will surface all the gaps, which is the feature, but it is still a broad change. The `union` keyword is also a C# 15 feature, and LSP tooling in some editors still flags false errors on the declaration; the compiler handles it correctly, but the red squiggles are real until tooling catches up.
- **Alternative considered — discriminator property:** A `ContentItem` class with a `Stage` enum and nullable `Metadata`, `Html`, and `Error` fields was rejected because every consumer ends up writing the same null-check sequence, and the compiler has no way to prove which combination of fields is actually populated at any given call site. The safety that pattern matching provides would have to be replicated manually everywhere.
- **Alternative considered — inheritance hierarchy:** An abstract `ContentItem` with `ParsedItem : ContentItem` and `RenderedItem : ParsedItem` was rejected because it creates a subtyping relationship that does not reflect how the cases are actually used, and because it forces `FailedItem` into either a parallel branch that breaks `is`-checks downstream or a nullable error field on every subclass — which leads straight back to the nullable-field problem.
- **Consequence:** Downstream code that only needs the `ContentRoute` still pays a pattern match unless it goes through the `Route` property lifted onto the union. That one projection is why the design resists lifting anything else: each additional shared property needs a sensible value for all four cases, and "sensible for all four cases" is the definition of the nullable-field trap. Adding another lifted property is a deliberate design decision, not a convenience shortcut.

## Further reading

- Reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline) — the catalog of `IContentService`, `IContentParser`, `IContentRenderer`, and every case record with members.
- How-to: [Implement a custom `IContentService`](xref:how-to.extensibility.custom-content-service) — how to plug a new source into the discovery stage.
