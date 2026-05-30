---
title: Content visibility markers
description: Demonstrates humans-only and robots-only classes for splitting content between browser display and llms.txt extraction.
sectionLabel: authoring
order: 235
uid: kitchen-sink.main.content-visibility
---

Two paired classes split what humans see from what the llms.txt extractor records. Browsers hide `.robots-only`; the llms pipeline strips `.humans-only`. Unmarked content flows to both.

## Shared content

This paragraph has no marker class, so it shows up both in the browser and in the llms.txt sidecar.

## Human-only content

<div class="humans-only">

**For humans:** an interactive tour would land here, rendered inline. A visiting LLM does not need the widget, so the llms.txt sidecar skips this block entirely.

</div>

## Robots-only content

<div class="robots-only">

**For robots:** the full signature reference for `IContentService.DiscoverAsync` is `IAsyncEnumerable<DiscoveredItem> DiscoverAsync()`. Callers enumerate on each pipeline run; yielded items must be distinct by `Route.CanonicalPath`.

</div>

## Verification

- View this page in a browser and confirm only the "For humans" and shared paragraphs render.
- Fetch `/_llms/main/content-visibility.md` and confirm the "For humans" paragraph is gone and the "For robots" paragraph is present.
