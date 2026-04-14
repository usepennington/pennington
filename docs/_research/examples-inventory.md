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
section toward the top. The `section:` front-matter key is carried through
to `ContentTocItem.Section` and surfaced on `NavigationInfo.SectionName`
for prev/next breadcrumbs, but **does not** drive sidebar grouping — the
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
