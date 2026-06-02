# examples/ — Example site conventions

## Naming
All example projects suffix `Example`. Categories:
- `GettingStarted*` — baseline Pennington setup, used by tutorials
- `Beyond*` — advanced features (tree-sitter fragments, locales, custom Razor components)
- `DocSite*` / `BlogSite*` — template-specific (scaffold, kitchen sink, sections, authors)
- `Focused*` / `Multiple*` — feature-focused (code samples, multi-source)

## Shape
- Every example has `Program.cs` + `.csproj` + `Content/`.
- Some examples add `Components/` for Mdazor-referenced Razor components.
- Template/library refs are relative: `..\..\src\Pennington\...`.

## Shared content (`_shared/`)
`examples/_shared/` holds fixture content that is not itself a project. `_shared/Bramble`
is a fixed ~100-document fictional corpus (docs + blog). Examples that need volume
rather than bespoke pages mount it via a relative `ContentRootPath` /
`ContentPath` (`../_shared/Bramble/Content[/subfolder]`) instead of bundling their
own markdown — see `DocSiteSharedCorpusExample` and `_shared/Bramble/README.md`.
Such an example legitimately has no local `Content/`.

## Per-example README
Every example folder has a `README.md` describing its purpose, the concepts it teaches, and where it is referenced from the docs site. Keep that README current when an example's teaching surface changes — it is the per-folder analogue of the table below. New examples must ship with one.

## Index of examples

