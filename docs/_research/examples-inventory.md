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
