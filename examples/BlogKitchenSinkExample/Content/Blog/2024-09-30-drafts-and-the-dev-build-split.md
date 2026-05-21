---
title: Drafts, scheduled posts, and the dev/build split
description: How isDraft and a future date: both hide a post at build time while keeping it previewable in dev.
date: 2024-09-30
author: Jamie Rivers
tags:
  - pennington
  - drafts
  - publishing
series: Pennington Field Notes
sectionLabel: field-notes
---

Set `isDraft: true` and the post renders in dev so you can preview it, but
falls out of the build so it doesn't ship. That covers most authoring flows
— you draft, you preview, you flip the flag, you commit.

## Embargoed publishing

For "this post goes live at 9am on Friday," set `date:` to that moment. The
build skips any post whose `date:` is in the future, exactly the same way it
skips `isDraft: true`. The dev server still renders it, so you can preview
right up to the wall-clock crossing — no flag flip, no manual gate. Whatever
hourly (or per-merge) CI you already run picks the post up on the first build
after the date passes.