| Example | Purpose | Docs pages |
|---|---|---|
| `BareHostRazorPageExample` | Render a Razor component as a full response on a bare `AddPennington` host via `HtmlRenderer` + `MapGet`. | `how-to/response-pipeline/razor-page-on-bare-host.md` |
| `BareHostSearchExample` | Light up the Pennington.UI search modal on a bare (non-DocSite) host — reference `Pennington.UI`, load `dewey-search.js` + `scripts.js`, add an `id="search-input"` trigger; styled by the `PenningtonApplies` safelist. Mounts the `_shared/Bramble` corpus (blog excluded). | `how-to/discovery/search-on-a-bare-host.md` |
| `BeyondClientWidgetExample` | Ship a custom client-side widget (an image-gallery lightbox): a server-rendered Mdazor `<ImageGallery>` component, a `wwwroot/gallery.js` enhancer, and a CDN library loaded via `DocSiteOptions.AdditionalHtmlHeadContent`. Gallery images are linked from `_shared/Images` by a copy target (gitignored). | `how-to/rich-content/client-side-widget.md` |
| `BeyondCustomRazorComponentExample` | Author a Razor component (`PricingCard`) and register it with Mdazor via `AddMdazorComponent<T>()`. | `tutorials/beyond-basics/custom-razor-component.md` |
| `BeyondRemoteContentExample` | Source content entirely from a remote API (GitHub Releases) via a typed `HttpClient` + `IContentService`: one `AsyncLazy`-cached fetch shared across discovery/TOC/xref/endpoint, the markdown `body` rendered through `IContentRenderer` and wrapped in `<article>` so the self-fetch heading-indexes it, an `EndpointSource` per release (listed in the sitemap, since it serves real HTML), and a committed fixture fallback for offline/rate-limited builds. No local `Content/`. | `how-to/content-services/source-from-a-remote-api.md` |
| `BeyondLocaleExample` | Add a second URL-prefixed locale to a DocSite via `ConfigureLocalization` + `Content/<locale>/`. | `tutorials/beyond-basics/add-a-locale.md`, `how-to/discovery/localization.md` || `BeyondTreeSitterExample` | `AddTreeSitter` against a `Samples/` folder — `<lang>:symbol` fences extract declarations by name path (`Type.Member`) across Python/Rust/Go/TypeScript, plus `,bodyonly` and whole-file forms. | _(reference for `Pennington.TreeSitter`)_ |
| `BeyondTranslationAuditExample` | Wire `AddTranslationAudit` so missing translations surface in the dev overlay and build report. | `how-to/discovery/audit-translations.md` |
| `BlogKitchenSinkExample` | Wide BlogSite configuration (hero, projects, socials, RSS, sitemap, JSON-LD) split across helpers for symbol fencing. | `how-to/feeds/rss.md`, `how-to/feeds/sitemap.md` |
| `BlogSiteFirstPostExample` | Extend the BlogSite scaffold with a fully populated post exercising every `BlogSiteFrontMatter` field. | `tutorials/blogsite/first-post.md`, `reference/front-matter/keys.md` |
| `BlogSiteHeroProjectsSocialsExample` | Populate `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks` with the built-in `SocialIcons` fragments. | `tutorials/blogsite/hero-projects-socials.md`, `how-to/feeds/blogsite-homepage.md` |
| `BlogSiteScaffoldExample` | Smallest BlogSite — `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync` with one post under `Content/Blog/`. | `tutorials/blogsite/scaffold.md`, `reference/blogsite/routes.md` |
| `DocSiteBlogExample` | DocSite with a `Content/blog/` folder — the folder convention activates the blog index, post pages, `/blog/tags/` pages, the "Blog" header link, and `/rss.xml` with no `Program.cs` wiring. | `tutorials/docsite/add-a-blog.md` |
| `DocSitePagesAndLinksExample` | Single-area DocSite with two content pages (`install`, `configure`) and a hub `index` demonstrating relative, absolute, and `uid:`-based linking, plus `Components/Index.razor` — a Razor landing page routed at `/` with `FullWidthLayout`. `snippets/markdown-{alert,tabs}-example.md` back the alerts/tabs sections of the markdown reference. | `tutorials/docsite/first-doc-page.md`, `tutorials/docsite/landing-page.md`, `reference/markdown/extensions.md` |
| `DocSiteChromeOverridesExample` | Override DocSite chrome via `DocSiteOptions` + head-slot fragment + custom routed `@page` + `AdditionalRoutingAssemblies`. | `how-to/response-pipeline/override-docsite-components.md` |
| `DocSiteKitchenSinkExample` | Wide DocSite configuration (areas, locales, theming, fonts, custom front matter, custom Mdazor component) split across helpers for symbol fencing. | `how-to/navigation/{customize-sidebar,cross-references}.md`, `how-to/theming/{monorail-css,fonts}.md`, `how-to/pages/{front-matter,redirects}.md`, `how-to/discovery/{search,multiple-sources}.md`, `how-to/feeds/llms-txt.md`, `how-to/rich-content/ui-components-in-markdown.md`, `reference/front-matter/keys.md` |
| `DocSiteScaffoldExample` | Smallest DocSite — `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` with a `Content/guides/` folder that auto-groups in the sidebar (no areas). | `tutorials/docsite/scaffold.md`, `reference/host/extensions.md` |
| `DocSiteSectionsExample` | Structure `Content/` into areas and subfolder-backed sections; `order:` / `section:` drive sidebar grouping. | `tutorials/docsite/sections-and-areas.md` |
| `DocSiteSharedCorpusExample` | DocSite with no `Content/` of its own — mounts the shared `_shared/Bramble` corpus via a relative `ContentRootPath`, four Diátaxis areas + auto-activated blog. A site-at-scale fixture host. | _(fixture/scale host)_ |
| `ExtensibilityLabExample` | Bare-host lab exercising every Pennington extension seam in one project: custom highlighter, code-block preprocessor, custom/emit-only `IContentService`, response processor, diagnostics processor, HTML rewriter, MonorailCSS customization, llms.txt opt-in, tabbed-code class override. | `how-to/markdown-pipeline/{custom-highlighter,code-block-preprocessor}.md`, `how-to/content-services/{custom-content-service,emit-generated-artifacts}.md`, `how-to/response-pipeline/{response-processor,html-rewriter}.md`, `how-to/theming/monorail-css.md`, `how-to/feeds/llms-txt.md`, `how-to/code-samples/tabbed-code.md`, `explanation/positioning/docsite-positioning.md`, `reference/diagnostics/request-context.md` |
| `FocusedCodeSamplesExample` | Console app — two implementations of a word-counter (`Monolith`/`Modular`) for narrating a refactor via symbol fences. | `how-to/code-samples/focused-code-samples.md` |
| `FusionCacheDocSiteExample` | Real-target DocSite — API reference generated from `ZiggyCreatures.FusionCache` via `AddApiMetadataFromCompiledAssembly` + `AddApiReference`. | _(reference for `Pennington.ApiMetadata.Reflection`)_ |
| `GettingStartedBlazorPagesExample` | Replace the bare `MapGet` host with a Blazor Server `@page` catch-all (`MarkdownPage.razor`) that renders markdown through the same content pipeline. | `tutorials/getting-started/first-page.md` |
| `GettingStartedMinimalSiteExample` | Smallest viable Pennington host — `AddPennington` + `AddMarkdownContent` + a catch-all `MapGet`. | `tutorials/getting-started/first-site.md` |
| `GettingStartedNavigationExample` | Add a navigation menu to the styled bare host — a `NavMenu.razor` component renders the tree `NavigationBuilder` builds from each `IContentService`'s TOC entries. | `tutorials/getting-started/navigation.md` |
| `GettingStartedStylingExample` | Layer MonorailCSS onto the BlazorPages site via a styled `MainLayout.razor` and `NamedColorScheme`. | `tutorials/getting-started/styling.md` |
| `MultipleSourcesExample` | Bare host with two `AddMarkdownContent<T>` calls — separate front-matter types, separate content roots, overlap variant toggled by env var. | `how-to/discovery/multiple-sources.md` |
| `ScaleStressExample` | Bare host that pre-generates 5000 markdown files (Markov-chained off `_shared/Bramble`, gitignored, regenerated on first launch) and serves them through a single `MarkdownContentService` with an index list + naive catch-all render. | _(scale fixture host)_ |
| `SpaPlaygroundExample` | Minimal site for observing `spa-engine.js` — two `data-spa-region` blocks, one persistent `<nav>`, on-page event log for the three lifecycle events. | _(SPA-engine sandbox)_ |
| `SpectreConsoleDocSiteExample` | Real-target DocSite — two named metadata providers + two `AddApiReference` registrations document `Spectre.Console` and `Spectre.Console.Cli` as separate trees. | _(multi-source reference for `Pennington.ApiMetadata.Reflection`)_ |
| `SubPathDeployableExample` | Tiny DocSite host whose teaching surface is the sibling deploy fixtures: `.github/workflows/deploy.yml`, `staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`. | `how-to/deployment/{static-build,self-host,adapt-for-other-hosts,base-url,github-pages}.md` |
| `VersionedDocSiteExample` | Two-area DocSite documenting two versions of `Humanizer.Core` (2.8.26 + 2.14.1) as parallel `/v1/` and `/v2/` trees, each with its own content area and its own API reference. The off-version is staged via `<PackageDownload>` to work around NuGet's one-version-per-assembly rule. | `how-to/versioning/docsite.md` |

