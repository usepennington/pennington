---
title: "BlogSiteOptions"
description: "Configuration surface passed to AddBlogSite — blog metadata, content paths, author chrome, homepage data, and feed toggles."
sectionLabel: "Configuration Options"
order: 30
tags: [options, blog, configuration]
uid: reference.options.blogsite-options
---

> **In this page.** Every property on `BlogSiteOptions` — metadata (`SiteTitle`, `Description`, `CanonicalBaseUrl`), content paths (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), styling (`ColorScheme`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, `AdditionalHtmlHeadContent`, `FontPreloads`), author chrome (`AuthorName`, `AuthorBio`), homepage data (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`), feature toggles (`EnableRss`, `EnableSitemap`), and integration hooks (`SocialMediaImageUrlFactory`, `AdditionalRoutingAssemblies`) — plus the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`.
>
> **Not in this page.** `DocSiteOptions` or `PenningtonOptions` — see the preceding pages in `/reference/options/`.

## Summary

_**One sentence: what it is.** The options record supplied to `services.AddBlogSite(Func<BlogSiteOptions>)` that configures the `Pennington.BlogSite` template — site identity, blog content layout, homepage composition, and feed toggles._
_**One sentence: where it lives.** Declared in namespace `Pennington.BlogSite` at `src/Pennington.BlogSite/BlogSiteOptions.cs`, alongside the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`._

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

_Show the full `BlogSiteOptions` record declaration so the reader sees the `required` markers on `SiteTitle`/`Description` and the `init` surface of every property in one place._

## Properties

_Ceremonial grouping — metadata → content paths → styling → author → homepage → toggles → hooks — mirrors the TOC `Covers` line; entries inside each group are alphabetical. One-sentence descriptions only; every row is a lookup entry, not a walkthrough._

### Metadata

| Name | Type | Default | Description |
|---|---|---|---|
| `CanonicalBaseUrl` | `string?` | `null` | _One-sentence: absolute origin used to build canonical links, RSS `<link>` entries, and `og:url` meta tags; forwarded to `PenningtonOptions.CanonicalBaseUrl`._ |
| `Description` | `string` (required) | — | _One-sentence: human-readable site description emitted into `<meta>` tags, RSS channel metadata, and layout chrome._ |
| `SiteTitle` | `string` (required) | — | _One-sentence: human-readable site title emitted into the document `<title>`, RSS channel, and header branding._ |

### Content paths

| Name | Type | Default | Description |
|---|---|---|---|
| `BlogBaseUrl` | `string` | `"/blog"` | _One-sentence: URL prefix under which individual post pages render (`/{BlogBaseUrl}/{slug}`)._ |
| `BlogContentPath` | `string` | `"Blog"` | _One-sentence: subdirectory under `ContentRootPath` that the blog content service scans for post markdown files._ |
| `ContentRootPath` | `string` | `"Content"` | _One-sentence: root directory (relative to the app content root) containing markdown sources and static assets for the site._ |
| `TagsPageUrl` | `string` | `"/tags"` | _One-sentence: URL of the tag index page and prefix for per-tag listing routes (`/{TagsPageUrl}/{tag}`)._ |

### Styling

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | _One-sentence: raw HTML injected into `<head>` on every page — used for analytics, fonts from external CDNs, or favicons._ |
| `BodyFontFamily` | `string?` | `null` | _One-sentence: CSS `font-family` value applied to body text via MonorailCSS typography tokens._ |
| `ColorScheme` | `IColorScheme?` | `null` | _One-sentence: the MonorailCSS color scheme passed through to `MonorailCssOptions.ColorScheme`; see [`MonorailCssOptions`](xref:reference.options.monorail-css-options)._ |
| `DisplayFontFamily` | `string?` | `null` | _One-sentence: CSS `font-family` value applied to display-style headings via MonorailCSS typography tokens._ |
| `ExtraStyles` | `string?` | `null` | _One-sentence: additional CSS appended to the generated MonorailCSS stylesheet; forwarded to `MonorailCssOptions.ExtraStyles`._ |
| `FontPreloads` | `FontPreload[]` | `[]` | _One-sentence: font files emitted as `<link rel="preload" as="font">` tags in `<head>`; see [`FontPreload`](xref:reference.options.auxiliary-options)._ |

### Author chrome

| Name | Type | Default | Description |
|---|---|---|---|
| `AuthorBio` | `string?` | `null` | _One-sentence: short biography rendered alongside the author name in the blog footer and author card._ |
| `AuthorName` | `string?` | `null` | _One-sentence: human-readable author name surfaced in the footer, RSS `<author>` entries, and structured-data author fields._ |

### Homepage data

| Name | Type | Default | Description |
|---|---|---|---|
| `HeroContent` | `HeroContent?` | `null` | _One-sentence: hero headline block rendered at the top of the home page; when `null`, the hero section is omitted._ |
| `MainSiteLinks` | `HeaderLink[]` | `[]` | _One-sentence: nav links rendered in the top-nav and footer of `MainLayout.razor` (so each link appears twice)._ |
| `MyWork` | `Project[]` | `[]` | _One-sentence: project cards rendered in the "My Work" homepage sidebar module; empty array omits the module._ |
| `Socials` | `SocialLink[]` | `[]` | _One-sentence: social icons rendered under the "My Work" module, each pairing a `RenderFragment` icon with a URL._ |

### Feature toggles

| Name | Type | Default | Description |
|---|---|---|---|
| `EnableRss` | `bool` | `true` | _One-sentence: when `true`, `UseBlogSite` maps `/rss.xml` and `BlogSiteContentService` emits an RSS feed entry for every post._ |
| `EnableSitemap` | `bool` | `true` | _One-sentence: when `true`, posts and homepage routes are included in `/sitemap.xml` emitted by `SitemapService`._ |

### Integration hooks

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | _One-sentence: extra assemblies scanned for `@page` Razor components beyond `Pennington.BlogSite` and the host assembly._ |
| `SocialMediaImageUrlFactory` | `Func<BlogPostPage, string>?` | `null` | _One-sentence: per-post hook that returns the `og:image` URL for a `BlogPostPage`; when `null`, no social image `<meta>` is emitted._ |

## Helper records

_The TOC explicitly bundles these four helper records on this page; each gets its own declaration fence and sub-table so the page remains one coherent unit._

### `HeroContent`

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | _One-sentence: hero headline text rendered as the `<h1>` of the home page hero section._ |
| `Description` | `string` | _One-sentence: hero subtitle rendered as `MarkupString` beneath the headline, so inline HTML passes through unescaped._ |

### `Project`

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | _One-sentence: project name rendered as the link text inside the "My Work" card._ |
| `Description` | `string` | _One-sentence: one-line summary rendered beneath the project title in the card._ |
| `Url` | `string` | _One-sentence: absolute or root-relative URL the project card links to._ |

### `SocialLink`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

| Name | Type | Description |
|---|---|---|
| `Icon` | `RenderFragment` | _One-sentence: Blazor render fragment used as the anchor's inner content — typically one of the static fields on `Pennington.BlogSite.Components.SocialIcons` (`GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`)._ |
| `Url` | `string` | _One-sentence: destination URL for the social icon anchor._ |

### `HeaderLink`

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | _One-sentence: link text rendered inside the top-nav and footer anchors._ |
| `Url` | `string` | _One-sentence: destination URL for the nav anchor (absolute or root-relative)._ |

## Example

_One minimal example pulled from `BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions` — it populates every surface described above in a single builder method, so a reader can see the shape of a fully-configured `BlogSiteOptions` without leaving the page._

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

_A single sentence of context: every homepage surface, feed toggle, and author field is set here so this method serves as the shape reference for the rest of the BlogSite how-tos._

## See also

- How-to: [Configure the BlogSite homepage](xref:how-to.configuration.blogsite-homepage)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) <!-- TODO verify explanation URL once the page lands -->
