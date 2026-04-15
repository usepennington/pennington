---
title: "Generate a sitemap"
description: "Expose an auto-built /sitemap.xml that enumerates every canonical URL, skips drafts and redirects, and uses front-matter dates for lastmod."
uid: how-to.configuration.sitemap
order: 202080
sectionLabel: Configuration
tags: [sitemap, seo, canonical-base-url, front-matter]
---

Pennington registers and serves `/sitemap.xml` automatically on any `AddPennington`-based host, so if you have a working site you are already emitting a sitemap. Come here when you need crawlers to see absolute `<loc>` values, need to confirm that drafts and redirects are excluded, or need to turn off sitemap generation on a BlogSite host. If you do not have a site yet, start with <xref:tutorials.getting-started.first-site>.

## Assumptions

- You have a working Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- Your pages use an `IFrontMatter` implementation — `DocFrontMatter`, `BlogFrontMatter`, or your own — so `IsDraft` and (optionally) `Date` flow through to the sitemap builder
- You know whether you are publishing at a fully-qualified URL (set `CanonicalBaseUrl`) or under a sub-path via `dotnet run -- build /sub/` (the sitemap falls back to `OutputOptions.BaseUrl`)

---

## Steps

### 1. Confirm `/sitemap.xml` is already wired

`AddPennington` registers `SitemapService` and `UsePennington` maps `GET /sitemap.xml` to it — there is no `AddSitemap(...)` call to make and no toggle on `PenningtonOptions`. The service walks every registered `IContentService.DiscoverAsync` result, skipping non-HTML outputs and `RedirectSource` placeholders before the builder applies its own filters.

```csharp:xmldocid
T:Pennington.Feeds.SitemapService
```

### 2. Set `CanonicalBaseUrl` so `<loc>` values resolve

When `CanonicalBaseUrl` is set on `PenningtonOptions`, `DocSiteOptions`, or `BlogSiteOptions`, the sitemap builder prefixes every URL with it — typically `https://your-domain.com/` — producing the absolute `<loc>` entries crawlers require. When it is not set and the static build targets a sub-path (`dotnet run -- build /sub/`), the builder falls back to `OutputOptions.BaseUrl`, producing entries like `/sub/page/`; crawlers can resolve those relative to the sitemap URL, but fully-qualified values are preferred.

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

The backing property on `BlogSiteOptions`:

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl
```

### 3. Use `IsDraft` and `redirectUrl:` to exclude pages

`SitemapBuilder.Build` drops any candidate whose front matter has `isDraft: true` and drops any candidate whose front matter implements `IRedirectable` with a non-empty `RedirectUrl` — redirect stubs are never listed as canonical URLs. Note that `search: false` and `llms: false` are not honored here; those are search-UX preferences, not SEO directives, so opting a page out of client-side search does not remove it from the sitemap.

```csharp:xmldocid,bodyonly
M:Pennington.Feeds.SitemapBuilder.Build(System.Collections.Generic.IReadOnlyList{Pennington.Feeds.SitemapCandidate})
```

The two front-matter members that drive the filter:

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.IsDraft
P:Pennington.FrontMatter.IRedirectable.RedirectUrl
```

### 4. (BlogSite only) Set `EnableSitemap = false` to turn it off

On an `AddBlogSite` host, `BlogSiteOptions.EnableSitemap` (default `true`) is the one knob that unregisters the `/sitemap.xml` endpoint — set it to `false` when the host environment owns its own sitemap. On a bare `AddPennington` or `AddDocSite` host the endpoint is always mapped; there is no equivalent toggle because the sitemap has no per-request cost when nothing fetches it.

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap
```

---

## Verify

- Run `dotnet run` and fetch `/sitemap.xml` — expect a `<urlset>` document with one `<url><loc>…</loc></url>` per non-draft, non-redirect page
- Mark a page `isDraft: true` or set `redirectUrl:` on it and refetch — expect that URL to be absent from the `<urlset>`
- Publish with `CanonicalBaseUrl = "https://example.com"` and confirm every `<loc>` starts with `https://example.com/`; omit it and run `dotnet run -- build /sub/` to see `<loc>` values start with `/sub/`

## Related

- Reference: [Auxiliary options](xref:reference.options.auxiliary-options)
- How-to: [Generate RSS feeds](xref:how-to.configuration.rss)
- How-to: [Configure redirects](xref:how-to.content-authoring.redirects)
