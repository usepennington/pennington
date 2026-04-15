---
title: "Register an island renderer"
description: "Wire a server-rendered SPA island by implementing IIslandRenderer (or subclassing RazorIslandRenderer<T>), registering it on IslandsOptions, and marking the target region with data-spa-island in your content."
uid: how-to.extensibility.island-renderer
order: 203060
sectionLabel: Extensibility
tags: [extensibility, islands, spa, razor-components]
---

Use an island renderer when one region of a content page — a chart, a live counter, a comment thread — must be server-rendered by Razor on first load and re-rendered from JSON on client-side navigation without a full page reload. The region's content must depend on the current route. When every page needs the same chrome, reach for an `IResponseProcessor` or a layout slot instead.

## Assumptions

- You have a working Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- SPA navigation is already wired: `builder.Services.AddSpaNavigation()` plus `app.UseSpaNavigation()`, and your layout emits the `data-spa-*` attributes the client script expects
- You have a Razor component (or plain HTML string) ready to render into the island slot
- You understand the content pipeline at a conceptual level (<xref:explanation.core.content-pipeline>)

A working reference: `examples/ExtensibilityLabExample` — `ChartIslandRenderer`, `Components/ChartIsland.razor`, and `Content/chart-demo.md` together form the minimal island.

---

## Steps

### 1. Build the Razor component the island will render

Create a Razor component whose `[Parameter]` surface matches the dictionary your renderer will produce. The component should be pure presentation — it takes its data through parameters and fetches nothing itself. Every value it touches must be passable through the `IDictionary<string, object?>` parameters payload, because that is what `RazorIslandRenderer<T>` hands to the `ComponentRenderer`.

```razor:path
examples/ExtensibilityLabExample/Components/ChartIsland.razor
```

### 2. Subclass `RazorIslandRenderer<TComponent>`

Derive from [`RazorIslandRenderer<T>`](xref:reference.extension-points.islands) rather than implementing [`IIslandRenderer`](xref:reference.extension-points.islands) directly. The base class wires the `ComponentRenderer` call, leaving `IslandName` and `BuildParametersAsync` as the only members to override. Reach for `IIslandRenderer.RenderAsync` only when emitting a non-Razor fragment — a pre-rendered string, a cached snippet, or a remote include.

```csharp:xmldocid
T:ExtensibilityLabExample.ChartIslandRenderer
```

### 3. Expose `IslandName` and gate parameters on the route

`IslandName` is the key the SPA envelope uses for this island, and it must match the `data-spa-island` attribute on the markup. `BuildParametersAsync` receives the [`ContentRoute`](xref:reference.extension-points.routing) for the page being rendered — inspect `CanonicalPath` and return `null` for any route that does not carry this island so the base class skips rendering. Returning parameters on every route wastes work and produces orphan HTML in pages that have no slot to hold it.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ChartIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute)
```

### 4. Register the renderer on `IslandsOptions`

Call `options.Islands.Register<TRenderer>("islandName")` inside your `AddPennington` configuration. The generic type argument is the renderer; the string is both the `data-spa-island` attribute value and the key `SpaPageDataService` writes into the `islands` slot of the JSON envelope — the two must agree exactly. Register one entry per island. The dictionary is keyed by name, so registering twice with the same name replaces the earlier entry.

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

### 5. Author the `data-spa-island` slot in your content

Wrap the server-rendered region in an element that carries `data-spa-island="islandName"`. Nothing else is required — the SPA runtime replaces the element's innerHTML on navigation, and on first load the renderer's output is already there. Keep a `<noscript>` fallback or a plain-markup default inside the slot so the page still makes sense before the island hydrates.

```markdown:path
examples/ExtensibilityLabExample/Content/chart-demo.md
```

### 6. Keep `AddSpaNavigation` / `UseSpaNavigation` wired

Islands run because `SpaNavigationContentService` emits per-page envelopes at `/_spa-data/{slug}.json`, and the `ComponentRenderer` the renderer depends on is registered as a scoped service alongside it. If either line is missing from `Program.cs` the renderer never runs — even on first load — because the DocSite content island short-circuits without its services.

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
