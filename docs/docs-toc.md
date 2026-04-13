# Pennington Documentation — Proposed Table of Contents

Organized by the Diataxis framework. Two levels deep within each quadrant (quadrant → section → page). Each page entry has a **title**, **URL**, **order** (sequential 10/20/30… within its section), a one-to-two-sentence description of what it covers, and one sentence describing what it deliberately does *not* cover (to push the reader to the right neighboring page).

Synthesis sources: Astro docs, VitePress docs, GitBook docs, the Diataxis framework, MyLittleContentEngine (the Pennington precursor), and a feature audit of the current `B:\Penn` codebase.

---

## 1. Tutorials

> Learning-oriented. The author picks the example; success is defined by what the reader *learned*. Linear, numbered, hand-holding. Each tutorial is a single arc of 30–60 minutes.

### 1.1 Getting Started with Pennington

| Title | URL | Order |
|---|---|---|
| **Create your first Pennington site** | `/tutorials/getting-started/first-site` | 10 |
Covers: bootstrapping a minimal ASP.NET host with `AddPennington` + `UsePennington`, pointing `ContentRootPath` at a folder of markdown, running in dev mode with hot reload, and verifying a page renders with front matter.
Does not cover: the DocSite template, styling, or deployment — those arrive in later tutorials.

| Title | URL | Order |
|---|---|---|
| **Add your first markdown page** | `/tutorials/getting-started/first-page` | 20 |
Covers: writing a YAML front-matter block, the required `title` key, how the file path becomes a URL, and seeing navigation auto-assemble as you add a second and third file.
Does not cover: custom front-matter types, capability interfaces, or non-markdown content sources.

| Title | URL | Order |
|---|---|---|
| **Style the site with MonorailCSS** | `/tutorials/getting-started/styling` | 30 |
Covers: registering `AddMonorailCss` + `UseMonorailCss`, picking a `NamedColorScheme`, adding a utility class to a layout, and watching the stylesheet regenerate on demand.
Does not cover: algorithmic color schemes, custom `CssFrameworkSettings`, or dark-mode wiring — see the how-to on theme customization.

| Title | URL | Order |
|---|---|---|
| **Ship it: build and deploy to GitHub Pages** | `/tutorials/getting-started/deploy-github-pages` | 40 |
Covers: running the static build via `RunOrBuildAsync(args)` with `build [baseUrl] [output]`, inspecting the `BuildReport`, writing a GitHub Actions workflow, and pushing `.nojekyll`-safe output.
Does not cover: subdirectory hosting gotchas, other hosts (Netlify, Azure, Docker), or custom base-URL rewriting — each is its own how-to.

### 1.2 Getting Started with DocSite

| Title | URL | Order |
|---|---|---|
| **Scaffold a documentation site with DocSite** | `/tutorials/docsite/scaffold` | 10 |
Covers: replacing the barebones setup with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, configuring `DocSiteOptions` (site title, GitHub URL, header/footer), and understanding how areas map to top-level folders.
Does not cover: authoring markdown content (covered next) or overriding the DocSite layout — treated as a customization how-to.

| Title | URL | Order |
|---|---|---|
| **Author a documentation page with DocFrontMatter** | `/tutorials/docsite/first-doc-page` | 20 |
Covers: writing a page with `DocSiteFrontMatter` (title, description, tags, section, order), adding alerts and a tabbed code group, and seeing the outline navigation populate.
Does not cover: cross-references, snippets, or diagram blocks — those are per-feature how-tos.

| Title | URL | Order |
|---|---|---|
| **Organize content with sections and areas** | `/tutorials/docsite/sections-and-areas` | 30 |
Covers: structuring `Content/` into areas and sections, using `section`/`order` in front matter, and how `NavigationBuilder` turns a flat file tree into a sidebar.
Does not cover: locale-prefixed navigation, Razor-page integration, or custom `IContentService` implementations.

### 1.3 Getting Started with BlogSite

