# Pennington

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Pennington.slnx`
- Test: `dotnet test Pennington.slnx`
- Single test: `dotnet test Pennington.slnx --filter "FullyQualifiedName~TestName"`
- Run docs site: `dotnet run --project docs/Pennington.Docs`
- CLI verbs (any host): `dotnet run --project <site>` serves; `-- build [--base-url /x] [--output dir]` generates the static site; `-- diag <info|toc|routes|warnings|translation|frontmatter|llms>` runs read-only inspection (text output) for humans and AI assistants — plus DI-discovered verbs from optional packages (`books` from Pennington.Book, `standard-site` when `PenningtonOptions.StandardSite` is configured); `-- diag --help` lists what's registered

## Project Structure
- `src/Pennington/` — Core library (Markdig, SharpYaml, AngleSharp, TextMateSharp)
- `src/Pennington.UI/` — Razor component library (TableOfContentsNav, OutlineNav, Badge, Card, CodeBlock, etc.)
  - `Pennington.UI.Styling` — components style themselves: each renders inline default class literals (in a method body, so an edit hot-reloads) for its slots, optionally selected by a component `Variant` enum (e.g. `TocVariant` Rail/Pill, whose per-slot bases live in `TocVariantStyles.For`). A nullable per-instance `*Class` param Tailwind-merges over the slot base via `ClassMerge` — a thin wrapper over the `Func<string,string,string>` delegate from `MonorailCssService.CreateClassMerger(options)` (conflicts derived from the site's own `CssFramework`). Site templates register one `ClassMerge` singleton; bare hosts leave it unregistered and the component falls back to appending (conflicting base utilities not removed). All class values must be IL string literals for MonorailCss discovery; functional classes the outline script needs (`relative`, `absolute`, `opacity-0`) stay hardcoded in markup. Pennington.UI stays MonorailCss-free (it only sees the delegate). The DocSite sidebar look is `TableOfContentsNavigation`'s `Pill` variant.
- `src/Pennington.MonorailCss/` — MonorailCSS integration (utility-first CSS generation)
- `src/Pennington.DocSite/` — Documentation site template (layout, pages, content resolver)
- `src/Pennington.BlogSite/` — Blog site template (home/archive/tag pages, blog front matter, content service)
  - Both site templates share their blog **data/logic** through core (`BlogPostQuery`, `PagedList<T>`, the `Pennington.StructuredData` JSON-LD types, and `TaxonomyContentService` for browse-by-tag). What stays template-specific is chrome: layouts, `App.razor`, the options record, and the front-matter type (`BlogPostFrontMatter` vs `BlogSiteFrontMatter` — kept separate because their `SearchOnly` policy and `Repository`/`Series` fields differ). Tag pages are `@page` components backed by a registered taxonomy axis (not `MapTaxonomy`), so they keep full chrome + search indexing.
- `src/Pennington.TreeSitter/` — Optional tree-sitter-based multi-language code-fragment extraction (`:symbol` fence, name-path addressing) via the `TreeSitter.DotNet` package
- `src/Pennington.ApiMetadata/` — Backend-neutral API-reference metadata layer: `IApiMetadataProvider` (async type/member/extension-method/xmldoc lookups keyed by uid), the `ApiTypeSummary`/`ApiTypeDetail`/`ApiMember` models, and the `IXmlDocParser`/`IXmlDocHtmlRenderer` seam. No DI entry of its own — backend packages register providers.
- `src/Pennington.ApiMetadata.Reflection/` — Reflection backend reading compiled assemblies: `AddApiMetadataFromCompiledAssembly(name, …)` registers a **keyed singleton** provider per documented library (own `MetadataLoadContext` + xmldoc index; `CompiledAssemblyApiOptions` takes assembly dirs/files).
- `src/Pennington.DocSite.Api/` — DocSite add-on publishing an API-reference content tree from a named metadata provider: `AddApiReference(name, …)` must be called **after `AddDocSite`** (it mutates `SearchIndexOptions.PrefixPriorities` and `DocSiteOptions.AdditionalRoutingAssemblies`). `ApiReferenceContentService` serves the tree (default prefix `/reference/api/`); Mdazor components under `Components/Reference/` inherit `ApiReferenceComponentBase`. Multiple libraries = repeated named registrations.
- `src/Pennington.TranslationAudit/` — Build-time translation freshness auditor (`AddTranslationAudit`): a core `IBuildAuditor` that compares git commit timestamps (libgit2) to classify each translation Up-to-date/Outdated/Missing; surfaces in the dev overlay and build report.
- `src/Pennington.Beck/` — Build-time Beck diagram rendering: `AddPenningtonBeck` registers a `BeckCodeBlockPreprocessor` (`ICodeBlockPreprocessor`, priority 500) that renders ```` ```beck ```` fences to self-animating inline SVG via the pure-C# Beck engine (no client rendering). Each embed gets a fullscreen-zoom lightbox via a head-contributed script + CSS (`BeckZoomHeadContributor`; opt out with `BeckOptions.Zoom = false`). Supports `beck:symbol` file embeds (resolved against `BeckOptions.ContentRoot`), `static`/`scrub` animation flags, and `style=` overrides; hosts tune fonts/measurer/default style via `BeckOptions.RenderOptions`. Fails loud: a malformed fence renders a `.beck-embed--error` box and lands in `DiagnosticContext`. Note: the SVG keys dark mode off `[data-theme]`, which the DocSite toggle does not set — the docs site mirrors its `.dark` class onto the attribute via `AdditionalHtmlHeadContent`.
- `src/Pennington.Templates/` — `dotnet new` template package (`pennington`, `pennington-docs`, `pennington-blog`); no runtime code.
- `src/Pennington.Book/` — Optional PDF book generation: carves the TOC into `BookDefinition(Title, RoutePrefix)` books (or one whole-site book), composes each into a self-contained print document (per-page HTML→markdown→re-render like llms.txt, inline print CSS + vendored paged.js, images as data: URIs), and renders to PDF via PuppeteerSharp/Chromium. Ships as an artifact-tier service (`BookArtifactService` + `BookArtifactContentService` claiming `/pdf/**.pdf` and `/book-preview/**`): core's artifact router serves both in dev, the static build writes the enumerated PDFs through the same resolver (previews stay dev-only — resolvable, never enumerated). Registration is DI-only (`AddPenningtonBook`; no `Use*` call). `diag books` prints the chapters→pages tree. Registers an `IDownloadLinkProvider` (core `Pennington.Navigation`) so host chrome like the DocSite sidebar advertises the PDFs without referencing this Chromium-dependent project.
- `docs/Pennington.Docs/` — The Pennington docs site (Divio-style: tutorials, how-to, reference, explanation); `docs/docs-voice.md` is the writing-voice guide
- `docs/cloudflare/` — `_worker.js`, a Cloudflare Pages advanced-mode worker serving the per-page Markdown twin (`{route}.md`) when a request sends `Accept: text/markdown` (adds `Vary: Accept`); copied into the deployed output by `.github/workflows/deploy-docs.yml`
- `examples/` — Variety of example sites used for reference and verification across scenarios
- `tests/Pennington.Tests/` — Unit tests (xunit.v3, Shouldly)
- `tests/Pennington.IntegrationTests/` — Integration tests (WebApplicationFactory)
- `tests/Pennington.TreeSitter.Tests/` — Tests for the TreeSitter package (resolver/grammar configs, fragment service, render pipeline)
- `tests/Pennington.Book.Tests/` — Tests for the Book package (catalog locale-URL math, composer composition, asset inlining, DI registration; gated Chromium PDF smoke test)
- `tests/Pennington.BeyondCookFormat.Tests/` — Boots `BeyondCookFormatExample` in-memory (WebApplicationFactory) to prove the multi-format content seam: `.cook` recipes discover/parse/render through the same pipeline as markdown

