---
title: Welcome to your Pennington site
description: The smallest Pennington host that renders Markdown with front matter.
---

> [!TIP]
> The page title comes from the `title:` in this file's front matter — Pennington
> renders it as the heading, so you don't repeat it with a `#` heading.

This page lives at `Content/index.md`. The `title` in the front matter above is
what the host renders in the page `<title>` and `<h1>`.

## What happens when you run this

1. `AddPennington` registers the content pipeline.
2. `AddMarkdownContent<DocFrontMatter>` points it at the `Content/` folder.
3. `UsePennington` wires the middleware into the request pipeline.
4. The `MapGet` catch-all walks the content service, renders the matching file,
   and returns the HTML.

Add more Markdown files under `Content/` — they'll be served at matching URLs.
Run `dotnet run -- build / output` to write static HTML to `output/`.
