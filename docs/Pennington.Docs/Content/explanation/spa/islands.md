---
title: "SPA navigation through region swaps"
description: "Why Pennington's SPA fetches the canonical HTML page and swaps marked regions — one render path, one set of rewriters, no parallel JSON envelope."
sectionLabel: "SPA"
order: 304010
tags: [spa, navigation, regions, hydration]
uid: explanation.spa.islands
---

Why does in-site navigation fetch the same URL the address bar shows and parse it client-side, instead of round-tripping a small JSON envelope or letting the browser do a full reload?

## Context

Classic server-rendered sites swap the whole document on every click — simple, full-fidelity, but heavy and visually jarring when the shared chrome redrawing is indistinguishable from the content changing. Full SPAs hydrate the entire app in the browser, make every navigation instant, and pay for it with a multi-megabyte runtime on first load, a separate SEO story, and a rendering path that diverges from whatever the server would have produced. Documentation sites sit awkwardly between those two extremes. Most of each page is static prose that does not benefit from client rendering, yet a handful of regions — the active-page highlight in the sidebar, the page outline, the language switcher — genuinely want to update without re-initializing from scratch.

Pennington takes a third position. Every page is fully server-rendered on first load. When the visitor clicks an in-site link, the browser fetches the same URL — the canonical HTML page — parses it with `DOMParser`, swaps regions tagged `data-spa-region` from the new document into the current one, and merges head metadata. There is one rendering path, one set of HTML rewriters, and no parallel envelope to keep in sync.

## How it works

### One render, many slices

The first request to any URL returns complete server-rendered HTML, exactly as it would without SPA support — good for cold loads, good for crawlers, functional when JavaScript is disabled. Once the browser has that page and the `spa-engine.js` script from `Pennington.UI` is active, the client intercepts same-origin link clicks and re-fetches the destination URL. The response is parsed into a `Document` and used as a source of truth for both the regions that change and the head deltas that follow them.

Every server-side rewriter — xref resolution, locale-aware link rewriting, base-URL prefixing, anything else registered as `IHtmlResponseRewriter` — applies to that response by default, because it travelled through the same `ResponseProcessingMiddleware` as a fresh-tab visit. There is no second pipeline to mirror.

### The `data-spa-region` contract

Anywhere in the layout that should update on navigation gets a `data-spa-region="name"` attribute. The DocSite layout marks three regions out of the box:

- `content` — the article body, including breadcrumbs and prev/next links.
- `sidebar` — the table of contents, with the per-page active state.
- `header` — the top bar, including the language switcher and area pills.

Anything outside a marked region — the outer page chrome, the mobile menu's expanded state, scroll position — stays put.

The client picks the regions in the current document, finds elements with the same name in the parsed response, and swaps `innerHTML`. If the set of regions does not match — for example, navigating from a `MainLayout` page (three regions) to a `FullWidthLayout` page (only `content`) — the engine triggers a full page load. That is the explicit layout boundary, not a silent half-update.

### Head merging

The `<head>` of the parsed response is the source of truth for everything page-specific. The client updates the title and a fixed list of managed tags: the description meta, OpenGraph and Twitter card metadata, the canonical link, hreflang alternates, and JSON-LD scripts. Stylesheet `<link>` elements are merged by href — any new ones append to the head before the region swap so the browser has the rules ready when the new content paints. A stylesheet tagged `data-spa-reload` re-fetches with a cache buster on every navigation, the opt-in workaround for JIT stylesheets like MonorailCSS in dev where the URL stays constant but the contents diverge per page.

### Loading lifecycle

The round trip is small but not instant, and a click that does nothing visible for 200ms feels broken. Each region opts into a loading behavior via `data-spa-loading`. The `keep` mode (the default) leaves the previous content visible until new HTML arrives, which works for regions whose content is broadly similar across pages. The `clear` mode empties the region immediately on navigation, which suits regions whose stale content would mislead — an outline panel showing headings from the previous page. The `skeleton` mode shows a shimmer placeholder, but only after a configurable threshold elapses, so navigations that resolve quickly never flash; the engine then holds the skeleton for a minimum duration to avoid strobing.

That choice is per-region rather than global because the right answer differs by region. For prefetched destinations the entire skeleton path is bypassed.

### Why one render path

A second rendering path — the JSON-envelope shape an earlier version of this engine used — looked elegant on paper: a small payload, a typed metadata header, no head parsing. In practice it carved a permanent fork down the middle of the codebase. Locale rewriting did not apply to JSON responses unless re-implemented. Per-island parameter dictionaries duplicated whatever the page's Razor render already built. Active-state nav, breadcrumbs in the sidebar, language-switcher hrefs — anything outside the swapped region went stale, and consumers patched it back up with bespoke client-side JavaScript. Each new HTML rewriter had to be applied twice, or quietly skipped on SPA navigation.

The single-path approach trades some payload size for the elimination of all of that. The full HTML response gzips to within a few KB of the JSON envelope it replaced, and the prefetch-on-hover path hides whatever cost remains.

## Trade-offs

- **Cost — payload is the full page including chrome the client throws away.** Mitigated by HTTP-level caching, prefetch-on-hover, and View Transitions API masking the latency. If profiling shows it matters, a server-side response processor can detect a `Sec-Fetch-Dest`/`X-Spa-Fragment` header and pre-extract regions before send, opt-in.
- **Cost — `innerHTML` swap blows away form state and focus inside swapped regions.** Acceptable for a docs site. A morphing strategy (Idiomorph or similar) is the upgrade path if interactive controls move into a swapped region later. The contract — mark regions, server renders them — does not change.
- **Cost — inline `<script>` tags in fetched HTML do not execute on `parseFromString`.** This is a feature for chrome scripts that should not re-run; it is a constraint for any future region whose content includes a JS-driven widget. Hook the `spa:commit` event from outside the region and re-initialize from there.
- **Alternative considered — full client-side SPA (Blazor WebAssembly, React, etc.).** Would make every navigation instant at the cost of a multi-megabyte runtime on first load and a separate SEO story. Rejected for a content engine where most of each page is prose the client doesn't need to render.
- **Alternative considered — JSON envelope of pre-rendered region fragments.** The previous shape, removed for the reasons enumerated above. The fact that the rewriter pipeline already produces correct HTML for the canonical URL is what makes the simpler approach work; the envelope was solving for a problem that did not exist once the rewriters were robust enough to trust.

## Further reading

- Reference: <xref:reference.api.doc-site-options>
- External: [Islands Architecture (Jason Miller)](https://jasonformat.com/islands-architecture/) — the term "island" originates here; Pennington's regions are a degenerate case where the server renders every "island" itself.
