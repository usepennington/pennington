---
title: Welcome
description: The home page of a styled Pennington site.
---

# Welcome to the styled site

This is the home page. It still lives at `Content/index.md` and the Blazor
catch-all from the previous tutorial still serves it — what changed is that
every response now flows through a styled `MainLayout.razor` whose utility
classes the MonorailCSS class collector turns into real CSS rules.

Open the page source. The `<link rel="stylesheet" href="/styles.css">` tag in
`<head>` points at the endpoint `app.UseMonorailCss()` mounts. That stylesheet
regenerates whenever a new utility class appears in rendered HTML.
