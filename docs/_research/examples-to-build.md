# Examples to Build

Build queue for the 16 example apps that back the Pennington docs (`docs/docs-toc.md`). Each entry is self-contained so an AI agent can pick up exactly one app and build it without reading the rest of this file.

Update the **Status** line on each entry as work progresses:

- `not_started` — no project exists yet
- `scaffolded` — project, csproj, and slnx entry exist; builds cleanly; no content or features wired
- `features_wired` — all listed features demonstrated in source; app runs in dev; app builds statically
- `docs_linked` — `docs/_research/examples-inventory.md` carries a verified xmldocid symbol table for this app; `docs/docs-manifest.yaml` evidence entries updated
- `complete` — all of the above; `tools/docs-lint.ps1` passes

## Design principles

1. **Tutorials get one app each, 1:1.** Tutorial apps are small, low-ceremony, minimal UI (unless the topic is UI). Apps 1–12 each back exactly one tutorial page.
2. **How-tos share kitchen-sink demos.** One how-to demo app backs many how-to pages via `xmldocid,bodyonly` fences into specific symbols or `path` fences into specific files. Apps 13–16 each back a whole section of how-tos.
3. **Stage files carry intermediate tutorial states.** `xmldocid,bodyonly` extracts a symbol's body without requiring the code to be wired into the running host. A tutorial app may carry multiple `StageN_*.cs` files that compile but are never instantiated — only the final stage is in `Program.cs`. Stage files are referenced from tutorial prose via `xmldocid,bodyonly`.
4. **No new apps for Reference or Explanation.** Those pages link into the tutorial and how-to apps.

## Shared conventions

- Folder: `examples/<AppName>/`, csproj of the same name
- All projects inherit `examples/Directory.Build.props` (sets `IsPackable=false`)
- Top-level `Program.cs` is the canonical entry point
- Content lives under `Content/`
- Each app is registered in `Pennington.slnx`
- When a new app is built, the agent updates `docs/_research/examples-inventory.md` with a verified xmldocid symbol table and updates `docs/docs-manifest.yaml` evidence entries for the backing doc pages

---

## Tutorial apps (12)

### 1. `GettingStartedMinimalSiteExample`

- **Status:** complete
- **Backs:** Tutorial §1.1.10 `/tutorials/getting-started/first-site`
- **Goal:** Bootstrap a minimal ASP.NET host with `AddPennington` + `UsePennington`, `ContentRootPath` pointing at a folder of markdown, dev-mode hot reload, one page renders with front matter.
- **Shape:**
  - `Program.cs` — final wired state
  - `Content/index.md` — one page with `title:` front matter
  - Stage files: `Stage1_BareHost.cs`, `Stage2_AddPennington.cs`, `Stage3_UsePennington.cs` (compile-only, never instantiated)
- **Does not cover:** DocSite template, styling, deployment

### 2. `GettingStartedFirstPageExample`

- **Status:** not_started
- **Backs:** Tutorial §1.1.20 `/tutorials/getting-started/first-page`
- **Goal:** Write a YAML front-matter block, required `title` key, file path → URL, nav auto-assembles as a second and third file are added.
- **Shape:**
  - `Program.cs`
  - `Content/index.md`, `Content/about.md`, `Content/contact.md`
  - Stage files showing a one-file site, two-file site, three-file site transitions if useful
- **Does not cover:** custom front-matter types, capability interfaces, non-markdown sources

### 3. `GettingStartedStylingExample`

- **Status:** not_started
- **Backs:** Tutorial §1.1.30 `/tutorials/getting-started/styling`
- **Goal:** Register `AddMonorailCss` + `UseMonorailCss`, pick a `NamedColorScheme`, add a utility class to a layout, stylesheet regenerates on demand.
- **Shape:**
  - `Program.cs` with MonorailCSS registered
  - A minimal layout component using utility classes
  - Same content shape as app 2
- **Does not cover:** algorithmic color schemes, custom `CssFrameworkSettings`, dark-mode wiring, deployment

### 4. `DocSiteScaffoldExample`

