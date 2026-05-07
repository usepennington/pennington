# Cross-site evaluation follow-up — todo

Source plan: `feature-requests/cross-site-evaluation-2026-05.md`
Approved plan: `~/.claude/plans/i-ve-worked-on-a-pure-lobster.md`

Scope: engine fixes + canonical docs only. FR-1 through FR-5 already filed; DG-* doc-content fixes deferred.

## Constraints

- `AddFileWatched<T>` + `AsyncLazy<>` is for services that **read files directly**. Composing services trust their deps; pure renderers don't cache. Indicate the contract via xmldoc `<remarks>`.
- DocSite stays simple — no second locale or custom `IContentService` in canonical docs.
- No public `IRenderedContentStore`.

---

## Phase 1 — Pennington core engine

- [x] **1.1 FR-13** — front-matter strict mode + unknown-key diagnostic
  - `FrontMatterParserOptions.StrictUnknownKeys` toggles a strict deserializer (no `.IgnoreUnmatchedProperties()`); flag hangs off `PenningtonOptions.FrontMatter`
  - Default-on for build, default-off for serve (flipped in `AddPennington` via `PenningtonBuildMode.IsBuildMode()`)
  - Pre-scan emits a `Warning` diagnostic per unknown key via `DiagnosticContext` with key + file + line in both modes; strict mode additionally throws `YamlException` during deserialize
  - All five `FrontMatterParser` callers (`MarkdownContentParser`, `MarkdownContentService` ×2, `RazorPageContentService`, `ContentResolver`, both BlogSite services) thread `sourcePath`
  - Verified: `tests/Pennington.Tests/FrontMatter/FrontMatterParserDiagnosticsTests.cs` (7 cases); docs build produces 341 pages clean

- [x] **1.2 FR-18** — unknown-language warning from `HighlightingService`
  - Added `DiagnosticSeverity.Info` + `DiagnosticContext.AddInfo`
  - `HighlightingService` gained an optional `IHttpContextAccessor` ctor param; on fallthrough to `PlainTextHighlighter` emits an Info diagnostic once per language (per-instance, threadsafe via `ConcurrentDictionary`, case-insensitive dedup); empty/whitespace languages skip emission
  - DI wired in `PenningtonExtensions.cs:110-113`
  - Verified: 6 new cases in `tests/Pennington.Tests/Highlighting/HighlightingServiceTests.cs` (first/second/third occurrence dedup, case-insensitive, known language no-emit, empty no-emit, wildcard claim no-emit, null accessor no-throw); full suite 553/553 green; `Pennington.AllConsumers.slnx` build 0 warnings 0 errors

