---
title: About
description: Who made this example and why.
order: 20
---

# About this site

This tiny site has three markdown files under `Content/`. Each one exposes a
`title:` in its front matter — the only key Pennington truly requires — and
each one becomes a URL built from its file path.

- `Content/index.md` serves `/`
- `Content/about.md` serves `/about`
- `Content/contact.md` serves `/contact`

No routing configuration was added to `Program.cs` between the second and third
page. The content pipeline walks the folder, reads the front matter, and the
navigation strip fills in on its own.