| Title | URL | Order |
|---|---|---|
| **Scaffold a blog with BlogSite** | `/tutorials/blogsite/scaffold` | 10 |
Covers: replacing `AddPennington` with `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, configuring the core `BlogSiteOptions` (site title, author, canonical base URL, content paths), and understanding the difference between `DocSite` and `BlogSite` defaults.
Does not cover: authoring individual posts (next page) or customizing the homepage hero (see the final page in this section).

| Title | URL | Order |
|---|---|---|
| **Author your first post with BlogSiteFrontMatter** | `/tutorials/blogsite/first-post` | 20 |
Covers: writing a post with `BlogSiteFrontMatter` (title, description, date, author, tags, series, repository, section, redirectUrl), seeing the post appear on the blog index, and enabling the built-in RSS feed. `AddBlogSite` wires posts through `AddMarkdownContent<BlogSiteFrontMatter>` — not the core `BlogFrontMatter` — so this is the record the tutorial must teach.
Does not cover: customizing the post template or adding a tag-index page.

| Title | URL | Order |
|---|---|---|
| **Add a hero, projects, and social links** | `/tutorials/blogsite/hero-projects-socials` | 30 |
Covers: populating `HeroContent`, a list of `Project` entries for a "my work" section, `SocialLink` entries (with the built-in `BlueskyIcon`/`GithubIcon`), and `HeaderLink` entries for top-nav items.
Does not cover: custom icon components or deep homepage layout overrides — covered in the Extensibility how-tos.

### 1.4 Beyond the Basics

| Title | URL | Order |
|---|---|---|
| **Add a second locale to your site** | `/tutorials/beyond-basics/add-a-locale` | 10 |
Covers: enabling `LocalizationOptions`, creating a locale subdirectory with translated markdown, wiring `UsePenningtonLocaleRouting`, and adding the `LanguageSwitcher` component.
Does not cover: per-locale search index internals or UI string translation plumbing — those are reference pages.

| Title | URL | Order |
|---|---|---|
| **Connect to a Roslyn solution for live API snippets** | `/tutorials/beyond-basics/connect-roslyn` | 20 |
Covers: pointing Pennington at a `.sln` via `SolutionPath`, using `xmldocid` code fences (e.g., ` ```csharp xmldocid="M:Ns.Type.Method"`) to pull method/class snippets straight from source, and letting hot reload update the docs when the source changes.
Does not cover: generating full API-reference pages — that requires the planned `Pennington.Roslyn` package.

| Title | URL | Order |
|---|---|---|
| **Author a custom Razor component for your content** | `/tutorials/beyond-basics/custom-razor-component` | 30 |
Covers: creating a Razor component for Pennington's Mdazor-based markdown flow, keeping its parameters simple, and linking to the Mdazor project for the underlying tag syntax and parser behavior.
Does not cover: writing an `IIslandRenderer` for SPA-interactive regions, Mdazor internals, or authoring brand-new Markdig extensions.

---

## 2. How-To Guides

> Task-oriented. User arrives with a goal already in mind. Titled "How to …". Short, scannable, no pedagogical detours.

### 2.1 Content Authoring

| Title | URL | Order |
|---|---|---|
| **Work with front matter** | `/how-to/content-authoring/front-matter` | 10 |
Covers: declaring front matter in YAML, the baseline `IFrontMatter` keys (`title`, `description`, `tags`, `section`, `order`, `isDraft`, `uid`, `date`, `search`, `llms`), and defining your own front-matter record.
Does not cover: the full key reference (see Reference) or the capability-interface architecture (see Explanation).

| Title | URL | Order |
|---|---|---|
| **Manage drafts, tags, and ordering** | `/how-to/content-authoring/drafts-tags-ordering` | 20 |
Covers: hiding pages with `isDraft: true`, using `tags` for grouping, and using `order` to control sidebar position within a section.
Does not cover: tag-index pages or custom taxonomy generation — those require a custom content service.

| Title | URL | Order |
|---|---|---|
| **Create tabbed code groups** | `/how-to/content-authoring/tabbed-code` | 30 |
Covers: marking a fenced block with `tabs=true title="…"`, grouping adjacent blocks into a single tabbed widget, and customizing the rendered CSS classes via `TabbedCodeBlockRenderOptions`.
Does not cover: the UI-component `<Tabs>` equivalent from Pennington.UI or per-tab analytics.

| Title | URL | Order |
|---|---|---|
| **Annotate code blocks** | `/how-to/content-authoring/code-annotations` | 40 |
Covers: line-highlight ranges (`{1,3}`), diff add/remove (`{+1}`/`{-1}`), focus (`{focus 1-3}`), and error/warning markers on fenced blocks.
Does not cover: writing a custom `ICodeBlockPreprocessor` — see the extensibility how-to.

| Title | URL | Order |
|---|---|---|
| **Add alerts and callouts** | `/how-to/content-authoring/alerts` | 50 |
Covers: GitHub-style alert syntax (`> [!NOTE]`, `[!TIP]`, `[!CAUTION]`, `[!WARNING]`, `[!IMPORTANT]`) and how they render.
Does not cover: custom alert styles, Mermaid diagrams, or the `<Card>` component — those live on separate pages.

| Title | URL | Order |
|---|---|---|
| **Embed diagrams** | `/how-to/content-authoring/diagrams` | 60 |
Covers: authoring Mermaid blocks with `mermaid` fences and how the diagram renders client-side with theme awareness.
Does not cover: server-side diagram rendering, non-Mermaid diagram systems, or embedding raw SVG.

| Title | URL | Order |
|---|---|---|
| **Use UI components inside markdown** | `/how-to/content-authoring/ui-components-in-markdown` | 70 |
Covers: using Pennington.UI components inside markdown through Pennington's Mdazor-based component support, and linking to the Mdazor project for the deeper syntax, nesting rules, and limitations.
Does not cover: authoring your own Razor component or documenting Mdazor internals in full.

| Title | URL | Order |
|---|---|---|
| **Cross-reference pages by `uid`** | `/how-to/content-authoring/cross-references` | 80 |
Covers: setting `uid:` in front matter, linking via `<xref uid="…">text</xref>` or `[text](xref:uid)`, and letting `XrefHtmlRewriter` resolve links at request/build time.
Does not cover: cross-referencing Roslyn symbols by xmldocid — that is a planned separate package (Pennington.Roslyn).

