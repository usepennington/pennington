---
title: "Built-in BlogSite routes"
description: "Catalog of the routes the Pennington.BlogSite package ships out of the box, keyed to the BlogSiteOptions knobs that control them."
sectionLabel: "BlogSite Built-ins"
order: 1
tags: [blogsite, routes, razor-pages, rss]
uid: reference.blogsite.routes
---

`UseBlogSite` mounts the Razor pages that serve the homepage, archive, tag index, per-tag listing, and individual posts, plus an optional `/rss.xml` endpoint. Pages are discovered via `RazorPageContentService`; `AddBlogSite` registers `Pennington.BlogSite` as an additional routing assembly so Razor picks them up.

## Entry point

`UseBlogSite` calls `MapRazorComponents<App>`, which discovers every `@page`-annotated component in `Pennington.BlogSite.dll`; when `BlogSiteOptions.EnableRss` is `true` it additionally maps a `MapGet("/rss.xml", …)` endpoint that returns the RSS feed. The `/sitemap.xml` endpoint is mounted by `UsePennington` via `SitemapService` (gated on `PenningtonOptions.MapSitemap`, which `AddBlogSite` mirrors from `BlogSiteOptions.EnableSitemap`).

## Routes

Every route a BlogSite host serves, ordered by the surface they serve. Most are `@page` Razor components mapped by `UseBlogSite`, including the `/{*path:nonfile}` root catch-all; `/sitemap.xml` is the exception, mounted by `UsePennington`. `UseBlogSite` runs `UsePennington` before mapping the component endpoint, so `redirectUrl:` pages short-circuit with a redirect rather than falling through to the catch-all.

| Path | Method | Option controlling it | Description |
|---|---|---|---|
| `/` | `GET` | — (fixed) | Homepage Razor page (`Home.razor`); renders `BlogSiteOptions.HeroContent`, recent posts via `BlogSummary`, and sidebar modules from `MyWork`/`Socials`/`AuthorBio`. |
| `/archive` | `GET` | — (fixed) | Full archive Razor page (`Archive.razor`); renders every post in reverse chronological order through `BlogSummary`. |
| `/archive/page/{Page:int}` | `GET` | `PostsPerPage` | Second `@page` directive on `Archive.razor` for paginated archive views; only renders extra pages when `PostsPerPage > 0` and there are more posts than fit on one page. |
| `/tags` | `GET` | `TagsPageUrl` (see note) | Tag index Razor page (`Tags.razor`); lists every `BlogTag` with post counts. |
| `/topics` | `GET` | — (fixed alias) | Second `@page` directive on `Tags.razor` exposing the same component under `/topics` as a built-in alias. |
| `/tags/{TagEncodedName}` | `GET` | `TagsPageUrl` (see note) | Per-tag listing Razor page (`Tag.razor`); resolves the encoded tag name and renders matching posts through `BlogPostsList`. |
| `/tags/{TagEncodedName}/page/{Page:int}` | `GET` | `PostsPerPage` | Paginated per-tag listing Razor page (`Tag.razor`); only renders extra pages when `PostsPerPage > 0` and the tag has more posts than fit on one page. |
| `/topics/{TagEncodedName}` | `GET` | — (fixed alias) | Second `@page` directive on `Tag.razor` exposing the per-tag listing under `/topics/{TagEncodedName}` as a built-in alias. |
| `/topics/{TagEncodedName}/page/{Page:int}` | `GET` | `PostsPerPage` | Paginated alias under `/topics/...` for the per-tag listing. |
| `/blog/{*fileName:nonfile}` | `GET` | `BlogBaseUrl` (see note) | Post-rendering catch-all Razor page (`Blog.razor`); the `:nonfile` route constraint excludes paths that look like static files. Looks up the post by `{BlogBaseUrl}/{fileName}` and renders it through `BlogPost` with OpenGraph and structured-data head tags. |
| `/{*path:nonfile}` | `GET` | — (fixed) | Root catch-all Razor page (`Pages.razor`); the lowest-priority route that handles any request matching no other page. Renders a content-root `404.md`, then a host-provided `NotFound` component, then the built-in localized not-found message, and marks the response 404. The build's sentinel request also lands here, so whatever it renders becomes `output/404.html`. |
| `/rss.xml` | `GET` | `EnableRss` | `MapGet` endpoint in `UseBlogSite` that returns the RSS feed; omitted entirely when `EnableRss` is `false`. |
| `/sitemap.xml` | `GET` | `EnableSitemap` | `MapGet` endpoint mounted by `UsePennington` (not `UseBlogSite`); `AddBlogSite` forwards `EnableSitemap` into `PenningtonOptions.MapSitemap` to gate it. |

The `@page` directives on `Tags.razor`, `Tag.razor`, and `Blog.razor` are fixed string literals. `TagsPageUrl` and `BlogBaseUrl` only affect the URLs `BlogContentResolver` and `BlogSiteContentService` compose for tags and posts; the page routes themselves stay at `/tags`, `/topics`, and `/blog/{*fileName:nonfile}` unless replacement Razor pages are supplied via `AdditionalRoutingAssemblies`.

## Option-to-route matrix

`BlogSiteOptions` knobs that affect route registration or URL resolution, one row per option.

| Option | Default | Routes it affects | Effect |
|---|---|---|---|
| `BlogBaseUrl` | `"/blog"` | `/blog/{*fileName:nonfile}` | The URL prefix `Blog.razor` strips before resolving a post; must match the `/blog/...` literal in the page route (see the fixed-string-literal note above). |
| `EnableRss` | `true` | `/rss.xml` | Gates the `MapGet("/rss.xml", …)` call in `UseBlogSite`; when `false` the endpoint is not registered and the static crawler does not emit `rss.xml`. |
| `PostsPerPage` | `10` | `/archive/page/{Page:int}`, `/tags/{TagEncodedName}/page/{Page:int}`, `/topics/{TagEncodedName}/page/{Page:int}` | Page size for the archive and per-tag paginated routes. Set to `0` to disable pagination — all posts then render on the first page. |
| `EnableSitemap` | `true` | `/sitemap.xml` (from `UsePennington`) | Forwards into `PenningtonOptions.MapSitemap`; when `false`, `UsePennington` skips the `/sitemap.xml` `MapGet` and the static crawler omits it from the build output. |
| `TagsPageUrl` | `"/tags"` | `/tags`, `/tags/{TagEncodedName}` | Consumed by `BlogContentResolver` when composing per-tag URLs. The `/topics` aliases are always present. |

## Example

A BlogSite host that mounts every route above via a single `UseBlogSite` call.

```csharp:symbol
examples/BlogSiteScaffoldExample/Program.cs
```

## See also

- Reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options)
- Reference: [Built-in `SocialIcons` `RenderFragment`s](xref:reference.blogsite.social-icons)
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components)
- How-to: [Configure the BlogSite homepage](xref:how-to.theming.blogsite-homepage)
