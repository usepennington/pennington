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

## `examples/GettingStartedMinimalSiteExample`

Backs tutorial §1.1.10 `/tutorials/getting-started/first-site`. Minimal
ASP.NET host demonstrating `AddPennington` + `UsePennington` with one markdown
page served via a plain `MapGet` endpoint. No DocSite template, no styling.

**Files**

- `examples/GettingStartedMinimalSiteExample/Program.cs` — canonical final state
- `examples/GettingStartedMinimalSiteExample/Content/index.md` — single page with `title` front matter
- `examples/GettingStartedMinimalSiteExample/Stage1_BareHost.cs` — tutorial stage 1
- `examples/GettingStartedMinimalSiteExample/Stage2_AddPennington.cs` — tutorial stage 2
- `examples/GettingStartedMinimalSiteExample/Stage3_UsePennington.cs` — tutorial stage 3

**Symbols**

- `T:GettingStartedMinimalSiteExample.Stage1`
- `M:GettingStartedMinimalSiteExample.Stage1.Run(System.String[])` (short)
- `T:GettingStartedMinimalSiteExample.Stage2`
- `M:GettingStartedMinimalSiteExample.Stage2.Run(System.String[])` (short)
- `T:GettingStartedMinimalSiteExample.Stage3`
- `M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])` (short)

Each `Run` is a static method whose body captures the tutorial's state at that
stage. None are invoked at runtime — they exist so tutorial prose can pull a
focused snippet with `csharp:xmldocid,bodyonly`.

**Raw-file fence candidates**

- `examples/GettingStartedMinimalSiteExample/Program.cs` (top-level statements, no xmldocid)
- `examples/GettingStartedMinimalSiteExample/Content/index.md`

## `examples/GettingStartedFirstPageExample`

Backs tutorial §1.1.20 `/tutorials/getting-started/first-page`. Three-page
markdown site that demonstrates the required `title:` front-matter key, the
file-path-to-URL mapping, and navigation auto-assembling as more files land
on disk. Same bare `AddPennington` host shape as the minimal example plus a
`NavigationBuilder` call so the nav strip on each page shows the current TOC.

**Files**

- `examples/GettingStartedFirstPageExample/Program.cs` — canonical final state (three pages wired)
- `examples/GettingStartedFirstPageExample/Content/index.md` — home page, title "Welcome", URL `/`
- `examples/GettingStartedFirstPageExample/Content/about.md` — second page, title "About", URL `/about`, `order: 20`
- `examples/GettingStartedFirstPageExample/Content/contact.md` — third page, title "Contact", URL `/contact`, `order: 30`
- `examples/GettingStartedFirstPageExample/Stage1_OneFile.cs` — tutorial stage 1 (one markdown file on disk)
- `examples/GettingStartedFirstPageExample/Stage2_AddAboutPage.cs` — tutorial stage 2 (two files; host code unchanged)
- `examples/GettingStartedFirstPageExample/Stage3_AddContactPage.cs` — tutorial stage 3 (three files; host code unchanged)

**Symbols**

- `T:GettingStartedFirstPageExample.Stage1`
- `M:GettingStartedFirstPageExample.Stage1.Run(System.String[])`
- `T:GettingStartedFirstPageExample.Stage2`
- `M:GettingStartedFirstPageExample.Stage2.Run(System.String[])` (short)
- `T:GettingStartedFirstPageExample.Stage3`
- `M:GettingStartedFirstPageExample.Stage3.Run(System.String[])` (short)

Each `Run` is a static method whose body captures the tutorial's state at that
stage. Stages 2 and 3 intentionally delegate to `Stage1.Run` — the tutorial's
point is that adding markdown files does **not** change the host code, so the
stage bodies reflect that by not diverging.

**Raw-file fence candidates**

- `examples/GettingStartedFirstPageExample/Program.cs` (top-level statements, no xmldocid)
- `examples/GettingStartedFirstPageExample/Content/index.md`
- `examples/GettingStartedFirstPageExample/Content/about.md`
- `examples/GettingStartedFirstPageExample/Content/contact.md`

## `examples/GettingStartedStylingExample`

Backs tutorial §1.1.30 `/tutorials/getting-started/styling`. Extends the
three-page shape from app #2 with `AddMonorailCss` + `UseMonorailCss`, a
`NamedColorScheme` (indigo/pink/cyan/amber/slate), and a minimal `Layout`
helper that wraps every rendered page in a utility-class scaffold so the
`CssClassCollectorProcessor` has classes to discover. The response-pipeline
collector means no explicit component or Razor layout is needed — the HTML
string returned from `MapGet` *is* the layout at the bare-host level.

**Files**

- `examples/GettingStartedStylingExample/Program.cs` — canonical final state (AddMonorailCss + UseMonorailCss + utility layout wired)
- `examples/GettingStartedStylingExample/Layout.cs` — `Layout.Render(title, navTree, bodyHtml)` shared HTML shell
- `examples/GettingStartedStylingExample/Content/index.md` — home page, title "Welcome", URL `/`
- `examples/GettingStartedStylingExample/Content/about.md` — second page, title "About", URL `/about`, `order: 20`
- `examples/GettingStartedStylingExample/Content/contact.md` — third page, title "Contact", URL `/contact`, `order: 30`, contains an inline `<p class="text-primary-700 font-semibold">` demonstrating class collection
- `examples/GettingStartedStylingExample/Stage1_WithoutStyling.cs` — tutorial stage 1 (no MonorailCSS yet; bare `AddPennington` + `UsePennington`)
- `examples/GettingStartedStylingExample/Stage2_AddMonorailCss.cs` — tutorial stage 2 (DI registration + layout wired, no endpoint yet)
- `examples/GettingStartedStylingExample/Stage3_UseMonorailCss.cs` — tutorial stage 3 (adds `app.UseMonorailCss()`, fully styled)

**Symbols**

- `T:GettingStartedStylingExample.Layout`
- `M:GettingStartedStylingExample.Layout.Render(System.String,System.Collections.Generic.IReadOnlyList{Pennington.Navigation.NavigationTreeItem},System.String)` (short)
- `T:GettingStartedStylingExample.Stage1`
- `M:GettingStartedStylingExample.Stage1.Run(System.String[])`
- `T:GettingStartedStylingExample.Stage2`
- `M:GettingStartedStylingExample.Stage2.Run(System.String[])`
- `T:GettingStartedStylingExample.Stage3`
- `M:GettingStartedStylingExample.Stage3.Run(System.String[])`

Each `Run` is a static method whose body captures the tutorial's state at
that stage — none are invoked at runtime. Stage 1 is the pre-MonorailCSS
snapshot; Stage 2 adds the DI registration and swaps in `Layout.Render`
(but does not yet mount `/styles.css`); Stage 3 is identical to the
top-level `Program.cs`, closing the loop with `app.UseMonorailCss()`.

**Raw-file fence candidates**

- `examples/GettingStartedStylingExample/Program.cs` (top-level statements, no xmldocid)
- `examples/GettingStartedStylingExample/Content/index.md`
- `examples/GettingStartedStylingExample/Content/about.md`
- `examples/GettingStartedStylingExample/Content/contact.md`

## `examples/DocSiteScaffoldExample`

Backs tutorial §1.2.10 `/tutorials/docsite/scaffold`. Swaps the bare
`AddPennington` host from app #1 for the DocSite template: `AddDocSite`
populates a `DocSiteOptions` (site title, description, GitHub URL, header /
footer content, two `ContentArea` entries) in one call, `UseDocSite` mounts
the full Razor-component chrome (sidebar area selector, header with search +
dark-mode + GitHub icon, footer, outline nav), and `RunDocSiteAsync` delegates
to `RunOrBuildAsync` so the same host handles dev and static build. Each area
slug maps to a top-level folder under `Content/` — `Content/guides/` and
`Content/reference/` — with one markdown page each.

**Files**

- `examples/DocSiteScaffoldExample/Program.cs` — canonical final state (AddDocSite + UseDocSite + RunDocSiteAsync, two areas)
- `examples/DocSiteScaffoldExample/Content/guides/index.md` — Guides area landing page
- `examples/DocSiteScaffoldExample/Content/reference/index.md` — Reference area landing page
- `examples/DocSiteScaffoldExample/Stage1_PenningtonOnly.cs` — tutorial stage 1 (pre-DocSite shape from tutorial 1)
- `examples/DocSiteScaffoldExample/Stage2_AddDocSite.cs` — tutorial stage 2 (DI call only, no `UseDocSite` yet)
- `examples/DocSiteScaffoldExample/Stage3_UseDocSite.cs` — tutorial stage 3 (final; matches `Program.cs`)

**Symbols**

- `T:DocSiteScaffoldExample.Stage1`
- `M:DocSiteScaffoldExample.Stage1.Run(System.String[])`
- `T:DocSiteScaffoldExample.Stage2`
- `M:DocSiteScaffoldExample.Stage2.Run(System.String[])` (short)
- `T:DocSiteScaffoldExample.Stage3`
- `M:DocSiteScaffoldExample.Stage3.Run(System.String[])` (short)

Production symbols the tutorial leans on (live in `src/Pennington.DocSite`):

