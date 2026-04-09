---
title: "Adding Razor Pages with Content Metadata"
description: "Use @page Razor components alongside markdown content via RazorPageContentService and AdditionalRoutingAssemblies — covering navigation participation, search integration, sidecar metadata files, and trailing-slash conventions"
uid: "penn.how-to.adding-razor-pages-with-content-metadata"
order: 50
---

## Beat 1: The Problem — Interactive Pages in a Markdown-First Site

Introduce the scenario: Forge's documentation is primarily markdown, but the team needs an interactive architecture diagram page with clickable SVG nodes and JavaScript-driven tooltips. Markdown cannot support this level of interactivity. The page should appear in the sidebar, participate in search results, and feel like a native part of the documentation site.

### What to show
- The gap: `T:Pennington.Content.MarkdownContentService`1` discovers `.md` files and feeds them through the content pipeline, but an interactive page needs full Razor/HTML/JS capabilities
- The solution: `@page` Razor components alongside markdown, integrated via `T:Pennington.Content.RazorPageContentService` at `:path:src/Pennington/Content/RazorPageContentService.cs`

### Key points
- Pennington treats markdown and Razor pages as equal citizens in the content pipeline — both implement `T:Pennington.Content.IContentService`
- `T:Pennington.Content.RazorPageContentService` scans assemblies for `@page` components and feeds them into the same navigation, search, and sitemap pipeline as markdown content
- This enables a hybrid approach: use markdown for content-heavy pages, use Razor for interactive or programmatic pages

## Beat 2: How RazorPageContentService Discovers Pages

Walk through the discovery mechanism so the reader understands what happens behind the scenes.

### What to show
- `T:Pennington.Content.RazorPageContentService` at `:path:src/Pennington/Content/RazorPageContentService.cs`:
  - Constructor receives `Assembly[]` (the assemblies to scan), `IFileSystem`, `FrontMatterParser`, and `ILogger`
  - The private `BuildComponentMetadataCache` method scans assemblies for types inheriting `ComponentBase` that have non-parameterized `[RouteAttribute]` routes
  - For each matching type, it calls `TryLoadMetadata` to look for a sidecar `.razor.metadata.yml` file
  - The result is cached in `_componentMetadataCache` as `List<ComponentWithMetadata>` — a private record of `(Type Component, List<ContentRoute> Routes, DocFrontMatter? Metadata)`
- `M:Pennington.Content.IContentService.DiscoverAsync` implementation:
  - Iterates the component metadata cache
  - Skips components where metadata implements `IDraftable { IsDraft: true }`
  - For each route, yields a `T:Pennington.Pipeline.DiscoveredItem` with a `T:Pennington.Pipeline.RazorPageSource` (one of the `T:Pennington.Pipeline.ContentSource` union cases)
- Route creation via `M:Pennington.Routing.ContentRouteFactory.FromRazorPage(System.String,System.String)` — converts the `@page` template string into a `T:Pennington.Routing.ContentRoute` with `CanonicalPath` and `OutputFile`

### Key points
- Parameterized routes (containing `{`) are skipped — `RazorPageContentService` only handles static routes
- The assembly scan happens once at startup and is cached
- The private `_razorFileCache` maps component names to their `.razor` file paths on disk — this is how the service locates sidecar metadata files

## Beat 3: Create the Razor Component with @page

Build an `ArchitectureDiagram.razor` file with the `@page` directive and interactive content.

### What to show
- File: `Components/Pages/ArchitectureDiagram.razor`
- `@page "/guides/architecture/"` — note the **trailing slash**, which is required by Pennington's routing convention
- `@layout DocSiteLayout` — use the site's shared layout for visual consistency with markdown pages
- Component body: an SVG-based architecture diagram with clickable nodes that link to doc pages, plus a `<script>` block for hover tooltips
- ~50 lines of markup + 15 lines of JavaScript

### Key points
- The trailing slash is critical: `T:Pennington.Content.RazorPageContentService` tracks pages missing trailing slashes via `P:Pennington.Content.RazorPageContentService.MissingTrailingSlashPages` — this is a diagnostic aid, not enforcement, but inconsistent URLs cause routing issues
- `M:Pennington.Routing.ContentRouteFactory.FromRazorPage(System.String,System.String)` calls `EnsureTrailingSlash()` on the canonical path, so `/guides/architecture` becomes `/guides/architecture/` internally — but the `@page` directive should match to avoid confusion
- The `@layout` directive ensures the Razor page renders inside the same chrome as markdown pages

## Beat 4: Create the Sidecar Metadata File

Add a `.razor.metadata.yml` file alongside the component to provide front matter for navigation, search, and sitemap.

### What to show
- File: `Components/Pages/ArchitectureDiagram.razor.metadata.yml` (placed in the same directory as the `.razor` file)
- YAML content mapping to `T:Pennington.FrontMatter.DocFrontMatter` at `:path:src/Pennington/FrontMatter/DocFrontMatter.cs`:
  ```yaml
  title: "Architecture Diagram"
  description: "Interactive view of Forge's service architecture"
  order: 30
  section: "Guides"
  uid: "forge.architecture"
  tags: ["architecture", "overview"]
  ```
- Show how each field maps to `T:Pennington.FrontMatter.DocFrontMatter` capabilities:
  - `title` -> `P:Pennington.FrontMatter.IFrontMatter.Title` (required by `T:Pennington.FrontMatter.IFrontMatter`)
  - `description` -> `P:Pennington.FrontMatter.IDescribable.Description`
  - `order` -> `P:Pennington.FrontMatter.IOrderable.Order` (controls sort position in sidebar)
  - `section` -> `P:Pennington.FrontMatter.ISectionable.Section` (which navigation section to appear in)
  - `uid` -> `P:Pennington.FrontMatter.ICrossReferenceable.Uid` (enables `xref:forge.architecture` links)
  - `tags` -> `P:Pennington.FrontMatter.ITaggable.Tags`
- The private `GetSidecarFilePath` method in `T:Pennington.Content.RazorPageContentService` resolves the metadata file: it looks up the component name in `_razorFileCache`, then checks for `{ComponentName}.razor.metadata.yml` in the same directory

### Key points
- The sidecar file is optional — without it, the Razor page routes correctly but does not participate in navigation, search, or sitemap
- The filename convention is strict: `{ComponentName}.razor.metadata.yml` — not `.yaml`, not without the `.razor.` prefix
- `T:Pennington.FrontMatter.DocFrontMatter` implements multiple capability interfaces (`T:Pennington.FrontMatter.IDraftable`, `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.ISectionable`, `T:Pennington.FrontMatter.ICrossReferenceable`, `T:Pennington.FrontMatter.IOrderable`, `T:Pennington.FrontMatter.IDescribable`) — each field activates a pipeline feature

## Beat 5: Register the Assembly via AdditionalRoutingAssemblies

Configure `T:Pennington.Infrastructure.PenningtonOptions` to scan the assembly containing the Razor component.

### What to show
- In `Program.cs`, inside the `AddPennington` callback:
  ```csharp
  options.AdditionalRoutingAssemblies = [typeof(ArchitectureDiagram).Assembly];
  ```
- Show the `P:Pennington.Infrastructure.PenningtonOptions.AdditionalRoutingAssemblies` property at `:path:src/Pennington/Infrastructure/PenningtonOptions.cs`: `Assembly[] AdditionalRoutingAssemblies { get; set; } = []`
- Show the registration logic in `M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})` at `:path:src/Pennington/Infrastructure/PenningtonExtensions.cs`: when `AdditionalRoutingAssemblies.Length > 0`, a `T:Pennington.Content.RazorPageContentService` is registered as `IContentService` with those assemblies
- The `RazorPageContentService` is registered as a singleton — the assembly scan and metadata cache are built once at startup

### Key points
- If the Razor page is in the same assembly as the host project, you still need to register it: `typeof(Program).Assembly` or `Assembly.GetExecutingAssembly()`
- Multiple assemblies can be registered: `[typeof(ArchitectureDiagram).Assembly, typeof(AnotherPage).Assembly]`
- The service is only registered when `AdditionalRoutingAssemblies` is non-empty — there is no overhead if you do not use Razor content pages

## Beat 6: Navigation Integration — How the Page Appears in the Sidebar

Walk through how `T:Pennington.Content.RazorPageContentService` contributes to the navigation tree.

### What to show
- `M:Pennington.Content.RazorPageContentService.GetContentTocEntriesAsync` at `:path:src/Pennington/Content/RazorPageContentService.cs`:
  - Iterates `_componentMetadataCache`, skips entries with null metadata or `IsDraft: true`
  - Builds `T:Pennington.Content.ContentTocItem` from the metadata:
    - `Title` from `P:Pennington.FrontMatter.IFrontMatter.Title`
    - `Route` from the first discovered route
    - `Order` from `P:Pennington.FrontMatter.IOrderable.Order` (defaults to `int.MaxValue` if not set)
    - `HierarchyParts` derived from the canonical path: `/guides/architecture/` becomes `["guides", "architecture"]`
    - `Section` from `P:Pennington.FrontMatter.ISectionable.Section` (falls back to `P:Pennington.Content.IContentService.DefaultSection` which is `""`)
- `M:Pennington.Content.RazorPageContentService.GetCrossReferencesAsync` — registers `T:Pennington.Pipeline.CrossReference` entries for pages with a non-null `P:Pennington.FrontMatter.ICrossReferenceable.Uid`

### Key points
- `HierarchyParts` determines sidebar nesting: the path `/guides/architecture/` naturally nests under "guides" because `HierarchyParts = ["guides", "architecture"]`
- `Order: 30` from the sidecar file places the architecture page at position 30 among its siblings in the guides section
- Pages without sidecar metadata are routable (the `@page` directive works) but invisible in navigation — they are "unlisted" pages

## Beat 7: Search and Sitemap Participation

Explain how the metadata enables search indexing and sitemap generation.

### What to show
- Search: Pennington's `T:Pennington.Search.SearchIndexService` queries all `T:Pennington.Content.IContentService` implementations for TOC entries. The `title`, `description`, and `tags` from the sidecar metadata feed the search index. `P:Pennington.Content.IContentService.SearchPriority` on `RazorPageContentService` is `5` — lower than markdown's typical `10`, but the page still appears in search
- Sitemap: Pennington's `T:Pennington.Feeds.SitemapService` similarly discovers all routes from all content services. The architecture page appears in `sitemap.xml` at its canonical URL
- Cross-references: other pages can link with `xref:forge.architecture` because `GetCrossReferencesAsync` registered the UID

### Key points
- Without the sidecar metadata file, the page does not appear in search results or the sitemap — the `GetContentTocEntriesAsync` method skips pages with null metadata
- `SearchPriority` of `5` means Razor pages rank below markdown pages in search results by default — this is configurable if you create a custom `RazorPageContentService` subclass

## Beat 8: The Trailing-Slash Convention

Explain why trailing slashes matter and how Pennington handles inconsistencies.

### What to show
- Pennington uses a trailing-slash URL convention: every content page's canonical URL ends with `/` (e.g., `/guides/architecture/` not `/guides/architecture`)
- `M:Pennington.Routing.ContentRouteFactory.FromRazorPage(System.String,System.String)` always calls `EnsureTrailingSlash()` on the canonical path
- `P:Pennington.Content.RazorPageContentService.MissingTrailingSlashPages` — a diagnostic property that collects `@page` templates missing trailing slashes: `IReadOnlyList<(string Template, string TypeName)>`
- If the `@page` directive uses `/guides/architecture` (no trailing slash), the component still works — Pennington normalizes the route — but the `@page` template and the canonical URL will mismatch, which can cause confusion with Blazor's own routing

### Key points
- Always use trailing slashes in `@page` directives: `@page "/guides/architecture/"` not `@page "/guides/architecture"`
- The static build generates `guides/architecture/index.html` — the trailing slash convention ensures clean URLs on static hosting
- Pennington logs a warning for pages missing trailing slashes to help catch this during development

## Beat 9: Limitations — Parameterized Routes

Document the key limitation of `T:Pennington.Content.RazorPageContentService`.

### What to show
- In the `BuildComponentMetadataCache` method: `if (template.Contains('{')) continue;` — any route with parameters is skipped entirely
- A `@page "/items/{id}"` component routes correctly via Blazor's router (the `@page` directive still works for HTTP requests), but it will NOT:
  - Appear in sidebar navigation
  - Be indexed for search
  - Appear in the sitemap
  - Be generated during static builds (no known set of parameter values)
  - Support xref linking

### Key points
- Parameterized routes are inherently unbounded — Pennington cannot enumerate all possible URLs to build static pages or navigation entries
- For pages with a finite set of parameter values, consider creating individual `@page` components for each value, or use `T:Pennington.Content.IContentService` with `T:Pennington.Pipeline.ProgrammaticSource` to generate them programmatically
- The Blazor routing for parameterized pages still works in dev mode (`dotnet run`) — the limitation only affects Pennington's content pipeline features
