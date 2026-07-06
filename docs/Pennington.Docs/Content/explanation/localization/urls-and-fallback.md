---
title: "Locale-aware URLs and content fallback"
description: "Why Pennington treats the URL path prefix as the authoritative locale signal, how DocSiteContentResolver strips it and falls back to the default locale, and why the search index is split per locale."
uid: explanation.localization.urls-and-fallback
order: 1
sectionLabel: "Localization"
tags: [localization, routing, fallback, search]
---

When a reader hits `/fr/guides/intro`, how does Pennington know which language to serve, which markdown file to load, and what to do if `guides/intro.md` only exists in English?

## Why the URL is the locale signal

A multilingual site has to pick a locale signal early enough that routing, content resolution, HTML rewriters, and the search index all agree on it. The obvious candidate is the browser's `Accept-Language` header, but that signal is noisy: users travel, share links, and sit behind corporate proxies, and a locale chosen from a header cannot be bookmarked, cached by a CDN, or indexed distinctly by search engines. Pennington uses the URL instead. The path prefix (`/fr/…`, `/es/…`) is the single source of truth, and the default locale owns the unprefixed root. That same prefix drives both request-time content resolution in development and build-time output placement during the static crawl — one code path, one answer. Because translation coverage is rarely complete, the question "what happens when `/fr/foo` has no French source?" needs a principled answer, and that is what the rest of this page explores.

## How it works

### URL prefix drives detection

`LocaleDetectionMiddleware` reads `HttpContext.Request.Path`, asks `LocalizationOptions.GetLocaleFromUrl` which locale the first path segment maps to, and populates the scoped `LocaleContext`. When the detected locale is not the default, the middleware rewrites the request path to strip the prefix and pushes that prefix onto `PathBase` so URL generation stays correct. The net effect is that Blazor routing and every downstream consumer see `/guides/intro` rather than `/fr/guides/intro`, which means a single `@page "/guides/intro"` directive serves every locale without duplication.

The middleware never consults `Accept-Language`. The culture provider (`PenningtonUrlRequestCultureProvider`) also derives its answer from the URL, so the entire request pipeline agrees on the same locale code without any negotiation step. Prefix-first means the same link produces the same page for everyone who clicks it, regardless of their browser settings or location.

### Resolving content: precomputed routes, then a runtime miss

`DocSiteContentResolver.GetContentByUrlAsync` takes the full URL — locale prefix intact — and resolves it against the registered content services. The first attempt asks every `IContentService` for a route that matches the exact URL, prefix and all. This is where two distinct flavors of fallback get conflated if you are not careful, so it is worth separating them.

The first is *startup-precomputed* fallback. When a multi-locale `MarkdownContentService` discovers a default-locale file, it registers not only that file's own route but also an extra route at each non-default locale prefix that lacks its own copy — `/fr/guides/intro` pointing at the English `guides/intro.md`, with `ContentRoute.IsFallback` already set to `true`. That route exists in the table before any request arrives. So the exact-URL match for `/fr/guides/intro` can succeed outright, and the resolver reads the fallback flag off the route it found (`rendered.Route.IsFallback`) rather than computing anything. This is the common path: most missing translations are known at startup.

The second is *runtime* fallback, and it only runs when the first attempt finds nothing at all. If no route matched, the site is multi-locale, and the active locale is not the default, the resolver strips the prefix via `LocalizationOptions.StripLocalePrefix` and resolves again against the content-relative path (`guides/intro`). If that second resolve succeeds, the resolver sets `IsFallback` itself. This path covers content sources that don't precompute fallback routes the way `MarkdownContentService` does, so the resolver still degrades gracefully to the default locale.

Either way — flag read off a precomputed route, or set after a runtime miss — the resolver records `RequestedLocale` so the view layer can render a "this page has not been translated yet" notice via `FallbackNotice`. The resolver never rewrites URLs — the URL the reader typed stays in the address bar, only the content source changes.

### Fallback: default locale stands in

There is exactly one fallback step: the default locale. A missing `/es/guides/intro` falls through to `guides/intro` — the unprefixed default-locale source — not to `/fr/guides/intro`. There is no cascade across non-default locales, no per-locale fallback chain, and no "similar language" matcher. The reason is that a cascade hides coverage gaps. Readers end up seeing a page in a language they did not request and cannot explain why, and translators cannot tell which pages still need coverage because every miss silently inherits from a neighbor. A single fallback step keeps the rule easy to reason about and makes missing translations easy to find.

The default locale is not a special kind of locale — it is the locale that owns the unprefixed URL space. `StripLocalePrefix` is a no-op for the default locale, `BuildLocaleUrl` emits unprefixed URLs for it, and content authored at `Content/guides/intro.md` is default-locale content by virtue of sitting outside every locale subdirectory. The fallback rule is a consequence of those URL-math rules, not a separate "fallback locale" setting.

`StripLocalePrefix` (see <xref:reference.api.localization-options>) is pure URL math with no file-system knowledge. `DocSiteContentResolver` decides whether the stripped path resolves to a file on disk, and that separation is what lets the same helper serve both request-time fallback and build-time URL generation.

### Per-locale search indices

The search index is split per locale — a query typed into the Spanish UI ranks against Spanish content, not a mixed-language pool — and <xref:explanation.discovery.search> covers how that split is built and served. What belongs here is how the split interacts with fallback.

A page that exists only in the default locale appears once, in the default-locale index. Fallback happens when a page renders; it does not add that page to other locales' indices. That asymmetry is deliberate. When a French reader searches for a term that only exists in English content, the right answer is "no results in French" rather than silently returning English hits the reader cannot read. The same per-locale split applies to sitemap construction and `hreflang` alternate-language tags, so search, sitemap, and alternates report the same set of pages for each locale.

## Further reading

- Reference: [`LocalizationOptions`](xref:reference.api.localization-options) — `DefaultLocale`, `AddLocale`, and the URL helpers that back this mechanism.
- How-to: [Enable multiple locales](xref:how-to.discovery.localization) — the recipe for populating `LocalizationOptions`, organizing content subdirectories, and wiring `UseLocaleRouting`.
- External: [W3C — Language tags in HTML and XML](https://www.w3.org/International/articles/language-tags/) — background on BCP 47 locale codes, which the URL prefix scheme surfaces one-to-one.
