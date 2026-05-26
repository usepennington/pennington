---
title: "Paginate archive and tag listings"
description: "Split long blog archives and tag pages into numbered pages, and apply the same pattern to a custom MarkdownContentService."
uid: how-to.discovery.pagination
order: 5
sectionLabel: "Content Discovery"
tags: [blog-site, pagination, content-services]
---

Long listings — a five-year archive, a popular tag with hundreds of posts — get unwieldy past a few dozen entries. BlogSite paginates archives and tag pages out of the box; custom content services can do the same with a few lines of code and the shared `Pagination` component.

## In BlogSite

Set `PostsPerPage` on `BlogSiteOptions`. Paginated URLs appear automatically.

```csharp
builder.Services.AddBlogSite(new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "Posts and notes.",
    PostsPerPage = 10,
});
```

Resulting routes:

- `/archive` — canonical, page 1 (unchanged).
- `/archive/page/2/`, `/archive/page/3/`, … — emitted only when the post count exceeds `PostsPerPage`.
- `/tags/{tag}/` — canonical per-tag page (unchanged).
- `/tags/{tag}/page/2/`, … — emitted only for tags that exceed `PostsPerPage`.

The default is `10`. A non-positive value disables pagination entirely (all posts on one page). The home page is intentionally not paginated — it stays curated with the recent-post slot and a link to the archive.

## In a custom MarkdownContentService

The pattern is three pieces: a tiny `PagedList<T>` record, a Razor page with two `@page` directives, and an `IContentService` that yields the paginated routes during discovery.

### The PagedList record

```csharp
public sealed record PagedList<T>(
    IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => TotalItems <= 0 || PageSize <= 0
        ? 1
        : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
```

### The Razor page

Two `@page` directives keep the canonical URL clean and add the paginated variant. Read the optional `Page` parameter, slice the source list, and render the shared `Pagination` component.

```razor
@page "/articles"
@page "/articles/page/{Page:int}"
@inject ArticleResolver Resolver

@if (_page is null)
{
    <p>No articles.</p>
    return;
}

<h1>Articles</h1>
<ul>
    @foreach (var article in _page.Items)
    {
        <li><a href="@article.Url">@article.Title</a></li>
    }
</ul>

<Pagination CurrentPage="@_page.Page" TotalPages="@_page.TotalPages" UrlFor="@PageUrl" />

@code {
    [Parameter] public int? Page { get; set; }
    private PagedList<Article>? _page;

    protected override async Task OnInitializedAsync()
    {
        _page = await Resolver.GetPagedAsync(Page ?? 1, pageSize: 20);
    }

    private static string PageUrl(int page) => page <= 1
        ? "/articles"
        : $"/articles/page/{page}/";
}
```

### The content service

A parameterized `@page` template (`{Page:int}`) is skipped by Pennington's automatic Razor route discovery. Emit each paginated route explicitly so the static build crawls them.

```csharp
public sealed class ArticleListingContentService(
    IEnumerable<IContentService> services) : IContentService
{
    public string DefaultSectionLabel => "";
    public int SearchPriority => 0;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var count = 0;
        await foreach (var item in services.DiscoverAllAsync())
        {
            if (item.Source.Value is MarkdownFileSource &&
                item.Route.CanonicalPath.Value.StartsWith("/articles/"))
            {
                count++;
            }
        }

        const int pageSize = 20;
        var componentType = typeof(ArticlesPage);
        ContentSource source = new RazorPageSource(componentType.AssemblyQualifiedName!);
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);
        for (var page = 2; page <= totalPages; page++)
        {
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    CanonicalPath = new UrlPath($"/articles/page/{page}/"),
                    OutputFile = new FilePath($"articles/page/{page}/index.html"),
                },
                source);
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);
}
```

Register it alongside the markdown source:

```csharp
builder.Services.AddTransient<IContentService, ArticleListingContentService>();
```

## What ends up where

- **Sitemap.** Paginated routes appear in `sitemap.xml` automatically — they come from `DiscoverAsync` as HTML routes and `SitemapService` includes everything that isn't a redirect, endpoint, or llms-only sidecar.
- **Search index and llms.txt.** Excluded by default: `BlogSiteContentService` and the custom service above return empty `GetContentTocEntriesAsync()`, so their routes never enter the search or llms paths. If a custom service does emit TOC entries for paginated routes, set `ExcludeFromSearch = true` and `ExcludeFromLlms = true` on those entries.
- **Navigation tree.** Same — paginated routes have no TOC entry, so they don't show in the sidebar or breadcrumbs.

## Related

- Reference: [`BlogSiteOptions.PostsPerPage`](xref:reference.api.blog-site-options)
- Background: [Content pipeline overview](xref:explanation.core.content-pipeline)
- Extensibility: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
