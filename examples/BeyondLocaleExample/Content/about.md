---
title: About
description: About this localized DocSite example.
order: 20
---

This is a minimal DocSite that demonstrates **locale-aware URLs**. Every
markdown file under `Content/` is the English (default) version. Every
matching file under `Content/es/` is the Spanish translation.

When a visitor navigates to `/es/about`, `LocaleDetectionMiddleware` strips
the `/es` prefix, stores `"es"` in `LocaleContext`, and the DocSite's
`DocSiteContentResolver` picks up the Spanish markdown from `Content/es/about.md`.
If a Spanish file is missing, the resolver falls back to the English copy
and marks the page as a translation-fallback so the reader knows.
