---
title: "Using Multiple Content Sources"
description: "Register multiple AddMarkdownContent<T> calls with different front matter types, content paths, base URLs, and sections, and understand how the pipeline merges items from all sources"
uid: "penn.how-to.using-multiple-content-sources"
order: 30
---

## Beat 1: The problem -- different content types need different metadata

The reader understands why a single front matter type cannot serve all content. Documentation pages need `Order` for sidebar sorting. Blog posts need `Date` for chronological listing. Landing pages need only a title. Trying to cram all these into one type leads to unused fields and confusing YAML.

### What to show
- Show `T:Penn.FrontMatter.IFrontMatter` -- the minimal interface with just `P:Penn.FrontMatter.IFrontMatter.Title`
- Show the capability interfaces that add optional metadata: `T:Penn.FrontMatter.IOrderable` (`P:Penn.FrontMatter.IOrderable.Order`), `T:Penn.FrontMatter.IDateable` (`P:Penn.FrontMatter.IDateable.Date`), `T:Penn.FrontMatter.IDraftable` (`P:Penn.FrontMatter.IDraftable.IsDraft`), `T:Penn.FrontMatter.ITaggable` (`P:Penn.FrontMatter.ITaggable.Tags`), `T:Penn.FrontMatter.ISectionable` (`P:Penn.FrontMatter.ISectionable.Section`), `T:Penn.FrontMatter.ICrossReferenceable` (`P:Penn.FrontMatter.ICrossReferenceable.Uid`), `T:Penn.FrontMatter.IDescribable` (`P:Penn.FrontMatter.IDescribable.Description`), `T:Penn.FrontMatter.IRedirectable` (`P:Penn.FrontMatter.IRedirectable.RedirectUrl`)

### Key points
- `IFrontMatter` is the only required interface -- all capability interfaces are opt-in via composition
- The pipeline uses pattern matching on these interfaces: `IDraftable` items with `IsDraft = true` are skipped during generation, `IOrderable` items sort by `Order` in the TOC, `ICrossReferenceable` items register xref entries
- This design means each content source can implement exactly the capabilities it needs -- no wasted fields, no confusion

---

## Beat 2: Define the front matter types

The reader uses Penn's built-in `DocFrontMatter` and `BlogFrontMatter` for docs and blog, then creates a minimal custom `PageFrontMatter` for landing pages.

### What to show
- Show `T:Penn.FrontMatter.DocFrontMatter` -- implements `IFrontMatter`, `IDraftable`, `ITaggable`, `ISectionable`, `ICrossReferenceable`, `IOrderable`, `IDescribable`. Properties: `P:Penn.FrontMatter.DocFrontMatter.Title`, `P:Penn.FrontMatter.DocFrontMatter.Description`, `P:Penn.FrontMatter.DocFrontMatter.IsDraft`, `P:Penn.FrontMatter.DocFrontMatter.Tags`, `P:Penn.FrontMatter.DocFrontMatter.Section`, `P:Penn.FrontMatter.DocFrontMatter.Uid`, `P:Penn.FrontMatter.DocFrontMatter.Order`
- Show `T:Penn.FrontMatter.BlogFrontMatter` -- implements `IFrontMatter`, `IDraftable`, `ITaggable`, `IDescribable`, `IDateable`, `ICrossReferenceable`. Properties: `P:Penn.FrontMatter.BlogFrontMatter.Title`, `P:Penn.FrontMatter.BlogFrontMatter.Description`, `P:Penn.FrontMatter.BlogFrontMatter.IsDraft`, `P:Penn.FrontMatter.BlogFrontMatter.Tags`, `P:Penn.FrontMatter.BlogFrontMatter.Date`, `P:Penn.FrontMatter.BlogFrontMatter.Author`, `P:Penn.FrontMatter.BlogFrontMatter.Series`, `P:Penn.FrontMatter.BlogFrontMatter.Uid`
- Create a custom minimal type:
  ```csharp
  public record PageFrontMatter : IFrontMatter
  {
      public string Title { get; init; } = "";
  }
  ```

