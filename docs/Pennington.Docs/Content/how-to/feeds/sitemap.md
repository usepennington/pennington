---
title: "Publish a sitemap"
description: "Expose an auto-built /sitemap.xml that enumerates every canonical URL, skips drafts and redirects, and uses front-matter dates for lastmod."
uid: how-to.feeds.sitemap
order: 207030
sectionLabel: "Feeds & Indexes"
tags: [sitemap, seo, canonical-base-url, front-matter]
---

`/sitemap.xml` is registered and served automatically on every `AddPennington`-based host; a working site already emits one. The knobs below tune what crawlers see — absolute `<loc>` values, draft/redirect exclusion, or turning the sitemap off on BlogSite. For a first site, start with <xref:tutorials.getting-started.first-site>.

## Before you begin
- A working Pennington site (see <xref:tutorials.getting-started.first-site> if not)
- Pages using an `IFrontMatter` implementation — `DocFrontMatter`, `BlogFrontMatter`, or a custom one — so `IsDraft` and (optionally) `Date` flow through to the sitemap builder
- A known publishing target: either a fully-qualified URL (set `CanonicalBaseUrl`) or a sub-path via `dotnet run -- build /sub/` (the sitemap falls back to `OutputOptions.BaseUrl`)

---

## Options

### Set `CanonicalBaseUrl` so `<loc>` values resolve

When `CanonicalBaseUrl` is set on `PenningtonOptions`, `DocSiteOptions`, or `BlogSiteOptions`, the sitemap builder prefixes every URL with it — typically `https://your-domain.com/` — producing the absolute `<loc>` entries crawlers require. Without it, entries fall back to the build's `--base-url` value or to `/`.

```csharp
new BlogSiteOptions
{
    CanonicalBaseUrl = "https://example.com",
    // ...
}
```

### Exclude drafts and redirects with front matter

`SitemapBuilder.Build` drops any candidate whose front matter has `isDraft: true` or implements `IRedirectable` with a non-empty `RedirectUrl`. `search: false` and `llms: false` are not honored — those are client-side UX preferences, not SEO directives, so opting a page out of search does not remove it from the sitemap.

### (BlogSite only) Turn the sitemap off with `EnableSitemap = false`

On an `AddBlogSite` host, set `BlogSiteOptions.EnableSitemap = false` to unregister the `/sitemap.xml` endpoint — useful when the host environment owns its own sitemap. On bare `AddPennington` or `AddDocSite`, the endpoint is always mapped.

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
- How-to: [Publish an RSS feed](xref:how-to.feeds.rss)
- How-to: [Configure redirects](xref:how-to.pages.redirects)
