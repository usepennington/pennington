---
title: Drafts, scheduled posts, and the dev/build split
description: How isDraft behaves across dotnet run and dotnet run -- build, and the gap that scheduled posts still leave.
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

## What it doesn't cover

True embargoed publishing — "this post goes live at 9am on Friday" — needs
a `publishDate` cutoff the build can compare against. Today that's a manual
flip; treat it as a one-line cron job in CI until the framework grows the
primitive.
