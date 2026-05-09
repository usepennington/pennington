---
title: "Publish an RSS feed"
description: "Confirm /rss.xml is on, give every post a date so it appears in the channel, and point CanonicalBaseUrl at your production origin so links resolve."
uid: how-to.feeds.rss
order: 207020
sectionLabel: "Feeds & Indexes"
tags: [configuration, rss, blogsite, feeds]
---

When subscribers should be able to follow the blog from a reader, `/rss.xml` is wired by `UseBlogSite` out of the box (`BlogSiteOptions.EnableRss` defaults to `true`). The two things that most often break a working feed are missing `date:` front matter — the post is silently dropped from the channel — and an unset `CanonicalBaseUrl` — feed links emit relative URLs that aggregators cannot follow. On bare `AddPennington` without `AddBlogSite`, see <xref:tutorials.blogsite.scaffold> first; the feed endpoint ships with the BlogSite template, not with core Pennington.

## Assumptions

- A working BlogSite (see <xref:tutorials.blogsite.scaffold> if not)
- Posts under `Content/Blog/` that parse as `BlogSiteFrontMatter`
- A known production origin (such as `https://blog.example.com`). `CanonicalBaseUrl` needs the scheme and no trailing slash

---

## Options

### Confirm `EnableRss` is on

`BlogSiteOptions.EnableRss` defaults to `true`. Setting it explicitly in the options builder makes the intent visible and guards against a future default change.

The kitchen-sink example wires `EnableRss`, `CanonicalBaseUrl`, and `AuthorName` together in one place:

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

### Give every post a `date:`

`BlogSiteContentService` builds the channel from posts where `Date` is non-null, ordered by descending date. A post without `date:` renders normally at its URL but does not appear in the feed. Use ISO-8601 (`2024-01-15`) so YAML parses the value as a `DateTimeOffset`.

Minimal front matter for a post that appears in the feed:

```yaml
title: Getting started with Pennington
description: A first-post walkthrough.
date: 2024-01-15
author: Jamie Rivers
tags: [pennington, getting-started]
```

### Set `CanonicalBaseUrl` to your production origin

`RssFeedBuilder` prefixes every `<link>` and `<guid>` with the canonical base. Without it, aggregators receive relative URLs that do not resolve. Use the production scheme and host with no trailing slash, even when running locally.

### Where the feed is served and discovered

In dev mode the feed is served live by the `MapGet("/rss.xml", ...)` handler registered in `UseBlogSite`. In a static build it is written to `wwwroot/rss.xml` alongside the other generated routes. The `<link rel="alternate" type="application/rss+xml">` tag is injected into `<head>` automatically when `EnableRss` is on, so browser RSS extensions detect it without further configuration.

---

## Result

`/rss.xml` returns an RSS 2.0 channel listing every dated post, newest first, with absolute URLs:

```xml
<rss version="2.0">
  <channel>
    <title>Jamie's Blog</title>
    <link>https://blog.example.com/</link>
    <description>Notes on shipping software.</description>
    <item>
      <title>Getting started with Pennington</title>
      <link>https://blog.example.com/blog/getting-started-with-pennington/</link>
      <guid>https://blog.example.com/blog/getting-started-with-pennington/</guid>
      <pubDate>Mon, 15 Jan 2024 00:00:00 +0000</pubDate>
      <description>A first-post walkthrough.</description>
    </item>
  </channel>
</rss>
```

## Verify

- Run `dotnet run` and fetch `/rss.xml`. Expect a `<rss version="2.0">` document with one `<item>` per dated post, newest first
- Inspect one `<item>`. `<link>` and `<guid>` start with the `CanonicalBaseUrl`, not a relative `/blog/...` path
- Posts without a `date:` field are absent from the channel. When an expected post is missing, add `date:` to its front matter

## Related

- Reference: <xref:reference.api.blog-site-options>
- Reference: <xref:reference.blogsite.routes>
- Reference: <xref:reference.front-matter.keys>
