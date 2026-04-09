---
title: "Search Index Format"
description: "Reference for SearchIndexDocument record (Title, Body, Url, Section, Locale, Priority), the /search-index.json schema, SearchIndexBuilder HTML stripping and draft filtering, and SearchIndexService lazy computation"
uid: "penn.reference.search-index-format"
order: 10
---

Document the `SearchIndexDocument` record: Title (string), Body (string, HTML-stripped), Url (string), Section (string), Locale (string), Priority (int). Show the JSON schema of `/search-index.json` — the array format that FlexSearch consumes on the client side. Document `SearchIndexBuilder`: the `Build(RenderedItem)` method, how it strips HTML from rendered content, skips drafts, and assigns the default priority of 5. Document `SearchIndexService`: lazy computation, file-watch invalidation via `FileWatchDependencyFactory`, and the `/search-index.json` endpoint registration.
