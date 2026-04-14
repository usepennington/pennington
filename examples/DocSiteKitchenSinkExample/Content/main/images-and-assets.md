---
title: Images and assets
description: Two ways to place static assets next to content.
tags: [authoring, assets]
sectionLabel: authoring
order: 50
uid: kitchen-sink.main.images-and-assets
---

# Images and assets

Pennington serves static files from two places at once:

1. Anything under `wwwroot/` is served at the matching URL.
2. Anything next to a markdown file inside `Content/` is served at the
   same URL as the content tree.

## Shared assets in `wwwroot/`

Drop a file into `wwwroot/shared.png` to reach it at `/shared.png`.
These files are appropriate for shared images referenced from more than
one page — site logos, cover photos, and fonts.

![A shared asset from wwwroot](/shared.png)

## Colocated assets in `Content/`

Drop a file into `Content/main/assets/colocated.png` and reference it
with a relative path from the markdown file. This keeps the image and
the page together — moving the folder moves both.

![A colocated asset](./assets/colocated.png)

Both files land in the published `output/` directory, each at its
original URL.
