---
title: "SPA navigation and island architecture"
description: "Why Pennington serves a JSON envelope at a _spa-data path instead of full page HTML on navigation, how islands are hydrated selectively, and the skeleton/loading lifecycle."
section: spa
order: 10
tags: []
uid: explanation.spa.islands
isDraft: true
search: false
llms: false
---

> **In this page.** Why Pennington serves a JSON envelope at a `_spa-data` path instead of full page HTML on navigation, how islands are hydrated selectively, and the skeleton/loading lifecycle (`data-spa-loading` modes).
>
> **Not in this page.** Registering an island (see How-Tos) or the raw attribute list (see Reference).

## The question

- One sentence: _why does Pennington ship a JSON envelope on in-site navigation, and how does that interact with server-rendered islands?_

## Context

- Pennington is a static-first content engine — every page is fully rendered HTML on first visit.
- The naive alternative for in-site navigation is "fetch the next page's HTML and swap `<main>`," but that re-parses unchanged chrome (header, nav, sidebar, styles) on every click.
- Explored alternatives and why they lost:
  - Full page reload — correct but flashes, loses scroll state on back/forward, and drops view transitions.
  - HTML-fragment swap — still requires server-side rendering of the surrounding shell per navigation.
  - Client-rendered SPA — defeats static hosting and the unified dev/build code path.
- Pennington's shape: keep first-load HTML, but for subsequent navigation serve a small pre-generated JSON file describing only what changes.

## How it works

Budget: 500–1,500 words, single concept (JSON envelope + island hydration + loading lifecycle).

### Mechanism 1 — The `_spa-data` JSON envelope

- Path is configurable via `SpaNavigationOptions.DataPath`; default is `/_spa-data`.
- One JSON file per page: `/_spa-data/{slug}.json`, slug derived from canonical path (`SpaSlug`).
- Generated, not dynamic-only: `SpaNavigationContentService` participates in `IContentService` discovery so the static-build crawler writes a `.json` file next to every `.html` output.
- This preserves the unified dev/build code path — dev serves the endpoint via `MapGet`, build fetches it over HTTP like any other page.
- Envelope shape (`SpaEnvelopeDto` / `SpaEnvelopeSerializer`):
  - `title` — page title (client prepends site title).
  - `description` — meta/OG description.
  - `islands` — dictionary of `name -> rendered HTML fragment`.
  - `diagnostics?` — per-request diagnostics so the dev overlay still surfaces.
  - `reload?` — signal for "can't SPA-navigate, do a full reload."
- Anchor with a code fence if prose gets fuzzy:
  ```csharp:xmldocid
  T:Pennington.Islands.SpaEnvelopeDto
  ```

### Mechanism 2 — The islands model (selective hydration)

- An "island" is a DOM region marked `data-spa-island="name"` on the first-load HTML.
- Server side: each island is produced by an `IIslandRenderer` (`IslandName`, `RenderAsync(ContentRoute, RenderContext)`); `SpaPageDataService` walks every registered renderer and returns only those that produce non-empty HTML.
- Client side (`spa-engine.js`): `discoverIslands()` finds `[data-spa-island]` at navigation time and updates each island's `innerHTML` from `envelope.islands[name]`.
- Chrome outside any island (header, nav, footer, `<head>` chrome) is never touched — that's the whole point: the JSON envelope is explicitly the per-page delta.
- View transitions: engine auto-assigns a unique `viewTransitionName` per island so animations are per-region, not per-page.
- If zero islands are on the page, the engine falls back to a full location change — there is nothing useful to swap.

### Mechanism 3 — Loading lifecycle and `data-spa-loading` modes

- The engine races the envelope fetch against a short threshold (`data-spa-skeleton-delay`, default ~100 ms). If data arrives first, islands swap directly — no placeholder flicker on fast networks or prefetched hits.
- If the fetch is slow, each island shows a loading state chosen by its `data-spa-loading` attribute. Three modes, defined in `spa-engine.js`:
  - `skeleton` — replace contents with a shimmer placeholder (or a custom `<template data-spa-skeleton-for="…">` if provided).
  - `clear` — empty the island immediately.
  - `keep` — leave the previous content visible until the new data arrives (default).
- Minimum skeleton hold (`data-spa-min-skeleton`, default ~250 ms) avoids "flash of skeleton": once a placeholder is shown, it stays long enough to read as intentional.
- Prefetch-on-hover/focus warms `_prefetchCache` so the common case skips the skeleton entirely.
- Accessibility: a `role=status` live region announces each navigation; focus moves to the first heading inside the first island so keyboard users land in context.
- Lifecycle events dispatched on `document`: `spa:before-navigate`, `spa:commit`, `spa:diagnostics`.

## Trade-offs

- **Cost.** Every page ships twice on build — `.html` for first load, `.json` for subsequent hops. Disk and CDN bytes grow roughly linearly.
- **Alternative considered: HTML-fragment navigation.** Rejected because it either requires a live server (breaks static hosting) or pre-renders the chrome into every fragment file (larger than the JSON envelope and no cheaper at runtime).
- **Alternative considered: client-rendered SPA.** Rejected because it would fork the dev-vs-build code path and move rendering off the server — losing the "everything runs through one HTTP pipeline" invariant.
- **Consequence: the chrome must live outside islands.** Anything expected to change on navigation must be inside a `data-spa-island` region; anything outside is effectively frozen until a full reload. The `reload` envelope flag exists as the escape hatch for navigations that can't be done via swap.
- **Consequence: response processors don't run on JSON.** The SPA endpoint runs xref resolution explicitly on island HTML before serializing; other rewriters (locale links, base URL) are applied at island-render time, not as a post-process of the envelope.

## Further reading

- How-to: [Register an island renderer](/how-to/extensibility/island-renderer)
- Reference: [Island rendering interfaces](/reference/extension-points/islands)
- Explanation: [Dev mode and build mode share one code path](/explanation/core/dev-vs-build)
