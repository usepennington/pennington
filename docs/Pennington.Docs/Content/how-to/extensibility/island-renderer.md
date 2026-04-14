---
title: "Register an island renderer"
description: "Wire a server-rendered SPA island by implementing IIslandRenderer (or subclassing RazorIslandRenderer<T>), registering it on IslandsOptions, and marking the target region with data-spa-island in your content."
uid: how-to.extensibility.island-renderer
order: 60
sectionLabel: Extensibility
tags: [extensibility, islands, spa, razor-components]
---

> **In this page.** Implement `IIslandRenderer` (or subclass `RazorIslandRenderer<TComponent>`), register it with `IslandsOptions.Register<T>("islandName")`, and author matching `data-spa-island="islandName"` markup so the island is server-rendered on first hit and swapped in on SPA navigation.
>
> **Not in this page.** The full SPA data envelope — how `SpaPageDataService` builds `_spa-data/{slug}.json`, which islands map to which slots, and how the client hydrates them. See [_SPA navigation and island architecture_](xref:explanation.spa.islands) for that.

## When to use this

_Two to three sentences. The reader has a content page that already renders and wants one region of it — a chart, a live counter, a comment thread — to be server-rendered by Razor on first load and re-rendered from JSON on client-side navigation without a full page refresh. Reach for an island renderer when the region's content depends on the current route (or a small projection of it) and you want the HTML to come from the server rather than the browser. If every page needs the same chrome, you want an `IResponseProcessor` or a layout slot instead — not an island._

## Assumptions

_Short list. The reader already has a working Pennington host and understands that SPA navigation is opt-in via `AddSpaNavigation` / `UseSpaNavigation`. This page does not re-teach that wiring — it assumes the infrastructure is in place._

- You have a working Pennington site (see [_Create your first Pennington site_](xref:tutorials.getting-started.first-site) if not)
- SPA navigation is already wired: `builder.Services.AddSpaNavigation()` plus `app.UseSpaNavigation()`, and your layout emits the `data-spa-*` attributes the client script expects
- You have a Razor component (or plain HTML string) ready to render into the island slot
- You understand the four-stage content pipeline at a conceptual level ([_The content pipeline and union types_](xref:explanation.core.content-pipeline))

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `ChartIslandRenderer`, `Components/ChartIsland.razor`, and `Content/chart-demo.md` together form the minimal island. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Build the Razor component the island will render

_Create a normal Razor component whose `[Parameter]` surface matches the dictionary your renderer will produce. Keep it dumb — the component should be pure presentation, taking its data through parameters rather than fetching anything itself. Anything it touches must be serializable through the `IDictionary<string, object?>` parameters payload, because that same shape is what `RazorIslandRenderer<T>` hands to the `ComponentRenderer`._

```razor:path
examples/ExtensibilityLabExample/Components/ChartIsland.razor
```

### 2. Subclass `RazorIslandRenderer<TComponent>`

_Derive from [`RazorIslandRenderer<T>`](xref:reference.extension-points.islands) rather than implementing [`IIslandRenderer`](xref:reference.extension-points.islands) directly — the base class wires the `ComponentRenderer` call for you, leaving `IslandName` and `BuildParametersAsync` as the only members you override. Override `IIslandRenderer.RenderAsync` yourself only when you need to emit a non-Razor HTML fragment (a pre-rendered string, a cached snippet, a remote include)._

```csharp:xmldocid
T:ExtensibilityLabExample.ChartIslandRenderer
```

### 3. Expose `IslandName` and gate parameters on the route

_`IslandName` is the string the SPA envelope keys islands by, and it must match the `data-spa-island` attribute on the markup. `BuildParametersAsync` receives the [`ContentRoute`](xref:reference.extension-points.routing) for the page being rendered — inspect `CanonicalPath` and return `null` for any route that does not carry this island so the base class skips rendering entirely. Returning parameters on every route wastes work and can produce orphan HTML in pages that have no slot to hold it._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ChartIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute)
```

### 4. Register the renderer on `IslandsOptions`

_Call `options.Islands.Register<TRenderer>("islandName")` inside your `AddPennington` configuration. The generic type argument is the renderer; the string is both the `data-spa-island` attribute value and the key `SpaPageDataService` writes into the `islands` slot of the JSON envelope — the two must agree exactly. Register one entry per island; the dictionary is keyed by name so registering twice with the same name replaces the earlier entry._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

### 5. Author the `data-spa-island` slot in your content

_Wrap the server-rendered region in an element that carries `data-spa-island="islandName"`. Nothing else is required — the SPA runtime replaces the element's innerHTML on navigation, and on first load the renderer's output is already there. Keep a `<noscript>` fallback or a plain-markup default inside the slot so the page still makes sense before the island hydrates._

```markdown:path
examples/ExtensibilityLabExample/Content/chart-demo.md
```

### 6. Keep `AddSpaNavigation` / `UseSpaNavigation` wired

_Islands only get invoked because `SpaNavigationContentService` emits per-page envelopes at `/_spa-data/{slug}.json`, and the `ComponentRenderer` your renderer depends on is registered as a scoped service next to it. If either line is missing from `Program.cs` the renderer never runs — even on first load — because the DocSite content island also short-circuits._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/chart-demo/` — the chart `<figure>` is present in the initial HTML (view source, not devtools)
- Request `/_spa-data/chart-demo.json` directly — the response contains an `islands` object with a `chart` key whose value is the rendered HTML
- Navigate to `/chart-demo/` from another page via a link click — the region updates without a full page reload, and routes that do not carry `data-spa-island="chart"` show no chart HTML in their envelope

## Related

- Reference: [_Island rendering interfaces_](xref:reference.extension-points.islands)
- Reference: [_Routing types_](xref:reference.extension-points.routing)
- Background: [_SPA navigation and island architecture_](xref:explanation.spa.islands)
- Background: [_The content pipeline and union types_](xref:explanation.core.content-pipeline)
