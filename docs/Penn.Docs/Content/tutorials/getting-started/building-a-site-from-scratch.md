---
title: "Building a Site from Scratch"
description: "Create a working content site using Pennington core — no DocSite, no BlogSite, just the engine"
uid: "penn.tutorials.building-a-site-from-scratch"
order: 10
---

## Beat 1: Create the project and install packages

The reader scaffolds an empty ASP.NET project and adds the two NuGet packages required for a Pennington site without DocSite or BlogSite. The goal is to show that Pennington is a library you add to a standard ASP.NET app, not a separate framework.

### What to show
- Terminal commands: `dotnet new web -n NorthwindHandbook`, then `dotnet add package Pennington` and `dotnet add package Pennington.MonorailCss`
- Brief explanation that `Pennington` provides the content engine (markdown parsing, navigation, static generation) and `Pennington.MonorailCss` provides utility-first CSS styling
- Show the resulting `.csproj` file with the two PackageReference entries

### Key points
- Pennington targets .NET 11 -- the `dotnet new web` template must use net11.0
- No DocSite or BlogSite package is needed; those are convenience layers built on top of these two packages
- The reader will wire everything by hand in this tutorial to understand what the convenience packages automate

## Beat 2: Wire up Program.cs with AddPennington and AddMonorailCss

The reader writes the minimal Program.cs that registers Pennington services, configures a single markdown content source with `DocFrontMatter`, adds MonorailCSS, and ends with `RunOrBuildAsync`. The goal is to introduce the three core extension methods and the options pattern.

### What to show
- Complete Program.cs (~25 lines) using these APIs:
  - `M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})`
  - `M:Pennington.Infrastructure.PenningtonExtensions.UsePennington(Microsoft.AspNetCore.Builder.WebApplication)`
  - `M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`
- Inside the `AddPennington` lambda, show configuration of:
  - `P:Pennington.Infrastructure.PenningtonOptions.SiteTitle` set to `"Northwind Engineering Handbook"`
  - `P:Pennington.Infrastructure.PenningtonOptions.SiteDescription` set to `"How we build software at Northwind"`
  - `M:Pennington.Infrastructure.PenningtonOptions.AddMarkdownContent``1(System.Action{Pennington.Infrastructure.MarkdownContentOptions})` called with `DocFrontMatter` as the type parameter
- Inside the `AddMarkdownContent<DocFrontMatter>` lambda, show:
  - `P:Pennington.Infrastructure.MarkdownContentOptions.ContentPath` set to `"Content"`
  - `P:Pennington.Infrastructure.MarkdownContentOptions.BasePageUrl` set to `"/"`
- After `AddPennington`, show `M:Pennington.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Pennington.MonorailCss.MonorailCssOptions})`
- In the middleware pipeline, show `app.UseStaticFiles()`, `M:Pennington.MonorailCss.MonorailServiceExtensions.UseMonorailCss(Microsoft.AspNetCore.Builder.WebApplication,System.String)`, `app.UsePennington()`, and `app.RunOrBuildAsync(args)`
- Code reference for DocFrontMatter: `T:Pennington.FrontMatter.DocFrontMatter`

### Key points
- `AddPennington` registers all core services: markdown parsing, highlighting, navigation, output generation, response processing
- `AddMarkdownContent<TFrontMatter>` tells Pennington where to find content files and what front matter shape to expect
- `DocFrontMatter` is the built-in front matter type supporting `Title`, `Description`, `IsDraft`, `Tags`, `Section`, `Uid`, and `Order`
- `UsePennington` configures the middleware pipeline: static files from content directories, response processing, live reload, search index and sitemap endpoints
- `RunOrBuildAsync` checks for a `build` CLI argument -- if present it generates static HTML, otherwise it starts the dev server
- `UseMonorailCss` maps the `/styles.css` endpoint that generates utility CSS on the fly

## Beat 3: Create the first markdown page

The reader creates `Content/index.md` with YAML front matter and runs the site for the first time. The goal is to see content rendered in the browser and understand the front matter fields.

### What to show
- Create the `Content/` directory
- Create `Content/index.md` with YAML front matter: `title: "Northwind Engineering Handbook"`, `description: "How we build software at Northwind"`, and 2-3 sentences of body content welcoming the reader
- Terminal command: `dotnet watch`
- Screenshot or description of the page rendering at `http://localhost:5000/` -- raw HTML content with MonorailCSS styling but no layout yet
- Explain the front matter fields by referencing `T:Pennington.FrontMatter.DocFrontMatter`:
  - `P:Pennington.FrontMatter.DocFrontMatter.Title` -- required, appears in navigation and page headers
  - `P:Pennington.FrontMatter.DocFrontMatter.Description` -- optional, used for meta tags and search
  - `P:Pennington.FrontMatter.DocFrontMatter.Order` -- controls sort position in navigation (defaults to `int.MaxValue`)

