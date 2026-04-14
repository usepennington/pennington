---
title: "Built-in BlogSite routes"
description: "Catalog of the routes the Pennington.BlogSite package ships out of the box, keyed to the BlogSiteOptions knobs that control them."
sectionLabel: "BlogSite Built-ins"
order: 408010
tags: [blogsite, routes, razor-pages, rss]
uid: reference.blogsite.routes
---

> **In this page.** The routes the `Pennington.BlogSite` package ships out of the box — `/` (home), `/archive`, `/tags` and its alias `/topics`, `/tags/{TagEncodedName}` and its alias `/topics/{TagEncodedName}`, `/blog/{*fileName}` for individual posts, and `/rss.xml` when `EnableRss` is on — plus which `BlogSiteOptions` knobs (`TagsPageUrl`, `BlogBaseUrl`, `EnableRss`, `EnableSitemap`) each route honors.
>
> **Not in this page.** Customizing the Razor page bodies themselves — see [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components), which applies symmetrically to BlogSite.

## Summary

_**One sentence: what it is.** The fixed route surface mounted by `UseBlogSite` — five Razor pages inside `src/Pennington.BlogSite/Components/Pages/` plus an optional `/rss.xml` `MapGet` endpoint — that together render the homepage, archive, tag index, per-tag listings, individual posts, and the RSS feed._
_**One sentence: where it lives.** Page routes are declared by `@page` directives on the Razor components in `Pennington.BlogSite.Components.Pages` (discovered via `RazorPageContentService` because `AddBlogSite` adds the BlogSite assembly to `PenningtonOptions.AdditionalRoutingAssemblies`); the `/rss.xml` endpoint is mapped by `BlogSiteServiceExtensions.UseBlogSite`._

## Declaration

