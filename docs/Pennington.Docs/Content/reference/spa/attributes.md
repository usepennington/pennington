---
title: "SPA engine attributes and events"
description: "The data-spa-* attribute contract the spa-engine.js script in Pennington.UI reads — region markers, scroll keys, stylesheet handling, root tuning, and lifecycle events."
sectionLabel: "SPA"
order: 1
tags: [spa, attributes, navigation, regions]
uid: reference.spa.attributes
---

`Pennington.UI` ships `spa-engine.js`, a navigation script that fetches the destination URL, parses the HTML response, swaps marked regions, and merges head changes. This page catalogs the `data-spa-*` attribute contract the engine reads from markup, plus the three custom events it dispatches on `document`.

For the design rationale, see [SPA navigation through region swaps](xref:explanation.spa.islands).

## Region attributes

Every attribute in this table is read on a region element — the element marked `data-spa-region`. The engine discovers regions on every navigation, so attributes can be added or removed by other code at runtime.

| Name | Values | Description |
|---|---|---|
| `data-spa-region` | identifier | Marks the element as a swap target; the value is the region name and must be unique per page. |
| `data-spa-region-key` | any string | Identifies the content set inside the region; when the incoming element's key differs from the current one, the region and its closest scrollable ancestor have `scrollTop` reset to `0`. Absent keys carry scroll position across navigations. |

## Anchor and stylesheet attributes

| Selector | Attribute | Description |
|---|---|---|
| `<a>` | `data-spa-reload` | Forces a full-page navigation for that link; the engine treats it as a non-SPA anchor. Used by `LanguageSwitcher` so the locale change re-runs the request pipeline. |
| `<a>` | `target="_blank"` or `download` | Excluded from SPA handling automatically; no opt-in attribute required. |
| `<link rel="stylesheet">` | `data-spa-reload` | Re-fetches the stylesheet with a `_spa=<timestamp>` cache-buster query on every navigation. Opt-in only; see [why dev needs it and production doesn't](xref:explanation.core.dev-vs-build). |

## Document-root tuning

One integer attribute on `<html>` adjusts the progress bar timing.

| Name | Type | Default | Description |
|---|---|---|---|
| `data-spa-progress-delay` | milliseconds | `100` | Threshold the engine waits before showing the top-of-viewport progress bar; navigations that resolve faster never show it. |

Parsed once on script load. Changing it after the script runs has no effect.

## Lifecycle events

All three events fire on `document`. Listeners attached outside a swapped region survive every navigation; listeners attached inside a region must be re-bound from a `spa:commit` handler because the region's contents are replaced via `innerHTML`.

| Event | `detail` shape | When it fires |
|---|---|---|
| `spa:before-navigate` | `{ url, slug }` | After a same-origin link click is intercepted, before the fetch is issued. |
| `spa:commit` | `{ url, slug, doc }` | After head and region swaps complete, in the same synchronous block as the DOM replacement. `doc` is the parsed `Document` of the fetched response — the read-side contract for patching elements that live outside any region. See [Persistent chrome](xref:explanation.spa.islands#persistent-chrome). |
| `spa:diagnostics` | `Diagnostic[]` | After `spa:commit`, only when the response carries a `<script type="application/spa-diagnostics+json">` block. |

`<script>` elements inside a swapped region are re-created so the parser executes them on commit; scripts of type `application/ld+json` and `application/spa-diagnostics+json` are skipped.

## Boundary fallbacks

The engine falls back to `location.href = url` (full page load) in three cases.

| Trigger | Reason |
|---|---|
| No `[data-spa-region]` on the current page | Nothing to swap. |
| Incoming document's region set differs from the current one | Layout boundary; for example, `MainLayout` (`content`, `outline`) → `FullWidthLayout` (`content`). |
| Fetch fails | Network or server error; the browser handles the navigation. |

## See also

- Background: [SPA navigation through region swaps](xref:explanation.spa.islands) — why the engine re-fetches the canonical URL instead of a JSON envelope, and when persistent chrome beats a swap region.
- Related reference: [Utility components](xref:reference.ui.utility) — `LanguageSwitcher` is the canonical consumer of `data-spa-reload` on an anchor.
- Related reference: [Diagnostics request context](xref:reference.diagnostics.request-context) — origin of the payload `spa:diagnostics` carries.