- [x] **1.3 FR-8** — register `ApiReferenceOptions` as plain singleton in addition to keyed
  - `src/Pennington.Roslyn/ApiMetadata/RoslynApiMetadataExtensions.cs` — when `name == "default"`, also `TryAddSingleton` a non-keyed alias
  - Test: `tests/Pennington.Roslyn.Tests/ApiMetadata/ApiReferenceOptionsRegistrationTests.cs`
  - (Note: alias lives in `Pennington.Roslyn`, not `Pennington.DocSite.Api` — that's where `ApiReferenceOptions` is registered.)

- [x] **1.4 FR-15** — `ContentTocItem.SearchOnly` flag
  - Added `bool SearchOnly { get; init; }` on `ContentTocItem` and on `IFrontMatter` (default `false`); explicit init properties on `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` so `searchOnly: true` parses cleanly under FR-13 strict mode
  - `MarkdownContentService.BuildTocItem` and `RazorPageContentService` (both `GetContentTocEntriesAsync` and `GetIndexableEntriesAsync`) thread `SearchOnly` from front matter onto the TOC item
  - `NavigationBuilder.GetOrBuildStructural` filters `SearchOnly = true` after `FilterByLocale`; `ComputeCacheKey` now includes `SearchOnly` so cached structural trees invalidate when the flag toggles
  - `SearchIndexBuilder.Build` is unchanged — items pass through unfiltered (the search/llms channels gate on `ExcludeFromSearch`/`ExcludeFromLlms`, which `SearchOnly` does not flip)
  - Verified: 4 new `NavigationBuilderTests` cases (filter, no auto-section, all-search-only, cache invalidation), 1 `SearchIndexBuilderTests` case (SearchOnly TOC still produces a doc), 2 `MarkdownContentServiceTests` cases (front-matter wired through, indexable channel keeps it). Full suite: 560/560 unit + 37/37 integration green; `Pennington.AllConsumers.slnx` build 0 warnings 0 errors

- [x] **1.5 FR-20** — `services.ReplaceContentRenderer<TOld, TNew>()` extension
  - New file `src/Pennington/Pipeline/ContentRendererServiceExtensions.cs` with two overloads: type-based (`ReplaceContentRenderer<TOld, TNew>()`) and factory-based (`ReplaceContentRenderer<TOld, TNew>(Func<IServiceProvider, TNew>)`) — the factory variant covers cases like Cake's `ShortcodeContentRenderer(string cakeVersion)` where the new renderer takes ctor args DI can't resolve
  - Implementation calls `services.RemoveAll<IContentRenderer>()` then re-registers, removing the dependence on registration order
  - Cake's `B:\cake\website\Hosting\CakebuildServiceCollectionExtensions.cs` migrated from the last-wins `services.AddTransient<IContentRenderer>(...)` hack to `services.ReplaceContentRenderer<MarkdownContentRenderer, ShortcodeContentRenderer>(...)` — the comment block kept the design rationale, dropped the "last-wins" caveat
  - Verified: 4 new `ContentRendererServiceExtensionsTests` cases (type overload, factory overload, no prior registration, null factory throws); full suite 564/564 unit + `Pennington.AllConsumers.slnx` build 0 warnings 0 errors (proving Cake still compiles + runs against the new helper)

- [x] **1.6 FR-6** — audit cache boundaries; `FileWatched` only at file-IO seams; document the contract
  - Audit table committed as a top-of-file comment block in `src/Pennington/Infrastructure/FileWatchDependencyFactory.cs` (categorises every registration in `AddPennington` / `AddDocSite` / `AddBlogSite` as file-reading / composing / pure transform / state-container / scoped / options)
  - Confirmed: `MarkdownContentService<TFm>` is plain-singleton + internal `AddPathWatch` (open-generic precludes `AddFileWatched<T>`; path-scoped subscription is functionally equivalent to instance replacement and avoids cross-source invalidation)
  - Confirmed: `RazorPageContentService` is plain-singleton with `Lazy<>` and **no** file-watch subscription. Documented as intentional — `@page` routes are compile-time so a .razor edit forces a host restart; sidecar `.razor.metadata.yml` reloads only on restart. Audit notes the switch to `AddFileWatched` is the path forward if live sidecar reload becomes a requirement.
  - Confirmed: `ContentResolver` (DocSite) stays transient — composes
  - Added `<remarks>` lifetime statements to: `NavigationBuilder`, `HighlightingService`, `RedirectContentService`, `RazorPageContentService`, `BlogSiteContentService`, `MarkdownContentService<TFm>`. Other services already carried sufficient lifetime context in their existing xmldoc; the audit table is the canonical reference.
  - **No redundant caches found.** Every file-watched service uses a single `AsyncLazy<>` (or `ConcurrentDictionary` memo for `NavigationBuilder`) with no internal watcher subscription — exactly the pattern `FileWatchDependencyFactory<T>` is designed for. The audit table notes the four cases where caches are intentionally retained (NavigationBuilder memo, HighlightingService dedup, MarkdownContentService path-scoped lazy, RedirectContentService alias-scoped lazy) and explains why each is correct.
  - Verified: full suite 560/560 unit + `Pennington.AllConsumers.slnx` build 0 warnings 0 errors
  - **Deferred:** dev-mode file-read counter (separate instrumentation feature; current cache structure is correct without it — added as a TODO if future drift suspected).

- [~] **1.7 FR-17** — SDK preflight log line in build mode — **skipped**
  - Log resolved SDK version + `global.json` path at top of `RunOrBuildAsync` build path
  - **Verify:** `dotnet run --project docs/Pennington.Docs -- build` shows both lines first

---

## Phase 2 — Pennington.UI

- [x] **2.1 FR-14** — `<CodeBlock>` Razor composes with Roslyn preprocessor chain
  - Investigation: the wiring already works. `CodeBlock.razor:61` calls `CodeBlockRenderingService.Render(code, languageId)` — the same entry point the Markdig fenced-code path uses. `Render` iterates `IEnumerable<ICodeBlockPreprocessor>` (priority-sorted), so any `:xmldocid` / `:path` / `:xmldocid-diff` modifier routes through `RoslynCodeBlockPreprocessor` when `AddPenningtonRoslyn` is wired with a `SolutionPath`.
  - **End-to-end proof:** static-built `examples/BeyondRoslynExample` (`dotnet run --project examples/BeyondRoslynExample -- build`). All four `<CodeBlock Language="csharp:xmldocid[,bodyonly]">` invocations on `/codeblock-razor/` render resolved Roslyn output (whole `Calculator` class, `Multiply`'s `return a * b;` body, `Add` method with declaration). `<CodeBlock Language="csharp">var x = 1;</CodeBlock>` sentinel still highlights as plain csharp.
  - Updated `CodeBlock.razor`'s `Language` parameter xmldoc to document the modifier grammar (`csharp:xmldocid`, `,bodyonly`, `,usings`, `:path`, `:xmldocid-diff`) and call out that they route through registered `ICodeBlockPreprocessor` chain (e.g. `RoslynCodeBlockPreprocessor`).
  - **No rename needed** — `Language` stays the canonical name. The `:xmldocid` modifier syntax is supported on the same parameter.
  - Corrected `B:\spectre\spectre-docs\PENNINGTON-MIGRATION-POSTMORTEM.md` section 4 from "doesn't run the preprocessor" to "RESOLVED upstream" with the BeyondRoslynExample reference.
  - Verified: 6 new `CodeBlockRenderingServiceTests` cases (matching preprocessor returns, no-match falls through, null-result falls through, priority order, no preprocessors, modifier on language passes through to base for fallback). Full suite 570/570 unit tests pass; `Pennington.AllConsumers.slnx` builds clean across all 5 repos.
  - **Note for Spectre:** Spectre.Docs's `/console/` and `/cli/` index Razor pages render correctly when triggered, but `Program.cs` doesn't set `penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly]`, so `RazorPageContentService` doesn't crawl their `@page` directives at static build time. That's a separate Spectre wiring issue, not an FR-14 concern.

- [x] **2.2 FR-19** — promote Breadcrumb to `Pennington.UI`
  - Discovered: DocSite has no visible Breadcrumb today (only feeds `JsonLdBreadcrumbList` for SEO via `<StructuredData>`); Cake was the real consumer rolling its own at `Components/Content/Breadcrumb.razor`. Treated the spec's "DocSite re-uses the public component" line as referring to the broader bare-host case Cake represents.
  - Created public `src/Pennington.UI/Components/Breadcrumb.razor`. API: `ImmutableList<BreadcrumbItem> Items` + optional `RenderFragment? TrailingContent` for "Edit on GitHub"-style chrome. Adds `<nav aria-label="Breadcrumb">` wrapper and `aria-current="page"` on the last item; semantic palette only (`base-*`, `primary-*`) so it works on any Pennington site.
  - `Pennington.UI/Components/_Imports.razor` already had `@using Pennington.UI.Components` (no change needed).
  - Cake migrated: deleted `B:\cake\website\Components\Content\Breadcrumb.razor`; added `@using Pennington.UI.Components` to `B:\cake\website\Components\_Imports.razor`. `MarkdownPage.razor` line 37 (`<Breadcrumb Items="@Breadcrumbs" />`) now resolves to the public component without code changes.
  - DocSite breadcrumbs unchanged (still JSON-LD only).
  - Verified: full Pennington suite 564/564 unit tests pass; `Pennington.AllConsumers.slnx` builds clean across all 5 repos; Cake's own test suite 27/27 passes against the migrated component. Visual fidelity preserved — the public component reuses Cake's exact wrapper/inner/item utility classes, only adding non-visual a11y attributes.
  - Skipped synthetic `examples/` Breadcrumb demo: Cake's migration is stronger verification than a contrived example.

- [~] **2.3** — bare-host `_Imports.razor.snippet` example — **skipped**
  - Add to `examples/` so consumers see `@using Pennington.UI.Components`

---

## Phase 3 — Pennington.DocSite.Api

- [ ] **3.1 FR-7** — promote `FrontMatterKeyIndex` to public
  - Move `docs/Pennington.Docs/ApiReference/FrontMatterKeyIndex.cs` (~215 lines, `internal sealed`) → `src/Pennington.DocSite.Api/FrontMatterKeyIndex.cs` (public sealed)
  - Make `FrontMatterKeyEntry` public record
  - Add `services.AddFrontMatterKeyIndex()` extension
  - Ship `<FrontMatterKeys />` Razor component (in `Pennington.DocSite.Api` or `Pennington.UI` depending on dep direction)
  - **Verify:** canonical docs reference page renders identically after swapping to public service

---

## Phase 4 — Pennington.MonorailCss

- [x] **4.1 FR-9** — additive `CustomUtilities` — **resolved without new API**
  - Investigation showed the existing API already supports the additive idiom: the user callback receives a fully-baked `CssFrameworkSettings` with `PenningtonApplies.ScrollbarUtilities` pre-applied, so `settings with { CustomUtilities = settings.CustomUtilities.AddRange([mine]) }` preserves defaults. The only trap is the natural-looking replace form `CustomUtilities = [mine]` which silently clobbers them.
  - Real bug was in Cake (`B:\cake\website\SiteStyles.cs`): a 30-line `CustomCssFrameworkSettings` block hand-duplicated all 4 scrollbar utilities for no reason — they were already in `settings.CustomUtilities` by the time the callback fired. Brittle (would silently drop a 5th utility if Pennington added one) and required reading `internal` engine source to know what to copy. **Deleted the entire block** plus the now-unused `System.Collections.Immutable` and `MonorailCss.Parser.Custom` imports. Cake's scrollbars still flow through from the engine defaults.
  - Added a `<remarks>` warning to `MonorailCssOptions.CustomCssFrameworkSettings` showing the `AddRange` pattern and calling out that plain assignment silently clobbers engine defaults.
  - No new API surface, no `[Obsolete]` — the existing callback shape is fine for both replace and append intents once consumers know the pattern.
  - Verified: 570/570 unit tests pass; `Pennington.AllConsumers.slnx` builds clean across all 5 repos.

- [ ] **4.2 FR-10** — declaration-fragment CSS highlighter
  - Add `CssFragment` enum (`Declarations` / `Stylesheet` / `Selector`) or `HighlightDeclarations(string)` method
  - **Verify:** unit test highlights `color: red; padding: 1rem;` with no wrapping selector

- [ ] **4.3 FR-11** — `MonorailCssEngine.CompileUtilityClass(string)` direct method
  - Direct shortcut to `Framework.CompileUtilityClass`
  - **Verify:** MonorailCSS docs site can drop parallel `CssFramework` registration

- [ ] **4.4 FR-12** — promote `humans-only` / `robots-only` to public
  - Locate the markers (exploration agent couldn't find — search `src/` thoroughly first)
  - Surface as constants on `Pennington.LlmsTxt.ContentVisibility` (or similar)
  - xmldoc the search-vs-rendered contract
  - **Verify:** canonical docs consumes the constants in one place; intellisense shows them

---

## Phase 5 — Pennington.Roslyn

- [ ] **5.1 FR-16** — promote `Project.GetCleanName()` / `StripTargetFrameworkSuffix` to public
  - Public extension method on `Pennington.Roslyn`
  - **Verify:** canonical docs deletes its local strip helper; output identical

## End-to-end verification (after each phase)

1. `dotnet build Pennington.slnx` — clean, no new warnings
2. `dotnet test Pennington.slnx`
3. `dotnet run --project docs/Pennington.Docs -- build` — completes; (after 6.5) Playwright verify passes
4. Smoke-run `dotnet run --project docs/Pennington.Docs` — click nav, search, redirects, an api-reference page, home

## Out of scope

- FR-1 through FR-5 (already filed)
- DG-1 through DG-11 (separate doc-content pass)
- Public `IRenderedContentStore` API
- Second locale / custom `IContentService` in canonical docs
- "Porting from MyLittleContentEngine" mapping table (DG-11)
- Antipattern docs warnings