- **Status:** not_started
- **Backs:** Tutorial §1.2.10 `/tutorials/docsite/scaffold`
- **Goal:** Replace barebones setup with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, configure `DocSiteOptions` (site title, GitHub URL, header/footer), demonstrate areas mapping to top-level folders.
- **Shape:**
  - `Program.cs` with `AddDocSite` + `DocSiteOptions`
  - Two areas (`Content/guides/`, `Content/reference/`) each with one page
- **Does not cover:** authoring (next tutorial), layout overrides (extensibility how-to)

### 5. `DocSiteAuthorExample`

- **Status:** not_started
- **Backs:** Tutorial §1.2.20 `/tutorials/docsite/first-doc-page`
- **Goal:** Write a page with `DocSiteFrontMatter` (title, description, tags, section, order), add an alert + a tabbed code group, outline nav populates.
- **Shape:**
  - `Program.cs` (`AddDocSite`)
  - `Content/guides/authoring.md` with `DocSiteFrontMatter` plus a `> [!NOTE]` alert and a tabbed code group
- **Does not cover:** cross-references, snippets, diagrams

### 6. `DocSiteSectionsExample`

- **Status:** not_started
- **Backs:** Tutorial §1.2.30 `/tutorials/docsite/sections-and-areas`
- **Goal:** Structure `Content/` into areas and sections, use `section:` / `order:` in front matter, `NavigationBuilder` turns the flat tree into a sidebar.
- **Shape:**
  - Two areas, each with two named sections, each section with 2–3 ordered pages
- **Does not cover:** locale navigation, Razor-page integration, custom `IContentService`

### 7. `BlogSiteScaffoldExample`

