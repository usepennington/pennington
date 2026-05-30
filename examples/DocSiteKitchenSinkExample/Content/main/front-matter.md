---
title: Front matter
description: The YAML block at the top of every markdown page.
tags: [authoring, front-matter]
sectionLabel: authoring
order: 20
uid: kitchen-sink.main.front-matter
---

Every page in this site opens with a YAML block between `---` markers.
Those keys drive the sidebar title, description, tags, ordering, draft
state, and cross-reference `uid`. Each built-in front-matter record maps
the same keys onto a strongly-typed record.

## The built-in DocSite record

The DocSite template uses `DocSiteFrontMatter` under the hood. Its fields
cover the full capability surface — `Title`, `Description`, `IsDraft`,
`Tags`, `Order`, `RedirectUrl`, `Section`, `Uid`, `Search`, and `Llms`.

## A custom front-matter record

When you need extra fields, declare a record implementing `IFrontMatter`
(plus any capability interfaces you want). This site ships an
`ApiFrontMatter` record used by the API area to add `Namespace` and
`Stability` fields:

```yaml
---
title: Symbol reference
namespace: Pennington.Search
stability: preview
order: 30
---
```

Declare the record alongside your host project; Pennington discovers it
by type when you call `AddMarkdownContent<ApiFrontMatter>(...)`.