| Title | URL | Order |
|---|---|---|
| **Link between pages and assets** | `/how-to/content-authoring/linking` | 90 |
Covers: relative links, anchor fragments, external links, and how `BaseUrlHtmlRewriter` handles sub-path deployments.
Does not cover: cross-references by uid (see previous) or programmatic URL construction (see Reference → Routing).

| Title | URL | Order |
|---|---|---|
| **Configure redirects** | `/how-to/content-authoring/redirects` | 100 |
Covers: setting `redirectUrl:` in `DocSiteFrontMatter` to emit a meta-refresh stub page with `noindex`.
Does not cover: HTTP 301 responses at the hosting layer or batch redirects from a sidecar file.

### 2.2 Configuration

| Title | URL | Order |
|---|---|---|
| **Configure `PenningtonOptions`** | `/how-to/configuration/pennington-options` | 10 |
Covers: setting `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, and the nested option groups (`Highlighting`, `Islands`, `Localization`, `SearchIndex`, `LlmsTxt`).
Does not cover: each option's full schema (see Reference) or DocSite-specific options.

| Title | URL | Order |
|---|---|---|
| **Configure DocSite** | `/how-to/configuration/docsite-options` | 20 |
Covers: filling out `DocSiteOptions` (fonts, colors, header/footer content, GitHub URL, social image, solution path, areas).
Does not cover: runtime Razor overrides of DocSite layouts — see the customization how-to.

| Title | URL | Order |
|---|---|---|
| **Use multiple content sources** | `/how-to/configuration/multiple-sources` | 30 |
Covers: chaining `WithMarkdownContentService<T>` calls with different `ContentPath`/`BasePageUrl`/`Section`/`ExcludePaths`, and how overlap detection warns on misconfiguration.
Does not cover: implementing a non-markdown `IContentService` — see the extensibility section.

| Title | URL | Order |
|---|---|---|
| **Configure search indexing** | `/how-to/configuration/search` | 40 |
Covers: tuning `SearchIndexOptions.ContentSelector`, setting `DefaultPriority`, opting pages out via `search: false` or Razor-page sidecar metadata, and per-locale output files.
Does not cover: replacing the FlexSearch client or building a server-side search backend.

| Title | URL | Order |
|---|---|---|
| **Customize MonorailCSS** | `/how-to/configuration/monorail-css` | 50 |
Covers: swapping between `NamedColorScheme` and `AlgorithmicColorScheme`, injecting `CustomCssFrameworkSettings`, adding `ExtraStyles`, and configuring `ContentPaths` for class collection.
Does not cover: the `CssClassCollectorProcessor` internals (Explanation) or writing a standalone color scheme (advanced customization).

| Title | URL | Order |
|---|---|---|
| **Configure fonts and typography** | `/how-to/configuration/fonts` | 60 |
Covers: setting `DisplayFontFamily`/`BodyFontFamily` on `DocSiteOptions`, declaring `FontPreloads`, and serving font assets.
Does not cover: self-hosting vs. Google Fonts trade-offs (out of scope).

| Title | URL | Order |
|---|---|---|
| **Enable multiple locales** | `/how-to/configuration/localization` | 70 |
Covers: populating `LocalizationOptions` with `DefaultLocale` and `Locales`, organizing content in locale subdirectories, adding UI translations, and wiring `UsePenningtonLocaleRouting`.
Does not cover: implementing a custom culture provider — the built-in `PenningtonUrlRequestCultureProvider` is explained in Reference.

| Title | URL | Order |
|---|---|---|
| **Generate an llms.txt** | `/how-to/configuration/llms-txt` | 80 |
Covers: enabling `LlmsTxtOptions`, setting `OutputDirectory` and `GenerateFullFile`, and opting pages out with `llms: false`.
Does not cover: the LLM-training implications of your output or MCP server generation.

| Title | URL | Order |
|---|---|---|
| **Configure the BlogSite package** | `/how-to/configuration/blogsite` | 90 |
Covers: a tour of every `BlogSiteOptions` knob — site metadata, content paths (`BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), author bio, color scheme, fonts, and feature toggles (`EnableRss`, `EnableSitemap`).
Does not cover: the hero/projects/socials data shapes (see the next three pages) or the low-level markdown pipeline (see Core Pennington How-Tos).

| Title | URL | Order |
|---|---|---|
| **Customize the BlogSite hero** | `/how-to/configuration/blogsite-hero` | 100 |
Covers: filling out `HeroContent` (headline, intro paragraph, CTA) and how the homepage layout renders it above the recent-posts list.
Does not cover: replacing the hero component entirely — see Extensibility.

| Title | URL | Order |
|---|---|---|
| **Showcase projects with "my work"** | `/how-to/configuration/blogsite-projects` | 110 |
Covers: populating `MyWork` with `Project` entries (title, description, URL, color/icon) and how the list renders on the homepage.
Does not cover: per-project landing pages — use ordinary markdown content for those.