- `T:Pennington.DocSite.DocSiteOptions`
- `P:Pennington.DocSite.DocSiteOptions.SiteTitle`
- `P:Pennington.DocSite.DocSiteOptions.Description`
- `P:Pennington.DocSite.DocSiteOptions.GitHubUrl`
- `P:Pennington.DocSite.DocSiteOptions.HeaderContent`
- `P:Pennington.DocSite.DocSiteOptions.FooterContent`
- `P:Pennington.DocSite.DocSiteOptions.Areas`
- `T:Pennington.DocSite.ContentArea`
- `M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})`
- `M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)`
- `M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`

Each `Run` is static, never invoked at runtime — the tutorial pulls each body
via `csharp:xmldocid,bodyonly`.

**Raw-file fence candidates**

- `examples/DocSiteScaffoldExample/Program.cs` (top-level statements, no xmldocid)
- `examples/DocSiteScaffoldExample/Content/guides/index.md`
- `examples/DocSiteScaffoldExample/Content/reference/index.md`

## `examples/DocSiteAuthorExample`

Backs tutorial §1.2.20 `/tutorials/docsite/first-doc-page`. Same single-file
DocSite host shape as app #4, but trimmed to **one** area (`Guides`) so the
tutorial's focus stays on *authoring*: a fully-populated `DocSiteFrontMatter`
block, a GitHub-style alert (`> [!NOTE]`), and a tabbed code group (adjacent
fenced blocks marked `tabs=true title="…"`). The outline nav on the rendered
page auto-populates from the markdown `##`/`###` headings via
`OutlineNavigation` JS. Stage files are static classes whose `Source()`
helpers return the markdown string for each tutorial state — stage 1 bare
front matter + h1, stage 2 adds the alert, stage 3 adds the tabbed code
group. Tutorial prose pulls each body via `csharp:xmldocid,bodyonly`.

**Files**

- `examples/DocSiteAuthorExample/Program.cs` — canonical final state (single-area DocSite host)
- `examples/DocSiteAuthorExample/Content/guides/index.md` — Guides area landing page
- `examples/DocSiteAuthorExample/Content/guides/authoring.md` — the teaching page with full `DocSiteFrontMatter`, a `[!NOTE]` alert, and a three-panel tabbed code group
- `examples/DocSiteAuthorExample/Stage1_BareFrontMatter.cs` — stage 1 markdown source (front matter + h1 only)
- `examples/DocSiteAuthorExample/Stage2_AddAlert.cs` — stage 2 markdown source (adds `[!NOTE]` block)
- `examples/DocSiteAuthorExample/Stage3_AddTabbedCode.cs` — stage 3 markdown source (adds tabbed code group)

**Symbols**

- `T:DocSiteAuthorExample.Stage1`
- `M:DocSiteAuthorExample.Stage1.Source` (short)
- `T:DocSiteAuthorExample.Stage2`
- `M:DocSiteAuthorExample.Stage2.Source` (short)
- `T:DocSiteAuthorExample.Stage3`
- `M:DocSiteAuthorExample.Stage3.Source` (short)

Production symbols the tutorial leans on (live in `src/Pennington.DocSite`
and `src/Pennington/Markdown/Extensions`):

- `T:Pennington.DocSite.DocSiteFrontMatter`
- `P:Pennington.DocSite.DocSiteFrontMatter.Title`
- `P:Pennington.DocSite.DocSiteFrontMatter.Description`
- `P:Pennington.DocSite.DocSiteFrontMatter.Tags`
- `P:Pennington.DocSite.DocSiteFrontMatter.Section`
- `P:Pennington.DocSite.DocSiteFrontMatter.Order`

Each `Source()` is a static method whose return value encodes the tutorial's
markdown at that stage — none is invoked at runtime. The final stage mirrors
the markdown that actually ships in `Content/guides/authoring.md`.

**Raw-file fence candidates**

- `examples/DocSiteAuthorExample/Program.cs` (top-level statements, no xmldocid)
- `examples/DocSiteAuthorExample/Content/guides/index.md`
- `examples/DocSiteAuthorExample/Content/guides/authoring.md`

## `examples/DocSiteSectionsExample`

Backs tutorial §1.2.30 `/tutorials/docsite/sections-and-areas`. Same
`AddDocSite` + `UseDocSite` + `RunDocSiteAsync` host shape as apps #4 and #5
— the focus here is the **structure** of `Content/`. Two areas (`Guides`,
`Reference`), each split into two subfolder-backed sections, with 2–3
ordered pages per section (nine content pages plus two area index pages,
eleven total). `NavigationBuilder` — invoked implicitly by the DocSite
`MainLayout` component — auto-creates a non-navigable section header for
each subfolder under an area and sorts the children by `order:` (tiebreaker:
title). The section header's own order is the minimum `order:` among its
children, so bumping a page's number early in a section pulls the whole
section toward the top. **Author tie-break gotcha:** when two sibling
sections share their minimum `order:`, the tie breaks alphabetically on the
title-cased folder name — `advanced/` with 10/20 sorts before
`getting-started/` with 10/20 because both minima are 10. Stagger numbers
across sibling sections (e.g. 10/20 for one, 40/50 for the next) so the
minima don't collide. The page tutorial at §1.2.30 must surface this rule
in prose. The `section:` front-matter key is carried through to
`ContentTocItem.Section` and surfaced on `NavigationInfo.SectionName` for
prev/next breadcrumbs, but **does not** drive sidebar grouping — the
subfolder is what groups.

**Files**

- `examples/DocSiteSectionsExample/Program.cs` — canonical final state (two areas, no other wiring)
- `examples/DocSiteSectionsExample/Content/guides/index.md` — Guides area landing page (order 0)
- `examples/DocSiteSectionsExample/Content/guides/getting-started/installation.md` — order 10
- `examples/DocSiteSectionsExample/Content/guides/getting-started/first-project.md` — order 20
- `examples/DocSiteSectionsExample/Content/guides/getting-started/configuration.md` — order 30
- `examples/DocSiteSectionsExample/Content/guides/advanced/custom-layouts.md` — order 40
- `examples/DocSiteSectionsExample/Content/guides/advanced/response-pipeline.md` — order 50
- `examples/DocSiteSectionsExample/Content/reference/index.md` — Reference area landing page (order 0)
- `examples/DocSiteSectionsExample/Content/reference/core-api/pennington-options.md` — order 10
- `examples/DocSiteSectionsExample/Content/reference/core-api/content-pipeline.md` — order 20
- `examples/DocSiteSectionsExample/Content/reference/extensions/markdown-extensions.md` — order 30
- `examples/DocSiteSectionsExample/Content/reference/extensions/content-services.md` — order 40
- `examples/DocSiteSectionsExample/Stage1_FlatArea.cs` — stage 1 markdown source (no `section:`/`order:` front matter, page lives directly under area folder)
- `examples/DocSiteSectionsExample/Stage2_SectionAndOrder.cs` — stage 2 markdown source (same page moved under `getting-started/` with `section:` + `order: 10`)

**Symbols**

- `T:DocSiteSectionsExample.Stage1`
- `M:DocSiteSectionsExample.Stage1.Source` (short)
- `T:DocSiteSectionsExample.Stage2`
- `M:DocSiteSectionsExample.Stage2.Source` (short)

Production symbols the tutorial leans on (live in `src/Pennington/Navigation`
and `src/Pennington.DocSite`):

- `T:Pennington.Navigation.NavigationBuilder`
- `M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)`
- `T:Pennington.Navigation.NavigationTreeItem`
- `T:Pennington.Content.ContentTocItem`
- `P:Pennington.DocSite.DocSiteFrontMatter.Section`
- `P:Pennington.DocSite.DocSiteFrontMatter.Order`

Each `Source()` is a static method whose return value encodes the tutorial's
markdown at that stage — none is invoked at runtime.

**Raw-file fence candidates**

- `examples/DocSiteSectionsExample/Program.cs` (top-level statements, no xmldocid)
- `examples/DocSiteSectionsExample/Content/guides/index.md`
- `examples/DocSiteSectionsExample/Content/guides/getting-started/installation.md`
- `examples/DocSiteSectionsExample/Content/guides/getting-started/first-project.md`
- `examples/DocSiteSectionsExample/Content/guides/getting-started/configuration.md`
- `examples/DocSiteSectionsExample/Content/guides/advanced/custom-layouts.md`
- `examples/DocSiteSectionsExample/Content/guides/advanced/response-pipeline.md`
- `examples/DocSiteSectionsExample/Content/reference/index.md`
- `examples/DocSiteSectionsExample/Content/reference/core-api/pennington-options.md`
- `examples/DocSiteSectionsExample/Content/reference/core-api/content-pipeline.md`
- `examples/DocSiteSectionsExample/Content/reference/extensions/markdown-extensions.md`
- `examples/DocSiteSectionsExample/Content/reference/extensions/content-services.md`

## `examples/BlogSiteScaffoldExample`

Backs tutorial §1.3.10 `/tutorials/blogsite/scaffold`. Swaps the bare
`AddPennington` host from app #1 for the BlogSite template: `AddBlogSite`
populates a `BlogSiteOptions` (site title, description, canonical base
URL, content paths, author name/bio) in one call; `UseBlogSite` mounts the
full Razor-component chrome (Home listing, `/archive`, `/blog/<slug>`,
`/tags`, `/tags/<name>`, `/topics` aliases) plus the `/rss.xml` endpoint;
`RunBlogSiteAsync` delegates to `RunOrBuildAsync` so the same host serves
dev and static build. Posts live under `Content/Blog/` (default
`BlogContentPath`, under default `ContentRootPath = "Content"`); a single
placeholder post `hello-world.md` keeps the Home listing and RSS feed
non-empty until tutorial §1.3.20 teaches the real `BlogSiteFrontMatter`
fields. `AddBlogSite` internally calls `AddPennington`, `AddMonorailCss`,
`AddRazorComponents`, and registers the Pennington.UI Mdazor components —
later BlogSite apps must not re-register these.

