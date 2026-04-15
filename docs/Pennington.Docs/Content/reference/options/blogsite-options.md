---
title: "BlogSiteOptions"
description: "Configuration surface passed to AddBlogSite — blog metadata, content paths, author chrome, homepage data, and feed toggles."
sectionLabel: "Configuration Options"
order: 401030
tags: [options, blog, configuration]
uid: reference.options.blogsite-options
---

`BlogSiteOptions` is the record supplied to `services.AddBlogSite(...)` that configures the `Pennington.BlogSite` template — site identity, content layout, homepage composition, and feed toggles. It is declared in namespace `Pennington.BlogSite`, alongside the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`.

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

## Properties

### Metadata

| Name | Type | Default | Description |
|---|---|---|---|
| `CanonicalBaseUrl` | `string?` | `null` | Absolute origin used to build canonical links, RSS `<link>` entries, and `og:url` meta tags; forwarded to `PenningtonOptions.CanonicalBaseUrl`. |
| `Description` | `string` (required) | — | Human-readable site description emitted into `<meta>` tags, RSS channel metadata, and layout chrome. |
| `SiteTitle` | `string` (required) | — | Human-readable site title emitted into the document `<title>`, RSS channel, and header branding. |

### Content paths

| Name | Type | Default | Description |
|---|---|---|---|
| `BlogBaseUrl` | `string` | `"/blog"` | URL prefix under which individual post pages render (`/{BlogBaseUrl}/{slug}`). |
| `BlogContentPath` | `string` | `"Blog"` | Subdirectory under `ContentRootPath` that the blog content service scans for post markdown files. |
| `ContentRootPath` | `string` | `"Content"` | Root directory (relative to the app content root) containing markdown sources and static assets for the site. |
| `TagsPageUrl` | `string` | `"/tags"` | URL of the tag index page and prefix for per-tag listing routes (`/{TagsPageUrl}/{tag}`). |

### Styling

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalHtmlHeadContent` | `string?` | `null` | Raw HTML injected into `<head>` on every page — used for analytics, fonts from external CDNs, or favicons. |
| `BodyFontFamily` | `string?` | `null` | CSS `font-family` value applied to body text via MonorailCSS typography tokens. |
| `ColorScheme` | `IColorScheme?` | `null` | MonorailCSS color scheme passed through to `MonorailCssOptions.ColorScheme`; see <xref:reference.options.monorail-css-options>. |
| `DisplayFontFamily` | `string?` | `null` | CSS `font-family` value applied to display-style headings via MonorailCSS typography tokens. |
| `ExtraStyles` | `string?` | `null` | Additional CSS appended to the generated MonorailCSS stylesheet; forwarded to `MonorailCssOptions.ExtraStyles`. |
| `FontPreloads` | `FontPreload[]` | `[]` | Font files emitted as `<link rel="preload" as="font">` tags in `<head>`; see <xref:reference.options.auxiliary-options>. |

### Author chrome

| Name | Type | Default | Description |
|---|---|---|---|
| `AuthorBio` | `string?` | `null` | Short biography rendered alongside the author name in the blog footer and author card. |
| `AuthorName` | `string?` | `null` | Author name surfaced in the footer, RSS `<author>` entries, and structured-data author fields. |

### Homepage data

| Name | Type | Default | Description |
|---|---|---|---|
| `HeroContent` | `HeroContent?` | `null` | Hero headline block rendered at the top of the home page; when `null`, the hero section is omitted. |
| `MainSiteLinks` | `HeaderLink[]` | `[]` | Nav links rendered in the top-nav and footer of `MainLayout.razor` (each link appears in both locations). |
| `MyWork` | `Project[]` | `[]` | Project cards rendered in the "My Work" homepage sidebar module; an empty array omits the module. |
| `Socials` | `SocialLink[]` | `[]` | Social icons rendered under the "My Work" module, each pairing a `RenderFragment` icon with a URL. |

### Feature toggles

| Name | Type | Default | Description |
|---|---|---|---|
| `EnableRss` | `bool` | `true` | When `true`, `UseBlogSite` maps `/rss.xml` and `BlogSiteContentService` emits an RSS feed entry for every post. |
| `EnableSitemap` | `bool` | `true` | When `true`, posts and homepage routes are included in `/sitemap.xml` emitted by `SitemapService`. |

### Integration hooks

| Name | Type | Default | Description |
|---|---|---|---|
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Extra assemblies scanned for `@page` Razor components beyond `Pennington.BlogSite` and the host assembly. |
| `SocialMediaImageUrlFactory` | `Func<BlogPostPage, string>?` | `null` | Per-post delegate that returns the `og:image` URL for a `BlogPostPage`; when `null`, no social image `<meta>` is emitted. |

## Helper records

### `HeroContent`

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Hero headline text rendered as the `<h1>` of the home page hero section. |
| `Description` | `string` | Hero subtitle rendered as `MarkupString` beneath the headline; inline HTML passes through unescaped. |

### `Project`

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Project name rendered as the link text inside the "My Work" card. |
| `Description` | `string` | One-line summary rendered beneath the project title in the card. |
| `Url` | `string` | Absolute or root-relative URL the project card links to. |

### `SocialLink`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

| Name | Type | Description |
|---|---|---|
| `Icon` | `RenderFragment` | Blazor render fragment used as the anchor's inner content — typically one of the static fields on `Pennington.BlogSite.Components.SocialIcons` (`GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`). |
| `Url` | `string` | Destination URL for the social icon anchor. |

### `HeaderLink`

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Link text rendered inside the top-nav and footer anchors. |
| `Url` | `string` | Destination URL for the nav anchor (absolute or root-relative). |

## Example

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions
```

## See also

- How-to: [Configure the BlogSite homepage](xref:how-to.configuration.blogsite-homepage)
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning) <!-- TODO verify explanation URL once the page lands -->
