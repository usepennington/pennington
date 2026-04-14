---
title: "SPA navigation and island architecture"
description: "Why Pennington ships a JSON envelope at a data path and selectively hydrates server-rendered islands instead of swapping full pages or going full SPA."
sectionLabel: "SPA and Islands"
order: 10
tags: [spa, islands, architecture, hydration]
uid: explanation.spa.islands
---

> **In this page.** Why Pennington serves a JSON envelope at a `_spa-data` path instead of full page HTML on navigation, how islands are hydrated selectively, and the skeleton/loading lifecycle (`data-spa-loading` modes).
>
> **Not in this page.** Registering an island — see the [how-to](/how-to/extensibility/island-renderer). The raw attribute list and renderer interfaces — see the [reference](/reference/extension-points/islands).

## The question

_One sentence, phrased as the reader's question — something like: "Why does in-site navigation fetch a tiny JSON file from `/_spa-data/...` instead of either reloading the full HTML page or booting a client-side SPA framework?" Keep it a single question; the rest of the page is the answer._

## Context

_Three to five sentences setting up the tradeoff the reader is walking into. Start with the two endpoints of the spectrum: classic server-rendered sites swap the whole document on every click (simple, full-fidelity, but heavy and visually jarring), while full SPAs hydrate the whole app in the browser (snappy, interactive, but ships a framework and fights SEO). Note that documentation sites sit awkwardly between the two — most of a page is static prose that does not benefit from client rendering, but a handful of regions (search, nav highlight, interactive widgets) genuinely want to survive across navigations. Close by previewing the position this page defends: Pennington serves every page server-rendered on first load, then on in-site clicks fetches a **small JSON envelope of pre-rendered island HTML** and swaps only those regions. Neither extreme; a middle path the rest of the page unpacks._

## How it works

_The narrative spine: envelope shape, island hydration, loading modes, ordering. Anchor with one or two signatures from `Pennington.Islands` so the reader can see the envelope is a real record. Do not drift into "how to register one" — that belongs in the how-to — and do not enumerate attributes — that belongs in the reference._

### The JSON envelope

_A few sentences. The first request to any URL returns full server-rendered HTML, exactly as it would without SPA — good for cold loads, good for crawlers, good when JavaScript is disabled. Once the browser has that page and the `spa-engine.js` script from `Pennington.UI` is loaded, the client intercepts same-origin link clicks and issues a GET for a sibling JSON document at `SpaNavigationOptions.DataPath` (default `/_spa-data/<slug>.json`). The endpoint is registered by `SpaNavigationExtensions.UseSpaNavigation` and produced by `SpaPageDataService`, which assembles an `SpaEnvelope` record carrying only `Title`, `Description`, optional `Social`, and an `Islands` dictionary of pre-rendered HTML fragments — no chrome, no layout, no script tags, no CSS. The envelope is typically a small fraction of the full-page HTML because the shell, header, footer, sidebar, and every asset reference are already in the DOM from the first load. Contrast this with a naive partial-HTML approach that round-trips the whole `<body>` — the envelope pays for exactly the regions that changed, and nothing more._

```csharp:xmldocid
T:Pennington.Islands.SpaEnvelope
```

_Optional — pull the envelope record so the reader sees how small it is: title, description, social metadata, and a string → string island map. If the prose already stood up, drop the fence._

### Island hydration

_A few sentences on selective hydration. The word "hydration" normally means shipping a framework runtime and re-running component constructors in the browser — in Pennington it means the far weaker act of swapping an HTML substring into a DOM node marked with `data-spa-island="<name>"`. The server did the render; the client only routes the fragment to the right slot. Each `IIslandRenderer` registered in the DI container produces one keyed HTML string per route; `SpaPageDataService` composes them into the `Islands` dictionary; the browser engine iterates island elements and replaces each `innerHTML` with the corresponding entry. Regions that are not islands — site chrome, navigation, scripts — stay put across navigations, which is the point: scroll position on the sidebar survives, focused elements outside the swapped region keep focus, and nothing re-downloads a framework runtime because there is no framework runtime._

