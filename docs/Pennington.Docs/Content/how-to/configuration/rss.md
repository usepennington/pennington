---
title: "Generate RSS feeds"
description: "Enable RSS on a blog source, use BlogFrontMatter.Date, and locate the generated feed file."
section: "configuration"
order: 70
tags: []
uid: how-to.configuration.rss
isDraft: true
search: false
llms: false
---

> **In this page.** Enable RSS on a blog source, populate `BlogFrontMatter.Date`, and find where the feed is written.
>
> **Not in this page.** JSON Feed, Atom format selection, and multi-source feed merging.

## When to use this

- One to two sentences: reader has a running `BlogSite` and wants a subscribable `/rss.xml` for the posts they already publish.
- Point upstream to the Blog tutorial if the reader has no `BlogSite` yet.

## Assumptions

- You have an existing Pennington `BlogSite` wired with `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync`.
- Posts already render at `/blog/...` using `BlogSiteFrontMatter` (or equivalent `BlogFrontMatter`).
- You know the public URL of the site (you will set `CanonicalBaseUrl` so feed links are absolute).
- To copy a working setup, see [`examples/AlexBlogExample`](https://github.com/usepennington/pennington/tree/main/examples/AlexBlogExample).

---

## Steps

### 1. Enable RSS on the blog site

- Keep the default or set `EnableRss = true` on `BlogSiteOptions`.
- Set `CanonicalBaseUrl` so feed `<link>` elements resolve to absolute URLs.
- No separate service registration — `AddBlogSite` already wires `BlogSiteContentService`, and `UseBlogSite` maps `/rss.xml` when `EnableRss` is true.

```csharp:xmldocid,bodyonly
T:AlexBlogExample.Program
```

### 2. Put a date on every post

- Add a `date:` key to each post's YAML front matter; `BlogSiteFrontMatter.Date` (and core `BlogFrontMatter.Date`) bind from it.
- Posts without `Date` are skipped by `RssFeedBuilder.Build` and by `BlogSiteContentService.GetRssXmlAsync()`.
- Drafts (`isDraft: true`) are excluded from the feed regardless of date.

```yaml
title: Building a CLI, part 1
date: 2026-03-15
description: Short summary that becomes the RSS item description.
tags: [cli, dotnet]
```

### 3. Link the feed from the site head (optional, default layout already does this)

- The built-in `BlogSite` `App.razor` emits `<link rel="alternate" type="application/rss+xml" href="/rss.xml" />` whenever `EnableRss` is true.
- If you ship a custom layout, mirror that tag so feed readers can autodiscover.

### 4. Build the site

- Run `dotnet run --project <your-blog-project> -- build <baseUrl> <output>`; the static crawler fetches `/rss.xml` the same way the dev server serves it.
- The same HTTP code path runs in `dotnet run` and in `build` — no separate feed exporter.

---

## Verify

- Run `dotnet run` and open `/rss.xml` — expect a `<rss version="2.0">` document with one `<item>` per dated, non-draft post, sorted newest first.
- After `build`, confirm the generated file exists at `<output>/rss.xml`.
- Each `<item>` should carry `<title>`, `<link>` (absolute when `CanonicalBaseUrl` is set), `<guid isPermaLink="true">`, `<pubDate>` (RFC 1123), and `<description>` when front matter supplies one.

## Related

- Reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- Reference: [Built-in front-matter types](/reference/front-matter/built-in-types)
- Related reference: [Built-in BlogSite routes](/reference/blogsite/routes)
