---
title: Welcome
description: The home page of a styled three-page Pennington site.
---

# Welcome to the styled site

This is the home page. It lives at `Content/index.md`, so Pennington maps it to
the site root `/`. The same three-page shape as the previous tutorial picks up
a MonorailCSS stylesheet and a handful of utility classes.

Open the page source and you will see a `<link rel="stylesheet" href="/styles.css">`
tag. That endpoint is served by `UseMonorailCss` and regenerates whenever a new
utility class appears in rendered HTML.
