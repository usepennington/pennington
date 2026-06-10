---
title: "Front matter key reference"
description: "Every built-in YAML front-matter key recognized by the shipped IFrontMatter implementations, with type, default, source interface, and applicable front-matter record."
sectionLabel: "Front Matter"
order: 1
tags: [front-matter, yaml, keys, reference]
uid: reference.front-matter.keys
---

YAML keys parsed into the five shipped `IFrontMatter` records — `DocFrontMatter`, `BlogFrontMatter`, `BlogPostFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`. See <xref:reference.api.i-front-matter> for the interface surface and <xref:explanation.core.front-matter-capabilities> for the design rationale.

## Keys

Rows are alphabetical by YAML key. Each entry shows the records that expose the key, the declaring interface (`IFrontMatter`, one of the capability interfaces, or `record-local`), and every distinct type and default across records.

<FrontMatterKeys />

### Parse rules

- YAML keys are the camelCase form of the C# property names. Matching is case-insensitive.
- Unknown keys are dropped with a warning diagnostic in lenient mode (the default outside build); in strict mode (the build default) they throw a `YamlException` and fail the parse.
- Lenient versus strict is controlled by `PenningtonOptions.FrontMatter.StrictUnknownKeys`, settable in the `AddPennington(options => …)` callback. It defaults to `false` (lenient), and `-- build` flips it to `true` unless the host has already set it. `diag frontmatter` prints the active value.
- Absent keys fall through to the record's `init` default.

### Drafts and scheduled pages

`isDraft: true` excludes a page from build output entirely: `-- build` skips the route, so it is never written, never crawled, and its `uid` does not resolve in the static site. Development requests still render it so authors can preview. A `date:` set after the build clock has the same effect until the clock catches up (scheduled publishing).

This is the canonical statement of the rule. `isDraft` is a build switch, not a navigation switch — to keep a page published but out of the sidebar, use `searchOnly: true` instead, which leaves the route in the build and indexes while hiding it from the rendered navigation tree.

## Example

A `DocSiteFrontMatter` page populating the most common keys:

```markdown:symbol
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

A `BlogSiteFrontMatter` page populating the blog-only keys (`author`, `series`, `repository`, `date`):

```markdown:symbol
examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md
```

## See also

- Related reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter)
- Related reference: [Built-in front-matter types](xref:reference.api.doc-front-matter)
- How-to: [Work with front matter](xref:how-to.pages.front-matter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
