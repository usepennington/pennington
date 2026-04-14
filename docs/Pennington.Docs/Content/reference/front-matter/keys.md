---
title: "Front matter key reference"
description: "Every built-in YAML front-matter key recognized by the shipped IFrontMatter implementations, with type, default, source interface, and applicable front-matter record."
sectionLabel: "Front Matter"
order: 402010
tags: [front-matter, yaml, keys, reference]
uid: reference.front-matter.keys
---

> **In this page.** _One sentence paraphrased from `docs-toc.md` "Covers": every built-in key — `title`, `description`, `isDraft`, `search`, `llms`, `uid`, `date`, `tags`, `sectionLabel`, `order`, `redirectUrl`, `author`, `series`, `repository` — with its YAML key, CLR type, default value, source interface, and the built-in front-matter records that expose it._
>
> **Not in this page.** _One sentence paraphrased from TOC "Does not cover": authoring practice (hiding drafts, grouping with tags, picking sidebar order) lives in the How-To quadrant at `/how-to/content-authoring/*`. The capability-interface architecture and the `IFrontMatter` contract itself are documented on neighboring reference pages._

## Summary

_**One sentence: what it is.** The flat catalog of YAML keys parsed into the four shipped `IFrontMatter` records — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` — via `FrontMatterParser` with `CamelCaseNamingConvention`._
_**One sentence: where it lives.** Keys are declared as `init`-only properties on records in `Pennington.FrontMatter` (core), `Pennington.DocSite`, and `Pennington.BlogSite`; the `CamelCaseNamingConvention` on `FrontMatterParser` maps each PascalCase property to a camelCase YAML key._

_Authoring-flavored sentences (rationale, "when to set this") belong in Explanation and How-Tos, not here. This page is one big ctrl-F table._

## Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

_One sentence. The base interface every front-matter record implements; default interface members supply the opt-out values (`IsDraft = false`, `Search = true`, `Llms = true`, `Uid = null`, `Description = null`, `Date = null`) so records only declare what they actually parse._

## Keys

_The whole page is this one table. Rows ordered alphabetically by YAML key so ctrl-F lands on the row regardless of which record the reader has open. Every row: one sentence, no rationale, no steps._

_"Applies to" column lists the built-in `IFrontMatter` records that surface the key. If the key is declared on `IFrontMatter` itself (or one of the four capability interfaces that every shipped record implements), the row may list **all** to signal universal coverage — otherwise the row names the specific records (`DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`)._

_"Source interface" column names the declaring interface: `IFrontMatter` itself, or one of the four capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`), or the concrete record when the key is record-local (`author`, `series`, `repository` on blog records)._

