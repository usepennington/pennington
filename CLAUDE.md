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
- `docs/Pennington.Docs/` — The actual docs site (29 markdown pages + homepage)
- `tests/Pennington.Tests/` — Unit tests (xunit.v3, Shouldly)
- `tests/Pennington.IntegrationTests/` — Integration tests (WebApplicationFactory)

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
- `Pennington.Infrastructure` — PenningtonExtensions (AddPennington/UsePennington/RunOrBuildAsync), ResponseProcessingMiddleware, IResponseProcessor

## DI Wiring
- `services.AddDocSite(...)` — registers Pennington, MonorailCSS, SPA, Razor components
- `app.UseDocSite()` — configures middleware pipeline
- `app.RunDocSiteAsync(args)` — serve or build static site

## Cross-Platform (WSL)
- When switching between Windows and WSL/Linux, run `dotnet clean Pennington.slnx` first — stale `obj/` artifacts from the other OS cause build failures (NuGet fallback paths, Razor editorconfig paths)
- Culture handling differs: Linux ICU synthesizes cultures for any string instead of throwing `CultureNotFoundException`. The `TryGetCulture` method in `PenningtonUrlRequestCultureProvider` guards against this.

## Conventions
- C# 15 union types (construction: `new UnionType(caseInstance)`, pattern matching: case types directly)
- Records for data types, ImmutableList for collections
- File-scoped namespaces
- LSP reports false errors on `union` keyword and ASP.NET/Markdig types — the compiler handles them correctly
- Roslyn highlighting deferred to optional Pennington.Roslyn package (not yet created)
