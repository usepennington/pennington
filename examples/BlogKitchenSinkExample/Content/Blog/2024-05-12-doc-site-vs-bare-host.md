---
title: AddDocSite, AddBlogSite, or AddPennington — picking the right ceiling
description: The three Pennington wiring choices, and the moment each one stops being the right fit.
date: 2024-05-12
author: Jamie Rivers
tags:
  - pennington
  - docsite
  - blogsite
series: Pennington Field Notes
sectionLabel: field-notes
---

`AddPennington` is the bare host — no layout, no chrome, just a markdown
pipeline. `AddDocSite` adds documentation chrome with sidebars, breadcrumbs,
and search. `AddBlogSite` adds the home page, archive, tag pages, and RSS.

## When to drop down

You stay on a template until the chrome stops being right. `AddDocSite` is
the right call until your sidebar isn't hierarchical or your front matter
needs fields the built-in `DocSiteFrontMatter` doesn't expose. At that point
drop to `AddPennington`, wire your own `IContentService`, and bring only the
pieces you need.