_The entry point that mounts the built-in Razor pages and the optional `/rss.xml` endpoint._

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)
```

_One to two neutral sentences: `UseBlogSite` calls `MapRazorComponents<App>`, which discovers every `@page`-annotated component in `Pennington.BlogSite.dll`; when `BlogSiteOptions.EnableRss` is true it additionally maps a plain `MapGet("/rss.xml", …)` endpoint that delegates to `BlogSiteContentService.GetRssXmlAsync`. The `/sitemap.xml` endpoint is not mounted here — `UsePennington` owns it via `SitemapService`._

## Routes

_Every TOC-listed route must have a row. Columns: Path / Method / Option controlling it / Description (one sentence). Entries ordered by the surface they back (home → archive → tag index → per-tag → per-post → feed)._

| Path | Method | Option controlling it | Description |
|---|---|---|---|
| `/` | `GET` | — (fixed) | _One-sentence: homepage Razor page at `src/Pennington.BlogSite/Components/Pages/Home.razor`; renders `BlogSiteOptions.HeroContent`, the ten most recent posts via `BlogSummary`, and the sidebar modules fed by `MyWork`/`Socials`/`AuthorBio`._ |
| `/archive` | `GET` | — (fixed) | _One-sentence: full archive Razor page at `src/Pennington.BlogSite/Components/Pages/Archive.razor`; renders `BlogContentResolver.GetAllPostsAsync()` in reverse chronological order through `BlogSummary`._ |
| `/tags` | `GET` | `TagsPageUrl` (see note) | _One-sentence: tag index Razor page at `src/Pennington.BlogSite/Components/Pages/Tags.razor`; lists every `BlogTag` with post counts from `BlogContentResolver.GetTagsWithCountsAsync()`._ |
| `/topics` | `GET` | — (fixed alias) | _One-sentence: second `@page` directive on `Tags.razor` exposing the same component under `/topics` as a built-in alias._ |
| `/tags/{TagEncodedName}` | `GET` | `TagsPageUrl` (see note) | _One-sentence: per-tag listing Razor page at `src/Pennington.BlogSite/Components/Pages/Tag.razor`; resolves the encoded tag name via `BlogContentResolver.GetPostsByTagAsync` and renders matching posts through `BlogPostsList`._ |
| `/topics/{TagEncodedName}` | `GET` | — (fixed alias) | _One-sentence: second `@page` directive on `Tag.razor` exposing the per-tag listing under `/topics/{TagEncodedName}` as a built-in alias._ |
| `/blog/{*fileName}` | `GET` | `BlogBaseUrl` (see note) | _One-sentence: post-rendering Razor page at `src/Pennington.BlogSite/Components/Pages/Blog.razor`; catch-all that looks up the post by `{BlogBaseUrl}/{fileName}` via `BlogContentResolver.GetPostByUrlAsync` and renders it through `BlogPost` with OpenGraph and structured-data head tags._ |
| `/rss.xml` | `GET` | `EnableRss` | _One-sentence: plain `MapGet` endpoint in `UseBlogSite` that delegates to `BlogSiteContentService.GetRssXmlAsync`; omitted entirely when `EnableRss` is `false`._ |

> **Note on `TagsPageUrl` and `BlogBaseUrl`.** The `@page` directives on `Tags.razor`, `Tag.razor`, and `Blog.razor` are string literals (`"/tags"`, `"/topics"`, `"/blog/{*fileName}"`) and are not templated from `BlogSiteOptions`. `TagsPageUrl` (default `"/tags"`) and `BlogBaseUrl` (default `"/blog"`) are used by `BlogContentResolver` / `BlogSiteContentService` to build tag and post URLs that match these page routes; changing them away from the defaults requires supplying replacement Razor pages via `AdditionalRoutingAssemblies`. See [`BlogSiteOptions`](xref:reference.options.blogsite-options).

## Option-to-route matrix

_Which `BlogSiteOptions` knob affects which route. Lookup-shape, one row per (option, route) pair. Covers only the four knobs named in the TOC `Covers` line._

| Option | Default | Routes it affects | Effect |
|---|---|---|---|
| `BlogBaseUrl` | `"/blog"` | `/blog/{*fileName}` | _One-sentence: consumed by `BlogContentResolver.GetPostByUrlAsync` as the URL prefix; `@page "/blog/{*fileName:nonfile}"` on `Blog.razor` is a fixed string, so the prefix here must match the literal route for posts to resolve._ |
| `EnableRss` | `true` | `/rss.xml` | _One-sentence: gates the `MapGet("/rss.xml", …)` call in `UseBlogSite`; when `false` the endpoint is not registered and the static crawler does not emit `rss.xml`._ |
| `EnableSitemap` | `true` | `/sitemap.xml` (from `UsePennington`) | _One-sentence: controls inclusion of BlogSite routes in the sitemap emitted by `SitemapService`; does not gate a route on this page — the sitemap endpoint itself is mounted by `UsePennington`, not `UseBlogSite`._ |
| `TagsPageUrl` | `"/tags"` | `/tags`, `/tags/{TagEncodedName}` | _One-sentence: consumed by `BlogContentResolver` when composing per-tag URLs; the `@page` directives on `Tags.razor` and `Tag.razor` are fixed string literals, so tag URLs and page routes only align at the default `"/tags"` value (the `/topics` aliases are always present)._ |

## Example

_A minimal BlogSite host that mounts every route on this page via a single `UseBlogSite` call. This is the canonical wiring — no additional route registration required on the caller side._

```csharp:path
examples/BlogSiteScaffoldExample/Program.cs
```

_One-sentence context: the example boots `Pennington.BlogSite` with the scaffold options, after which `/`, `/archive`, `/tags`, `/topics`, `/tags/{TagEncodedName}`, `/topics/{TagEncodedName}`, `/blog/{*fileName}`, and (because `EnableRss` defaults to `true`) `/rss.xml` are all live in dev and in the static build._

## See also

- Reference: [`BlogSiteOptions`](xref:reference.options.blogsite-options)
- Reference: [Built-in `SocialIcons` `RenderFragment`s](xref:reference.blogsite.social-icons)
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components)
- How-to: [Configure the BlogSite homepage](xref:how-to.configuration.blogsite-homepage)
