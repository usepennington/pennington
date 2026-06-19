---
title: Getting Started
description: Stand up a site with a folder and a pennington.toml.
order: 1
---

# Getting Started

1. Drop your markdown under `Content/`.
2. Describe the site in `pennington.toml` (set `template = "docs"`).
3. Run `pennington` to serve, or `pennington build` to generate a static site.

```bash
pennington --root=./my-docs          # serve live
pennington build --root=./my-docs    # static output/
```
