# Pennington site architecture — grounded reference

Target audience: documentation-writing subagents. Every identifier below has been verified in `src/`.
Paths are repo-relative.

## 1. Namespace map

### `Pennington.Routing` — `src/Pennington/Routing/`
Owns URL and filesystem paths as value types plus canonical route construction.
- `UrlPath` (`readonly record struct`) — URL math (composition via `/` operator, `EnsureLeadingSlash`, `EnsureTrailingSlash`, `RemoveTrailingSlash`, `RemoveLeadingSlash`, `Matches`). Normalizes `/index.html` to directory form when comparing.
- `FilePath` (`readonly record struct`) — filesystem path value type with `Extension`, `FileNameWithoutExtension`, `FileName`, implicit-from-`string`.
- `ContentRoute` (`sealed record`) — `CanonicalPath`, `OutputFile`, `SourceFile?`, `Locale`, `IsFallback`, plus `WithBaseUrl` / `AbsoluteUrl` / `IsDefaultLocale`.
- `ContentRouteFactory` (static) — `FromMarkdownFile`, `FromRazorPage`, `FromUrl`, `FromCustom`, `ForRedirect`.

Every downstream namespace depends on this one; routes are the universal coordinate.

### `Pennington.FrontMatter` — `src/Pennington/FrontMatter/`
YAML parsing and the capability model for page metadata.
- `IFrontMatter` — the single required property is `Title`. After commit `984dc7a` it carries default members for `IsDraft`, `Search`, `Llms`, `Uid`, `Description`, `Date`.
- Remaining capability interfaces in `Capabilities.cs`: `ITaggable` (string[] Tags), `IRedirectable` (string? RedirectUrl), `ISectionable` (string? Section), `IOrderable` (int Order).
- `DocFrontMatter`, `BlogFrontMatter` — record implementations in this namespace.
- `FrontMatterParser` — wraps `YamlDotNet` with `CamelCaseNamingConvention`; `Parse<T>(string)` returns `(metadata, body)`, `DeserializeYaml<T>` for sidecar files. Uses `SafeYamlParser` to swallow malformed input.

### `Pennington.Pipeline` — `src/Pennington/Pipeline/`
The union-typed processing pipeline.
- `union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)` — exposes a shared `Route` property that pattern-matches to each case.
- `union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource)`.
- `IContentPipeline` with `DiscoverAsync`/`ParseAsync`/`RenderAsync`/`GenerateAsync` plus `ContentPipeline` implementation.
- `IContentParser.ParseAsync(DiscoveredItem)` and `IContentRenderer.RenderAsync(ParsedItem)` — both return `ContentItem` so failures can surface as `FailedItem`.
- Supporting records: `ContentError`, `RenderedContent`, `OutlineEntry`, `Tag`, `CrossReference`, `SocialMetadata`, `IProgrammaticContentGenerator` and `union ProgrammaticContent(TextProgrammaticContent, BinaryProgrammaticContent)`.

### `Pennington.Content` — `src/Pennington/Content/`
Content services — adapters from source formats into the pipeline.
- `IContentService` — `DiscoverAsync`, `GetContentToCopyAsync`, `GetContentToCreateAsync`, `GetContentTocEntriesAsync`, `GetIndexableEntriesAsync` (default member), `GetCrossReferencesAsync`, `DefaultSection`, `SearchPriority`.
- `MarkdownContentService<TFrontMatter>` (implements `IContentService` and `IMarkdownContentSource`), `RazorPageContentService`.
- `MarkdownContentServiceOptions` — `ContentPath`, `BasePageUrl`, `Section`, `FilePattern` (default `*.md`), `Locale`, `SearchPriority`, `ExcludePaths`.
- `ContentToCopy`, `ContentToCreate`, `ContentTocItem`, `MarkdownSourceOverlapDetector`.

