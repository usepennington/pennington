---
title: "Built-in BlogSite routes"
description: "Routes the Pennington.BlogSite package ships by default — home, archive, tags/topics index, per-tag pages, per-post pages, and the RSS feed — plus which BlogSiteOptions knobs each route honors."
section: "blogsite"
order: 10
tags: []
uid: reference.blogsite.routes
isDraft: true
search: false
llms: false
---

> **In this page.** The routes the `Pennington.BlogSite` package ships out of the box — `/`, `/archive`, `/tags` and its alias `/topics`, `/tags/{TagEncodedName}` and its alias `/topics/{TagEncodedName}`, `/blog/{*fileName}`, and `/rss.xml` when `EnableRss` is on — plus which `BlogSiteOptions` knobs each route honors.
>
> **Not in this page.** Customizing the Razor page bodies themselves (see the "Customize DocSite layouts and components" how-to, which applies symmetrically to BlogSite).

## Summary

- Seven routes are registered automatically when a site calls `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`.
- Five are Razor-page routes registered via the `Pennington.BlogSite.dll` routing assembly; the sixth is the per-post Razor page; the seventh (`/rss.xml`) is a minimal-API endpoint mapped by `UseBlogSite` when `BlogSiteOptions.EnableRss` is `true`.

## Route table

| Route(s) | Source | Bound by | Honors |
|---|---|---|---|
| `/` | `Pennington.BlogSite.Components.Pages.Home` (`src/Pennington.BlogSite/Components/Pages/Home.razor`) | `@page "/"` | `SiteTitle`, `Description`, `HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`, `AuthorName`, `AuthorBio`. |
| `/archive` | `Pennington.BlogSite.Components.Pages.Archive` (`src/Pennington.BlogSite/Components/Pages/Archive.razor`) | `@page "/archive"` | All non-draft posts from `BlogContentResolver.GetAllPostsAsync()`, ordered by `date` descending. |
| `/tags`, `/topics` | `Pennington.BlogSite.Components.Pages.Tags` (`src/Pennington.BlogSite/Components/Pages/Tags.razor`) | `@page "/tags"`, `@page "/topics"` | Tag-with-count listing from `BlogContentResolver.GetTagsWithCountsAsync()`. Both routes resolve to the same page. |
| `/tags/{TagEncodedName}`, `/topics/{TagEncodedName}` | `Pennington.BlogSite.Components.Pages.Tag` (`src/Pennington.BlogSite/Components/Pages/Tag.razor`) | `@page "/tags/{TagEncodedName}"`, `@page "/topics/{TagEncodedName}"` | Posts whose `Tags` contain the URL-decoded tag name. `BlogSiteContentService` also emits a `ContentToCreate` for every tag seen, so the static-build crawler discovers each per-tag URL. |
| `/blog/{*fileName}` | `Pennington.BlogSite.Components.Pages.Blog` (`src/Pennington.BlogSite/Components/Pages/Blog.razor`) | `@page "/blog/{*fileName:nonfile}"` | Each post in `BlogContentPath`. The `{*fileName}` catch-all maps to the post's file path under `BlogBaseUrl`. |
| `/rss.xml` | `BlogSiteServiceExtensions.UseBlogSite` (`src/Pennington.BlogSite/BlogSiteServiceExtensions.cs`) | `app.MapGet("/rss.xml", …)` | Registered only when `BlogSiteOptions.EnableRss == true` (default). Serves `BlogSiteContentService.GetRssXmlAsync()` with `Content-Type: application/xml`. |

## Options-to-route map

| Option | Routes affected | Effect |
|---|---|---|
| `ContentRootPath` (default `"Content"`) | `/blog/{fileName}`, `/archive`, `/tags/*` | Root directory for all content; the blog content directory is `Path.Combine(ContentRootPath, BlogContentPath)`. |
| `BlogContentPath` (default `"Blog"`) | `/blog/{fileName}`, `/archive`, `/tags/*` | Subfolder of `ContentRootPath` where `*.md` posts live; bound via `AddMarkdownContent<BlogSiteFrontMatter>` with `ContentPath = Path.Combine(ContentRootPath, BlogContentPath)`. |
| `BlogBaseUrl` (default `/blog`) | `/blog/{fileName}` | `BasePageUrl` passed to `AddMarkdownContent<BlogSiteFrontMatter>`; the post slug is appended to this base. |
| `TagsPageUrl` (default `/tags`) | `/tags`, `/topics`, `/tags/{TagEncodedName}`, `/topics/{TagEncodedName}` | Documented on `BlogSiteOptions`; both `/tags` and `/topics` are always registered by the shipped Razor pages. |
| `EnableRss` (default `true`) | `/rss.xml` | Gate for the minimal-API registration in `UseBlogSite`. `false` removes the route entirely. |
| `EnableSitemap` (default `true`) | `/sitemap.xml` (from core Pennington, not BlogSite-specific) | Controls whether the core sitemap endpoint is mapped. |

## Layout / page behavior notes

- `Tags.razor` and `Archive.razor` both use the `ContentWithProseLayout` layout and inject `BlogContentResolver` plus `PenningtonOptions`.
- `Tag.razor` URL-decodes `{TagEncodedName}` before filtering posts; tag names with spaces or Unicode are expected to be percent-encoded in the URL.
- The RSS endpoint's absolute URLs depend on `BlogSiteOptions.CanonicalBaseUrl`; leaving it `null` produces a validating but semantically broken feed (relative links).

## See also

- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options) — full option surface.
- Related reference: [Built-in `SocialIcons` `RenderFragment`s](/reference/blogsite/social-icons).
- Tutorial: [Scaffold a blog with BlogSite](/tutorials/blogsite/scaffold).
