# Post-mortem — ExtensibilityLabExample

## What was built

`examples/ExtensibilityLabExample/` — the third how-to demo app,
backing all seven §2.3 Extensibility recipes. Bare `AddPennington`
host (no `AddDocSite`) so each extension registration is visible raw
in `Program.cs`. Seven extension files, one per how-to, named for
their topic. Five markdown content pages plus two JSON release
sources under `Content/` exercise every extension at least once.

## Host-choice resolution

Spec required `AddPennington`, but how-to 2.3.70
(`override-docsite-components`) is DocSite-shaped. Resolution: main
`Program.cs` stays on bare `AddPennington` + `AddMonorailCss` +
`AddSpaNavigation`, and `SiteChromeOverrides.cs` is a compile-only
static helper that returns a populated `DocSiteOptions`. The
`Pennington.DocSite` package is referenced by the csproj solely to
make this helper compile; it is never wired into the running host.
The how-to fences the helper methods via `M:...,bodyonly` — each
slot seam's wire-up is a copy-pasteable fragment even though this
app does not render with a DocSite layout. A real user's DocSite
host copies `BuildDocSiteOptions()` into their `AddDocSite(...)`
factory and gets the same four slot seams populated.

## Seven extensions: contract, registration, observable effect

1. **`ReleaseNotesContentService : IContentService`** (2.3.10).
   Reads `Content/releases/*.json`, yields three `DiscoveredItem`s
   (index + two versions), emits matching `ContentTocItem`s and
   `CrossReference`s. Registered as both the concrete type (so the
   fallback endpoint can inject it) and as `IContentService`
   forwarded to the same instance. Verified: `/releases/`,
   `/releases/1.0.0/`, `/releases/1.1.0/` all return 200 with the
   expected `data-release-version` markup.
2. **`LineCountPreprocessor : ICodeBlockPreprocessor`** (2.3.20).
   Priority 500. `TryProcess` returns a `CodeBlockPreprocessResult`
   with `SkipTransform=true` for `linecount`-tagged fences, null
   otherwise. Registered as `AddSingleton<ICodeBlockPreprocessor, …>`.
   Verified: `/line-count-demo/` renders `<figure class="linecount">…
   <figcaption>Line count: <strong>5</strong></figcaption>`.
3. **`PipelineHighlighter : ICodeHighlighter`** (2.3.30). Priority
   100, language `pipeline`. Registered via
   `penn.Highlighting.AddHighlighter(new PipelineHighlighter())`.
   Verified: 10 highlight spans with four distinct classes
   (`pipeline-keyword`, `pipeline-arrow`, `pipeline-pipe`,
   `pipeline-string`) on `/pipeline-demo/`.
4. **`FeedbackWidgetProcessor : IResponseProcessor`** (2.3.40).
   Order 500, gates on `text/html` + 2xx in `ShouldProcess`,
   injects before `</body>` in `ProcessAsync`. Registered as
   `AddSingleton<IResponseProcessor, …>`. Verified: every HTML page
   carries `<aside class="feedback-widget">`.
5. **`AnchorLowercaseRewriter : IHtmlResponseRewriter`** (2.3.50).
   Order 500. `PreParseAsync` string-replaces
   `<!--LOWERCASE-SENTINEL-->` out before AngleSharp runs;
   `ApplyAsync` walks `a[data-lowercase]` and lowercases
   `TextContent`. Registered as
   `AddSingleton<IHtmlResponseRewriter, …>`. Verified: `/lowercase-demo/`
   shows `home`, `pipeline demo`, `line-count demo` post-rewrite;
   the `Untouched` anchor stays mixed-case; the comment form of the
   sentinel is absent from the served HTML (only one occurrence of
   the bare string, in the prose that describes it).
6. **`ChartIslandRenderer : RazorIslandRenderer<ChartIsland>`**
   (2.3.60). `IslandName="chart"`. `BuildParametersAsync` returns
   parameters only when the route path contains `/chart-demo`,
   null otherwise. Registered via
   `penn.Islands.Register<ChartIslandRenderer>("chart")` plus
   `AddScoped<ComponentRenderer>()` and `AddSpaNavigation()`.
   Verified: `<div data-spa-island="chart">` is on `/chart-demo/`
   and `/_spa-data/chart-demo.json` returns
   `{"islands":{"chart":"<figure class=\"chart-island\">…Quarterly widgets…"}}`.
