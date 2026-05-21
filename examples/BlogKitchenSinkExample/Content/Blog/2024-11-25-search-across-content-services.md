---
title: Search across multiple content services
description: How SearchIndexService combines entries from every IContentService into a single per-locale index.
date: 2024-11-25
author: Jamie Rivers
tags:
  - pennington
  - search
  - discovery
series: Pennington Field Notes
sectionLabel: field-notes
---

Each `IContentService` returns `ContentTocItem`s through
`GetIndexableEntriesAsync`. `SearchIndexService` runs every registered
service, applies `search:` and `searchOnly:` front-matter filters, and
emits one JSON index per locale.

## The two opt-outs

`search: false` drops the page from the index entirely. `searchOnly: true`
keeps it in the index but hides it from the sidebar — useful for landing
pages whose chrome already gets to them through a curated nav, but whose
content should still appear in search hits.
