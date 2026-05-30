---
title: Customize the sidebar
description: Shape the generated sidebar through folder structure and front-matter keys.
tags: [authoring, navigation]
sectionLabel: authoring
order: 40
uid: kitchen-sink.main.customize-sidebar
---

The DocSite sidebar is built from the folder tree under each area plus
the `order:` and `section:` front-matter keys on each page.

## What drives grouping

Subfolders under an area become sidebar sections automatically —
`Content/main/widgets/` renders as a "Widgets" group. Folder names are
converted from kebab-case to title case (`getting-started` → "Getting
Started").

## What drives order

Lower `order:` sorts earlier. Two pages tied on order fall back to
alphabetical order on `title`. A section's aggregate sort key is the
**minimum** `order` value of its direct children — so promoting one page
to `order: 10` pulls the whole section up.

## Hiding a page from the sidebar

Three flags each hide a page from different surfaces:

- `isDraft: true` — hides from sidebar, search, and llms.txt
- `search: false` — hides from the search index only
- `llms: false` — hides from llms.txt only

A redirect page (one with a `redirectUrl:` key set) never appears in the
sidebar regardless of other settings — the engine treats it as a
transport hop, not content.
