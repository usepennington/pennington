---
title: "Manage drafts, tags, and ordering"
description: "Hide pages with `isDraft`, group them with `tags`, and control sidebar position with `order`."
section: "content-authoring"
order: 20
tags: []
uid: how-to.content-authoring.drafts-tags-ordering
isDraft: true
search: false
llms: false
---

> **In this page.** Hiding pages with `isDraft: true`, using `tags` for grouping, and using `order` to control sidebar position within a section.
>
> **Not in this page.** Tag-index pages or custom taxonomy generation — those require a custom content service.

## When to use this

When you have an existing Pennington markdown page and want to hide it from production, group it with others, or shuffle its sidebar position. These are the three front-matter switches that cover 90% of authoring needs without touching C#.

## Assumptions

- You have an existing Pennington site with at least one markdown content source registered via `AddMarkdownContent<T>`.
- Your front-matter type implements `IFrontMatter`, plus `ITaggable` (for `tags`) and `IOrderable` (for `order`). `IsDraft` is on `IFrontMatter` itself.
- You are editing YAML front matter inside `.md` files under your content path.

To copy a working setup, see `examples/MultipleContentSourceExample` — it ships `DocsFrontMatter` with tags and order, and `examples/BeaconDocsExample` ships a `draft-page.md`.

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

---

## Steps

### 1. Hide a page with `isDraft: true`

Draft pages are skipped by build output, TOC, search index, sitemap, and RSS — they still render on `dotnet run` so you can preview them. The flag is `IFrontMatter.IsDraft` (default `false`); no capability interface is needed.

```yaml
---
title: "Upcoming Features"
description: "Features planned for v4"
isDraft: true
---
```

See `examples/BeaconDocsExample/Content/draft-page.md` for the canonical pattern.

### 2. Group pages with `tags`

Your front-matter record must implement `ITaggable` (exposes `string[] Tags`). YAML list syntax — each tag becomes an entry in `Tags`. Tags are emitted for blog-site tag pages; for doc sites they are metadata available to your layout but do not auto-generate tag-index pages.

```yaml
---
title: "Ultimate Coffee Brewing Guide"
tags:
  - coffee
  - brewing
  - guide
---
```

### 3. Control sidebar position with `order`

Your front-matter record must implement `IOrderable` (exposes `int Order`, default `int.MaxValue`). Lower numbers sort first; unset pages fall to the end by default. Prefer tidy 10 / 20 / 30 sequences so later inserts have room — avoid negative values.

```yaml
---
title: "Installation"
description: "Detailed installation instructions"
order: 20
---
```

### 4. Confirm your record carries the capabilities you need

If you only use `IsDraft`, any `IFrontMatter` works. For `tags` you need `ITaggable`; for `order` you need `IOrderable`. A record that carries all four capability interfaces:

```csharp:xmldocid
T:UserInterfaceExample.DocsFrontMatter
```

---

## Verify

- Run `dotnet run --project <your-site>` and confirm the draft page renders (dev) but does not appear in the sidebar.
- Run `dotnet run --project <your-site> -- build` and confirm the build report lists the draft under skipped pages and does not emit its HTML.
- Reorder two sibling pages and confirm the sidebar reflects the new `order` values on reload.
- Confirm `tags` appear in your layout wherever you render tag metadata.

## Related

- Reference: [Front matter keys](/reference/front-matter/keys) — `isDraft`, `tags`, `order`.
- Background: [Front-matter capabilities](/explanation/core/front-matter-capabilities)