### `Pennington.Markdown` — `src/Pennington/Markdown/`
Markdig pipeline wiring and project-specific extensions.
- `MarkdownPipelineFactory.CreateWithExtensions` — `UseAdvancedExtensions`, `UseYamlFrontMatter`, `UseSyntaxHighlighting`, `UseTabbedCodeBlocks`, `UseCustomAlerts`.
- `MarkdownContentParser<TFrontMatter>`, `MarkdownContentRenderer`, `MarkdownLinkResolver` (file-watched, resolves relative links by scanning the source → URL index), `MarkdownOutlineGenerator`.
- `Markdown/Extensions/`: `CodeBlockExtensions`, `CodeHighlightRenderer`, `CodeTransformer`, `CodeBlockHtmlBuilder`, `CustomAlertInlineParser`, `ICodeBlockPreprocessor`, `Tabs/` subfolder.

### `Pennington.Highlighting` — `src/Pennington/Highlighting/`
- `ICodeHighlighter` contract, `HighlightingService` (priority-ordered dispatcher with `PlainTextHighlighter` fallback), built-in `TextMateHighlighter` (backed by `TextMateLanguageRegistry`) and `ShellHighlighter`.
- Additional highlighters (notably `RoslynHighlighter`) are registered by `PenningtonOptions.Highlighting.AddHighlighter`.

### `Pennington.Generation` — `src/Pennington/Generation/`
Static site crawler and build reporting.
- `OutputGenerationService.GenerateAsync()` — crawls the running ASP.NET pipeline via `IInProcessHttpDispatcher` (TestServer in build mode, Kestrel in dev). Constant `NotFoundGeneratorPath = "/__pennington-404-generator"` drives `404.html` rendering.
- `OutputOptions` — `OutputDirectory`, `BaseUrl`, `CleanOutput`; `FromArgs(args)` parses the `build [baseUrl] [output]` CLI shape and no-ops on non-`build` invocations.
- `BuildReport`, `BuildReportBuilder`, `BuildDiagnostic`, `BrokenLink`.

### `Pennington.Navigation` — `src/Pennington/Navigation/`
- `NavigationBuilder.BuildTree(items, currentRoute?, locale?)`, `BuildNavigationInfo(...)`, `BreadcrumbItem`, `NavigationTreeItem` (record with `Title`, `Route`, `Order`, `Section`, `IsSelected`, `IsExpanded`, `Children`), `NavigationInfo` (previous/next/current plus breadcrumbs).

### `Pennington.Islands` — `src/Pennington/Islands/`
SPA navigation and server-rendered island system.
- `IIslandRenderer` (`IslandName`, `RenderAsync(ContentRoute, RenderContext)`), `RazorIslandRenderer<T>`, `ComponentRenderer`.
- `SpaPageDataService`, `SpaNavigationContentService` (emits per-page `_spa-data/*.json`), `SpaNavigationOptions.DataPath` (default `/_spa-data`), `SpaEnvelope`, `SpaEnvelopeJsonContext`, `SpaSlug`.
- `SpaNavigationExtensions.AddSpaNavigation` / `UseSpaNavigation`.

### `Pennington.Localization` — `src/Pennington/Localization/`
- `LocaleContext` (scoped), `LocaleInfo`, `AlternateLanguagePage`.
- `LocaleDetectionMiddleware` — detects locale, populates `LocaleContext`, rewrites `HttpContext.Request.Path` to strip the prefix (preserves prefix in `PathBase`).
- `PenningtonUrlRequestCultureProvider` — implements `IRequestCultureProvider`; `TryGetCulture` protects against Linux ICU synthesizing cultures for any string.
- `LocaleLinkHtmlRewriter` (`IHtmlResponseRewriter`, `Order => 20`) — prefixes internal links with the active locale.
- `PenningtonStringLocalizer` / `PenningtonStringLocalizerFactory` — `IStringLocalizer` backed by `TranslationOptions`.
- `TranslationOptions` — in-memory per-locale key/value dictionary.

### `Pennington.Search` — `src/Pennington/Search/`
- `SearchIndexOptions` — `ContentSelector`, `DefaultPriority`.
- `SearchIndexBuilder.Build(ContentTocItem, bodyHtml)`, `SearchIndexDocument`, `SearchIndexService` — produces one JSON file per locale using post-pipeline HTML fetched via `RenderedHtmlFetcher`.

