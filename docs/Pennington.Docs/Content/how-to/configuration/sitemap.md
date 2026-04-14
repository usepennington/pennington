---
title: "Generate a sitemap"
description: "Expose an auto-built /sitemap.xml that enumerates every canonical URL, skips drafts and redirects, and uses front-matter dates for lastmod."
uid: how-to.configuration.sitemap
order: 202080
sectionLabel: Configuration
tags: [sitemap, seo, canonical-base-url, front-matter]
---

> **In this page.** _Paraphrase TOC "Covers": sitemap generation is on by default on any `AddPennington`-based host, the endpoint lives at `/sitemap.xml`, and drafts plus `redirectUrl:`-bearing pages are filtered out. Two sentences max — skip the "what is a sitemap" preamble._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": submitting the generated sitemap to Google Search Console, Bing Webmaster Tools, or any other search console. One sentence, no inbound link needed._

## When to use this

_Two to three sentences. Frame the realistic arrival state: the reader has a Pennington site serving HTML and wants crawlers to find every canonical URL with a correct absolute `<loc>` and, where available, a `<lastmod>` from the page's `date:` front matter. Note that the work is almost entirely configuration, not code: `/sitemap.xml` is already mapped by `UsePennington`, so the only real knob is making sure `CanonicalBaseUrl` is set and that drafts and redirects are using the right front-matter keys. If the reader has no site yet, point back to [_Your first Pennington site_](xref:tutorials.getting-started.first-site)._

## Assumptions

_Keep to three bullets. The non-obvious one is the `CanonicalBaseUrl`/build-BaseUrl fallback — it is the only thing that meaningfully changes the emitted XML._

- You have a working Pennington site (see [_Your first Pennington site_](xref:tutorials.getting-started.first-site) if not)
- Your pages use an `IFrontMatter` implementation — `DocFrontMatter`, `BlogFrontMatter`, or your own — so `IsDraft` and (optionally) `Date` flow through to the sitemap builder
- You know whether you are publishing at a fully-qualified URL (set `CanonicalBaseUrl`) or under a sub-path via `dotnet run -- build /sub/` (the sitemap falls back to `OutputOptions.BaseUrl`)

To copy a working setup, see [`examples/BlogKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogKitchenSinkExample) — `ServiceConfiguration.BuildBlogSiteOptions` sets `CanonicalBaseUrl` and explicitly pins `EnableSitemap = true` even though that is the default. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Step 1 orients the reader on the default-on behaviour so they stop looking for a switch. Step 2 is the one real config lever (`CanonicalBaseUrl`). Step 3 covers the two filter paths that remove pages. Step 4 is the BlogSite-specific opt-out. Verb-first headings, prose under two sentences per step._

### 1. Know that `/sitemap.xml` is already wired

_One or two sentences. `AddPennington` registers `SitemapService` behind `FileWatchDependencyFactory<T>` (so the XML is rebuilt when content changes) and `UsePennington` maps `GET /sitemap.xml` to it — there is no `AddSitemap(...)` call to make and no toggle on `PenningtonOptions`. The service walks every registered `IContentService.DiscoverAsync` result, skipping non-HTML outputs (SPA JSON, static assets) and `RedirectSource` placeholders before the builder applies its own filters._

```csharp:xmldocid
T:Pennington.Feeds.SitemapService
```

### 2. Set `CanonicalBaseUrl` so `<loc>` values resolve

_Two sentences. When `PenningtonOptions.CanonicalBaseUrl` (or `DocSiteOptions.CanonicalBaseUrl` / `BlogSiteOptions.CanonicalBaseUrl`) is set, the sitemap builder prefixes every URL with it verbatim — typically a fully-qualified `https://host/` so the emitted `<loc>` entries are absolute as crawlers require. When it is not set but the static build targets a sub-path (`dotnet run -- build /sub/`), the builder falls back to `OutputOptions.BaseUrl` so entries become `/sub/page/` rather than bare `/page/`; crawlers resolve those relative to the sitemap URL itself, which still works but is weaker than a fully-qualified base._

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

_Show the backing property so the reader can cross-check:_

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl
```

### 3. Rely on `IsDraft` and `redirectUrl:` to keep pages out

_Two sentences. `SitemapBuilder.Build` drops any candidate whose metadata reports `IsDraft = true` (so `isDraft: true` in front matter keeps a compiled page out of the XML) and drops any candidate whose metadata implements `IRedirectable` with a non-empty `RedirectUrl` (so a redirect stub is never listed as a canonical URL). Note that `search: false` / `llms: false` are deliberately **not** honored here — those are search UX preferences, not SEO directives, so a page opted out of client-side search still appears in the sitemap unless it is also a draft or a redirect._

```csharp:xmldocid,bodyonly
M:Pennington.Feeds.SitemapBuilder.Build(System.Collections.Generic.IReadOnlyList{Pennington.Feeds.SitemapCandidate})
```

_For the two front-matter members that drive the filter, see:_

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.IsDraft
P:Pennington.FrontMatter.IRedirectable.RedirectUrl
```

### 4. (BlogSite only) Flip `EnableSitemap = false` to turn it off

_One or two sentences. On an `AddBlogSite` host, `BlogSiteOptions.EnableSitemap` (default `true`) is the one knob that unregisters the `/sitemap.xml` endpoint — set it to `false` only when you are hosting the blog somewhere that owns its own sitemap. On a bare `AddPennington` or `AddDocSite` host the endpoint is always mapped; there is no equivalent toggle on `PenningtonOptions` or `DocSiteOptions` because the sitemap has no per-request cost when nothing fetches it._

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap
```

---

## Verify

_Terse. Three bullets — one per filter path plus one for the base-URL behaviour._

- Run `dotnet run` and fetch `/sitemap.xml` — expect a `<urlset>` document with one `<url><loc>…</loc></url>` per non-draft, non-redirect page
- Mark a page `isDraft: true` or set `redirectUrl:` on it and refetch — expect that URL to be absent from the `<urlset>`
- Publish with `CanonicalBaseUrl = "https://example.com"` and confirm every `<loc>` starts with `https://example.com/`; omit it and run `dotnet run -- build /sub/` to see `<loc>` values start with `/sub/`

## Related

_Three cross-quadrant links: the reference page for the auxiliary options catalogue (where `OutputOptions.BaseUrl` lives), the sibling RSS how-to (same `CanonicalBaseUrl` story), and the redirects how-to (the other side of the redirect-filter behaviour). Do not link to the next how-to in this section — auto-generated._

- Reference: [_`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`_](xref:reference.options.auxiliary-options)
- How-to: [_Generate RSS feeds_](xref:how-to.configuration.rss)
- How-to: [_Configure redirects_](xref:how-to.content-authoring.redirects)
