---
title: Search that lands on the right heading
description: Search is rebuilt on the DeweySearch engine and indexes at the heading level, so a result links straight to the section that matched, and the client loads only when you open it.
author: Phil Scott
date: 2026-05-26
isDraft: false
tags:
  - search
---

Search used to point at a page and leave you to find the part you wanted. Now it
points at the heading.

## Results at the heading level

When a page renders, Pennington splits its HTML into one record per heading, each
carrying the page-to-heading trail. A query for "base URL" returns the exact
section that covers it, and the link drops you at `/page/#that-heading` rather
than the top of a long page. It's the model DocSearch uses, built at your site's
build time instead of crawled afterward.

The engine underneath is [DeweySearch](https://www.nuget.org/packages/DeweySearch),
a .NET full-text index: Pennington builds the index, DeweySearch tokenizes,
stems, and ranks. There's no search server and no third-party service. The index is static JSON,
sharded per locale under `/search/{locale}/`, and the browser queries it
directly.

## The client loads on demand

The search client is lazy. A page ships no search JavaScript until you open the
box; the client script loads then, and the index downloads on the first
keystroke. A site nobody searches never pays for search. The [search
how-to](xref:how-to.discovery.search) covers turning it on, and [search on a bare
host](xref:how-to.discovery.search-on-a-bare-host) wires the modal into a
non-template site.

## Tuning what ranks

By default, an exact title match wins: search "DocSiteOptions" and the type's
own page sits above the API-reference entries that merely mention it. Beyond
that, you can weight a URL prefix or a content area, add synonyms, and turn on
facet filters through `SearchIndexOptions`. The reasoning behind the
heading-level model is in [how search works](xref:explanation.discovery.search).