### `Pennington.Feeds` — `src/Pennington/Feeds/`
- `RssFeedBuilder(UrlPath canonicalBase)`, `RssFeedItem`.
- `SitemapBuilder`, `SitemapEntry`, `SitemapService` — registered as file-watched singleton; exposed at `/sitemap.xml`.

### `Pennington.LlmsTxt` — `src/Pennington/LlmsTxt/`
- `LlmsTxtOptions` — `OutputDirectory` (default `_llms`), `GenerateFullFile`, `ContentSelector`.
- `LlmsTxtService` — generates `llms.txt` and stripped markdown files; converts HTML→markdown via `HtmlToMarkdownConverter`.
- `LlmsTxtContentService` — `IContentService` that emits the markdown sidecar files into the output tree.

### `Pennington.StructuredData` — `src/Pennington/StructuredData/`
- `JsonLdSerializer` and `JsonLdTypes.cs` (records: `JsonLdArticle`, `JsonLdBreadcrumbItem`, `JsonLdBreadcrumbList`, `JsonLdWebSite`, …).

### `Pennington.Diagnostics` — `src/Pennington/Diagnostics/`
- `DiagnosticSeverity` (`Info`, `Warning`, `Error`), `Diagnostic`, `DiagnosticContext` (scoped accumulator with `Add`, `AddWarning`, `AddError`, `AddInfo`, `HasAny`, `HasErrors`). Surfaced via `X-Pennington-Diagnostic` headers and the dev overlay.

### `Pennington.Infrastructure` — `src/Pennington/Infrastructure/`
Host wiring, middleware, and the response-processor scaffolding.
- `PenningtonExtensions` (static) — `AddPennington`, `UsePennington`, `RunOrBuildAsync`, `UsePenningtonLocaleRouting`.
- `PenningtonOptions` + nested `MarkdownContentOptions`, `HighlightingOptions`, `IslandsOptions`, `LocalizationOptions`, `AlternateLanguage`.
- `IResponseProcessor`, `IHtmlResponseRewriter`, `ResponseProcessingMiddleware`, `HtmlResponseRewritingProcessor`, `XrefHtmlRewriter`, `BaseUrlHtmlRewriter`, `LiveReloadScriptProcessor`, `DiagnosticOverlayProcessor`.
- `XrefResolver`, `XrefResolvingService`, `RenderedHtmlFetcher`, `LinkVerificationService`, `LinkCheckResult`, `HtmlToMarkdownConverter`.
- `FileWatcher` / `IFileWatcher`, `FileWatchDependencyFactory<T>`, `FileWatchedServiceExtensions` (`AddFileWatched<T>`), `AsyncLazy<T>`.
- `LiveReloadServer`, `LiveReloadExtensions.UsePenningtonLiveReload`, `FontPreload`, `StringExtensions`, `UnionPolyfills`.

### `Pennington.UI` (separate project) — `src/Pennington.UI/`
Razor component library. Components include `Badge.razor`, `BigTable.razor`, `Card.razor`, `CardGrid.razor`, `CodeBlock.razor`, `FallbackNotice.razor`, `LanguageSwitcher.razor`, `LinkCard.razor`, `Step.razor`, `Steps.razor`, `StructuredData.razor`, plus `Navigation/TableOfContentsNavigation.razor` and `Navigation/OutlineNavigation.razor`. Static assets under `wwwroot/`.

### `Pennington.MonorailCss` (separate project) — `src/Pennington.MonorailCss/`
- `MonorailCssOptions` — `ColorScheme`, `CustomCssFrameworkSettings`, `ExtraStyles`, `ContentPaths`. `IColorScheme` / `NamedColorScheme`.
- `CssClassCollector`, `CssClassCollectorProcessor` (`IResponseProcessor`), `ColorPaletteGenerator`.
- `MonorailServiceExtensions.AddMonorailCss(...)` / `UseMonorailCss(string path = "/styles.css")`.

