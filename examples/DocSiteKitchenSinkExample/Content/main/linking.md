---
title: Linking between pages and assets
description: Relative links, absolute links, and asset references.
tags: [authoring, linking]
section: authoring
order: 130
uid: kitchen-sink.main.linking
---

# Linking

## Relative links to sibling pages

A relative link resolves against the current page's URL. From this file
(`/main/linking/`), `[sidebar](./customize-sidebar)` walks to
`/main/customize-sidebar/`:

- [Customize the sidebar](./customize-sidebar)
- [Drafts, tags, ordering](./drafts-tags-ordering)

## Absolute links inside the site

Use an absolute URL to link across areas:

- [API index](/api/)
- [API reference example](/api/reference-example/)

## Asset links

Reference assets under `wwwroot/` with an absolute path and assets
under `Content/` with a relative path:

- Shared: [shared.png](/shared.png)
- Colocated: [colocated.png](./assets/colocated.png)

## Uid links

When the target page's location may change, prefer a `xref:` link:

- [by-uid pointer](xref:kitchen-sink.main.cross-references-b)
