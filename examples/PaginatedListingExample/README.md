# PaginatedListingExample

Bare `AddPennington` host that puts a paginated `/articles` listing on top of a folder of markdown articles. Backs the custom-content-service half of the pagination how-to.

## Concepts

- A `PagedList<T>` record carrying the page metadata the shared `Pagination` component needs
- A Razor `@page` component (`ArticlesPage`) with two directives — canonical `/articles` plus `/articles/page/{Page:int}`
- An `ArticleResolver` that walks every `IContentService` to collect the `/articles/` markdown pages and slice them into pages
- An `ArticleListingContentService` that emits the numbered `/articles/page/N/` routes so the static build crawls them — registered as `IContentService`, so it resolves its siblings on demand inside `DiscoverAsync` and excludes itself (`!ReferenceEquals(s, this)`) to avoid a DI cycle, the same pattern `SocialCardContentService` uses

Twenty-two sample articles ship under `Content/articles/`, so the listing spans two pages (`/articles` and `/articles/page/2/`).

## Referenced from

- `docs/Pennington.Docs/Content/how-to/discovery/pagination.md`
