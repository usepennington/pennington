---
title: "SPA navigation through region swaps"
description: "Why Pennington's SPA fetches the canonical HTML page and swaps marked regions — reusing the server render and its rewriters instead of a parallel JSON envelope."
sectionLabel: "SPA"
order: 1
tags: [spa, navigation, regions, hydration]
uid: explanation.spa.islands
---

DocSite ships a small SPA navigation engine — in-site clicks fetch the canonical URL, parse the response, swap marked regions, and merge head metadata. The design question this page answers: why fetch the same URL the address bar shows and parse it client-side, instead of round-tripping a small JSON envelope or letting the browser do a full reload?

## Context

Classic server-rendered sites swap the whole document on every click — simple, full-fidelity, but heavy and visually jarring when the shared chrome redrawing is indistinguishable from the content changing. Full SPAs hydrate the entire app in the browser, make every navigation instant, and pay for it with a multi-megabyte runtime on first load, a separate SEO story, and a rendering path that diverges from whatever the server would have produced. Documentation sites sit awkwardly between those two extremes. Most of each page is static prose that does not benefit from client rendering, a couple of areas — the article body, the top bar with the language switcher — genuinely want to update on each navigation, and a third category of chrome (the sidebar's active link, the page outline) should not re-render at all but does need its state nudged when the page changes around it.

Pennington takes a third position. Every page is fully server-rendered on first load. When the visitor clicks an in-site link, the browser fetches the same URL — the canonical HTML page — parses it with `DOMParser`, swaps regions tagged `data-spa-region` from the new document into the current one, and merges head metadata. The server render and its HTML rewriters are reused as-is, so there is no second pipeline to keep in sync.

## How it works

### One render, many slices

The first request to any URL returns complete server-rendered HTML, exactly as it would without SPA support — good for cold loads, good for crawlers, functional when JavaScript is disabled. Once the browser has that page and the `spa-engine.js` script from `Pennington.UI` is active, the client intercepts same-origin link clicks and re-fetches the destination URL. The response is parsed into a `Document` that supplies both the regions that change and the head deltas that follow them.

Every server-side rewriter — xref resolution, locale-aware link rewriting, base-URL prefixing, anything else registered as `IHtmlResponseRewriter` — applies to that response by default, because it travelled through the same `ResponseProcessingMiddleware` as a fresh-tab visit. There is no second pipeline to mirror.

```beck
type: sequence
participants:
  - { id: reader, title: Reader, kind: user }
  - { id: client, title: SPA client }
  - { id: server, title: Server }
messages:
  - { from: reader, to: client, label: click in-site link }
  - { from: client, to: client, label: intercept }
  - { from: client, to: server, label: GET canonical URL }
  - { from: server, to: client, label: HTML response, reply: true }
  - { from: client, to: client, label: DOMParser }
  - { from: client, to: client, label: swap data-spa-region regions }
  - { from: client, to: client, label: data-head sweep }
  - { from: client, to: client, label: "spa:commit fires" }
```

### The `data-spa-region` contract

Anywhere in the layout that should update on navigation gets a `data-spa-region="name"` attribute. The DocSite layout marks two regions out of the box:

- `content` — the article body, including breadcrumbs and prev/next links.
- `outline` — the right-rail page outline, populated client-side from the article headings on every commit.

Anything outside a marked region — the top bar, the sidebar, the outer page chrome, the mobile menu's expanded state, scroll position — stays put. The header and sidebar are intentionally outside the region system: the search button keeps its event handlers across navigations, and the sidebar keeps its scroll position while its active-state flags are patched in place from the destination's HTML. Both follow the persistent-chrome pattern covered in the next section.

The client picks the regions in the current document, finds elements with the same name in the parsed response, and swaps `innerHTML`. If the set of regions does not match — for example, navigating from a `MainLayout` page (`content` plus `outline`) to a `FullWidthLayout` page (only `content`) — the engine triggers a full page load. Crossing a layout boundary reloads rather than half-updating the page.

### Persistent chrome

Some chrome has the same shape on every page. The DocSite sidebar is the canonical example — across every doc page within a layout, the table of contents is structurally identical, and the only thing that varies is which link carries `data-current="true"`. Marking that as a swap region works, but it throws away DOM nodes the engine is about to rebuild to the same shape, and along with them: the user's scroll position in a long sidebar, focus on the link they tabbed to, any expand/collapse state a reader interacted with, and any iframe or animation state inside the region.

Leaving the element outside `data-spa-region` is the answer. The engine never queries it, never swaps its `innerHTML`, never re-runs scripts inside it. The same DOM nodes survive every navigation — `scrollTop`, focus, and live state are preserved by the browser automatically, because nothing relocates them. The cost is that the active-state attributes no longer change automatically; the server-rendered destination has the right `data-current` flags, but they live in HTML the engine no longer looks at.

The `spa:commit` event is the extension point. It fires after each navigation with `detail.doc` — the parsed `Document` of the destination, the same HTML the server would have rendered for that URL. Consumers read the destination's chrome out of `doc`, copy whatever state actually changed onto the live nodes, and let the server-rendered destination stay authoritative. Active-state flags, the active-area pill, anything the server already computes — gets patched in place rather than re-derived on the client. The DocSite uses this to keep the sidebar's `data-current` flags in sync without ever rebuilding the tree.

### Head merging

The `<head>` of the parsed response is authoritative for everything page-specific. The client sets the title from the destination, then reconciles the rest with one generic sweep — it removes every `data-head` element from the live head and clones every `data-head` element out of the fetched document, so any tag the server marks survives navigation without a per-tag allowlist to maintain. [The head subsystem](xref:explanation.core.head-subsystem) owns that attribute and the server-side composition behind it. Stylesheet `<link>` elements are merged by href — any new ones append to the head before the region swap so the browser has the rules ready when the new content paints. A stylesheet tagged `data-spa-reload` re-fetches with a cache buster on every navigation, the opt-in workaround for JIT stylesheets like [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) in dev where the URL stays constant but the contents diverge per page; the attribute is documented in <xref:reference.spa.attributes>.

### Synchronous swap, no animation

The round trip is small but not instant. View-transition wrappers (`document.startViewTransition` with a short cross-fade) and per-region skeleton placeholders were both on the table during the design and rejected. Both layers introduce more visible motion than they mask: the cross-fade is a flash for the eye to notice, and a skeleton replaces real content with shimmer the moment the network takes longer than a tick. The engine waits instead — old content stays on screen while the fetch runs — and the swap, scroll reset, and head update all execute in one synchronous block so the browser paints the new page as a single frame. Hover-prefetch hides the wait for the cases where it would otherwise be felt.

A top-of-viewport progress bar handles the unusual case where the response takes longer than the engine's silent threshold — a cold cache, a slow CDN edge. It only shows after the threshold elapses, so fast navigations never see it.

### Why one render path

A second rendering path — the JSON-envelope approach an earlier version of this engine used — looked appealing at first: a small payload, a typed metadata header, no head parsing. In practice it carved a permanent fork down the middle of the codebase. Locale rewriting did not apply to JSON responses unless re-implemented. Per-island parameter dictionaries duplicated whatever the page's Razor render already built. Active-state nav, breadcrumbs in the sidebar, language-switcher hrefs — anything outside the swapped region went stale, and consumers patched it back up with one-off client-side JavaScript. Each new HTML rewriter had to be applied twice, or quietly skipped on SPA navigation.

The single-path approach trades some payload size for the elimination of all of that. The full HTML response gzips to within a few KB of the JSON envelope it replaced, and the prefetch-on-hover path hides whatever cost remains.

## Further reading

- Reference: <xref:reference.spa.attributes> — the `data-spa-*` attribute contract and the `spa:commit`/`spa:before-navigate` events this page describes.
- How-to: <xref:how-to.rich-content.client-side-widget> — attach your own browser behavior to the server-rendered HTML and re-bind it from `spa:commit` after each navigation.
- Reference: <xref:reference.api.doc-site-options>
- External: [Islands Architecture (Jason Miller)](https://jasonformat.com/islands-architecture/) — the term "island" originates here; Pennington's regions are a degenerate case where the server renders every "island" itself.
