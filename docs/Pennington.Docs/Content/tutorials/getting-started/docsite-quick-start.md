---
title: "DocSite Quick Start"
description: "Stand up a polished documentation site in under 20 lines with the Pennington.DocSite package"
uid: "penn.tutorials.docsite-quick-start"
order: 20
---

## Beat 1: Create the project and install Pennington.DocSite

The reader scaffolds an empty ASP.NET project and adds a single NuGet package. The goal is to show that DocSite is a one-package solution that bundles Pennington core, MonorailCSS, SPA navigation, and Razor components.

### What to show
- Terminal commands: `dotnet new web -n TempoDocumentation`, then `dotnet add package Pennington.DocSite`
- Emphasize the contrast with the "Building a Site from Scratch" tutorial: one package instead of two, no layout component to build, no manual MonorailCSS registration
- Show the `.csproj` with the single PackageReference

### Key points
- `Pennington.DocSite` depends on `Pennington`, `Pennington.MonorailCss`, and `Pennington.UI` transitively -- the reader does not need to reference them individually
- DocSite provides a complete documentation site layout: sidebar navigation, top header with search and dark mode, outline navigation ("On This Page"), breadcrumbs, prev/next links, SPA transitions, and llms.txt generation
- This tutorial produces a polished site in under 20 lines of C# -- the point is speed and polish out of the box

## Beat 2: Write Program.cs with the three-call setup

The reader writes a minimal Program.cs using `AddDocSite`, `UseDocSite`, and `RunDocSiteAsync`. The goal is to show the entire server configuration in ~15 lines and introduce DocSiteOptions.

### What to show
- Complete Program.cs (~15 lines) using:
  - `M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})` -- takes a factory function returning `DocSiteOptions`
  - `M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)` -- configures the full middleware pipeline
  - `M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` -- delegates to `RunOrBuildAsync`
- The `DocSiteOptions` record configured with:
  - `P:Pennington.DocSite.DocSiteOptions.SiteTitle` -- `"Tempo"` (required)
  - `P:Pennington.DocSite.DocSiteOptions.Description` -- `"Task scheduling for .NET"` (required)
  - `P:Pennington.DocSite.DocSiteOptions.GitHubUrl` -- a URL to a fictional repo
  - `P:Pennington.DocSite.DocSiteOptions.ColorScheme` -- a `NamedColorScheme` with Emerald primary (explained in Beat 5)
  - `P:Pennington.DocSite.DocSiteOptions.HeaderIcon` -- a small inline SVG clock icon
- Code reference: `T:Pennington.DocSite.DocSiteOptions`

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
- Code reference for the front matter type: `T:Pennington.DocSite.DocSiteFrontMatter`

### Key points
- `DocSiteFrontMatter` implements these capability interfaces: `T:Pennington.FrontMatter.IFrontMatter`, `T:Pennington.FrontMatter.IDraftable`, `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.ISectionable`, `T:Pennington.FrontMatter.ICrossReferenceable`, `T:Pennington.FrontMatter.IOrderable`, `T:Pennington.FrontMatter.IDescribable`, `T:Pennington.FrontMatter.IRedirectable`
- Compared to the core `T:Pennington.FrontMatter.DocFrontMatter`, `DocSiteFrontMatter` adds `P:Pennington.DocSite.DocSiteFrontMatter.RedirectUrl` for implementing page redirects
- The `uid` field (`P:Pennington.DocSite.DocSiteFrontMatter.Uid`) enables cross-referencing with `xref:` links -- Pennington's `XrefResolver` resolves them to actual URLs during rendering
- `[!NOTE]` and `[!WARNING]` are GitHub-flavored markdown alerts that Pennington renders with styled callout boxes
- Tabbed code blocks use Pennington's markdown tab extension for switching between code variants
- Each page should have 2-3 headings (H2/H3) so the outline navigation has content to track

## Beat 4: Run the site and explore the features

