# Penn

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Penn.slnx`
- Test: `dotnet test Penn.slnx`
- Single test: `dotnet test Penn.slnx --filter "FullyQualifiedName~TestName"`
- Run docs site: `dotnet run --project docs/Penn.Docs`

## Project Structure
- `src/Penn/` — Core library (Markdig, YamlDotNet, AngleSharp, TextMateSharp)
- `src/Penn.UI/` — Razor component library (TableOfContentsNav, OutlineNav, Badge, Card, CodeBlock, etc.)
- `src/Penn.MonorailCss/` — MonorailCSS integration (utility-first CSS generation)
- `src/Penn.DocSite/` — Documentation site template (layout, pages, content resolver)
- `docs/Penn.Docs/` — The actual docs site (29 markdown pages + homepage)
- `tests/Penn.Tests/` — Unit tests (xunit.v3, Shouldly)
- `tests/Penn.IntegrationTests/` — Integration tests (WebApplicationFactory)

## Key Namespaces (Penn core)
- `Penn.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Penn.FrontMatter` — IFrontMatter, capability interfaces, FrontMatterParser
- `Penn.Pipeline` — ContentItem/ContentSource unions, ContentPipeline, IContentParser/IContentRenderer
- `Penn.Content` — IContentService, MarkdownContentService, RazorPageContentService
- `Penn.Markdown` — MarkdownContentParser/Renderer, MarkdownPipelineFactory, extensions (highlighting, tabs, alerts)
- `Penn.Highlighting` — ICodeHighlighter, TextMateHighlighter, ShellHighlighter, HighlightingService
- `Penn.Generation` — BuildReport, OutputGenerationService, OutputOptions
- `Penn.Navigation` — NavigationBuilder, NavigationTreeItem, NavigationInfo
- `Penn.Islands` — SpaPageDataService, SpaNavigationExtensions, IIslandRenderer
- `Penn.Infrastructure` — PennExtensions (AddPenn/UsePenn/RunOrBuildAsync), ResponseProcessingMiddleware, IResponseProcessor

## DI Wiring
- `services.AddDocSite(...)` — registers Penn, MonorailCSS, SPA, Razor components
- `app.UseDocSite()` — configures middleware pipeline
- `app.RunDocSiteAsync(args)` — serve or build static site

## Conventions
- C# 15 union types (construction: `new UnionType(caseInstance)`, pattern matching: case types directly)
- Records for data types, ImmutableList for collections
- File-scoped namespaces
- LSP reports false errors on `union` keyword and ASP.NET/Markdig types — the compiler handles them correctly
- Roslyn highlighting deferred to optional Penn.Roslyn package (not yet created)
