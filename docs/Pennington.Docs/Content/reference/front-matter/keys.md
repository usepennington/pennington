---
title: "Front matter key reference"
description: "Every built-in YAML front-matter key recognized by the shipped IFrontMatter implementations, with type, default, source interface, and applicable front-matter record."
sectionLabel: "Front Matter"
order: 402010
tags: [front-matter, yaml, keys, reference]
uid: reference.front-matter.keys
---

The flat catalog of YAML keys parsed into the four shipped `IFrontMatter` records — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` — via `FrontMatterParser` with `CamelCaseNamingConvention`. Keys are declared as `init`-only properties on records in `Pennington.FrontMatter` (core), `Pennington.DocSite`, and `Pennington.BlogSite`.

The base `IFrontMatter` interface every front-matter record implements supplies default member values (`IsDraft = false`, `Search = true`, `Llms = true`, `Uid = null`, `Description = null`, `Date = null`) so records only declare what they parse. See <xref:reference.front-matter.ifrontmatter> for the interface surface.

## Keys

Rows are ordered alphabetically by YAML key. The "Applies to" column lists the built-in `IFrontMatter` records that expose the key (**all** indicates universal coverage via `IFrontMatter` or a capability interface). The "Source interface" column names the declaring interface — `IFrontMatter`, one of the four capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`), or record-local for keys declared directly on a concrete record.

| YAML key | CLR property | Type | Default | Applies to | Source interface | Description |
|---|---|---|---|---|---|---|
| `author` | `Author` | `string` / `string?` | `""` (BlogSite) / `null` (Blog) | `BlogFrontMatter`, `BlogSiteFrontMatter` | record-local | Byline rendered on the post header, the `/archive` card, the RSS `<author>` element, and JSON-LD metadata; when it matches `BlogSiteOptions.AuthorName` the blog chrome surfaces the configured `AuthorBio`. |
| `date` | `Date` | `DateTime?` | `null` | **all** (default member) | `IFrontMatter` | Publication timestamp used for RSS `<pubDate>`, archive sort order, sitemap `<lastmod>`, and JSON-LD `datePublished`; posts missing a `date` are excluded from the RSS feed. |
| `description` | `Description` | `string?` | `null` | **all** (default member) | `IFrontMatter` | Short prose summary emitted as the `<meta name="description">` tag, Open Graph / Twitter card copy, RSS item description, and archive card subtitle. |
| `isDraft` | `IsDraft` | `bool` | `false` | **all** (default member) | `IFrontMatter` | When `true` the page is compiled into the output (so `xref:` links still resolve) but omitted from the sidebar, search index, `llms.txt`, and sitemap via `ContentPipeline.GenerateAsync`. |
| `llms` | `Llms` | `bool` | `true` | **all** (default member) | `IFrontMatter` | Opt-out flag for `LlmsTxtService` — when `false` the page is skipped by `llms.txt` generation and its stripped-markdown sidecar is not emitted. |
| `order` | `Order` | `int` | `int.MaxValue` | `DocFrontMatter`, `DocSiteFrontMatter` | `IOrderable` | Sort key used by `NavigationBuilder` to position the page inside its section; lower values sort earlier, and a section's own sort key is the minimum `order` among its children. |
| `redirectUrl` | `RedirectUrl` | `string?` | `null` | `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `IRedirectable` | Destination URL for a meta-refresh stub page marked `noindex`; when set, the page renders as a redirect shell instead of normal content. |
| `repository` | `Repository` | `string` | `""` | `BlogSiteFrontMatter` | record-local | External URL rendered as a "source repository" link card on the blog post layout (`Components/Layout/BlogPost.razor`); empty values suppress the card. |
| `search` | `Search` | `bool` | `true` | **all** (default member) | `IFrontMatter` | Opt-out flag for `SearchIndexService` — when `false` the page is excluded from the per-locale `search-index-{code}.json` output. |
| `sectionLabel` | `SectionLabel` | `string?` | `null` | `DocFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `ISectionable` | Display label surfaced on breadcrumbs and prev/next navigation; does **not** drive sidebar grouping — the subfolder under an area drives grouping (see `NavigationBuilder`). |
| `series` | `Series` | `string` / `string?` | `""` (BlogSite) / `null` (Blog) | `BlogFrontMatter`, `BlogSiteFrontMatter` | record-local | Groups related posts under a named series banner; posts sharing a non-empty `series` render a cross-links strip in the blog chrome. |
| `tags` | `Tags` | `string[]` | `[]` | `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `ITaggable` | Free-form string array flowing through `RenderedContent.Tags`; `BlogSiteContentService` emits a `/tags/<tag>/` index page per distinct tag, while DocSite surfaces tags only as metadata. |
| `title` | `Title` | `string` | `""` (Doc/DocSite) / `"Empty title"` (BlogSite) | **all** (required) | `IFrontMatter` | The only required front-matter key; used for the `<title>` tag, sidebar label, breadcrumb leaf, JSON-LD `headline`, and social-card title. |
| `uid` | `Uid` | `string?` | `null` | **all** (default member) | `IFrontMatter` | Stable cross-reference identifier consumed by `XrefResolver`; `[text](xref:my.uid)` and `<xref:my.uid>` both resolve through this key. |

### Notes

- YAML keys are matched case-insensitively under `CamelCaseNamingConvention` (`src/Pennington/FrontMatter/FrontMatterParser.cs`); unknown keys are silently ignored (`IgnoreUnmatchedProperties`).
- Absent keys fall through to the record's `init` default; `IFrontMatter` default members (`IsDraft`, `Search`, `Llms`, `Uid`, `Description`, `Date`) apply when the implementing record does not declare the property itself.
- The concrete records `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter` all re-declare every default-member key explicitly, so their defaults are the ones in this table (not the interface defaults) when parsing is wired to a specific record type.

## Example

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

A `DocSiteFrontMatter` page populating `title`, `description`, `tags`, `sectionLabel`, `order`, and `uid`; the blog-only keys (`author`, `series`, `repository`, `date`) are demonstrated in `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`.

## See also

- Related reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter)
- Related reference: [Built-in front-matter types](xref:reference.front-matter.built-in-types)
- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
