---
title: Welcome to your first Pennington site
description: The smallest Pennington host that renders a markdown page with front matter.
---

# Hello from Pennington

This page is a single markdown file in `Content/index.md`. Its `title` in the
front matter above is what the host reads out when it renders the page.

## What just happened

1. `AddPennington` registered the content pipeline.
2. `AddMarkdownContent<DocFrontMatter>` pointed Pennington at this folder.
3. `UsePennington` wired the middleware into the request pipeline.
4. A tiny `MapGet` endpoint walks the content service, renders this file, and
   returns the HTML.

Everything else you see in later tutorials builds on top of these four moves.
