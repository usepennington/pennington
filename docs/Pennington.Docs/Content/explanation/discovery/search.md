---
title: "How the search index is built and queried"
description: "Why Pennington ships a sharded, heading-level search index built at render time and queried entirely in the browser — and what that shape buys the reader."
uid: explanation.discovery.search
order: 1
sectionLabel: "Discovery"
tags: [search, architecture, deweysearch, localization]
---

Pennington has no search server. The index is a set of static JSON files generated alongside the rest of the site, and the query runs in the visitor's browser. That single constraint — no backend at query time — shapes every other decision: how the index is split, what a record represents, and how a multi-locale site keeps its indexes apart.

## Context

The search engine itself is not Pennington's. The tokenizer, stemmer, inverted index, and ranking live in the external **DeweySearch** package; the browser client ships from `DeweySearch.Web` as `dewey-search.js`. Pennington's job is the adapter layer: turn rendered pages into `DeweySearch.SearchDocument` records, hand them to DeweySearch's index builder, and lay the resulting artifacts out on disk where the client can fetch them.

Keeping the engine external means Pennington never re-implements ranking, and an upgrade to DeweySearch's relevance model arrives without a Pennington change. What Pennington owns is everything domain-specific: what counts as a record, what URL a result links to, and which dimensions become filter facets.

```beck
type: architecture
meta: { animate: false, direction: TB }
nodes:
  - { id: render, title: Rendered corpus, subtitle: "site projection" }
  - { id: extractor, title: HeadingSectionExtractor, subtitle: "one section per heading" }
  - { id: builder, title: SearchIndexBuilder, subtitle: "section to SearchDocument" }
  - { id: shards, title: Per-locale shards, subtitle: "/search/{locale}/", kind: db }
  - { id: client, title: Browser client, subtitle: "dewey-search.js", kind: user }
edges:
  - { from: render, to: extractor, label: render fold }
  - { from: extractor, to: builder, label: sections }
  - { from: builder, to: shards, label: "index.json · t-* · f-*" }
  - { from: shards, to: client, label: fetched on demand }
```

## Records are heading-level, not page-level

A naive index has one record per page. Search a thousand-word reference page and the whole page matches; the result drops the reader at the top and leaves them to scroll.

Pennington indexes at the heading instead. After a page renders, `HeadingSectionExtractor` walks the post-pipeline HTML and splits it into one section per `h2`–`h6` heading, plus a *lead* section for the text before the first heading. `SearchIndexBuilder` maps each section onto its own `SearchDocument`: the lead section carries the page's title, description, and URL; every heading section carries the heading text as its title and an anchored URL (`/page/#heading`). Each non-lead record also carries a page→heading breadcrumb trail, so the client can group results by their source page and a result deep-links to the exact section that matched.

The tradeoff is record count — a page with twelve headings produces thirteen records instead of one. That cost is paid in the index, which the visitor never downloads whole, and bought back as precision: the result is the section, not the page.

## The index is sharded

A site of any size produces an index too large to ship as one file and download on the first keystroke. So the build splits it.

Each locale gets a tree under `/search/{locale}/`:

- `index.json` — the entrypoint: the document table (one row per record: URL, title, length, priority, facet ids), the facet label vocabularies, ranking statistics, and the stemmed synonym map.
- `t-*.json` — term shards. Terms are bucketed by the first few characters of their stemmed form, so a query fetches only the shards for the terms it contains.
- `f-*.json` — per-page fragments holding the indexed body text, fetched only when a page surfaces in results.

The client downloads `index.json` once, then pulls term shards and fragments on demand. Typing a query fetches a handful of small files rather than one large one; opening a result fetches that page's fragment and nothing else. The shard granularity is tunable, but the default keeps shards small enough that no single fetch dominates.

## The build is a fold over the render

The index is not a second pass over the content. `SearchArtifactService` folds over the same site projection that produced every page's HTML, so each page's rendered body and heading split already exist by the time search sees them — building the index is a pure mapping from rendered page to records, not a re-render.

The same service feeds two consumers: the build-time emitter that writes the JSON files into the static output, and the dev-time middleware that serves them live. One source of truth means the index a developer queries locally is the index that ships. Because the service derives its state from content files, it is file-watched: edit a page and the index it holds is dropped and rebuilt.

Two kinds of page never reach a record. Pages marked `search: false` are excluded upstream — the content service's table-of-contents builder sets `ExcludeFromSearch`, and the fold skips them — while still rendering at their URL and appearing in the sidebar. Pages with no HTML body (endpoint and llms-only sources) have nothing to index and are skipped too.

## What becomes a facet

DeweySearch's facet model is an open dictionary — any axis the host emits becomes a filterable dimension. Pennington maps three built-in axes onto it: the content *area* (the first URL segment after any locale prefix), the *section* label, and the page *tags*. A record carries an axis only when the page actually has a value for it, which is exactly how DeweySearch decides a facet exists.

Area is the only facet on by default. Areas are few and stable, so they read well as a short row of filter chips; section and tag vocabularies grow large enough to bury the filter bar, so they are opt-in. A front-matter record can also declare custom facet axes by implementing `IHasSearchFacets`; those ride alongside the built-ins but can never overwrite `area`, `section`, or `tag`, which stay authoritative.

## One index tree per locale

A multi-locale site is really several sites sharing a host, and a French query should not match English bodies. So the fold groups records by the page's locale and builds a separate DeweySearch index per group, laid out under its own `/search/{locale}/` tree. Every configured locale gets a tree even when it has no content yet — a registered-but-empty locale serves a valid entrypoint with an empty document table rather than a 404, so the client's fetch always resolves.

The browser client picks which tree to query from the first URL path segment, matched against the `data-locales` list the layout emits, falling back to the default locale. The locale routing that puts `/fr/` in front of a French page is the same signal that selects the French index — the two stay aligned without a separate configuration.

## Further reading

- How-to: [Tune what the search box returns](xref:how-to.discovery.search) — exclude pages, weight priority, scope the indexed region, add synonyms, choose facets.
- How-to: [Add the search modal to a non-DocSite site](xref:how-to.discovery.search-on-a-bare-host) — surface this index in a search UI on a bare `AddPennington` host.
- Reference: [`SearchIndexOptions`](xref:reference.api.search-index-options) — the knobs that shape the build.
