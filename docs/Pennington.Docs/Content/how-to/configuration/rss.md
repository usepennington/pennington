---
title: "Generate RSS feeds"
description: "Turn on /rss.xml for a BlogSite, make sure posts carry a date, and point CanonicalBaseUrl at your production origin so feed links resolve."
uid: how-to.configuration.rss
order: 202070
sectionLabel: Configuration
tags: [configuration, rss, blogsite, feeds]
---

`BlogSiteOptions.EnableRss` is `true` by default, so the `/rss.xml` endpoint is wired by `UseBlogSite` out of the box. This page covers the two things that most often break a feed: missing `date:` front matter (the post is silently dropped from the channel) and an unset `CanonicalBaseUrl` (feed links emit relative URLs that aggregators cannot follow). If you are on bare `AddPennington` without `AddBlogSite`, see <xref:tutorials.blogsite.scaffold> first — the feed endpoint ships with the BlogSite template, not with core Pennington.

## Assumptions

- You have a working BlogSite (see <xref:tutorials.blogsite.scaffold> if not)
- Your posts live under `Content/Blog/` and parse as `BlogSiteFrontMatter`
- You know your production origin (such as `https://blog.example.com`) — `CanonicalBaseUrl` must include the scheme and no trailing slash

---

## Steps

### 1. Confirm `EnableRss` is on

`BlogSiteOptions.EnableRss` defaults to `true`. Set it explicitly in your options builder to make the intent visible and guard against a future default change.

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.EnableRss
```

The kitchen-sink example wires `EnableRss`, `CanonicalBaseUrl`, and `AuthorName` together in one place:

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

### 2. Make sure every post carries a `date:`

`BlogSiteContentService` builds the channel from posts where `Date` is non-null, ordered by descending date. A post without `date:` renders normally at its URL but does not appear in the feed. Use ISO-8601 (`2024-01-15`) so YAML parses the value as a `DateTimeOffset`.

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteFrontMatter.Date
```

Minimal front matter for a post that will appear in the feed:

```yaml
title: Getting started with Pennington
description: A first-post walkthrough.
date: 2024-01-15
author: Jamie Rivers
tags: [pennington, getting-started]
```

### 3. Set `CanonicalBaseUrl` to your production origin

`RssFeedBuilder` prefixes every `<link>` and `<guid>` with the canonical base. Without it, aggregators receive relative URLs that will not resolve. Use the production scheme and host with no trailing slash, even when running locally.

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl
```

The underlying feed builder and item record:

```csharp:xmldocid
T:Pennington.Feeds.RssFeedBuilder
T:Pennington.Feeds.RssFeedItem
```

### 4. Verify the feed URL

In dev mode the feed is served live by the `MapGet("/rss.xml", ...)` handler registered in `UseBlogSite`. In a static build it is written to `wwwroot/rss.xml` alongside the other generated routes. The `<link rel="alternate" type="application/rss+xml">` tag is injected into `<head>` automatically when `EnableRss` is on, so browser RSS extensions detect it without further configuration.

---

## Verify

- Run `dotnet run` and fetch `/rss.xml` — expect a `<rss version="2.0">` document with one `<item>` per dated post, newest first
- Inspect one `<item>` — `<link>` and `<guid>` must start with your `CanonicalBaseUrl`, not a relative `/blog/...` path
- Posts without a `date:` field must be absent from the channel — if an expected post is missing, add `date:` to its front matter

## Related

- Reference: <xref:reference.options.blogsite-options>
- Reference: <xref:reference.blogsite.routes>
- Reference: <xref:reference.front-matter.keys>
