---
title: "Built-in BlogSite routes"
description: "Catalog of the routes the Pennington.BlogSite package ships out of the box, keyed to the BlogSiteOptions knobs that control them."
sectionLabel: "BlogSite Built-ins"
order: 408010
tags: [blogsite, routes, razor-pages, rss]
uid: reference.blogsite.routes
---

`UseBlogSite` mounts the Razor pages that serve the homepage, archive, tag index, per-tag listing, and individual posts, plus an optional `/rss.xml` endpoint. Pages are discovered via `RazorPageContentService`; `AddBlogSite` registers `Pennington.BlogSite` as an additional routing assembly so Razor picks them up.

## Entry point

`UseBlogSite` calls `MapRazorComponents<App>`, which discovers every `@page`-annotated component in `Pennington.BlogSite.dll`; when `BlogSiteOptions.EnableRss` is `true` it additionally maps a `MapGet("/rss.xml", …)` endpoint that delegates to `BlogSiteContentService.GetRssXmlAsync`. The `/sitemap.xml` endpoint is mounted by `UsePennington` via `SitemapService`, not by `UseBlogSite`.

## Routes

All routes registered by `UseBlogSite`, ordered by the surface they serve.

| Path | Method | Option controlling it | Description |
|---|---|---|---|
| `/` | `GET` | — (fixed) | Homepage Razor page (`Home.razor`); renders `BlogSiteOptions.HeroContent`, the ten most recent posts via `BlogSummary`, and the sidebar modules fed by `MyWork`/`Socials`/`AuthorBio`. |
| `/archive` | `GET` | — (fixed) | Full archive Razor page (`Archive.razor`); renders `BlogContentResolver.GetAllPostsAsync()` in reverse chronological order through `BlogSummary`. |
| `/tags` | `GET` | `TagsPageUrl` (see note) | Tag index Razor page (`Tags.razor`); lists every `BlogTag` with post counts from `BlogContentResolver.GetTagsWithCountsAsync()`. |
| `/topics` | `GET` | — (fixed alias) | Second `@page` directive on `Tags.razor` exposing the same component under `/topics` as a built-in alias. |
| `/tags/{TagEncodedName}` | `GET` | `TagsPageUrl` (see note) | Per-tag listing Razor page (`Tag.razor`); resolves the encoded tag name via `BlogContentResolver.GetPostsByTagAsync` and renders matching posts through `BlogPostsList`. |
| `/topics/{TagEncodedName}` | `GET` | — (fixed alias) | Second `@page` directive on `Tag.razor` exposing the per-tag listing under `/topics/{TagEncodedName}` as a built-in alias. |
| `/blog/{*fileName}` | `GET` | `BlogBaseUrl` (see note) | Post-rendering catch-all Razor page (`Blog.razor`); looks up the post by `{BlogBaseUrl}/{fileName}` via `BlogContentResolver.GetPostByUrlAsync` and renders it through `BlogPost` with OpenGraph and structured-data head tags. |
| `/rss.xml` | `GET` | `EnableRss` | `MapGet` endpoint in `UseBlogSite` that delegates to `BlogSiteContentService.GetRssXmlAsync`; omitted entirely when `EnableRss` is `false`. |

> **Note on `TagsPageUrl` and `BlogBaseUrl`.** The `@page` directives on `Tags.razor`, `Tag.razor`, and `Blog.razor` are string literals (`"/tags"`, `"/topics"`, `"/blog/{*fileName}"`) and are not templated from `BlogSiteOptions`. `TagsPageUrl` (default `"/tags"`) and `BlogBaseUrl` (default `"/blog"`) are used by `BlogContentResolver` / `BlogSiteContentService` to build tag and post URLs that match these page routes; changing them away from the defaults requires supplying replacement Razor pages via `AdditionalRoutingAssemblies`. See [`BlogSiteOptions`](xref:reference.api.blog-site-options).

## Option-to-route matrix

`BlogSiteOptions` knobs that affect route registration or URL resolution, one row per option.

| Option | Default | Routes it affects | Effect |
|---|---|---|---|
| `BlogBaseUrl` | `"/blog"` | `/blog/{*fileName}` | Consumed by `BlogContentResolver.GetPostByUrlAsync` as the URL prefix; the `@page "/blog/{*fileName}"` directive on `Blog.razor` is a fixed string, so this value must match the literal route for posts to resolve. |
| `EnableRss` | `true` | `/rss.xml` | Gates the `MapGet("/rss.xml", …)` call in `UseBlogSite`; when `false` the endpoint is not registered and the static crawler does not emit `rss.xml`. |
| `EnableSitemap` | `true` | `/sitemap.xml` (from `UsePennington`) | Controls inclusion of BlogSite routes in the sitemap emitted by `SitemapService`; the sitemap endpoint itself is mounted by `UsePennington`, not `UseBlogSite`. |
| `TagsPageUrl` | `"/tags"` | `/tags`, `/tags/{TagEncodedName}` | Consumed by `BlogContentResolver` when composing per-tag URLs; the `@page` directives on `Tags.razor` and `Tag.razor` are fixed string literals, so tag URLs and page routes only align at the default `"/tags"` value (the `/topics` aliases are always present). |

## Example

A minimal BlogSite host that mounts every route on this page via a single `UseBlogSite` call.

```csharp:path
examples/BlogSiteScaffoldExample/Program.cs
```

The example boots `Pennington.BlogSite` with scaffold options; all eight routes listed above — including `/rss.xml`, because `EnableRss` defaults to `true` — are live in dev and in the static build.

## See also

- Reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options)
- Reference: [Built-in `SocialIcons` `RenderFragment`s](xref:reference.blogsite.social-icons)
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components)
- How-to: [Configure the BlogSite homepage](xref:how-to.configuration.blogsite-homepage)