### `Pennington.DocSite` (separate project) — `src/Pennington.DocSite/`
- `DocSiteOptions` (record) — `SiteTitle`, `Description`, `ColorScheme`, `CanonicalBaseUrl`, `ContentRootPath`, `HeaderIcon`, `HeaderContent`, `FooterContent`, `GitHubUrl`, `SocialImageUrl`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, `AdditionalHtmlHeadContent`, `FontPreloads`, `AdditionalRoutingAssemblies`, `SolutionPath`, `ConfigureLocalization`, `Areas`.
- `DocSiteFrontMatter`, `ContentArea`.
- `Services/ContentResolver` — resolves pages by URL; recently updated (commit `d4947a0`) to resolve by the full URL so the locale prefix reaches it.
- `Slots/DocSiteArticleSlotRenderer` — `RazorIslandRenderer<DocSiteArticle>` with `IslandName => "content"`.
- `DocSiteServiceExtensions.AddDocSite`, `UseDocSite`, `RunDocSiteAsync`.

### `Pennington.BlogSite` (separate project) — `src/Pennington.BlogSite/`
- `BlogSiteOptions` — `SiteTitle`, `Description`, `CanonicalBaseUrl`, `ColorScheme`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `AuthorName`/`AuthorBio`, `EnableRss`, `EnableSitemap`, `HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`, `SocialMediaImageUrlFactory`, plus font and styling fields.
- `BlogSiteFrontMatter`, `BlogPostPage`, records `SocialLink`, `HeaderLink`, `Project`, `HeroContent`.
- `Services/BlogContentResolver`, `Services/BlogSiteContentService` (`IContentService` that yields per-tag routes and `/rss.xml`).
- `BlogSiteServiceExtensions.AddBlogSite`, `UseBlogSite`, `RunBlogSiteAsync`.

### `Pennington.Roslyn` (separate, optional) — `src/Pennington.Roslyn/`
- `RoslynOptions` — `SolutionPath`, `ProjectFilter`.
- `Highlighting/RoslynHighlighter`, `Highlighting/SyntaxHighlighter`.
- `Workspace/ISolutionWorkspaceService` + `SolutionWorkspaceService`.
- `Symbols/ISymbolExtractionService` + `SymbolExtractionService`.
- `Preprocessing/RoslynCodeBlockPreprocessor` — implements `ICodeBlockPreprocessor`, turns xmldocid fences into code fragments.
- `RoslynExtensions.AddPenningtonRoslyn`.

## 2. Host integration extensions

