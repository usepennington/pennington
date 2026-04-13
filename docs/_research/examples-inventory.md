# Pennington Examples Inventory

## How to use this file

This document catalogs every sample project in `B:\Penn\examples\` so documentation
writers can pick accurate code-fence targets. Every `xmldocid` entry below was
verified by reading the matching source file — do not add symbols here that you
have not confirmed exist. When you are about to author a code fence, grep this
file for the example project you want to reference and copy the `T:`/`M:`/`P:`
string verbatim. If you need a symbol that is not listed, read the referenced
source file directly and add it here.

Short methods (≤ ~15 lines) are marked `(short)` — these work best as focused
illustrations inside doc prose. Raw-file fence candidates at the bottom of each
section identify Markdown, YAML, and JSON fixtures that can be embedded verbatim.

Repo-relative paths use forward slashes (e.g. `examples/MinimalExample/...`).

---

## AlexBlogExample

One-line purpose: Minimal `BlogSite` template configured with `AddBlogSite` — a
personal developer blog with hero, socials, and a single blog content folder.

### Pennington features shown
- `Pennington.BlogSite.AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`
- `BlogSiteOptions` (hero, socials, projects, author info, RSS, sitemap)
- `HeroContent`, `SocialLink`, `SocialIcons`, `Project` components
- Out-of-the-box blog rendering with no custom C# types

### xmldocid symbol table
This project has no project-local types beyond `Program` and a test-partial.
All behavior is expressed through `BlogSiteOptions` in `Program.cs`.

| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Example consists only of top-level statements in Program.cs | examples/AlexBlogExample/Program.cs |

### Raw-file fence candidates
- `examples/AlexBlogExample/Program.cs` — canonical minimal blog-site wiring
- `examples/AlexBlogExample/Content/Blog/building-a-cli-part-1.md`
- `examples/AlexBlogExample/Content/Blog/building-a-cli-part-2.md`
- `examples/AlexBlogExample/Content/Blog/why-i-switched-to-linux.md`
- `examples/AlexBlogExample/appsettings.json`

---

## BeaconDocsExample

One-line purpose: `DocSite` template for a fictional HTTP monitoring product,
showing algorithmic color scheme, GitHub URL, header icon/badge, and draft
content handling.

### Pennington features shown
- `Pennington.DocSite.AddDocSite` + `UseDocSite` + `RunDocSiteAsync`
- `DocSiteOptions` (header icon, header content badge, footer, GitHub URL)
- `Pennington.MonorailCss.AlgorithmicColorScheme` with `PrimaryHue` and `BaseColorName`
- Draft pages (`isDraft: true`) under `Content/`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only — all customization flows through `DocSiteOptions` | examples/BeaconDocsExample/Program.cs |

### Raw-file fence candidates
- `examples/BeaconDocsExample/Program.cs`
- `examples/BeaconDocsExample/Content/index.md`
- `examples/BeaconDocsExample/Content/setup.md`
- `examples/BeaconDocsExample/Content/draft-page.md`
- `examples/BeaconDocsExample/Content/getting-started/install.md`
- `examples/BeaconDocsExample/Content/getting-started/index.md`
- `examples/BeaconDocsExample/Content/guides/configuration.md`
- `examples/BeaconDocsExample/Content/guides/pipeline-config.md`
- `examples/BeaconDocsExample/Content/guides/migration-v3.md`
- `examples/BeaconDocsExample/Content/api/index.md`
- `examples/BeaconDocsExample/Content/changelog/v3-2.md`
- `examples/BeaconDocsExample/appsettings.json`

---

## BlogExample

One-line purpose: Fuller `BlogSite` sample (Calvin's Chewing Chronicles) showing
custom fonts, algorithmic color scheme, multiple socials, per-page social images,
and `AdditionalRoutingAssemblies`.

### Pennington features shown
- `Pennington.BlogSite.AddBlogSite` with extended `BlogSiteOptions`
- `AdditionalRoutingAssemblies = [typeof(Program).Assembly]`
- `SocialMediaImageUrlFactory` per-page image URL resolver
- `AdditionalHtmlHeadContent` (Google Fonts preconnect + stylesheet)
- `MainSiteLinks` (`HeaderLink`), multiple `SocialLink` icons
- `AlgorithmicColorScheme` with PrimaryHue 300 (magenta)

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only | examples/BlogExample/Program.cs |

### Raw-file fence candidates
- `examples/BlogExample/Program.cs`
- `examples/BlogExample/Content/Blog/2024/03/chewing-magazine-review.md`
- `examples/BlogExample/Content/Blog/2024/04/gum-chewing-apparel-guide.md`
- `examples/BlogExample/Content/Blog/2024/05/bazooka-joe-interview.md`
- `examples/BlogExample/appsettings.json`

---

## ForgePortalExample

One-line purpose: Demonstrates an internal developer portal with three
content sources (docs/blog/pages), a custom `IContentService` that generates
release-notes pages from JSON, a custom `ICodeHighlighter`, and a custom
`IResponseProcessor` that injects a feedback widget.

### Pennington features shown
- `AddPennington` with three `AddMarkdownContent<T>` registrations
- Custom `IContentService` producing programmatic content from JSON files
- Custom `ICodeHighlighter` registered via `penn.Highlighting.AddHighlighter<T>`
- Custom `IResponseProcessor` injected via DI (`IResponseProcessor`)
- `ProgrammaticSource` / `IProgrammaticContentGenerator` / `TextProgrammaticContent`
- `CrossReference` emission for `forge.release.v*` uids

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:ForgePortalExample.PageFrontMatter | Minimal `IFrontMatter` record with a `Title` property (short) | examples/ForgePortalExample/PageFrontMatter.cs |
| P:ForgePortalExample.PageFrontMatter.Title | The page title property (short) | examples/ForgePortalExample/PageFrontMatter.cs |
| T:ForgePortalExample.ReleaseNote | Record describing a release (`Version`, `Date`, `Summary`, `Changes`) (short) | examples/ForgePortalExample/ReleaseNote.cs |
| T:ForgePortalExample.FeedbackWidgetProcessor | `IResponseProcessor` that appends a floating Feedback button before `</body>` | examples/ForgePortalExample/FeedbackWidgetProcessor.cs |
| P:ForgePortalExample.FeedbackWidgetProcessor.Order | Pipeline order (`500`) (short) | examples/ForgePortalExample/FeedbackWidgetProcessor.cs |
| M:ForgePortalExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext) | Predicate that skips non-HTML and search-index responses (short) | examples/ForgePortalExample/FeedbackWidgetProcessor.cs |
| M:ForgePortalExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext) | Inserts the widget HTML before the closing body tag | examples/ForgePortalExample/FeedbackWidgetProcessor.cs |
| T:ForgePortalExample.PipelineHighlighter | Partial `ICodeHighlighter` that colorizes a fictional `pipeline` DSL using regex | examples/ForgePortalExample/PipelineHighlighter.cs |
| P:ForgePortalExample.PipelineHighlighter.SupportedLanguages | Declares `pipeline` / `pipe` aliases (short) | examples/ForgePortalExample/PipelineHighlighter.cs |
| P:ForgePortalExample.PipelineHighlighter.Priority | Returns `60` (short) | examples/ForgePortalExample/PipelineHighlighter.cs |
| M:ForgePortalExample.PipelineHighlighter.Highlight(System.String,System.String) | Per-line highlighter emitting `hljs-*` spans | examples/ForgePortalExample/PipelineHighlighter.cs |
| T:ForgePortalExample.ReleaseNotesContentService | `IContentService` that reads `Content/releases/*.json` and emits programmatic HTML pages | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| P:ForgePortalExample.ReleaseNotesContentService.DefaultSection | Returns `"Releases"` (short) | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| P:ForgePortalExample.ReleaseNotesContentService.SearchPriority | Returns `50` (short) | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| M:ForgePortalExample.ReleaseNotesContentService.DiscoverAsync | Enumerates JSON files and yields `DiscoveredItem` with `ProgrammaticSource` | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| M:ForgePortalExample.ReleaseNotesContentService.GetContentTocEntriesAsync | Produces TOC entries under the `Releases` section | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| M:ForgePortalExample.ReleaseNotesContentService.GetCrossReferencesAsync | Emits `forge.release.v*` cross references | examples/ForgePortalExample/ReleaseNotesContentService.cs |
| T:ForgePortalExample.ContentHelper | Helper for discovering and rendering markdown pages across all registered services | examples/ForgePortalExample/ContentHelper.cs |
| M:ForgePortalExample.ContentHelper.GetAllPagesAsync | Walks every `IContentService` and parses `DocFrontMatter` | examples/ForgePortalExample/ContentHelper.cs |
| M:ForgePortalExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders the page whose canonical path matches a URL | examples/ForgePortalExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/ForgePortalExample/Program.cs`
- `examples/ForgePortalExample/Content/docs/getting-started.md`
- `examples/ForgePortalExample/Content/docs/pipeline-config.md`
- `examples/ForgePortalExample/Content/docs/api-keys.md`
- `examples/ForgePortalExample/Content/blog/welcome.md`
- `examples/ForgePortalExample/Content/blog/q1-retro.md`
- `examples/ForgePortalExample/Content/pages/index.md`
- `examples/ForgePortalExample/Content/pages/about.md`
- `examples/ForgePortalExample/appsettings.json`

---

## LocalizationExample

One-line purpose: `DocSite` with five playful locales (English, Pig Latin,
Bork Bork, Pirate, Klingon) — stress-test fixture for `ConfigureLocalization`
and `LocaleInfo` `HtmlLang` overrides.

### Pennington features shown
- `DocSiteOptions.ConfigureLocalization`
- `Pennington.Localization.LocaleInfo` with custom `HtmlLang` values (`sv-chef`, `en-pirate`, `tlh`)
- Per-locale content subfolders (`Content/pl/`, `Content/pi/`, `Content/sv/`, `Content/kl/`)
- Partial locale coverage (Klingon only has `index.md` and `menu.md`)

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only | examples/LocalizationExample/Program.cs |

### Raw-file fence candidates
- `examples/LocalizationExample/Program.cs`
- `examples/LocalizationExample/Content/index.md`
- `examples/LocalizationExample/Content/about.md`
- `examples/LocalizationExample/Content/menu.md`
- `examples/LocalizationExample/Content/faq.md`
- `examples/LocalizationExample/Content/getting-started.md`
- `examples/LocalizationExample/Content/pl/index.md`
- `examples/LocalizationExample/Content/pi/index.md`
- `examples/LocalizationExample/Content/sv/index.md`
- `examples/LocalizationExample/Content/kl/index.md`

---

## LocalizationTutorialExample

One-line purpose: Tutorial-grade DocSite with exactly two locales (English
and Spanish) for step-by-step localization walkthroughs.

### Pennington features shown
- Minimal `ConfigureLocalization` with `DefaultLocale = "en"`
- Two `AddLocale` calls — `en` (English) and `es` (Español)
- Partial locale coverage (`es/` only has `index.md` and `getting-started.md`)

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only | examples/LocalizationTutorialExample/Program.cs |

### Raw-file fence candidates
- `examples/LocalizationTutorialExample/Program.cs`
- `examples/LocalizationTutorialExample/Content/index.md`
- `examples/LocalizationTutorialExample/Content/getting-started.md`
- `examples/LocalizationTutorialExample/Content/configuration.md`
- `examples/LocalizationTutorialExample/Content/es/index.md`
- `examples/LocalizationTutorialExample/Content/es/getting-started.md`
- `examples/LocalizationTutorialExample/appsettings.json`

---

## MaraBlogExample

One-line purpose: `BlogSite` with customized content paths (`BlogContentPath = "Posts"`,
`TagsPageUrl = "/topics"`), a warm hue color scheme, and `MyWork` / `Socials`.

### Pennington features shown
- `BlogSiteOptions` with non-default `BlogContentPath` / `TagsPageUrl`
- `AlgorithmicColorScheme` (`PrimaryHue = 25`, base `Zinc`)
- `RunBlogSiteAsync(args)`, RSS + Sitemap enabled
- `Project` list for portfolio sidebar

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only | examples/MaraBlogExample/Program.cs |

### Raw-file fence candidates
- `examples/MaraBlogExample/Program.cs`
- `examples/MaraBlogExample/Content/Posts/allocation-traps.md`
- `examples/MaraBlogExample/Content/Posts/span-patterns.md`
- `examples/MaraBlogExample/Content/Posts/config-pitfalls.md`
- `examples/MaraBlogExample/Content/Posts/upcoming-talk-prep.md`
- `examples/MaraBlogExample/appsettings.json`

---

## MinimalExample

One-line purpose: Smallest possible Pennington setup — `AddPennington` +
`AddMarkdownContent<BlogFrontMatter>` + Razor components + MonorailCSS.
Used as the canonical "Hello World" for the docs.

### Pennington features shown
- `AddPennington` with `AddMarkdownContent<BlogFrontMatter>` (uses the core
  `Pennington.FrontMatter.BlogFrontMatter` record — not a project-local type)
- `AddMonorailCss` + `UseMonorailCss`
- `RunOrBuildAsync(args)` switch between serve and publish
- Custom `ContentHelper` that walks `IContentService.DiscoverAsync()` and calls
  `FrontMatterParser.Parse<T>` and `IContentRenderer.RenderAsync`
- Razor page component (`Pages.razor`) rendering markup string

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:MinimalExample.ContentHelper | Helper wrapping `IContentService` + `FrontMatterParser` + `IContentRenderer` | examples/MinimalExample/ContentHelper.cs |
| M:MinimalExample.ContentHelper.GetAllPagesAsync | Returns `(ContentRoute, BlogFrontMatter)` tuples for every markdown file discovered | examples/MinimalExample/ContentHelper.cs |
| M:MinimalExample.ContentHelper.GetPageByUrlAsync(System.String) | Parses front matter and renders HTML for a specific URL | examples/MinimalExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/MinimalExample/Program.cs`
- `examples/MinimalExample/ContentHelper.cs`
- `examples/MinimalExample/Components/App.razor`
- `examples/MinimalExample/Components/Routes.razor`
- `examples/MinimalExample/Components/Layout/MainLayout.razor`
- `examples/MinimalExample/Components/Layout/Home.razor`
- `examples/MinimalExample/Components/Layout/Pages.razor`
- `examples/MinimalExample/Content/index.md`
- `examples/MinimalExample/Content/sub-folder/page-one.md`
- `examples/MinimalExample/Content/sub-folder/page-two.md`
- `examples/MinimalExample/Content/sub-folder/sample-post.md`
- `examples/MinimalExample/Content/sub-folder/page-1.md` (redirect-only front matter)
- `examples/MinimalExample/appsettings.json`

---

## MultipleContentSourceExample

One-line purpose: Three parallel markdown content sources (root pages, blog,
docs) sharing one Pennington instance — demonstrates `ExcludePaths`,
`BasePageUrl`, and custom `Section` per source.

### Pennington features shown
- Three distinct `AddMarkdownContent<T>` registrations with different
  `ContentPath` / `BasePageUrl` / `Section` / `ExcludePaths`
- Three `IFrontMatter` records implementing different capability combinations
  (`ITaggable`, `IOrderable`, `IRedirectable`, `ISectionable`)
- Primary-constructor-style `ContentHelper` using generic `GetRenderedPageAsync<T>`
- `NavigationBuilder.BuildTree` scoped by `DefaultSection`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:MultipleContentSourceExample.BlogFrontMatter | Blog post front matter (`ITaggable`, `ISectionable`, `IRedirectable`) (short) | examples/MultipleContentSourceExample/BlogFrontMatter.cs |
| T:MultipleContentSourceExample.DocsFrontMatter | Docs front matter (`ITaggable`, `IOrderable`, `IRedirectable`) (short) | examples/MultipleContentSourceExample/DocsFrontMatter.cs |
| T:MultipleContentSourceExample.ContentFrontMatter | Root-page front matter (`ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable`) (short) | examples/MultipleContentSourceExample/ContentFrontMatter.cs |
| T:MultipleContentSourceExample.ContentHelper | Helper using `IContentService` + `FrontMatterParser` + `IContentRenderer` + `NavigationBuilder` | examples/MultipleContentSourceExample/ContentHelper.cs |
| M:MultipleContentSourceExample.ContentHelper.GetRenderedPageAsync``1(System.String) | Generic page lookup parameterized by `IFrontMatter` type | examples/MultipleContentSourceExample/ContentHelper.cs |
| M:MultipleContentSourceExample.ContentHelper.GetNavigationAsync(System.String,System.String) | Builds a navigation tree for a given `Section` | examples/MultipleContentSourceExample/ContentHelper.cs |
| T:MultipleContentSourceExample.RenderedPage\`1 | Record `(T FrontMatter, string Html, OutlineEntry[] Outline)` (short) | examples/MultipleContentSourceExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/MultipleContentSourceExample/Program.cs`
- `examples/MultipleContentSourceExample/BlogFrontMatter.cs`
- `examples/MultipleContentSourceExample/DocsFrontMatter.cs`
- `examples/MultipleContentSourceExample/ContentFrontMatter.cs`
- `examples/MultipleContentSourceExample/Content/index.md`
- `examples/MultipleContentSourceExample/Content/about.md`
- `examples/MultipleContentSourceExample/Content/blog/best-pizza-toppings.md`
- `examples/MultipleContentSourceExample/Content/blog/mystery-of-missing-socks.md`
- `examples/MultipleContentSourceExample/Content/blog/office-plant-survival-guide.md`
- `examples/MultipleContentSourceExample/Content/docs/coffee-brewing-guide.md`
- `examples/MultipleContentSourceExample/Content/docs/home-organization-systems.md`
- `examples/MultipleContentSourceExample/Content/docs/indoor-herb-garden.md`

---

## NorthwindHandbookExample

One-line purpose: Engineering handbook mixing a general doc source and a
dedicated changelog source with its own front matter shape, using
`ExcludePaths` to carve `changelog/` out of the default doc tree.

### Pennington features shown
- Two `AddMarkdownContent<T>` calls with overlapping paths + `ExcludePaths = ImmutableArray.Create("changelog")`
- Project-local `ChangelogFrontMatter` with `Version` / `IsBreaking` / `IOrderable`
- `AlgorithmicColorScheme` (PrimaryHue 220, Stone base)
- `ContentHelper` reading `MarkdownFileSource.Path` and rendering via `IContentRenderer`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:NorthwindHandbookExample.ChangelogFrontMatter | Record for changelog entries (`IOrderable`, `ITaggable`, with `Version`, `IsBreaking`) (short) | examples/NorthwindHandbookExample/ChangelogFrontMatter.cs |
| T:NorthwindHandbookExample.ContentHelper | Walks registered services, parses `DocFrontMatter`, renders HTML | examples/NorthwindHandbookExample/ContentHelper.cs |
| M:NorthwindHandbookExample.ContentHelper.GetAllPagesAsync | Returns all discovered markdown pages | examples/NorthwindHandbookExample/ContentHelper.cs |
| M:NorthwindHandbookExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders the markdown file whose canonical path matches a URL | examples/NorthwindHandbookExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/NorthwindHandbookExample/Program.cs`
- `examples/NorthwindHandbookExample/ChangelogFrontMatter.cs`
- `examples/NorthwindHandbookExample/Content/index.md`
- `examples/NorthwindHandbookExample/Content/development/index.md`
- `examples/NorthwindHandbookExample/Content/development/coding-standards.md`
- `examples/NorthwindHandbookExample/Content/development/pr-process.md`
- `examples/NorthwindHandbookExample/Content/operations/index.md`
- `examples/NorthwindHandbookExample/Content/operations/deployment-checklist.md`
- `examples/NorthwindHandbookExample/Content/operations/incident-response.md`
- `examples/NorthwindHandbookExample/Content/changelog/v2-0-0.md`
- `examples/NorthwindHandbookExample/Content/changelog/v2-0-1.md`
- `examples/NorthwindHandbookExample/Content/changelog/v2-1-0.md`

---

## PrismDocsExample

One-line purpose: `DocSite` backed by the Pennington.Roslyn integration —
demonstrates xmldocid code fences resolving symbols from two small on-disk
"Prism" projects (`Prism.Generators` v2 and `Prism.Legacy` v1).

### Pennington features shown
- `AddDocSite` + `AddPenningtonRoslyn` with `SolutionPath` under `Content/src/Prism.slnx`
- xmldocid fences in docs that resolve to types inside the embedded Prism projects
- V1 vs V2 diff-style comparisons via sibling source files

### xmldocid symbol table
These symbols live inside the fixture projects under `Content/src/`. They are
compiled by Roslyn at runtime, so any fence targeting them must run under the
DocSite with `AddPenningtonRoslyn` active.

| xmldocid | description | source file |
| --- | --- | --- |
| T:Prism.V2.Generators.EnumGenerator | Modern enum generator with `Initialize`/`Execute` (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| M:Prism.V2.Generators.EnumGenerator.Initialize(Prism.V2.Generators.GeneratorContext) | Registers syntax receiver and clears diagnostics (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| M:Prism.V2.Generators.EnumGenerator.Execute(Prism.V2.Generators.GeneratorContext) | Iterates candidate enums and calls `AddSource` (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| T:Prism.V2.Generators.GeneratorContext | Record capturing `SyntaxReceiver`, with no-op `RegisterForSyntaxNotifications` / `AddSource` (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| T:Prism.V2.Generators.EnumDeclaration | Positional record `(Name, Namespace)` (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| T:Prism.V2.Generators.EnumSyntaxReceiver | Empty receiver with a `Candidates` list (short) | examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs |
| T:Prism.V1.Generators.EnumGenerator | Legacy v1 generator preserved for diffs (short) | examples/PrismDocsExample/src/Prism.Legacy/EnumGenerator.cs |
| M:Prism.V1.Generators.EnumGenerator.Initialize(System.Object) | v1 stub initialization (short) | examples/PrismDocsExample/src/Prism.Legacy/EnumGenerator.cs |
| M:Prism.V1.Generators.EnumGenerator.Execute(System.Object) | Reflection-based enum enumeration across loaded assemblies (short) | examples/PrismDocsExample/src/Prism.Legacy/EnumGenerator.cs |

### Raw-file fence candidates
- `examples/PrismDocsExample/Program.cs`
- `examples/PrismDocsExample/Content/index.md`
- `examples/PrismDocsExample/Content/guides/enum-generator.md`
- `examples/PrismDocsExample/Content/guides/migration-v2.md`
- `examples/PrismDocsExample/src/Prism.Generators/EnumGenerator.cs`
- `examples/PrismDocsExample/src/Prism.Legacy/EnumGenerator.cs`

---

## RecipeExample

One-line purpose: CookLang-based recipe site — custom `IContentService`
parsing `.cook` files, plus a responsive-image service that emits WebP
variants through a programmatic route + minimal API endpoint.

### Pennington features shown
- Two custom `IContentService` implementations registered by the same DI descriptor twice (single instance, two contracts)
- `IRecipeContentService` / `IResponsiveImageContentService` project-local interfaces
- `RazorPageSource` for recipe pages, custom routes emitted per image size
- `NamedColorScheme` with `Amber` + `Sky` + `Orange` + `Yellow` palette
- `AdditionalRoutingAssemblies = [typeof(Program).Assembly]`
- MinimalAPI `app.MapGet` integrating with a Pennington content service

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:RecipeExample.Models.RecipeFrontMatter | YAML-annotated POCO with `Servings`, `PrepTime`, `CookTime`, `RestTime`, `Tags`, `Title`, `Description` | examples/RecipeExample/Models/RecipeFrontMatter.cs |
| T:RecipeExample.Models.RecipeContentPage | Rendered page wrapper around a CooklangSharp `Recipe` | examples/RecipeExample/Models/RecipeContentPage.cs |
| P:RecipeExample.Models.RecipeContentPage.DisplayName | Picks `Title` / `FileName` fallback (short) | examples/RecipeExample/Models/RecipeContentPage.cs |
| P:RecipeExample.Models.RecipeContentPage.Slug | Lower-cased `FileName` (short) | examples/RecipeExample/Models/RecipeContentPage.cs |
| T:RecipeExample.IRecipeContentService | Extends `IContentService` with `GetRecipeByUrlOrDefault` / `GetAllRecipesAsync` (short) | examples/RecipeExample/RecipeContentService.cs |
| T:RecipeExample.RecipeContentService | `IContentService` that scans `*.cook`, parses YAML front matter + Cooklang body | examples/RecipeExample/RecipeContentService.cs |
| P:RecipeExample.RecipeContentService.DefaultSection | Returns `"recipes"` (short) | examples/RecipeExample/RecipeContentService.cs |
| P:RecipeExample.RecipeContentService.SearchPriority | Returns `10` (short) | examples/RecipeExample/RecipeContentService.cs |
| M:RecipeExample.RecipeContentService.DiscoverAsync | Emits `DiscoveredItem` with `RazorPageSource` for each recipe URL | examples/RecipeExample/RecipeContentService.cs |
| M:RecipeExample.RecipeContentService.GetContentTocEntriesAsync | Builds immutable TOC entries with hierarchy parts | examples/RecipeExample/RecipeContentService.cs |
| M:RecipeExample.RecipeContentService.GetRecipeByUrlOrDefault(System.String) | Cache lookup by URL (short) | examples/RecipeExample/RecipeContentService.cs |
| M:RecipeExample.RecipeContentService.GetAllRecipesAsync | Returns every cached `RecipeContentPage` (short) | examples/RecipeExample/RecipeContentService.cs |
| T:RecipeExample.IResponsiveImageContentService | Interface for processing / LQIP / dimensions of responsive images | examples/RecipeExample/ResponsiveImageContentService.cs |
| T:RecipeExample.ResponsiveImageContentService | `IContentService` + `IResponsiveImageContentService` generating WebP variants via ImageSharp | examples/RecipeExample/ResponsiveImageContentService.cs |
| M:RecipeExample.ResponsiveImageContentService.GetImageDimensions(System.String,System.Int32,System.Int32) | Pure calculation of target width/height per size token | examples/RecipeExample/ResponsiveImageContentService.cs |
| M:RecipeExample.ResponsiveImageContentService.ProcessImageAsync(System.String,System.String) | Loads source, resizes, encodes WebP (LQIP adds Gaussian blur) | examples/RecipeExample/ResponsiveImageContentService.cs |
| M:RecipeExample.ResponsiveImageContentService.GenerateLqipAsync(System.String) | Thin wrapper around `ProcessImageAsync` with `"lqip"` (short) | examples/RecipeExample/ResponsiveImageContentService.cs |
| M:RecipeExample.ResponsiveImageContentService.GetOriginalImageDimensionsAsync(System.String) | Reads the source image dimensions (short) | examples/RecipeExample/ResponsiveImageContentService.cs |
| M:RecipeExample.ResponsiveImageContentService.DiscoverAsync | Emits a route per `{filename}-{size}.webp` combination | examples/RecipeExample/ResponsiveImageContentService.cs |

### Raw-file fence candidates
- `examples/RecipeExample/Program.cs`
- `examples/RecipeExample/Models/RecipeFrontMatter.cs`
- `examples/RecipeExample/recipes/chili.cook`
- `examples/RecipeExample/recipes/beer-cheese.cook`
- `examples/RecipeExample/recipes/bacon-wrapped-jalapenos.cook`
- `examples/RecipeExample/recipes/cajun-chicken-pasta.cook`
- `examples/RecipeExample/recipes/chicken-piccata.cook`
- `examples/RecipeExample/recipes/chex-mix.cook`
- `examples/RecipeExample/recipes/zuppa-toscana.cook`
- `examples/RecipeExample/appsettings.json`

---

## RoslynIntegrationExample

One-line purpose: Mirrors `MinimalExample` but layers `AddPenningtonRoslyn`
on top so markdown pages can embed xmldocid code fences pointing at the
Pennington solution itself.

### Pennington features shown
- `AddPenningtonRoslyn` with `SolutionPath = "../../Pennington.slnx"`
- Project-local `BlogFrontMatter` record implementing `ITaggable`, `IRedirectable`, `ISectionable`
- Standard Pennington markdown wiring identical to MinimalExample
- xmldocid code-fence content in `roslyn-integration-demo.md`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:RoslynIntegrationExample.BlogFrontMatter | Record for this site's posts (`ITaggable`, `IRedirectable`, `ISectionable`) (short) | examples/RoslynIntegrationExample/BlogFrontMatter.cs |
| T:RoslynIntegrationExample.ContentHelper | `IContentService` / parser / renderer helper identical in shape to MinimalExample's | examples/RoslynIntegrationExample/ContentHelper.cs |
| M:RoslynIntegrationExample.ContentHelper.GetAllPagesAsync | Enumerates all discovered markdown pages | examples/RoslynIntegrationExample/ContentHelper.cs |
| M:RoslynIntegrationExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders the markdown page whose canonical path matches a URL | examples/RoslynIntegrationExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/RoslynIntegrationExample/Program.cs`
- `examples/RoslynIntegrationExample/BlogFrontMatter.cs`
- `examples/RoslynIntegrationExample/Content/index.md`
- `examples/RoslynIntegrationExample/Content/roslyn-integration-demo.md`
- `examples/RoslynIntegrationExample/Content/sub-folder/page-one.md`
- `examples/RoslynIntegrationExample/Content/sub-folder/page-two.md`
- `examples/RoslynIntegrationExample/Content/sub-folder/sample-post.md`
- `examples/RoslynIntegrationExample/appsettings.json`

---

## SearchExample

One-line purpose: `DocSite` plus a custom `IContentService` that generates
1000 fake pages via Bogus / Faker — used to smoke-test the per-locale search
index at scale.

### Pennington features shown
- `DocSiteOptions` with `AdditionalRoutingAssemblies` and `ExtraStyles`
- Custom `IContentService` emitting `ProgrammaticSource` + `IProgrammaticContentGenerator`
- `Markdig` pipeline construction directly via `MarkdownPipelineBuilder`
- Inline `record` front matter (`RandomFrontMatter`) with `required` member
- `AlgorithmicColorScheme` + font overrides (Manrope / Petrona)
- Debug minimal API endpoint (`/debug/routes`) enumerating `EndpointDataSource`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:SearchExample.Services.RandomContentService | `IContentService` that generates 1000 fake content pages from Faker | examples/SearchExample/Services/RandomContentService.cs |
| P:SearchExample.Services.RandomContentService.DefaultSection | Returns `""` (short) | examples/SearchExample/Services/RandomContentService.cs |
| P:SearchExample.Services.RandomContentService.SearchPriority | Returns `1` (short) | examples/SearchExample/Services/RandomContentService.cs |
| M:SearchExample.Services.RandomContentService.DiscoverAsync | Yields a `DiscoveredItem` with a `ProgrammaticSource` per generated URL | examples/SearchExample/Services/RandomContentService.cs |
| M:SearchExample.Services.RandomContentService.GetContentTocEntriesAsync | Builds TOC entries with hierarchy parts sliced from the URL | examples/SearchExample/Services/RandomContentService.cs |
| M:SearchExample.Services.RandomContentService.GetContent(System.String) | Returns cached HTML for a URL, or `"not found"` (short) | examples/SearchExample/Services/RandomContentService.cs |
| T:SearchExample.Services.RandomFrontMatter | Record with a single required `Title` property (short) | examples/SearchExample/Services/RandomContentService.cs |

Note: the `Random` razor component referenced by `AdditionalRoutingAssemblies = [typeof(Random).Assembly]`
is the Razor page file `examples/SearchExample/Services/Random.razor` with a `@page "/random/{*fileName:nonfile}"`
directive. It is a `.razor` component, not a plain C# type — there is no usable `T:` xmldocid for it.

### Raw-file fence candidates
- `examples/SearchExample/Program.cs`
- `examples/SearchExample/Services/RandomContentService.cs`
- `examples/SearchExample/Services/Random.razor`
- `examples/SearchExample/appsettings.json`

---

## SpaNavigationExample

One-line purpose: Recipe book that demonstrates SPA navigation with two
`RazorIslandRenderer`-backed slots — a `content` slot for the article body
and a `recipe-info` slot for the sidebar metadata card.

### Pennington features shown
- `Pennington.Islands.AddSpaNavigation` / `UseSpaNavigation`
- Two `penn.Islands.Register<TRenderer>("slot-name")` calls
- `RazorIslandRenderer<TComponent>` subclasses delegating presentation to `.razor` components
- Per-page shared `ContentHelper` that exposes parsed `RecipeFrontMatter`
- `ComponentRenderer` scoped service

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:SpaNavigationExample.RecipeFrontMatter | Recipe-specific front matter with `PrepTime`, `CookTime`, `Servings`, `Difficulty` (short) | examples/SpaNavigationExample/RecipeFrontMatter.cs |
| T:SpaNavigationExample.ContentHelper | Standard `IContentService`/parser/renderer helper typed to `RecipeFrontMatter` | examples/SpaNavigationExample/ContentHelper.cs |
| M:SpaNavigationExample.ContentHelper.GetAllPagesAsync | Lists every discovered markdown recipe page | examples/SpaNavigationExample/ContentHelper.cs |
| M:SpaNavigationExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders HTML + front matter for a URL | examples/SpaNavigationExample/ContentHelper.cs |
| T:SpaNavigationExample.Slots.RecipeContentSlotRenderer | `RazorIslandRenderer<RecipeContent>` that wraps markdown with a title header | examples/SpaNavigationExample/Slots/RecipeContentSlotRenderer.cs |
| P:SpaNavigationExample.Slots.RecipeContentSlotRenderer.IslandName | Returns `"content"` (short) | examples/SpaNavigationExample/Slots/RecipeContentSlotRenderer.cs |
| M:SpaNavigationExample.Slots.RecipeContentSlotRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute) | Resolves page by URL and hands `Title` + `HtmlContent` to the razor component | examples/SpaNavigationExample/Slots/RecipeContentSlotRenderer.cs |
| T:SpaNavigationExample.Slots.RecipeInfoSlotRenderer | `RazorIslandRenderer<RecipeInfoCard>` producing the sidebar metadata card | examples/SpaNavigationExample/Slots/RecipeInfoSlotRenderer.cs |
| P:SpaNavigationExample.Slots.RecipeInfoSlotRenderer.IslandName | Returns `"recipe-info"` (short) | examples/SpaNavigationExample/Slots/RecipeInfoSlotRenderer.cs |
| M:SpaNavigationExample.Slots.RecipeInfoSlotRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute) | Skips index pages; projects `PrepTime`/`CookTime`/`Servings`/`Difficulty`/`Tags` | examples/SpaNavigationExample/Slots/RecipeInfoSlotRenderer.cs |

### Raw-file fence candidates
- `examples/SpaNavigationExample/Program.cs`
- `examples/SpaNavigationExample/RecipeFrontMatter.cs`
- `examples/SpaNavigationExample/Content/index.md`
- `examples/SpaNavigationExample/Content/chocolate-cake.md`
- `examples/SpaNavigationExample/Content/pasta-carbonara.md`
- `examples/SpaNavigationExample/Content/thai-green-curry.md`
- `examples/SpaNavigationExample/Slots/Components/RecipeContent.razor`
- `examples/SpaNavigationExample/Slots/Components/RecipeInfoCard.razor`

---

## SpaNavigationTutorialExample

One-line purpose: Tutorial-scoped SPA navigation sample using the framework's
`DocFrontMatter` directly and two islands — `article` body and `nav` sidebar
built from `NavigationBuilder.BuildTree`.

### Pennington features shown
- `AddMarkdownContent<DocFrontMatter>` with the framework-provided front matter
- Two `RazorIslandRenderer` subclasses (`ArticleIslandRenderer` + `NavIslandRenderer`)
- `NavigationBuilder.BuildTree` invoked from inside an island to populate the sidebar
- `ComponentRenderer` scoped service for island composition

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:SpaNavigationTutorialExample.ContentHelper | Walks services and renders `DocFrontMatter` pages | examples/SpaNavigationTutorialExample/ContentHelper.cs |
| M:SpaNavigationTutorialExample.ContentHelper.GetAllPagesAsync | Lists every markdown page | examples/SpaNavigationTutorialExample/ContentHelper.cs |
| M:SpaNavigationTutorialExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders the markdown page for a URL | examples/SpaNavigationTutorialExample/ContentHelper.cs |
| T:SpaNavigationTutorialExample.Islands.ArticleIslandRenderer | `RazorIslandRenderer<ArticleContent>` wiring `Title` + `HtmlContent` (short) | examples/SpaNavigationTutorialExample/Islands/ArticleIslandRenderer.cs |
| P:SpaNavigationTutorialExample.Islands.ArticleIslandRenderer.IslandName | Returns `"article"` (short) | examples/SpaNavigationTutorialExample/Islands/ArticleIslandRenderer.cs |
| M:SpaNavigationTutorialExample.Islands.ArticleIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute) | Resolves page by URL and hands parameters to the razor component (short) | examples/SpaNavigationTutorialExample/Islands/ArticleIslandRenderer.cs |
| T:SpaNavigationTutorialExample.Islands.NavIslandRenderer | `RazorIslandRenderer<SidebarNav>` that aggregates every TOC entry | examples/SpaNavigationTutorialExample/Islands/NavIslandRenderer.cs |
| P:SpaNavigationTutorialExample.Islands.NavIslandRenderer.IslandName | Returns `"nav"` (short) | examples/SpaNavigationTutorialExample/Islands/NavIslandRenderer.cs |
| M:SpaNavigationTutorialExample.Islands.NavIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute) | Walks services, collects `ContentTocItem`s, calls `NavigationBuilder.BuildTree` | examples/SpaNavigationTutorialExample/Islands/NavIslandRenderer.cs |

### Raw-file fence candidates
- `examples/SpaNavigationTutorialExample/Program.cs`
- `examples/SpaNavigationTutorialExample/Islands/ArticleIslandRenderer.cs`
- `examples/SpaNavigationTutorialExample/Islands/NavIslandRenderer.cs`
- `examples/SpaNavigationTutorialExample/Islands/Components/ArticleContent.razor`
- `examples/SpaNavigationTutorialExample/Islands/Components/SidebarNav.razor`
- `examples/SpaNavigationTutorialExample/Content/index.md`
- `examples/SpaNavigationTutorialExample/Content/introduction.md`
- `examples/SpaNavigationTutorialExample/Content/configuration.md`
- `examples/SpaNavigationTutorialExample/Content/lifecycle.md`
- `examples/SpaNavigationTutorialExample/Content/advanced.md`

---

## Spectre.Console.Examples

One-line purpose: **Not a Pennington site.** Console app that demonstrates
Spectre.Console features (tables, prompts, live display, progress). It serves
as a reference library of runnable snippets that `SpectreConsoleExample` docs
link to.

### Pennington features shown
- *(none — this project has no dependency on Pennington)*
- Plain `dotnet run <example-name>` dispatcher discovering `IExample` implementations via reflection

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:Spectre.Console.Examples.IExample | Interface with a single `Run(string[] args)` method (short) | examples/Spectre.Console.Examples/IExample.cs |
| M:Spectre.Console.Examples.IExample.Run(System.String[]) | Runs the example with CLI arguments (short) | examples/Spectre.Console.Examples/IExample.cs |
| T:StringExtensions | Top-level extension class in `Program.cs` with `ToKebabCase` | examples/Spectre.Console.Examples/Program.cs |
| M:StringExtensions.ToKebabCase(System.String) | Converts PascalCase to kebab-case (short) | examples/Spectre.Console.Examples/Program.cs |
| T:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample | Beginner tutorial covering markup, tables, text styling, progress | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.Run(System.String[]) | Runs all four demo steps in sequence | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowColoredHelloWorld | Prints colored markup greetings (short) | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowDataTable | Creates a `Table` with colored column headers and four rows | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowTextStyling | Demonstrates color/decoration markup and `Style` objects | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowProgressBar | Three-task progress simulation | examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs |
| T:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample | Interactive tutorial combining prompts, multi-selection, spinners, live display | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.Run(System.String[]) | Enters demo or interactive mode based on `--demo`/`-d` flag | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowBasicPrompts | `AnsiConsole.Ask` + `Confirm` walkthrough | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowMultiSelectionMenu | `MultiSelectionPrompt<string>` walkthrough | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowStatusSpinner | `AnsiConsole.Status().Start(...)` with changing spinners | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowLiveDisplay | `AnsiConsole.Live(table).Start` loop with random metrics | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |
| M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowCompleteDashboard | Menu-driven dashboard using `SelectionPrompt<string>` | examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs |

### Raw-file fence candidates
- `examples/Spectre.Console.Examples/Program.cs`
- `examples/Spectre.Console.Examples/IExample.cs`
- `examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs`
- `examples/Spectre.Console.Examples/console/tutorials/InteractivePromptAndDashboardTutorialExample.cs`

---

## SpectreConsoleExample

One-line purpose: Pennington documentation site *about* Spectre.Console,
organised as a three-sided doc portal (console, cli, blog) and laid out
using the Divio topic structure (tutorials / how-to / reference / explanation).

### Pennington features shown
- Three `AddMarkdownContent<T>` registrations sharing `SpectreDocFrontMatter` for docs + `SpectreBlogFrontMatter` for the blog
- `NamedColorScheme` with Sky / Zinc / Pink / Indigo / Violet palette
- `AdditionalRoutingAssemblies = [typeof(Program).Assembly]`
- Per-section navigation via `SpectreContentHelper.GetNavigationAsync`
- `NavigationInfo` (prev/next breadcrumb) via `NavigationBuilder.BuildNavigationInfo`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:SpectreConsoleExample.SpectreDocFrontMatter | Docs front matter with `Title`, `Section`, `Order`, `Tags`, etc. (short) | examples/SpectreConsoleExample/SpectreConsoleFrontMatter.cs |
| T:SpectreConsoleExample.SpectreBlogFrontMatter | Blog front matter with `Author`, `Series`, `Repository`, `Date` (short) | examples/SpectreConsoleExample/SpectreConsoleFrontMatter.cs |
| T:SpectreConsoleExample.SpectreContentHelper | Primary-constructor helper covering pages, navigation, and nav-info across sections | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| M:SpectreConsoleExample.SpectreContentHelper.GetRenderedPageAsync``1(System.String) | Generic render-by-URL matching any `IFrontMatter` subtype | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| M:SpectreConsoleExample.SpectreContentHelper.GetAllPagesAsync``1(System.String) | Generic enumeration filtered by URL prefix | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| M:SpectreConsoleExample.SpectreContentHelper.GetNavigationAsync(System.String,System.String) | Section-scoped `NavigationBuilder.BuildTree` wrapper | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| M:SpectreConsoleExample.SpectreContentHelper.GetNavigationInfoAsync(System.String,System.String) | Returns `NavigationInfo` (prev/next/breadcrumbs) for a URL within a section | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| T:SpectreConsoleExample.RenderedPage\`1 | Record `(T FrontMatter, string Html, OutlineEntry[] Outline)` (short) | examples/SpectreConsoleExample/SpectreContentHelper.cs |
| T:SpectreConsoleExample.PageInfo\`1 | Record `(string Url, T FrontMatter)` (short) | examples/SpectreConsoleExample/SpectreContentHelper.cs |

### Raw-file fence candidates
- `examples/SpectreConsoleExample/Program.cs`
- `examples/SpectreConsoleExample/SpectreConsoleFrontMatter.cs`
- `examples/SpectreConsoleExample/Content/index.md`
- `examples/SpectreConsoleExample/Content/console/index.md`
- `examples/SpectreConsoleExample/Content/console/tutorials/getting-started-building-rich-console-app.md`
- `examples/SpectreConsoleExample/Content/console/tutorials/interactive-prompt-and-dashboard-tutorial.md`
- `examples/SpectreConsoleExample/Content/console/how-to/displaying-tables-and-trees.md`
- `examples/SpectreConsoleExample/Content/console/reference/color-reference.md`
- `examples/SpectreConsoleExample/Content/console/explanation/understanding-rendering-model.md`
- `examples/SpectreConsoleExample/Content/cli/index.md`
- `examples/SpectreConsoleExample/Content/cli/tutorials/quick-start-your-first-cli-app.md`
- `examples/SpectreConsoleExample/Content/cli/how--to/defining-commands-and-arguments.md`
- `examples/SpectreConsoleExample/Content/cli/reference/api-reference.md`
- `examples/SpectreConsoleExample/Content/blog/spectre-console-0-50-0-aot-testing-cli-improvements.md`
- `examples/SpectreConsoleExample/appsettings.json`
- `examples/SpectreConsoleExample/Properties/launchSettings.json`

---

## TempoDocsExample

One-line purpose: Clean `DocSite` for a fictional task-scheduling library,
used to demonstrate the minimal happy-path for `AddDocSite` with
`AlgorithmicColorScheme` and a custom header SVG.

### Pennington features shown
- `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`
- `DocSiteOptions.HeaderIcon` inline SVG
- `AlgorithmicColorScheme` (PrimaryHue 150, base Zinc)
- `GitHubUrl` footer link

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| (none) | Top-level `Program.cs` only | examples/TempoDocsExample/Program.cs |

### Raw-file fence candidates
- `examples/TempoDocsExample/Program.cs`
- `examples/TempoDocsExample/Content/index.md`
- `examples/TempoDocsExample/Content/getting-started.md`
- `examples/TempoDocsExample/Content/configuration.md`
- `examples/TempoDocsExample/Content/api-reference.md`
- `examples/TempoDocsExample/appsettings.json`

---

## UserInterfaceExample

One-line purpose: "Daily Life Hub" sample — single-source content site
exercising the `Pennington.UI` component library (TableOfContentsNav,
OutlineNav, Badge, Card, CodeBlock, etc.) through a custom layout.

### Pennington features shown
- `AddMarkdownContent<DocsFrontMatter>` with project-local front matter
- `NavigationBuilder.BuildTree` + `OutlineEntry[]` returned from `IContentRenderer`
- `ContentHelper` returning a 3-tuple `(DocsFrontMatter, string Html, OutlineEntry[])`
- Emphasis on consuming `Pennington.UI` Razor components from user layouts

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:UserInterfaceExample.DocsFrontMatter | Record with `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` (short) | examples/UserInterfaceExample/DocsFrontMatter.cs |
| T:UserInterfaceExample.ContentHelper | Helper combining `IContentRenderer`, `FrontMatterParser`, and `NavigationBuilder` | examples/UserInterfaceExample/ContentHelper.cs |
| M:UserInterfaceExample.ContentHelper.GetPageByUrlAsync(System.String) | Renders a page and returns front matter + HTML + outline | examples/UserInterfaceExample/ContentHelper.cs |
| M:UserInterfaceExample.ContentHelper.GetNavigationTocAsync(System.String) | Collects TOC items from every service and builds a navigation tree | examples/UserInterfaceExample/ContentHelper.cs |

### Raw-file fence candidates
- `examples/UserInterfaceExample/Program.cs`
- `examples/UserInterfaceExample/DocsFrontMatter.cs`
- `examples/UserInterfaceExample/Content/index.md`
- `examples/UserInterfaceExample/Content/getting-started.md`
- `examples/UserInterfaceExample/Content/configuration.md`
- `examples/UserInterfaceExample/Content/api-reference.md`
- `examples/UserInterfaceExample/Content/troubleshooting.md`
- `examples/UserInterfaceExample/appsettings.json`

---

## YogaStudioExample

One-line purpose: Most elaborate sample — a full yoga studio marketing site
with a custom color scheme, Applies dictionary, JSON-backed schedule and
instructor catalog, SPA navigation islands, localization (English + Gen-Z),
and a programmatic route enumerator that fills gaps left by the Razor scanner.

### Pennington features shown
- `AddPennington` with `AdditionalRoutingAssemblies`
- `penn.Localization` (default `en` + `gen-z` with `HtmlLang = "en-genz"`)
- `penn.Islands.Register<YogaContentIslandRenderer>("content")`
- `UsePenningtonLocaleRouting` middleware chain
- `AddMonorailCss` with custom `IColorScheme` (`YogaColorScheme`) and `CustomCssFrameworkSettings.Applies.AddRange(YogaComponentApplies.All())`
- Custom `IContentService` (`YogaRouteContentService`) emitting parameterized and locale-prefixed routes via `ContentRouteFactory.FromRazorPage`
- `AddSpaNavigation` + `ComponentRenderer`

### xmldocid symbol table
| xmldocid | description | source file |
| --- | --- | --- |
| T:YogaStudioExample.Models.ScheduleEntry | Record `(Id, ClassName, InstructorId, DayOfWeek, StartTime, EndTime, ClassType, Level, Description, Room, MaxCapacity)` (short) | examples/YogaStudioExample/Models/ClassSchedule.cs |
| T:YogaStudioExample.Models.ScheduleData | Record wrapping `List<ScheduleEntry> Classes` (short) | examples/YogaStudioExample/Models/ClassSchedule.cs |
| T:YogaStudioExample.Models.InstructorProfile | Record `(Id, Name, Slug, Title, Bio, Specialties, PhotoUrl, YearsExperience, Certifications, Quote)` (short) | examples/YogaStudioExample/Models/Instructor.cs |
| T:YogaStudioExample.Models.InstructorData | Record wrapping `List<InstructorProfile> Instructors` (short) | examples/YogaStudioExample/Models/Instructor.cs |
| T:YogaStudioExample.Models.YogaFrontMatter | Record with `IOrderable`, `ISectionable`, `ITaggable`, adds `HeroImage` / `Layout` (short) | examples/YogaStudioExample/Models/YogaFrontMatter.cs |
| T:YogaStudioExample.Models.YogaBlogFrontMatter | Blog front matter with `Author`, `FeaturedImage`, `ReadingTimeMinutes` (short) | examples/YogaStudioExample/Models/BlogFrontMatter.cs |
| T:YogaStudioExample.Models.YogaColorScheme | Custom `IColorScheme` defining primary / accent / base palettes by hand | examples/YogaStudioExample/Models/YogaColorScheme.cs |
| M:YogaStudioExample.Models.YogaColorScheme.ApplyToTheme(MonorailCss.Theme.Theme) | Adds five color palettes onto the incoming MonorailCSS theme | examples/YogaStudioExample/Models/YogaColorScheme.cs |
| T:YogaStudioExample.Models.YogaComponentApplies | Static class exposing `All()` which returns an `ImmutableDictionary<string,string>` of component class mappings | examples/YogaStudioExample/Models/YogaComponentApplies.cs |
| M:YogaStudioExample.Models.YogaComponentApplies.All | Returns the yoga-specific `.yoga-*` Applies dictionary | examples/YogaStudioExample/Models/YogaComponentApplies.cs |
| T:YogaStudioExample.Services.InstructorService | JSON-backed service loading `Content/Data/instructors.json` lazily | examples/YogaStudioExample/Services/InstructorService.cs |
| M:YogaStudioExample.Services.InstructorService.GetAll | Returns every `InstructorProfile` (short) | examples/YogaStudioExample/Services/InstructorService.cs |
| M:YogaStudioExample.Services.InstructorService.GetBySlug(System.String) | Slug lookup, case-insensitive (short) | examples/YogaStudioExample/Services/InstructorService.cs |
| M:YogaStudioExample.Services.InstructorService.GetById(System.String) | ID lookup (short) | examples/YogaStudioExample/Services/InstructorService.cs |
| T:YogaStudioExample.Services.ScheduleService | JSON-backed service loading `Content/Data/schedule.json` | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetAllClasses | Returns every `ScheduleEntry` (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetClassesForDay(System.String) | Filters classes by day (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetClassById(System.String) | ID lookup (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetClassesByInstructor(System.String) | Filters by instructor ID (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetClassTypes | Distinct class types (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetLevels | Distinct difficulty levels (short) | examples/YogaStudioExample/Services/ScheduleService.cs |
| M:YogaStudioExample.Services.ScheduleService.GetWeeklySchedule | Returns a day-ordered grouping of classes | examples/YogaStudioExample/Services/ScheduleService.cs |
| T:YogaStudioExample.Services.ContentHelper | Multi-purpose content helper: static pages, blog posts, tag queries, locale segments | examples/YogaStudioExample/Services/ContentHelper.cs |
| M:YogaStudioExample.Services.ContentHelper.GetStaticPageAsync(System.String) | Reads `Content/pages/...` off disk honoring locale prefix | examples/YogaStudioExample/Services/ContentHelper.cs |
| M:YogaStudioExample.Services.ContentHelper.GetRenderedBlogPostAsync(System.String) | Renders a blog post via registered markdown services | examples/YogaStudioExample/Services/ContentHelper.cs |
| M:YogaStudioExample.Services.ContentHelper.GetAllBlogPostsAsync | Returns non-draft blog posts ordered by `Date` descending | examples/YogaStudioExample/Services/ContentHelper.cs |
| M:YogaStudioExample.Services.ContentHelper.GetBlogPostsByTagAsync(System.String) | Filter helper over `GetAllBlogPostsAsync` (short) | examples/YogaStudioExample/Services/ContentHelper.cs |
| M:YogaStudioExample.Services.ContentHelper.GetAllBlogTagsAsync | Distinct tag list across every blog post | examples/YogaStudioExample/Services/ContentHelper.cs |
| T:YogaStudioExample.Services.YogaRouteContentService | Programmatic `IContentService` that emits parameterized detail routes and locale-prefixed copies | examples/YogaStudioExample/Services/YogaRouteContentService.cs |
| P:YogaStudioExample.Services.YogaRouteContentService.DefaultSection | Returns `""` (short) | examples/YogaStudioExample/Services/YogaRouteContentService.cs |
| P:YogaStudioExample.Services.YogaRouteContentService.SearchPriority | Returns `5` (short) | examples/YogaStudioExample/Services/YogaRouteContentService.cs |
| M:YogaStudioExample.Services.YogaRouteContentService.DiscoverAsync | Yields `RazorPageSource` items for every enumerated route | examples/YogaStudioExample/Services/YogaRouteContentService.cs |
| T:YogaStudioExample.Islands.YogaContentIslandRenderer | `RazorIslandRenderer<YogaArticle>` that dispatches to blog or static-page helpers | examples/YogaStudioExample/Islands/YogaContentIslandRenderer.cs |
| P:YogaStudioExample.Islands.YogaContentIslandRenderer.IslandName | Returns `"content"` (short) | examples/YogaStudioExample/Islands/YogaContentIslandRenderer.cs |
| M:YogaStudioExample.Islands.YogaContentIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute) | Tries blog URL first, falls back to static pages (including locale variants) | examples/YogaStudioExample/Islands/YogaContentIslandRenderer.cs |

### Raw-file fence candidates
- `examples/YogaStudioExample/Program.cs`
- `examples/YogaStudioExample/Models/YogaColorScheme.cs`
- `examples/YogaStudioExample/Models/YogaComponentApplies.cs`
- `examples/YogaStudioExample/Content/blog/yoga-for-beginners.md`
- `examples/YogaStudioExample/Content/blog/morning-yoga-routine.md`
- `examples/YogaStudioExample/Content/blog/breathing-techniques.md`
- `examples/YogaStudioExample/Content/blog/gen-z/yoga-for-beginners.md`
- `examples/YogaStudioExample/Content/pages/about.md`
- `examples/YogaStudioExample/Content/pages/contact.md`
- `examples/YogaStudioExample/Content/pages/faq.md`
- `examples/YogaStudioExample/Content/pages/pricing.md`
- `examples/YogaStudioExample/Content/pages/gen-z/about.md`
- `examples/YogaStudioExample/appsettings.json`
