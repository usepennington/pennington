---
title: About
description: Proves that adding a markdown file is enough to expose a new URL.
---

This file is `Content/about.md` and the catch-all serves it at `/about`. The
Blazor router didn't gain a new entry — `MarkdownPage.razor` matches every URL
through `@page "/{*Path}"` and asks the content pipeline whether anything on
disk corresponds to the requested path.

Rename this file to `reach-out.md` and `/reach-out` works on the next request.
The only thing routing the URL is the file's name.
