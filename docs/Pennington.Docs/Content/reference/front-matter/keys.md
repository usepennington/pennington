---
title: "Front matter key reference"
description: "Every built-in front matter key — type, default, and applicable front-matter type."
section: "front-matter"
order: 10
tags: []
uid: reference.front-matter.keys
isDraft: true
search: false
llms: false
---

> **In this page.** Every built-in key — `title`, `description`, `isDraft`, `search`, `llms`, `uid`, `date`, `tags`, `section`, `order`, `redirectUrl`, `author`, `series`, `repository` — with type, default, and applicable front-matter type.
>
> **Not in this page.** Authoring practice (see How-Tos).

## Summary

- _One sentence: what it is._ The catalog of YAML keys recognized by Pennington's built-in front-matter types.
- _One sentence: where it lives._ Namespace `Pennington.FrontMatter` (core `IFrontMatter` + capability interfaces), with `DocFrontMatter`, `BlogFrontMatter`, `Pennington.DocSite.DocSiteFrontMatter`, and `Pennington.BlogSite.BlogSiteFrontMatter` as the built-in record implementations.

## Declaration — built-in front-matter types

- `T:Pennington.FrontMatter.IFrontMatter` — universal contract (default members).
- `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.IRedirectable`, `T:Pennington.FrontMatter.ISectionable`, `T:Pennington.FrontMatter.IOrderable` — capability interfaces.
- `T:Pennington.FrontMatter.DocFrontMatter` — implements `IFrontMatter, ITaggable, ISectionable, IOrderable`.
- `T:Pennington.FrontMatter.BlogFrontMatter` — implements `IFrontMatter, ITaggable`.
- `T:Pennington.DocSite.DocSiteFrontMatter` — implements `IFrontMatter, ITaggable, ISectionable, IOrderable, IRedirectable`.
- `T:Pennington.BlogSite.BlogSiteFrontMatter` — implements `IFrontMatter, ITaggable, ISectionable, IRedirectable`; wired by `AddBlogSite`.

## Key table (alphabetical)

_Applicable types abbreviations: **IFM** = `IFrontMatter` (all implementers inherit defaults); **Doc** = `DocFrontMatter`; **Blog** = `BlogFrontMatter`; **DocSite** = `DocSiteFrontMatter`; **BlogSite** = `BlogSiteFrontMatter`. "Capability" names the interface that contributes the key._

| Key | Type | Default | Applicable types | Description |
|---|---|---|---|---|
| `author` | `string` / `string?` | `""` (BlogSite) / `null` (Blog) | Blog, BlogSite | Author name for the post; declared on `BlogFrontMatter` and `BlogSiteFrontMatter`. |
| `date` | `DateTime?` | `null` | IFM (default member), Blog, BlogSite (declared) | Publication date; materialized as a parsed property on both blog records. Not declared on `DocFrontMatter` or `DocSiteFrontMatter`. |
| `description` | `string?` | `null` | IFM (default member), Doc, Blog, DocSite, BlogSite | One-line summary used for social metadata, search snippets, and RSS `<description>`. |
| `isDraft` | `bool` | `false` | IFM (default member), Doc, Blog, DocSite, BlogSite | When `true`, the page is excluded from generated output by `ContentPipeline.GenerateAsync`. |
| `llms` | `bool` | `true` | IFM (default member), Doc, Blog, DocSite, BlogSite | When `false`, the page is excluded from `llms.txt` and the stripped-markdown sidecar tree. |
| `order` | `int` | `int.MaxValue` | Doc, DocSite (via `IOrderable`) | Sort key within a section; used by `NavigationBuilder` to order navigation and previous/next links. Not present on `BlogFrontMatter` or `BlogSiteFrontMatter`. |
| `redirectUrl` | `string?` | `null` | DocSite, BlogSite (via `IRedirectable`) | Target URL for a redirect page. Not present on `DocFrontMatter` or `BlogFrontMatter`. |
| `repository` | `string` | `""` | BlogSite | Source-repository URL for a post. When non-empty, `Pennington.BlogSite.Components.Layout.BlogPost` renders a "source repository" link card on the rendered post. Template-only key. |
| `search` | `bool` | `true` | IFM (default member), Doc, Blog, DocSite, BlogSite | When `false`, the page is excluded from the per-locale search index. |
| `section` | `string?` | `null` | Doc, DocSite, BlogSite (via `ISectionable`) | Navigation section slug; groups pages in `NavigationBuilder` output. Not present on `BlogFrontMatter`. |
| `series` | `string?` / `string` | `null` (Blog) / `""` (BlogSite) | Blog, BlogSite | Series identifier linking related posts. |
| `tags` | `string[]` | `[]` | Doc, Blog, DocSite, BlogSite (via `ITaggable`) | Taxonomy tags; consumed by tag-index pages and `RenderedContent.Tags`. |
| `title` | `string` | — (required) / `"Empty title"` (BlogSite) | IFM (required), Doc, Blog, DocSite, BlogSite | Page title; the only member required by `IFrontMatter`. `BlogSiteFrontMatter` supplies `"Empty title"` as a safety default so malformed posts still parse. |
| `uid` | `string?` | `null` | IFM (default member), Doc, Blog, DocSite, BlogSite | Cross-reference identifier resolved by `XrefResolver` for `<xref:uid>` and `href="xref:uid"` links. |

## Per-type key availability

| Key | `DocFrontMatter` | `BlogFrontMatter` | `DocSiteFrontMatter` | `BlogSiteFrontMatter` |
|---|---|---|---|---|
| `title` | yes | yes | yes | yes |
| `description` | yes | yes | yes | yes |
| `isDraft` | yes | yes | yes | yes |
| `search` | yes | yes | yes | yes |
| `llms` | yes | yes | yes | yes |
| `uid` | yes | yes | yes | yes |
| `date` | no (inherits `IFrontMatter` default `null`) | yes (declared) | no (inherits `IFrontMatter` default `null`) | yes (declared) |
| `tags` | yes | yes | yes | yes |
| `section` | yes | no | yes | yes |
| `order` | yes | no | yes | no |
| `redirectUrl` | no | no | yes | yes |
| `author` | no | yes | no | yes |
| `series` | no | yes | no | yes |
| `repository` | no | no | no | yes |

## YAML parsing notes

- Keys are matched using `YamlDotNet`'s `CamelCaseNamingConvention` via `FrontMatterParser` (`src/Pennington/FrontMatter/FrontMatterParser.cs`).
- `SafeYamlParser` tolerates malformed input by swallowing parse errors.
- Default-member keys (`isDraft`, `search`, `llms`, `uid`, `description`, `date`) only surface on a record when the record declares a property for them — otherwise the `IFrontMatter` default applies.

## See also

- Related reference: [Built-in front-matter types](/reference/front-matter/built-in-types)
- Related reference: [`IFrontMatter` and capability defaults](/reference/front-matter/ifrontmatter)
- How-to: [Work with front matter](/how-to/content-authoring/front-matter)
- Background: [The front-matter capability system](/explanation/core/front-matter-capabilities)