```csharp:xmldocid
T:Pennington.Islands.IIslandRenderer
```

_Optional — the two-member contract (an `IslandName` and a `RenderAsync` returning a string) makes the "one server render, many client swaps" shape concrete. Drop if prose covered it._

### The loading lifecycle — `data-spa-loading` modes

_Two to four sentences. The round trip to fetch the envelope is small but not instant, and a click that does nothing visible for 200ms feels broken. Each island opts into one of three loading behaviours via `data-spa-loading`: `keep` (default — leave the previous HTML visible until the new HTML arrives, best for regions whose content is similar across pages), `clear` (empty the island immediately on navigation start, best for regions whose old content would actively mislead — e.g. an outline from the previous page), and `skeleton` (show a shimmer placeholder after a short delay, best for large regions where a visible holding pattern beats stale content). The skeleton delay is deliberately threshold-gated so navigations that resolve quickly never flash a placeholder; the engine waits past a configurable delay before swapping to skeleton, then holds the skeleton for a minimum duration so it doesn't strobe. The tradeoff across the three modes is a classic perceived-latency choice — stale, blank, or shimmer — and is made per-island because different regions want different answers._

### Why server-render first, then hydrate

_A few sentences closing the loop. Because every page is fully server-rendered on first load, the site works without JavaScript, is crawlable by search engines and LLM indexers byte-for-byte, and passes Core Web Vitals against real HTML rather than a blank shell waiting on a bundle. Because in-site navigation uses the same server-rendered HTML — just delivered in a JSON envelope and swapped into islands — there is no second rendering path to keep in sync with the first. The rendering that happens in the browser is string-to-DOM; the rendering that happens on the server is Razor-to-string. They are the same HTML, produced by the same pipeline, for both the first page load and every subsequent island swap. This is the same dev-vs-build invariant Pennington applies elsewhere — one renderer, consumed differently — and it is why adding an island does not mean adding a client-side component._

## Trade-offs

- **Cost — islands must be side-effect-free and round-trip over JSON.** Island HTML is assembled on the server per request, serialized as a JSON string, and re-inserted with `innerHTML` on the client. `<script>` tags inside islands do not execute on swap; stateful client widgets must hook `spa:commit` lifecycle events rather than assume `DOMContentLoaded` fires again. This rules out dropping arbitrary JS-heavy components into an island and expecting them to "just work."
- **Alternative considered — full client-side SPA (Blazor WebAssembly, React, etc.).** Would make every navigation instant at the cost of a multi-megabyte runtime on first load, a separate SEO story, and a second rendering path diverging from the server one. Rejected for a content engine where most of each page is prose the client doesn't need to render.
- **Alternative considered — full-document partial HTML (htmx-style `hx-swap="outerHTML"` on `<body>`).** Simpler than the envelope (no JSON layer) but pays for the whole document body on every navigation, including chrome that never changed, and leaves the client with no structured way to tell title/description/social metadata apart from island content. The envelope is strictly a superset: it's the partial HTML approach plus a typed metadata header plus per-region granularity.
- **Consequence — the set of islands is known at DI registration time.** A page cannot invent a new island on the fly; `SpaPageDataService` iterates the registered `IIslandRenderer` collection, and the host markup must declare matching `data-spa-island` attributes. That ceiling is the price of a statically-known envelope shape, and in exchange you get full server-side type safety on what an island renders.
- **Consequence — the `_spa-data` endpoint runs the same response processors as HTML pages, minus the HTML rewriters.** Xref resolution, diagnostics, and locale handling still apply inside island HTML (the handler resolves xrefs explicitly because the default HTML rewriter chain only fires on `text/html` responses). Treat the envelope as a first-class output of the pipeline, not a side channel.

## Further reading

- Reference: [Island rendering interfaces](/reference/extension-points/islands)
- How-to: [Register an island renderer](/how-to/extensibility/island-renderer)
- External: [Islands Architecture (Jason Miller)](https://jasonformat.com/islands-architecture/)