**Files**

- `examples/BlogSiteScaffoldExample/Program.cs` — canonical final state (AddBlogSite + UseBlogSite + RunBlogSiteAsync, single placeholder post)
- `examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md` — placeholder post keeping the pipeline happy (title, description, date, author)
- `examples/BlogSiteScaffoldExample/Stage1_BeforeAddBlogSite.cs` — tutorial stage 1 (pre-BlogSite bare AddPennington host)
- `examples/BlogSiteScaffoldExample/Stage2_AfterAddBlogSite.cs` — tutorial stage 2 (final; matches `Program.cs`)

**Symbols**

- `T:BlogSiteScaffoldExample.Stage1`
- `M:BlogSiteScaffoldExample.Stage1.Run(System.String[])`
- `T:BlogSiteScaffoldExample.Stage2`
- `M:BlogSiteScaffoldExample.Stage2.Run(System.String[])` (short)

Production symbols the tutorial leans on (live in `src/Pennington.BlogSite`):

- `T:Pennington.BlogSite.BlogSiteOptions`
- `P:Pennington.BlogSite.BlogSiteOptions.SiteTitle`
- `P:Pennington.BlogSite.BlogSiteOptions.Description`
- `P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.ContentRootPath`
- `P:Pennington.BlogSite.BlogSiteOptions.BlogContentPath`
- `P:Pennington.BlogSite.BlogSiteOptions.BlogBaseUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.TagsPageUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.AuthorName`
- `P:Pennington.BlogSite.BlogSiteOptions.AuthorBio`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableRss`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap`
- `T:Pennington.BlogSite.BlogSiteServiceExtensions`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`

Each `Run` is a static method whose body captures the tutorial's state at
that stage. Neither is invoked at runtime — the tutorial pulls each body
via `csharp:xmldocid,bodyonly`.

**Raw-file fence candidates**

- `examples/BlogSiteScaffoldExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md`

## `examples/BlogSiteFirstPostExample`

Backs tutorial §1.3.20 `/tutorials/blogsite/first-post`. Extends the
`BlogSiteScaffoldExample` host with a fully-populated post that lights up
every field on `BlogSiteFrontMatter` a post author will touch — `title`,
`description`, `date`, `author`, `tags`, `series`, `repository`,
`section`, and `redirectUrl`. `EnableRss = true` is specified explicitly
in `Program.cs` (redundant because the default is already `true`) so the
tutorial has a symbol to point at when it introduces the RSS feed. The
post replaces the scaffold's placeholder `hello-world.md`; `/`, `/archive`,
`/blog/my-first-post/`, `/tags/<tag>/`, and `/rss.xml` all surface the
populated front-matter fields. The post page shows the series banner,
the tag chips, and a "Source Code" link driven by `repository:`. The RSS
item carries `<title>`, `<link>`, `<guid>`, `<description>`, `<pubDate>`,
and `<author>`.

**Files**

- `examples/BlogSiteFirstPostExample/Program.cs` — canonical final state (AddBlogSite with explicit `EnableRss = true`, no placeholder post)
- `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md` — the fully-populated post
- `examples/BlogSiteFirstPostExample/Stage1_BareFrontMatter.cs` — stage 1 markdown source (title + description + date only)
- `examples/BlogSiteFirstPostExample/Stage2_FullFrontMatter.cs` — stage 2 markdown source (every BlogSiteFrontMatter field populated)

**Symbols**

- `T:BlogSiteFirstPostExample.Stage1`
- `M:BlogSiteFirstPostExample.Stage1.Source` (short)
- `T:BlogSiteFirstPostExample.Stage2`
- `M:BlogSiteFirstPostExample.Stage2.Source` (short)

Production symbols the tutorial leans on (live in `src/Pennington.BlogSite`):

- `T:Pennington.BlogSite.BlogSiteFrontMatter`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Title`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Description`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Date`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Author`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Tags`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Series`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Repository`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Section`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.RedirectUrl`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.IsDraft`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Uid`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Search`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Llms`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableRss`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap`

Each `Source()` is a static method whose return value encodes the
tutorial's markdown at that stage — none is invoked at runtime. The
final stage mirrors the markdown that actually ships in
`Content/Blog/my-first-post.md`.

**Raw-file fence candidates**

- `examples/BlogSiteFirstPostExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BlogSiteFirstPostExample/Content/Blog/my-first-post.md`

## `examples/BlogSiteHeroProjectsSocialsExample`

Backs tutorial §1.3.30 `/tutorials/blogsite/hero-projects-socials`. Extends
the tutorial-8 host by populating the four homepage surfaces on
`BlogSiteOptions`: `HeroContent` (headline block at the top of `/`),
`MyWork` (a `Project[]` rendered as the "My Work" sidebar card), `Socials`
(a `SocialLink[]` rendered as an icon row under the card), and
`MainSiteLinks` (a `HeaderLink[]` rendered in the site top-nav and the
footer). The four built-in icon `RenderFragment`s live as `static
readonly` fields on the `Pennington.BlogSite.Components.SocialIcons`
component — `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`
— and are passed to `SocialLink` directly (no wrapper type). One post
(`weekend-content-engine.md`) populates the recent-posts slot.

**Files**

- `examples/BlogSiteHeroProjectsSocialsExample/Program.cs` — canonical final state (all four surfaces populated, four social icons wired)
- `examples/BlogSiteHeroProjectsSocialsExample/Content/Blog/weekend-content-engine.md` — single post keeping the recent-posts list non-empty
- `examples/BlogSiteHeroProjectsSocialsExample/Stage1_HeroOnly.cs` — stage 1 (HeroContent populated, projects and socials still empty)
- `examples/BlogSiteHeroProjectsSocialsExample/Stage2_AddProjects.cs` — stage 2 (adds `MyWork = [Project, Project, Project]`)
- `examples/BlogSiteHeroProjectsSocialsExample/Stage3_AddSocialsAndHeader.cs` — stage 3 (adds `Socials` + `MainSiteLinks`; matches `Program.cs`)

**Symbols**

- `T:BlogSiteHeroProjectsSocialsExample.Stage1`
- `M:BlogSiteHeroProjectsSocialsExample.Stage1.Run(System.String[])`
- `T:BlogSiteHeroProjectsSocialsExample.Stage2`
- `M:BlogSiteHeroProjectsSocialsExample.Stage2.Run(System.String[])`
- `T:BlogSiteHeroProjectsSocialsExample.Stage3`
- `M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])`

Production symbols the tutorial leans on (live in `src/Pennington.BlogSite`):

- `T:Pennington.BlogSite.BlogSiteOptions`
- `P:Pennington.BlogSite.BlogSiteOptions.HeroContent`
- `P:Pennington.BlogSite.BlogSiteOptions.MyWork`
- `P:Pennington.BlogSite.BlogSiteOptions.Socials`
- `P:Pennington.BlogSite.BlogSiteOptions.MainSiteLinks`
- `T:Pennington.BlogSite.HeroContent`
- `T:Pennington.BlogSite.Project`
- `T:Pennington.BlogSite.SocialLink`
- `T:Pennington.BlogSite.HeaderLink`
- `T:Pennington.BlogSite.Components.SocialIcons`
- `F:Pennington.BlogSite.Components.SocialIcons.GithubIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.BlueskyIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.LinkedInIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.MastodonIcon`

Each `Run` is a static method whose body captures the tutorial's state at
that stage. None are invoked at runtime — the tutorial pulls each body
via `csharp:xmldocid,bodyonly`.

**Record shapes — locked for app #14**

- `HeroContent(string Title, string Description)` — two positional
  parameters. The `Description` is rendered as `MarkupString` in
  `Home.razor`, so light HTML is OK (the tutorial sticks to plain prose).
- `Project(string Title, string Description, string Url)` — three
  positional parameters. `Url` is used as an `<a href>` wrapping the
  `<dt>`/`<dd>` pair.
- `SocialLink(RenderFragment Icon, string Url)` — `Icon` is a
  `Microsoft.AspNetCore.Components.RenderFragment`, NOT a component
  type / generic / string name. The icon ships as a `static readonly
  RenderFragment` field on the `SocialIcons` Razor component. `Url`
  is used as the `<a href>` target; the fragment renders inside.
- `HeaderLink(string Title, string Url)` — two positional parameters.
  Rendered both in the top-nav and the footer nav of `MainLayout.razor`.

**Raw-file fence candidates**

- `examples/BlogSiteHeroProjectsSocialsExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BlogSiteHeroProjectsSocialsExample/Content/Blog/weekend-content-engine.md`

## `examples/BeyondLocaleExample`

Backs tutorial §1.4.10 `/tutorials/beyond-basics/add-a-locale`. Same
`AddDocSite` + `UseDocSite` + `RunDocSiteAsync` host shape as apps #4–#6 —
the focus here is **localization**. A single `ConfigureLocalization` action
on `DocSiteOptions` registers two locales (`en` default, `es` secondary)
and the rest of the site behavior follows: `UseDocSite` already calls
`UsePenningtonLocaleRouting` first thing, so the locale detection middleware
rewrites `/es/about` to `/about` inside `PathBase` before endpoint routing;
the built-in `LanguageSwitcher` in `MainLayout.razor` lights up as soon as
`LocalizationOptions.IsMultiLocale` is true; and `ContentResolver` reads
translated markdown from `Content/<locale>/` subfolders, falling back to the
default locale when a translation is missing. Three English pages live
directly under `Content/` (the default locale owns the URL root — `/`,
`/about/`, `/getting-started/`); three Spanish translations live under
`Content/es/` and serve at `/es/`, `/es/about/`, `/es/getting-started/`.
No manual layout edits — the switcher is baked into DocSite's chrome.