## Key Namespaces (Pennington core)
- `Pennington.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Pennington.FrontMatter` — IFrontMatter, capability interfaces, FrontMatterParser
- `Pennington.Pipeline` — ContentItem/ContentSource unions, ContentPipeline, IContentParser/IContentRenderer; ISiteProjection (the shared rendered-corpus projection every site-wide aggregator folds over — its lifecycle invariant is runtime-enforced via `CorpusFetchScope`: consuming it from a projection-issued page fetch or from inside its own materialization throws instead of deadlocking, see commit b719d73)
- `Pennington.Artifacts` — the corpus-derived artifact tier: `IArtifactContentService` (Claims + ResolveAsync + DiscoverAsync — one byte path serving dev requests AND producing build output), `ArtifactClaim`/`ArtifactClaimShape` (Exact/Prefix/Suffix URL-territory union; Suffix expresses mid-path catch-alls like `**/llms.txt` that no route template can), `ArtifactRouterMiddleware` (the single dev-serving router — claims gate cheaply, resolvers decline to fall through). Artifact services register ONLY under `IArtifactContentService`, never `IContentService`, so request-path discovery walks, sitemap, and the projection's own input can't trigger their (expensive, projection-folding) discovery. Search shards, llms.txt files, book PDFs, well-known files, and consumer artifacts (e.g. robots.txt) all ship through this. `ClaimConflictAuditor` warns on content routes inside a claimed territory; `diag routes` lists claims.
- `Pennington.Content` — IContentService, MarkdownContentService, RazorPageContentService, ContentRecordRegistry; BlogPostQuery (the shared blog read model — listings, pagination, single-post render, and RSS over the cached records; both site templates consume it), PagedList\<T\>, BlogPostRef\<T\>/RenderedBlogPost\<T\>; **IMetaContentService** (marker for a service that derives from the *other* registered services — taxonomy, paginated listings, social cards — which must filter its sibling set through `ContentServiceExtensions.SourceServices()` so meta-services never recurse into each other or a transient copy of themselves); FolderMetadata/FolderMetadataRegistry/IFolderMetadataProvider (folder-level `_meta.yml` sidecars: per-folder title + order + optional llms.txt subtree opt-in)
- `Pennington.Markdown` — MarkdownContentParser/Renderer, MarkdownPipelineFactory, extensions (highlighting, tabs, alerts)
- `Pennington.Highlighting` — ICodeHighlighter, TextMateHighlighter, ShellHighlighter, HighlightingService
- `Pennington.Generation` — BuildReport, OutputGenerationService, OutputOptions
- `Pennington.Navigation` — NavigationBuilder, NavigationTreeItem, NavigationInfo; IDownloadLinkProvider/DownloadLink (DI-discovered downloadable-artifact links — providers return display-ready labels and locale-appropriate URLs keyed to a route prefix; the DocSite sidebar renders every registered provider's links under the matching area's TOC, and `Pennington.Book` registers its catalog through this)
- `Pennington.Localization` — LocalizationOptions (locale config + URL math: GetLocaleFromUrl/StripLocalePrefix/BuildLocaleUrl/GetAlternateLanguages), AlternateLanguage (the one record for a page's other-locale versions — language switcher + hreflang), LocaleContext, LocaleDetectionMiddleware, LocaleLinkHtmlRewriter, PenningtonStringLocalizer, TranslationOptions
- `Pennington.Search` — host adapter over the external **DeweySearch** engine: SearchArtifactService (index build) + SearchArtifactContentService (artifact-tier façade claiming `/search/**.json`) + HeadingSectionExtractor (splits post-pipeline HTML into one section per heading) + SearchIndexBuilder (maps each section onto a `DeweySearch.SearchDocument` — anchor URL, page→heading breadcrumb, open facets), SearchIndexOptions/SearchFacetField (host config). Records are **heading-level** (DocSearch-style): results deep-link to `/page/#heading` and carry crumbs for grouping. The engine (tokenizer/stemmer/inverted index) is the `DeweySearch` NuGet package; the JS client ships from `DeweySearch.Web` at `_content/DeweySearch.Web/dewey-search.js`. Per-locale sharded index under `/search/{locale}/`.
- `Pennington.Taxonomy` — TaxonomyContentService/TaxonomyOptions (browse-by-field axes), TaxonomyAccessor (resolve a registered axis's terms by base URL from a routed `@page` — the blog tag pages use this for full chrome + search instead of `MapTaxonomy`'s bare render), TaxonomySlug (shared term-slug encoding so links and discovered routes agree). A meta content service (see `IMetaContentService`).
- `Pennington.Feeds` — RssFeedBuilder, SitemapBuilder, SitemapService, RssFeedWriter
- `Pennington.Data` — `AddDataFile<T>(path, name)`: typed YAML/JSON data files exposed via `IDataFiles.Get<T>` (format inferred from extension, cached value invalidated on file change) — see `EventMicrositeExample`
- `Pennington.Head` — head-composition pipeline: `IHeadContributor` + HeadBuilder/HeadTag/HeadOrder, applied by `HeadCompositionHtmlRewriter`; built-in Canonical/AlternateLinks/StructuredData contributors. Registered via `AddHead()`/`AddHeadContributor<T>()`; inert (byte-identical output) until a contributor is registered
- `Pennington.Favicon` — favicon/icon `<link>` head tags from `PenningtonOptions.Favicons` (`FaviconOptions`/`FaviconLink`); emits discovery markup only — the icon files stay host static assets
- `Pennington.SocialCards` — on-demand OpenGraph card PNGs: `SocialCardOptions.Render` host hook + `SocialCardContentService` (a meta content service) + `MapSocialCards()` mapping `{BaseUrl}/{**slug}.png` (served live in dev, crawled at build). Resolves page metadata from `ContentRecordRegistry`, never the request-forbidden `ISiteProjection`
- `Pennington.StandardSite` — AT Protocol (atproto) `site.standard` publication verification: `StandardSiteOptions` (`Did`, `PublicationRkey`), `WellKnownArtifactService` (artifact-tier) serving `/.well-known/site.standard.publication*` and `/.well-known/atproto-did`. Fail-safe — emits nothing when unconfigured. Inspect with `diag standard-site`
- `Pennington.LlmsTxt` — LlmsTxtService (index + stripped markdown; optional author header read from `{contentRoot}/llms-header.txt` — deliberately not `llms.txt`, which static files would serve verbatim) + LlmsArtifactContentService (artifact-tier façade owning `/llms.txt`, optional `/llms-full.txt`, `**/llms.txt` subtrees, and per-page Markdown twins at `{route}.md` — root page at `/index.md` — each carrying a YAML metadata header over the stripped body). `*.llms.md` source files are llms-only pages: they feed llms.txt and the twins but never render as an HTML page, and they win a route collision against an HTML page at the same canonical route (the docs agent home is `index.llms.md`)
- `Pennington.StructuredData` — JsonLdEntity, JsonLdSerializer, IHasStructuredData, and the concrete schema.org types shared by both templates: JsonLdArticle/JsonLdPerson/JsonLdWebSite/JsonLdBreadcrumbList
- `Pennington.Diagnostics` — Diagnostic, DiagnosticContext, DiagnosticSeverity (per-request diagnostics)
- `Pennington.Infrastructure` — PenningtonExtensions (AddPennington/UsePennington/RunOrBuildAsync), ResponseProcessingMiddleware, IResponseProcessor, NotFoundStatusProcessor + `NotFoundResponseExtensions` (public `HttpContext.MarkNotFound()`/`IsMarkedNotFound()` — pages signal a 404 with this, never the `"Pennington.NotFound"` literal), LiveReloadServer
- `Pennington.Cli` — System.CommandLine host CLI. `PenningtonCli` is the single source of run-mode detection (`PenningtonRunMode` serve/build/diag; `IsHeadlessOneShot`/`WritesOutput` gate TestServer swap, logging, dev overlays). `RunOrBuildAsync` dispatches on it. `IDiagCommand` (DI-discovered) + the `diag` subcommands under `Cli/Diag` (info/toc/routes/warnings/translation/frontmatter/llms, plus the conditional standard-site; other packages contribute their own, e.g. Book's `books`), plus `AsciiTreeWriter`. Read-only, text-only output.

## DI Wiring
- `services.AddPennington(...)` / `app.UsePennington()` / `app.RunOrBuildAsync(args)` — core
- `services.AddDocSite(...)` / `app.UseDocSite()` / `app.RunDocSiteAsync(args)` — doc site template
- `services.AddBlogSite(...)` — blog site template
- `services.AddTreeSitter(...)` — optional tree-sitter multi-language `:symbol` fragment extraction (registers only when `ContentRoot` is set)
- `services.AddApiMetadataFromCompiledAssembly(name, ...)` + `services.AddApiReference(name, ...)` — named API-reference trees (AddApiReference must follow AddDocSite)
- `services.AddPenningtonBook(...)` — PDF book generation (DI-only; no `Use*` call)
- `services.AddPenningtonBeck(...)` — server-side ```` ```beck ```` diagram fence rendering
- `services.AddTranslationAudit(...)` — translation freshness audit
- `services.AddDataFile<T>(path, name)` — typed hot-reloading data files

## Cross-Platform (WSL)
- When switching between Windows and WSL/Linux, run `dotnet clean Pennington.slnx` first — stale `obj/` artifacts from the other OS cause build failures (NuGet fallback paths, Razor editorconfig paths)
- Culture handling differs: Linux ICU synthesizes cultures for any string instead of throwing `CultureNotFoundException`. The `TryGetCulture` method in `PenningtonUrlRequestCultureProvider` guards against this.

## Absolute Paths

Trust the working directory. Use paths relative to the root of the site as a priority, do not prefix with drive and folder unless absolutely necessary. Do not cd into the folder superfluously. Trust your working directory.

# Coding Guidelines

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.