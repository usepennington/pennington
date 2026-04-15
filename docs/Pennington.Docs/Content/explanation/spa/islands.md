---
title: "SPA navigation and island architecture"
description: "Why Pennington ships a JSON envelope at a data path and selectively hydrates server-rendered islands instead of swapping full pages or going full SPA."
sectionLabel: "SPA and Islands"
order: 304010
tags: [spa, islands, architecture, hydration]
uid: explanation.spa.islands
---

Why does in-site navigation fetch a small JSON file from `/_spa-data/...` instead of either reloading the full HTML page or booting a client-side SPA framework?

## Context

Classic server-rendered sites swap the whole document on every click — simple, full-fidelity, but heavy and visually jarring when the shared chrome redrawing is indistinguishable from the content changing. Full SPAs take the opposite position: hydrate the entire app in the browser, make every navigation instant, and pay for it with a multi-megabyte runtime on first load, a separate SEO story, and a rendering path that diverges from whatever the server would have produced. Documentation sites sit awkwardly between those two extremes. Most of each page is static prose that does not benefit from client rendering, yet a handful of regions — the active-page highlight in the sidebar, an interactive search overlay, a page outline — genuinely want to survive navigation without re-initializing from scratch.

The position Pennington takes is neither extreme. Every page arrives fully server-rendered on first load. When the visitor clicks an in-site link, the browser fetches a small JSON envelope of pre-rendered HTML fragments — one per registered island — and swaps only those regions into the existing DOM. The shell, sidebar, scripts, and stylesheets never move. The rest of this page unpacks why that shape was chosen and what it costs.

## How it works

### The JSON envelope

The first request to any URL returns complete server-rendered HTML, exactly as it would without SPA support in the picture — good for cold loads, good for crawlers, functional when JavaScript is disabled. Once the browser has that page and the `spa-engine.js` script from `Pennington.UI` is active, the client intercepts same-origin link clicks and issues a GET for a sibling JSON document at the configured data path (default `/_spa-data/<slug>.json`). `SpaPageDataService` assembles an `SpaEnvelope` record carrying only a title, description, optional social metadata, and an `Islands` dictionary of pre-rendered HTML strings — no chrome, no layout, no asset references.

```csharp:xmldocid
T:Pennington.Islands.SpaEnvelope
```

The envelope is typically a small fraction of the full-page HTML because everything outside the island slots is already in the DOM from the first load. Contrast this with a naive partial-HTML approach that round-trips the whole `<body>`: the envelope pays for exactly the regions that changed, and nothing more. The typed metadata header also separates concerns cleanly — title, description, and social data are first-class fields rather than HTML the client has to parse out of a `<head>` fragment.

### Island hydration

The word "hydration" in the broader ecosystem usually implies shipping a framework runtime and re-running component constructors in the browser to attach event listeners to server-rendered markup. In Pennington it means something far more modest: swapping an HTML substring into a DOM node marked with `data-spa-island="<name>"`. The server did the render; the client routes the fragment to the right slot.

```csharp:xmldocid
T:Pennington.Islands.IIslandRenderer
```

Each `IIslandRenderer` registered in DI produces one keyed HTML string per route. `SpaPageDataService` composes them into the `Islands` dictionary. The browser engine iterates elements with `data-spa-island` attributes and replaces each one's `innerHTML` with the matching entry. Regions outside islands — chrome, navigation, scripts — stay put across navigations. Scroll position on the sidebar survives. Focused elements outside the swapped region keep focus. Nothing re-downloads a runtime because there is no runtime.

### The loading lifecycle

The round trip to fetch the envelope is small but not instant, and a click that does nothing visible for 200ms feels broken. Each island opts into one of three loading behaviors via a `data-spa-loading` attribute. The `keep` mode (the default) leaves the previous HTML visible until new HTML arrives, which works well for regions whose content is broadly similar across pages. The `clear` mode empties the island immediately on navigation start, which makes sense for regions whose stale content would actively mislead — an outline panel showing headings from the previous page, for example. The `skeleton` mode shows a shimmer placeholder, but only after a configurable threshold has elapsed, so navigations that resolve quickly never flash a placeholder at all; the engine then holds the skeleton for a minimum duration to avoid strobing.

The tradeoff across those three modes is a classic perceived-latency question — stale, blank, or shimmer — and the answer differs by region. That is why the choice is made per-island rather than globally.

### Why server-render first, then hydrate

Because every page is fully server-rendered on first load, the site works without JavaScript, is crawlable by search engines and LLM indexers byte-for-byte, and satisfies Core Web Vitals against real HTML rather than a blank shell waiting on a bundle. Because in-site navigation uses the same server-rendered HTML — delivered in a JSON envelope and swapped into islands — there is no second rendering path to keep in sync with the first. The rendering that happens in the browser is string-to-DOM assignment. The rendering that happens on the server is Razor-to-string. They produce the same HTML, through the same pipeline, for both the initial load and every subsequent island swap. Adding an island does not mean adding a client-side component.

## Trade-offs

- **Cost — islands must be side-effect-free and round-trip over JSON.** Island HTML is assembled on the server per request, serialized as a JSON string, and re-inserted with `innerHTML` on the client. `<script>` tags inside islands do not execute on swap; stateful client widgets must hook `spa:commit` lifecycle events rather than assume `DOMContentLoaded` fires again. This rules out dropping arbitrary JS-heavy components into an island and expecting them to "just work."
- **Alternative considered — full client-side SPA (Blazor WebAssembly, React, etc.).** Would make every navigation instant at the cost of a multi-megabyte runtime on first load, a separate SEO story, and a second rendering path diverging from the server one. Rejected for a content engine where most of each page is prose the client doesn't need to render.
- **Alternative considered — full-document partial HTML (htmx-style `hx-swap="outerHTML"` on `<body>`).** Simpler than the envelope (no JSON layer) but pays for the whole document body on every navigation, including chrome that never changed, and leaves the client with no structured way to tell title/description/social metadata apart from island content. The envelope is strictly a superset: it's the partial HTML approach plus a typed metadata header plus per-region granularity.
- **Consequence — the set of islands is known at DI registration time.** A page cannot invent a new island on the fly; `SpaPageDataService` iterates the registered `IIslandRenderer` collection, and the host markup must declare matching `data-spa-island` attributes. That ceiling is the price of a statically-known envelope shape, and in exchange you get full server-side type safety on what an island renders.
- **Consequence — the `_spa-data` endpoint runs the same response processors as HTML pages, minus the HTML rewriters.** Xref resolution, diagnostics, and locale handling still apply inside island HTML (the handler resolves xrefs explicitly because the default HTML rewriter chain only fires on `text/html` responses). Treat the envelope as a first-class output of the pipeline, not a side channel.

## Further reading

- Reference: [Island rendering interfaces](xref:reference.extension-points.islands)
- How-to: [Register an island renderer](xref:how-to.extensibility.island-renderer)
- External: [Islands Architecture (Jason Miller)](https://jasonformat.com/islands-architecture/)