7. **`SiteChromeOverrides` static helper** (2.3.70). Compile-only.
   Four helper methods return `AdditionalHtmlHeadContent` /
   `ExtraStyles` / `Header`+`FooterContent` / `AdditionalRoutingAssemblies`
   from a populated `DocSiteOptions` record, plus a tiny
   `ExtraHeadFragment.razor` component as the head-slot fence
   target. Verification is by `dotnet build` only — runtime behaviour
   of these seams lives in `DocSiteKitchenSinkExample` (#13).

## API surfaces found vs. spec

- **`ContentRouteFactory.FromUrl` signature is `(UrlPath, string locale = "")`** — not `(UrlPath, FilePath)`. Used `FromCustom(UrlPath, FilePath?, string)` to attach a `SourceFile` for the release JSON. Spec-adjacent, not a blocker.
- **Pipeline has a single `IContentParser` and `IContentRenderer`**, and `MarkdownContentParser` rejects non-`MarkdownFileSource` items. A non-markdown content service therefore cannot flow through the pipeline — the two options are: (a) render the content via a sibling `MapGet` endpoint and let the static crawler fetch it, or (b) emit `ContentToCreate` entries for direct file writes. `ReleaseNotesContentService` takes option (a) so dev and static builds share one code path (matches the locked "one code path" convention).
- **`OutputGenerationService` parallel write race** — FIXED (plan P0-2). Surfaced when a URL was emitted both as a content-service `DiscoveredItem` *and* a `MapGet` route. The example still uses the single catch-all `MapGet("/{*path}")` pattern because that's a sensible convention for non-markdown content. But the engine now defends itself: `OutputGenerationService.GenerateAsync` deduplicates discovered routes by output file. Duplicates from two content services, or from a content service plus a dedicated MapGet, emit a warning and the later entry is dropped instead of racing on the output path. Regression coverage: `GenerateAsync_TwoContentServices_SameOutputFile_WarnsAndDedupes` in `tests/Pennington.Tests/Generation/OutputGenerationServiceTests.cs`.
- **`RazorIslandRenderer<T>` needs `ComponentRenderer` in DI** — FIXED (plan P2-1). Previously `AddPennington` didn't register it; bare hosts had to add `AddScoped<ComponentRenderer>()` manually. `ComponentRenderer` is now registered by `AddPennington` itself (moved from `AddDocSite`), so any host that uses `RazorIslandRenderer<T>` — bare or templated — has the dependency available without extra wiring. This example's explicit `AddScoped<ComponentRenderer>()` is now redundant but harmless.
- **`SpaNavigationContentService` skips `RedirectSource` / `RazorPageSource`.** Release-notes routes use `RedirectSource` so they are intentionally left out of `/_spa-data/*.json` — the SPA engine falls back to a full navigation for those URLs, which is fine since they are non-markdown content.

## Conventions for app #16 (SubPathDeployableExample)

Minimal carryover — #16 goes back to `AddDocSite`, is deliberately
tiny to make `BaseUrlHtmlRewriter` behaviour observable, and ships
deployment fixtures (`.github/workflows/deploy.yml`, `netlify.toml`,
etc.) rather than code. The reminder worth carrying: pair a
`MapGet` endpoint with its content-service discovery only when you
intend the crawler to issue *one* HTTP fetch per route; otherwise
prefer `ContentToCreate` or a single catch-all.

## Caveats, not blockers

- **2.3.70 verification is compile-time only.** The DocSite slot
  seams render inside `Pennington.DocSite.Components.App.razor`, and
  this app does not mount that component tree. Runtime coverage of
  the seams lives in #13 (`DocSiteKitchenSinkExample`) which is a
  DocSite host. The seven-extensions-in-one-app constraint was met
  by making 2.3.70 a code-fence target; pushing it to runtime would
  force a second host in `Program.cs` or a second app.
- **`PenningtonOptions.AdditionalRoutingAssemblies`** is exposed on
  the bare host too, but making a custom `@page` Razor component
  actually route requires `MapRazorComponents<App>` with a root
  `App.razor` — significant ceremony for a one-line DI fence target.
  Kept the helper scoped to the `DocSiteOptions.AdditionalRoutingAssemblies`
  path via `SiteChromeOverrides.BuildAdditionalRoutingAssemblies`.

Entry #15 flipped to `complete`.