| Title | URL | Order |
|---|---|---|
| **Add social links and header navigation** | `/how-to/configuration/blogsite-socials` | 120 |
Covers: adding `SocialLink` entries with the built-in icons (`BlueskyIcon`, `GithubIcon`, etc.), populating `MainSiteLinks` with `HeaderLink` entries, and where each surfaces in the rendered chrome.
Does not cover: custom icon components — see the Razor-component how-to in Extensibility.

### 2.3 Extensibility

| Title | URL | Order |
|---|---|---|
| **Implement a custom `IContentService`** | `/how-to/extensibility/custom-content-service` | 10 |
Covers: discovering custom sources, yielding `DiscoveredItem`s and `ContentToCopy`/`ContentToCreate`, and emitting `ContentTocItem`s and cross-references.
Does not cover: parsing/rendering — those are separate interfaces covered below.

| Title | URL | Order |
|---|---|---|
| **Add a custom code-block preprocessor** | `/how-to/extensibility/code-block-preprocessor` | 20 |
Covers: implementing `ICodeBlockPreprocessor`, setting a priority, and returning `HighlightedHtml` with `SkipTransform` when needed.
Does not cover: writing a new highlighter (next page) or customizing the rendered CSS wrapper.

| Title | URL | Order |
|---|---|---|
| **Add a custom syntax highlighter** | `/how-to/extensibility/custom-highlighter` | 30 |
Covers: implementing `ICodeHighlighter`, declaring `SupportedLanguages`, setting priority, and registering via `HighlightingOptions.AddHighlighter`.
Does not cover: authoring TextMate grammars from scratch — see the upstream TextMateSharp docs.

| Title | URL | Order |
|---|---|---|
| **Write a response processor** | `/how-to/extensibility/response-processor` | 40 |
Covers: implementing `IResponseProcessor`, deciding an `Order`, using `ShouldProcess` to gate work, and mutating the HTTP response body.
Does not cover: HTML-specific rewriting — prefer `IHtmlResponseRewriter` (next).

| Title | URL | Order |
|---|---|---|
| **Write an HTML rewriter** | `/how-to/extensibility/html-rewriter` | 50 |
Covers: implementing `IHtmlResponseRewriter`, when to use `PreParseAsync` vs `ApplyAsync`, and how rewriters share one AngleSharp pass.
Does not cover: creating brand-new Markdig extensions.

| Title | URL | Order |
|---|---|---|
| **Register an island renderer** | `/how-to/extensibility/island-renderer` | 60 |
Covers: implementing `IIslandRenderer` (or subclassing `RazorIslandRenderer`), configuring `IslandsOptions.Register<T>("islandName")`, and adding matching `data-spa-island` markup.
Does not cover: the full SPA data envelope — see Explanation.

| Title | URL | Order |
|---|---|---|
| **Customize DocSite layouts and components** | `/how-to/extensibility/override-docsite-components` | 70 |
Covers: replacing a Pennington.UI component by registering your own in DI, overriding slots, and adding content to `AdditionalHtmlHeadContent`.
Does not cover: forking DocSite wholesale — use plain `AddPennington` if you need that freedom.

### 2.4 Publishing & Deployment

| Title | URL | Order |
|---|---|---|
| **Build a static site** | `/how-to/deployment/static-build` | 10 |
Covers: running the app with `build [baseUrl] [outputDirectory]`, understanding the crawler-based `OutputGenerationService`, and reading the `BuildReport` for broken links and failed pages.
Does not cover: platform-specific upload steps (see the per-host pages).

| Title | URL | Order |
|---|---|---|
| **Deploy to GitHub Pages** | `/how-to/deployment/github-pages` | 20 |
Covers: a ready-to-copy Actions workflow with `setup-dotnet@v4`, `upload-pages-artifact@v3`, `deploy-pages@v4`, and the `.nojekyll` marker.
Does not cover: custom-domain DNS setup beyond the GitHub Pages checkbox.

| Title | URL | Order |
|---|---|---|
| **Deploy to Azure Static Web Apps** | `/how-to/deployment/azure-static-web-apps` | 30 |
Covers: configuring the SWA pipeline, `app_location`/`output_location`, and handling routes and redirects with `staticwebapp.config.json`.
Does not cover: Azure Functions API backends.

| Title | URL | Order |
|---|---|---|
| **Deploy to Netlify** | `/how-to/deployment/netlify` | 40 |
Covers: the build command, publish directory, and a minimal `netlify.toml`.
Does not cover: Netlify Functions or edge middleware.

| Title | URL | Order |
|---|---|---|
| **Deploy to Cloudflare Pages** | `/how-to/deployment/cloudflare-pages` | 50 |
Covers: the Cloudflare Pages build settings and environment variables the static build reads.
Does not cover: Workers-based dynamic augmentation.

| Title | URL | Order |
|---|---|---|
| **Self-host behind Nginx or IIS** | `/how-to/deployment/self-host` | 60 |
Covers: serving the `output/` directory as static files, setting default-extension rules, and handling 404s with the generated `404.html`.
Does not cover: running the live Pennington app as an origin (a valid but separate topic).