| Extension method | Signature | Purpose |
| --- | --- | --- |
| `PenningtonExtensions.AddPennington` | `IServiceCollection AddPennington(this IServiceCollection, Action<PenningtonOptions>)` | Register core services, content sources, pipeline, rewriters, feeds, search, llms.txt, diagnostics. |
| `PenningtonExtensions.UsePennington` | `WebApplication UsePennington(this WebApplication)` | Static files per source/locale, locale routing, live reload, `ResponseProcessingMiddleware`, per-locale `/search-index-{code}.json`, `/sitemap.xml`, optional `/llms.txt`. |
| `PenningtonExtensions.RunOrBuildAsync` | `Task RunOrBuildAsync(this WebApplication, string[] args)` | Dev serve on `dotnet run`; on `build` starts the host, invokes `OutputGenerationService.GenerateAsync`, prints report, sets exit code on errors. |
| `PenningtonExtensions.UsePenningtonLocaleRouting` | `WebApplication UsePenningtonLocaleRouting(this WebApplication)` | Registers `RequestLocalizationOptions`, `PenningtonUrlRequestCultureProvider`, `LocaleDetectionMiddleware`, then `UseRouting()`. Idempotent via `Pennington.LocaleRoutingAdded` key. |
| `LiveReloadExtensions.UsePenningtonLiveReload` | `WebApplication UsePenningtonLiveReload(this WebApplication)` | Gated on `DOTNET_WATCH`; maps the `/__pennington/reload` WebSocket to `LiveReloadServer`. |
| `DocSiteServiceExtensions.AddDocSite` | `IServiceCollection AddDocSite(this IServiceCollection, Func<DocSiteOptions>)` | Composes `AddPennington` + `AddMonorailCss` + `AddSpaNavigation` + `ContentResolver` + `DocSiteArticleSlotRenderer`. |
| `DocSiteServiceExtensions.UseDocSite` | `WebApplication UseDocSite(this WebApplication)` | Locale routing → antiforgery → static files → `MapRazorComponents<App>` → MonorailCSS → SPA nav → `UsePennington`. |
| `DocSiteServiceExtensions.RunDocSiteAsync` | `Task RunDocSiteAsync(this WebApplication, string[] args)` | Thin delegate to `RunOrBuildAsync`. |
| `BlogSiteServiceExtensions.AddBlogSite` | `IServiceCollection AddBlogSite(this IServiceCollection, Func<BlogSiteOptions>)` | Composes `AddPennington` + `AddMonorailCss` + file-watched `BlogContentResolver` and `BlogSiteContentService`. |
| `BlogSiteServiceExtensions.UseBlogSite` | `WebApplication UseBlogSite(this WebApplication)` | Antiforgery → static files → `MapRazorComponents<App>` → MonorailCSS → `UsePennington`; maps `/rss.xml` when `EnableRss`. |
| `BlogSiteServiceExtensions.RunBlogSiteAsync` | `Task RunBlogSiteAsync(this WebApplication, string[] args)` | Thin delegate to `RunOrBuildAsync`. |
| `MonorailServiceExtensions.AddMonorailCss` | `IServiceCollection AddMonorailCss(this IServiceCollection, Func<IServiceProvider, MonorailCssOptions>? = null)` | Registers `CssClassCollector`, `MonorailCssService`, and `CssClassCollectorProcessor` as `IResponseProcessor`. |
| `MonorailServiceExtensions.UseMonorailCss` | `WebApplication UseMonorailCss(this WebApplication, string path = "/styles.css")` | Scans `ContentPaths` for classes, maps the stylesheet endpoint. |
| `RoslynExtensions.AddPenningtonRoslyn` | `IServiceCollection AddPenningtonRoslyn(this IServiceCollection, Action<RoslynOptions>? = null)` | Always registers `RoslynHighlighter`; when `SolutionPath` is set, adds workspace + symbol services + the xmldocid preprocessor. |
| `SpaNavigationExtensions.AddSpaNavigation` / `UseSpaNavigation` | `IServiceCollection AddSpaNavigation(this IServiceCollection, Action<SpaNavigationOptions>? = null)` / `IEndpointRouteBuilder UseSpaNavigation(this IEndpointRouteBuilder)` | Register/expose the `_spa-data` JSON endpoint for partial navigation. |
| `FileWatchedServiceExtensions.AddFileWatched<T>` | `IServiceCollection AddFileWatched<T>(this IServiceCollection)` where `T : class` | Register a singleton behind `FileWatchDependencyFactory<T>` that reconstructs on file changes. |

There is **no** `WithMarkdownContentService<T>`. The actual API is `PenningtonOptions.AddMarkdownContent<TFrontMatter>(Action<MarkdownContentOptions>)` in `src/Pennington/Infrastructure/PenningtonOptions.cs`.

## 3. Content pipeline stages

Four-stage pipeline, expressed as `union ContentItem`.

