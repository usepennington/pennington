---
title: Built for LLMs — llms.txt support
description: Pennington generates an llms.txt index and clean markdown sidecars for every page, so AI agents can read your docs without crawling rendered HTML.
author: Phil Scott
date: 2026-05-04
isDraft: false
tags:
  - llms-txt
  - ai-agents
---

When an LLM crawler reads your documentation, it parses rendered HTML — nav chrome,
sidebars, highlighting spans — to recover the prose underneath. The `llms.txt`
convention skips that by handing crawlers clean markdown directly. Pennington now
generates it.

## An index and a markdown copy of every page

During the static build, Pennington produces two things:

- `llms.txt` — an index of the whole site, organized by your navigation tree.
  Drop a custom `llms.txt` in your content root and it becomes the index header.
- a co-located markdown copy of every page at `<page>/index.md`, beside its
  `index.html` — a front-matter-stripped copy an agent reaches by appending
  `index.md` to the page URL.

The markdown is real markdown, not a degraded HTML dump. The converter handles
highlighted code blocks, tabbed groups, and GFM alerts, so a code sample
survives the round trip intact. The [llms.txt how-to](xref:how-to.feeds.llms-txt)
covers the setup.

## Agents find it on their own

Generating the files isn't enough if nothing points to them. DocSite and
BlogSite emit a `<link rel="alternate" type="text/markdown">` tag in every
content page's head, pointing at that page's co-located `index.md` — the
standard way to advertise an alternate representation. And because tools like
WebFetch strip `<head>` before an agent sees it, Pennington also drops a paired
hidden hint at the top of the `<body>`, so an agent reading the page the hard
way still learns there's a clean copy.

## Content that's only for agents

Two opt-in conventions let you write content for `llms.txt` without producing an
HTML page. The first is a `*.llms.md` file. The second is `WithLlmsTxtEntry` on a
`MapGet` endpoint, for serving the markdown dynamically:

```csharp
app.MapGet("/agents/architecture.md", () => Results.Text(overview, "text/markdown"))
   .WithLlmsTxtEntry("Architecture overview", "A high-level map for agents.");
```

Useful for an agent-oriented summary, or machine-readable detail you'd rather
keep off the human-facing site.
