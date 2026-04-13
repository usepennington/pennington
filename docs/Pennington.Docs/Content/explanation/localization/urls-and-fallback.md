---
title: Locale-aware URLs and content fallback
description: How the URL prefix feeds into ContentResolver to reach the correct localized content, the fallback rules when a locale lacks a page, and why the search index is split per locale.
section: localization
order: 10
uid: explanation.localization.urls-and-fallback
isDraft: true
search: false
llms: false
tags: []
---

> **In this page.** How the URL prefix feeds into `ContentResolver` to reach the correct localized content, the fallback rules when a locale lacks a page, and why the search index is split per locale.
>
> **Not in this page.** The `LocalizationOptions` API specifics — see the reference page for the full member list.

## The question

Why does a locale-prefixed URL like `/fr/guides/intro` first reach `ContentResolver` intact, only then fall back to the default locale's markdown — and why does that design force the search index to split into one JSON file per locale?

## Context

- Pennington targets sites that mix fully-translated pages with partially-translated ones; missing translations must degrade gracefully rather than 404.
- The older alternative — one `@page` directive per locale — bloats Razor routing and forces content services to choose between duplicating routes or registering locale-specific middleware.
- A second alternative — stripping the locale prefix *before* resolution — was rejected because the resolver then cannot distinguish "this locale has its own copy" from "this locale is falling back to the default."
- The adopted shape keeps one Razor route per page and moves locale awareness into two collaborators: `LocaleDetectionMiddleware` (for Blazor routing) and `ContentResolver` (for content lookup).

## How it works

### Outline

- **Mechanism part 1 — URL shape as the coordinate system.** The first path segment carries the locale (`/fr/...`) for non-default locales; the default locale has no prefix. `LocalizationOptions.GetLocaleFromUrl`, `StripLocalePrefix`, and `BuildLocaleUrl` are the pure URL math underneath.
- **Mechanism part 2 — Two consumers of the prefix.** `LocaleDetectionMiddleware` strips the prefix from `HttpContext.Request.Path` and stashes it in `PathBase`, so Blazor pages match a single `@page` directive. `ContentResolver.GetContentByUrlAsync` receives the **full, unstripped URL** (commit `d4947a0`) — the prefix is load-bearing for content lookup.
- **Mechanism part 3 — Why URL-first resolution in the resolver.** Code fence for `ContentResolver.GetContentByUrlAsync` (or a trimmed excerpt showing the sequence: full-URL match → locale detection → fallback-prefix strip). Narrative: matching by full URL first lets a locale-specific markdown file or a *pre-baked* fallback `ContentRoute` (`IsFallback = true`) win without ambiguity; the content-relative retry only fires when no locale-aware route exists.
- **Mechanism part 4 — The fallback chain.** Two layers of fallback, in this order:
  1. **Discovery-time fallbacks.** `MarkdownContentService.DiscoverRoutesWithFallbacks` emits a fallback `ContentRoute` (`IsFallback = true`) for every default-locale file that a non-default locale is missing, pointing at the default-locale source file. These show up in navigation and the search index as first-class entries.
  2. **Runtime fallbacks.** If `ContentResolver` still finds nothing for the full URL, it strips the locale prefix via `StripLocalePrefix` and retries against the content-relative path, marking the result `IsFallback = true` with `RequestedLocale` preserved for the `FallbackNotice` UI.
- **Mechanism part 5 — Why the search index splits per locale.** `SearchIndexService` groups documents by `toc.Route.Locale`, seeds an empty bucket per configured locale, and `UsePennington` maps one `/search-index-{code}.json` endpoint per locale (`PenningtonExtensions.cs`). Three reasons, in order of weight:
  - **Relevance.** A French reader searching "pipeline" should not rank English pages above French ones because of raw term frequency; per-locale buckets make the ranking problem local.
  - **Bundle size.** The client fetches exactly the locale it needs, not the cartesian product of all languages.
  - **Fallback honesty.** Fallback entries carry the *requesting* locale's code (the route inherits it from discovery), so a reader searching in `fr` finds fallback-English pages surfaced under their French URL — the index mirrors what the resolver will actually serve.

## Trade-offs

- **Cost.** Every request URL must stay full-fidelity from the Kestrel layer down to `ContentResolver`; middleware that rewrites `Request.Path` earlier breaks locale detection. The order in `UsePennington` (locale routing → response processing → endpoints) is load-bearing.
- **Alternative considered — strip once, early.** Rejected: the resolver can't tell "I have a French file" apart from "I'm serving the English file at a French URL," and the `FallbackNotice` UI loses the information it needs to warn the reader.
- **Alternative considered — single search index with a `locale` field.** Rejected: clients would download every language's documents on first search, and cross-locale term frequency pollutes ranking. The per-locale split is the simpler invariant.
- **Consequence.** Adding a new locale means regenerating one more JSON file and re-running discovery — but it does **not** mean touching Razor routes or duplicating markdown. The `IsFallback` flag is the single source of truth for "this page isn't really translated yet."

## Further reading

- Reference: [`LocalizationOptions` members](/reference/options/localization-options)
- How-to: [Add a new locale to a doc site](/how-to/localization/add-locale)
- Explanation: [The content pipeline and `ContentRoute`](/explanation/core/content-pipeline)
