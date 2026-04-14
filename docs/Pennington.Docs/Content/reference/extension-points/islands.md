---
title: "Island rendering interfaces"
description: "The IIslandRenderer contract, RazorIslandRenderer<T> base, SpaEnvelope shape, RenderContext, and the data-spa-* attribute surface."
section: "extension-points"
order: 40
tags: []
uid: reference.extension-points.islands
isDraft: true
search: false
llms: false
---

> **In this page.** Covers: `IIslandRenderer`, `RazorIslandRenderer<T>`, `SpaEnvelope`, `RenderContext`, and the `data-spa-*` attribute surface.
>
> **Not in this page.** Does not cover: the SPA client-side script (see Explanation).

## Summary

- The server-side contracts used to contribute HTML fragments ("islands") to a `SpaEnvelope` for SPA navigation.
- Namespace `Pennington.Islands` (project `src/Pennington/Islands/`); browser attributes consumed by `src/Pennington.UI/wwwroot/spa-engine.js`.

## `IIslandRenderer`

### Declaration

```csharp:xmldocid
T:Pennington.Islands.IIslandRenderer
```

### Members

| Name | Type | Description |
|---|---|---|
| `IslandName` | `string` | Key under which the produced HTML is stored in `SpaEnvelope.Islands`. Must match the `data-spa-island` attribute value on the target DOM region. |
| `RenderAsync(ContentRoute, RenderContext)` | `Task<string>` | Produces the HTML fragment for the island on a given route. Returning `""` omits the island from the envelope. |

## `RazorIslandRenderer<TComponent>`

### Declaration

```csharp:xmldocid
T:Pennington.Islands.RazorIslandRenderer`1
```

### Type parameters

| Name | Constraint | Description |
|---|---|---|
| `TComponent` | `IComponent` | The Razor component rendered for each invocation. |

### Members

| Name | Type | Description |
|---|---|---|
| `IslandName` | `abstract string` | Overridden by subclasses to name the island. |
| `BuildParametersAsync(ContentRoute)` | `protected abstract Task<IDictionary<string, object?>?>` | Builds the parameter dictionary for `TComponent`. Returning `null` skips the island (empty string is returned from `RenderAsync`). |
| `RenderAsync(ContentRoute, RenderContext)` | `Task<string>` | Invokes `BuildParametersAsync`, then delegates to `ComponentRenderer.RenderComponentAsync<TComponent>`. |

### Dependencies

| Name | Type | Description |
|---|---|---|
| `renderer` | `ComponentRenderer` | Scoped service injected by primary-constructor parameter; performs the actual Blazor render. |

## `SpaEnvelope`

### Declaration

```csharp:xmldocid
T:Pennington.Islands.SpaEnvelope
```

### Members

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Page title. Serialized as `title`. |
| `Description` | `string?` | Page description. Serialized as `description`; omitted when `null`. |
| `Social` | `SocialMetadata?` | Social-card metadata for the page. |
| `Islands` | `ImmutableDictionary<string, string>` | Map of `IslandName` -> HTML fragment. Serialized as `islands`. |

### JSON shape (as emitted to `/_spa-data/{slug}.json`)

| Field | Type | Description |
|---|---|---|
| `title` | `string` | From `SpaEnvelope.Title`. |
| `description` | `string?` | From `SpaEnvelope.Description`. |
| `islands` | `object` | Property name per island, value is the HTML fragment. |
| `diagnostics` | `Diagnostic[]?` | Per-request diagnostics when `DiagnosticContext.HasAny` is true. |
| `reload` | `bool?` | `true` when no renderers produced content; signals the client to perform a full page load. |

## `RenderContext`

### Declaration

```csharp:xmldocid
T:Pennington.Islands.RenderContext
```

### Members

| Name | Type | Description |
|---|---|---|
| `BaseUrl` | `UrlPath` | Canonical site base URL. Defaults to `PenningtonOptions.CanonicalBaseUrl` or `"/"`. |
| `SiteTitle` | `string` | Site title. Defaults to `PenningtonOptions.SiteTitle` or `""`. |
| `Locale` | `string?` | Active locale code, or `null` when locale-agnostic. |

## `data-spa-*` attribute surface

Attributes read by `src/Pennington.UI/wwwroot/spa-engine.js`.

| Name | Values | Purpose |
|---|---|---|
| `data-spa-island` | island name (matches `IIslandRenderer.IslandName`) | Marks a DOM region as an island target; its `innerHTML` is replaced on navigation. Applied to any element. |
| `data-spa-loading` | `skeleton` \| `clear` \| `keep` (default `keep`) | Loading-state mode for an island while data is in flight. Applied to the island element. |
| `data-spa-skeleton-for` | island name | On a `<template>` element; its content replaces the island when `data-spa-loading="skeleton"` is active. |
| `data-spa-reload` | (presence only) | On an `<a>` element; forces a full page load for that link instead of SPA navigation. |
| `data-spa-data-path` | URL path (default `/_spa-data`) | On `<html>`; overrides the envelope endpoint path. Mirrors `SpaNavigationOptions.DataPath`. |
| `data-spa-skeleton-delay` | integer milliseconds (default `100`) | On `<html>`; delay before showing the skeleton, so fast fetches skip it. |
| `data-spa-min-skeleton` | integer milliseconds (default `250`) | On `<html>`; minimum time the skeleton remains visible once shown. |
| `data-base-url` | URL path | On `<body>`; base-path prefix stripped from URLs when computing the slug. |

## Example

```csharp:xmldocid
T:SpaNavigationTutorialExample.Islands.ArticleIslandRenderer
```

A `RazorIslandRenderer<T>` subclass naming the island `"article"` and rendering an `ArticleContent` component; returning `null` from `BuildParametersAsync` omits the island for unknown routes.

## See also

- How-to: [Register an island renderer](/how-to/extensibility/island-renderer)
- Related reference: [DI and middleware extension methods](/reference/host/extensions)
- Background: [SPA navigation and island architecture](/explanation/spa/islands)
