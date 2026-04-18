---
title: "Island rendering interfaces"
description: "The contracts that produce island HTML for the SPA envelope — IIslandRenderer, RazorIslandRenderer<T>, SpaEnvelope, RenderContext — plus the data-spa-* attribute surface the client engine reads."
sectionLabel: "Extension Points"
order: 405040
tags: [islands, spa, extension-points, rendering]
uid: reference.extension-points.islands
---

## `IIslandRenderer`

<ApiSummary XmlDocId="T:Pennington.Islands.IIslandRenderer" />

<ApiMemberTable XmlDocId="T:Pennington.Islands.IIslandRenderer" Kind="All" />

## `RazorIslandRenderer<T>`

<ApiSummary XmlDocId="T:Pennington.Islands.RazorIslandRenderer`1" />

<ApiMemberTable XmlDocId="T:Pennington.Islands.RazorIslandRenderer`1" Kind="All" Access="PublicAndProtected" />

## `RenderContext`

<ApiSummary XmlDocId="T:Pennington.Islands.RenderContext" />

<ApiMemberTable XmlDocId="T:Pennington.Islands.RenderContext" />

## `SpaEnvelope`

<ApiSummary XmlDocId="T:Pennington.Islands.SpaEnvelope" />

<ApiMemberTable XmlDocId="T:Pennington.Islands.SpaEnvelope" />

## `data-spa-*` attribute reference

Lookup table for the attribute surface the client engine reads (`src/Pennington.UI/wwwroot/spa-engine.js`), grouped by the element that carries them.

| Attribute | Applies to | Values | Description |
|---|---|---|---|
| `data-spa-island` | Any element (conventionally `<article>`, `<aside>`, `<div>`) | Island name matching `IIslandRenderer.IslandName` | Marks the element as an island slot; the engine replaces its `innerHTML` with `SpaEnvelope.Islands[name]` on navigation. |
| `data-spa-loading` | An element carrying `data-spa-island` | `"skeleton"`, `"clear"`, `"keep"` (default) | Loading behaviour while the next envelope is in flight — `skeleton` injects a shimmer (or the matching `<template data-spa-skeleton-for="…">` if present), `clear` empties the slot, `keep` leaves the previous content in place. |
| `data-spa-skeleton-for` | `<template>` | Island name matching `data-spa-island` | Pairs a `<template>` with a skeleton-loading island; the template's content is cloned into the island instead of the default shimmer when `data-spa-loading="skeleton"`. |
| `data-spa-reload` | `<a>` | Presence only (boolean attribute) | Marks a link as "do not intercept" — the engine performs a full-document reload for this anchor instead of fetching the JSON envelope. |
| `data-spa-data-path` | `<html>` | URL path (default `/_spa-data`) | Overrides the path the engine fetches envelopes from; must match `SpaNavigationOptions.DataPath`. |
| `data-spa-skeleton-delay` | `<html>` | Integer milliseconds (default `100`) | Milliseconds the engine waits before showing a skeleton. |
| `data-spa-min-skeleton` | `<html>` | Integer milliseconds (default `250`) | Minimum time a skeleton stays visible once shown. |
| `data-base-url` | `<body>` | URL path | Stamped by `BaseUrlHtmlRewriter`; the engine strips this prefix from `location.pathname` when computing a route's `SpaSlug`. |

## Example

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.ChartIslandRenderer
```

## See also

- How-to: [Register an island renderer](xref:how-to.extensibility.island-renderer)
- Related reference: [`IslandsOptions`](xref:reference.options.auxiliary-options)
- Related reference: [Routing types](xref:reference.extension-points.routing)
- Background: [SPA navigation and island architecture](xref:explanation.spa.islands)