### Key points
- `index.md` at the root of a content directory maps to the directory's base URL (in this case `/`)
- Front matter uses standard YAML between `---` fences
- Pennington's `FrontMatterParser` (`T:Pennington.FrontMatter.FrontMatterParser`) deserializes the YAML into the `DocFrontMatter` record using YamlDotNet
- The `IFrontMatter` interface (`T:Pennington.FrontMatter.IFrontMatter`) requires only `Title` -- everything else comes from capability interfaces like `IOrderable`, `IDescribable`, `IDraftable`
- Live reload is active when running under `dotnet watch` -- editing the markdown file refreshes the browser automatically

## Beat 4: Add a Development section with multiple pages

The reader creates a `development/` folder with two markdown pages using `order` to control navigation sort order. The goal is to see how folder structure maps to URL structure and how Pennington auto-discovers content.

### What to show
- Create `Content/development/coding-standards.md` with front matter: `title: "Coding Standards"`, `order: 10`, `description: "Naming conventions and formatting rules"`. Body includes bullet points and a fenced C# code block to demonstrate syntax highlighting
- Create `Content/development/pr-process.md` with front matter: `title: "Pull Request Process"`, `order: 20`. Body uses a numbered list
- After saving, show that the dev server auto-discovers the new files
- The pages are now accessible at `/development/coding-standards/` and `/development/pr-process/`

### Key points
- Files inside `Content/development/` get URLs prefixed with `/development/`
- The `P:Pennington.FrontMatter.DocFrontMatter.Order` property (`P:Pennington.FrontMatter.IOrderable.Order`) controls navigation sort order -- lower numbers appear first
- Code blocks get syntax highlighting automatically via Pennington's `T:Pennington.Highlighting.HighlightingService` which uses TextMate grammars
- Pennington's `T:Pennington.Content.MarkdownContentService`1` scans the content directory recursively and creates `T:Pennington.Content.ContentTocItem` entries for the navigation system
- Each `ContentTocItem` has `HierarchyParts` derived from the folder path -- `["development", "coding-standards"]` for `Content/development/coding-standards.md`

## Beat 5: Add a second section to show navigation grouping

The reader creates an `operations/` folder with two more pages. The goal is to show how Pennington groups pages into sections in the navigation tree based on folder structure.

### What to show
- Create `Content/operations/deployment-checklist.md` with front matter: `title: "Deployment Checklist"`, `order: 10`. Body uses markdown checkboxes
- Create `Content/operations/incident-response.md` with front matter: `title: "Incident Response"`, `order: 20`. Body uses a markdown table for severity levels
- The navigation now has two distinct groups: "Development" and "Operations"
- Explain that folder names become section headers via `NavigationBuilder` -- `development` becomes "Development", `incident-response` would become "Incident Response" (kebab-case to Title Case)

### Key points
- `T:Pennington.Navigation.NavigationBuilder` transforms flat `ContentTocItem` lists into a tree of `T:Pennington.Navigation.NavigationTreeItem` records
- Auto-created section nodes use the folder name formatted as a title (the `FormatSectionTitle` method converts kebab-case to Title Case)
- `T:Pennington.Navigation.NavigationTreeItem` has `Children`, `IsSelected`, and `IsExpanded` properties for rendering stateful navigation
- No configuration change is needed in Program.cs -- adding folders and files is all it takes to expand the site structure

## Beat 6: Build the layout with TableOfContentsNavigation

The reader creates a Razor layout component with a sidebar using `TableOfContentsNavigation` and a main content area. The goal is to show how Pennington's navigation data feeds into the UI component.

### What to show
- Create `Components/Layout/MainLayout.razor` with:
  - A two-column flexbox layout: sidebar (fixed width) and main area (flex-1)
  - A header with the site title
  - The `TableOfContentsNavigation` component in the sidebar
  - `@Body` in the main content area
  - The `OutlineNavigation` component in a right sidebar (optional, for "On This Page" headings)
- Code reference for the TableOfContentsNavigation component: `:path src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor`
- Show the key parameters:
  - `TableOfContents` -- accepts `ImmutableList<NavigationTreeItem>` from `NavigationBuilder.BuildTree()`
  - `ListGapClass`, `ChildListClass`, `SectionHeaderStructureClass`, `SectionHeaderColorClass`, `LinkStructureClass`, `LinkColorClass`, `RootLinkStructureClass`, `RootLinkColorClass` -- all customizable via MonorailCSS utility classes
- Show injecting `NavigationBuilder` and `IContentService` into the layout to build the tree:
  - `T:Pennington.Navigation.NavigationBuilder` -- injected via DI
  - `M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)` -- builds the tree from TOC items
- Code reference for OutlineNavigation: `:path src/Pennington.UI/Components/Navigation/OutlineNavigation.razor`
  - `ContentSelector` parameter (required) -- CSS selector for the element containing headings to track (e.g., `"article main"`)

