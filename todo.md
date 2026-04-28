# Pennington Code-Smell Cleanup

Each task is self-contained — pick one, open a fresh context, knock it out, commit.

Build/verify commands (used throughout):
- `dotnet build Pennington.slnx`
- `dotnet test Pennington.slnx`
- Docs smoke: `dotnet run --project docs/Pennington.Docs` (Ctrl-C after first request renders)

---

## 1. Centralize build-mode detection ✅ DONE

**Why:** Six call sites detect build-vs-dev mode inconsistently. Two use `args[0]` (parameter from `Main`), four use `args[1]` (because they call `Environment.GetCommandLineArgs()`, where `args[0]` is the executable). A caller invoking `RunOrBuildAsync` directly with the wrong array shape gets silent divergence between which branch DI takes and which branch runtime takes.

**Sites (verified):**
- `src/Pennington/Generation/OutputOptions.cs:31` — uses `args[0]` (param-style)
- `src/Pennington/Infrastructure/PenningtonExtensions.cs:67` — uses `args[1]` (Environment-style, reads `Environment.GetCommandLineArgs()`)
- `src/Pennington/Infrastructure/PenningtonExtensions.cs:493` — uses `args[0]` (param-style, inside `RunOrBuildAsync`)
- `src/Pennington/Infrastructure/DiagnosticOverlayProcessor.cs:18-22` — uses `args[1]` (Environment-style)
- `src/Pennington/Infrastructure/LiveReloadScriptProcessor.cs:13-17` — uses `args[1]` (Environment-style)
- `src/Pennington/Infrastructure/LiveReloadServer.cs:101` — uses `args[1]` (Environment-style)

**Fix:**
1. Add `src/Pennington/Infrastructure/PenningtonBuildMode.cs` with two static methods:
   - `IsBuildMode(string[] args)` — pure check on the param-style array (verb at index 0)
   - `IsBuildMode()` — wraps `Environment.GetCommandLineArgs()`, slices off `args[0]` (executable), delegates to the first overload
2. Replace all six sites with calls to the appropriate overload. `RunOrBuildAsync` already has `args` in hand (use the param overload). Middlewares/processors use the no-arg overload.
3. Delete the inline `IsBuildMode()` private methods in the three processor files.

**Verify:** `dotnet build Pennington.slnx && dotnet test Pennington.slnx`. Smoke test both modes: `dotnet run --project docs/Pennington.Docs` and `dotnet run --project docs/Pennington.Docs -- build`. Confirm dev mode shows live-reload script in HTML and build mode does not.

---

## 2. Drop `IServiceProvider` from feature service constructors

**Why:** Violates the "no infrastructure plumbing in domain service ctors" rule (memory: feedback_no_infra_plumbing_in_ctors.md). Each of these services captures `IServiceProvider` to lazily resolve `IContentService` instances inside async methods — a workaround for a lifetime mismatch that should be fixed at the registration, not the injection.

**Sites:**
- `src/Pennington/Search/SearchIndexService.cs` — ctor captures `IServiceProvider`
- `src/Pennington/Feeds/SitemapService.cs` — ctor captures `IServiceProvider`
- `src/Pennington/LlmsTxt/LlmsTxtService.cs` — ctor captures `IServiceProvider`

**Fix per service:**
1. Read the constructor + the method that calls `_serviceProvider.GetServices<IContentService>()`. Confirm what's actually being resolved (likely `IEnumerable<IContentService>`).
2. Change the ctor to accept `IEnumerable<IContentService>` directly. If the lifetime story breaks (the services are singletons but content services are transient/scoped), the right fix is to register them at the matching lifetime, not to keep `IServiceProvider`.
3. If a scoped→singleton bridge is genuinely needed, document it on the registration with one-line `// Why:` comment — but first try fixing the lifetime.

**Verify:** Build, test, then run docs site and check `/sitemap.xml`, `/search-index.json`, `/llms.txt` all render with the same content as before.