| Title | URL | Order |
|---|---|---|
| **Host under a sub-path (base URL)** | `/how-to/deployment/base-url` | 70 |
Covers: passing `[baseUrl]` to the build command and how `BaseUrlHtmlRewriter` rewrites anchors, assets, and scripts.
Does not cover: client-side-router base handling outside the built-in SPA island system.

| Title | URL | Order |
|---|---|---|
| **Generate RSS feeds** | `/how-to/deployment/rss` | 80 |
Covers: enabling RSS on a blog source, using `BlogFrontMatter.Date`, and where the feed is written.
Does not cover: JSON Feed, Atom format selection, or multi-source feed merging.

| Title | URL | Order |
|---|---|---|
| **Generate a sitemap** | `/how-to/deployment/sitemap` | 90 |
Covers: enabling sitemap generation, the `/sitemap.xml` route, and how drafts/redirects are filtered.
Does not cover: submitting the sitemap to search consoles.

---

## 3. Reference

> Information-oriented. Dry, factual, exhaustive, lookup-shaped. One page per coherent unit (one options class, one interface group, one keyspace). No walkthroughs.

### 3.1 Configuration Options

| Title | URL | Order |
|---|---|---|
| **`PenningtonOptions`** | `/reference/options/pennington-options` | 10 |
Covers: every property on `PenningtonOptions` — `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, `Highlighting`, `Islands`, `Localization`, `Translations`, `SearchIndex`, `LlmsTxt`, `AdditionalRoutingAssemblies` — with types and defaults.
Does not cover: task recipes that use these options (see How-Tos).

| Title | URL | Order |
|---|---|---|
| **`DocSiteOptions`** | `/reference/options/docsite-options` | 20 |
Covers: every property on `DocSiteOptions` (title, description, color scheme, fonts, header/footer, GitHub URL, social image, solution path, areas, etc.).
Does not cover: `PenningtonOptions` (the base) — see the preceding page.

| Title | URL | Order |
|---|---|---|
| **`BlogSiteOptions`** | `/reference/options/blogsite-options` | 30 |
Covers: every property on `BlogSiteOptions` — metadata (`SiteTitle`, `Description`, `CanonicalBaseUrl`), content paths (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), styling (`PrimaryHue`, `BaseColorName`, fonts, `ExtraStyles`, `AdditionalHtmlHeadContent`), author chrome (`AuthorName`, `AuthorBio`), homepage data (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`), feature toggles (`EnableRss`, `EnableSitemap`), and integration hooks (`SolutionPath`, `SocialMediaImageUrlFactory`, `AdditionalRoutingAssemblies`) — plus the helper records `HeroContent`, `Project`, `SocialLink`, and `HeaderLink`.
Does not cover: `DocSiteOptions` or `PenningtonOptions` (preceding pages).

| Title | URL | Order |
|---|---|---|
| **`MarkdownContentOptions<T>`** | `/reference/options/markdown-content-options` | 40 |
Covers: `ContentPath`, `BasePageUrl`, `Section`, `ExcludePaths`, and their interaction with multi-source setups.
Does not cover: the content-pipeline interfaces that consume the options.

| Title | URL | Order |
|---|---|---|
| **`LocalizationOptions`** | `/reference/options/localization-options` | 50 |
Covers: `DefaultLocale`, `Locales`, `IsMultiLocale`, and the URL helpers `GetLocaleFromUrl`, `StripLocalePrefix`, `BuildLocaleUrl`, `GetAlternateLanguages`.
Does not cover: authoring translated content (see How-Tos).

| Title | URL | Order |
|---|---|---|
| **`TranslationOptions` and `PenningtonStringLocalizer`** | `/reference/options/translations` | 60 |
Covers: the `TranslationOptions.Add(locale, key, value)` / `Add(locale, dictionary)` overloads, how `PenningtonOptions.Translations` is populated, and how `PenningtonStringLocalizer` resolves UI strings against the current `LocaleContext` with fallback to the default locale.
Does not cover: enabling multiple locales at the routing layer (see `LocalizationOptions`).

| Title | URL | Order |
|---|---|---|
| **`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`** | `/reference/options/auxiliary-options` | 70 |
Covers: the remaining option classes on `PenningtonOptions` with properties, defaults, and what each controls.
Does not cover: MonorailCSS options (separate page).

| Title | URL | Order |
|---|---|---|
| **`MonorailCssOptions`** | `/reference/options/monorail-css-options` | 80 |
Covers: `ColorScheme`, `CustomCssFrameworkSettings`, `ExtraStyles`, `ContentPaths`, and the two built-in color-scheme types (`NamedColorScheme`, `AlgorithmicColorScheme`).
Does not cover: the generator internals — see Explanation.

| Title | URL | Order |
|---|---|---|
| **`RoslynOptions`** | `/reference/options/roslyn-options` | 90 |
Covers: `SolutionPath` (path to `.sln`/`.slnx`), the optional `ProjectFilter` with `IncludedProjects`/`ExcludedProjects`, and how `AddPenningtonRoslyn` wires the options into the symbol-extraction and `xmldocid` preprocessing services shipped in `Pennington.Roslyn`.
Does not cover: authoring `xmldocid` code fences (see the Roslyn tutorial and code-annotations how-to).