1. **Discovery** — `ContentPipeline.DiscoverAsync()` fan-ins `IContentService.DiscoverAsync()` from every registered service. Each yield is a `DiscoveredItem(Route, Source)`. The `ContentSource` union discriminates the origin: `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`.
2. **Parse** — `ContentPipeline.ParseAsync` delegates `DiscoveredItem` → `IContentParser.ParseAsync`, which returns `ParsedItem(Route, IFrontMatter Metadata, string RawMarkdown)`. Exceptions are caught and demoted to `FailedItem(Route, ContentError)`; already-parsed or already-rendered items pass through unchanged; existing `FailedItem`s propagate.
3. **Render** — `ContentPipeline.RenderAsync` delegates `ParsedItem` → `IContentRenderer.RenderAsync`, which returns `RenderedItem(Route, Metadata, RenderedContent)`. Errors again downgrade to `FailedItem`. `RenderedContent` carries `Html`, `Outline`, `Tags`, `CrossReferences`, `SearchDocument?`, `Social?`.
4. **Generate** — `ContentPipeline.GenerateAsync(items, OutputOptions)` pattern-matches each union case. `RenderedItem` that is a draft is skipped (`IFrontMatter.IsDraft`); otherwise `BuildReportBuilder.AddGeneratedPage`, then `LinkVerificationService.FindLinksWithoutTrailingSlash` adds warnings for bad internal links. `FailedItem` becomes an error. Razor pages with missing trailing slashes surface as warnings from `RazorPageContentService.MissingTrailingSlashPages`.

`FailedItem` is introduced either by the parser or the renderer catch blocks, and is threaded all the way through `Generate` without being re-processed, so every failure lands in `BuildReport`. In the live host pipeline, content services are also consulted directly by `ContentResolver` (doc) and `BlogContentResolver` (blog) for per-request rendering.

## 4. Response-processor pipeline

Two tiers wrap the response body:

**Tier A — `IResponseProcessor`.** Generic, captures the full body, sorted by `Order`, each can rewrite the string. Captured by `ResponseProcessingMiddleware` in `src/Pennington/Infrastructure/ResponseProcessingMiddleware.cs`, which also writes `X-Pennington-Diagnostic` headers just before flushing. Built-ins (from `AddPennington` and `AddMonorailCss`), in order:

| Order | Type | File |
| --- | --- | --- |
| 10 | `HtmlResponseRewritingProcessor` | `src/Pennington/Infrastructure/HtmlResponseRewritingProcessor.cs` |
| 20 | `LiveReloadScriptProcessor` (dev only) | `src/Pennington/Infrastructure/LiveReloadScriptProcessor.cs` |
| 30 | `DiagnosticOverlayProcessor` (dev only) | `src/Pennington/Infrastructure/DiagnosticOverlayProcessor.cs` |
| (unordered sibling) | `CssClassCollectorProcessor` | `src/Pennington.MonorailCss/CssClassCollectorProcessor.cs` |

**Tier B — `IHtmlResponseRewriter`.** Shared AngleSharp pass owned by `HtmlResponseRewritingProcessor`. Two-phase: `PreParseAsync(string, HttpContext)` for non-HTML constructs (`<xref:uid>`), then `ApplyAsync(IDocument, HttpContext)` mutating a single parsed document. Before this consolidation each rewriter parsed the DOM independently; the refactor is commit `4617f64`. The three built-in rewriters registered in `AddPennington`:

| Order | Rewriter | File |
| --- | --- | --- |
| 10 | `XrefHtmlRewriter` | `src/Pennington/Infrastructure/XrefHtmlRewriter.cs` |
| 20 | `LocaleLinkHtmlRewriter` | `src/Pennington/Localization/LocaleLinkHtmlRewriter.cs` |
| 30 | `BaseUrlHtmlRewriter` | `src/Pennington/Infrastructure/BaseUrlHtmlRewriter.cs` |

Ordering is load-bearing: xref resolution produces canonical paths that locale prefixing then transforms, with base-URL prefixing last so it is the outermost transport layer.

`ResponseProcessingMiddleware` is registered via `app.UseMiddleware<ResponseProcessingMiddleware>()` inside `UsePennington`, after locale routing and live reload but before the mapped feed/search endpoints.

## 5. Front-matter capability model

`IFrontMatter` is the universal contract: every content page must supply a `Title`. Before commit `984dc7a` there were ten interfaces — `IFrontMatter` plus `IDraftable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISearchable`, `ILlmsIndexable`, `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable`. That commit collapsed the six whose adoption was universal into default members on `IFrontMatter`. The current contract in `src/Pennington/FrontMatter/IFrontMatter.cs`:

| Member | Default | Origin |
| --- | --- | --- |
| `Title` (required) | — | always required |
| `IsDraft` | `false` | was `IDraftable` |
| `Search` | `true` | was `ISearchable` |
| `Llms` | `true` | was `ILlmsIndexable` |
| `Uid` | `null` | was `ICrossReferenceable` |
| `Description` | `null` | was `IDescribable` |
| `Date` | `null` | was `IDateable` |

The remaining four capability interfaces in `Capabilities.cs` stay separate because not every content type implements them:
- `ITaggable` — `string[] Tags`
- `IRedirectable` — `string? RedirectUrl`
- `ISectionable` — `string? Section`
- `IOrderable` — `int Order`

Consumers still pattern-match these interfaces when they care. `DocFrontMatter` implements `IFrontMatter, ITaggable, ISectionable, IOrderable`; `BlogFrontMatter` implements `IFrontMatter, ITaggable`.

## 6. Options classes catalog

- `PenningtonOptions` — `src/Pennington/Infrastructure/PenningtonOptions.cs`. Top-level engine options: sources, highlighting, islands, localization, llms.txt, search, translations, routing assemblies.
- `MarkdownContentOptions` — same file. Per-source `ContentPath`, `BasePageUrl`, `Section`, `ExcludePaths` (surfaced via `AddMarkdownContent<T>`).
- `MarkdownContentServiceOptions` — `src/Pennington/Content/MarkdownContentServiceOptions.cs`. Internal options handed to `MarkdownContentService<T>`; adds `FilePattern`, `Locale`, `SearchPriority`.
- `DocSiteOptions` — `src/Pennington.DocSite/DocSiteOptions.cs`.
- `BlogSiteOptions` — `src/Pennington.BlogSite/BlogSiteOptions.cs`.
- `LocalizationOptions` — in `PenningtonOptions.cs`. Locales + URL math (`GetLocaleFromUrl`, `StripLocalePrefix`, `BuildLocaleUrl`, `GetAlternateLanguages`).
- `TranslationOptions` — `src/Pennington/Localization/TranslationOptions.cs`.
- `HighlightingOptions` — in `PenningtonOptions.cs`. `AddHighlighter<T>`/`AddHighlighter(instance)`.
- `IslandsOptions` — in `PenningtonOptions.cs`. `Register<T>(name)`.
- `SpaNavigationOptions` — `src/Pennington/Islands/SpaNavigationOptions.cs`. `DataPath` (default `/_spa-data`).
- `SearchIndexOptions` — `src/Pennington/Search/SearchIndexOptions.cs`.
- `LlmsTxtOptions` — `src/Pennington/LlmsTxt/LlmsTxtOptions.cs`.
- `OutputOptions` — `src/Pennington/Generation/OutputOptions.cs`.
- `MonorailCssOptions` — `src/Pennington.MonorailCss/MonorailCssOptions.cs`.
- `RoslynOptions` — `src/Pennington.Roslyn/RoslynOptions.cs`.

## 7. Dev-vs-build unified code path

Dev serve and static build share one rendering path. In build mode (`dotnet run -- build [baseUrl] [output]`):

1. `RunOrBuildAsync` sees `args[0] == "build"`, invokes `app.StartAsync()` so the full ASP.NET host comes up exactly as in `dotnet run`.
2. It resolves `OutputGenerationService` (`src/Pennington/Generation/OutputGenerationService.cs`) and calls `GenerateAsync(app.Urls.First())`.
3. `OutputGenerationService` constructs an `HttpClient` with `AllowAutoRedirect = false` pointed at the running host, discovers pages via `IContentService.DiscoverAsync` and `DiscoverMapGetRoutes` from `EndpointDataSource`, issues HTTP GETs, and writes each response to `OutputOptions.OutputDirectory`. The sentinel `NotFoundGeneratorPath` (`"/__pennington-404-generator"`) is fetched to materialize `404.html`.
4. Because every page is produced by a real HTTP round-trip, the response processors, `IHtmlResponseRewriter` pipeline, Razor SSR, Markdig extensions, `MonorailCSS` class collection, and search-index/llms.txt endpoints all run identically to dev serve. The only thing that changes between modes is the output target (stdout/socket vs. disk).

