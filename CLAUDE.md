# Penn

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Penn.slnx`
- Test: `dotnet test Penn.slnx`
- Single test: `dotnet test Penn.slnx --filter "FullyQualifiedName~TestName"`

## Project Structure
- `src/Penn/` — Core library
- `tests/Penn.Tests/` — xunit.v3 tests with Shouldly

### Namespaces
- `Penn.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Penn.FrontMatter` — IFrontMatter, capability interfaces, DocFrontMatter, BlogFrontMatter
- `Penn.Pipeline` — ContentItem/ContentSource/ProgrammaticContent unions, RenderedContent, IContentPipeline
- `Penn.Content` — IContentService, ContentToCopy, ContentToCreate, ContentTocItem
- `Penn.Generation` — BuildReport, BuildDiagnostic union, BuildReportBuilder, OutputOptions, BrokenLink
- `Penn.Navigation` — NavigationTreeItem, NavigationInfo, BreadcrumbItem
- `Penn.Search` — SearchIndexDocument
- `Penn.Feeds` — SitemapEntry, RssFeedItem
- `Penn.Highlighting` — ICodeHighlighter, PlainTextHighlighter
- `Penn.Islands` — IIslandRenderer, SpaEnvelope, RenderContext
- `Penn.Localization` — LocaleInfo, AlternateLanguagePage
- `Penn.Infrastructure` — UnionPolyfills, LinkCheckResult union, PennOptions

## Conventions
- C# 15 union types for discriminated unions (not abstract base classes)
- Records for data types
- ImmutableList/ImmutableDictionary for collection properties on public types
- Async methods return IAsyncEnumerable or Task
- File-scoped namespaces
- Union polyfills in `Infrastructure/UnionPolyfills.cs` until .NET 11 RTM
- LSP reports false errors on `union` keyword — the compiler handles it correctly

## Union Types in This Codebase
- `ContentItem` — DiscoveredItem, ParsedItem, RenderedItem, FailedItem
- `ContentSource` — MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource
- `ProgrammaticContent` — TextProgrammaticContent, BinaryProgrammaticContent
- `BuildDiagnostic` — DiagnosticInfo, DiagnosticWarning, DiagnosticError
- `LinkCheckResult` — ValidLink, BrokenLinkResult, ExternalLink

Construction: `new UnionType(caseInstance)`. Pattern matching: case types directly in switch.
