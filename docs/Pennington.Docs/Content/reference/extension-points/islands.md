---
title: "Island rendering interfaces"
description: "The contracts that produce island HTML for the SPA envelope — IIslandRenderer, RazorIslandRenderer<T>, SpaEnvelope, RenderContext — plus the data-spa-* attribute surface the client engine reads."
sectionLabel: "Extension Points"
order: 40
tags: [islands, spa, extension-points, rendering]
uid: reference.extension-points.islands
---

> **In this page.** `IIslandRenderer`, `RazorIslandRenderer<T>`, `SpaEnvelope`, `RenderContext`, and the `data-spa-*` attribute surface.
>
> **Not in this page.** The SPA client-side script — see the explanation [SPA navigation and island architecture](/explanation/spa/islands).

## Summary

_**One sentence: what it is.** The four types in `Pennington.Islands` that implementers touch when adding a server-rendered island — the `IIslandRenderer` contract, the `RazorIslandRenderer<T>` convenience base, the `SpaEnvelope` record describing the per-route payload, and the `RenderContext` handed to renderers — together with the `data-spa-*` attribute surface the browser engine reads to locate and hydrate those islands._
_**One sentence: where it lives.** Namespace `Pennington.Islands` (`src/Pennington/Islands/`); registered through `PenningtonOptions.Islands.Register<T>(name)` and exposed to the browser by `SpaNavigationExtensions.AddSpaNavigation` / `UseSpaNavigation`._

## `IIslandRenderer`

```csharp:xmldocid
T:Pennington.Islands.IIslandRenderer
```

_The root contract: one property naming the island, one method that returns rendered HTML for a given `ContentRoute` and `RenderContext`. Returning an empty string signals "no island on this page" — `SpaPageDataService` drops empties before keying the `SpaEnvelope.Islands` dictionary._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `IslandName` | `string { get; }` | The island key; must match the `data-spa-island` attribute value in the host markup and becomes the key under which the rendered HTML is stored in `SpaEnvelope.Islands`. |
| `RenderAsync` | `Task<string> RenderAsync(ContentRoute route, RenderContext context)` | Returns the island HTML for the given route, or an empty string to skip this island on that route; called once per route per renderer by `SpaPageDataService.GetPageDataAsync`. |

## `RazorIslandRenderer<T>`