**Files**

- `examples/BeyondLocaleExample/Program.cs` — canonical final state (DocSite host with two locales)
- `examples/BeyondLocaleExample/Content/index.md` — English home (default locale, no prefix)
- `examples/BeyondLocaleExample/Content/about.md` — English about page
- `examples/BeyondLocaleExample/Content/getting-started.md` — English walkthrough
- `examples/BeyondLocaleExample/Content/es/index.md` — Spanish home (served at `/es/`)
- `examples/BeyondLocaleExample/Content/es/about.md` — Spanish about page (served at `/es/about/`)
- `examples/BeyondLocaleExample/Content/es/getting-started.md` — Spanish walkthrough
- `examples/BeyondLocaleExample/Stage1_EnglishOnly.cs` — stage 1 host (single-locale DocSite, switcher hidden)
- `examples/BeyondLocaleExample/Stage2_AddSecondLocale.cs` — stage 2 host (adds `ConfigureLocalization`; switcher appears)
- `examples/BeyondLocaleExample/Stage3_SwitcherAppears.cs` — stage 3 host (final; matches `Program.cs`)

**Symbols**

- `T:BeyondLocaleExample.Stage1`
- `M:BeyondLocaleExample.Stage1.Run(System.String[])` (short)
- `T:BeyondLocaleExample.Stage2`
- `M:BeyondLocaleExample.Stage2.Run(System.String[])` (short)
- `T:BeyondLocaleExample.Stage3`
- `M:BeyondLocaleExample.Stage3.Run(System.String[])` (short)

Production symbols the tutorial leans on (live in `src/Pennington`,
`src/Pennington.UI`, and `src/Pennington.DocSite`):

- `T:Pennington.Infrastructure.LocalizationOptions`
- `P:Pennington.Infrastructure.LocalizationOptions.DefaultLocale`
- `P:Pennington.Infrastructure.LocalizationOptions.Locales`
- `P:Pennington.Infrastructure.LocalizationOptions.IsMultiLocale`
- `M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,Pennington.Localization.LocaleInfo)`
- `M:Pennington.Infrastructure.LocalizationOptions.AddLocale(System.String,System.String)`
- `T:Pennington.Localization.LocaleInfo`
- `T:Pennington.Localization.LocaleContext`
- `T:Pennington.Localization.LocaleDetectionMiddleware`
- `M:Pennington.Infrastructure.PenningtonExtensions.UsePenningtonLocaleRouting(Microsoft.AspNetCore.Builder.WebApplication)`
- `T:Pennington.UI.Components.LanguageSwitcher`
- `P:Pennington.DocSite.DocSiteOptions.ConfigureLocalization`

Each `Run` is a static method whose body captures the tutorial's state at
that stage. None are invoked at runtime — tutorial prose pulls each body
via `csharp:xmldocid,bodyonly`.

**Locale URL scheme — locked for app #13**

- **Default locale lives at the URL root.** With `DefaultLocale = "en"`
  and English markdown under `Content/`, English pages serve from `/`,
  `/about/`, `/getting-started/` — no `/en/` prefix, no redirect.
- **Every non-default locale gets a code prefix.** Spanish markdown under
  `Content/es/` serves from `/es/`, `/es/about/`, `/es/getting-started/`.
  The prefix string is exactly the locale code passed to `AddLocale`.
- **`LocaleDetectionMiddleware` rewrites paths into `PathBase`** so Blazor
  routing (`@page "/{*fileName:nonfile}"` in `Pages.razor`) sees the
  locale-stripped path. `ContentResolver` re-reads the full request path
  from `NavigationManager.Uri` to pick the right translation.
- **Content fallback on missing translations.** If a Spanish file is
  absent, `ContentResolver` falls back to the English copy and renders a
  fallback banner naming the requested and default locales.

**Raw-file fence candidates**

- `examples/BeyondLocaleExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BeyondLocaleExample/Content/index.md`
- `examples/BeyondLocaleExample/Content/about.md`
- `examples/BeyondLocaleExample/Content/getting-started.md`
- `examples/BeyondLocaleExample/Content/es/index.md`
- `examples/BeyondLocaleExample/Content/es/about.md`
- `examples/BeyondLocaleExample/Content/es/getting-started.md`

## `examples/BeyondRoslynExample`

Backs tutorial §1.4.20 `/tutorials/beyond-basics/connect-roslyn`. This is
the first example app with a **dual-project structure**: the DocSite host
(`BeyondRoslynExample.csproj`) lives at the folder root, and a sibling
`Sample/BeyondRoslynExample.Sample.csproj` class library holds the types
that the tutorial's markdown pages fence via `csharp:xmldocid`. The two
csprojs don't reference each other at compile time — Pennington.Roslyn
loads the Sample library through an **inner slnx**
(`BeyondRoslynExample.slnx`) that `RoslynOptions.SolutionPath` points at.
Both csprojs are registered in the main `Pennington.slnx` under
`/examples/` so `dotnet build Pennington.slnx` compiles them. The host
csproj sets `DefaultItemExcludes` to skip `Sample\**` so the two projects
don't fight over the same `.cs` files.

`AddPenningtonRoslyn(options => options.SolutionPath = "BeyondRoslynExample.slnx")`
is the single DI line that registers `RoslynCodeBlockPreprocessor` as an
`ICodeBlockPreprocessor`. Markdown fences whose info string ends in
`:xmldocid`, `:xmldocid,bodyonly`, or `:xmldocid-diff` (body contains one
XmlDocId per line; two IDs for `-diff`) resolve against the loaded
solution and render real source highlighted via `SyntaxHighlighter`.

**Files**

- `examples/BeyondRoslynExample/BeyondRoslynExample.csproj` — docs host (references `Pennington.DocSite` + `Pennington.Roslyn`; excludes `Sample\**` from its globs)
- `examples/BeyondRoslynExample/BeyondRoslynExample.slnx` — inner slnx registering only the Sample library; `SolutionPath` points at it
- `examples/BeyondRoslynExample/Program.cs` — canonical final state (DocSite + AddPenningtonRoslyn)
- `examples/BeyondRoslynExample/Stage1_NoRoslyn.cs` — stage 1 host (DocSite only; xmldocid fences render raw)
- `examples/BeyondRoslynExample/Stage2_AddRoslyn.cs` — stage 2 host (adds `AddPenningtonRoslyn`; fences resolve)
- `examples/BeyondRoslynExample/Content/index.md` — landing page, links to `api-pulls`
- `examples/BeyondRoslynExample/Content/api-pulls.md` — five xmldocid fences (`T:`, `M:`, bodyonly, multi-symbol)
- `examples/BeyondRoslynExample/Sample/BeyondRoslynExample.Sample.csproj` — sibling library (`GenerateDocumentationFile=true`)
- `examples/BeyondRoslynExample/Sample/Calculator.cs` — fence target with `Add`, `Multiply`, `Mean`
- `examples/BeyondRoslynExample/Sample/Greeter.cs` — fence target with `Prefix` + `Greet`

**Symbols (stage files — host)**

- `T:BeyondRoslynExample.Stage1`
- `M:BeyondRoslynExample.Stage1.Run(System.String[])` (short)
- `T:BeyondRoslynExample.Stage2`
- `M:BeyondRoslynExample.Stage2.Run(System.String[])` (short)

**Symbols (Sample library — xmldocid fence targets)**

- `T:BeyondRoslynExample.Sample.Calculator`
- `M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)` (short)
- `M:BeyondRoslynExample.Sample.Calculator.Multiply(System.Int32,System.Int32)` (short)
- `M:BeyondRoslynExample.Sample.Calculator.Mean(System.Collections.Generic.IReadOnlyList{System.Int32})`
- `T:BeyondRoslynExample.Sample.Greeter`
- `P:BeyondRoslynExample.Sample.Greeter.Prefix` (short)
- `M:BeyondRoslynExample.Sample.Greeter.#ctor(System.String)` (short)
- `M:BeyondRoslynExample.Sample.Greeter.Greet(System.String)` (short)

**Production symbols the tutorial leans on** (live in
`src/Pennington.Roslyn`):

- `T:Pennington.Roslyn.RoslynOptions`
- `P:Pennington.Roslyn.RoslynOptions.SolutionPath`
- `P:Pennington.Roslyn.RoslynOptions.ProjectFilter`
- `T:Pennington.Roslyn.ProjectFilter`
- `T:Pennington.Roslyn.RoslynExtensions`
- `M:Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Roslyn.RoslynOptions})`
- `T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`
- `T:Pennington.Roslyn.Workspace.SolutionWorkspaceService`
- `T:Pennington.Roslyn.Symbols.SymbolExtractionService`

**xmldocid fence syntax — verified in source**

