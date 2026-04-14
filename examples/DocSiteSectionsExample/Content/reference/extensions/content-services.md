---
title: Custom content services
description: Teach Pennington a new content source by implementing IContentService.
sectionLabel: Extensions
order: 40
---

# Custom content services

`IContentService` is the extension point for loading content from anything
that isn't plain markdown — a JSON feed, a database, a remote API, an
embedded resource. Register an implementation and the pipeline treats its
items exactly like every other source.

## The four methods

- `DiscoverAsync()` — yield a `DiscoveredItem` per logical page.
- `GetContentTocEntriesAsync()` — flat list of TOC entries with title,
  order, and hierarchy parts.
- `GetCrossReferencesAsync()` — any `uid`-to-route mappings you want the
  xref resolver to see.
- `GetContentToCopyAsync()` — assets the static-build step should copy
  alongside the rendered HTML.

Implementations live next to `MarkdownContentService` in the DI container
and are iterated in registration order.
