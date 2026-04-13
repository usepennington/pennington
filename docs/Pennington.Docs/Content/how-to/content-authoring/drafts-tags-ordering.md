---
title: "Manage drafts, tags, and ordering"
description: "Hide pages with isDraft, group them with tags, and control sidebar position with order."
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

- Outline (not prose): the reader has an existing Pennington markdown page and wants to hide, group, or reorder it
- Aimed at authors who already have front matter working and just need the three switches
- Not a concept tour of the front-matter capability model — that lives in the explanation quadrant

## Assumptions

- Bullets (3-8):
- You have an existing Pennington site with at least one markdown content source registered via `AddMarkdownContent<TFrontMatter>`
- Your front matter type implements `IFrontMatter`, and additionally `ITaggable` (for `tags`) and `IOrderable` (for `order`) — `IsDraft` is on `IFrontMatter` itself
- You are editing YAML front matter inside `.md` files under your content path
- To copy a working setup, see `examples/MultipleContentSourceExample/` — it ships `DocsFrontMatter` with tags + order and `BeaconDocsExample/` ships a `draft-page.md`

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

- Reference snippet: `MultipleContentSourceExample` — front matter record implementing `ITaggable` + `IOrderable` + `IRedirectable`, the minimal shape required to use all three levers on this page

---

## Steps

### 1. Hide a page with `isDraft: true`

- One-line rationale: draft pages are skipped by build output, TOC, search index, sitemap, and RSS — they still render on `dotnet run` so you can preview them
- The flag is `IFrontMatter.IsDraft` (default `false`); no capability interface needed
- Use a plain YAML fence — this is not a C# symbol

```yaml
---
title: "Upcoming Features"
description: "Features planned for v4"
isDraft: true
---
```

- Reference snippet: `examples/BeaconDocsExample/Content/draft-page.md` — canonical draft page pattern verified in the examples inventory

### 2. Group pages with `tags`

- Your front matter record must implement `ITaggable` (exposes `string[] Tags`)
- YAML list syntax — each tag becomes an entry in `Tags`
- Tags surface on `RenderedContent.Tags` and are emitted for blog-site tag pages; for doc sites they are metadata available to your layout but do **not** auto-generate tag-index pages (see "Not in this page")

```yaml
---
title: "Ultimate Coffee Brewing Guide"
tags:
  - coffee
  - brewing
  - guide
---
```

- Reference snippet: `examples/MultipleContentSourceExample/Content/docs/coffee-brewing-guide.md` — three-tag front matter verified against `DocsFrontMatter`

### 3. Control sidebar position with `order`

- Your front matter record must implement `IOrderable` (exposes `int Order`, default `int.MaxValue`)
- Lower numbers sort first; unset pages fall to the end by the default
- `NavigationBuilder.BuildTree` respects `Order` when composing the sidebar; `MarkdownContentService` reads `IOrderable` when producing `ContentTocItem` entries
- Prefer tidy 10 / 20 / 30 sequences so later inserts have room; avoid negative values

```yaml
---
title: "Installation"
description: "Detailed installation instructions"
order: 20
---
```

- Reference snippet: `examples/BeaconDocsExample/Content/getting-started/install.md` — uses `order: 20` inside a sectioned doc tree

### 4. (Optional) Confirm your front matter type can hold all three

- If you only use `IsDraft`, any `IFrontMatter` works
- For `tags` you need `ITaggable`; for `order` you need `IOrderable`
- An alternative shape that carries all four capability interfaces:

```csharp:xmldocid
T:UserInterfaceExample.DocsFrontMatter
```

- Reference snippet: `UserInterfaceExample` — shows a front-matter record implementing `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` together

---

## Verify

- Run `dotnet run --project <your-site>` and confirm the draft page renders (dev) but does not appear in the sidebar
- Run `dotnet run --project <your-site> -- build` and confirm the build report lists the draft under skipped pages and does not emit its HTML into `output/`
- Reorder two sibling pages and confirm the sidebar reflects the new `order` values on reload
- Confirm `tags` appear in your layout wherever you render `RenderedContent.Tags` (tag-index pages are out of scope — see "Not in this page")

## Related

- Reference: Front matter capabilities (`IFrontMatter`, `ITaggable`, `IOrderable`, `ISectionable`) — `src/Pennington/FrontMatter/Capabilities.cs`
- Background: Explanation of the front-matter capability model and why drafts are a universal default member
