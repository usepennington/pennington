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
