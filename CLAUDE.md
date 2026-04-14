# Pennington

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Pennington.slnx`
- Test: `dotnet test Pennington.slnx`
- Single test: `dotnet test Pennington.slnx --filter "FullyQualifiedName~TestName"`
- Run docs site: `dotnet run --project docs/Pennington.Docs`

## Project Structure
- `src/Pennington/` — Core library (Markdig, YamlDotNet, AngleSharp, TextMateSharp)
- `src/Pennington.UI/` — Razor component library (TableOfContentsNav, OutlineNav, Badge, Card, CodeBlock, etc.)
- `src/Pennington.MonorailCss/` — MonorailCSS integration (utility-first CSS generation)
- `src/Pennington.DocSite/` — Documentation site template (layout, pages, content resolver)
- `src/Pennington.BlogSite/` — Blog site template (home/archive/tag pages, blog front matter, content service)
- `src/Pennington.Roslyn/` — Optional Roslyn-based highlighting, symbol extraction, xmldocid code fragment preprocessor
- `docs/Pennington.Docs/` — The Pennington docs site (Divio-style: tutorials, how-to, reference, explanation)
- `examples/` — Variety of example sites used for reference and verification across scenarios
- `tests/Pennington.Tests/` — Unit tests (xunit.v3, Shouldly)
- `tests/Pennington.IntegrationTests/` — Integration tests (WebApplicationFactory)
- `tests/Pennington.Roslyn.Tests/` — Tests for the Roslyn package

## Key Namespaces (Pennington core)
- `Pennington.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Pennington.FrontMatter` — IFrontMatter, capability interfaces, FrontMatterParser
- `Pennington.Pipeline` — ContentItem/ContentSource unions, ContentPipeline, IContentParser/IContentRenderer
- `Pennington.Content` — IContentService, MarkdownContentService, RazorPageContentService
- `Pennington.Markdown` — MarkdownContentParser/Renderer, MarkdownPipelineFactory, extensions (highlighting, tabs, alerts)
- `Pennington.Highlighting` — ICodeHighlighter, TextMateHighlighter, ShellHighlighter, HighlightingService
- `Pennington.Generation` — BuildReport, OutputGenerationService, OutputOptions
- `Pennington.Navigation` — NavigationBuilder, NavigationTreeItem, NavigationInfo
- `Pennington.Islands` — SpaPageDataService, SpaNavigationExtensions, IIslandRenderer
- `Pennington.Localization` — LocaleContext, LocaleDetectionMiddleware, LocaleLinkHtmlRewriter, PenningtonStringLocalizer, TranslationOptions
- `Pennington.Search` — SearchIndexBuilder, SearchIndexService, SearchIndexOptions (per-locale index from post-pipeline HTML)
- `Pennington.Feeds` — RssFeedBuilder, SitemapBuilder, SitemapService
- `Pennington.LlmsTxt` — LlmsTxtService, LlmsTxtContentService (llms.txt index + stripped markdown)
- `Pennington.StructuredData` — JsonLdSerializer, JsonLdTypes (schema.org)
- `Pennington.Diagnostics` — Diagnostic, DiagnosticContext, DiagnosticSeverity (per-request diagnostics)
- `Pennington.Infrastructure` — PenningtonExtensions (AddPennington/UsePennington/RunOrBuildAsync), ResponseProcessingMiddleware, IResponseProcessor, LiveReloadServer

## DI Wiring
- `services.AddPennington(...)` / `app.UsePennington()` / `app.RunOrBuildAsync(args)` — core
- `services.AddDocSite(...)` / `app.UseDocSite()` / `app.RunDocSiteAsync(args)` — doc site template
- `services.AddBlogSite(...)` — blog site template
- `services.AddPenningtonRoslyn(...)` — optional Roslyn highlighting + symbol services

## Cross-Platform (WSL)
- When switching between Windows and WSL/Linux, run `dotnet clean Pennington.slnx` first — stale `obj/` artifacts from the other OS cause build failures (NuGet fallback paths, Razor editorconfig paths)
- Culture handling differs: Linux ICU synthesizes cultures for any string instead of throwing `CultureNotFoundException`. The `TryGetCulture` method in `PenningtonUrlRequestCultureProvider` guards against this.

## Conventions
- C# 15 union types (construction: `new UnionType(caseInstance)`, pattern matching: case types directly)
- Records for data types, ImmutableList for collections
- File-scoped namespaces
- LSP reports false errors on `union` keyword and ASP.NET/Markdig types — the compiler handles them correctly


## Absolute Paths

Trust the working directory. Use paths relative to the root of the site as a priority, do not prefix with drive and folder unless absolutely nescassary. Do not cd into the folder superfulously. Trust your working directory.