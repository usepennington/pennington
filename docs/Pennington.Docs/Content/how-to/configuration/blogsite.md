---
title: "Configure BlogSite"
description: "Set the core BlogSite options for identity, content paths, author chrome, styling, and feature toggles."
section: "configuration"
order: 90
tags: []
uid: how-to.configuration.blogsite
isDraft: true
search: false
llms: false
---

> **In this page.** Setting the core `BlogSiteOptions` for site identity, content paths, author chrome, styling, and feature toggles.
>
> **Not in this page.** The hero/projects/socials data shapes (see the next three pages) or the low-level markdown pipeline (see Core Pennington How-Tos).

## When to use this

- You already have a BlogSite running and want to finish the common site-level setup in one pass.
- You want to set the values most teams touch first: title, URLs, author info, styles, and feed/site-map switches.

## Assumptions

- You have a working BlogSite (see [/tutorials/blogsite/scaffold](/tutorials/blogsite/scaffold) if not).
- You can edit the `new BlogSiteOptions { ... }` block in `Program.cs`.
- For a full property list, use the reference page for [`BlogSiteOptions`](/reference/options/blogsite-options).

To copy a working setup, see [`examples/AlexBlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/AlexBlogExample), [`examples/MaraBlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/MaraBlogExample), or [`examples/BlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/BlogExample).

---

## Steps

### 1. Set site metadata (`SiteTitle`, `Description`, `CanonicalBaseUrl`)

- Set `SiteTitle` and `Description` first so the site chrome and metadata have real values.
- Set `CanonicalBaseUrl` to the production URL you plan to publish at.
- Keep these three values together near the top of the options block so they are easy to revisit later.

```csharp:path
examples/AlexBlogExample/Program.cs
```

_This example shows the three identity fields set together._

### 2. Relocate content (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`)

- Point `ContentRootPath` at the content root you want the site to scan.
- Set `BlogContentPath`, `BlogBaseUrl`, and `TagsPageUrl` so the on-disk folders and published URLs line up with each other.
- If you rename one of these paths, update the others in the same edit so your content model stays obvious.

```csharp:path
examples/MaraBlogExample/Program.cs
```

_This example shows relocated post and tag paths._

### 3. Set author bio (`AuthorName`, `AuthorBio`)

- Add `AuthorName` if you want posts and feeds to show a byline.
- Add `AuthorBio` if you want a short author blurb in the site chrome.
- Stop here for basic author setup; hero content, projects, and socials belong on their own pages.

### 4. Set the color scheme (`ColorScheme`)

- Pick the color scheme you want the blog to ship with.
- If the default look is close enough, leave this alone and move on.
- If you do customize it, choose one direction and verify it against the homepage before tuning other visual options.

### 5. Set fonts (`DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, `AdditionalHtmlHeadContent`)

- Set `DisplayFontFamily` and `BodyFontFamily` if the defaults are not right for the site.
- Load the actual font assets through `AdditionalHtmlHeadContent` or self-host them and use `FontPreloads`.
- Verify the final typography in the browser before moving on to smaller tweaks.

```csharp:path
examples/BlogExample/Program.cs
```

_This example shows font families and head content set together._

### 6. Toggle features (`EnableRss`, `EnableSitemap`)

- Leave RSS and sitemap enabled unless you have a clear reason not to publish them.
- Turn either feature off only if another part of your stack is replacing it.
- Check both endpoints after you change these toggles.

### 7. (Advanced knobs) `ExtraStyles`, `AdditionalHtmlHeadContent`, `AdditionalRoutingAssemblies`, `SocialMediaImageUrlFactory`

- Add `ExtraStyles` or `AdditionalHtmlHeadContent` only when the main styling options are not enough.
- Add `AdditionalRoutingAssemblies` only if your site needs to discover extra Razor routes.
- Add `SocialMediaImageUrlFactory` only if you want per-post social images.
- Keep `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks` on their dedicated how-to pages.

---

## Verify

- Run `dotnet run` and visit `/` to confirm the homepage reflects your title, author, colors, and fonts.
- Visit the blog index and tags page to confirm the URLs you set actually resolve.
- Check `/rss.xml` and `/sitemap.xml` if you left those features enabled.

## Related

- Reference: [`BlogSiteOptions`](/reference/options/blogsite-options) — full property-by-property listing including `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks`.
- Reference: [`MonorailCssOptions`](/reference/monorailcss-options) — color scheme and content path details.
- Background: [BlogSite architecture](/explanation/blogsite-architecture) — why the options record is flat and how it relates to `DocSiteOptions`.
