# examples/ — Example site conventions

## Naming
All example projects suffix `Example`. Categories:
- `GettingStarted*` — baseline Pennington setup, used by tutorials
- `Beyond*` — advanced features (Roslyn, locales, custom Razor components)
- `DocSite*` / `BlogSite*` — template-specific (scaffold, kitchen sink, sections, authors)
- `Focused*` / `Multiple*` — feature-focused (code samples, multi-source)

## Shape
- Every example has `Program.cs` + `.csproj` + `Content/`.
- Some examples add `Components/` for Mdazor-referenced Razor components.
- Template/library refs are relative: `..\..\src\Pennington\...`.

## Per-example README
Every example folder has a `README.md` describing its purpose, the concepts it teaches, and where it is referenced from the docs site. Keep that README current when an example's teaching surface changes — it is the per-folder analogue of the table below. New examples must ship with one.

## Index of examples

| Example | Purpose | Docs pages |
|---|---|---|
| `BareHostRazorPageExample` | Render a Razor component as a full response on a bare `AddPennington` host via `HtmlRenderer` + `MapGet`. | `how-to/response-pipeline/razor-page-on-bare-host.md` |
| `BeyondCustomRazorComponentExample` | Author a Razor component (`PricingCard`) and register it with Mdazor via `AddMdazorComponent<T>()`. | `tutorials/beyond-basics/custom-razor-component.md` |
| `BeyondLocaleExample` | Add a second URL-prefixed locale to a DocSite via `ConfigureLocalization` + `Content/<locale>/`. | `tutorials/beyond-basics/add-a-locale.md`, `how-to/discovery/localization.md` |
| `BeyondRoslynExample` | `AddPenningtonRoslyn` against a sibling slnx — markdown fences resolve `:xmldocid` / `:xmldocid,bodyonly` / `:xmldocid-diff` / `:path`. | `tutorials/beyond-basics/connect-roslyn.md` |
| `BeyondTranslationAuditExample` | Wire `AddPenningtonTranslationAudit` so missing translations surface in the dev overlay and build report. | _(reference for `Pennington.TranslationAudit`)_ |
| `BeyondTuiExample` | Opt the host into the dev-time TUI dashboard via `AddPenningtonTui`. Build mode no-ops. | _(reference for `Pennington.Tui`)_ |
| `BlogKitchenSinkExample` | Wide BlogSite configuration (hero, projects, socials, RSS, sitemap, JSON-LD) split across helpers for xmldocid fencing. | `how-to/feeds/rss.md`, `how-to/feeds/sitemap.md` |
| `BlogSiteFirstPostExample` | Extend the BlogSite scaffold with a fully populated post exercising every `BlogSiteFrontMatter` field. | `tutorials/blogsite/first-post.md`, `reference/front-matter/keys.md` |
| `BlogSiteHeroProjectsSocialsExample` | Populate `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks` with the built-in `SocialIcons` fragments. | `tutorials/blogsite/hero-projects-socials.md`, `how-to/feeds/blogsite-homepage.md` |
| `BlogSiteScaffoldExample` | Smallest BlogSite — `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync` with one post under `Content/Blog/`. | `tutorials/blogsite/scaffold.md`, `reference/blogsite/routes.md` |
| `DocSiteAuthorExample` | Single-area DocSite focused on *authoring* a doc page (front matter, alerts, tabbed code, outline). Markdown stages under `snippets/`. | `tutorials/docsite/first-doc-page.md`, `reference/markdown/extensions.md` |
| `DocSiteChromeOverridesExample` | Override DocSite chrome via `DocSiteOptions` + head-slot fragment + custom routed `@page` + `AdditionalRoutingAssemblies`. | `how-to/response-pipeline/override-docsite-components.md` |
| `DocSiteKitchenSinkExample` | Wide DocSite configuration (areas, locales, theming, fonts, custom front matter, custom Mdazor component) split across helpers for xmldocid fencing. | `how-to/navigation/{customize-sidebar,cross-references}.md`, `how-to/theming/{monorail-css,fonts}.md`, `how-to/pages/{front-matter,redirects}.md`, `how-to/discovery/{search,multiple-sources}.md`, `how-to/feeds/llms-txt.md`, `how-to/rich-content/ui-components-in-markdown.md`, `reference/front-matter/keys.md` |
| `DocSiteScaffoldExample` | Smallest DocSite — `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` with two areas. | `tutorials/docsite/scaffold.md`, `reference/host/extensions.md` |
| `DocSiteSectionsExample` | Structure `Content/` into areas and subfolder-backed sections; `order:` / `section:` drive sidebar grouping. | `tutorials/docsite/sections-and-areas.md` |
| `ExtensibilityLabExample` | Bare-host lab exercising every Pennington extension seam in one project: custom highlighter, code-block preprocessor, custom/emit-only `IContentService`, response processor, diagnostics processor, HTML rewriter, MonorailCSS customization, llms.txt opt-in, tabbed-code class override. | `how-to/markdown-pipeline/{custom-highlighter,code-block-preprocessor}.md`, `how-to/content-services/{custom-content-service,emit-generated-artifacts}.md`, `how-to/response-pipeline/{response-processor,html-rewriter}.md`, `how-to/theming/monorail-css.md`, `how-to/feeds/llms-txt.md`, `how-to/code-samples/tabbed-code.md`, `explanation/positioning/docsite-positioning.md`, `reference/diagnostics/request-context.md` |
| `FocusedCodeSamplesExample` | Console app — two implementations of a word-counter (`Monolith`/`Modular`) for narrating a refactor via xmldocid fences. | `how-to/code-samples/focused-code-samples.md` |
| `FusionCacheDocSiteExample` | Real-target DocSite — API reference generated from `ZiggyCreatures.FusionCache` via `AddApiMetadataFromCompiledAssembly` + `AddApiReference`. | _(reference for `Pennington.ApiMetadata.Reflection`)_ |
| `GettingStartedFirstPageExample` | Add more `.md` pages and a `NavigationBuilder`-driven nav strip on top of the minimal host. | `tutorials/getting-started/first-page.md` |
| `GettingStartedMinimalSiteExample` | Smallest viable Pennington host — `AddPennington` + `AddMarkdownContent` + a catch-all `MapGet`. | `tutorials/getting-started/first-site.md` |
| `GettingStartedStylingExample` | Add MonorailCSS to the minimal host with a `NamedColorScheme`. | `tutorials/getting-started/styling.md` |
| `MultipleSourcesExample` | Bare host with two `AddMarkdownContent<T>` calls — separate front-matter types, separate content roots, overlap variant toggled by env var. | `how-to/discovery/multiple-sources.md` |
| `SpaPlaygroundExample` | Minimal site for observing `spa-engine.js` — two `data-spa-region` blocks, one persistent `<nav>`, on-page event log for the three lifecycle events. | _(SPA-engine sandbox)_ |
| `SpectreConsoleDocSiteExample` | Real-target DocSite — two named metadata providers + two `AddApiReference` registrations document `Spectre.Console` and `Spectre.Console.Cli` as separate trees. | _(multi-source reference for `Pennington.ApiMetadata.Reflection`)_ |
| `SubPathDeployableExample` | Tiny DocSite host whose teaching surface is the sibling deploy fixtures: `.github/workflows/deploy.yml`, `staticwebapp.config.json`, `netlify.toml`, `nginx.conf`, `web.config`. | `how-to/deployment/{static-build,self-host,adapt-for-other-hosts,base-url,github-pages}.md` |

