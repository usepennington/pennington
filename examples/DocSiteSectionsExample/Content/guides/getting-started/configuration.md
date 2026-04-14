---
title: Configure your site
description: Tour the PenningtonOptions knobs you will reach for on day one.
sectionLabel: Getting Started
order: 30
---

# Configure your site

`AddPennington` accepts a configuration callback that lets you tune the
content root, URL style, and a handful of feature toggles without leaving
`Program.cs`.

## Point at a content folder

By default Pennington reads from `Content/` next to your project. Override
the path when your content lives elsewhere in the repo or ships from an
embedded resource.

## Change the URL style

`LowercaseUrls` and `AppendTrailingSlash` control the shape of every
generated link. Pick a style early — changing it later invalidates existing
inbound links unless you pair the change with redirect front matter.

The next area — *Advanced* — covers layout overrides and the response
pipeline.