The preprocessor (`RoslynCodeBlockPreprocessor.ParseLanguageId`) scans the
fence info string for the substrings `:xmldocid-diff`, `:xmldocid`, or
`:path`. The **base language** is everything before the colon (`csharp`,
`razor`, `text`, …); the **modifier** is `xmldocid`, `xmldocid,bodyonly`,
`xmldocid-diff`, `xmldocid-diff,bodyonly`, or `path`. The **fence body** is
one XmlDocId per line (two for `-diff`), *not* a `key="value"` attribute
on the fence. So the canonical form is:

```` text
```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```
````

Append `,bodyonly` to strip declarations and render method bodies only.
Multiple XmlDocIds on separate lines in one fence are concatenated.

**Raw-file fence candidates**

- `examples/BeyondRoslynExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BeyondRoslynExample/BeyondRoslynExample.slnx`
- `examples/BeyondRoslynExample/Content/index.md`
- `examples/BeyondRoslynExample/Content/api-pulls.md`
- `examples/BeyondRoslynExample/Sample/Calculator.cs`
- `examples/BeyondRoslynExample/Sample/Greeter.cs`

## `examples/BeyondCustomRazorComponentExample`

Backs tutorial §1.4.30 `/tutorials/beyond-basics/custom-razor-component`.
Same DocSite host shape as the other beyond-basics tutorials (app #10, #11)
with one extra DI line — `services.AddMdazorComponent<PricingCard>()` — that
registers a Razor component authored in the example's own assembly with
Mdazor's component registry. The markdown page at `Content/pricing.md`
consumes the component twice (`<PricingCard Tier="Basic" ... />` and
`<PricingCard Tier="Pro" ... Highlighted="true" />`) so the two visual
variants are both exercised.

The csproj uses `Microsoft.NET.Sdk.Web` (the same SDK every other DocSite
example uses) — the Web SDK pulls in Razor tooling transitively, so no
SDK swap is required to author `.razor` files in an example app. A
top-level `_Imports.razor` pulls in `Microsoft.AspNetCore.Components` +
`Microsoft.AspNetCore.Components.Web` so the component file can use
`[Parameter]` without per-file `@using` directives.

**Files**

- `examples/BeyondCustomRazorComponentExample/BeyondCustomRazorComponentExample.csproj` — DocSite-only reference (Mdazor comes transitively)
- `examples/BeyondCustomRazorComponentExample/Program.cs` — canonical final state
- `examples/BeyondCustomRazorComponentExample/_Imports.razor` — component + component.web usings
- `examples/BeyondCustomRazorComponentExample/Components/PricingCard.razor` — the custom component
- `examples/BeyondCustomRazorComponentExample/Content/index.md` — landing page
- `examples/BeyondCustomRazorComponentExample/Content/pricing.md` — consumes `<PricingCard />` twice
- `examples/BeyondCustomRazorComponentExample/Stage1_ComponentAuthored.cs` — stage 1: component exists, Mdazor unaware
- `examples/BeyondCustomRazorComponentExample/Stage2_RegisterMdazorComponent.cs` — stage 2: `AddMdazorComponent<T>()` wired

**Symbols (component)**

- `T:BeyondCustomRazorComponentExample.Components.PricingCard`
- `P:BeyondCustomRazorComponentExample.Components.PricingCard.Tier`
- `P:BeyondCustomRazorComponentExample.Components.PricingCard.Price`
- `P:BeyondCustomRazorComponentExample.Components.PricingCard.Features`
- `P:BeyondCustomRazorComponentExample.Components.PricingCard.Highlighted`

**Symbols (stage files — host)**

- `T:BeyondCustomRazorComponentExample.Stage1`
- `M:BeyondCustomRazorComponentExample.Stage1.Source` (short)
- `T:BeyondCustomRazorComponentExample.Stage2`
- `M:BeyondCustomRazorComponentExample.Stage2.Run(System.String[])` (short)

**Production symbols the tutorial leans on** (Mdazor NuGet package):

- `M:Mdazor.ServiceCollectionExtensions.AddMdazorComponent``1(Microsoft.Extensions.DependencyInjection.IServiceCollection)` — the generic extension that registers a component type with the Mdazor registry; returns the `IServiceCollection` so it chains.

**Tag matching (verified against Mdazor README + `MdazorIntegrationTests`)**

- Component tag-name matching is **case-sensitive on the leading character
  (capital required)** and then matched by **component type name** against
  registered types. Unknown tags fall through as literal HTML with an HTML
  comment carrying the error.
- Attribute → parameter binding is **case-insensitive via reflection**
  (`Tier="Pro"` binds to `[Parameter] public string Tier`).
- Supported parameter types in markdown-driven binding are simple strings,
  numbers, and booleans — complex types are not supported from markdown
  attributes. The example sidesteps this by shipping the feature list as a
  pipe-delimited string and splitting it inside the component.
- Both **self-closing** (`<PricingCard ... />`) and **open/close**
  (`<PricingCard ...></PricingCard>`) forms are supported; the open/close
  form can carry `ChildContent` (markdown between the tags).

**Raw-file fence candidates**

- `examples/BeyondCustomRazorComponentExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BeyondCustomRazorComponentExample/_Imports.razor`
- `examples/BeyondCustomRazorComponentExample/Components/PricingCard.razor`
- `examples/BeyondCustomRazorComponentExample/Content/index.md`
- `examples/BeyondCustomRazorComponentExample/Content/pricing.md`

## `examples/DocSiteKitchenSinkExample`

Kitchen-sink DocSite host backing **eighteen how-to pages** across §2.1
(Content Authoring) and the DocSite-flavoured subset of §2.2
(Configuration). One `AddDocSite` call, one `AddMdazorComponent` call.
Configuration is pushed down into small static helper methods on
`DocSiteKitchenSinkExample.ServiceConfiguration` so individual how-tos
can `xmldocid,bodyonly` fence the exact surface they teach — areas,
locales, colour scheme, font preloads, extra styles, footer HTML.

Each how-to's target page sits in `Content/main/` (or `Content/api/`
for the secondary area) and is named after the how-to slug so authors
can `path:` fence it directly. A single custom Mdazor component —
`<FeatureCallout>` — ships under `Components/` and is consumed on
`Content/main/ui-components-in-markdown.md`.

### Host design — targeting configuration surfaces

The how-to bible calls for `xmldocid` fences that lift one configuration
surface out of a shared Program.cs. Rather than wrap each surface in a
`#region` block, this app splits configuration into small static methods
on `ServiceConfiguration` — each method is an xmldocid-addressable unit,
so a how-to can pull just the locale setup, just the font preloads, or
just the footer HTML without hand-clipping line ranges. `Program.cs`
itself stays at five lines of DI plumbing so the "minimal shape" fence
is `Program.cs` (top-level statements, no xmldocid).

### What is NOT demonstrated (by design)

- **`AddMarkdownContent<T>()` with a second front-matter type.**
  `AddDocSite` internally registers **one** markdown source using
  `DocSiteFrontMatter`; its public option surface does not expose
  `PenningtonOptions.AddMarkdownContent` or `MarkdownContentOptions.ExcludePaths`.
  The `ApiFrontMatter` record ships as a **compile-only** example of
  the capability-interface pattern (the "define your own front matter"
  half of §2.1.10), and the secondary `Content/api/` area demonstrates
  the "multiple content roots" half of §2.2.10 at the DocSite-idiomatic
  level — two `ContentArea` entries, one markdown pipeline.
- **`SearchIndexOptions.ContentSelector` / `LlmsTxtOptions.GenerateFullFile`.**
  `AddDocSite` pins `ContentSelector = "#main-content"` internally and
  exposes no override. The §2.2.20 search and §2.2.60 llms.txt how-tos
  are backed here by **front-matter exclusions** (`search: false`,
  `llms: false`) plus the verified output at `/search-index-en.json`
  and `/llms.txt`; consumers who need a different `ContentSelector`
  drop to bare `AddPennington`.
- **`MonorailCssOptions.CustomCssFrameworkSettings`.**
  `AddDocSite` wires MonorailCSS internally and exposes only
  `ColorScheme` + `ExtraStyles` via `DocSiteOptions`. The §2.2.30
  monorail-css how-to is backed by `AlgorithmicColorScheme` +
  `ExtraStyles` here.

### Files