| YAML key | CLR property | Type | Default | Applies to | Source interface | Description |
|---|---|---|---|---|---|---|
| `author` | `Author` | `string` / `string?` | `""` (BlogSite) / `null` (Blog) | `BlogFrontMatter`, `BlogSiteFrontMatter` | record-local | _One sentence. Byline rendered on the post header, the `/archive` card, the RSS `<author>` element, and JSON-LD metadata; when it matches `BlogSiteOptions.AuthorName` the blog chrome surfaces the configured `AuthorBio`._ |
| `date` | `Date` | `DateTime?` | `null` | **all** (default member) | `IFrontMatter` | _One sentence. Publication timestamp used for RSS `<pubDate>`, archive sort order, sitemap `<lastmod>`, and JSON-LD `datePublished`; posts missing a `date` are excluded from the RSS feed._ |
| `description` | `Description` | `string?` | `null` | **all** (default member) | `IFrontMatter` | _One sentence. Short prose summary emitted as the `<meta name="description">` tag, Open Graph / Twitter card copy, RSS item description, and archive card subtitle._ |
| `isDraft` | `IsDraft` | `bool` | `false` | **all** (default member) | `IFrontMatter` | _One sentence. When `true` the page is compiled into the output (so `xref:` links still resolve) but omitted from the sidebar, search index, `llms.txt`, and sitemap via `ContentPipeline.GenerateAsync`._ |
| `llms` | `Llms` | `bool` | `true` | **all** (default member) | `IFrontMatter` | _One sentence. Opt-out flag for `LlmsTxtService` — when `false` the page is skipped by `llms.txt` generation and its stripped-markdown sidecar is not emitted._ |
| `order` | `Order` | `int` | `int.MaxValue` | `DocFrontMatter`, `DocSiteFrontMatter` | `IOrderable` | _One sentence. Sort key used by `NavigationBuilder` to position the page inside its section; lower values sort earlier, and a section's own sort key is the minimum `order` among its children._ |
| `redirectUrl` | `RedirectUrl` | `string?` | `null` | `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `IRedirectable` | _One sentence. Destination URL for a meta-refresh stub page marked `noindex`; when set, the page renders as a redirect shell instead of normal content._ |
| `repository` | `Repository` | `string` | `""` | `BlogSiteFrontMatter` | record-local | _One sentence. External URL rendered as a "source repository" link card on the blog post layout (`Components/Layout/BlogPost.razor`); empty values suppress the card._ |
| `search` | `Search` | `bool` | `true` | **all** (default member) | `IFrontMatter` | _One sentence. Opt-out flag for `SearchIndexService` — when `false` the page is excluded from the per-locale `search-index-{code}.json` output._ |
| `sectionLabel` | `SectionLabel` | `string?` | `null` | `DocFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `ISectionable` | _One sentence. Display label surfaced on breadcrumbs and prev/next navigation; does **not** drive sidebar grouping — the subfolder under an area drives grouping (see `NavigationBuilder`). The YAML key is `sectionLabel`, matching the property name under `CamelCaseNamingConvention`._ |
| `series` | `Series` | `string` / `string?` | `""` (BlogSite) / `null` (Blog) | `BlogFrontMatter`, `BlogSiteFrontMatter` | record-local | _One sentence. Series banner grouping related posts together in the blog chrome; posts sharing a non-empty `series` render a cross-links strip._ |
| `tags` | `Tags` | `string[]` | `[]` | `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` | `ITaggable` | _One sentence. Free-form string array flowing through `RenderedContent.Tags`; `BlogSiteContentService` emits a `/tags/<tag>/` index page per distinct tag, while DocSite surfaces tags only as metadata._ |
| `title` | `Title` | `string` | `""` (Doc/DocSite) / `"Empty title"` (BlogSite) | **all** (required) | `IFrontMatter` | _One sentence. The only required front-matter key; used for the `<title>` tag, sidebar label, breadcrumb leaf, JSON-LD `headline`, and social-card title._ |
| `uid` | `Uid` | `string?` | `null` | **all** (default member) | `IFrontMatter` | _One sentence. Stable cross-reference identifier consumed by `XrefResolver`; `[text](xref:my.uid)` and `<xref:my.uid>` both resolve through this key._ |

_Self-check notes for the writer: every row has a YAML key in column 1, a CLR property in column 2, a type in column 3, a default in column 4, a record list in column 5, a source interface in column 6, and exactly one sentence in column 7. No row carries rationale or steps. No row spans multiple sentences except where a tiny clarifying clause is genuinely load-bearing for correctness (e.g., the `sectionLabel` note that it does not drive grouping — the feature is widely misunderstood, and the grouping sentence is the single correction that prevents miswriting)._

### Notes

_Two or three short bullets, each one sentence. These capture parser-level facts that are not per-key but are load-bearing for ctrl-F readers._

- _One sentence. YAML keys are matched case-insensitively under `CamelCaseNamingConvention` (`src/Pennington/FrontMatter/FrontMatterParser.cs`); unknown keys are silently ignored (`IgnoreUnmatchedProperties`)._
- _One sentence. Absent keys fall through to the record's `init` default; `IFrontMatter` default members (`IsDraft`, `Search`, `Llms`, `Uid`, `Description`, `Date`) apply when the implementing record does not declare the property itself._
- _One sentence. The concrete records `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter` all re-declare every default-member key explicitly, so their defaults are the ones in this table (not the interface defaults) when parsing is wired to a specific record type._

## Example

_One minimal example pulled from the `DocSiteKitchenSinkExample` fixture that already uses every DocSite-applicable key. Do not expand — this page is a catalog, not a tutorial._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

_One sentence of context. A `DocSiteFrontMatter` page populating `title`, `description`, `tags`, `sectionLabel`, `order`, and `uid`; the blog-only keys (`author`, `series`, `repository`, `date`) are demonstrated by `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`._

## See also

_Two to four cross-quadrant links. One pointer to the capability contract reference, one to the record-catalog reference, one to the authoring how-to, one to the design-rationale explanation._

- Related reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter)
- Related reference: [Built-in front-matter types](xref:reference.front-matter.built-in-types)
- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
