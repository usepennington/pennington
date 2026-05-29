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
- Unknown keys are silently ignored (`IgnoreUnmatchedProperties`).
- Absent keys fall through to the record's `init` default.

## Example

A `DocSiteFrontMatter` page populating the most common keys:

```markdown:symbol
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

The blog-only keys (`author`, `series`, `repository`, `date`) appear in `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`.

## See also

- Related reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter)
- Related reference: [Built-in front-matter types](xref:reference.api.doc-front-matter)
- How-to: [Work with front matter](xref:how-to.pages.front-matter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