The reader starts the dev server and walks through every feature DocSite provides automatically. The goal is the "wow moment" -- all these features came from ~15 lines of C# and 4 markdown files.

### What to show
- Terminal command: `dotnet watch`
- Walk through each feature in the browser:
  1. **Sidebar navigation** -- pages organized by order, current page highlighted (rendered by `TableOfContentsNavigation` from `:path src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor`)
  2. **Search** -- press Ctrl+K (or Cmd+K) to open the search modal, type a query, see results from `search-index.json`
  3. **Outline navigation** -- scroll the page content, watch the "On This Page" sidebar track the visible heading (rendered by `OutlineNavigation` from `:path src/Pennington.UI/Components/Navigation/OutlineNavigation.razor`)
  4. **Breadcrumbs and prev/next links** -- navigate to a subpage, see breadcrumbs at the top and prev/next navigation at the bottom (powered by `T:Pennington.Navigation.NavigationInfo`)
  5. **Dark mode toggle** -- click the sun/moon icon in the header
  6. **SPA navigation** -- click between pages, note there is no full page reload (content area swaps via JavaScript, the sidebar and header remain stable)
  7. **Cross-references** -- on the API Reference page, click the `xref:tempo.configuration` link and verify it navigates to the Configuration page
  8. **GitHub link** -- the header shows a GitHub icon linking to the repository

### Key points
- All of these features are provided by DocSite's built-in layout component: `:path src/Pennington.DocSite/Components/Layout/MainLayout.razor`
- The content is resolved by `T:Pennington.DocSite.Services.ContentResolver` which handles URL matching, rendering, and navigation info computation
- The layout injects `T:Pennington.Navigation.NavigationBuilder` and calls `BuildTree()` to produce the sidebar tree from content TOC items
- Search is powered by a `search-index.json` endpoint auto-mapped by `UsePennington` -- the client-side JavaScript (from Pennington.UI) fetches and queries it
- SPA navigation is registered via `AddSpaNavigation` inside `AddDocSite` -- it intercepts link clicks and fetches page content via AJAX instead of full navigation
- llms.txt is available at `/llms.txt` for LLM consumption

## Beat 5: Customize the color scheme

The reader changes the site's color scheme and adds a header icon to see live updates. The goal is to introduce the MonorailCSS color system and the most common visual customization options.

### What to show
- Modify `DocSiteOptions.ColorScheme` to use a `NamedColorScheme`:
  - `T:Pennington.MonorailCss.NamedColorScheme` with all five `required` properties:
    - `P:Pennington.MonorailCss.NamedColorScheme.PrimaryColorName` -- set to `"Emerald"` (from `ColorNames.Emerald` in the `MonorailCss.Theme` namespace)
    - `P:Pennington.MonorailCss.NamedColorScheme.AccentColorName` -- set to `"Teal"`
    - `P:Pennington.MonorailCss.NamedColorScheme.TertiaryOneColorName` -- set to `"Cyan"` (used for code syntax: strings, numbers)
    - `P:Pennington.MonorailCss.NamedColorScheme.TertiaryTwoColorName` -- set to `"Pink"` (used for code syntax: variables, attributes)
    - `P:Pennington.MonorailCss.NamedColorScheme.BaseColorName` -- set to `"Slate"` (backgrounds, text, borders)
- Show the `P:Pennington.DocSite.DocSiteOptions.HeaderIcon` property -- inline SVG markup that renders next to the site title
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
- Mention content areas (`P:Pennington.DocSite.DocSiteOptions.Areas` using `T:Pennington.DocSite.ContentArea`) as a way to organize larger documentation sites into tabbed sections -- each area maps to a top-level directory and gets its own TOC

### Key points
- DocSite is the recommended starting point for documentation sites -- it handles layout, navigation, and styling so the reader can focus on content
- For blogs, point to the "BlogSite Quick Start" tutorial
- For full control over layout and behavior, the "Building a Site from Scratch" tutorial shows how to use Pennington core directly
