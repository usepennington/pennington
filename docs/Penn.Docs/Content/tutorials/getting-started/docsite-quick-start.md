---
title: "DocSite Quick Start"
description: "Stand up a polished documentation site in under 20 lines with the Penn.DocSite package"
uid: "penn.tutorials.docsite-quick-start"
order: 20
---

## Beat 1: Create the project and install Penn.DocSite

The reader scaffolds an empty ASP.NET project and adds a single NuGet package. The goal is to show that DocSite is a one-package solution that bundles Penn core, MonorailCSS, SPA navigation, and Razor components.

### What to show
- Terminal commands: `dotnet new web -n TempoDocumentation`, then `dotnet add package Penn.DocSite`
- Emphasize the contrast with the "Building a Site from Scratch" tutorial: one package instead of two, no layout component to build, no manual MonorailCSS registration
- Show the `.csproj` with the single PackageReference

### Key points
- `Penn.DocSite` depends on `Penn`, `Penn.MonorailCss`, and `Penn.UI` transitively -- the reader does not need to reference them individually
- DocSite provides a complete documentation site layout: sidebar navigation, top header with search and dark mode, outline navigation ("On This Page"), breadcrumbs, prev/next links, SPA transitions, and llms.txt generation
- This tutorial produces a polished site in under 20 lines of C# -- the point is speed and polish out of the box

## Beat 2: Write Program.cs with the three-call setup

The reader writes a minimal Program.cs using `AddDocSite`, `UseDocSite`, and `RunDocSiteAsync`. The goal is to show the entire server configuration in ~15 lines and introduce DocSiteOptions.

### What to show
- Complete Program.cs (~15 lines) using:
  - `M:Penn.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Penn.DocSite.DocSiteOptions})` -- takes a factory function returning `DocSiteOptions`
  - `M:Penn.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)` -- configures the full middleware pipeline
  - `M:Penn.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` -- delegates to `RunOrBuildAsync`
- The `DocSiteOptions` record configured with:
  - `P:Penn.DocSite.DocSiteOptions.SiteTitle` -- `"Tempo"` (required)
  - `P:Penn.DocSite.DocSiteOptions.Description` -- `"Task scheduling for .NET"` (required)
  - `P:Penn.DocSite.DocSiteOptions.GitHubUrl` -- a URL to a fictional repo
  - `P:Penn.DocSite.DocSiteOptions.ColorScheme` -- a `NamedColorScheme` with Emerald primary (explained in Beat 5)
  - `P:Penn.DocSite.DocSiteOptions.HeaderIcon` -- a small inline SVG clock icon
- Code reference: `T:Penn.DocSite.DocSiteOptions`

### Key points
- The `SiteTitle` and `Description` properties are `required` -- the compiler enforces them

## Beat 3: Create content files with DocSiteFrontMatter

The reader creates four markdown files to populate the site. The goal is to introduce `DocSiteFrontMatter` fields and demonstrate markdown features (alerts, tabbed code blocks, cross-references, LinkCard).

### What to show
- Create `Content/index.md` with:
  - Front matter: `title: "Tempo Documentation"`, `description: "Get started with Tempo task scheduling"`
  - Body: brief intro paragraph and a grid of LinkCard components pointing to the other 3 pages
- Create `Content/getting-started.md` with:
  - Front matter: `title: "Getting Started"`, `order: 10`, `description: "Install Tempo and schedule your first task"`
  - Body: NuGet install command in a fenced code block, a minimal C# example, a `[!NOTE]` alert about minimum .NET version
