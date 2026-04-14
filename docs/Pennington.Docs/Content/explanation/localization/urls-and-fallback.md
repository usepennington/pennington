---
title: "Locale-aware URLs and content fallback"
description: "Why Pennington treats the URL path prefix as the authoritative locale signal, how ContentResolver strips it and falls back to the default locale, and why the search index is split per locale."
uid: explanation.localization.urls-and-fallback
order: 10
sectionLabel: "Localization"
tags: [localization, routing, fallback, search]
---

> **In this page.** _Paraphrase the Covers line: the URL prefix is what drives locale detection, `ContentResolver` strips the prefix before searching content sources, a missing translation falls back to the default locale, and the search index is emitted one JSON file per locale. One sentence._
>
> **Not in this page.** _Paraphrase the Does-not-cover line: point readers who want the full `LocalizationOptions` surface — `DefaultLocale`, `AddLocale`, the URL helpers — to the reference page rather than recapping them here. One sentence._

## The question

_Ask the question in one sentence, shaped like: "When a reader hits `/fr/guides/intro`, how does Pennington know which language to serve, which markdown file to load, and what to do if `guides/intro.md` only exists in English?" Do not answer yet — the rest of the page is the answer._

## Context

_Three to five sentences. Start with the design tension: a multilingual static site has to pick a locale signal early enough that routing, content resolution, rewriters, and the search index all agree on it. The obvious candidate is the browser's `Accept-Language` header, but that is notoriously noisy — users travel, share links, and sit behind corporate proxies, and a locale chosen from a header cannot be bookmarked, cached by a CDN, or indexed distinctly by search engines. Pennington picks the opposite invariant: the URL path prefix (`/fr/…`, `/es/…`) is the single source of truth, and the default locale owns the unprefixed root. Note that the same prefix drives both request-time content resolution in dev and build-time output placement during the static crawl — one code path, one answer. Close by framing the fallback question the rest of the page resolves: because translation coverage is rarely complete, "what happens when `/fr/foo` has no French source?" has to have a principled answer, not an accident._

## How it works

_Four subsections, each picking up where the previous left off. Prose first; reach for a code fence only when the type signature makes the point sharper._

### URL prefix drives detection

_Two paragraphs. `LocaleDetectionMiddleware` reads `HttpContext.Request.Path`, asks `LocalizationOptions.GetLocaleFromUrl` which locale the first segment maps to, and populates the scoped `LocaleContext`. If the detected locale is not the default, it rewrites the request path to strip the prefix and pushes that prefix onto `PathBase` so URL generation stays correct. The net effect: Blazor routing and every downstream consumer see `/guides/intro`, not `/fr/guides/intro`, so a single `@page "/guides/intro"` directive serves every locale._

```csharp:xmldocid
T:Pennington.Localization.LocaleDetectionMiddleware
```

_After the fence, one paragraph. Emphasize that the middleware never consults `Accept-Language` — the culture provider that does (`PenningtonUrlRequestCultureProvider`) also derives its answer from the URL, so the entire request pipeline agrees on the same locale code. The Accept-Language header is advisory at most, never authoritative; prefix-first means the same link produces the same page regardless of who clicks it._

### ContentResolver normalizes then searches

_Two paragraphs. `ContentResolver.GetContentByUrlAsync` takes the full URL (locale prefix intact, post-commit `d4947a0`) and does two passes. First pass: ask every `IContentService` for a `DiscoveredItem` whose route matches the exact URL — this finds locale-specific markdown (`/fr/guides/intro`) and any `IsFallback` route a service pre-computed. Second pass (runtime fallback): if the first pass missed and the site is multi-locale and the current locale is not the default, strip the prefix via `LocalizationOptions.StripLocalePrefix` and search again against the content-relative path. Either a localized file wins or the default-locale file stands in._

```csharp:xmldocid
M:Pennington.DocSite.Services.ContentResolver.GetContentByUrlAsync(System.String)
```

_After the fence, one paragraph. Describe what the resolver carries forward: when the second pass succeeds, it sets `IsFallback: true` and records `RequestedLocale` so the view layer can render a "this page has not been translated yet" notice (see `FallbackNotice`). The resolver never rewrites URLs — the URL the reader typed stays in the address bar, only the content source changes._

### Fallback: default locale stands in

