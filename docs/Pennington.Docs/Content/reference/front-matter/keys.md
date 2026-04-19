---
title: "Front matter key reference"
description: "Every built-in YAML front-matter key recognized by the shipped IFrontMatter implementations, with type, default, source interface, and applicable front-matter record."
sectionLabel: "Front Matter"
order: 402010
tags: [front-matter, yaml, keys, reference]
uid: reference.front-matter.keys
---

The flat catalog of YAML keys parsed into the four shipped `IFrontMatter` records — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` — via `FrontMatterParser` with `CamelCaseNamingConvention`. Keys are declared as `init`-only properties on records in `Pennington.FrontMatter` (core), `Pennington.DocSite`, and `Pennington.BlogSite`.

The base `IFrontMatter` interface every front-matter record implements supplies default member values (`IsDraft = false`, `Search = true`, `Llms = true`, `Uid = null`, `Description = null`, `Date = null`) so records only declare what they parse. See <xref:reference.api.i-front-matter> for the interface surface.

## Keys

Rows are alphabetical by YAML key. Each entry shows the records that expose the key, the declaring interface (`IFrontMatter`, one of the capability interfaces, or `record-local` for concrete-record properties), and every distinct type and default across records.

<FrontMatterKeys />

### Notes

- YAML keys are matched case-insensitively under `CamelCaseNamingConvention` (`src/Pennington/FrontMatter/FrontMatterParser.cs`); unknown keys are silently ignored (`IgnoreUnmatchedProperties`).
- Absent keys fall through to the record's `init` default; `IFrontMatter` default members (`IsDraft`, `Search`, `Llms`, `Uid`, `Description`, `Date`) apply when the implementing record does not declare the property itself.
- The concrete records `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter` all re-declare every default-member key explicitly, so their defaults are the ones listed above (not the interface defaults) when parsing is wired to a specific record type.

## Example

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

A `DocSiteFrontMatter` page populating `title`, `description`, `tags`, `sectionLabel`, `order`, and `uid`; the blog-only keys (`author`, `series`, `repository`, `date`) are demonstrated in `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`.

## See also

- Related reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter)
- Related reference: [Built-in front-matter types](xref:reference.api.doc-front-matter)
- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