- Create `Content/configuration.md` with:
  - Front matter: `title: "Configuration"`, `order: 20`, `description: "Configure retry policies, concurrency, and persistence"`, `uid: "tempo.configuration"`
  - Body: tabbed code block (JSON vs C# config), a `[!WARNING]` alert about thread safety
- Create `Content/api-reference.md` with:
  - Front matter: `title: "API Reference"`, `order: 30`, `description: "Core types and extension methods"`
  - Body: table of types, code block with method signature, cross-reference link using `xref:tempo.configuration`
- Code reference for the front matter type: `T:Penn.DocSite.DocSiteFrontMatter`

### Key points
- `DocSiteFrontMatter` implements these capability interfaces: `T:Penn.FrontMatter.IFrontMatter`, `T:Penn.FrontMatter.IDraftable`, `T:Penn.FrontMatter.ITaggable`, `T:Penn.FrontMatter.ISectionable`, `T:Penn.FrontMatter.ICrossReferenceable`, `T:Penn.FrontMatter.IOrderable`, `T:Penn.FrontMatter.IDescribable`, `T:Penn.FrontMatter.IRedirectable`
- Compared to the core `T:Penn.FrontMatter.DocFrontMatter`, `DocSiteFrontMatter` adds `P:Penn.DocSite.DocSiteFrontMatter.RedirectUrl` for implementing page redirects
- The `uid` field (`P:Penn.DocSite.DocSiteFrontMatter.Uid`) enables cross-referencing with `xref:` links -- Penn's `XrefResolver` resolves them to actual URLs during rendering
- `[!NOTE]` and `[!WARNING]` are GitHub-flavored markdown alerts that Penn renders with styled callout boxes
- Tabbed code blocks use Penn's markdown tab extension for switching between code variants
- Each page should have 2-3 headings (H2/H3) so the outline navigation has content to track

## Beat 4: Run the site and explore the features

The reader starts the dev server and walks through every feature DocSite provides automatically. The goal is the "wow moment" -- all these features came from ~15 lines of C# and 4 markdown files.

### What to show
- Terminal command: `dotnet watch`
- Walk through each feature in the browser:
  1. **Sidebar navigation** -- pages organized by order, current page highlighted (rendered by `TableOfContentsNavigation` from `:path src/Penn.UI/Components/Navigation/TableOfContentsNavigation.razor`)
  2. **Search** -- press Ctrl+K (or Cmd+K) to open the search modal, type a query, see results from `search-index.json`
  3. **Outline navigation** -- scroll the page content, watch the "On This Page" sidebar track the visible heading (rendered by `OutlineNavigation` from `:path src/Penn.UI/Components/Navigation/OutlineNavigation.razor`)
  4. **Breadcrumbs and prev/next links** -- navigate to a subpage, see breadcrumbs at the top and prev/next navigation at the bottom (powered by `T:Penn.Navigation.NavigationInfo`)
  5. **Dark mode toggle** -- click the sun/moon icon in the header
  6. **SPA navigation** -- click between pages, note there is no full page reload (content area swaps via JavaScript, the sidebar and header remain stable)
  7. **Cross-references** -- on the API Reference page, click the `xref:tempo.configuration` link and verify it navigates to the Configuration page
  8. **GitHub link** -- the header shows a GitHub icon linking to the repository

### Key points
- All of these features are provided by DocSite's built-in layout component: `:path src/Penn.DocSite/Components/Layout/MainLayout.razor`
- The content is resolved by `T:Penn.DocSite.Services.ContentResolver` which handles URL matching, rendering, and navigation info computation
- The layout injects `T:Penn.Navigation.NavigationBuilder` and calls `BuildTree()` to produce the sidebar tree from content TOC items
- Search is powered by a `search-index.json` endpoint auto-mapped by `UsePenn` -- the client-side JavaScript (from Penn.UI) fetches and queries it
- SPA navigation is registered via `AddSpaNavigation` inside `AddDocSite` -- it intercepts link clicks and fetches page content via AJAX instead of full navigation
- llms.txt is available at `/llms.txt` for LLM consumption

## Beat 5: Customize the color scheme

The reader changes the site's color scheme and adds a header icon to see live updates. The goal is to introduce the MonorailCSS color system and the most common visual customization options.

### What to show
- Modify `DocSiteOptions.ColorScheme` to use a `NamedColorScheme`:
  - `T:Penn.MonorailCss.NamedColorScheme` with all five `required` properties:
    - `P:Penn.MonorailCss.NamedColorScheme.PrimaryColorName` -- set to `"Emerald"` (from `ColorNames.Emerald` in the `MonorailCss.Theme` namespace)
    - `P:Penn.MonorailCss.NamedColorScheme.AccentColorName` -- set to `"Teal"`
    - `P:Penn.MonorailCss.NamedColorScheme.TertiaryOneColorName` -- set to `"Cyan"` (used for code syntax: strings, numbers)
    - `P:Penn.MonorailCss.NamedColorScheme.TertiaryTwoColorName` -- set to `"Pink"` (used for code syntax: variables, attributes)
    - `P:Penn.MonorailCss.NamedColorScheme.BaseColorName` -- set to `"Slate"` (backgrounds, text, borders)
- Show the `P:Penn.DocSite.DocSiteOptions.HeaderIcon` property -- inline SVG markup that renders next to the site title
- After saving, the site updates live with the new colors

### Key points
- MonorailCSS maps five semantic color roles (`primary`, `accent`, `tertiary-one`, `tertiary-two`, and `base`) that drive the entire site palette
- For exhaustive coverage of all visual customization options (fonts, extra styles, social images, and more), see the Configuring DocSite how-to.

## Beat 6: What's next

The reader has a working DocSite and is pointed to further resources. The goal is to connect this tutorial to the rest of the documentation.

### What to show
- Summary: in ~15 lines of C# and 4 markdown files, the reader has a documentation site with sidebar nav, search, outline tracking, breadcrumbs, prev/next links, dark mode, SPA navigation, and llms.txt
- Link to the "Configuring DocSite" how-to guide for exhaustive coverage of all `DocSiteOptions` properties
- Link to the "Building a Site from Scratch" tutorial to understand what DocSite automates under the hood
- Link to "Deploying to GitHub Pages" to put the site live
- Mention content areas (`P:Penn.DocSite.DocSiteOptions.Areas` using `T:Penn.DocSite.ContentArea`) as a way to organize larger documentation sites into tabbed sections -- each area maps to a top-level directory and gets its own TOC

### Key points
- DocSite is the recommended starting point for documentation sites -- it handles layout, navigation, and styling so the reader can focus on content
- For blogs, point to the "BlogSite Quick Start" tutorial
- For full control over layout and behavior, the "Building a Site from Scratch" tutorial shows how to use Penn core directly
