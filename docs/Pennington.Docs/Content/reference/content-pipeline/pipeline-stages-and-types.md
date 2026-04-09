---
title: "Pipeline Stages and Types"
description: "Reference for the ContentItem union (DiscoveredItem, ParsedItem, RenderedItem, FailedItem), ContentSource union (MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource), IContentPipeline, IContentParser, IContentRenderer, RenderedContent, and ContentError"
uid: "penn.reference.pipeline-stages-and-types"
order: 10
---

Document the type system that drives the content pipeline. Start with the `ContentItem` union and its 4 cases: `DiscoveredItem` (Route, Source), `ParsedItem` (Route, Metadata, RawContent), `RenderedItem` (Route, Metadata, Content), `FailedItem` (Route, Error). For each case, list its properties and types. Then document the `ContentSource` union and its 4 cases: `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`. Document the `IContentPipeline` interface and its stage methods: `DiscoverAsync`, `ParseAsync`, `RenderAsync`, `GenerateAsync`, `RunAsync`. Document `IContentParser<T>` and `IContentRenderer`. Document `RenderedContent` (Html string, Outline OutlineEntry[]) and `ContentError`. Use `:xmldocid` code blocks for type definitions. This is a type catalog — no narrative, no tutorials.
