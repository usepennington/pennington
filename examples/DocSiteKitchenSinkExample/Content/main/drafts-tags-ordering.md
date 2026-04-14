---
title: Drafts, tags, and ordering
description: Front-matter keys for hiding pages and controlling sidebar position.
tags: [authoring, drafts, tags, ordering]
sectionLabel: authoring
order: 30
uid: kitchen-sink.main.drafts-tags-ordering
---

# Drafts, tags, and ordering

Three front-matter keys shape how a page shows up in the sidebar and in
feeds:

## `isDraft`

Set `isDraft: true` to keep a page compiled into the output but hidden
from navigation, search, and llms.txt. Useful for work-in-progress pages
that should compile without being linked.

```yaml
---
title: Coming soon
isDraft: true
---
```

## `tags`

Tags are free-form strings attached to the page for client-side filtering
and future tag-index pages:

```yaml
---
title: Deep dive
tags: [advanced, performance, pipeline]
---
```

## `order`

Lower `order` values sort earlier inside a section. The sidebar's section
sort key is the minimum order of the section's children, so bumping one
page's order can reshuffle the whole section.

```yaml
---
title: Install
order: 10
---
```

This page itself carries `order: 30`, so it sits third in the authoring
section under `index.md` (`10`) and `front-matter.md` (`20`).
