# Penn Documentation — Table of Contents

Organized using the [Diataxis framework](https://diataxis.fr/). Penn core is the primary subject; DocSite and BlogSite are "headstart" packages.

---

## Tutorials

Step-by-step walkthroughs that produce a working result. Ordered from simple → complex.

### Getting Started

- **Building a Site from Scratch** — Create a new ASP.NET project, wire `AddPenn`/`UsePenn`/`RunOrBuildAsync`, create markdown content with YAML front matter, build a Razor layout, run with `dotnet watch`. No DocSite — raw Penn core.
- **DocSite Quick Start** — `AddDocSite`/`UseDocSite`/`RunDocSiteAsync` in ~20 lines. Covers `DocSiteOptions`. Produces a polished site with search, SPA nav, outline, breadcrumbs.
- **BlogSite Quick Start** — Same pattern for `Penn.BlogSite`. Posts with dates, tags, series. Automatic archives/tags pages and RSS.
- **Deploying to GitHub Pages** — `dotnet run build`, output structure, GitHub Actions workflow, base URL for subdirectory hosting.

### Customization

- **Creating a Custom Front Matter Type** — Implement a custom record with `IFrontMatter` + capability interfaces. Register with `AddMarkdownContent<T>`. YAML mapping, pipeline consumption via pattern matching.
- **Styling Your Site with MonorailCSS** — Add `Penn.MonorailCss`. `NamedColorScheme` vs `AlgorithmicColorScheme`, 5 semantic color roles, dark mode, custom fonts, `ExtraStyles`.

### Advanced Features

- **Adding Localization** — Take a single-locale site, add two locales. `LocalizationOptions`, locale subdirectories, fallback behavior, `LanguageSwitcher` component.
- **Adding SPA Navigation with Islands** — Convert a static site to SPA navigation. `IIslandRenderer`, `RazorIslandRenderer<T>`, `data-spa-*` attributes, SPA envelope, loading strategies.

---

## How-to Guides

Recipes for specific tasks. Assume a working Penn site and basic knowledge.

### Content Authoring

- **Writing Markdown with Penn Extensions** — Code highlighting, tabbed code blocks, line directives (highlight, diff, focus, error, warning, word), snippet regions, alerts, Mermaid diagrams.
- **Linking and Cross-References** — Relative links, media assets, `xref:uid` syntax, assigning UIDs via `ICrossReferenceable`, resolution behavior, broken xref diagnostics.
- **Working with Drafts, Tags, and Ordering** — `IsDraft`, `Tags`, `Order`, `Section` front matter capabilities and how the pipeline uses them.
- **Implementing Redirects** — `IRedirectable` and `RedirectUrl`. Meta-refresh output in static builds.

### Configuration

- **Configuring DocSite** — Exhaustive `DocSiteOptions` coverage: content areas, Roslyn, additional assemblies, header/footer, social image, fonts.
- **Configuring BlogSite** — Exhaustive `BlogSiteOptions`: blog paths, author info, hero, socials, projects, RSS/sitemap toggles, social image factory. Series, archives, tag pages.
- **Using Multiple Content Sources** — Multiple `AddMarkdownContent<T>` calls with different front matter types, paths, sections. How the pipeline merges sources.
- **Customizing the CSS Framework** — `CustomCssFrameworkSettings`, `ContentPaths`, `ExtraStyles`, `CssClassCollector` behavior (runtime vs. startup scanning).

### Navigation & Search

- **Controlling Navigation and TOC** — Navigation tree algorithm, ordering (explicit > inherited > alphabetical), section headers, `ContentArea`, breadcrumbs, prev/next.
- **Adding Client-Side Search** — `SearchIndexService`, `/search-index.json`, FlexSearch, search priority weighting, keyboard shortcut, lazy loading.

### SEO & Feeds

- **RSS, Sitemap, and Structured Data** — `RssFeedBuilder`, `SitemapBuilder`, `CanonicalBaseUrl`, JSON-LD via `StructuredData` component, `AddLlmsTxt()`.

### Extending Penn

- **Building a Custom Content Service** — Implement `IContentService` (5 methods + 2 properties). Non-markdown sources (database, API, CMS). DI registration.
- **Adding a Custom Code Highlighter** — Implement `ICodeHighlighter`. Priority-based dispatch. Registration via `Highlighting.AddHighlighter<T>()`.
- **Building Custom Island Renderers** — `IIslandRenderer` vs `RazorIslandRenderer<T>`. `BuildParametersAsync`. `SpaPageDataService` coordination.
- **Writing a Custom Response Processor** — `IResponseProcessor`: Order, ShouldProcess, ProcessAsync. Middleware capture pattern. Use cases.
- **Adding Razor Pages with Content Metadata** — `@page` directives, `RazorPageContentService`, `AdditionalRoutingAssemblies`, navigation/search/sitemap participation.

### Roslyn Integration

- **Connecting to a Roslyn Workspace** — `Penn.Roslyn`, `RoslynOptions`, `ProjectFilter`. Modifiers: `:xmldocid`, `:xmldocid,bodyonly`, `:path`.
- **Showing Code Diffs** — `:xmldocid-diff` modifier, DiffPlex integration, before/after API comparison.

### Deployment

- **Deploying to a Subdirectory** — `BaseUrlRewritingProcessor`, `OutputOptions.BaseUrl`, CLI args, local testing.
- **Static Generation and Build Reports** — 9-phase generation, `BuildReport` (pages, errors, warnings, broken links), CI exit codes.

---

## Reference

Austere, accurate, comprehensive. Describes what things are, not how to use them.

### Configuration API

- **PennOptions** — All properties: SiteTitle, SiteDescription, CanonicalBaseUrl, ContentRootPath, HighlightingOptions, IslandsOptions, LocalizationOptions, TranslationOptions, AddMarkdownContent, AddLlmsTxt, AdditionalRoutingAssemblies.
- **DocSiteOptions** — All properties and ContentArea record.
- **BlogSiteOptions** — All properties and supporting records (SocialLink, HeaderLink, Project, HeroContent).
- **MonorailCssOptions** — ColorScheme, CustomCssFrameworkSettings, ExtraStyles, ContentPaths. NamedColorScheme, AlgorithmicColorScheme, IColorScheme.

### Front Matter

- **Front Matter System** — IFrontMatter + 8 capability interfaces. Property types, YAML conventions. Built-in types comparison table.

### Content Pipeline

- **Pipeline Stages and Types** — ContentItem union (4 cases), ContentSource union (4 cases), IContentPipeline, IContentParser, IContentRenderer, RenderedContent, ContentError.
- **Content Services** — IContentService interface (5 methods). MarkdownContentService, RazorPageContentService, LlmsTxtContentService, SpaNavigationContentService. Supporting types.

### Routing

- **Routing Types** — UrlPath, FilePath, ContentRoute, ContentRouteFactory. Operators, matching, normalization.

### Markdown

- **Code Block Directives** — Complete syntax for all line directives, snippet regions, comment markers, CSS classes, tabbed blocks.
- **Alert Types** — The 5 alert types, HTML output, CSS classes, GitHub compatibility.

### Navigation

- **Navigation Types** — NavigationBuilder, NavigationTreeItem, NavigationInfo, BreadcrumbItem, ContentTocItem.

### Search

- **Search Index Format** — SearchIndexDocument record, JSON schema, SearchIndexBuilder, SearchIndexService.

### Localization

- **Localization API** — LocalizationOptions, LocaleInfo, LocaleContext, TranslationOptions, AlternateLanguage, middleware behavior.

### Islands & SPA

- **Islands and SPA API** — IIslandRenderer, RazorIslandRenderer, ComponentRenderer, SpaPageDataService, SpaEnvelope/Dto, data-spa-* attributes, JS events.

### Response Processing

- **Response Processor Chain** — IResponseProcessor interface, middleware behavior, all 6 built-in processors in order, diagnostic headers.

### Generation

- **Static Generation** — OutputGenerationService 9 phases, OutputOptions, BuildReport/Builder, CLI format.

### Feeds & SEO

- **Feeds and Structured Data** — RssFeedBuilder, SitemapBuilder, JSON-LD types, JsonLdSerializer, LlmsTxtOptions.

### UI Components

- **UI Component Library** — Complete parameter reference for all Penn.UI components: Badge, Card, CardGrid, LinkCard, BigTable, CodeBlock, Steps/Step, TableOfContentsNavigation, OutlineNavigation, LanguageSwitcher, StructuredData, FallbackNotice.

### Roslyn

- **Roslyn API and XmlDocId Format** — RoslynOptions, ProjectFilter, code block modifiers, XmlDocId format specification (prefixes, generics, nested types, constructors).

### Infrastructure

- **File Watching and DI Wiring** — IFileWatcher, FileWatchDependencyFactory, AddFileWatched, AddPenn/UsePenn/RunOrBuildAsync details, service lifetimes, live reload protocol.

---

## Explanation

Illuminates the "why" — architecture, design decisions, trade-offs. No step-by-step instructions.

### Core Architecture

- **The Content Processing Pipeline** — Why a 4-stage streaming pipeline. Union-based error handling vs exceptions. IAsyncEnumerable streaming. ContentService vs ContentPipeline roles.
- **Dev Mode vs Build Mode** — Dual-personality architecture. HTTP self-crawl for static generation. Response processor parity. Phase ordering (HTML before CSS).
- **The Front Matter Capability System** — Why capability interfaces over inheritance or a single type. Pattern matching composition. YAML deserialization mechanics.

### Rendering & Theming

- **Syntax Highlighting Architecture** — Priority-based dispatch. Server-side vs client-side trade-offs. TextMate vs Roslyn. Preprocessor hooks. Transformation pipeline.
- **The MonorailCSS Integration** — Utility-first CSS rationale. Two collection strategies (runtime vs startup). OKLCH palette generation algorithm. Dark mode mechanism.

### SPA & Islands

- **SPA Navigation and Island Architecture** — SPA-without-WASM design. Server-rendered HTML fragments. SpaEnvelope contract. Client lifecycle. Loading strategies. View transitions. xref resolution in SPA mode.

### Hot Reload

- **Hot Reload and File Watching** — .NET hot reload vs Penn live reload. Invalidate-and-recreate pattern. FileWatchDependencyFactory services. WebSocket protocol. AsyncLazy.

### Navigation

- **Table of Contents Generation** — Tree assembly from flat lists. Hierarchy inference. Auto-created folder nodes. Sort algorithm. Locale filtering. Prev/next flattening. Breadcrumb path finding.

### Client-Side

- **JavaScript Architecture** — Minimal-JS philosophy. Client modules (SPA, search, live reload, view transitions, clipboard). No bundler. data-spa-* contract.

### Response Processing

- **The Response Processor Pipeline** — Why post-process responses vs modify at render time. Stream capture pattern. Ordering semantics. DiagnosticContext flow.

### Cross-References

- **Cross-Reference Resolution** — Two-phase xref system. Case-insensitive UID lookup. Lazy invalidation. SPA mode resolution differences.

---

## Summary

| Quadrant | Pages | Areas |
|----------|-------|-------|
| Tutorials | 8 | Getting Started, Customization, Advanced Features |
| How-to Guides | 19 | Content Authoring, Configuration, Navigation & Search, SEO & Feeds, Extending Penn, Roslyn, Deployment |
| Reference | 15 | Configuration API, Front Matter, Pipeline, Routing, Markdown, Navigation, Search, Localization, Islands & SPA, Response Processing, Generation, Feeds & SEO, UI Components, Roslyn, Infrastructure |
| Explanation | 10 | Core Architecture, Rendering & Theming, SPA & Islands, Hot Reload, Navigation, Client-Side, Response Processing, Cross-References |
| **Total** | **52** | |
