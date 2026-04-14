---
title: "Generate RSS feeds"
description: "Turn on /rss.xml for a BlogSite, make sure posts carry a date, and point CanonicalBaseUrl at your production origin so feed links resolve."
uid: how-to.configuration.rss
order: 202070
sectionLabel: Configuration
tags: [configuration, rss, blogsite, feeds]
---

> **In this page.** _Paraphrase TOC "Covers": keep `EnableRss` on for a BlogSite, confirm every post has a `date:` front-matter field, set `CanonicalBaseUrl` so `<link>`/`<guid>` resolve to absolute URLs, and find the generated feed at `/rss.xml`. One or two sentences, no preamble about what RSS is._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": JSON Feed, Atom format selection, and multi-source feed merging (Pennington only ships RSS 2.0 for BlogSite posts). One sentence — link out only if a neighbouring page lands later._

## When to use this

_Two to three sentences. The reader already has a BlogSite rendering posts and now wants the feed surface reachable at `/rss.xml` for readers and aggregators. Name the realistic state: `EnableRss = true` is already the default, so this page is mostly about the two things that go wrong — missing `date:` front matter (post is skipped from the channel) and an unset `CanonicalBaseUrl` (feed links emit relative URLs that break in aggregators). If the reader is on bare `AddPennington` without `AddBlogSite`, point them at [_Scaffold a BlogSite in one command_](xref:tutorials.blogsite.scaffold) — the `/rss.xml` endpoint is wired by `UseBlogSite`, not by core Pennington._

## Assumptions

_Keep to 3 bullets. The non-obvious one is the BlogSite-only scope — the `/rss.xml` endpoint ships with `Pennington.BlogSite`, not with bare `AddPennington`._

- You have a working BlogSite (see [_Scaffold a BlogSite in one command_](xref:tutorials.blogsite.scaffold) if not)
- Your posts live under `Content/Blog/` and parse as `BlogSiteFrontMatter`
- You know your production origin (e.g. `https://blog.example.com`) — `CanonicalBaseUrl` must match it exactly, with scheme and no trailing slash

To copy a working setup, see [`examples/BlogKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogKitchenSinkExample) — specifically `ServiceConfiguration.BuildBlogSiteOptions`, which sets `EnableRss = true`, `CanonicalBaseUrl`, and `AuthorName` in one place. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Step 1 confirms the toggle. Step 2 is the post-side checklist (`date:` is required, missing dates drop posts from the channel). Step 3 is `CanonicalBaseUrl` (the most common breakage). Step 4 points the reader at the generated feed. Keep each step under two sentences of prose plus at most one fence._

### 1. Confirm `EnableRss` is on

_One or two sentences. `BlogSiteOptions.EnableRss` defaults to `true`, so `AddBlogSite` wires the `/rss.xml` endpoint out of the box — `UseBlogSite` registers the `MapGet` handler only when the flag is set. Set it explicitly in your options builder to make the intent visible in code review and to protect against a future default change._

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.EnableRss
```

_Show the surface the kitchen-sink example wires, with `EnableRss`, `CanonicalBaseUrl`, and `AuthorName` set in one place:_

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

### 2. Make sure every post carries a `date:`

_One or two sentences. `BlogSiteContentService` builds the channel from posts where `Date` is non-null and orders items by descending `Date`; a post missing `date:` parses fine and still renders at `/blog/<slug>/` but will not appear in the feed. Put the date in ISO-8601 (`2024-01-15`) so YAML parses it as a `DateTimeOffset`, not a string._

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteFrontMatter.Date
```

_Minimal front matter for a post that will appear in the feed:_

```yaml
title: Getting started with Pennington
description: A first-post walkthrough.
date: 2024-01-15
author: Jamie Rivers
tags: [pennington, getting-started]
```

### 3. Set `CanonicalBaseUrl` to your production origin

_One or two sentences. `RssFeedBuilder` takes the canonical base as its only constructor argument and prefixes every `<link>` and `<guid>` with it — leave `CanonicalBaseUrl` empty and aggregators receive relative URLs that will not resolve. Use the production scheme+host with no trailing slash, even while running `dotnet run` locally._

```csharp:xmldocid
P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl
```

_For reference, the core feed builder and its item record:_

```csharp:xmldocid
T:Pennington.Feeds.RssFeedBuilder
T:Pennington.Feeds.RssFeedItem
```

### 4. Find the generated feed

_One or two sentences. In dev mode the feed is served live by the `MapGet("/rss.xml", ...)` handler in `UseBlogSite`; in a static build it is written to `wwwroot/rss.xml` alongside the other crawled routes. The `<link rel="alternate" type="application/rss+xml">` tag is already injected into `<head>` by `App.razor` when `EnableRss` is on, so browser RSS extensions pick it up automatically._

---

## Verify

_Three terse bullets — one per thing that commonly breaks. The reader should confirm each without reading anything else._

- Run `dotnet run` and fetch `/rss.xml` — expect a `<rss version="2.0">` document with one `<item>` per dated post, newest first
- Inspect one `<item>` — expect `<link>` and `<guid>` to start with your `CanonicalBaseUrl` (not a relative `/blog/...` path)
- Expect posts without a `date:` field to be absent from the channel — if one is missing that you expected to see, add `date:` to its front matter

## Related

_Three cross-quadrant links. Reference for the full options surface, reference for the route catalog (so the reader can audit every BlogSite endpoint at once), and the front-matter key reference for the exact `date:` contract. Do not link to the next how-to in this section — generated automatically._

- Reference: [_`BlogSiteOptions`_](xref:reference.options.blogsite-options)
- Reference: [_Built-in BlogSite routes_](xref:reference.blogsite.routes)
- Reference: [_Front matter key reference_](xref:reference.front-matter.keys)
