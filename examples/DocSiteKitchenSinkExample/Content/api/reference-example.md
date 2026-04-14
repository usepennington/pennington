---
title: Reference example
description: A sample API reference page in the second content area.
tags: [api, reference]
section: reference
order: 20
uid: kitchen-sink.api.reference-example
---

# Reference example

This page lives in the **API** area. DocSite picks it up from
`Content/api/reference-example.md` and renders it at `/api/reference-example/`.

Cross-area links work exactly like same-area links:

- Back to [Main index](/main/)
- Related: [Cross-references](/main/cross-references-a/)

Because `AddDocSite` uses the built-in `DocSiteFrontMatter` record for
every area, the same keys (`tags`, `section`, `order`) are available
here as in the Main area.
