# Penn

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Penn.slnx`
- Test: `dotnet test Penn.slnx`
- Single test: `dotnet test Penn.slnx --filter "FullyQualifiedName~TestName"`

## Project Structure
- `src/Penn/` — Core library (depends on YamlDotNet)
- `tests/Penn.Tests/` — xunit.v3 tests with Shouldly

### Namespaces
- `Penn.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Penn.FrontMatter` — IFrontMatter, capability interfaces, DocFrontMatter, BlogFrontMatter, FrontMatterParser
- `Penn.Pipeline` — ContentItem/ContentSource/ProgrammaticContent unions, ContentPipeline, IContentParser, IContentRenderer
- `Penn.Content` — IContentService, ContentToCopy, ContentToCreate, ContentTocItem
- `Penn.Generation` — BuildReport (with WriteTo), BuildDiagnostic union, BuildReportBuilder, OutputOptions
- `Penn.Navigation` — NavigationTreeItem, NavigationInfo, BreadcrumbItem, NavigationBuilder
- `Penn.Search` — SearchIndexDocument, SearchIndexBuilder
- `Penn.Feeds` — SitemapEntry, RssFeedItem, SitemapBuilder, RssFeedBuilder
- `Penn.Highlighting` — ICodeHighlighter, PlainTextHighlighter, HighlightingService
- `Penn.Islands` — IIslandRenderer, SpaEnvelope, RenderContext
- `Penn.Localization` — LocaleInfo, AlternateLanguagePage
- `Penn.Infrastructure` — LinkCheckResult union, LinkVerificationService, PennOptions

## Conventions
- C# 15 union types for discriminated unions (not abstract base classes)
- Records for data types
- ImmutableList/ImmutableDictionary for collection properties on public types
- Async methods return IAsyncEnumerable or Task
- File-scoped namespaces
- LSP reports false errors on `union` keyword — the compiler handles it correctly

## Union Types
- `ContentItem` — DiscoveredItem, ParsedItem, RenderedItem, FailedItem
- `ContentSource` — MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource
- `ProgrammaticContent` — TextProgrammaticContent, BinaryProgrammaticContent
- `BuildDiagnostic` — DiagnosticInfo, DiagnosticWarning, DiagnosticError
- `LinkCheckResult` — ValidLink, BrokenLinkResult, ExternalLink

Construction: `new UnionType(caseInstance)`. Pattern matching: case types directly in switch.

## Key Services
- `ContentPipeline` — Orchestrates Discover → Parse → Render → Generate; FailedItems propagate through
- `HighlightingService` — Priority-based dispatch to ICodeHighlighter instances
- `NavigationBuilder` — Builds tree from flat ContentTocItem list, computes breadcrumbs/prev/next
- `FrontMatterParser` — YAML front matter extraction and deserialization via YamlDotNet
- `SearchIndexBuilder` — Builds SearchIndexDocument with HTML stripping
- `SitemapBuilder` / `RssFeedBuilder` — Feed generation with draft exclusion
- `LinkVerificationService` — Static link analysis against known routes