### Key points
- `DocFrontMatter` has `IOrderable` but not `IDateable` -- docs sort by explicit order, not publication date
- `BlogFrontMatter` has `IDateable` but not `IOrderable` -- posts sort by date, not manual ordering
- `PageFrontMatter` implements only `IFrontMatter` -- the pipeline treats it as unordered, undated, non-draftable content
- Both built-in types are in the `Penn.FrontMatter` namespace; custom types can live anywhere as long as they implement `IFrontMatter`
- `DocFrontMatter.Order` defaults to `int.MaxValue` so pages without an explicit order sort last

---

## Beat 3: Register three content sources

The reader calls `AddMarkdownContent<T>` three times in `Program.cs`, each with a different front matter type, content path, base URL, and section. The `BasePageUrl` property prevents URL collisions.

### What to show
- Show `M:Penn.Infrastructure.PennOptions.AddMarkdownContent``1(System.Action{Penn.Infrastructure.MarkdownContentOptions})` -- the generic method that registers a markdown content source
- Show `T:Penn.Infrastructure.MarkdownContentOptions` with its properties: `P:Penn.Infrastructure.MarkdownContentOptions.ContentPath`, `P:Penn.Infrastructure.MarkdownContentOptions.BasePageUrl`, `P:Penn.Infrastructure.MarkdownContentOptions.Section`
- Register three sources in `Program.cs`:
  ```csharp
  services.AddPenn(penn =>
  {
      penn.SiteTitle = "Forge";
      penn.SiteDescription = "Internal Developer Portal";

      penn.AddMarkdownContent<DocFrontMatter>(md =>
      {
          md.ContentPath = "Content/docs";
          md.BasePageUrl = "/docs";
          md.Section = "Documentation";
      });

      penn.AddMarkdownContent<BlogFrontMatter>(md =>
      {
          md.ContentPath = "Content/blog";
          md.BasePageUrl = "/blog";
          md.Section = "Blog";
      });

      penn.AddMarkdownContent<PageFrontMatter>(md =>
      {
          md.ContentPath = "Content/pages";
          md.BasePageUrl = "/";
      });
  });
  ```
- Show how `P:Penn.Infrastructure.PennOptions.MarkdownSources` accumulates the sources as `IReadOnlyList<MarkdownContentOptions>`

### Key points
- `ContentPath` is the filesystem directory containing the markdown files -- it is resolved relative to the host's `ContentRootPath` at runtime
- `BasePageUrl` is the URL prefix for all pages from this source -- a file at `Content/docs/getting-started.md` with `BasePageUrl = "/docs"` maps to `/docs/getting-started/`
- `Section` is an optional label used in the navigation tree TOC -- pages from this source appear under this section heading
- URL collision prevention: because the docs source uses `/docs` and the blog uses `/blog`, both can have a `getting-started.md` without conflict (`/docs/getting-started/` vs `/blog/getting-started/`)
- The pages source uses `BasePageUrl = "/"` so `Content/pages/about.md` maps to `/about/`
- `Section` is not set for the pages source, so its pages appear without a section heading in navigation
- Note: `T:Penn.Content.MarkdownContentServiceOptions` is a public class (despite being an implementation detail) — it wraps the config-time `MarkdownContentOptions` for the runtime service

---

## Beat 4: Create content with cross-source xref links

The reader creates 2 doc pages, 2 blog posts, and 1 landing page. One doc and one blog post have `Uid` values set, enabling cross-source `xref:` link resolution.

### What to show
- Create `Content/docs/getting-started.md` with `uid: "forge.getting-started"` and an xref link in the body: `[see the announcement](xref:forge.blog.welcome)`
- Create `Content/docs/api-keys.md` with `uid: "forge.api-keys"` and `order: 20`
- Create `Content/blog/welcome.md` with `uid: "forge.blog.welcome"`, `date: 2026-03-01`, `tags: ["announcements"]`, and an xref link: `[getting started guide](xref:forge.getting-started)`
- Create `Content/blog/q1-retro.md` with `date: 2026-04-01` and `tags: ["team"]` (no uid)
- Create `Content/pages/about.md` with just `title: "About Forge"`
- Show how `T:Penn.Content.MarkdownContentService``1` discovers files and registers cross-references via `M:Penn.Content.MarkdownContentService``1.GetCrossReferencesAsync` -- it reads `ICrossReferenceable.Uid` from front matter and creates `T:Penn.Content.CrossReference` entries