- `examples/DocSiteKitchenSinkExample/DocSiteKitchenSinkExample.csproj`
- `examples/DocSiteKitchenSinkExample/Program.cs` — 5-line host: `AddDocSite(ServiceConfiguration.BuildDocSiteOptions)` + `AddMdazorComponent<FeatureCallout>()` + `UseDocSite()` + `RunDocSiteAsync(args)`
- `examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs` — small configuration helpers, one per surface
- `examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs` — custom front-matter record (compile-only demo)
- `examples/DocSiteKitchenSinkExample/_Imports.razor`
- `examples/DocSiteKitchenSinkExample/Components/FeatureCallout.razor` — custom Mdazor component
- `examples/DocSiteKitchenSinkExample/wwwroot/shared.png` — shared asset demo target
- `examples/DocSiteKitchenSinkExample/Content/main/index.md` — Main area landing
- `examples/DocSiteKitchenSinkExample/Content/main/front-matter.md` — backs §2.1.10
- `examples/DocSiteKitchenSinkExample/Content/main/drafts-tags-ordering.md` — backs §2.1.20
- `examples/DocSiteKitchenSinkExample/Content/main/customize-sidebar.md` — backs §2.1.30
- `examples/DocSiteKitchenSinkExample/Content/main/images-and-assets.md` — backs §2.1.40
- `examples/DocSiteKitchenSinkExample/Content/main/assets/colocated.png` — colocated asset demo target
- `examples/DocSiteKitchenSinkExample/Content/main/tabbed-code.md` — backs §2.1.50
- `examples/DocSiteKitchenSinkExample/Content/main/code-annotations.md` — backs §2.1.60
- `examples/DocSiteKitchenSinkExample/Content/main/alerts.md` — backs §2.1.70 (one of each kind)
- `examples/DocSiteKitchenSinkExample/Content/main/diagrams.md` — backs §2.1.80 (two mermaid fences)
- `examples/DocSiteKitchenSinkExample/Content/main/ui-components-in-markdown.md` — backs §2.1.90 (three FeatureCallout instances plus a Badge)
- `examples/DocSiteKitchenSinkExample/Content/main/cross-references-a.md` — backs §2.1.100 (source)
- `examples/DocSiteKitchenSinkExample/Content/main/cross-references-b.md` — backs §2.1.100 (target)
- `examples/DocSiteKitchenSinkExample/Content/main/linking.md` — backs §2.1.110
- `examples/DocSiteKitchenSinkExample/Content/main/redirect-source.md` — backs §2.1.120 (`redirectUrl:` front matter)
- `examples/DocSiteKitchenSinkExample/Content/main/hidden.md` — `search: false` fixture for §2.2.20
- `examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md` — `llms: false` fixture for §2.2.60
- `examples/DocSiteKitchenSinkExample/Content/api/index.md` — API area landing (second area for §2.2.10)
- `examples/DocSiteKitchenSinkExample/Content/api/reference-example.md` — API area reference page
- `examples/DocSiteKitchenSinkExample/Content/fr/main/index.md` — FR locale root (backs §2.2.50)
- `examples/DocSiteKitchenSinkExample/Content/fr/main/alerts.md` — FR translation of alerts.md (backs §2.2.50)

### Symbols (configuration helpers)

- `T:DocSiteKitchenSinkExample.ServiceConfiguration`
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions` — full `DocSiteOptions` record literal (top-level surface for §2.2.30 / §2.2.40 / any option-shape how-to)
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildAreas` — `ContentArea[]` literal for §2.2.10 multiple-sources (short)
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.ConfigureLocalization(Pennington.Infrastructure.LocalizationOptions)` — `LocalizationOptions` setup for §2.2.50 localization (short)
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildColorScheme` — `AlgorithmicColorScheme` for §2.2.30 monorail-css (short)
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildFontPreloads` — `FontPreload[]` literal for §2.2.40 fonts (short)
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildExtraStyles` — `@font-face` + extra CSS for §2.2.40 fonts / §2.2.30 monorail-css
- `M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildFooter` — footer HTML string (short)

### Symbols (custom types)

- `T:DocSiteKitchenSinkExample.ApiFrontMatter` — custom front-matter record for §2.1.10 (compile-only)
- `P:DocSiteKitchenSinkExample.ApiFrontMatter.Namespace`
- `P:DocSiteKitchenSinkExample.ApiFrontMatter.Stability`
- `T:DocSiteKitchenSinkExample.Components.FeatureCallout` — custom Mdazor component for §2.1.90
- `P:DocSiteKitchenSinkExample.Components.FeatureCallout.Title`
- `P:DocSiteKitchenSinkExample.Components.FeatureCallout.Kind`
- `P:DocSiteKitchenSinkExample.Components.FeatureCallout.Icon`
- `P:DocSiteKitchenSinkExample.Components.FeatureCallout.ChildContent`

### Production symbols the how-tos lean on

- `T:Pennington.DocSite.DocSiteOptions`
- `P:Pennington.DocSite.DocSiteOptions.Areas`
- `P:Pennington.DocSite.DocSiteOptions.ConfigureLocalization`
- `P:Pennington.DocSite.DocSiteOptions.ColorScheme`
- `P:Pennington.DocSite.DocSiteOptions.ExtraStyles`
- `P:Pennington.DocSite.DocSiteOptions.DisplayFontFamily`
- `P:Pennington.DocSite.DocSiteOptions.BodyFontFamily`
- `P:Pennington.DocSite.DocSiteOptions.FontPreloads`
- `P:Pennington.DocSite.DocSiteOptions.HeaderContent`
- `P:Pennington.DocSite.DocSiteOptions.FooterContent`
- `P:Pennington.DocSite.DocSiteOptions.GitHubUrl`
- `P:Pennington.DocSite.DocSiteOptions.CanonicalBaseUrl`
- `T:Pennington.DocSite.ContentArea`
- `T:Pennington.DocSite.DocSiteFrontMatter`
- `T:Pennington.Infrastructure.FontPreload`
- `T:Pennington.MonorailCss.AlgorithmicColorScheme`
- `T:Pennington.MonorailCss.NamedColorScheme`

### Raw-file fence candidates

- `examples/DocSiteKitchenSinkExample/Program.cs` (top-level statements, no xmldocid)
- `examples/DocSiteKitchenSinkExample/Components/FeatureCallout.razor`
- `examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs`
- `examples/DocSiteKitchenSinkExample/Content/main/front-matter.md`
- `examples/DocSiteKitchenSinkExample/Content/main/drafts-tags-ordering.md`
- `examples/DocSiteKitchenSinkExample/Content/main/customize-sidebar.md`
- `examples/DocSiteKitchenSinkExample/Content/main/images-and-assets.md`
- `examples/DocSiteKitchenSinkExample/Content/main/tabbed-code.md`
- `examples/DocSiteKitchenSinkExample/Content/main/code-annotations.md`
- `examples/DocSiteKitchenSinkExample/Content/main/alerts.md`
- `examples/DocSiteKitchenSinkExample/Content/main/diagrams.md`
- `examples/DocSiteKitchenSinkExample/Content/main/ui-components-in-markdown.md`
- `examples/DocSiteKitchenSinkExample/Content/main/cross-references-a.md`
- `examples/DocSiteKitchenSinkExample/Content/main/cross-references-b.md`
- `examples/DocSiteKitchenSinkExample/Content/main/linking.md`
- `examples/DocSiteKitchenSinkExample/Content/main/redirect-source.md`
- `examples/DocSiteKitchenSinkExample/Content/main/hidden.md`
- `examples/DocSiteKitchenSinkExample/Content/main/llms-hidden.md`
- `examples/DocSiteKitchenSinkExample/Content/api/index.md`
- `examples/DocSiteKitchenSinkExample/Content/api/reference-example.md`
- `examples/DocSiteKitchenSinkExample/Content/fr/main/index.md`
- `examples/DocSiteKitchenSinkExample/Content/fr/main/alerts.md`

## `examples/BlogKitchenSinkExample`

Backs the two blog-specific how-tos under §2.2 Configuration —
2.2.70 `/how-to/configuration/rss` and 2.2.90
`/how-to/configuration/blogsite-homepage` — and is the primary fixture for
the reference pages §3.1 `/reference/options/blogsite-options` and §3.8
`/reference/blogsite/social-icons`. One `AddBlogSite` call wires a
fully-populated `BlogSiteOptions` (hero, five `Project` entries, all four
built-in social icons, four `HeaderLink` entries, explicit `EnableRss` and
`EnableSitemap`, canonical base URL, author name and bio). Three dated
posts under `Content/Blog/` populate the recent-posts card, the archive,
the tag pages, and the RSS channel. Configuration lives in a small
`ServiceConfiguration` static class with one helper method per surface
(`BuildHero`, `BuildMyWork`, `BuildSocials`, `BuildMainSiteLinks`,
`BuildBlogSiteOptions`) so each how-to and reference page can fence into
one helper via `M:...,bodyonly`. `Program.cs` stays at five lines and is
itself a `:path`-addressable fence candidate.

### Files

- `examples/BlogKitchenSinkExample/BlogKitchenSinkExample.csproj` — Web SDK, references only `src/Pennington.BlogSite/`
- `examples/BlogKitchenSinkExample/Program.cs` — top-level statements; five-line host that calls `ServiceConfiguration.BuildBlogSiteOptions`
- `examples/BlogKitchenSinkExample/ServiceConfiguration.cs` — one helper method per option surface
- `examples/BlogKitchenSinkExample/Content/Blog/2024-01-15-getting-started-with-pennington.md` — full `BlogSiteFrontMatter` with series + repository + section
- `examples/BlogKitchenSinkExample/Content/Blog/2024-02-20-authoring-with-front-matter.md` — series-linked second post, no repository
- `examples/BlogKitchenSinkExample/Content/Blog/2024-03-10-wiring-the-homepage.md` — non-series standalone post

### `ServiceConfiguration` helper symbols (how-to fence targets)

- `T:BlogKitchenSinkExample.ServiceConfiguration`
- `M:BlogKitchenSinkExample.ServiceConfiguration.BuildHero` (short) — returns `HeroContent`
- `M:BlogKitchenSinkExample.ServiceConfiguration.BuildMyWork` (short) — returns `Project[]`
- `M:BlogKitchenSinkExample.ServiceConfiguration.BuildSocials` (short) — returns `SocialLink[]` wired to all four `SocialIcons.*Icon` fields
- `M:BlogKitchenSinkExample.ServiceConfiguration.BuildMainSiteLinks` (short) — returns `HeaderLink[]`
- `M:BlogKitchenSinkExample.ServiceConfiguration.BuildBlogSiteOptions` (short) — returns the populated `BlogSiteOptions`, wiring every other helper

