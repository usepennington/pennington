---
title: First guide page
description: A nested route so base-URL rewriting is observable on both the incoming and outgoing anchor.
section: Guides
order: 10
---

# First guide page

This page lives under `Content/guides/` so its canonical URL is
`/guides/first-page/` — a nested route that makes the effect of
`BaseUrlHtmlRewriter` easy to see on either end of a link.

## What to look for in the static output

Build the site with a sub-path:

```bash
dotnet run --project examples/SubPathDeployableExample -- build /my-sub-path
```

Open `output/guides/first-page/index.html` and look for:

- The anchor back to [the home page](/) rewritten to `/my-sub-path/`.
- The `<link rel="stylesheet" href="/styles.css">` rewritten to `/my-sub-path/styles.css`.
- Any `<script src="/_content/…">` rewritten to `/my-sub-path/_content/…`.
- The `<body>` element carrying `data-base-url="/my-sub-path"` so client-side
  navigation (the SPA island system) can reproduce the prefix.

See `/how-to/deployment/base-url` for the full walkthrough.
