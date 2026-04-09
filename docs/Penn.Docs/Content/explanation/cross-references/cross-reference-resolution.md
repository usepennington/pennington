---
title: "Cross-Reference Resolution"
description: "The two-phase xref system — first pass builds a case-insensitive UID-to-route lookup from all content services, second pass rewrites xref:uid tokens in HTML to links, lazy resolution with AsyncLazy, file-watch invalidation, and how xrefs work differently in SPA mode"
uid: "penn.explanation.cross-reference-resolution"
order: 10
---

Explain the two-phase cross-reference system. Phase 1 (index building): `XrefResolver` collects UIDs from all `IContentService` implementations via `GetCrossReferencesAsync()`, building a case-insensitive dictionary from UID to `CrossReference` (title + route). This is managed by `FileWatchDependencyFactory` so it rebuilds when content files change, using `AsyncLazy` for thread-safe lazy recomputation. Phase 2 (resolution): `XrefResolvingProcessor` (a response processor) scans HTML for `xref:uid` patterns and replaces them with `<a href="...">title</a>` links. Discuss why this is a response processor rather than a Markdig extension — xrefs need the full index from all content sources, which isn't available during individual page rendering. Explain the SPA complication: when the SPA data endpoint returns island HTML, it runs xref resolution directly (not through the response processor chain), because the SPA endpoint returns JSON fragments, not full HTTP responses. Discuss diagnostic reporting: unresolved xrefs emit warnings that appear in dev mode headers and build reports.