- **Status:** not_started
- **Backs:** Tutorial §1.3.10 `/tutorials/blogsite/scaffold`
- **Goal:** `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, core `BlogSiteOptions` (`SiteTitle`, `AuthorName`, `CanonicalBaseUrl`, content paths). Show the difference vs DocSite defaults.
- **Shape:**
  - `Program.cs`
  - Empty or nearly-empty `Content/Blog/`
  - Stage files for before/after the AddBlogSite call
- **Does not cover:** authoring posts, homepage customization

### 8. `BlogSiteFirstPostExample`

- **Status:** not_started
- **Backs:** Tutorial §1.3.20 `/tutorials/blogsite/first-post`
- **Goal:** Post authored with `BlogSiteFrontMatter` (title, description, date, author, tags, series, repository, section, redirectUrl), post appears on blog index, built-in RSS enabled. `AddBlogSite` wires `BlogSiteFrontMatter` (not `BlogFrontMatter`) — the tutorial teaches this record.
- **Shape:**
  - `Program.cs` extending app 7 with `EnableRss = true`
  - `Content/Blog/my-first-post.md` with full `BlogSiteFrontMatter`
- **Does not cover:** post-template customization, tag-index page

### 9. `BlogSiteHeroProjectsSocialsExample`

- **Status:** not_started
- **Backs:** Tutorial §1.3.30 `/tutorials/blogsite/hero-projects-socials`
- **Goal:** Populate `HeroContent`, `Project[]` for "my work", `SocialLink[]` (with built-in `BlueskyIcon`/`GithubIcon`), `HeaderLink[]` for top-nav.
- **Shape:**
  - `Program.cs` extending app 8 with homepage data populated
  - One post so the homepage has a recent-post slot
- **Does not cover:** custom icon components

### 10. `BeyondLocaleExample`

- **Status:** not_started
- **Backs:** Tutorial §1.4.10 `/tutorials/beyond-basics/add-a-locale`
- **Goal:** Enable `LocalizationOptions` (`DefaultLocale`, `Locales`), create a locale subdirectory with translated markdown, wire `UsePenningtonLocaleRouting`, add `LanguageSwitcher` component.
- **Shape:**
  - `Program.cs` with two locales
  - `Content/en/` and `Content/es/` each with the same 2–3 pages translated
  - A layout consuming `<LanguageSwitcher>`
- **Does not cover:** per-locale search index internals, UI string translation plumbing

### 11. `BeyondRoslynExample`

- **Status:** not_started
- **Backs:** Tutorial §1.4.20 `/tutorials/beyond-basics/connect-roslyn`
- **Goal:** Point Pennington at a `.sln`/`.slnx` via `SolutionPath`, use `xmldocid` code fences to pull method/class snippets straight from source, hot reload updates docs when source changes.
- **Shape:**
  - `BeyondRoslynExample.csproj` — docs host using `AddPenningtonRoslyn`
  - Sibling `BeyondRoslynExample.Sample.csproj` — library project with 2–3 small types whose members are referenced via xmldocid from markdown
  - `BeyondRoslynExample.slnx` at the app folder root
  - `Content/` pages that fence `csharp:xmldocid="M:...Sample.Type.Method"`
- **Does not cover:** full API-reference generation (planned `Pennington.Roslyn` reference-page feature)

### 12. `BeyondCustomRazorComponentExample`

- **Status:** not_started
- **Backs:** Tutorial §1.4.30 `/tutorials/beyond-basics/custom-razor-component`
- **Goal:** Build a Razor component, register with `AddMdazorComponent<T>()`, render from markdown.
- **Shape:**
  - `Program.cs` with `AddMdazorComponent<PricingCard>()`
  - `Components/PricingCard.razor`
  - `Content/pricing.md` consuming `<PricingCard />`
- **Does not cover:** packaging reusable component libraries, Mdazor internals in full

---

## How-to demo apps (4)

### 13. `DocSiteKitchenSinkExample`

- **Status:** not_started
- **Backs:** most of how-to §2.1 Content Authoring + DocSite-flavoured §2.2 Configuration
- **Shape:** `AddDocSite` host. Configuration is deliberately layered (fonts, monorail, search, multi-source, llms-txt, sitemap, localization). Some features coexist; some are kept on isolated content slices so each how-to snippet has a targetable symbol.
- **Structure:**
  - `Program.cs` wiring it all
  - `Content/main/` (primary source, custom front matter, tags/ordering, redirects)
  - `Content/api/` (second `AddMarkdownContent<T>` source with `ExcludePaths`)
  - `Content/en/`, `Content/fr/` (localized content)
  - One `Content/**/*.md` file per how-to feature, named after the how-to (e.g. `alerts.md`, `tabbed-code.md`, `code-annotations.md`, `cross-references-a.md` + `cross-references-b.md`, `mermaid.md`, `ui-components.md`, `redirect-source.md`, `hidden.md`)
  - `wwwroot/shared.png`, `Content/main/assets/colocated.png`
  - Custom `SearchIndexOptions.ContentSelector` + one `search: false` page
  - `CustomCssFrameworkSettings` + `ExtraStyles` + `AlgorithmicColorScheme`
  - `DisplayFontFamily`/`BodyFontFamily` + `FontPreloads`
  - `LlmsTxtOptions` + one `llms: false` page
- **Backs how-tos (18 total):**
  - 2.1.10 front-matter
  - 2.1.20 drafts-tags-ordering
  - 2.1.30 customize-sidebar
  - 2.1.40 images-and-assets
  - 2.1.50 tabbed-code
  - 2.1.60 code-annotations
  - 2.1.70 alerts
  - 2.1.80 diagrams
  - 2.1.90 ui-components-in-markdown
  - 2.1.100 cross-references
  - 2.1.110 linking
  - 2.1.120 redirects
  - 2.2.10 multiple-sources
  - 2.2.20 search
  - 2.2.30 monorail-css
  - 2.2.40 fonts
  - 2.2.50 localization
  - 2.2.60 llms-txt
  - 2.2.80 sitemap

### 14. `BlogKitchenSinkExample`

- **Status:** not_started
- **Backs:** blog-specific §2.2 Configuration how-tos
- **Shape:** `AddBlogSite` host with fully-populated homepage surfaces (more varied than tutorial app 9: multiple projects, multiple socials, multiple header links), three dated posts, RSS + sitemap enabled.
- **Structure:**
  - `Program.cs` with rich `BlogSiteOptions`
  - `Content/Blog/<date>/<slug>.md` × 3
  - All four icon kinds populated in `SocialLink[]`
- **Backs how-tos (2):**
  - 2.2.70 rss
  - 2.2.90 blogsite-homepage
- **Also serves:** Reference §3.8 `/reference/blogsite/social-icons` and §3.1 `/reference/options/blogsite-options`

### 15. `ExtensibilityLabExample`

- **Status:** not_started
- **Backs:** all of how-to §2.3 Extensibility
- **Shape:** `AddPennington` host (not `AddDocSite`) so extension points are visible raw. Each extension lives in its own `.cs` file named for the how-to it backs — xmldocid targets are predictable.
- **Structure:**
  - `Program.cs`
  - `ReleaseNotesContentService.cs` — `IContentService` reading `Content/releases/*.json`, emitting `DiscoveredItem`/`ContentToCopy`/`ContentTocItem` + `CrossReference`
  - `LineCountPreprocessor.cs` — `ICodeBlockPreprocessor`
  - `PipelineHighlighter.cs` — `ICodeHighlighter` for fictional DSL
  - `FeedbackWidgetProcessor.cs` — `IResponseProcessor` with `Order`, `ShouldProcess`, and body-mutation
  - `AnchorLowercaseRewriter.cs` — `IHtmlResponseRewriter` demonstrating `PreParseAsync` vs `ApplyAsync`
  - `ChartIslandRenderer.cs` — `IIslandRenderer` (or `RazorIslandRenderer<T>`), registered via `IslandsOptions.Register<T>("chart")`, consumed from a content page with `data-spa-island="chart"`
  - `SiteChromeOverrides.cs` + `ExtraHeadFragment.razor` — slot-override demo (`AdditionalHtmlHeadContent` + `ExtraStyles` + replacement component via `AdditionalRoutingAssemblies`)
- **Backs how-tos (7):**
  - 2.3.10 custom-content-service
  - 2.3.20 code-block-preprocessor
  - 2.3.30 custom-highlighter
  - 2.3.40 response-processor
  - 2.3.50 html-rewriter
  - 2.3.60 island-renderer
  - 2.3.70 override-docsite-components

### 16. `SubPathDeployableExample`

- **Status:** not_started
- **Backs:** all of how-to §2.4 Publishing & Deployment
- **Shape:** minimal `AddDocSite` host, deliberately tiny so `BaseUrlHtmlRewriter` behaviour is observable without noise. Workflow and host-config fixtures live in the repo as sibling files embedded via `path:` fences.
- **Structure:**
  - `Program.cs` (minimal)
  - `Content/index.md` + 1–2 pages
  - `.github/workflows/deploy.yml` — ready-to-copy using `setup-dotnet@v4`, `upload-pages-artifact@v3`, `deploy-pages@v4`, with `.nojekyll`
  - `staticwebapp.config.json` (Azure Static Web Apps)
  - `netlify.toml`
  - `nginx.conf` + `web.config`
  - App must produce valid static output when built with a sub-path `baseUrl`
- **Backs how-tos (5):**
  - 2.4.10 static-build
  - 2.4.20 github-pages
  - 2.4.30 adapt-for-other-hosts
  - 2.4.40 self-host
  - 2.4.50 base-url

---

## Agent checklist (per app)

When an agent is assigned an entry, it should:

1. Read this entry end-to-end and the matching tutorial/how-to page(s) in `docs/docs-toc.md`.
2. Create `examples/<AppName>/<AppName>.csproj` (inherits from `examples/Directory.Build.props`).
3. Add the project to `Pennington.slnx`.
4. Build `Program.cs` + `Content/` + any stage/feature files per the entry's Structure section.
5. Verify locally:
   - `dotnet build Pennington.slnx` passes
   - `dotnet run --project examples/<AppName>` serves pages
   - `dotnet run --project examples/<AppName> -- build _testoutput` produces static output with no build-report errors
6. Add a verified xmldocid symbol table to `docs/_research/examples-inventory.md`.
7. Update `docs/docs-manifest.yaml` evidence entries for each backing doc page.
8. Run `docs/tools/docs-lint.ps1`.
9. Update this file: flip the entry's **Status** to `complete`.

## Verification (whole set)

- `dotnet build Pennington.slnx` — all 16 new projects build
- `dotnet test Pennington.slnx` — existing tests still pass
- `dotnet run --project docs/Pennington.Docs` — docs site serves with no "symbol not found" diagnostics in the build report
- `docs/tools/docs-lint.ps1` — passes after manifest + inventory updates