### Key points
- `TableOfContentsNavigation` renders a `<nav>` with nested `<ul>` elements reflecting the navigation tree
- It marks the current page with `data-current="true"` on the link, enabling CSS-driven active state styling
- Section headers (auto-created from folders) render as non-clickable `<div>` elements; actual pages render as `<a>` links
- `OutlineNavigation` uses JavaScript to observe headings in the content area and generates an "On This Page" sidebar dynamically
- The `NavigationBuilder.BuildTree()` method handles all the hierarchy logic: grouping by folder, sorting by order then title, computing `IsSelected` and `IsExpanded` states
- `T:Pennington.Navigation.NavigationInfo` provides `PreviousPage`, `NextPage`, and `Breadcrumbs` for prev/next navigation and breadcrumb trails

## Beat 7: Understand file-to-URL mapping

The reader studies a table mapping file paths to URLs. The goal is to establish the mental model for how Pennington routes content.

### What to show
- A table of file paths and their resulting URLs:
  - `Content/index.md` maps to `/`
  - `Content/development/coding-standards.md` maps to `/development/coding-standards/`
  - `Content/development/pr-process.md` maps to `/development/pr-process/`
  - `Content/operations/deployment-checklist.md` maps to `/operations/deployment-checklist/`
- Explain the rules: file names are lowercased, hyphens preserved, `.md` stripped, trailing slash added, `index.md` maps to the directory root
- Reference `T:Pennington.Routing.UrlPath` and `T:Pennington.Routing.ContentRoute`:
  - `P:Pennington.Routing.ContentRoute.CanonicalPath` -- the URL path used for routing
  - `P:Pennington.Routing.ContentRoute.OutputFile` -- the file path used during static generation (e.g., `development/coding-standards/index.html`)
  - `P:Pennington.Routing.ContentRoute.SourceFile` -- the original markdown file path

### Key points
- Pennington uses `UrlPath` (a readonly record struct) for all URL handling -- it normalizes leading/trailing slashes
- `ContentRoute` ties together the canonical URL, the output file path, and the source file path
- The `MarkdownContentOptions.BasePageUrl` setting (`P:Pennington.Infrastructure.MarkdownContentOptions.BasePageUrl`) controls the URL prefix for a content source -- set to `"/"` for the root, or `"/docs"` to nest everything under `/docs/`
- Static generation writes each page as `{url-path}/index.html` so URLs work with trailing slashes on static hosts

## Beat 8: Generate static output

The reader runs the build command to generate a static site and inspects the output. The goal is to show the static generation workflow and the build report.

### What to show
- Terminal command: `dotnet run -- build`
- Show the console output: the build report from `M:Pennington.Generation.BuildReport.WriteTo(System.IO.TextWriter)` -- pages generated count, build time, any warnings
- List the contents of the `output/` directory: `index.html`, `development/coding-standards/index.html`, `styles.css`, `search-index.json`, `sitemap.xml`, `404.html`
- Open `output/index.html` in a browser to verify it works as a static file
- Reference `T:Pennington.Generation.OutputGenerationService` -- the service that crawls the running app via HTTP to generate static files
- Reference `T:Pennington.Generation.OutputOptions`:
  - `P:Pennington.Generation.OutputOptions.OutputDirectory` -- defaults to `"output"`, configurable via CLI args
  - `P:Pennington.Generation.OutputOptions.BaseUrl` -- defaults to `"/"`, used for subdirectory deployments (covered in the deployment tutorial)
- Reference `T:Pennington.Generation.BuildReport`:
  - `P:Pennington.Generation.BuildReport.GeneratedPages` -- list of successfully generated pages
  - `P:Pennington.Generation.BuildReport.FailedPages` -- any pages that errored during generation
  - `P:Pennington.Generation.BuildReport.BrokenLinks` -- internal links that don't resolve
  - `P:Pennington.Generation.BuildReport.Duration` -- total build time
  - `P:Pennington.Generation.BuildReport.HasErrors` -- true if any diagnostics have Error severity, or if there are broken links or failed pages

### Key points
- `RunOrBuildAsync` checks `args[0]` for `"build"` -- if found, it starts the app, runs `OutputGenerationService.GenerateAsync`, writes the report, and exits
- The static generator works by HTTP-crawling the running app -- it fetches every discovered page, every MapGet route (like `/styles.css`), and writes the responses to disk
- HTML content pages are fetched first, then MapGet routes last -- this ensures the CSS class collector has seen all HTML before generating the stylesheet
- The build also generates `404.html` by fetching a non-existent URL, `search-index.json` for client-side search, and `sitemap.xml`
- The build report exits with code 1 if there are errors, making it suitable for CI/CD pipelines
- Point the reader to the "Deploying to GitHub Pages" tutorial for the next step
