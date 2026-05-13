# Examples Audit Log

Started: 2026-05-13
Approach: For each example, read its README, run the project with `dotnet run`, drive it with Playwright (or invoke build mode where dev mode does not apply), and capture issues without fixing them.

Each finding is tagged:
- **[FW]** — framework bug (something in `src/Pennington/...` misbehaves)
- **[DOC]** — documentation/README shortcoming (missing or wrong)
- **[APP]** — example app itself misconfigured/broken
- **[INFRA]** — tooling/build/dev-loop issue
- combinations allowed: `[FW+DOC]` etc.

Severity:
- **(blocker)** — example does not run or core teaching is broken
- **(major)** — significant teaching gap or broken sub-feature
- **(minor)** — polish/wording/style
- **(question)** — needs author judgement

---

## Summary

All 25 examples build and run. Highlights worth triaging:

### Cross-cutting framework concerns (touch multiple examples)
- **SPA engine prefetch leakage** between example sessions on the same port (#8, #14, #19). One-line fix in `spa-engine.js` to invalidate prefetch on host fingerprint change. — resolved 2026-05-13 (LiveReloadScriptProcessor emits per-process `<meta name="x-pennington-host">`; spa-engine.js compares and full-reloads on mismatch)
- **"Post not found" returns HTTP 200** instead of 404 in BlogSite (#7, #10) — bad for SEO and link-checkers. — resolved 2026-05-13 (BlogSite/Blog.razor sets `HttpContext.Response.StatusCode = 404` when the post resolves null)
- **`HEAD /styles.css` returns 405** while GET returns 200 (#1) — affects every example using `AddMonorailCss()`. — resolved 2026-05-13 (`UseMonorailCss` now uses `MapMethods("/styles.css", ["GET", "HEAD"], …)`)
- **Stage-1 of tutorials storing markdown/razor in C# raw strings** leaks `"""` delimiters into the rendered docs (#2). Violates `examples/CLAUDE.md`. — resolved 2026-05-13 (BeyondCustomRazorComponentExample/snippets/stage1/PricingCard.razor + `razor:path` fence; csproj excludes `snippets/**` from compile)

### Framework bugs surfaced as blockers/major
- **#5** Dev overlay for missing translations doesn't render (build report works). — resolved 2026-05-13 (verified rendering; the audit's CSS-selector probe missed the actual `#penn-diag-root` element — no framework change needed)
- **#5** English content served with `<html lang="es">` when es translation is missing. — resolved 2026-05-13 (FallbackLangHtmlRewriter rewrites `<html lang/dir>` to the served content's locale when DocSite resolves a fallback)
- **#6** TUI emits raw ANSI when stdout isn't a TTY — no line-mode fallback.
- **#11** No "on this page" outline visible in DocSiteAuthorExample despite README advertising it.
- **#13** `wwwroot/fonts/*.woff2` referenced by font-preload but the files don't exist → 404 errors on every page.
- **#15** Folder-derived section labels mangle acronyms (`core-api/` → "Core Api", should be "Core API").
- **#16** Every page in ExtensibilityLab build report is flagged "missing canonical tag" — the bare-host reference inherits this.
- **#23** SPA engine fires 2 of the 3 lifecycle events the README documents (`spa:diagnostics` never fires on normal navigation).
- **#25** Build with sub-path baseUrl is broken under Git Bash due to MSYS path translation — needs documented workaround or non-leading-slash flag.

### Documentation gaps (no working code change needed)
- `:xmldocid-diff` advertised in #4 but never demonstrated in content.
- `series:` front-matter accepted by #8 but has no visible UI.
- README placeholders ("Tutorial stages: X → Y → Z" lists) inconsistently include actual stage file names.
- Several `Beyond*` examples claim docs references that do not exist yet (`Pennington.TranslationAudit`, `Pennington.Tui`, multi-source `ApiMetadata.Reflection`).

---

## 1. BareHostRazorPageExample

**README claim:** Bare `AddPennington` host that renders Razor component via `HtmlRenderer` for `/status/{slug}/` routes. Custom `IContentService` returns `EndpointSource` so build crawler picks up the routes.

**Verified in browser (`dotnet run`, port 5000):**
- `/status/intro/` — renders `StatusPage` with title, summary, definition list. Title bound. ✓
- `/status/verify/` — renders correctly. ✓
- `/status/unknown/` — 404 as expected (route guards against missing slug). ✓
- `/` — returns 5xx/error (no root handler) — expected for this example.
- Console: 0 warnings, 0 errors. ✓
- Inline stylesheet link `/styles.css` resolves (GET 200, 205 KB MonorailCSS). ✓

**Build mode (`-- build`):** Crawler discovered both `EndpointSource` entries; `output/status/{intro,verify}/index.html` written + sitemap.xml + search-index-en.json + styles.css. 5 pages in 0.3s. ✓

**Findings:**
- **[FW] (minor)** `HEAD /styles.css` returns `405 Method Not Allowed` while `GET` returns `200`. MonorailCSS endpoint accepts only GET — could break HEAD-based health/probe checks and feed crawlers that HEAD before GET. Worth a one-line `app.MapMethods(..., new[] { "GET", "HEAD" })` or equivalent in the MonorailCSS integration. Same likely applies across every example using `AddMonorailCss()`. **Resolved 2026-05-13 (cross-cutting):** `UseMonorailCss` switched from `MapGet` to `MapMethods` with both GET and HEAD; verified on BareHostRazorPageExample (HEAD returns 200 with `text/css`).
- **[DOC] (minor)** Doc page says "View source on a rendered page — the markup ends with `</html>`, with no surrounding chrome injected by the framework." It does end with `</html>` but Pennington injects a live-reload `<script>` block before `</body>` (visible: `…cript></body></html>`). That injection is a side-effect of `UseMonorailCss`/dev-mode live-reload — the doc's "no surrounding chrome" claim is slightly misleading. Either mention the dev-time injection or note that build-mode output is the one that is truly chrome-free.
- **[DOC] (minor)** README under `Concepts` lists `GetContentTocEntriesAsync` / `GetCrossReferencesAsync` as concepts on the hand-rolled service, but the doc page does not link to a TOC/xref reference — readers landing here won't know what those return shapes mean. Either add a one-line "and why" to the README or link to a TOC/xref reference page from the how-to.

**No fixes applied.**

## 2. BeyondCustomRazorComponentExample

**README claim:** DocSite host that adds `AddMdazorComponent<PricingCard>()`, exposing `<PricingCard ... />` tags inside markdown. Stages: `Stage1_ComponentAuthored.cs` → `Stage2_RegisterMdazorComponent.cs`.

**Verified in browser (`dotnet run`, port 5000):**
- `/pricing/` — two `<PricingCard />` instances render. Basic + Pro tiers, prices $9 and $49, all features, "Most Popular" pill on the highlighted card. ✓
- DOM has exactly two `.not-prose` card wrappers. ✓
- Console: clean. ✓

**Findings:**
- **[APP+DOC] (major)** `Stage1_ComponentAuthored.cs` violates `examples/CLAUDE.md`: it stores the Razor component source as a C# raw string just so `csharp:xmldocid,bodyonly` can pull it. The rendered tutorial at `docs/Pennington.Docs/output/tutorials/beyond-basics/custom-razor-component/index.html` line 107-137 leaks the `"""` delimiters AND the leading `Source() => ` indent into the rendered code block — the reader sees literal triple-quotes wrapping the component. Fix is the convention already documented in `examples/CLAUDE.md`: ship the stage-1 minimal component as `snippets/stage1/PricingCard.razor` (or similar) and reference it with `razor:path` instead. **Resolved 2026-05-13 (cross-cutting):** stage-1 markup moved to `snippets/stage1/PricingCard.razor`; tutorial fence switched to `razor:path`; csproj `<DefaultItemExcludes>` adds `snippets\**`; rendered HTML no longer contains `"""` in the Step-2 block.
- **[DOC] (major)** The same tutorial step 1.2 says "Create a `Components/` folder and add `PricingCard.razor` with four `[Parameter]` properties" then shows a stripped-down version. The example's actual shipped component `Components/PricingCard.razor` is the *fully-styled* dark-mode-aware version, not the minimal one in Stage1. A reader who types out the Stage-1 snippet and then runs `dotnet run` from this folder will see the elaborate version because that is what's on disk — the tutorial and the shipped artifact disagree. Either evolve the component across stages on disk, or make it explicit that the disk version is the final form.
- **[FW] (minor)** `H3` headings injected via Razor components inside markdown get auto-generated `id` attributes (visible on `<h3>Tier</h3>` inside each card). This bleeds the page's "TOC outline" anchor system into ad-hoc component-internal headings. Likely a Markdig/AnglesharpHeadingId processor running over the post-Mdazor HTML. Not catastrophic but unexpected for component authors who don't want their card titles in the page outline.
- **[DOC] (minor)** README mentions "Self-closing (`<PricingCard ... />`) and open/close (`<PricingCard ...></PricingCard>`) forms" supporting `ChildContent`. The tutorial does not test the open/close form. No verification possible here without amending the example.

**No fixes applied.**

## 25. SubPathDeployableExample

**README claim:** Tiny DocSite host whose teaching surface is the sibling deployment fixtures (`.github/workflows/deploy.yml`, `staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`). `dotnet run -- build /my-sub-path` exercises `BaseUrlHtmlRewriter`.

**Verified (build mode):**
- **PowerShell** `dotnet run -- build /my-app` — `output/index.html` carries `href="/my-app/styles.css"`, `src="/my-app/_content/..."`, `data-base-url="/my-app"`. ✓ All five fixture files exist (deploy.yml, staticwebapp.config.json, netlify.toml, nginx.conf, web.config). ✓
- **Git Bash on Windows (MSYS):** same command produces output with `href="C:/Program Files/Git/my-app/styles.css"` everywhere — MSYS auto-converts the leading-slash argument to a Windows path before .NET sees it. ✗

**Findings:**
- **[DOC+INFRA] (major)** Running `dotnet run -- build /my-app` from Git Bash (the default Windows shell for many .NET devs) silently produces broken output because MSYS path translation rewrites `/my-app` to `C:/Program Files/Git/my-app`. The example's documented invocation is unsafe on Windows-with-Bash. Either show the equivalent escape (`MSYS_NO_PATHCONV=1`, double leading slash `//my-app`, or PowerShell), OR have the build accept a flag form (`--base-url my-app`) that POSIX shells won't mangle.
- **[DOC] (minor)** README claims "nested `/guides/first-page/` route exists (deep links observable)" — but my smoke build only listed 6 pages. Worth a one-line README note pointing at which fixture demonstrates which deploy concern (e.g. "`nginx.conf` shows the `try_files` rule for SPA fallback").
- **[FW] (minor)** Build doesn't emit a warning when the base-URL argument doesn't look like a relative URL — a `C:/Program Files/...` value gets baked into every link and is plausibly a sign of shell mangling that the engine could detect.

**No fixes applied.**

## 24. SpectreConsoleDocSiteExample

**README claim:** Two-area DocSite documenting `Spectre.Console` and `Spectre.Console.Cli` as separate reference trees from compiled assemblies. Multi-source `Pennington.ApiMetadata.Reflection` shape with named providers + named `AddApiReference` registrations with distinct `RoutePrefix` values.

**Verified in browser (`dotnet run`, port 5000):**
- `/console/api/` (200) — 221 `/console/api/*` links discovered on the index page. ✓
- `/cli/api/` (200) — separate tree. ✓
- Both trees coexist in one sidebar. ✓
- Console: clean. ✓

**Findings:**
- **[FW+DOC] (minor)** From the `/console/api/` index, only 1 `/cli/api/` link is reachable, suggesting the cross-tree sidebar link is a single "switch tree" affordance rather than a flat unified sidebar. Worth documenting the cross-area linking pattern: two named providers produce two trees, but how does a reader navigate between them mid-browse?
- **[DOC] (minor)** README says the example is unreferenced from docs. For a real-target showcase of multi-source `Pennington.ApiMetadata.Reflection`, add a how-to like "Document multiple NuGet packages as separate API trees" — this is a non-trivial setup readers won't reverse-engineer alone.

**No fixes applied.**

## 23. SpaPlaygroundExample

**README claim:** Minimal site exercising `spa-engine.js`. Two `data-spa-region` regions (`header`, `content`), one persistent `<nav>` (no region), nav counter survives swaps, on-page event log for **three** lifecycle events: `spa:before-navigate`, `spa:commit`, `spa:diagnostics`.

**Verified in browser (`dotnet run`, port 5000):**
- 2 `data-spa-region` elements found (`header`, `content` — main is labelled `content`, not "main"). ✓
- `<nav>` has no `data-spa-region`, has visible "PERSISTENT · no data-spa-region" tag. ✓
- Nav counter increments to 1 after one swap, confirming the nav DOM survived. ✓
- After clicking Page two link: URL updates to `/page-two/`, title updates, log records `spa:before-navigate` and `spa:commit` events with `url` / `slug` payloads. ✓

**Findings:**
- **[FW+DOC] (major)** README claims **three** lifecycle events but only **two** fire on a normal navigation: `spa:before-navigate` and `spa:commit`. `spa:diagnostics` never appears in the event log. Either the diagnostics event fires under specific conditions (dev-mode HMR? error?) the example doesn't trigger, or the third event was renamed/removed and README is stale. Document the firing conditions, or hook this example up to whatever flow actually fires the third event so the playground is exhaustive.
- **[DOC] (minor)** README says regions are `<header>` and `<main>` but the actual DOM uses `<main data-spa-region="content">` — the label is `content`, not `main`. Either rename the attribute value, or update the README.
- **[DOC] (minor)** README does not document the `page-loaded` event the log shows on first visit. It's helpful but it's not in the README's "three lifecycle events" list. Either add it as a fourth event, or label the log line as a synthetic example bootstrap (not part of the SPA engine).

**No fixes applied.**

## 22. MultipleSourcesExample

**README claim:** Two `AddMarkdownContent<T>` calls with different content roots (`Content/docs`, `Content/blog`) and different front-matter types (`DocFrontMatter`, `BlogFrontMatter`). Overlap toggled by `MULTIPLE_SOURCES_OVERLAP=1` env var.

**Verified in browser (`dotnet run`, port 5000):**
- `/docs/` (200), `/docs/about/` (200) — docs source works. ✓
- `/blog/welcome/` (200), `/blog/second-post/` (200) — blog source works. ✓
- `/` (404), `/blog/` (404) — there's no top-level index from either source. ✓ (expected behaviour given each source maps to its own base URL)

**Findings:**
- **[DOC] (major)** README mentions the env-var-toggled overlap demonstration but never describes what overlap looks like or what to expect when both sources claim the same URL. A reader running `MULTIPLE_SOURCES_OVERLAP=1 dotnet run` has nothing to anchor to. Add an explicit "with `MULTIPLE_SOURCES_OVERLAP=1`, the docs source wins because…" section.
- **[DOC] (minor)** `/` returns 404 because neither source serves `/` directly. The README doesn't mention that a multi-source host must explicitly route or redirect `/`. A reader expecting "this is the homepage" will be surprised.
- **[DOC] (minor)** README mentions `ExcludePaths` but no markdown is excluded — the teaching surface for that capability is invisible. Either include a sample excluded file with a comment explaining why, or drop it from the README's "Concepts" list.

**No fixes applied.**

## 21. GettingStartedStylingExample

**README claim:** Adds MonorailCSS to the minimal host with `AddMonorailCss` + `UseMonorailCss`, `NamedColorScheme`, brand-vs-syntax theme split. Stages 1→2→3.

**Verified in browser (`dotnet run`, port 5000):**
- `/styles.css` link present and renders. ✓
- Body class `bg-base-50 text-base-900 min-h-screen` resolved by MonorailCSS. ✓
- H1 computed color is `oklch(0.208 0.042 265.755)` — a real OKLCH ramp color, not the browser default. ✓
- 1934 utility classes discovered at startup. ✓

**Findings:**
- **[DOC] (minor)** "Brand palette vs. syntax theme split" is mentioned but the example does not actually demonstrate a syntax-highlighted code block, so the syntax-theme claim is invisible. Add a code fence in the markdown so the reader sees the syntax palette applied.
- **[DOC] (minor)** README does not link to a reference page enumerating the `NamedColorScheme` values (sand, indigo, etc.). Worth a `reference/theming/color-schemes.md` page listing the built-ins.

**No fixes applied.**

## 20. GettingStartedMinimalSiteExample

**README claim:** Smallest viable Pennington host — `AddPennington` + `AddMarkdownContent<DocFrontMatter>` + catch-all `MapGet`. Stages `Stage1_BareHost.cs` → `Stage2_AddPennington.cs` → `Stage3_UsePennington.cs`.

**Verified in browser (`dotnet run`, port 5000):**
- `/` renders with title "Welcome to your first Pennington site". ✓
- Catch-all MapGet wires through to content resolution. ✓

**Findings:**
- **[DOC] (minor)** README does not call out the unstyled output — the minimal host has no MonorailCSS wired, so the page is text without any CSS. A reader following the tutorial might wonder if something is broken. Tutorial should note that styling is the next step (`GettingStartedStylingExample`).
- **[DOC] (minor)** README's "Manual catch-all rendering loop (the shape DocSite later replaces)" claim is exactly the kind of progression a tutorial reader would benefit from. Add a sibling diff/before-after section in the tutorial that contrasts this `MapGet` shape against the DocSite `UseDocSite` shape.

**No fixes applied.**

## 19. GettingStartedFirstPageExample

**README claim:** Builds on the minimal site by adding more `.md` pages and a `NavigationBuilder`-driven nav strip. Three stages: `Stage1_OneFile.cs` → `Stage2_AddAboutPage.cs` → `Stage3_AddContactPage.cs`.

**Verified in browser (`dotnet run`, port 5000):**
- `/` renders with nav strip showing all three pages: Welcome (`/`), About (`/about/`), Contact (`/contact/`). ✓
- All three pages return 200. ✓
- Nav links match filesystem layout (`Content/about.md` → `/about/`). ✓

**Findings:**
- **[INFRA] (confirmed)** Log shows a stale SPA prefetch `/api/fusion-cache/` request hitting the new process within the first second of startup. Cross-session SPA prefetch leakage — same as #8, #14. Worth a one-line fix in `spa-engine.js` to invalidate its prefetch cache on host fingerprint change. **Resolved 2026-05-13 (cross-cutting):** dev-mode pages now carry a per-process `<meta name="x-pennington-host">` and the SPA engine clears its prefetch cache + full-reloads when a fetched doc's fingerprint differs.
- **[DOC] (minor)** README's third concept ("Rendering a nav alongside the article in a hand-rolled layout") implies the nav and article live in the same render — `dotnet run` confirms this works, but no source pointer is given. Add "(see `Stage3_AddContactPage.cs` / `Program.cs`)" so a reader knows where to look.

**No fixes applied.**

## 18. FusionCacheDocSiteExample

**README claim:** Real-target DocSite. API reference generated from `ZiggyCreatures.FusionCache` via `AddApiMetadataFromCompiledAssembly(opts => opts.FromPackageReference(...))` + `AddApiReference`. Uses `<ApiSummary>`, `<ApiMemberTable>`, `<ApiParameterTable>` Mdazor components.

**Verified in browser (`dotnet run`, port 5000):**
- `/api/` — API index renders with 10+ type links (e.g. `/api/fusion-cache/`, `/api/fusion-cache-builder-ext-methods/`, `/api/cache-key-modifier-mode/`). ✓
- `/api/fusion-cache/` — h1 "ZiggyCreatures.Caching.Fusion.FusionCache", three sections (Properties, Constructors, Methods) populated. 310+ `<code>` spans (per-member signatures). ✓
- Console: clean. ✓

**Findings:**
- **[FW+DOC] (minor)** README says `<ApiMemberTable>` is used, but the rendered type page has 0 `<table>` elements. The member listing is presumably implemented as a `<div>`-tree styled to look tabular. The README's terminology mismatches the rendered output — a reader looking for a real HTML table to style won't find one.
- **[DOC] (minor)** README says the example is unreferenced from docs. For a real-target showcase of `Pennington.ApiMetadata.Reflection`, add at least an "Auto-generating API reference from a NuGet package" how-to with this example as the canonical fence target.
- **[FW] (minor)** Type slugs use kebab-cased PascalCase split (`FusionCache` → `fusion-cache`). Multi-word acronyms might split awkwardly (e.g. `FusionCacheXMLOptions` would become `fusion-cache-x-m-l-options`). Worth a regression test on acronym-heavy type names.

**No fixes applied.**

## 17. FocusedCodeSamplesExample

**README claim:** Console app (not a website). Two implementations of a word counter so docs can fence focused methods (`Tokenize`, `Tally`, `Format`) by xmldocid, and the build report surfaces unresolved-xmldocid diagnostics on rename.

**Verified (`dotnet run`):**
- Console output prints both monolith and modular variants with identical top-3 results (the=4, dog=2, fox=2). ✓

**Findings:**
- **[DOC] (minor)** README claims "the build report's unresolved-xmldocid behaviour (rename → diagnostic)" as a teaching point, but the example doesn't actually demonstrate it — there's no docs build that consumes the xmldocids and there's no script in the folder that simulates a rename → docs build → see diagnostic. Add a `SCENARIOS.md` (or expand the README) walking through the rename loop, OR move that claim to the how-to page itself.
- **[APP] (minor)** `StringBuilderPool.cs` exists in the project but isn't referenced from either word-counter or the README. Either it's intended for a future stage, or it's leftover scaffolding — clarify.
- **[DOC] (minor)** README says "Two implementations of the same word-counter" but `ModularWordCounter.cs` and `MonolithWordCounter.cs` are the *exact same outputs*. The teaching is "look at how the structure differs" but there's no in-file narration calling out the diff. A short doc comment at the top of each file ("This is the monolith version — note how all logic is in one method") would let the file stand alone.

**No fixes applied.**

## 16. ExtensibilityLabExample

**README claim:** Bare-host kitchen-sink for every extension seam: custom highlighter, code-block preprocessor, tabbed-code class override, custom `IContentService`, emit-only `IContentService` (robots.txt), response processor, diagnostics processor, HTML rewriter, MonorailCSS customization, llms.txt opt-in.

**Verified in browser (`dotnet run`, port 5000):**
- `/` (200), `/llms.txt` (200), `/releases/` (200), `/pipeline-demo/` (200). ✓
- `/robots.txt` (404 in dev) — but build-mode emits it at `output/robots.txt`. ✓ This is the entire point of the "emit-only" service: it doesn't expose a runtime route, only writes a file at build time. README does not say so explicitly.

**Verified in build mode (`-- build`):**
- `output/robots.txt`, `output/llms.txt`, `output/sitemap.xml`, `output/search-index-en.json` all emit. ✓
- Build warnings: every page (`/`, `/releases/`, `/releases/1.1.0/`, `/releases/1.0.0/`) flagged with `Page is missing a <link rel="canonical"> tag.`

**Findings:**
- **[FW+APP] (major)** Every page in the build report carries the warning `Page is missing a <link rel="canonical"> tag.`. The ExtensibilityLab is the *canonical bare-host reference*; a reader copying its skeleton inherits a build that fails canonical tag emission. Either the bare-host needs a one-line helper to wire up canonical URLs from `SiteUrl`, or the example should explicitly demonstrate setting them on each page.
- **[DOC] (major)** README's "Emit-only `IContentService` — `RobotsTxtContentService`" item does not tell the reader that this artifact is **build-mode only** — `/robots.txt` is a 404 in dev mode. The how-to page should make this explicit (it's the central design difference between a `DiscoveredItem` and a `ContentToCreate`).
- **[DOC] (minor)** The release notes JSON-backed service generates `/releases/1.0.0/` and `/releases/1.1.0/` (semver in URL); README mentions "JSON-backed" but a reader trying to probe these by intuition would more naturally write `/releases/v1.0/`. Add the slug convention to the README.
- **[INFRA] (minor)** This example lights up 10+ extension seams in one project. That makes it useful as a reference but unwieldy to teach from. Worth a "minimal index" file in this folder that maps each registered service to its single-purpose how-to so a reader knows where to start reading. The README has the list, but no in-project links.

**No fixes applied.**

## 15. DocSiteSectionsExample

**README claim:** Two areas, each broken into two subfolder-backed sections. `order:` / `section:` front matter drive sidebar grouping; subfolders become non-navigable section headers.

**Verified in browser (`dotnet run`, port 5000):**
- `/guides/` sidebar shows two section headers ("Getting Started", "Advanced") with their pages grouped beneath each. ✓
- `/reference/` shows two sections ("Core Api", "Extensions"). ✓
- Page ordering within each section appears correct. ✓
- Console: clean. ✓

**Findings:**
- **[FW] (major)** Folder-name-derived section labels use simple title-case (`first letter upper, rest lower`) — `core-api/` becomes "Core Api" instead of "Core API". `NavigationBuilder` should either expose a section-label override in front matter (`section:` on a parent `index.md`?) or use a smarter casing rule that preserves all-caps acronyms. Either way, the example produces a visibly wrong label.
- **[DOC] (minor)** README says ordering uses `order:` "with title as the tiebreaker". A reader exploring `installation.md`, `configuration.md`, `first-project.md` would benefit from a callout showing what happens without `order:` — does the engine alphabetize the slug, the title, or fall back to filename?
- **[DOC] (minor)** README doesn't link to or define `section:` front matter — the key is mentioned but its semantics ("override the folder-derived label") are not explained inline. Confirm whether any of this example's pages actually exercise `section:` or if it's purely demonstrating folder-based grouping.

**No fixes applied.**

## 14. DocSiteScaffoldExample

**README claim:** Smallest DocSite — `AddDocSite`/`UseDocSite`/`RunDocSiteAsync` with two areas (`Guides`, `Reference`).

**Verified in browser (`dotnet run`, port 5000):**
- `/guides/` and `/reference/` both render with correct area-prefixed titles. ✓
- Console: clean. ✓

**Findings:**
- **[INFRA] (minor)** First request on startup hit `/main/ui-components-in-markdown/` (logged in process). That URL doesn't exist in this example — it's the route from the previous DocSiteKitchenSink run still cached in the live-reload SPA engine or browser history. Suggests the SPA prefetch persists across hot-reload restarts where the same port is recycled. Same observation as #8. **Resolved 2026-05-13 (cross-cutting):** dev-mode pages now carry a per-process `<meta name="x-pennington-host">` and the SPA engine clears its prefetch cache + full-reloads when a fetched doc's fingerprint differs.
- **[DOC] (minor)** README claims `UseDocSite` orders middleware (locale → antiforgery → static files → routing → MonorailCSS → SPA → Pennington middleware). That order is invisible to the reader unless they trace `src/Pennington.DocSite`. A diagnostics endpoint that dumps the middleware order would make this concrete; absent that, document the order on the `reference/host/extensions.md` page explicitly.

**No fixes applied.**

## 13. DocSiteKitchenSinkExample

**README claim:** Wide-surface kitchen-sink DocSite — two areas (`main`, `api`), two locales (default + `fr`), custom color scheme, font preloads (`/fonts/display.woff2`, `/fonts/body.woff2`), extra CSS, custom Mdazor component `FeatureCallout`, custom footer, GitHub URL, search opt-out (`hidden.md`), llms.txt opt-out (`llms-hidden.md`).

**Verified in browser (`dotnet run`, port 5000):**
- All routes return 200: `/`, `/llms.txt`, `/search-index-en.json`, `/fr/`, `/api/`. ✓
- `/llms.txt` lists both areas and the `Fr` localized tree. ✓
- `FeatureCallout` component renders 3 callouts on `/main/ui-components-in-markdown/` with `Title`, `Kind` (tip/info/warn → primary/accent/amber colour ramps), `Icon` (bolt/book/shield → ⚡/📘/🛡 glyphs). ✓
- Console: **2 errors** — `Failed to load resource: 404 fonts/body.woff2` and `fonts/display.woff2`. ✗

**Findings:**
- **[APP] (major)** Font preload references `/fonts/display.woff2` and `/fonts/body.woff2` in the rendered HTML head, but the files do not exist in `wwwroot/fonts/` (neither file is present). Every page loads with two console 404 errors and "font preload" teaching surface (a key README concept) is broken on disk. Either ship placeholder font files (open-source faces, properly licensed), or change `FontFamilies` to point at fonts that ship with MonorailCSS, or document the expected file paths so a reader can drop their own font in.
- **[FW+DOC] (minor)** README lists "Sidebar customization (sections, ordering, hidden pages, redirects)" — verified in `/llms.txt` listing that `hidden.md` and `llms-hidden.md` are excluded from the respective indexes, but the rendered sidebar effect isn't directly testable from a single page. Worth a separate sidebar-visibility how-to.
- **[FW] (minor)** The 404 on font files happens despite `wwwroot/` directory existing on the example — `wwwroot` is set up but empty of fonts. Pennington's font-preload helper should either log a build-time warning when a referenced font file does not exist, or fall back gracefully.

**No fixes applied.**

## 12. DocSiteChromeOverridesExample

**README claim:** Four chrome seams exercised — head-slot fragment, custom routed `@page`, `AdditionalRoutingAssemblies`, `DocSiteOptions` header/footer/layout overrides.

**Verified in browser (`dotnet run`, port 5000):**
- `/extra` — routed Razor component returns 200, page title "Extra Page", h1 "Extra Page". ✓ (AdditionalRoutingAssemblies seam)
- `/` head includes the custom fragment marker `<meta name="x-chrome-overrides-head" content="extra-head-fragment">` plus `og:site_name` and `twitter:site` from the override. ✓ (head-slot fragment seam)
- Header shows custom "Chrome Overrides" branding (`DocSiteOptions` header override). ✓

**Findings:**
- **[DOC] (minor)** README states the four seams but doesn't enumerate the actual artifacts (file paths in the example) that map to each. A reader looking for "head slot fragment" has to grep — adding "→ `Components/ExtraHeadFragment.razor`" beside each bullet would be friendlier.
- **[FW+DOC] (minor)** The custom `@page "/extra"` route does not use a trailing slash (DocSite's other routes do). `/extra` returns 200; `/extra/` also returns 200 (presumably via redirect or both — not verified). README could call out the canonical form to use.
- **[DOC] (minor)** README does not mention what happens if you set `AdditionalRoutingAssemblies` to a list containing a non-routable assembly — does the host throw, log a warning, or silently ignore? Worth documenting since this is a public API surface.

**No fixes applied.**

## 11. DocSiteAuthorExample

**README claim:** Single-area DocSite focused on authoring concepts: `DocSiteFrontMatter` keys, alerts, tabbed code groups, outline nav from h2/h3.

**Verified in browser (`dotnet run`, port 5000):**
- `/guides/authoring/` — h1 "Authoring a doc page", page title binds. ✓
- H2 sections "Front matter", "Callouts", "Tabbed code groups" all present. ✓
- 2 alert/callout blocks rendered. ✓
- 4 tab-related elements (panels/lists). ✓
- Outline/aside detection found no element matching `aside`, `[class*=outline]`, or `[class*=on-this-page]`. ✗
- Console: clean. ✓

**Findings:**
- **[FW] (major)** No "on this page" outline visible. README specifically advertises "Outline nav generated from `h2`/`h3` in the rendered HTML" as a teaching concept — the rendered page has three h2 sections perfect for outlining, but the rendered DOM contains no `aside`, no element with `outline`/`on-this-page` classes, and no nav matching that pattern. Either the OutlineNav component is gated on a viewport breakpoint (e.g. `xl:`) hiding it in tests, or it's not wired in this example. If it's breakpoint-gated, README should mention the viewport width needed; if not wired, fix the example or remove the claim.
- **[DOC] (minor)** README mentions "alerts" and "tab groups" as part of markdown extensions but does not link to the `reference/markdown/extensions.md` page that ostensibly fences stage2/stage3 of this example. Add a cross-link so readers can jump to the extension reference.
- **[DOC] (minor)** Front matter keys `uid` and `tags` are listed in README but `Content/guides/authoring.md` may or may not exercise all of them — worth a glance.

**No fixes applied.**

## 10. BlogSiteScaffoldExample

**README claim:** Smallest BlogSite — `AddBlogSite`/`UseBlogSite`/`RunBlogSiteAsync` with `Content/Blog/hello-world.md` produces `/`, `/archive`, `/blog/<slug>`, `/tags`, `/tags/<name>`, and `/rss.xml`. Stages `Stage1_BeforeAddBlogSite.cs` → `Stage2_AfterAddBlogSite.cs`.

**Verified in browser (`dotnet run`, port 5000):**
- All routes return 200: `/`, `/archive/`, `/tags/`, `/rss.xml`, `/sitemap.xml`, `/blog/hello-world/`. ✓
- `/blog/hello-world/` h1 "Hello world", author surfaced, Article JSON-LD present. ✓
- Console: clean. ✓

**Findings:**
- **[DOC] (minor)** README's bullet "BlogSite template defaults (`BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`)" implies these are showcased — but the scaffold uses defaults, so a reader sees no `BlogContentPath = ...` etc. assignment in `Program.cs`. The README should clarify that these are *defaults* that you can read about in reference docs.
- **[DOC] (minor)** README mentions `/tags/<name>` as a route but with only one post the `/tags/<name>` route is reachable only by guessing. Add `<TagsBadge>` or "View tag" links from the post page to make this discoverable.
- **[FW] (minor)** Same "Post not found" 200-status issue observed in #7 is implicit here: any wrong slug returns 200 with stub content instead of 404. (Not retested.) **Resolved 2026-05-13 (cross-cutting):** `BlogSite/Blog.razor` now sets `HttpContext.Response.StatusCode = 404` when `BlogContentResolver` returns null; verified on this example's scaffold.

**No fixes applied.**

## 9. BlogSiteHeroProjectsSocialsExample

**README claim:** Populates four `BlogSiteOptions` homepage surfaces (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`) with four built-in `SocialIcons` render fragments (Github, Bluesky, LinkedIn, Mastodon).

**Verified in browser (`dotnet run`, port 5000):**
- `/` — h1 "Field notes from a weekend content engine" (HeroContent). ✓
- 4 GitHub project cards + 1 main GitHub link (MyWork section). ✓
- 4 social links to all four advertised platforms: github.com, bsky.app, www.linkedin.com, hachyderm.io. ✓
- 10 inline `<svg>` elements (social icons + project icons). ✓
- Stages exist: `Stage1_HeroOnly.cs`, `Stage2_AddProjects.cs`, `Stage3_AddSocialsAndHeader.cs`. ✓
- Console: clean. ✓

**Findings:**
- **[DOC] (minor)** README claims "MastodonIcon" but the social link points at `hachyderm.io` — there's no `mastodon.social` or similar generic URL. That's fine but the test/teaching example should make clear that Mastodon icon takes any `@user@instance` URL and renders the generic icon.
- **[FW+DOC] (minor)** Hero `<h1>` and project section share heading hierarchy on the home page (multiple h1/h2). No standout teaching of how to keep accessibility headings sane when both `HeroContent` and `Projects` titles compete.
- **[DOC] (minor)** The README lists "Tutorial stages" but the tutorial page at `docs/.../tutorials/blogsite/hero-projects-socials.md` should be checked to confirm each stage is actually fenced. (Not verified live; would require navigating docs build.)

**No fixes applied.**

## 8. BlogSiteFirstPostExample

**README claim:** Extends scaffold by populating `Content/Blog/my-first-post.md` with every `BlogSiteFrontMatter` field a reader will touch (title, date, author, tags, series, repository, summary), and `EnableRss` / `EnableSitemap` / `CanonicalBaseUrl` set explicitly.

**Verified in browser (`dotnet run`, port 5000):**
- `/` — landing lists `/blog/my-first-post/`. ✓ (no date prefix in URL — URL = filename verbatim)
- `/blog/my-first-post/` — h1 "Shipping a tiny content engine for weekend projects". ✓
- Page surfaces: author (Author Name) ✓, date (April 10) ✓, tags (pennington, dotnet, blogging) ✓, repo link to `github.com/example/pennington-field-notes` ✓, Article JSON-LD with author.name ✓.
- Page does NOT surface `series: Pennington Field Notes`. ✗ (front matter contains it; no visible "Part of series:" affordance)
- `/rss.xml` — 1 `<item>` with title, description, date, author, link via `CanonicalBaseUrl`. ✓
- Console: clean. ✓

**Findings:**
- **[FW+DOC] (major)** The `series` front-matter field has no visible UI in the rendered post. README explicitly lists "series" as a `BlogSiteFrontMatter` field a reader will touch, and the tutorial promises "every key…lights up a different surface — the archive card, the post header, the `/tags/<tag>` listings, the RSS channel, the JSON-LD metadata". A reader following the tutorial will set `series:` and see nothing in the page. Either ship a `<SeriesBadge>` component in the BlogSite template, or document that `series:` is data-only and surface it some other way (e.g., series index page at `/series/<slug>/`).
- **[DOC] (minor)** README lists `redirectUrl:` indirectly under "every field" (`my-first-post.md` has `redirectUrl:` blank). What does `redirectUrl` do? The blank value teaches nothing. Either include a meaningful example or call it out as opt-out.
- **[DOC] (minor)** Front matter has `sectionLabel: field-notes`. With one post, the rendered effect of `sectionLabel` is invisible. The reader who reads the front matter sees a field that does nothing.
- **[FW] (minor)** Same "Post not found" 200 status from #7 applies if you guess the URL wrong here. (Not retested, but the BlogSite template is shared.)
- **[INFRA] (minor)** Server log shows incoming traffic for `/blog/2024-01-15-getting-started-with-pennington/` before any browser request, suggesting a stale prefetch from the previous BlogKitchenSinkExample run hit this process via a kept-alive SPA connection or browser tab. Worth verifying that the SPA-engine's prefetch cache isn't accidentally cross-contaminating across separate ports/sessions. **Resolved 2026-05-13 (cross-cutting):** dev-mode pages now carry a per-process `<meta name="x-pennington-host">` and the SPA engine clears its prefetch cache + full-reloads when a fetched doc's fingerprint differs.

**No fixes applied.**

## 7. BlogKitchenSinkExample

**README claim:** Kitchen-sink BlogSite with full `BlogSiteOptions` surface, JSON-LD `StructuredDataBuilder`, RSS + sitemap, 3 dated posts so archive/tags/RSS are populated.

**Verified in browser (`dotnet run`, port 5000):**
- `/` — landing page renders hero ("Field notes from the Pennington workshop"), 3 article cards. ✓
- 5 GitHub social-icon links present + main-site links (Home, Archive, Tags, About). ✓
- `/archive/` (200), `/tags/` (200) — both populated. ✓
- `/sitemap.xml` — 14 `<url>` entries. ✓
- `/rss.xml` — 3 `<item>` entries. ✓
- `/feed.xml` — 404. ✗
- `/blog/getting-started-with-pennington/` — 200 but "Post not found" (the actual route is dated `/blog/2024-01-15-getting-started-with-pennington/`).
- `/blog/2024-01-15-getting-started-with-pennington/` — renders the post. JSON-LD `Article` block present. ✓
- Landing page has WebSite JSON-LD with proper @context + url + description. ✓
- Console: clean. ✓

**Findings:**
- **[FW+DOC] (major)** Blog post URLs include the leading `YYYY-MM-DD-` slug taken from the filename (`2024-01-15-getting-started-with-pennington`). This is implementation detail leaking into the public URL — readers expect a `/blog/<slug>/` route based on the front matter's `slug` or stripped filename, not the raw filename. README does not document the date-prefix convention. Either strip the leading date prefix when computing route slugs (and document it), or document the current behaviour and explain the recommendation.
- **[FW+DOC] (major)** Page returns 200 for `/blog/getting-started-with-pennington/` rendering "Post not found" body. Should be a proper 404. Returning 200 on a not-found surface poisons search-engine indexing and any link-checking tool relying on HTTP status. **Resolved 2026-05-13 (cross-cutting):** `BlogSite/Blog.razor` now sets `HttpContext.Response.StatusCode = 404` when `BlogContentResolver` returns null; verified `/blog/getting-started-with-pennington/` returns 404.
- **[DOC] (minor)** README mentions "RSS channel + sitemap" but does not specify the URLs (`/rss.xml`, `/sitemap.xml`). For a docs-feed example, the discoverable path is part of the teaching.
- **[FW] (minor)** Individual blog post page has no `og:title` / `og:description` / `twitter:card` meta tags. For a kitchen-sink BlogSite example that includes structured data (`StructuredDataBuilder`), Open Graph / Twitter Card metadata would be the natural sibling. README says "structured data" — OG is the most common form most readers will expect.
- **[DOC] (minor)** README references `docs/.../how-to/feeds/rss.md` and `docs/.../how-to/feeds/sitemap.md`. Verify those pages actually `xmldocid` into `ServiceConfiguration` helpers — broken xref would silently break the docs.

**No fixes applied.**

## 6. BeyondTuiExample

**README claim:** `AddPenningtonTui` enables a dev-time TUI dashboard. Under `dotnet run -- build` the TUI hosted service no-ops so static publish is unaffected.

**Verified:**
- **Build mode (`-- build`)** — runs cleanly with no TUI artifact; emits standard "Build Complete — 5 pages in 0.6s". ✓
- **Dev mode (`dotnet run`)** — process binds HTTP 5000, content renders (`/` returns 200, h1 "Beyond TUI"), but stdout is dominated by raw ANSI escape sequences the TUI emits (captured as `Pp` glyphs in a non-TTY). DocSite content itself works.

**Findings:**
- **[FW+DOC] (major)** When stdout is not a TTY (CI logs, container logs, `dotnet run > log.txt`), the TUI emits raw ANSI but nothing readable. The README says "under `dotnet run` the terminal hosts a full-screen validator dashboard" — but does not mention what happens when the terminal is not a TTY. Either fall back to line-mode (write the same validator messages as regular log lines) when `Console.IsOutputRedirected` is true, or document the non-TTY fallback explicitly. As-is, redirecting output of a dev run is unusable for grepping.
- **[DOC] (minor)** README mentions "dry-run validator on startup + debounced re-runs on file change" — those terms aren't defined anywhere in the docs site. A reference/how-to page documenting what the validator checks (broken xrefs, missing translations, dead links, etc.) would be valuable so readers know what value the TUI adds.
- **[DOC] (minor)** README says the example is unreferenced. Add a how-to/reference page for `Pennington.Tui` so this example becomes discoverable from the docs site.

**No fixes applied.**

## 5. BeyondTranslationAuditExample

**README claim:** `AddPenningtonTranslationAudit` registers an `IBuildAuditor`. Spanish `getting-started.md` is deliberately missing. Auditor produces "missing es translation" warning visible in dev overlay AND in `dotnet run -- build` diagnostics.

**Verified in browser (`dotnet run`, port 5000):**
- `/es/getting-started/` — page returns 200 with English content but `<html lang="es">`. (English fallback served when translation missing.)
- DOM has no fixed-position element matching `[id*=overlay] [class*=audit] [class*=translation]` etc. No visible dev overlay surface on the page. ✗

**Verified in build mode (`dotnet run -- build`):**
- Build report explicitly emits:
  ```
  Build Complete — 11 pages in 0.5s
    11 pages generated
    1 warnings
  WARNINGS
    /getting-started/: Missing Español (es) translation for "Getting Started" (/getting-started/).
  ```
- ✓ Audit warning surfaces with locale label, route, and title. This half of the claim works perfectly.

**Findings:**
- **[FW] (major)** README claims the dev overlay surfaces the translation warning when visiting `/es/getting-started/`. No overlay is visible in the rendered page — no fixed-position element, no Pennington-branded badge, no banner. Either the overlay opt-in is implicit and not actually wired in the example, or the overlay UI is gated behind a toggle the README doesn't mention, or the overlay was never built. Build-report half works; UI half does not appear. **Resolved 2026-05-13 (framework-blocker re-check):** the overlay does render — `#penn-diag-root` is positioned fixed at `bottom:20px;right:20px;z-index:99999` and `#penn-diag-badge` reads "1 warning" on `/es/getting-started/`. The original audit's CSS selectors (`[id*=overlay]`, `[class*=audit]`, `[class*=translation]`) just never matched the actual id `penn-diag-root`. Verified via Playwright (`audit5-overlay-verification.png`). No framework change required.
- **[FW+DOC] (major)** When a translation is missing on a non-default locale, the fallback page renders with `<html lang="es">` (the request locale) but English body content. Either the fallback should switch `lang` to the actual content locale (`en`), or the page should show some user-visible indicator that this is an English fallback. The audit knows; the rendered page does not. **Resolved 2026-05-13 (framework-blocker):** DocSite's `Pages.razor` stashes the actual content locale in `HttpContext.Items["Pennington.FallbackContentLocale"]` when a fallback resolves, and the new `FallbackLangHtmlRewriter` (Order=40) rewrites `<html lang/dir>` to match. Verified: `/es/getting-started/` now renders `<html lang="en">`, while real `/es/` content keeps `<html lang="es">`. (Visible `<FallbackNotice />` was already shipped for the second remediation option.)
- **[DOC] (minor)** README says "Repository auto-discovers from the current working directory." Worth documenting what happens when the cwd isn't a git repo or the example is consumed outside a clone (e.g. user copies the folder into their own repo).
- **[DOC] (minor)** README says the example exists "as a working reference for the `Pennington.TranslationAudit` package" but the package is in the docs site nowhere — there is no how-to or reference page linking back. Either add one, or add at least a "see also" callout on the example so the docs surface this audit workflow.

**No fixes applied.**

## 4. BeyondRoslynExample

**README claim:** `AddPenningtonRoslyn` against the sibling `Sample/` library. Markdown fences resolve `:xmldocid`, `:xmldocid,bodyonly`, `:xmldocid-diff`, and `:path` against the inner `BeyondRoslynExample.slnx`.

**Verified in browser (`dotnet run`, port 5000):**
- Symbol warmup logs `Symbol extraction warmup completed in 1809ms`. ✓
- `/api-pulls/` renders 5 distinct code blocks:
  1. `T:Calculator` — full class with xmldoc comments. ✓
  2. `M:Calculator.Add(...)` — method with xmldoc. ✓
  3. `M:Calculator.Multiply(...)` + `,bodyonly` — single line `return a * b;`. ✓ (correctly strips declaration)
  4. `T:Greeter` — full class with xmldoc. ✓
  5. Two M-ids in one fence — both members concatenated. ✓
- Console: clean. ✓

**Findings:**
- **[DOC+APP] (minor)** README explicitly advertises `:xmldocid-diff` as part of the teaching surface but no markdown in this example uses it. Tutorial body (line 88) also lists `:xmldocid-diff` in the fence-modifier menu, then never demonstrates it. Either add a section in `api-pulls.md` showing a before/after with two `M:` IDs and the diff fence, or drop `:xmldocid-diff` from both the README and the tutorial fence-modifier list and surface it in an explanation-quadrant page instead.
- **[DOC] (minor)** README "Tutorial stages" section is a placeholder: it says only "The inner `BeyondRoslynExample.slnx` + `Sample/` library is part of the teaching surface." but the example has actual staged C# files (`Stage1_NoRoslyn.cs`, `Stage2_AddRoslyn.cs`). The standard pattern from `examples/CLAUDE.md` is `Stage1 → Stage2 → …` — call them out by name in the README so consumers know to look for them.
- **[DOC] (minor)** README's "Concepts" list mentions `<DefaultItemExcludes>` keeping `Sample/` out of the host's compile — this is true but the tutorial buries it as a one-liner in step 1.2 ("set `DefaultItemExcludes` … otherwise the two projects compete over the same `.cs` files"). Promote it: a reader copying the csproj fragment from the tutorial today won't see the `<DefaultItemExcludes>` line, only a generic instruction to add it.

**No fixes applied.**

## 3. BeyondLocaleExample

**README claim:** DocSite + `ConfigureLocalization` adds a second URL-prefixed locale (`es`); content lives under `Content/<locale>/`; `LanguageSwitcher` appears once `Locales.Count > 1`; translations registered via `ConfigurePennington` escape hatch.

**Verified in browser (`dotnet run`, port 5000):**
- `/` — English landing page, title "Welcome — Beyond Locale", `<html lang="en">`. ✓
- `/es/` — Spanish landing page, title "Bienvenido — Beyond Locale", `<html lang="es">`, h1 "Bienvenido". ✓
- `/about/` — English about page. ✓ / `/es/about/` — Spanish about page. ✓
- `/es/missing-page/` — 404 (no fallback). ✓
- Language switcher: both `English` (`/`) and `Español` (`/es/`) links present in nav. ✓
- Console: clean. ✓

**Findings:**
- **[FW+DOC] (major)** When a locale-specific page is missing on the `es` side, Pennington returns a generic English "Not Found" page (title `Not Found`, English chrome) at `/es/missing-page/`. For a localization tutorial this is the very fallback behaviour the reader would expect to be exercised/documented. README does not mention fallback semantics. The behaviour itself may be by design (English fallback) but the README/doc should state it explicitly and ideally the 404 page should be localized when the request locale resolves to a non-default locale.
- **[DOC] (minor)** README's bullet "`LanguageSwitcher` lighting up once `Locales.Count > 1`" implies a discoverable component named `LanguageSwitcher`. In the rendered DOM there is no element matching `[class*=language-switcher]` — the two locale links are plain `<a>` elements inside the header. Either the component is named differently in the markup, or it doesn't decorate itself with a recognisable class. Either way the README's terminology doesn't match what a reader inspecting the page can find.
- **[DOC] (question)** The `TranslationRegistration.cs` mention in `Program.cs` says "TranslationOptions registered through the ConfigurePennington escape hatch." That works, but it's worth checking whether a first-class `DocSiteOptions.Translations` shortcut would simplify the tutorial. Author judgement.
- **[INFRA] (minor)** Sibling SPA reload connection sees an abort within a couple hundred ms after first request (`Connection id "0HNLH31PSG6UG"…the application aborted the connection`) — non-fatal but appears in every example run that loads the SPA engine.

**No fixes applied.**

