---
title: Register an island renderer
description: Implement IIslandRenderer (or subclass RazorIslandRenderer), configure IslandsOptions.Register<T>("islandName"), and add matching data-spa-island markup.
section: extensibility
order: 60
tags: []
uid: how-to.extensibility.island-renderer
isDraft: true
search: false
llms: false
---

> **In this page.** Implementing `IIslandRenderer` (or subclassing `RazorIslandRenderer`), configuring `IslandsOptions.Register<T>("islandName")`, and adding matching `data-spa-island` markup.
>
> **Not in this page.** The full SPA data envelope — see Explanation.

## When to use this

- You have a Pennington site with SPA navigation wired up and want to add a new server-rendered region that updates on client-side navigation.
- Reach for this page when a page area (sidebar card, breadcrumb, per-route nav, metadata panel) should re-render from fresh route data without a full page load.

## Assumptions

- You have an existing Pennington site with `AddSpaNavigation()` and `UseSpaNavigation()` already wired (see the SPA navigation tutorial if not).
- You have `ComponentRenderer` registered as a scoped service for Razor-based islands.
- `MapRazorComponents<App>()` is in your `UsePennington` pipeline.
- A target Razor component exists (or you will create one) with `[Parameter]` properties.

To copy a working setup, see [`examples/SpaNavigationExample`](https://github.com/usepennington/pennington/tree/main/examples/SpaNavigationExample) or [`examples/SpaNavigationTutorialExample`](https://github.com/usepennington/pennington/tree/main/examples/SpaNavigationTutorialExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Create the target Razor component

- Place the component under a project folder such as `Islands/Components/` or `Slots/Components/`.
- Declare `[Parameter]` properties for each piece of data the island needs.
- Keep presentation only; data loading happens in the renderer.

```csharp:xmldocid
T:SpaNavigationTutorialExample.Islands.Components.ArticleContent
```

### 2. Subclass `RazorIslandRenderer<TComponent>`

- Inject whatever services the renderer needs (content lookup, navigation, etc.) plus `ComponentRenderer`.
- Override `IslandName` with the string clients will reference in `data-spa-island="..."`.
- Override `BuildParametersAsync(ContentRoute route)` to return a `Dictionary<string, object?>` of parameters, or `null` to skip this island for this route.

```csharp:xmldocid
T:SpaNavigationTutorialExample.Islands.ArticleIslandRenderer
```

### 3. Register the renderer with `IslandsOptions`

- Inside `AddPennington(...)`, call `penn.Islands.Register<TRenderer>("islandName")` where the name matches `IslandName` and the markup attribute.
- Repeat once per island; multiple islands can coexist on one page.

```csharp:xmldocid,bodyonly
M:SpaNavigationExample.Program.Main
```

### 4. Add matching `data-spa-island` markup in your layout

- Mark each DOM region with `data-spa-island="islandName"`.
- Optionally add `data-spa-loading="skeleton|keep|clear"` to control the transition behavior during navigation.
- For skeleton mode, supply a `<template data-spa-skeleton-for="islandName">` sibling with the placeholder markup.

```razor
<article data-spa-island="article" data-spa-loading="skeleton">
    @Body
</article>

<template data-spa-skeleton-for="article">
    <div class="animate-pulse">...</div>
</template>
```

### 5. Implement `IIslandRenderer` directly (non-Razor islands only)

- Skip this step when subclassing `RazorIslandRenderer<T>`.
- Use this path when the island produces HTML without Razor (for example, a pre-serialized JSON fragment or static snippet).
- Return an empty string from `RenderAsync` to omit the island for a given route.

```csharp
public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
```

---

## Verify

- Run `dotnet run` and visit a content URL; view-source the page and confirm the `data-spa-island="islandName"` element is populated on first render.
- Request `/_spa-data/<slug>.json` and confirm the response contains your island name as a key in the `islands` object.
- Click a link to another content page and confirm the island region updates without a full reload.

## Related

- Reference: [Island rendering interfaces](/reference/extension-points/islands)
- Reference: [`IslandsOptions`](/reference/options/auxiliary-options)
- Background: [SPA navigation and island architecture](/explanation/spa/islands)