### 3.2 Front Matter

| Title | URL | Order |
|---|---|---|
| **Front matter key reference** | `/reference/front-matter/keys` | 10 |
Covers: every built-in key — `title`, `description`, `isDraft`, `search`, `llms`, `uid`, `date`, `tags`, `section`, `order`, `redirectUrl`, `author`, `series`, `repository` — with type, default, and applicable front-matter type. `repository` is specific to `BlogSiteFrontMatter` and renders as a "source repository" link card on blog posts.
Does not cover: authoring practice (see How-Tos).

| Title | URL | Order |
|---|---|---|
| **`IFrontMatter` and capability defaults** | `/reference/front-matter/ifrontmatter` | 20 |
Covers: the `IFrontMatter` contract, which keys have default implementations, and how the consolidated capabilities (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) surface as default members.
Does not cover: the rationale for consolidation — see Explanation.

| Title | URL | Order |
|---|---|---|
| **Built-in front-matter types** | `/reference/front-matter/built-in-types` | 30 |
Covers: `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter` — the supported keys on each, which template wires each one (`AddBlogSite` binds `BlogSiteFrontMatter`, not `BlogFrontMatter`), and when to choose which.
Does not cover: defining custom front-matter types (see How-Tos).

### 3.3 Markdown Extensions

| Title | URL | Order |
|---|---|---|
| **Markdown extensions catalog** | `/reference/markdown/extensions` | 10 |
Covers: every non-CommonMark feature in one scannable page — tabs, alerts, code annotations, cross-reference tags — with syntax, arguments, and a minimal example each.
Does not cover: Markdig core syntax (tables, footnotes, etc.) — those follow Markdig's own docs.

| Title | URL | Order |
|---|---|---|
| **Code-block argument reference** | `/reference/markdown/code-block-args` | 20 |
Covers: the fence info-string grammar — language token, key/value attributes, quoted values — and the line-annotation syntax (`{1,3}`, `{+1}`, `{-1}`, `{focus …}`, `{error …}`).
Does not cover: theme selection at render time (see Explanation).

| Title | URL | Order |
|---|---|---|
| **Alert blocks** | `/reference/markdown/alerts` | 30 |
Covers: the five alert kinds (`NOTE`, `TIP`, `CAUTION`, `WARNING`, `IMPORTANT`), their emitted CSS classes, and default icons.
Does not cover: defining new alert kinds (would require a Markdig extension).

### 3.4 UI Components (Pennington.UI)

| Title | URL | Order |
|---|---|---|
| **Navigation components** | `/reference/ui/navigation` | 10 |
Covers: `TableOfContentsNavigation` and `OutlineNavigation` — parameters, slots, and how they bind to `NavigationInfo`.
Does not cover: rendering custom navigation shapes (a Razor authoring topic).

| Title | URL | Order |
|---|---|---|
| **Content components** | `/reference/ui/content` | 20 |
Covers: `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable` — parameters, render behavior, and the Pennington-facing component surface used from Mdazor-backed markdown content.
Does not cover: Mdazor parser internals or step-by-step authoring workflow (see the authoring how-to).

| Title | URL | Order |
|---|---|---|
| **Utility components** | `/reference/ui/utility` | 30 |
Covers: `LanguageSwitcher`, `StructuredData`, and `FallbackNotice` — parameters and when to use each.
Does not cover: authoring your own Razor components.

### 3.5 Extension Points

| Title | URL | Order |
|---|---|---|
| **Content pipeline interfaces** | `/reference/extension-points/content-pipeline` | 10 |
Covers: `IContentService`, `IContentParser`, `IContentRenderer`, `IContentPipeline`, and the `ContentItem`/`ContentSource` union types with every case.
Does not cover: implementation examples — see How-Tos.

| Title | URL | Order |
|---|---|---|
| **Routing types** | `/reference/extension-points/routing` | 20 |
Covers: `UrlPath`, `FilePath`, `ContentRoute`, and `ContentRouteFactory` — constructors, operators, and helper methods (`EnsureLeadingSlash`, `WithBaseUrl`, `AbsoluteUrl`, `IsDefaultLocale`).
Does not cover: the broader URL design philosophy (see Explanation).

| Title | URL | Order |
|---|---|---|
| **Response processing interfaces** | `/reference/extension-points/response-processing` | 30 |
Covers: `IResponseProcessor`, `IHtmlResponseRewriter`, execution order, and the three built-in rewriters (`XrefHtmlRewriter`, `LocaleLinkHtmlRewriter`, `BaseUrlHtmlRewriter`).
Does not cover: middleware ordering in ASP.NET at large.

| Title | URL | Order |
|---|---|---|
| **Island rendering interfaces** | `/reference/extension-points/islands` | 40 |
Covers: `IIslandRenderer`, `RazorIslandRenderer<T>`, `SpaEnvelope`, `RenderContext`, and the `data-spa-*` attribute surface.
Does not cover: the SPA client-side script (see Explanation).

