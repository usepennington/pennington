---
title: Generating sitemaps that actually help
description: Sitemap.xml is mostly automatic, but two front-matter knobs decide what crawlers see.
date: 2025-01-14
author: Jamie Rivers
tags:
  - pennington
  - sitemap
  - seo
series: Pennington Field Notes
sectionLabel: field-notes
---

`SitemapService` walks every `IContentService.DiscoverAsync` and emits an
entry for each HTML route, with `<lastmod>` pulled from front-matter `date`.
Redirect stubs and endpoint routes drop out automatically.

## The knobs you do control

`canonicalBaseUrl` decides the absolute URL prefix — get it wrong and the
sitemap points at `http://localhost`. Per-page `searchOnly:` and `llms:`
flags don't affect the sitemap (different question, different answer).
Drafts do drop, because a draft has no canonical URL to advertise.