## Staged tutorial files
Examples used by step-by-step tutorials split the teaching into per-stage artifacts:
- **C# stages** live at the project root as `StageN_<Label>.cs` (e.g., `Stage1_BareHost.cs`, `Stage2_AddPennington.cs`). Docs pull them via `csharp:symbol,bodyonly` fences addressing `<file> > StageN.Member`.
- **Markdown (or other non-C#) stages** live under `snippets/` as `stageN.md` (e.g., `examples/DocSiteSectionsExample/snippets/stageN.md`). Docs pull them via `<lang>:symbol` fences with a bare file path (whole-file embed).

Keep non-C# stages as their own files rather than C# raw-string literals. Don't collapse stages into a single file.

## Docs coupling — renames break docs
Examples are referenced from `docs/Pennington.Docs/Content/` via tree-sitter `:symbol` fences:
- **Whole-file embed** — a bare `<file>` path (`csharp:symbol` for `Program.cs` / `StageN_*.cs`, `markdown:symbol` for `snippets/stageN.md`, `razor:symbol` for `.razor`, etc.).
- **Member embed** — `<file> > <Member.Path>` (`Type`, `Type.Method`, `Type.Property`); add `,bodyonly` to drop the declaration.

Before renaming a file, moving a symbol, or restructuring an example, grep `docs/Pennington.Docs/Content/` for the old name/path. The docs build will fail on a broken reference — but you want to know before the build stage.
