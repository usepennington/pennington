---
title: "The content pipeline and union types"
description: "Why Pennington models content as a four-case union that flows Discovered to Parsed to Rendered — with Failed as a peer case rather than an exception."
uid: explanation.core.content-pipeline
order: 10
sectionLabel: "Core Architecture"
tags: [pipeline, unions, architecture]
---

> **In this page.** _Paraphrase the Covers line: why `ContentItem` and `ContentSource` are unions, how items advance `DiscoveredItem` → `ParsedItem` → `RenderedItem` (or divert to `FailedItem`), and what that shape buys over a class hierarchy. One sentence._
>
> **Not in this page.** _Paraphrase the Does-not-cover line: pointer readers needing the interface catalog at `/reference/extension-points/content-pipeline`. One sentence._

## The question

_Ask the reader's question in one sentence, something like: "Why does Pennington model its content flow as a union of four records instead of a single `ContentItem` class with a status field or a polymorphic base class?" Do not answer yet; the rest of the page is the answer._

## Context

_Two to five sentences. Set up the problem: a content engine needs to move a page through discovery, parsing, rendering, and emission, and each stage legitimately carries different data — a discovered file has a `ContentSource` but no parsed body, a parsed item has metadata but no HTML, a rendered item has both plus an outline. Contrast the alternatives briefly: a single mutable `ContentItem` class with nullable `Metadata`/`Html`/`Error` fields invites "is it safe to touch this property yet?" checks at every call site; a traditional inheritance hierarchy (`ContentItem` → `ParsedItem` → `RenderedItem`) forces the failure case to either throw or squat inside every subclass. End the section hinting that C# 15 unions let the pipeline carry exactly one state per item with the compiler enforcing exhaustive handling._

## How it works

### The union shape

_One or two paragraphs. Describe the four-case shape: each case is a plain record carrying only the fields that exist at that stage, and the union itself exposes a `Route` that every case shares. Note that the Route projection lives on the union so call sites that only need the route avoid pattern matching — this is the one shared property, because the invariant "every item, even a failed one, belongs to a route" is real._

```csharp:xmldocid
T:Pennington.Pipeline.ContentItem
```

_After the fence, point out the four case records are siblings — there is no base class and no status enum — and that the `Route` property is the only thing lifted onto the union itself._

### `ContentSource` discriminates where an item came from

_One short paragraph. A `DiscoveredItem` pairs a `ContentRoute` with a second union, `ContentSource`, which records the origin: a file on disk, a Razor `@page`, a redirect, or a programmatic generator. Explain briefly that this keeps discovery pluggable without polluting later stages — once an item parses, its source has done its job and the parser and renderer only see the already-resolved metadata and markdown text._

```csharp:xmldocid
T:Pennington.Pipeline.ContentSource
```

_One sentence after the fence reinforcing that the four `ContentSource` cases cover the shipped origins and that downstream stages do not pattern match on source — they work against the parsed shape instead._

### Stage transitions replace the item

_Two or three paragraphs, this is the heart of the page. Walk through the three transitions: `ParseAsync` receives `DiscoveredItem` and yields `ParsedItem`, `RenderAsync` receives `ParsedItem` and yields `RenderedItem`, `GenerateAsync` pattern-matches the union and records the outcome in a `BuildReport`. Emphasize the replacement invariant: a `ParsedItem` flowing through `RenderAsync` gets replaced by a `RenderedItem`; a `RenderedItem` flowing through either earlier stage passes through unchanged. This is what makes the pipeline re-entrant and composable — you can hand it a half-processed stream and it picks up where the stream left off. Consider pulling the `ParseAsync` body to show the replacement in code._

```csharp:xmldocid,bodyonly
M:Pennington.Pipeline.ContentPipeline.ParseAsync(System.Collections.Generic.IAsyncEnumerable{Pennington.Pipeline.ContentItem})
```

_After the fence, point out the three explicit branches: `FailedItem` passes through, `DiscoveredItem` is handed to the parser inside a try/catch that demotes exceptions to `FailedItem`, and any already-advanced item (`ParsedItem`, `RenderedItem`) passes through unchanged. Note the `RedirectSource` guard as an aside — it is the one origin that deliberately leaves the pipeline early because redirects are served by middleware at request time._

### `FailedItem` as a peer case, not an exception

_Two short paragraphs. Exceptions thrown inside `IContentParser.ParseAsync` or `IContentRenderer.RenderAsync` are caught at the pipeline boundary and rewritten as a `FailedItem` that carries the route and a `ContentError`. From that point on the failed item rides the same stream as the successful items; downstream stages check `is FailedItem` and short-circuit. The payoff lands in `GenerateAsync`, which routes `FailedItem` to `BuildReportBuilder.AddError` so every parse or render exception shows up in the build report instead of aborting the crawl. One failure does not fail the build prematurely — it is one reported error among many._

```csharp:xmldocid
T:Pennington.Pipeline.FailedItem
```

_After the fence, make the mental-model point: treating failure as a data case means the "happy path" and the "sad path" have the same shape, which is exactly what makes exhaustive pattern matching inside `GenerateAsync` a meaningful check rather than a formality._

## Trade-offs

_Two to four bullets. Name the real costs, not stylistic preferences. Required bullets:_

- **Cost:** _Adding a fifth stage or a new terminal state means touching every `switch` that matches on `ContentItem` — the compiler will nag, which is a feature, but it does mean the union is a change-amplification point. Mention also that unions are a C# 15 feature and LSP tooling still flags false errors on the `union` keyword._
- **Alternative considered — discriminator property:** _A `ContentItem` class with a `Stage` enum and nullable `Metadata`/`Html`/`Error` fields was rejected because every consumer ends up null-checking the same three or four properties and the compiler cannot prove which combination is valid at which stage._
- **Alternative considered — inheritance hierarchy:** _An abstract `ContentItem` with `ParsedItem : ContentItem`, `RenderedItem : ParsedItem` was rejected because it forces `FailedItem` into either a parallel branch that breaks `is` checks downstream or a nullable error field on every subclass, which puts us back in the discriminator-property bind._
- **Consequence:** _Downstream code that only needs the `ContentRoute` still pays a pattern match unless it goes through the lifted `Route` property on the union — so the pipeline exposes that one projection and resists the temptation to lift more. Adding another lifted property is a real design decision, not a convenience._

## Further reading

_Two to four cross-quadrant links, one per line. Do NOT include a sibling explanation link (those auto-generate). Suggested set:_

- Reference: [Content pipeline interfaces](/reference/extension-points/content-pipeline/) — the catalog of `IContentService`, `IContentParser`, `IContentRenderer`, and every case record with members.
- How-to: [Implement a custom `IContentService`](/how-to/extensibility/custom-content-service/) — how to plug a new source into the Discovery stage.
- External: _TODO — cite the C# 11/15 unions design note or an Ekblad/Eric Lippert post on discriminated unions if one is on record; otherwise drop this bullet._
