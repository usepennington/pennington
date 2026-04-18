---
title: "Content pipeline interfaces"
description: "The five pipeline contracts — IContentService, IContentParser, IContentRenderer, IContentPipeline — together with the ContentItem and ContentSource union types and every case."
sectionLabel: "Extension Points"
order: 405010
tags: [pipeline, content-service, unions, extension-points]
uid: reference.extension-points.content-pipeline
---

## `IContentService`

<ApiSummary XmlDocId="T:Pennington.Content.IContentService" />

<ApiMemberTable XmlDocId="T:Pennington.Content.IContentService" Kind="All" />

## `IContentParser`

<ApiSummary XmlDocId="T:Pennington.Pipeline.IContentParser" />

<ApiMemberTable XmlDocId="T:Pennington.Pipeline.IContentParser" Kind="All" />

## `IContentRenderer`

<ApiSummary XmlDocId="T:Pennington.Pipeline.IContentRenderer" />

<ApiMemberTable XmlDocId="T:Pennington.Pipeline.IContentRenderer" Kind="All" />

## `IContentPipeline`

<ApiSummary XmlDocId="T:Pennington.Pipeline.IContentPipeline" />

<ApiMemberTable XmlDocId="T:Pennington.Pipeline.IContentPipeline" Kind="All" />

## `ContentItem` union

<ApiSummary XmlDocId="T:Pennington.Pipeline.ContentItem" />

```csharp:path
src/Pennington/Pipeline/ContentItem.cs
```

## `ContentSource` union

<ApiSummary XmlDocId="T:Pennington.Pipeline.ContentSource" />

```csharp:path
src/Pennington/Pipeline/ContentSource.cs
```

## Example

See `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs` for a complete custom `IContentService` that yields `DiscoveredItem`s from a JSON source.

## See also

- How-to: [Implement a custom `IContentService`](xref:how-to.extensibility.custom-content-service)
- Related reference: [Routing types](xref:reference.extension-points.routing)
- Related reference: [Markdown content options](xref:reference.options.markdown-content-options)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
