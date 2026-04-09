---
title: "The Content Processing Pipeline"
description: "Why Pennington uses a 4-stage streaming pipeline with union-based error handling — covering IAsyncEnumerable streaming, ContentItem exhaustive matching vs exceptions, and the division between IContentService (discovery) and IContentPipeline (processing)"
uid: "penn.explanation.content-processing-pipeline"
order: 10
---

Explain the design decisions behind Pennington's 4-stage content pipeline. Why a streaming `IAsyncEnumerable<ContentItem>` rather than materializing all content into a list — memory efficiency with large sites, and the ability to fail partially rather than all-or-nothing. Why the `ContentItem` union type with `FailedItem` as a case rather than throwing exceptions — exhaustive pattern matching forces callers to handle errors, prevents silent failures, preserves context about what failed and why. Discuss the separation between `IContentService` (knows how to discover and describe content) and `IContentPipeline` (knows how to process it through stages) — this separation lets custom content services participate without reimplementing parsing or rendering. Explain why `ContentSource` is also a union rather than an interface hierarchy — the cases are closed and known at compile time, enabling the compiler to enforce exhaustive handling.
