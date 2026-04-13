---
title: "Generate a sitemap"
description: "Enable sitemap generation, serve /sitemap.xml, and filter drafts and redirects."
section: "deployment"
order: 90
tags: []
uid: how-to.deployment.sitemap
isDraft: true
search: false
llms: false
---

> **In this page.** Enabling sitemap generation, the `/sitemap.xml` route, and how drafts/redirects are filtered.
>
> **Not in this page.** Submitting the sitemap to search consoles.

## When to use this

- Outline bullet: You already have a Pennington site running and need a `/sitemap.xml` for SEO / crawlers before deployment.
- Outline bullet: Use before the host-specific recipes in this section — the static build output should already contain `sitemap.xml` when you upload it.

## Assumptions

- Outline bullet: You have an existing Pennington site (DocSite, BlogSite, or bare `AddPennington`) that serves pages under `dotnet run`.
- Outline bullet: You have set `CanonicalBaseUrl` on `DocSiteOptions` / `BlogSiteOptions` (or `PenningtonOptions.CanonicalBaseUrl`) to the absolute production URL — the sitemap emits absolute `<loc>` values through `SitemapBuilder`'s canonical base.
- Outline bullet: You understand that sitemap generation runs through the same HTTP pipeline as dev-serve; nothing extra is needed for the build verb.
- Outline bullet: Example to reference: `examples/AlexBlogExample` (BlogSite with `EnableSitemap = true`, `CanonicalBaseUrl` set).

---

## Steps

### 1. Confirm the sitemap endpoint is wired

- Outline bullet: `AddPennington` registers `SitemapBuilder` and file-watched `SitemapService` (`src/Pennington/Infrastructure/PenningtonExtensions.cs` lines 174 and 189) — no separate opt-in is required for core sites.
- Outline bullet: `UsePennington` maps `app.MapGet("/sitemap.xml", ...)` unconditionally (same file, line 380) — the endpoint exists as soon as middleware is wired.
- Outline bullet: `BlogSiteOptions.EnableSitemap` (default `true`) lives alongside `EnableRss` for discoverability; the core `/sitemap.xml` route is still provided by `UsePennington`.

### 2. Set the canonical base URL

- Outline bullet: The sitemap serializer emits absolute `<loc>` values built from `ContentRoute.AbsoluteUrl(canonicalBase)` in `SitemapBuilder.Build` — without a canonical base, URLs start with `/` and will be rejected by search engines.
- Outline bullet: For DocSite / BlogSite, set `CanonicalBaseUrl` on the options record; for bare `AddPennington`, set `PenningtonOptions.CanonicalBaseUrl`.
- Outline bullet: Reference fence: `examples/AlexBlogExample/Program.cs` shows `CanonicalBaseUrl = "https://alexchen.dev"` next to `EnableSitemap = true`.

### 3. Understand what gets included

- Outline bullet: `SitemapService.BuildSitemapAsync` enumerates every registered `IContentService.DiscoverAsync` — markdown sources, Razor pages, and programmatic generators all land as candidates.
- Outline bullet: Non-HTML outputs are filtered by extension check (`.html` / `.htm` only); the SPA navigation service's `/_spa-data/*.json` routes are dropped here.
- Outline bullet: Only `MarkdownFileSource` entries go through the parser to pick up `Date` (→ `<lastmod>`) and `IsDraft`; programmatic and Razor routes are emitted with URL only (no `<lastmod>`) — parsing every programmatic generator at sitemap time is avoided by design.
- Outline bullet: Per-page `search:` / `llms:` opt-outs are **not** honored by the sitemap — they are UX preferences, not SEO directives (see `SitemapService` XML doc).

### 4. Understand draft and redirect filtering

- Outline bullet: Drafts: `SitemapBuilder.Build` skips candidates whose metadata has `IsDraft == true` (markdown front matter `isDraft: true`).
- Outline bullet: Redirects: two layers drop them — `SitemapService.BuildSitemapAsync` skips any `DiscoveredItem` whose `Source is RedirectSource`, and `SitemapBuilder.Build` additionally skips metadata implementing `IRedirectable` with a non-empty `RedirectUrl`.
- Outline bullet: Markdown pages whose parse fails are dropped (the page is unlikely to render anyway); treat parse failures in the `BuildReport` as the signal to fix, not as missing sitemap entries.

### 5. Handle multi-locale sites

- Outline bullet: When `LocalizationOptions.IsMultiLocale` is true, `SitemapService` emits `xhtml:link rel="alternate" hreflang="..."` entries alongside each `<loc>` for every locale where the content-relative URL has a translation.
- Outline bullet: `hreflang` values come from `LocaleInfo.HtmlLang` when set, otherwise the locale code itself.
- Outline bullet: No configuration change is needed — the hreflang block appears automatically when more than one locale is registered.

### 6. Rebuild the static site

- Outline bullet: Run `dotnet run --project <yourSite> -- build https://your.site output` — the crawler fetches `/sitemap.xml` as part of the normal HTTP crawl (see `how-to/deployment/static-build`).
- Outline bullet: `SitemapService` is registered via `AddFileWatched<T>` so dev-serve reflects content edits instantly without a rebuild; the build verb produces the same XML because both paths share the HTTP pipeline.

---

## Verify

- Outline bullet: Run `dotnet run` and request `/sitemap.xml` — expect an `application/xml` response with a `<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">` root.
- Outline bullet: Confirm a draft page (`isDraft: true`) is absent and a redirect (`redirectUrl: "..."`) is absent from the `<loc>` list.
- Outline bullet: After `build`, open `output/sitemap.xml` and confirm every `<loc>` begins with your `CanonicalBaseUrl`.

## Related

- Reference: [PenningtonOptions (CanonicalBaseUrl)](/reference/options/pennington-options)
- Reference: [BlogSiteOptions (EnableSitemap)](/reference/options/blogsite-options)
- Background: [How static build mirrors dev-serve](/explanation/architecture/dev-build-unified-path)
- Neighbor how-to: [Build a static site](/how-to/deployment/static-build)
