---
title: "Routing types"
description: "The four types in Pennington.Routing that own URL math, filesystem paths, canonical routes, and route construction across every content source."
sectionLabel: "Extension Points"
order: 405020
tags: [routing, url-path, content-route, extension-points]
uid: reference.extension-points.routing
---

The four types in `Pennington.Routing` that form Pennington's canonical route coordinate system: two value-type path wrappers (`UrlPath`, `FilePath`), the `ContentRoute` record carried through every pipeline stage, and the `ContentRouteFactory` static that constructs routes from each supported content origin.

## `UrlPath`

<ApiSummary XmlDocId="T:Pennington.Routing.UrlPath" />

<ApiMemberTable XmlDocId="T:Pennington.Routing.UrlPath" Kind="All" />

## `FilePath`

<ApiSummary XmlDocId="T:Pennington.Routing.FilePath" />

<ApiMemberTable XmlDocId="T:Pennington.Routing.FilePath" Kind="All" />

## `ContentRoute`

<ApiSummary XmlDocId="T:Pennington.Routing.ContentRoute" />

<ApiMemberTable XmlDocId="T:Pennington.Routing.ContentRoute" Kind="All" />

## `ContentRouteFactory`

<ApiSummary XmlDocId="T:Pennington.Routing.ContentRouteFactory" />

<ApiMemberTable XmlDocId="T:Pennington.Routing.ContentRouteFactory" Kind="Methods" />

## Example

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

## See also

- How-to: [Implement a custom `IContentService`](xref:how-to.extensibility.custom-content-service)
- Related reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline)
- Related reference: [Navigation types](xref:reference.extension-points.navigation)
- Background: [URL paths and content routes](xref:explanation.routing.url-paths)
