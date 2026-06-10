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
---

Welcome to the first real post on this blog. The scaffold from the
previous tutorial gave us a running BlogSite with one placeholder
post; this post replaces it with something the BlogSite template
actually has opinions about.

## What the front matter is doing

Each field in the block above lights up a different surface — the
archive card, the post header, the /tags/<tag> listings, the RSS
channel, the JSON-LD metadata — and walking through them in order
is the point of this tutorial.
