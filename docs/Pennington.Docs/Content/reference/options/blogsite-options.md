---
title: BlogSiteOptions
description: "Every property on BlogSiteOptions — metadata, content paths, styling, author chrome, homepage data, feature toggles, and integration hooks — plus the helper records HeroContent, Project, SocialLink, and HeaderLink."
section: options
order: 30
tags: []
uid: reference.options.blogsite-options
isDraft: true
search: false
llms: false
---

> **In this page.** Every property on `BlogSiteOptions` — metadata (`SiteTitle`, `Description`, `CanonicalBaseUrl`), content paths (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), styling (`PrimaryHue`, `BaseColorName`, fonts, `ExtraStyles`, `AdditionalHtmlHeadContent`), author chrome (`AuthorName`, `AuthorBio`), homepage data (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`), feature toggles (`EnableRss`, `EnableSitemap`), and integration hooks (`SolutionPath`, `SocialMediaImageUrlFactory`, `AdditionalRoutingAssemblies`) — plus the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`.
>
> **Not in this page.** `DocSiteOptions` or `PenningtonOptions` (preceding pages).

## Summary

The record that configures a Pennington blog-site template. Namespace `Pennington.BlogSite`, supplied to `services.AddBlogSite(() => new BlogSiteOptions { ... })`.

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

## Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | Extra HTML injected into the `<head>` of every page. |
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Extra assemblies scanned for Razor components and routing. |
| `AuthorBio` | `string?` | `null` | Author biography displayed in site chrome. |
| `AuthorName` | `string?` | `null` | Author display name. |
| `BlogBaseUrl` | `string` | `"/blog"` | URL prefix under which blog posts are served. |
| `BlogContentPath` | `string` | `"Blog"` | Filesystem folder (under `ContentRootPath`) containing blog markdown. |
| `BodyFontFamily` | `string?` | `null` | CSS `font-family` stack for body text. |
| `CanonicalBaseUrl` | `string?` | `null` | Canonical absolute base URL used for feeds, sitemap, and social metadata. |
| `ColorScheme` | `IColorScheme?` | `null` | MonorailCSS color scheme applied to the site palette. |
| `ContentRootPath` | `string` | `"Content"` | Root filesystem folder for all site content. |
| `Description` | `string` (required) | — | Site description used in metadata and feeds. |
| `DisplayFontFamily` | `string?` | `null` | CSS `font-family` stack for display/heading text. |
| `EnableRss` | `bool` | `true` | When `true`, maps `/rss.xml`. |
| `EnableSitemap` | `bool` | `true` | When `true`, generates `sitemap.xml`. |
| `ExtraStyles` | `string?` | `null` | Raw CSS appended to the generated stylesheet. |
| `FontPreloads` | `FontPreload[]` | `[]` | Font files emitted as `<link rel="preload">` in `<head>`. |
| `HeroContent` | `HeroContent?` | `null` | Hero block rendered on the homepage. |
| `MainSiteLinks` | `HeaderLink[]` | `[]` | Header navigation links to the author's main site. |
| `MyWork` | `Project[]` | `[]` | Projects listed in the homepage work section. |
| `SiteTitle` | `string` (required) | — | Site title used in metadata, feeds, and chrome. |
| `SocialMediaImageUrlFactory` | `Func<BlogPostPage, string>?` | `null` | Produces a per-post social-card image URL. |
| `Socials` | `SocialLink[]` | `[]` | Social-media links displayed in site chrome. |
| `TagsPageUrl` | `string` | `"/tags"` | URL of the tag-index page. |

### Not present on `BlogSiteOptions`

The TOC row referenced `PrimaryHue`, `BaseColorName`, and `SolutionPath`; none of these exist on `BlogSiteOptions`. Color configuration is supplied via `ColorScheme` (`IColorScheme`). There is no `SolutionPath` property — Roslyn integration is configured through `services.AddPenningtonRoslyn(...)`.

## Helper records

### `HeroContent`

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

| Parameter | Type | Description |
|---|---|---|
| `Title` | `string` | Hero title text. |
| `Description` | `string` | Hero description text. |

### `Project`

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

| Parameter | Type | Description |
|---|---|---|
| `Title` | `string` | Project title. |
| `Description` | `string` | Project description. |
| `Url` | `string` | Link to the project. |

### `SocialLink`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

| Parameter | Type | Description |
|---|---|---|
| `Icon` | `RenderFragment` | Razor render fragment for the social icon. |
| `Url` | `string` | Target URL of the social profile. |

### `HeaderLink`

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

| Parameter | Type | Description |
|---|---|---|
| `Title` | `string` | Link text. |
| `Url` | `string` | Target URL. |

## See also

- Related reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