CLI argument surface is parsed in `OutputOptions.FromArgs`: `args[0]` must equal `build`. Named flags `--base-url` and `--output` (space- or `=`-joined value) are the preferred form; legacy positional `args[1] = BaseUrl`, `args[2] = OutputDirectory` still works as a back-compat fallback. Defaults: `BaseUrl = "/"`, `OutputDirectory = "output"`. Non-`build` invocations return a no-op `OutputOptions` so integration tests and `dotnet watch` don't misread positional args.

This unification is a deliberate invariant — do not propose designs that add a separate "offline build" renderer that bypasses the HTTP pipeline.

## 8. Cross-cutting infrastructure

**Live reload.** `src/Pennington/Infrastructure/LiveReloadServer.cs` holds a `ConcurrentDictionary<string, WebSocket>` of connected browser clients and subscribes to `IFileWatcher`. On change it broadcasts the literal bytes `"reload"`. `LiveReloadExtensions.UsePenningtonLiveReload` maps `/__pennington/reload` (internal constant `ReloadPath`) as a WebSocket endpoint, gated on `DOTNET_WATCH`. `LiveReloadScriptProcessor` (`Order => 20`) injects the reconnection script before `</body>` in HTML responses, also gated on `DOTNET_WATCH`.

**File watching.** `FileWatcher : IFileWatcher` in `src/Pennington/Infrastructure/FileWatcher.cs` wraps `IFileSystem.FileSystemWatcher.New`, dedupes by `path|pattern`, and exposes `SubscribeToChanges(Action)`. `FileWatchDependencyFactory<T>` caches a service instance and invalidates it on any watcher notification, re-creating via `ActivatorUtilities.CreateInstance`. Services registered with `AddFileWatched<T>` (currently `MarkdownLinkResolver`, `XrefResolver`, `SearchIndexService`, `SitemapService`, `LlmsTxtService`, `BlogContentResolver`, `BlogSiteContentService`) pick up fresh data without explicit cache busts.

**Middleware placement.** In `UsePennington` the order is: content-root and per-source `UseStaticFiles` → per-locale `UseStaticFiles` → `UsePenningtonLocaleRouting` (localization + `UseRouting`) → `UsePenningtonLiveReload` → `UseMiddleware<ResponseProcessingMiddleware>` → `MapGet` endpoints for search, sitemap, llms.txt. `ResponseProcessingMiddleware` wraps the rest of the pipeline's response body so every endpoint — Blazor, mapped GETs, static files served via middleware — runs through the processor chain.

**Locale routing.** `LocaleDetectionMiddleware` runs inside `UsePenningtonLocaleRouting`, after `UseRequestLocalization` and before `UseRouting`, so Blazor endpoints match against the already-stripped path. `PenningtonUrlRequestCultureProvider.TryGetCulture` guards against Linux ICU synthesizing cultures from unknown strings (see CLAUDE.md cross-platform notes).

**Content resolution.** `src/Pennington.DocSite/Services/ContentResolver.cs` takes the full request URL (post commit `d4947a0`) so the locale prefix reaches it; it normalizes `/index` → `/`, searches every `IContentService` for a matching `DiscoveredItem`, then re-parses and re-renders via the injected `IContentParser` / `IContentRenderer`. The blog site has an equivalent `BlogContentResolver` in `src/Pennington.BlogSite/Services/BlogContentResolver.cs`.

**Xref resolution.** `XrefResolver` owns the uid → URL map (file-watched). `XrefResolvingService` does the work, invoked first as a raw-string pre-pass (for `<xref:uid>` tag syntax that is not valid HTML) and then as a DOM pass (for `href="xref:uid"` attribute syntax). Unknown uids are emitted as diagnostics via `DiagnosticContext` and surfaced in the dev overlay.