## Staged tutorial files
Examples used by step-by-step tutorials split the teaching into per-stage artifacts:
- **C# stages** live at the project root as `StageN_<Label>.cs` (e.g., `Stage1_BareHost.cs`, `Stage2_AddPennington.cs`). Docs pull them via `csharp:xmldocid[,bodyonly]` fences.
- **Markdown (or other non-C#) stages** live under `snippets/` as `stageN.md` (e.g., `examples/DocSiteAuthorExample/snippets/stage1.md`). Docs pull them via `markdown:path` fences — `:xmldocid` is C#/VB-only.

Don't store markdown-as-a-C#-raw-string just to reach it by xmldocid; the `"""` delimiters leak into the rendered code block. Don't collapse stages into a single file.

## Roslyn examples
`BeyondRoslynExample` (and any future Roslyn-using example) sets `<DefaultItemExcludes>` in the csproj so the sibling `Sample/` library isn't swept into its own compile. Preserve that when editing the csproj.

## Docs coupling — renames break docs
Examples are referenced from `docs/Pennington.Docs/Content/` via:
- `<lang>:path` fences (file paths) — `csharp:path` for `Program.cs` / `StageN_*.cs`, `markdown:path` for `snippets/stageN.md`, `razor:path` for `.razor`, etc.
- `csharp:xmldocid` / `csharp:xmldocid,bodyonly` (XmlDocIds — `T:Ns.Type`, `M:Ns.Type.Method`, `P:Ns.Type.Prop`). **C#/VB only.**

Before renaming a file, moving a symbol, or restructuring an example, grep `docs/Pennington.Docs/Content/` for the old name/ID. The docs build will fail on a broken reference — but you want to know before the build stage.
