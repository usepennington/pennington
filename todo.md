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

## 2. Drop `IServiceProvider` from feature service constructors ✅ DONE

**Why:** Violates the "no infrastructure plumbing in domain service ctors" rule (memory: feedback_no_infra_plumbing_in_ctors.md). Each of these services captured `IServiceProvider` to lazily resolve `IContentService` instances inside async methods — a workaround for a lifetime mismatch that should be fixed at the registration, not the injection.

**Sites (all DONE):**
- `src/Pennington/Search/SearchIndexService.cs` — injects `IEnumerable<IContentService>` directly; `LlmsTxtContentService` filter dropped (no longer in the set).
- `src/Pennington/Feeds/SitemapService.cs` — injects `IEnumerable<IContentService>`, `LocalizationOptions`, `IEnumerable<IContentParser>`.
- `src/Pennington/LlmsTxt/LlmsTxtService.cs` — injects `IEnumerable<IContentService>`, `IContentParser`, `IContentRenderer`, `XrefResolvingService`, `RenderedHtmlFetcher`, `IEnumerable<LlmsSubtree>`, `IFileSystem`, `IWebHostEnvironment`. Filter dropped.

**Cycle resolution (for LlmsTxtService):** Direct injection of `IEnumerable<IContentService>` originally created a cycle: `LlmsTxtService` → enumerable → `LlmsTxtContentService` → `LlmsTxtService` → … . Fixed by promoting `LlmsTxtContentService` to a new `IContentEmitter` interface (just `GetContentToCreateAsync`). `IContentService` now extends `IContentEmitter`, so existing services keep their build-time emission for free. `LlmsTxtContentService` is registered as `IContentEmitter` only, so it isn't in the `IContentService` set that `LlmsTxtService` consumes — cycle broken. `OutputGenerationService` iterates both the IContentService set (for content discovery + emission) and the standalone IContentEmitter set (for emitter-only sources).

**Verify:** Build + tests pass at baseline parity (1312 passing, 11 skipped). `dotnet run --project docs/Pennington.Docs -- build` writes `output/sitemap.xml`, `output/search-index-en.json`, `output/llms.txt`, plus per-page sidecars under `output/_llms/` and per-subtree `output/{prefix}/llms.txt` files (400 pages, 35s).

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

## 7. ContentRouteFactory locale-prefix duplication

**Site:**
- `src/Pennington/Routing/ContentRouteFactory.cs:46-119` — locale prefix logic appears in `FromRazorPage`, `FromUrl`, `FromCustom` (lines ~78, 93, 109).

**Fix:** Extract `private static UrlPath ApplyLocalePrefix(UrlPath path, string locale)` and call from all three.

**Verify:** Build + test. Existing routing tests should cover this.

---

## 8. BlogContentResolver silent exception swallow

**Site:**
- `src/Pennington.BlogSite/Services/BlogContentResolver.cs:67, 69` — `catch { /* skip unparseable */ }`

**Fix:** Inject `ILogger<BlogContentResolver>` and log at `Warning` with the file path and exception message. Keep the skip behavior.

**Verify:** Drop a malformed `.md` in a blog example; confirm a single warning is logged and the rest of the site still builds.

