---
title: "Hydrate a Razor component as a client island"
description: "Subclass RazorIslandRenderer<T> to server-render a Razor component into a data-spa-island slot and re-render it from JSON on SPA navigation."
uid: how-to.extensibility.island-renderer
order: 203060
sectionLabel: Extensibility
tags: [extensibility, islands, spa, razor-components]
---

To hydrate a Razor component as a client island — a chart, a live counter, a comment thread that needs to be server-rendered on first load and re-rendered from JSON on SPA navigation — subclass `RazorIslandRenderer<T>` and register it on `IslandsOptions`. The region's content depends on the current route. When every page needs the same chrome, reach for an `IResponseProcessor` or a layout slot instead.

## Before you begin

- A working Pennington site (see <xref:tutorials.getting-started.first-site> if not).
- SPA navigation already wired: `builder.Services.AddSpaNavigation()` plus `app.UseSpaNavigation()`, and the layout emits the `data-spa-*` attributes the client script expects.
- A Razor component (or plain HTML string) ready to render into the island slot.
- Familiarity with the content pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).

A working reference: `examples/ExtensibilityLabExample` — `ChartIslandRenderer`, `Components/ChartIsland.razor`, and `Content/chart-demo.md` together form the minimal island.

## Build the Razor component

Create a Razor component whose `[Parameter]` surface matches the dictionary the renderer will produce. The component should be pure presentation — it takes its data through parameters and fetches nothing itself. Every value it touches needs to be passable through the `IDictionary<string, object?>` parameters payload, because that is what `RazorIslandRenderer<T>` hands to the `ComponentRenderer`.

```razor:path
examples/ExtensibilityLabExample/Components/ChartIsland.razor
```

## Implement the renderer

Derive from [`RazorIslandRenderer<T>`](xref:reference.api.i-island-renderer) rather than implementing [`IIslandRenderer`](xref:reference.api.i-island-renderer) directly. The base class wires the `ComponentRenderer` call, leaving `IslandName` and `BuildParametersAsync` as the only members to override. Reach for `IIslandRenderer.RenderAsync` only to emit a non-Razor fragment — a pre-rendered string, a cached snippet, or a remote include.

```csharp:xmldocid
T:ExtensibilityLabExample.ChartIslandRenderer
```

`IslandName` is the key the SPA envelope uses for this island, and it has to match the `data-spa-island` attribute on the markup. `BuildParametersAsync` receives the [`ContentRoute`](xref:reference.api.content-route) for the page being rendered — inspect `CanonicalPath` and return `null` for any route that does not carry this island so the base class skips rendering. Returning parameters on every route wastes work and produces orphan HTML in pages with no slot to hold it.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ChartIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute)
```

## Author the slot in your content

Wrap the server-rendered region in an element that carries `data-spa-island="islandName"`. Nothing else is required — the SPA runtime replaces the element's innerHTML on navigation, and on first load the renderer's output is already there. Keep a `<noscript>` fallback or a plain-markup default inside the slot so the page still reads sensibly before the island hydrates.

```markdown:path
examples/ExtensibilityLabExample/Content/chart-demo.md
```

## Register the implementation

Call `options.Islands.Register<TRenderer>("islandName")` inside the `AddPennington` configuration. The generic type argument is the renderer; the string is both the `data-spa-island` attribute value and the key `SpaPageDataService` writes into the `islands` slot of the JSON envelope — the two have to agree exactly. Register one entry per island. The dictionary is keyed by name, so registering twice with the same name replaces the earlier entry.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.Islands.Register<ChartIslandRenderer>("chart");
    // ...
});
```

Islands run because `SpaNavigationContentService` emits per-page envelopes at `/_spa-data/{slug}.json`, and the `ComponentRenderer` the renderer depends on is registered as a scoped service alongside it. If either line is missing from `Program.cs` the renderer never runs — even on first load — because the DocSite content island short-circuits without its services.

```csharp
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddSpaNavigation();

// ...

app.UseSpaNavigation();
```

## Result

On first load of `/chart-demo/`, the `ChartIsland` component is rendered directly into the page inside the `data-spa-island="chart"` element — the chart `<figure>` appears in view-source, not just in devtools. On client-side navigation back to `/chart-demo/` from another page, the SPA runtime fetches `/_spa-data/chart-demo.json`, reads the `islands.chart` HTML from the envelope, and swaps it into the same slot without a full page reload. Routes whose `CanonicalPath` does not match `/chart-demo` get a `null` parameters dictionary and no chart HTML in their envelope.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/chart-demo/` — the chart `<figure>` is present in the initial HTML (view source, not devtools).
- Request `/_spa-data/chart-demo.json` directly — the response contains an `islands` object with a `chart` key whose value is the rendered HTML.
- Navigate to `/chart-demo/` from another page via a link click — the region updates without a full page reload, and routes that do not carry `data-spa-island="chart"` show no chart HTML in their envelope.

## Related

- Reference: [Island rendering interfaces](xref:reference.api.i-island-renderer)
- Reference: [Routing types](xref:reference.api.content-route)
- Background: [SPA navigation and island architecture](xref:explanation.spa.islands)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
