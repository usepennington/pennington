---
title: Welcome
description: The home page of the Blazor-pages tutorial site.
---

# Welcome to the site

This page is `Content/index.md`. The browser asked for `/`; the Blazor catch-all
in `Components/Pages/MarkdownPage.razor` matched, walked the configured
`IContentService` instances to find this file, ran it through the parser and
renderer, and dropped the rendered HTML into the page's `<article>` element.

Add a second markdown file under `Content/` and its file path becomes its URL —
no router-table edit required.
