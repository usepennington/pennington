---
title: "Paginate archive and tag listings"
description: "Split long blog archives and tag pages into numbered pages, and apply the same pattern to a custom IContentService."
uid: how-to.discovery.pagination
order: 5
sectionLabel: "Content Discovery"
tags: [blog-site, pagination, content-services]
---

Long listings — a five-year archive, a popular tag with hundreds of posts — get unwieldy past a few dozen entries. BlogSite includes pagination for archives and tag pages; custom content services can reuse the shared `Pagination` component to do the same.

## Before you begin

- A working Pennington site (see [Your first Pennington site](xref:tutorials.getting-started.first-page) if not).
- For the custom-service half: a bare `AddPennington` host that renders Razor `@page` components — the same Blazor wiring the [first-page tutorial](xref:tutorials.getting-started.first-page) sets up (`AddRazorComponents` + `MapRazorComponents`).
- Familiarity with [custom content services](xref:how-to.content-services.custom-content-service) — the recipe below adds one.

The custom-service recipe is implemented end to end in [`examples/PaginatedListingExample`](https://github.com/usepennington/pennington/tree/main/examples/PaginatedListingExample).

## In BlogSite

Set `PostsPerPage` on `BlogSiteOptions`. Paginated URLs appear automatically.

```csharp
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    SiteDescription = "Posts and notes.",
    PostsPerPage = 10,
});
```

Resulting routes:

- `/archive` — canonical, page 1 (unchanged).
- `/archive/page/2/`, `/archive/page/3/`, … — emitted only when the post count exceeds `PostsPerPage`.
- `/tags/{tag}/` — canonical per-tag page (unchanged).
- `/tags/{tag}/page/2/`, … — emitted only for tags that exceed `PostsPerPage`.

The default is `10`. A non-positive value disables pagination entirely (all posts on one page). The home page is intentionally not paginated — it stays curated with the recent-post slot and a link to the archive.

## In a custom content service

The pattern is three pieces: a tiny `PagedList<T>` record, a Razor page with two `@page` directives, and an `IContentService` that yields the paginated routes during discovery.

### The PagedList record

```csharp:symbol
examples/PaginatedListingExample/PagedList.cs > PagedList
```

### The Razor page

Two `@page` directives keep the canonical URL clean and add the paginated variant. Read the optional `Page` parameter, slice the source list through `ArticleResolver`, and render the shared `Pagination` component. `PageUrl` maps page 1 back to the canonical `/articles` URL.

```razor:symbol
examples/PaginatedListingExample/Components/Pages/ArticlesPage.razor
```

`ArticleResolver` collects the markdown articles from every content source and slices them into pages. It is a plain service — not an `IContentService` — so it can inject `IEnumerable<IContentService>` directly with no risk of a cycle.

```csharp:symbol
examples/PaginatedListingExample/ArticleResolver.cs
```

### The content service

A parameterized `@page` template (`{Page:int}`) is skipped by Pennington's automatic Razor route discovery. Emit each paginated route explicitly so the static build crawls them.

The service is itself one of the registered `IContentService` instances, so it must **not** constructor-inject `IEnumerable<IContentService>` — that forms a dependency cycle and throws at startup. Inject `IServiceProvider` instead and resolve the siblings on demand inside `DiscoverAsync`, excluding self with `!ReferenceEquals(s, this)`. This is the same pattern the library's own `SocialCardContentService` uses.

```csharp:symbol
examples/PaginatedListingExample/ArticleListingContentService.cs > ArticleListingContentService
```

Register the resolver and the service alongside the markdown source. `ArticleResolver` is a plain transient; `ArticleListingContentService` joins the `IContentService` set the same way any markdown source does.

```csharp
builder.Services.AddTransient<ArticleResolver>();
builder.Services.AddTransient<IContentService, ArticleListingContentService>();
```

## What ends up where

- **Sitemap.** Paginated routes appear in `sitemap.xml` automatically — they come from `DiscoverAsync` as HTML routes and `SitemapService` includes everything that isn't a redirect or llms-only sidecar.
- **Search index and llms.txt.** Excluded by default: `BlogSiteContentService` and the custom service above return empty `GetIndexableEntriesAsync()` (the default forwards to `GetContentTocEntriesAsync()`), so their routes never enter the search or llms paths. If a custom service does emit indexable entries for paginated routes, set `ExcludeFromSearch = true` and `ExcludeFromLlms = true` on those entries.
- **Navigation tree.** Same — paginated routes have no TOC entry, so they don't show in the sidebar or breadcrumbs.

## Verify

- Run `dotnet run` and visit `/articles`. The first 20 articles render, with the `Pagination` controls below them.
- Visit `/articles/page/2/`. The remaining articles render and the control highlights page 2 — confirming `ArticleListingContentService.DiscoverAsync` emitted the overflow route.
- Run `dotnet run -- build` and open `sitemap.xml` in the output directory. It lists `/articles/page/2/` alongside the individual article URLs, because the route flows through `DiscoverAsync` as an HTML route.

## Related

- Reference: [`BlogSiteOptions.PostsPerPage`](xref:reference.api.blog-site-options)
- Background: [Content pipeline overview](xref:explanation.core.content-pipeline)
- Extensibility: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
