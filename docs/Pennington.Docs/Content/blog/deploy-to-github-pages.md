---
title: Deploy your docs to GitHub Pages
description: A ready-made GitHub Actions workflow builds the static site and publishes it to GitHub Pages on every push to main.
author: Phil Scott
date: 2026-04-17
isDraft: false
tags:
  - announcements
  - deployment
---

Pennington's own docs site needed somewhere to live, and building locally and
copying files up isn't much of a workflow. So there's now a GitHub Actions
workflow that runs the static build and publishes to GitHub Pages on every push
to `main` — the same workflow any Pennington site can use.

## Push to main, your site updates

The workflow wires together two steps. It runs the [static
build](xref:how-to.deployment.static-build) to produce the site, then hands the
output folder to GitHub Pages:

```yaml
on:
  push:
    branches: [main]
```

Merge a pull request or push a doc fix, and the published site catches up on its
own. Because the static build emits plain HTML, GitHub Pages — which has no .NET
runtime — serves it without trouble.

One thing to set up front: GitHub Pages serves project sites from a sub-path
like `/my-repo/`. Set `CanonicalBaseUrl` so links and assets resolve correctly
under that prefix. The [GitHub Pages how-to](xref:how-to.deployment.github-pages)
has the complete workflow file and the base-URL setup.

## Smaller CSS, for free

A later follow-up wired PurgeCSS into the same workflow. Pennington generates
utility CSS on demand, and PurgeCSS trims the result against the built HTML, so
the published site ships only the classes it actually uses. The deploy step does
the trimming — nothing in your project changes.