All five helpers are parameterless static methods whose bodies are the full
option surface a how-to or reference page fences. None are called outside
`Program.cs` (which uses `BuildBlogSiteOptions` only); the other four are
entirely for documentation reach.

### Production symbols the reference pages lean on

`BlogSiteOptions` (complete surface, for §3.1 `/reference/options/blogsite-options`):

- `T:Pennington.BlogSite.BlogSiteOptions`
- `P:Pennington.BlogSite.BlogSiteOptions.SiteTitle`
- `P:Pennington.BlogSite.BlogSiteOptions.Description`
- `P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.ColorScheme`
- `P:Pennington.BlogSite.BlogSiteOptions.ContentRootPath`
- `P:Pennington.BlogSite.BlogSiteOptions.BlogContentPath`
- `P:Pennington.BlogSite.BlogSiteOptions.BlogBaseUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.TagsPageUrl`
- `P:Pennington.BlogSite.BlogSiteOptions.ExtraStyles`
- `P:Pennington.BlogSite.BlogSiteOptions.DisplayFontFamily`
- `P:Pennington.BlogSite.BlogSiteOptions.BodyFontFamily`
- `P:Pennington.BlogSite.BlogSiteOptions.AdditionalHtmlHeadContent`
- `P:Pennington.BlogSite.BlogSiteOptions.FontPreloads`
- `P:Pennington.BlogSite.BlogSiteOptions.AdditionalRoutingAssemblies`
- `P:Pennington.BlogSite.BlogSiteOptions.AuthorName`
- `P:Pennington.BlogSite.BlogSiteOptions.AuthorBio`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableRss`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap`
- `P:Pennington.BlogSite.BlogSiteOptions.HeroContent`
- `P:Pennington.BlogSite.BlogSiteOptions.MyWork`
- `P:Pennington.BlogSite.BlogSiteOptions.Socials`
- `P:Pennington.BlogSite.BlogSiteOptions.MainSiteLinks`
- `P:Pennington.BlogSite.BlogSiteOptions.SocialMediaImageUrlFactory`

Helper records on the same page (all positional records in `src/Pennington.BlogSite/BlogSiteOptions.cs`):

- `T:Pennington.BlogSite.HeroContent` — `HeroContent(string Title, string Description)`
- `T:Pennington.BlogSite.Project` — `Project(string Title, string Description, string Url)`
- `T:Pennington.BlogSite.SocialLink` — `SocialLink(RenderFragment Icon, string Url)`
- `T:Pennington.BlogSite.HeaderLink` — `HeaderLink(string Title, string Url)`

Service-extension entry points:

- `T:Pennington.BlogSite.BlogSiteServiceExtensions`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)`
- `M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`

Built-in `SocialIcons` fragments (for §3.8 `/reference/blogsite/social-icons`):

- `T:Pennington.BlogSite.Components.SocialIcons`
- `F:Pennington.BlogSite.Components.SocialIcons.GithubIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.BlueskyIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.LinkedInIcon`
- `F:Pennington.BlogSite.Components.SocialIcons.MastodonIcon`

All four are `public static readonly RenderFragment` fields on the
`SocialIcons` Razor component. Each is a 24x24 viewBox SVG that inherits
`currentColor` (`stroke="currentColor"`, `fill="none"`). The path counts,
verified against source, are: `GithubIcon` 1, `BlueskyIcon` 1,
`LinkedInIcon` 4, `MastodonIcon` 2. The field syntax is the only way to
reference them — they are `RenderFragment` values, not component tags.

`BlogSiteFrontMatter` surface the posts populate (also cited by the RSS
how-to):

- `T:Pennington.BlogSite.BlogSiteFrontMatter`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Title`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Author`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Description`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Repository`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Date`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.IsDraft`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Tags`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Series`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.RedirectUrl`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Section`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Uid`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Search`
- `P:Pennington.BlogSite.BlogSiteFrontMatter.Llms`

### Raw-file fence candidates

- `examples/BlogKitchenSinkExample/Program.cs` (top-level statements, no xmldocid)
- `examples/BlogKitchenSinkExample/Content/Blog/2024-01-15-getting-started-with-pennington.md`
- `examples/BlogKitchenSinkExample/Content/Blog/2024-02-20-authoring-with-front-matter.md`
- `examples/BlogKitchenSinkExample/Content/Blog/2024-03-10-wiring-the-homepage.md`

## `examples/ExtensibilityLabExample`

Backs all seven how-tos under §2.3 Extensibility (2.3.10 through 2.3.70).
One bare-`AddPennington` host wires six extension points raw — each lives
in its own `.cs` file named for the how-to it backs, so xmldocid targets
are predictable: one interface implementation per file, no shared
`ServiceConfiguration` helper (unlike apps #13 and #14). A seventh file,
`SiteChromeOverrides.cs`, is a compile-only static helper that returns a
populated `DocSiteOptions` — the how-to that backs
`override-docsite-components` needs a DocSite host to exercise its slot
seams, so the file is shape-accurate but never invoked by this app's
`Program.cs`.

Runtime verification (Playwright + curl on `localhost:5555`): the
release-notes index and the two per-version pages resolve;
`.pipeline-keyword`, `.pipeline-arrow`, `.pipeline-pipe`, and
`.pipeline-string` spans all present on `/pipeline-demo/`;
`<figure class="linecount">` carries `Line count: 5` on
`/line-count-demo/`; every rendered page carries
`<aside class="feedback-widget" data-extensibility-lab="feedback-widget">`;
`<a data-lowercase>` text reads `home`, `pipeline demo`,
`line-count demo` post-rewrite on `/lowercase-demo/` with the raw
`<!--LOWERCASE-SENTINEL-->` comment stripped; `<div data-spa-island="chart">`
is present on `/chart-demo/` and `/_spa-data/chart-demo.json` carries
`islands.chart` with a `figcaption>Quarterly widgets` figure.

Static build: `dotnet run -- build output` emits 16/16 pages; each
marker survives into the HTML on disk.

### Files

- `examples/ExtensibilityLabExample/ExtensibilityLabExample.csproj` — Web SDK, references `Pennington`, `Pennington.MonorailCss`, `Pennington.UI`, and `Pennington.DocSite` (last only so `SiteChromeOverrides.cs` can build)
- `examples/ExtensibilityLabExample/Program.cs` — top-level statements; bare `AddPennington` + `AddMonorailCss` + `AddSpaNavigation` + seven DI registrations, one per extension
- `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs` — backs §2.3.10
- `examples/ExtensibilityLabExample/LineCountPreprocessor.cs` — backs §2.3.20
- `examples/ExtensibilityLabExample/PipelineHighlighter.cs` — backs §2.3.30
- `examples/ExtensibilityLabExample/FeedbackWidgetProcessor.cs` — backs §2.3.40
- `examples/ExtensibilityLabExample/AnchorLowercaseRewriter.cs` — backs §2.3.50
- `examples/ExtensibilityLabExample/ChartIslandRenderer.cs` — backs §2.3.60
- `examples/ExtensibilityLabExample/SiteChromeOverrides.cs` — backs §2.3.70 (compile-only)
- `examples/ExtensibilityLabExample/Components/ChartIsland.razor` — Razor component the chart island renders
- `examples/ExtensibilityLabExample/Components/ExtraHeadFragment.razor` — head-slot fence target paired with `SiteChromeOverrides.BuildHtmlHeadContent`
- `examples/ExtensibilityLabExample/Content/index.md` — landing page, links to every demo
- `examples/ExtensibilityLabExample/Content/pipeline-demo.md` — consumes the `pipeline` fenced code block
- `examples/ExtensibilityLabExample/Content/line-count-demo.md` — consumes the `linecount` fenced code block
- `examples/ExtensibilityLabExample/Content/chart-demo.md` — embeds `data-spa-island="chart"`
- `examples/ExtensibilityLabExample/Content/lowercase-demo.md` — carries `<a data-lowercase>` anchors plus the sentinel comment
- `examples/ExtensibilityLabExample/Content/releases/v1.0.0.json`
- `examples/ExtensibilityLabExample/Content/releases/v1.1.0.json`

### Extension symbols (how-to fence targets)

2.3.10 custom-content-service:

- `T:ExtensibilityLabExample.ReleaseNotesContentService`
- `M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync`
- `M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentTocEntriesAsync`
- `M:ExtensibilityLabExample.ReleaseNotesContentService.GetCrossReferencesAsync`
- `M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCopyAsync` (short)
- `M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCreateAsync` (short)
- `T:ExtensibilityLabExample.ReleaseEntry`
- Contract: `T:Pennington.Content.IContentService`
- Related production types: `T:Pennington.Pipeline.DiscoveredItem`, `T:Pennington.Content.ContentToCopy`, `T:Pennington.Content.ContentToCreate`, `T:Pennington.Content.ContentTocItem`, `T:Pennington.Pipeline.CrossReference`, `M:Pennington.Routing.ContentRouteFactory.FromUrl(Pennington.Routing.UrlPath,System.String)`, `M:Pennington.Routing.ContentRouteFactory.FromCustom(Pennington.Routing.UrlPath,Pennington.Routing.FilePath,System.String)`

2.3.20 code-block-preprocessor:

- `T:ExtensibilityLabExample.LineCountPreprocessor`
- `M:ExtensibilityLabExample.LineCountPreprocessor.TryProcess(System.String,System.String)` (short)
- `P:ExtensibilityLabExample.LineCountPreprocessor.Priority` (short)
- Contract: `T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor`
- Related production types: `T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult`

2.3.30 custom-highlighter:

- `T:ExtensibilityLabExample.PipelineHighlighter`
- `M:ExtensibilityLabExample.PipelineHighlighter.Highlight(System.String,System.String)`
- `P:ExtensibilityLabExample.PipelineHighlighter.SupportedLanguages` (short)
- `P:ExtensibilityLabExample.PipelineHighlighter.Priority` (short)
- Contract: `T:Pennington.Highlighting.ICodeHighlighter`
- Registration shape: `M:Pennington.Infrastructure.HighlightingOptions.AddHighlighter(Pennington.Highlighting.ICodeHighlighter)`

2.3.40 response-processor:

- `T:ExtensibilityLabExample.FeedbackWidgetProcessor`
- `M:ExtensibilityLabExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)` (short)
- `M:ExtensibilityLabExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)`
- `P:ExtensibilityLabExample.FeedbackWidgetProcessor.Order` (short)
- Contract: `T:Pennington.Infrastructure.IResponseProcessor`

2.3.50 html-rewriter:

- `T:ExtensibilityLabExample.AnchorLowercaseRewriter`
- `M:ExtensibilityLabExample.AnchorLowercaseRewriter.ShouldApply(Microsoft.AspNetCore.Http.HttpContext)` (short)
- `M:ExtensibilityLabExample.AnchorLowercaseRewriter.PreParseAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)` (short)
- `M:ExtensibilityLabExample.AnchorLowercaseRewriter.ApplyAsync(AngleSharp.Dom.IDocument,Microsoft.AspNetCore.Http.HttpContext)` (short)
- `P:ExtensibilityLabExample.AnchorLowercaseRewriter.Order` (short)
- Contract: `T:Pennington.Infrastructure.IHtmlResponseRewriter`

2.3.60 island-renderer:

- `T:ExtensibilityLabExample.ChartIslandRenderer`
- `M:ExtensibilityLabExample.ChartIslandRenderer.BuildParametersAsync(Pennington.Routing.ContentRoute)`
- `P:ExtensibilityLabExample.ChartIslandRenderer.IslandName` (short)
- `T:ExtensibilityLabExample.Components.ChartIsland`
- Contract / base: `T:Pennington.Islands.IIslandRenderer`, `T:Pennington.Islands.RazorIslandRenderer\`1`
- Registration: `options.Islands.Register<ChartIslandRenderer>("chart")` via `P:Pennington.Infrastructure.PenningtonOptions.Islands` and `M:Pennington.Infrastructure.IslandsOptions.Register\`\`1(System.String)`