| Title | URL | Order |
|---|---|---|
| **Highlighting interfaces** | `/reference/extension-points/highlighting` | 50 |
Covers: `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`.
Does not cover: writing TextMate grammars.

| Title | URL | Order |
|---|---|---|
| **Navigation types** | `/reference/extension-points/navigation` | 60 |
Covers: `NavigationBuilder`, `NavigationInfo`, `NavigationTreeItem`, and `BreadcrumbItem`.
Does not cover: replacing the default sidebar (UI component topic).

### 3.6 Host Integration

| Title | URL | Order |
|---|---|---|
| **DI and middleware extension methods** | `/reference/host/extensions` | 10 |
Covers: every `IServiceCollection` and `WebApplication` extension — `AddPennington`, `AddDocSite`, `AddMonorailCss`, `UsePennington`, `UseDocSite`, `UsePenningtonLocaleRouting`, `UsePenningtonLiveReload`, `UseMonorailCss`, `RunOrBuildAsync`, `RunDocSiteAsync`.
Does not cover: underlying services the extensions wire up.

| Title | URL | Order |
|---|---|---|
| **CLI and build arguments** | `/reference/host/cli` | 20 |
Covers: the `build [baseUrl] [outputDirectory]` arguments, dev-mode behavior, and environment variables consulted (e.g., `DOTNET_WATCH`).
Does not cover: platform-specific deployment (see How-Tos).

### 3.7 Diagnostics

| Title | URL | Order |
|---|---|---|
| **Build report fields** | `/reference/diagnostics/build-report` | 10 |
Covers: `BuildReport`, `BuildDiagnostic`, `BrokenLink`, severity levels, and how to read `Duration`/`GeneratedPages`/`FailedPages`.
Does not cover: what each warning means at the per-rule level.

| Title | URL | Order |
|---|---|---|
| **Request-scoped diagnostics** | `/reference/diagnostics/request-context` | 20 |
Covers: `DiagnosticContext`, `Diagnostic`, `DiagnosticSeverity`, and how the dev-mode overlay surfaces them.
Does not cover: wiring custom log sinks.

### 3.8 BlogSite Built-ins

| Title | URL | Order |
|---|---|---|
| **Built-in BlogSite routes** | `/reference/blogsite/routes` | 10 |
Covers: the routes the `Pennington.BlogSite` package ships out of the box — `/` (home), `/archive`, `/tags` and its alias `/topics`, `/tags/{TagEncodedName}` and its alias `/topics/{TagEncodedName}`, `/blog/{*fileName}` for individual posts, and `/rss.xml` when `EnableRss` is on — plus which `BlogSiteOptions` knobs (`TagsPageUrl`, `BlogBaseUrl`, `EnableRss`, `EnableSitemap`) each route honors.
Does not cover: customizing the Razor page bodies themselves (see the DocSite-components how-to, which applies symmetrically to BlogSite).

