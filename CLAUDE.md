# Pennington

Content engine library targeting .NET 11 / C# 15 with union types.

## Build & Test
- Build: `dotnet build Pennington.slnx`
- Test: `dotnet test Pennington.slnx`
- Single test: `dotnet test Pennington.slnx --filter "FullyQualifiedName~TestName"`
- Run docs site: `dotnet run --project docs/Pennington.Docs`

## Project Structure
- `src/Pennington/` — Core library (Markdig, SharpYaml, AngleSharp, TextMateSharp)
- `src/Pennington.UI/` — Razor component library (TableOfContentsNav, OutlineNav, Badge, Card, CodeBlock, etc.)
- `src/Pennington.MonorailCss/` — MonorailCSS integration (utility-first CSS generation)
- `src/Pennington.DocSite/` — Documentation site template (layout, pages, content resolver)
- `src/Pennington.BlogSite/` — Blog site template (home/archive/tag pages, blog front matter, content service)
- `src/Pennington.Roslyn/` — Optional Roslyn-based highlighting, symbol extraction, xmldocid code fragment preprocessor
- `src/Pennington.TreeSitter/` — Optional tree-sitter-based multi-language code-fragment extraction (`:symbol` fence, name-path addressing) via the `TreeSitter.DotNet` package
- `docs/Pennington.Docs/` — The Pennington docs site (Divio-style: tutorials, how-to, reference, explanation)
- `examples/` — Variety of example sites used for reference and verification across scenarios
- `tests/Pennington.Tests/` — Unit tests (xunit.v3, Shouldly)
- `tests/Pennington.IntegrationTests/` — Integration tests (WebApplicationFactory)
- `tests/Pennington.Roslyn.Tests/` — Tests for the Roslyn package
- `tests/Pennington.TreeSitter.Tests/` — Tests for the TreeSitter package (resolver/grammar configs, fragment service, render pipeline)

## Key Namespaces (Pennington core)
- `Pennington.Routing` — UrlPath, FilePath, ContentRoute, ContentRouteFactory
- `Pennington.FrontMatter` — IFrontMatter, capability interfaces, FrontMatterParser
- `Pennington.Pipeline` — ContentItem/ContentSource unions, ContentPipeline, IContentParser/IContentRenderer
- `Pennington.Content` — IContentService, MarkdownContentService, RazorPageContentService
- `Pennington.Markdown` — MarkdownContentParser/Renderer, MarkdownPipelineFactory, extensions (highlighting, tabs, alerts)
- `Pennington.Highlighting` — ICodeHighlighter, TextMateHighlighter, ShellHighlighter, HighlightingService
- `Pennington.Generation` — BuildReport, OutputGenerationService, OutputOptions
- `Pennington.Navigation` — NavigationBuilder, NavigationTreeItem, NavigationInfo
- `Pennington.Localization` — LocaleContext, LocaleDetectionMiddleware, LocaleLinkHtmlRewriter, PenningtonStringLocalizer, TranslationOptions
- `Pennington.Search` — host adapter over the external **DeweySearch** engine: SearchArtifactService/Emitter/Middleware + HeadingSectionExtractor (splits post-pipeline HTML into one section per heading) + SearchIndexBuilder (maps each section onto a `DeweySearch.SearchDocument` — anchor URL, page→heading breadcrumb, open facets), SearchIndexOptions/SearchFacetField (host config). Records are **heading-level** (DocSearch-style): results deep-link to `/page/#heading` and carry crumbs for grouping. The engine (tokenizer/stemmer/inverted index) is the `DeweySearch` NuGet package; the JS client ships from `DeweySearch.Web` at `_content/DeweySearch.Web/dewey-search.js`. Per-locale sharded index under `/search/{locale}/`.
- `Pennington.Feeds` — RssFeedBuilder, SitemapBuilder, SitemapService
- `Pennington.LlmsTxt` — LlmsTxtService, LlmsTxtContentService (llms.txt index + stripped markdown)
- `Pennington.StructuredData` — JsonLdSerializer, JsonLdTypes (schema.org)
- `Pennington.Diagnostics` — Diagnostic, DiagnosticContext, DiagnosticSeverity (per-request diagnostics)
- `Pennington.Infrastructure` — PenningtonExtensions (AddPennington/UsePennington/RunOrBuildAsync), ResponseProcessingMiddleware, IResponseProcessor, LiveReloadServer

## DI Wiring
- `services.AddPennington(...)` / `app.UsePennington()` / `app.RunOrBuildAsync(args)` — core
- `services.AddDocSite(...)` / `app.UseDocSite()` / `app.RunDocSiteAsync(args)` — doc site template
- `services.AddBlogSite(...)` — blog site template
- `services.AddPenningtonRoslyn(...)` — optional Roslyn highlighting + symbol services (C#/VB `:xmldocid` fence)
- `services.AddPenningtonTreeSitter(...)` — optional tree-sitter multi-language `:symbol` fragment extraction (registers only when `ContentRoot` is set)

## Cross-Platform (WSL)
- When switching between Windows and WSL/Linux, run `dotnet clean Pennington.slnx` first — stale `obj/` artifacts from the other OS cause build failures (NuGet fallback paths, Razor editorconfig paths)
- Culture handling differs: Linux ICU synthesizes cultures for any string instead of throwing `CultureNotFoundException`. The `TryGetCulture` method in `PenningtonUrlRequestCultureProvider` guards against this.

## Absolute Paths

Trust the working directory. Use paths relative to the root of the site as a priority, do not prefix with drive and folder unless absolutely nescassary. Do not cd into the folder superfulously. Trust your working directory.

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