**Related (separate task — see #3):** `Infrastructure/FileWatchDependencyFactory.cs:17` does the same anti-pattern with `ActivatorUtilities.CreateInstance<T>`. Tackle that with whatever lifetime mismatch it was introduced to bridge.

---

## 3. Audit `FileWatchDependencyFactory` and fix the underlying lifetime

**Why:** `FileWatchDependencyFactory<T>` exists to bridge stale-singleton-vs-fresh-transient. This is the same anti-pattern as #2 in different clothing.

**Site:**
- `src/Pennington/Infrastructure/FileWatchDependencyFactory.cs:17-34`

**Fix:**
1. Find every `FileWatchDependencyFactory<T>` consumer (grep `FileWatchDependencyFactory`). Note what `T` is in each case.
2. For each `T`, decide the correct lifetime: if the dep needs to be re-resolved on file change, register a `Transient` that itself watches files — don't bolt watching on with a factory wrapper.
3. Delete `FileWatchDependencyFactory<T>` once no consumers remain. If at least one consumer genuinely needs the indirection, document why on the type.

**Verify:** Edit a content file while `dotnet run --project docs/Pennington.Docs` is up; confirm the change is reflected on the next request.

---

## 4. Unify build-mode dependency in `PenningtonExtensions` (depends on #1)

**Why:** `PenningtonExtensions.cs:67` decides at DI registration time whether to register TestServer (build mode), but `RunOrBuildAsync` line 493 re-checks at startup time. The two checks read different array shapes (#1) and could in principle disagree.

**Fix:**
1. After #1 lands, both call sites use `PenningtonBuildMode.IsBuildMode(args)`. Verify they agree.
2. Consider moving the TestServer registration *into* `RunOrBuildAsync` immediately before `app.StartAsync()`, so there's only one decision point. (May not be feasible if TestServer needs to be registered before `Build()` — check before changing.)

**Verify:** Build + test. Run a sample `examples/` site in both modes.

---

## 5. Extract shared HTML extractor for Search + LlmsTxt

**Why:** `SearchIndexBuilder.StripHtml()` (regex) and `Infrastructure/HtmlToMarkdownConverter` (DOM walk) both transform pipeline HTML output to derived text. They take opposite approaches — the regex one is fragile (entity decoding, no DOM understanding); the markdown one is correct but heavier. They should at least share a DOM-traversal seam.

Both also independently fetch rendered HTML and apply a CSS scope selector (`SearchIndexOptions.ContentSelector`, `LlmsTxtOptions.ContentSelector`).

**Sites:**
- `src/Pennington/Search/SearchIndexBuilder.cs:44-61` — `StripHtml`
- `src/Pennington/Infrastructure/HtmlToMarkdownConverter.cs` — full DOM walker
- `src/Pennington/Search/SearchIndexService.cs` (`RenderedHtmlFetcher` + scope selector)
- `src/Pennington/LlmsTxt/LlmsTxtService.cs` (same pattern)

**Fix:**
1. Add `IHtmlContentExtractor` in `Pennington.Infrastructure` (or `Pennington.Markdown`) with two implementations: `TextExtractor` (search-style) and `MarkdownExtractor` (llms-txt-style). Both operate on AngleSharp `IElement` after the scope selector has been applied.
2. Add a shared `FetchAndScopeHtmlAsync(route, selector)` helper that handles fetching + scoping; both services call it.
3. Replace `SearchIndexBuilder.StripHtml` with `TextExtractor`. Re-point `LlmsTxtService` HTML→markdown path through `MarkdownExtractor`.

**Verify:** Compare `/search-index.json` and `/llms.txt` output before vs after on docs site — content should match (modulo whitespace).

---

## 6. Inline `*Builder` into `*Service` for Search and Sitemap

**Why:** Each Builder is stateless and is called from exactly one Service. The split is ceremony.

**Sites:**
- `src/Pennington/Search/SearchIndexBuilder.cs` + `SearchIndexService.cs`
- `src/Pennington/Feeds/SitemapBuilder.cs` + `SitemapService.cs`

**Fix:**
1. Move each `Builder.Build(...)` method into the corresponding Service as `private` (or `private static`).
2. Delete the Builder file. Keep public surface area unchanged.
3. Check `RssFeedBuilder.cs` — if it's truly a builder pattern (multiple incremental calls), leave it. If it's a one-shot like the other two, fold it into a service.

**Verify:** Build + test. Tests that exercised the Builder directly (if any) need to retarget the Service.

---

## 7. Extract DocSite/BlogSite shared template scaffolding

**Why:** Two parallel implementations of the same site-template concept. Diverging APIs guarantee one will silently lag behind the other (BlogSite is already missing `RenderedFixture` from the Mdazor list).

**Duplicate sites:**
- `src/Pennington.DocSite/DocSiteServiceExtensions.cs` AddMonorailCss block (~line 41-43) ↔ `src/Pennington.BlogSite/BlogSiteServiceExtensions.cs` (~line 67-75)
- `DocSiteServiceExtensions` Mdazor registration (~line 45-46) ↔ `BlogSiteServiceExtensions` (~line 56-65) — and BlogSite is missing `RenderedFixture`
- `DocSiteServiceExtensions.BuildRoutingAssemblies` (~line 133-149) ↔ inline assembly dedup in `BlogSiteServiceExtensions` (~line 43-52)
- `ContentResolver` (DocSite) ↔ `BlogContentResolver` (BlogSite) — same shape, different API

**Fix (staged — pick one sub-task per session):**
- 7a. Extract `BuildRoutingAssemblies` to `Pennington.Infrastructure` (or wherever fits) and call from both.
- 7b. Extract `AddMdazorPenningtonComponents()` extension that registers the canonical Pennington Mdazor component set; both site templates call it. Backfill the missing components in BlogSite.
- 7c. Extract MonorailCSS setup helper.
- 7d. (Bigger) Unify the two resolvers behind a shared base type or strategy. Defer until 7a–7c land — they're cheap and de-risk the bigger one.

**Verify:** `dotnet run --project docs/Pennington.Docs` and at least one example blog site (find under `examples/`). Pages render, navigation works, code blocks highlight.

---

## 8. Replace `LlmsTxtContentService` with an `IFileGenerator`

**Why:** `LlmsTxtContentService` implements `IContentService` but stubs out `DiscoverAsync`, `GetContentToCopyAsync`, `GetContentTocEntriesAsync` (all return empty). It exists only to emit `ContentToCreate` for one file. The interface implementation is dishonest; nobody mocks it.

**Site:**
- `src/Pennington/LlmsTxt/LlmsTxtContentService.cs:15-96`

**Fix:**
1. Define a minimal `IFileGenerator` interface in `Pennington.Generation` (or `Pennington.Pipeline`): `IAsyncEnumerable<ContentToCreate> GenerateFilesAsync(CancellationToken ct)`.
2. Make `LlmsTxtService` implement `IFileGenerator` directly. Delete `LlmsTxtContentService`.
3. Update the build pipeline to collect `IEnumerable<IFileGenerator>` and emit their files alongside the `IContentService` outputs.

**Verify:** `/llms.txt` and any subtree-split llms files still appear in the build output unchanged.

---

## 9. Dead-code sweep ✅ DONE

Pure deletes — quickest wins, do in one session.

- `src/Pennington/Generation/BuildReport.cs` — `BuildDiagnostic.Exception` is set in `BuildReportBuilder.AddError()` but never serialized in `WriteTo()`. Either render it (preferred — exceptions help debugging) or delete the field. Pick one.
- `src/Pennington/Diagnostics/DiagnosticSeverity.cs` + `DiagnosticContext.AddInfo()` + `BuildReportBuilder.AddInfo()` — `Info` severity is never produced and `WriteTo` has no Info section. Delete the enum value and both `AddInfo` methods. (If you'd rather keep `Info` for future use, render it in `WriteTo` — but no half-state.)
- `src/Pennington/Generation/OutputGenerationService.cs:46` — ctor parameter `pennOptions` is captured but never used. Remove it.
- `src/Pennington.UI/Components/Steps.razor:8` — `[Parameter] public string Type` is declared, never referenced. Remove.
- `src/Pennington.MonorailCss/CssClassCollector.cs:52` — `ShouldProcess()` always returns true and is documented as no-op. Remove the method (and decide: if `Classes` is meant to be static singleton, drop the `AddSingleton<CssClassCollector>` registration; if instance-based, remove the static).

**Verify:** Build + test after each removal.

---

## 10. UI palette violations

**Why:** Direct Tailwind color classes (`text-gray-*`, `amber-*`) bypass the semantic palette (memory: feedback_semantic_color_tokens.md).

**Sites:**
- `src/Pennington.UI/Components/Steps.razor:3` — `text-gray-500` → `text-base-500`
- `src/Pennington.UI/Components/FallbackNotice.razor:3` — `border-amber-300 bg-amber-50 text-amber-800` → semantic equivalent (likely `accent-one` family, but check the existing palette in `Pennington.MonorailCss` for the warning/caution tone).

**Fix:** Replace direct color classes with palette tokens. If the right semantic token doesn't exist for "caution," either pick the closest one or extend the palette in `Pennington.MonorailCss` (preferred over inventing a one-off CSS rule per memory: feedback_tailwind_utility_styling.md).

**Verify:** Run docs site; confirm Steps and any FallbackNotice rendering still looks right in light + dark.

---

## 11. Card / LinkCard duplication

**Why:** ~90% identical markup; LinkCard wraps Card's body in `<a>`.

**Sites:**
- `src/Pennington.UI/Components/Card.razor`
- `src/Pennington.UI/Components/LinkCard.razor`

**Fix:** Make `LinkCard` compose `Card` (render `<a>` around or as the root, with `Card` as `ChildContent`), or extract a shared internal partial. Pick whichever keeps consumer call sites unchanged.

**Verify:** `git grep -E '<(Card|LinkCard)\b'` across `examples/` and `docs/`; render each and eyeball.

---

## 12. RazorPageContentService TOC duplication

**Site:**
- `src/Pennington/Content/RazorPageContentService.cs:88-161` — `GetContentTocEntriesAsync` and `GetIndexableEntriesAsync` are near-identical loops differing only in filter and title-fallback.

**Fix:** Extract a private helper that takes a `Func<Entry,bool>` filter and a `Func<Entry,string>` title-source. Both public methods become thin call-throughs.

**Verify:** Build + test. Diff the docs site's TOC and its rendered search index — both should be unchanged.

---

## 13. ContentRouteFactory locale-prefix duplication

**Site:**
- `src/Pennington/Routing/ContentRouteFactory.cs:46-119` — locale prefix logic appears in `FromRazorPage`, `FromUrl`, `FromCustom` (lines ~78, 93, 109).

**Fix:** Extract `private static UrlPath ApplyLocalePrefix(UrlPath path, string locale)` and call from all three.

**Verify:** Build + test. Existing routing tests should cover this.

---

## 14. BlogContentResolver silent exception swallow

**Site:**
- `src/Pennington.BlogSite/Services/BlogContentResolver.cs:67, 69` — `catch { /* skip unparseable */ }`

**Fix:** Inject `ILogger<BlogContentResolver>` and log at `Warning` with the file path and exception message. Keep the skip behavior.

**Verify:** Drop a malformed `.md` in a blog example; confirm a single warning is logged and the rest of the site still builds.

---

## 15. Pre-warm Roslyn symbol extraction (don't `RunSync` in request path)

**Site:**
- `src/Pennington.Roslyn/RoslynCodeBlockPreprocessor.cs:122` — `AsyncHelpers.RunSync()` blocks the HTTP pipeline.

**Fix:** A `SymbolExtractionWarmupService` may already exist (per the agent report — verify with `grep`). If yes, ensure it's run at startup and that `RoslynCodeBlockPreprocessor` reads from the warm cache synchronously. If no, add an `IHostedService` that warms symbols once per solution before the first request.

**Verify:** Cold-start the docs site; first page with Roslyn-highlighted code blocks should not hang.

---

## Suggested order

Phase 1 (small, mechanical, high signal): 1, 9, 10, 13
Phase 2 (lifetime/DI cleanup): 2, 3, 4
Phase 3 (extraction): 5, 6, 8, 11, 12, 14
Phase 4 (structural): 7 (in stages 7a → 7b → 7c → 7d)
Phase 5 (perf): 15