_Two paragraphs. Explain the fallback policy plainly: there is exactly one fallback rung, the default locale. A missing `/es/guides/intro` falls through to `guides/intro` (the unprefixed default-locale source), not to `/fr/guides/intro`. There is no cascade across non-default locales, no per-locale fallback chain, no "similar language" matcher. Why: a cascade is easy to write and hard to reason about — readers end up seeing a page in a language they did not request and cannot explain, and translators cannot tell which pages still need coverage because every miss silently inherits from a neighbor. One rung keeps the mental model compact and the coverage report honest._

_Second paragraph. The default locale is not a special kind of locale — it is just the locale that owns the unprefixed URL space. `StripLocalePrefix` is a no-op on the default locale, `BuildLocaleUrl` emits unprefixed URLs for it, and content authored at `Content/guides/intro.md` is default-locale content by virtue of sitting outside every locale subdirectory. The fallback rule follows from those URL-math rules, not from a separate "fallback locale" setting._

```csharp:xmldocid
M:Pennington.Localization.LocalizationOptions.StripLocalePrefix(System.String,System.String)
```

_After the fence, one sentence: `StripLocalePrefix` is pure URL math with no file-system knowledge — `ContentResolver` decides whether the stripped path actually resolves to a file, and that separation is what lets the same helper serve both request-time fallback and build-time URL generation._

### Per-locale search indices

_Two paragraphs. `SearchIndexService` emits one JSON bucket per configured locale, keyed by the TOC item's route locale (or `DefaultLocale` when the route has none). `UsePennington` maps a per-locale endpoint — `/search-index-{code}.json` — and the client fetches only the index for the active locale. This keeps the client payload small on multilingual sites (a reader browsing `/es/` never downloads French or Japanese documents) and, more importantly, keeps search semantics scoped: a query typed into the Spanish UI ranks against Spanish content, not against a pool where mixed-language term frequencies distort results._

```csharp:xmldocid
T:Pennington.Search.SearchIndexService
```

_After the fence, one paragraph connecting the split to the fallback rule above. A page that exists only in the default locale appears once, in the default-locale index — fallback is a rendering-time courtesy, not an indexing decision. That asymmetry is deliberate: if a French reader searches for a term that only exists in English content, the right answer is "no results in French" rather than "silently return English hits that the reader cannot read." The same per-locale split flows through sitemap construction and `hreflang` alternate-language tags, so every discovery channel agrees on what lives in which locale._

## Trade-offs

_Three to four bullets. Name the real costs, not stylistic preferences._

- **Cost — every translation is an extra file with an extra URL.** _There is no in-band `locale:` front-matter flag that turns one file into many; each translation of `guides/intro.md` lives at `fr/guides/intro.md`, `es/guides/intro.md`, and so on. This is more boilerplate than a single-file scheme, but it makes coverage obvious (list the directory) and diffing trivial (a PR that touches `fr/` is a French-translation PR)._
- **Alternative considered — Accept-Language-driven locale routing.** _A content-negotiation shape where `/guides/intro` transparently serves French to French browsers was rejected: it breaks bookmarking, CDN caching, search-engine indexing, and link sharing, all for the sake of saving a `/fr/` prefix in the URL. The prefix-first rule trades one cosmetic cost for five structural wins._
- **Cost — fallback silence.** _A reader hitting `/fr/only-english-page` sees English content under a French URL. The `FallbackNotice` banner softens this, but the URL itself lies about the language of the body until a French translation lands. The alternative — a 404 for untranslated pages — was rejected as too hostile to partial-coverage sites, which is almost every real-world multilingual site during rollout._
- **Consequence — search stays honest per locale.** _Because the index is split, a Japanese search UI that is missing half its content shows "no results" instead of pretending otherwise. Authors get an accurate signal about translation gaps from the search experience itself, which is exactly the feedback loop that a cross-locale index would hide._

## Further reading

_Three cross-quadrant links, one per line. Do NOT include the sibling explanation link — that is auto-generated._

- Reference: [`LocalizationOptions`](xref:reference.options.localization-options) — `DefaultLocale`, `AddLocale`, and the URL helpers that back this mechanism.
- How-to: [Enable multiple locales](xref:how-to.configuration.localization) — the recipe for populating `LocalizationOptions`, organizing content subdirectories, and wiring `UsePenningtonLocaleRouting`.
- External: [W3C — Language tags in HTML and XML](https://www.w3.org/International/articles/language-tags/) — background on BCP 47 locale codes, which the URL prefix scheme surfaces one-to-one.