### Key points
- `xref:` links resolve across all registered content sources -- a blog post can link to a doc page by UID and vice versa
- The `Uid` field comes from `T:Penn.FrontMatter.ICrossReferenceable` -- only front matter types that implement this interface can participate in cross-referencing
- `PageFrontMatter` does not implement `ICrossReferenceable`, so landing pages cannot be xref targets (they can still use xref links to reference other content)
- The xref resolver is registered in `M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})` and operates as an `T:Penn.Infrastructure.IResponseProcessor` that rewrites xref links in rendered HTML

---

## Beat 5: Verify the merged pipeline

The reader runs the site and confirms that the content pipeline merges all three sources: the navigation tree shows docs under their section, blog posts under theirs, and landing pages at the root. Cross-source xref links resolve correctly.

### What to show
- Show `T:Penn.Pipeline.ContentPipeline` and its `M:Penn.Pipeline.ContentPipeline.DiscoverAsync` method -- it iterates over all registered `T:Penn.Content.IContentService` instances and yields `DiscoveredItem` entries from each
- Show that each `AddMarkdownContent<T>` call registers a separate `T:Penn.Content.MarkdownContentService``1` as `IContentService` -- the pipeline aggregates them all
- Show `M:Penn.Content.MarkdownContentService``1.GetContentTocEntriesAsync` -- each service produces `T:Penn.Content.ContentTocItem` entries with the `Section` from its options, and `Order` from the front matter's `IOrderable` implementation
- Show how `M:Penn.Pipeline.ContentPipeline.GenerateAsync(System.Collections.Generic.IAsyncEnumerable{Penn.Pipeline.ContentItem},Penn.Generation.OutputOptions)` processes items from all sources uniformly: it checks `IDraftable` to skip drafts, collects warnings for missing trailing slashes, and reports errors
- Verify the navigation tree: docs appear under "Documentation" section sorted by `Order`, blog posts appear under "Blog" section sorted by `Date`, and the about page appears at `/about/` without a section heading

### Key points
- The pipeline is source-agnostic -- `ContentPipeline.DiscoverAsync` simply iterates `IEnumerable<IContentService>` and yields everything. There is no special merging logic; aggregation happens naturally because multiple `IContentService` registrations are resolved by DI
- Each `MarkdownContentService<T>` is an independent `IContentService` with its own content path, base URL, and front matter type
- `ContentTocItem` entries carry the `Section` label so the navigation builder can group them -- this is how "Documentation" and "Blog" appear as separate sidebar sections
- Cross-references merge across all sources because each `IContentService.GetCrossReferencesAsync` contributes to a single shared xref resolver
- Static file serving is also configured per-source: `UsePenn` maps `UseStaticFiles` for each `MarkdownContentOptions.ContentPath` at its `BasePageUrl` request path, so images and media in each content directory are served correctly

---

## Beat 6: Understand search priority across sources

The reader learns how `SearchPriority` on the internal `MarkdownContentServiceOptions` controls ranking when search terms match content from multiple sources. Doc pages rank higher than blog posts by default.

### What to show
- Show `P:Penn.Content.MarkdownContentServiceOptions.SearchPriority` -- defaults to `10` for markdown sources
- Show `P:Penn.Content.IContentService.SearchPriority` on the interface
- Show the different default priorities: `T:Penn.Content.MarkdownContentService``1` uses `10` (from `MarkdownContentServiceOptions`), `T:Penn.Content.RazorPageContentService` uses `5`, `T:Penn.Islands.SpaNavigationContentService` uses `0`, `T:Penn.LlmsTxt.LlmsTxtContentService` uses `0`
- Explain that `SearchPriority` is a ranking hint used by the search index builder -- higher values mean the content ranks higher when relevance scores are equal

### Key points
- All markdown content sources currently share the same default `SearchPriority` of `10` -- to differentiate, the `MarkdownContentServiceOptions.SearchPriority` would need to be set per-source (this is on the internal options object `T:Penn.Content.MarkdownContentServiceOptions`, not directly on the public `T:Penn.Infrastructure.MarkdownContentOptions`)
- The search index at `/search-index.json` includes content from all sources, with `SearchPriority` influencing result ordering
- Razor pages have a lower default priority (`5`) than markdown content (`10`), and infrastructure services (SPA navigation, llms.txt) have priority `0` to stay out of user-facing search results
- The `T:Penn.Search.SearchIndexBuilder` consumes `IContentService.SearchPriority` when building the client-side search index