2.3.70 override-docsite-components (compile-only helper):

- `T:ExtensibilityLabExample.SiteChromeOverrides`
- `M:ExtensibilityLabExample.SiteChromeOverrides.BuildDocSiteOptions` (short)
- `M:ExtensibilityLabExample.SiteChromeOverrides.BuildHtmlHeadContent` (short)
- `M:ExtensibilityLabExample.SiteChromeOverrides.BuildExtraStyles` (short)
- `M:ExtensibilityLabExample.SiteChromeOverrides.BuildAdditionalRoutingAssemblies` (short)
- `T:ExtensibilityLabExample.Components.ExtraHeadFragment`
- Production surfaces the helper leans on: `T:Pennington.DocSite.DocSiteOptions`, `P:Pennington.DocSite.DocSiteOptions.AdditionalHtmlHeadContent`, `P:Pennington.DocSite.DocSiteOptions.ExtraStyles`, `P:Pennington.DocSite.DocSiteOptions.HeaderContent`, `P:Pennington.DocSite.DocSiteOptions.FooterContent`, `P:Pennington.DocSite.DocSiteOptions.AdditionalRoutingAssemblies`

### Raw-file fence candidates

- `examples/ExtensibilityLabExample/Program.cs` (top-level statements, no xmldocid)
- `examples/ExtensibilityLabExample/Content/pipeline-demo.md`
- `examples/ExtensibilityLabExample/Content/line-count-demo.md`
- `examples/ExtensibilityLabExample/Content/chart-demo.md`
- `examples/ExtensibilityLabExample/Content/lowercase-demo.md`
- `examples/ExtensibilityLabExample/Content/releases/v1.0.0.json`
- `examples/ExtensibilityLabExample/Content/releases/v1.1.0.json`

## `examples/SubPathDeployableExample`

Backs how-to §2.4 Publishing & Deployment — five recipes (`/how-to/deployment/static-build`,
`/how-to/deployment/github-pages`, `/how-to/deployment/adapt-for-other-hosts`,
`/how-to/deployment/self-host`, `/how-to/deployment/base-url`). Deliberately
tiny `AddDocSite` host: one area (`Guides`), one home page, one nested page so
sub-path rewriting is observable on a deep link. The teaching artefacts are
the **sibling fixture files** of the csproj — workflow YAML and host configs
that the how-to pages embed via `path:` fences.

### Files

- `examples/SubPathDeployableExample/SubPathDeployableExample.csproj` — Web SDK, references only `Pennington.DocSite`
- `examples/SubPathDeployableExample/Program.cs` — top-level statements; minimal `AddDocSite(ServiceConfiguration.BuildDocSiteOptions)` + `UseDocSite()` + `RunDocSiteAsync(args)`
- `examples/SubPathDeployableExample/ServiceConfiguration.cs` — `BuildDocSiteOptions` factory (named so the §2.4 how-tos can fence it via xmldocid)
- `examples/SubPathDeployableExample/Content/index.md` — landing page, links to `/guides/first-page/` so base-url rewriting is observable on a content-authored anchor
- `examples/SubPathDeployableExample/Content/guides/first-page.md` — nested route under the `Guides` area
- `examples/SubPathDeployableExample/.github/workflows/deploy.yml` — GitHub Pages workflow (setup-dotnet@v4, upload-pages-artifact@v3, deploy-pages@v4, `.nojekyll`, base URL derived from `${{ github.event.repository.name }}`)
- `examples/SubPathDeployableExample/staticwebapp.config.json` — Azure Static Web Apps routes, `navigationFallback`, MIME map, cache headers
- `examples/SubPathDeployableExample/netlify.toml` — Netlify build command, publish dir, environment, headers, 404 fallback
- `examples/SubPathDeployableExample/nginx.conf` — self-host server block with `try_files $uri $uri/ /404.html`, immutable cache for `_content/`, MIME for sitemap/llms
- `examples/SubPathDeployableExample/web.config` — IIS staticContent + httpErrors + rewrite + custom headers

### Helper symbols

- `T:SubPathDeployableExample.ServiceConfiguration`
- `M:SubPathDeployableExample.ServiceConfiguration.BuildDocSiteOptions` (short)
- `T:SubPathDeployableExample.BuildHost` — addressable wrappers for `RunOrBuildAsync` / `BuildReport` so §2.4.20 can fence small copy-pasteable snippets
- `M:SubPathDeployableExample.BuildHost.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` — primary fence target for the dev-vs-build switch on the GitHub Pages how-to
- `M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)` — primary fence target for the "inspect the BuildReport in CI" paragraph on the GitHub Pages how-to

Helpers are compile-only on the call path — `Program.cs` invokes
`ServiceConfiguration.BuildDocSiteOptions` via the `AddDocSite` factory and
calls the extension `app.RunDocSiteAsync(args)` rather than `BuildHost`
itself, so every helper body is also reachable via `,bodyonly` for how-to
fences that need the logic inline.

### Production surfaces the §2.4 how-tos point at

- `T:Pennington.Generation.OutputOptions` — base-URL + output-directory record materialized from the CLI
- `P:Pennington.Generation.OutputOptions.BaseUrl`
- `P:Pennington.Generation.OutputOptions.OutputDirectory`
- `P:Pennington.Generation.OutputOptions.CleanOutput`
- `M:Pennington.Generation.OutputOptions.FromArgs(System.String[])` — single source of truth for how `build [baseUrl] [outputDirectory]` is parsed
- `T:Pennington.Generation.BuildReport` — what the static build prints to stdout
- `P:Pennington.Generation.BuildReport.GeneratedPages`
- `P:Pennington.Generation.BuildReport.FailedPages`
- `P:Pennington.Generation.BuildReport.BrokenLinks`
- `P:Pennington.Generation.BuildReport.HasErrors`
- `T:Pennington.Generation.OutputGenerationService` — the HTTP crawler that drives the static build
- `T:Pennington.Infrastructure.BaseUrlHtmlRewriter` — the rewriter that prefixes `href`/`src`/`action` and stamps `data-base-url` on `<body>`
- `M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` — the `dotnet run` vs `dotnet run -- build …` switch

### Raw-file fence candidates

- `examples/SubPathDeployableExample/Program.cs` (top-level statements, no xmldocid) — primary fence target for §2.4.10
- `examples/SubPathDeployableExample/Content/index.md`
- `examples/SubPathDeployableExample/Content/guides/first-page.md`
- `examples/SubPathDeployableExample/.github/workflows/deploy.yml` — primary fence target for §2.4.20
- `examples/SubPathDeployableExample/staticwebapp.config.json` — primary fence target for §2.4.30 (Azure)
- `examples/SubPathDeployableExample/netlify.toml` — primary fence target for §2.4.30 (Netlify)
- `examples/SubPathDeployableExample/nginx.conf` — primary fence target for §2.4.40 (Nginx)
- `examples/SubPathDeployableExample/web.config` — primary fence target for §2.4.40 (IIS)
