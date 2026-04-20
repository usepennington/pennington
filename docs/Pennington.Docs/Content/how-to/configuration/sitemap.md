---
title: "Publish a sitemap"
description: "Expose an auto-built /sitemap.xml that enumerates every canonical URL, skips drafts and redirects, and uses front-matter dates for lastmod."
uid: how-to.configuration.sitemap
order: 202080
sectionLabel: Configuration
tags: [sitemap, seo, canonical-base-url, front-matter]
---

When crawlers need a canonical URL list to ingest the site, `/sitemap.xml` is registered and served automatically on any `AddPennington`-based host — a working site already emits one. The knobs below give crawlers absolute `<loc>` values, confirm that drafts and redirects are excluded, or turn off sitemap generation on a BlogSite host. For a first site, start with <xref:tutorials.getting-started.first-site>.

## Assumptions

- A working Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- Pages using an `IFrontMatter` implementation — `DocFrontMatter`, `BlogFrontMatter`, or a custom one — so `IsDraft` and (optionally) `Date` flow through to the sitemap builder
- A known publishing target: either a fully-qualified URL (set `CanonicalBaseUrl`) or a sub-path via `dotnet run -- build /sub/` (the sitemap falls back to `OutputOptions.BaseUrl`)

---

## Options

### Confirm `/sitemap.xml` is already wired

`AddPennington` registers `SitemapService` and `UsePennington` maps `GET /sitemap.xml` to it. There is no `AddSitemap(...)` call to make and no toggle on `PenningtonOptions`. The service walks every registered `IContentService.DiscoverAsync` result, skipping non-HTML outputs and `RedirectSource` placeholders before the builder applies its own filters.

### Set `CanonicalBaseUrl` so `<loc>` values resolve

When `CanonicalBaseUrl` is set on `PenningtonOptions`, `DocSiteOptions`, or `BlogSiteOptions`, the sitemap builder prefixes every URL with it — typically `https://your-domain.com/` — producing the absolute `<loc>` entries crawlers require. When it is not set and the static build targets a sub-path (`dotnet run -- build /sub/`), the builder falls back to `OutputOptions.BaseUrl`, producing entries like `/sub/page/`. Crawlers can resolve those relative to the sitemap URL, but fully-qualified values are preferred.

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

See <xref:reference.api.blog-site-options> for the backing `CanonicalBaseUrl` property.

### Use `IsDraft` and `redirectUrl:` to exclude pages

`SitemapBuilder.Build` drops any candidate whose front matter has `isDraft: true` and drops any candidate whose front matter implements `IRedirectable` with a non-empty `RedirectUrl`; redirect stubs are never listed as canonical URLs. `search: false` and `llms: false` are not honored here. Those are search-UX preferences, not SEO directives, so opting a page out of client-side search does not remove it from the sitemap.

```csharp:xmldocid,bodyonly
M:Pennington.Feeds.SitemapBuilder.Build(System.Collections.Generic.IReadOnlyList{Pennington.Feeds.SitemapCandidate})
```

The two front-matter members that drive the filter are `IFrontMatter.IsDraft` and `IRedirectable.RedirectUrl`; see <xref:reference.api.i-front-matter>.

### (BlogSite only) Set `EnableSitemap = false` to turn it off

On an `AddBlogSite` host, `BlogSiteOptions.EnableSitemap` (default `true`) is the one knob that unregisters the `/sitemap.xml` endpoint. Set it to `false` when the host environment owns its own sitemap. On a bare `AddPennington` or `AddDocSite` host the endpoint is always mapped; there is no equivalent toggle because the sitemap has no per-request cost when nothing fetches it.

---

## Result

`/sitemap.xml` returns a `<urlset>` with one `<url>` per non-draft, non-redirect page, with absolute `<loc>` values when `CanonicalBaseUrl` is set:

```xml
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url>
    <loc>https://example.com/</loc>
    <lastmod>2024-01-15</lastmod>
  </url>
  <url>
    <loc>https://example.com/how-to/configuration/sitemap/</loc>
    <lastmod>2024-02-03</lastmod>
  </url>
</urlset>
```

## Verify

- Run `dotnet run` and fetch `/sitemap.xml`. Expect a `<urlset>` document with one `<url><loc>…</loc></url>` per non-draft, non-redirect page
- Mark a page `isDraft: true` or set `redirectUrl:` on it and refetch. That URL is absent from the `<urlset>`
- Publish with `CanonicalBaseUrl = "https://example.com"` and confirm every `<loc>` starts with `https://example.com/`. Omit it and run `dotnet run -- build /sub/` to see `<loc>` values start with `/sub/`

## Related

- Reference: [`SitemapService`](xref:reference.api.sitemap-service)
- How-to: [Publish an RSS feed](xref:how-to.configuration.rss)
- How-to: [Configure redirects](xref:how-to.content-authoring.redirects)