| Title | URL | Order |
|---|---|---|
| **Built-in `SocialIcons` `RenderFragment`s** | `/reference/blogsite/social-icons` | 20 |
Covers: the four static `RenderFragment` fields on `Pennington.BlogSite.Components.SocialIcons` — `GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, `MastodonIcon` — their SVG viewBoxes, the `currentColor`/`stroke` conventions they follow, and how to reference them directly (not as components) when populating `SocialLink.Icon`.
Does not cover: authoring new icon fragments — see the Razor-component how-to in Extensibility.

### 3.9 Structured Data

| Title | URL | Order |
|---|---|---|
| **JSON-LD schema types** | `/reference/structured-data/types` | 10 |
Covers: the record types in `Pennington.StructuredData.JsonLdTypes` — `JsonLdArticle`, `JsonLdBreadcrumbList` and its `JsonLdBreadcrumbItem`, `JsonLdWebSite` — plus `JsonLdSerializer` and how they feed the `<StructuredData>` UI component.
Does not cover: the schema.org vocabulary itself (out of scope) or the `<StructuredData>` component parameters (see `reference/ui/utility`).

---

## 4. Explanation

> Understanding-oriented. Discursive, read-away-from-the-keyboard. Answers "why" and "how does this fit together." Each page is a single concept, roughly 500–1,500 words.

### 4.1 Core Architecture

| Title | URL | Order |
|---|---|---|
| **The content pipeline and union types** | `/explanation/core/content-pipeline` | 10 |
Covers: why `ContentItem` and `ContentSource` are union types, how items move from `DiscoveredItem` → `ParsedItem` → `RenderedItem` (or `FailedItem`), and what the union affords over a class hierarchy.
Does not cover: the specific interfaces (see Reference).

| Title | URL | Order |
|---|---|---|
| **Dev mode and build mode share one code path** | `/explanation/core/dev-vs-build` | 20 |
Covers: the deliberate decision to run the same HTTP pipeline whether serving live or generating static output — the static build is a crawler driven by `OutputGenerationService` hitting the running app — and why this keeps dev fidelity and publish output in lockstep.
Does not cover: static-build CLI arguments (see Reference).

| Title | URL | Order |
|---|---|---|
| **The front-matter capability system** | `/explanation/core/front-matter-capabilities` | 30 |
Covers: why capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) were collapsed into `IFrontMatter` default members, what that buys users, and how to extend with custom keys.
Does not cover: key-by-key documentation (see Reference).

| Title | URL | Order |
|---|---|---|
| **The response-processing pipeline** | `/explanation/core/response-processing` | 40 |
Covers: why HTML rewriting is consolidated into a single AngleSharp pass, how `IResponseProcessor` differs from `IHtmlResponseRewriter`, and the order the built-in rewriters run.
Does not cover: writing a rewriter (see How-Tos).

### 4.2 Rendering and Theming

| Title | URL | Order |
|---|---|---|
| **MonorailCSS integration** | `/explanation/rendering/monorail-css` | 10 |
Covers: how classes are collected at response time (`CssClassCollectorProcessor`), how the stylesheet is generated on demand, and the color-scheme model (named vs. algorithmic with OKLCH palette generation).
Does not cover: MonorailCSS's own syntax (upstream docs).

| Title | URL | Order |
|---|---|---|
| **The syntax-highlighting cascade** | `/explanation/rendering/highlighting` | 20 |
Covers: the priority-ordered highlighter chain (custom → ShellHighlighter → TextMateHighlighter → PlainTextHighlighter), why TextMateSharp was chosen, and where deferred Roslyn support fits in.
Does not cover: how to add a highlighter (see How-Tos).

### 4.3 Routing and Navigation

| Title | URL | Order |
|---|---|---|
| **URL paths and content routes** | `/explanation/routing/url-paths` | 10 |
Covers: the `UrlPath`/`FilePath` value-type design, how `ContentRoute` carries canonical vs. output paths, and why path handling avoids string-typing.
Does not cover: member-level API details (see Reference).

| Title | URL | Order |
|---|---|---|
| **Navigation-tree construction** | `/explanation/routing/navigation-tree` | 20 |
Covers: how `NavigationBuilder` folds flat `ContentTocItem`s into a tree via `HierarchyParts`, what happens when a section has no direct content, and how locale prefixes are stripped for ordering.
Does not cover: the UI that renders the tree (see Reference).

| Title | URL | Order |
|---|---|---|
| **Cross-reference resolution** | `/explanation/routing/cross-references` | 30 |
Covers: how `uid`s are gathered during the content phase, how `XrefHtmlRewriter` resolves tags at request time, and how broken xrefs surface in the build report.
Does not cover: authoring cross-references (see How-Tos).

### 4.4 SPA and Islands

| Title | URL | Order |
|---|---|---|
| **SPA navigation and island architecture** | `/explanation/spa/islands` | 10 |
Covers: why Pennington serves a JSON envelope at a `_spa-data` path instead of full page HTML on navigation, how islands are hydrated selectively, and the skeleton/loading lifecycle (`data-spa-loading` modes).
Does not cover: registering an island (see How-Tos) or the raw attribute list (see Reference).

### 4.5 Localization

| Title | URL | Order |
|---|---|---|
| **Locale-aware URLs and content fallback** | `/explanation/localization/urls-and-fallback` | 10 |
Covers: how the URL prefix feeds into `ContentResolver` to reach the correct localized content, the fallback rules when a locale lacks a page, and why the search index is split per locale.
Does not cover: `LocalizationOptions` API specifics (see Reference).

### 4.6 Developer Experience

| Title | URL | Order |
|---|---|---|
| **Hot reload and file watching** | `/explanation/dev-experience/hot-reload` | 10 |
Covers: how `FileWatcher` observes content and asset directories, how `LiveReloadServer` pushes change events over WebSocket, and how `LiveReloadScriptProcessor` keeps the script out of production builds.
Does not cover: ASP.NET's own `dotnet watch` hot reload (upstream feature).

---

## Design notes (informing the choices above)

- **Four-quadrant split** mirrors the existing `Content/{tutorials,how-to,reference,explanation}` folders.
- **Two levels deep, no more.** Every quadrant has 4–8 sections; every section has 3–10 leaves. No three-level nesting; if a topic wants to grow, split into two sibling sections instead of nesting deeper.
- **Order numbers are sequential multiples of 10** within each section so new pages can be slotted in without renumbering siblings.
- **VitePress-style one-page catalogs** are used for dense areas (markdown extensions, front-matter keys, UI components) to keep search-by-ctrl-F viable.
- **Astro-style per-platform deployment pages** — one short page per host under How-To → Deployment — beat one mega-guide, and invite community contributions.
- **GitBook-inspired coverage of "publishing" concerns** — redirects, base URLs, sitemap, RSS, llms.txt — because a content-engine audience cares about those as much as a SaaS-docs audience does.
- **Explanation tier stays small and opinionated**, ~10 pages total. Concept pages that duplicate the reference get killed; concept pages that justify non-obvious design choices (unified dev/build path, capability consolidation, single-AngleSharp rewriting pass) stay.
- **Every "does not cover" line names a neighboring page** so readers bounce between quadrants instead of getting stuck or demanding a one-stop-shop page.
