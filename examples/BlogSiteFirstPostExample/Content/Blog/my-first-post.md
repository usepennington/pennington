---
title: Shipping a tiny content engine for weekend projects
description: Notes from the first month of building Pennington — why Markdig plus Razor components plus a little YAML beats reaching for a heavier framework.
date: 2026-04-10
author: Author Name
tags:
  - pennington
  - dotnet
  - blogging
series: Pennington Field Notes
repository: https://github.com/example/pennington-field-notes
sectionLabel: field-notes
redirectUrl:
---

Welcome to the first real post on this blog. The scaffold from the previous
tutorial gave us a running BlogSite with one placeholder post; this post
replaces that placeholder with something the BlogSite template actually
has opinions about. Every key in the front-matter block above lights up a
different surface — the archive card, the post header, the `/tags/<tag>`
listings, the RSS channel, the JSON-LD metadata — and walking through them
in order is the point of this tutorial.

## What the front matter is doing

`title`, `description`, and `date` are the three fields that drive every
listing surface: the home page card, the `/archive` page, and the RSS item.
`author` flows into the same places plus the per-post byline and the RSS
`<author>` element; when it matches `BlogSiteOptions.AuthorName` the blog
defaults to the configured author bio for the post chrome.

`tags` build the `/tags/<tag>/` index pages. The three tags above mean this
post shows up under `/tags/pennington/`, `/tags/dotnet/`, and
`/tags/blogging/`. `series` lets several posts thread together under a
shared banner — a later tutorial walks through grouping posts by series.
`repository` is a hint for "view source" links; `section` groups this post
under a named slice of the archive; `redirectUrl` is left empty because
this post has no previous home elsewhere on the web.

## Why you'd bother populating all of it

You can absolutely ship a post with only `title`, `description`, and
`date` — the BlogSite will still render it. But each field you populate
turns on one more piece of chrome: RSS readers show the author, social
previews show the description, `/tags/<tag>/` listings pick up the post,
and the series banner threads together the posts that belong together.
Populating the full set once, on the first post, is how you make the rest
of the blog's defaults work for you.

The next tutorial wires up the homepage hero, the project grid, and the
social-link icons — so the scaffolded blog starts to feel like a real
personal site rather than an archive of posts.
