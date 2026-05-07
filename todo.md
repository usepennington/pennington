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

- [ ] **1.2 FR-18** — unknown-language warning from `HighlightingService`
  - On fallthrough to `PlainTextHighlighter`, emit `Diagnostic` (Info) once per language per instance
  - **Verify:** unit test asserts one diagnostic for first occurrence, no duplicate for second

- [x] **1.3 FR-8** — register `ApiReferenceOptions` as plain singleton in addition to keyed
  - `src/Pennington.Roslyn/ApiMetadata/RoslynApiMetadataExtensions.cs` — when `name == "default"`, also `TryAddSingleton` a non-keyed alias
  - Test: `tests/Pennington.Roslyn.Tests/ApiMetadata/ApiReferenceOptionsRegistrationTests.cs`
  - (Note: alias lives in `Pennington.Roslyn`, not `Pennington.DocSite.Api` — that's where `ApiReferenceOptions` is registered.)

- [ ] **1.4 FR-15** — `ContentTocItem.SearchOnly` flag
  - Add `bool SearchOnly { get; init; }` to `ContentTocItem`
  - `NavigationBuilder.BuildTree` filters `SearchOnly` items
  - `SearchIndexBuilder` includes them
  - **Verify:** integration test — item appears in `/search-index.json`, absent from rendered nav

- [ ] **1.5 FR-20** — `services.ReplaceContentRenderer<TOld, TNew>()` extension
  - First-class replacement helper — replaces Cake's last-wins ordering hack
  - **Verify:** unit test confirms only the replacement runs

- [ ] **1.6 FR-6** — audit cache boundaries; `FileWatched` only at file-IO seams; document the contract
  - **Audit pass:** categorize every `AddPennington`/`AddDocSite`/`AddBlogSite` registration as: file-reading / composing / pure transform
  - Confirm `MarkdownContentService`, `RazorPageContentService` are file-watched
  - **Leave `ContentResolver` transient** — it composes; trust its deps
  - Add xmldoc `<remarks>` on each registered service in one of the three forms (file-watched / composes / pure transform)
  - Drop redundant `Lazy<>` / `ConcurrentDictionary` cache fields on file-watched services (the factory drops the instance)
  - Commit the audit table near `FileWatchDependencyFactory.cs` as a comment block
  - **Verify:** dev-mode file-read counter shows zero re-reads between watcher events

- [ ] **1.7 FR-17** — SDK preflight log line in build mode
  - Log resolved SDK version + `global.json` path at top of `RunOrBuildAsync` build path
  - **Verify:** `dotnet run --project docs/Pennington.Docs -- build` shows both lines first

---

## Phase 2 — Pennington.UI

- [ ] **2.1 FR-14** — `<CodeBlock>` Razor composes with Roslyn preprocessor chain
  - Trace why `CodeBlock.razor:61-65` → `CodeBlockRenderingService.Render` → preprocessors fails to resolve `:xmldocid` on Spectre's index pages
  - Either fix the wiring **or** rename `Language` → `LanguageId` and add a "no `:xmldocid` here" callout
  - **Verify:** integration test — `<CodeBlock Language="csharp:xmldocid,bodyonly">M:Foo.Bar</CodeBlock>` renders the resolved body

- [ ] **2.2 FR-19** — promote DocSite Breadcrumb to `Pennington.UI`
  - Move to `src/Pennington.UI/Components/Breadcrumb.razor`
  - DocSite re-uses the public component
  - Add `@using Pennington.UI.Components` to `Pennington.UI._Imports`
  - **Verify:** DocSite breadcrumbs unchanged; one `examples/` site consumes the public component

- [ ] **2.3** — bare-host `_Imports.razor.snippet` example
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

- [ ] **4.1 FR-9** — additive `CustomUtilities`
  - Add `MonorailCssOptions.AdditionalCustomUtilities` (concatenated with engine defaults)
  - `[Obsolete]` the existing replace-only `CustomUtilities` with a nudge
  - **Verify:** test confirms `PenningtonApplies.ScrollbarUtilities` survives `with { AdditionalCustomUtilities = mine }`

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

---

## Phase 6 — Canonical Pennington docs site

(Skipped per scope: second-locale registration, custom `IContentService`.)

- [ ] **6.1** — add one `IRedirectable` page with an alias
  - **Verify:** old URL → 301 → new URL

- [ ] **6.2** — register one custom highlighter via `ConfigurePennington`
  - Tiny made-up DSL or TOML highlighter
  - **Verify:** markdown fence with new language renders tokenized

- [ ] **6.3** — rebuild `Components/Index.razor` on `<LinkCard>`/`<CardGrid>`/`<Card>`
  - Replace 488-line hand-rolled home page with public components
  - File any rendering gaps as separate UI bugs
  - **Verify:** functional parity with prior home page

- [ ] **6.4** — add one `JsonLdArticle` override
  - **Verify:** view-source shows customised JSON-LD on that page

- [ ] **6.5** — wire Playwright verification into build
  - Port CooklangSharp's `docs/.verify/verify.mjs` to canonical docs
  - **Verify:** verify script produces report; baseline established

- [ ] **6.6** — consume public `FrontMatterKeyIndex` (after 3.1)
  - Replace canonical-docs internal copy with public service
  - **Verify:** front-matter-keys reference page renders identically; no duplicate type under `docs/`

- [x] **6.7** — delete the `ApiReferenceOptions` keyed-service dance
  - `docs/Pennington.Docs/Program.cs:93-99` — collapsed to plain `services.AddSingleton<FrontMatterKeyIndex>()`
  - Verified: `dotnet run --project docs/Pennington.Docs -- build` produces 340 pages clean

---

## Dependency notes

- 1.3 → 6.7
- 3.1 → 6.6
- 2.2 independent of DocSite — DocSite re-imports
- Phase 4 parallelisable with Phases 1–3
- Phase 6 lands last (proves the engine work was sufficient)

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
