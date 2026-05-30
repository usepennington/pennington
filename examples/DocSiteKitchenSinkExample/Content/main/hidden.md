---
title: Not in search
description: This page is intentionally excluded from the search index.
sectionLabel: authoring
order: 220
search: false
uid: kitchen-sink.main.hidden
---

This page carries `search: false` in its front matter. It still renders
at its URL and still appears in the sidebar, but the search index
JSON does **not** contain it. Open `/search-index-en.json` to verify
— this title and body are absent from the `documents` array.

Pair this with `llms: false` on a separate page to carve the opposite
hole in `/llms.txt`.
