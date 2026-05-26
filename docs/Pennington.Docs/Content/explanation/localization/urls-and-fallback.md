---
title: "Locale-aware URLs and content fallback"
description: "Why Pennington treats the URL path prefix as the authoritative locale signal, how ContentResolver strips it and falls back to the default locale, and why the search index is split per locale."
uid: explanation.localization.urls-and-fallback
order: 1
sectionLabel: "Localization"
tags: [localization, routing, fallback, search]
---

When a reader hits `/fr/guides/intro`, how does Pennington know which language to serve, which markdown file to load, and what to do if `guides/intro.md` only exists in English?

## Context

A multilingual site has to pick a locale signal early enough that routing, content resolution, HTML rewriters, and the search index all agree on it. The obvious candidate is the browser's `Accept-Language` header, but that signal is notoriously noisy — users travel, share links, and sit behind corporate proxies, and a locale chosen from a header cannot be bookmarked, cached by a CDN, or indexed distinctly by search engines. Pennington picks the opposite invariant: the URL path prefix (`/fr/…`, `/es/…`) is the single source of truth, and the default locale owns the unprefixed root. That same prefix drives both request-time content resolution in development and build-time output placement during the static crawl — one code path, one answer. Because translation coverage is rarely complete, the question "what happens when `/fr/foo` has no French source?" needs a principled answer, and that is what the rest of this page explores.

## How it works

### URL prefix drives detection

`LocaleDetectionMiddleware` reads `HttpContext.Request.Path`, asks `LocalizationOptions.GetLocaleFromUrl` which locale the first path segment maps to, and populates the scoped `LocaleContext`. When the detected locale is not the default, the middleware rewrites the request path to strip the prefix and pushes that prefix onto `PathBase` so URL generation stays correct. The net effect is that Blazor routing and every downstream consumer see `/guides/intro` rather than `/fr/guides/intro`, which means a single `@page "/guides/intro"` directive serves every locale without duplication.

The middleware never consults `Accept-Language`. The culture provider (`PenningtonUrlRequestCultureProvider`) also derives its answer from the URL, so the entire request pipeline agrees on the same locale code without any negotiation step. The Accept-Language header is advisory at most, never authoritative. Prefix-first means the same link produces the same page for everyone who clicks it, regardless of their browser settings or location.

### ContentResolver normalizes then searches

`ContentResolver.GetContentByUrlAsync` takes the full URL — locale prefix intact — and runs two passes. The first pass asks every `IContentService` for a `DiscoveredItem` whose route matches the exact URL. This catches locale-specific markdown (`/fr/guides/intro`) and any `IsFallback` route a service pre-computed at startup. The second pass is the runtime fallback: when the first pass misses and the site is multi-locale and the active locale is not the default, the resolver strips the prefix via `LocalizationOptions.StripLocalePrefix` and searches again against the content-relative path. Either a localized file wins or the default-locale file stands in.

When the second pass succeeds, the resolver sets `IsFallback: true` and records `RequestedLocale` so the view layer can render a "this page has not been translated yet" notice via `FallbackNotice`. The resolver never rewrites URLs — the URL the reader typed stays in the address bar, only the content source changes.

### Fallback: default locale stands in

There is exactly one fallback rung: the default locale. A missing `/es/guides/intro` falls through to `guides/intro` — the unprefixed default-locale source — not to `/fr/guides/intro`. There is no cascade across non-default locales, no per-locale fallback chain, and no "similar language" matcher. The reason is that a cascade hides coverage gaps. Readers end up seeing a page in a language they did not request and cannot explain why, and translators cannot tell which pages still need coverage because every miss silently inherits from a neighbor. One rung keeps the mental model compact and the coverage report honest.

The default locale is not a special kind of locale — it is the locale that owns the unprefixed URL space. `StripLocalePrefix` is a no-op for the default locale, `BuildLocaleUrl` emits unprefixed URLs for it, and content authored at `Content/guides/intro.md` is default-locale content by virtue of sitting outside every locale subdirectory. The fallback rule is a consequence of those URL-math rules, not a separate "fallback locale" setting.

`StripLocalePrefix` (see <xref:reference.api.localization-options>) is pure URL math with no file-system knowledge. `ContentResolver` decides whether the stripped path resolves to a file on disk, and that separation is what lets the same helper serve both request-time fallback and build-time URL generation.

### Per-locale search indices

`SearchIndexService` emits one JSON bucket per configured locale, keyed by each TOC item's route locale (or `DefaultLocale` when the route has none). `UsePennington` maps a per-locale endpoint — `/search-index-{code}.json` — and the client fetches only the index for the active locale. This keeps the client payload small on multilingual sites: a reader browsing `/es/` never downloads French or Japanese documents. More importantly, it keeps search semantics scoped. A query typed into the Spanish UI ranks against Spanish content, not against a mixed-language pool where term frequencies across languages distort each other's results.

A page that exists only in the default locale appears once, in the default-locale index — fallback is a rendering-time courtesy, not an indexing decision. That asymmetry is deliberate. When a French reader searches for a term that only exists in English content, the right answer is "no results in French" rather than silently returning English hits the reader cannot read. The same per-locale split flows through sitemap construction and `hreflang` alternate-language tags, so every discovery channel agrees on what lives in which locale.

## Trade-offs

- **Cost — every translation is an extra file with an extra URL.** There is no in-band `locale:` front-matter flag that turns one file into many; each translation of `guides/intro.md` lives at `fr/guides/intro.md`, `es/guides/intro.md`, and so on. This is more boilerplate than a single-file scheme, but it makes coverage obvious (list the directory) and diffing trivial (a PR that touches `fr/` is a French-translation PR).
- **Alternative considered — Accept-Language-driven locale routing.** A content-negotiation shape where `/guides/intro` transparently serves French to French browsers was rejected: it breaks bookmarking, CDN caching, search-engine indexing, and link sharing, all for the sake of saving a `/fr/` prefix in the URL. The prefix-first rule trades one cosmetic cost for five structural wins.
- **Cost — fallback silence.** A reader hitting `/fr/only-english-page` sees English content under a French URL. The `FallbackNotice` banner softens this, but the URL itself reflects a different language than the body until a translation lands. The alternative — a 404 for untranslated pages — was rejected as too hostile to partial-coverage sites, which is almost every real-world multilingual site during rollout.
- **Consequence — search stays honest per locale.** Because the index is split, a Japanese search UI that is missing half its content shows "no results" instead of pretending otherwise. Authors get an accurate signal about translation gaps from the search experience itself, which is exactly the feedback loop that a cross-locale index would hide.

## Further reading

- Reference: [`LocalizationOptions`](xref:reference.api.localization-options) — `DefaultLocale`, `AddLocale`, and the URL helpers that back this mechanism.
- How-to: [Enable multiple locales](xref:how-to.discovery.localization) — the recipe for populating `LocalizationOptions`, organizing content subdirectories, and wiring `UsePenningtonLocaleRouting`.
- External: [W3C — Language tags in HTML and XML](https://www.w3.org/International/articles/language-tags/) — background on BCP 47 locale codes, which the URL prefix scheme surfaces one-to-one.
