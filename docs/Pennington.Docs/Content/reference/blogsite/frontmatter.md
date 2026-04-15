---
title: "BlogSiteFrontMatter"
description: "Field-by-field reference for the BlogSiteFrontMatter record used by Pennington.BlogSite posts."
sectionLabel: "BlogSite Built-ins"
order: 408015
tags: [blogsite, front-matter, yaml]
uid: reference.blogsite.frontmatter
---

`BlogSiteFrontMatter` is the front-matter record `Pennington.BlogSite` binds every post to. It lives in namespace `Pennington.BlogSite` and implements `IFrontMatter`, `ITaggable`, `ISectionable`, and `IRedirectable`, so posts participate in navigation, tagging, section grouping, and redirect rules alongside regular Pennington content.

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteFrontMatter
```

## Fields

| Key | Type | Default | Description |
|---|---|---|---|
| `title` | `string` | `"Empty title"` | Post title; required by `IFrontMatter` and surfaced in the home card, archive card, RSS `<title>`, and the rendered `<h1>` for default layouts. |
| `author` | `string` | `""` | Author byline rendered in the post header and used in JSON-LD `Article` payloads. |
| `description` | `string?` | `null` | Short blurb used by the home and archive cards, RSS `<description>`, and OpenGraph metadata. |
| `repository` | `string` | `""` | Optional repository URL rendered as a "View source" link on the post layout. |
| `date` | `DateTime?` | `null` | Publish date (ISO-8601 in YAML). Drives archive sort order and the RSS `<pubDate>` element. |
| `isDraft` | `bool` | `false` | Excludes the post from home, archive, tag pages, and the RSS feed when `true`. |
| `tags` | `string[]` | `[]` | Tag list for `/tags` index, per-tag pages, and the RSS `<category>` entries. |
| `series` | `string` | `""` | Series label used by templated layouts that group posts under a shared heading. |
| `redirectUrl` | `string?` | `null` | When set, the post URL serves a redirect to this target instead of rendering content (`IRedirectable`). |
| `sectionLabel` | `string?` | `null` | Section grouping key consumed by `ISectionable`-aware navigation layouts. |
| `uid` | `string?` | `null` | Stable cross-reference identifier; when set, the post registers with `XrefResolver` and can be linked via `<xref:uid>` or `[text](xref:uid)`. |
| `search` | `bool` | `true` | Include the post in the generated search index. |
| `llms` | `bool` | `true` | Include the post in the generated `llms.txt` catalogue and per-page markdown sidecars. |

## Example

```yaml
---
title: "Shipping a tiny content engine for weekend projects"
description: "Why I built Pennington and what you can do with it in an afternoon."
author: "Phil Scott"
date: 2026-04-10
tags: [dotnet, content, static-site]
series: "Building Pennington"
uid: blog.shipping-pennington
---
```

## See also

- Reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- Reference: [`BlogSiteOptions`](xref:reference.options.blogsite-options)
- Reference: [Core `IFrontMatter` contract](xref:reference.front-matter.ifrontmatter)
- Tutorial: [Write your first post](xref:tutorials.blogsite.first-post)