```csharp:xmldocid
T:Pennington.Islands.RazorIslandRenderer`1
```

_Abstract base that implements `IIslandRenderer` for the common case of rendering a Razor component `TComponent`. Subclasses override `IslandName` and `BuildParametersAsync`; the base resolves parameters to HTML through the scoped `ComponentRenderer`. Returning `null` from `BuildParametersAsync` skips the island for that route (the base returns an empty string, which `SpaPageDataService` filters out)._

### Members

_Alphabetical; `IslandName` restated here because every subclass must override it._

| Name | Signature | Description |
|---|---|---|
| `BuildParametersAsync` | `protected abstract Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)` | Subclass hook that returns the parameter dictionary passed to `TComponent`, or `null` to skip the island for that route; consulted once per `RenderAsync` invocation. |
| `IslandName` | `public abstract string { get; }` | Subclass-supplied island key; see the `IIslandRenderer.IslandName` row above for the contract. |
| `RenderAsync` | `public Task<string> RenderAsync(ContentRoute route, RenderContext context)` | Sealed base implementation that calls `BuildParametersAsync`, returns `""` when the result is `null`, and otherwise delegates to `ComponentRenderer.RenderComponentAsync<TComponent>(parameters)`. |

## `RenderContext`

```csharp:xmldocid
T:Pennington.Islands.RenderContext
```

_Record supplied to every `IIslandRenderer.RenderAsync` call; the scoped `SpaPageDataService` injects a single instance per request. Carries the pieces of request state islands typically need — base URL for building links, site title for titling, active locale for locale-aware content — without pulling in the full `HttpContext`._

### Members

_Declaration order matches the record's positional constructor._

| Name | Type | Description |
|---|---|---|
| `BaseUrl` | `UrlPath` | Site base URL; matches `PenningtonOptions.CanonicalBaseUrl` at build time and is the value islands should prepend when constructing absolute links. |
| `SiteTitle` | `string` | Site title from `PenningtonOptions.SiteTitle`; supplied to islands that render page headings or `<title>` fragments. |
| `Locale` | `string?` | Active locale code for the current request, or `null` when `LocalizationOptions.IsMultiLocale` is false; renderers that vary by locale read this value. |

## `SpaEnvelope`

```csharp:xmldocid
T:Pennington.Islands.SpaEnvelope
```

_Record describing the server-side shape of the per-route payload — title, optional description, optional `SocialMetadata`, and the `ImmutableDictionary<string, string>` of island-name → HTML. The JSON served at `SpaNavigationOptions.DataPath` (default `/_spa-data`) is the serialized form of the sibling `SpaEnvelopeDto` (`Pennington.Islands.SpaEnvelopeDto`), which flattens the dictionary and carries `Diagnostics` plus an optional `Reload` flag — consult that type for the wire contract, not this one._

### Members

_Declaration order matches the record's positional constructor._

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Page title surfaced in `<title>` on SPA navigation; the client engine writes it into `document.title` on `spa:commit`. |
| `Description` | `string?` | Page description; optional, used for `<meta name="description">` updates on navigation. |
| `Social` | `SocialMetadata?` | Per-page social-card metadata (from the `RenderedItem`'s `RenderedContent.Social`); optional and not currently serialized to the wire DTO. |
| `Islands` | `ImmutableDictionary<string, string>` | Map of island name → rendered HTML, one entry per `IIslandRenderer` that returned a non-empty string for the route. |

## `data-spa-*` attribute reference

_Lookup table for the attribute surface the client engine (`src/Pennington.UI/wwwroot/spa-engine.js`) reads. Attributes are grouped by the element that carries them and ordered alphabetically within each group._

| Attribute | Applies to | Values | Description |
|---|---|---|---|
| `data-spa-island` | Any element (conventionally `<article>`, `<aside>`, `<div>`) | Island name matching `IIslandRenderer.IslandName` | Marks the element as an island slot; the engine replaces its `innerHTML` with `SpaEnvelope.Islands[name]` on navigation. |
| `data-spa-loading` | An element carrying `data-spa-island` | `"skeleton"`, `"clear"`, `"keep"` (default) | Loading behaviour while the next envelope is in flight — `skeleton` injects a shimmer (or the matching `<template data-spa-skeleton-for="…">` if present), `clear` empties the slot, `keep` leaves the previous content in place. |
| `data-spa-skeleton-for` | `<template>` | Island name matching `data-spa-island` | Pairs a `<template>` with a skeleton-loading island; the template's content is cloned into the island instead of the default shimmer when `data-spa-loading="skeleton"`. |
| `data-spa-reload` | `<a>` | Presence only (boolean attribute) | Marks a link as "do not intercept" — the engine performs a full-document reload for this anchor instead of fetching the JSON envelope. |
| `data-spa-data-path` | `<html>` | URL path (default `/_spa-data`) | Overrides the path the engine fetches envelopes from; must match `SpaNavigationOptions.DataPath`. |
| `data-spa-skeleton-delay` | `<html>` | Integer milliseconds (default `100`) | Milliseconds the engine waits before showing a skeleton — shorter fetches skip the skeleton entirely to avoid flicker. |
| `data-spa-min-skeleton` | `<html>` | Integer milliseconds (default `250`) | Minimum time a skeleton stays visible once shown, to prevent sub-frame flashes of loading UI. |
| `data-base-url` | `<body>` | URL path | Stamped by `BaseUrlHtmlRewriter`; the engine strips this prefix from `location.pathname` when computing a route's `SpaSlug`. Not an island attribute per se — listed here because `spa-engine.js` reads it. |

## Example

_The canonical subclass of `RazorIslandRenderer<TComponent>` from the extensibility lab — overrides `IslandName`, implements `BuildParametersAsync`, and returns `null` to skip the island on routes that do not carry a matching `data-spa-island` slot. Registered via `options.Islands.Register<ChartIslandRenderer>("chart")`._

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.ChartIslandRenderer
```

_Reference shape for a Razor-backed island renderer; implementation walkthrough lives in the how-to._

## See also

- How-to: [Register an island renderer](/how-to/extensibility/island-renderer)
- Related reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](/reference/options/auxiliary-options)
- Related reference: [Routing types](/reference/extension-points/routing)
- Background: [SPA navigation and island architecture](/explanation/spa/islands)
