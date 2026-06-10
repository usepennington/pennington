---
title: "Publish an RSS feed"
description: "Confirm /rss.xml is on, give every post a date so it appears in the channel, and point CanonicalBaseUrl at your production origin so links resolve."
uid: how-to.feeds.rss
order: 2
sectionLabel: "Feeds & Indexes"
tags: [configuration, rss, blogsite, feeds]
---

`/rss.xml` is wired by `UseBlogSite` and enabled by default — `BlogSiteOptions.EnableRss` defaults to `true`. Two things break a working feed: a post missing `date:` (silently dropped from the channel), and an unset `CanonicalBaseUrl` (feed links emit relative URLs that aggregators cannot follow). The feed endpoint ships with the BlogSite template; to emit a feed from a bare `AddPennington` host or any non-blog content type, see <xref:how-to.feeds.custom-feed>.

## Before you begin
- A working BlogSite (see <xref:tutorials.blogsite.scaffold> if not)
- Posts under `Content/Blog/` that parse as `BlogSiteFrontMatter`
- A known production origin (such as `https://blog.example.com`). `CanonicalBaseUrl` needs the scheme and no trailing slash

---

## Options

### Give every post a `date:`

`BlogSiteContentService` builds the channel from posts where `Date` is non-null, ordered by descending date. A post without `date:` renders normally at its URL but does not appear in the feed. Use ISO-8601 (`2024-01-15`) so YAML parses the value as a `DateTime`.

Minimal front matter for a post that appears in the feed:

```yaml
title: Getting started with Pennington
description: A first-post walkthrough.
date: 2024-01-15
author: Jamie Rivers
tags: [pennington, getting-started]
```

### Set `CanonicalBaseUrl` to your production origin

`BlogSiteContentService` prefixes every `<link>` and `<guid>` with the canonical base. Use the production scheme and host with no trailing slash:

```csharp
new BlogSiteOptions
{
    CanonicalBaseUrl = "https://blog.example.com",
    // ...
}
```

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
- After a static build (see <xref:how-to.deployment.static-build>), `output/rss.xml` exists and carries the same dated items with absolute `CanonicalBaseUrl` links

## Related

- How-to: <xref:how-to.feeds.custom-feed>
- Reference: <xref:reference.api.blog-site-options>
- Reference: <xref:reference.blogsite.routes>
- Reference: <xref:reference.front-matter.keys>
