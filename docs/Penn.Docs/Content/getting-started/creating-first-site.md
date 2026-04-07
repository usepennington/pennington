---
title: "Creating Your First Site"
description: "Build a working documentation site from scratch with Penn.DocSite"
uid: "penn.getting-started.creating-first-site"
order: 1001
---

In this tutorial, you'll build a working documentation site from scratch using Penn.DocSite. You'll start with an empty ASP.NET project and finish with a site that renders markdown content, generates sidebar navigation, supports live reload during development, and produces static HTML for deployment.

The whole process takes about ten minutes.

## What You'll Build

A documentation site with:

- Markdown pages rendered with syntax highlighting and a styled layout
- YAML front matter controlling titles, ordering, and navigation
- Automatic sidebar navigation generated from your content structure and directory hierarchy
- A dev server with live reload that picks up new and changed files
- Static HTML output suitable for deployment to any host

## Prerequisites

- **.NET 11 SDK** or later installed on your machine
- **A code editor** -- Visual Studio, VS Code, Rider, or anything that edits text files
- **A terminal** for running CLI commands

<Steps>
<Step stepNumber="1">
## Create a New ASP.NET Project

Start with an empty ASP.NET project. Penn doesn't need Blazor templates or MVC scaffolding. It brings its own Razor components, layout, and middleware.

```bash
dotnet new web -n MyDocsSite
cd MyDocsSite
```

This gives you a minimal `Program.cs` and a `.csproj` file. That's all you need.
</Step>

<Step stepNumber="2">
## Add Penn.DocSite

Penn ships as focused NuGet packages. `Penn.DocSite` is the batteries-included option for documentation sites. It pulls in the core content engine, UI components, MonorailCSS styling, and SPA navigation.

```bash
dotnet add package Penn.DocSite --prerelease
```

This single package registers everything your site needs. You don't need to install Penn's core library, UI components, or CSS framework separately -- `Penn.DocSite` depends on all of them and brings them in transitively.

> [!NOTE]
> If you need Roslyn-powered code extraction (embedding live source code from your .NET solution), that's a separate package you add later. See [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) after completing this tutorial.
</Step>

<Step stepNumber="3">
## Configure Program.cs

Replace the contents of `Program.cs` with Penn's setup. Here's the minimal configuration:

```csharp
using Penn.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions // [!code highlight]
{
    SiteTitle = "My Docs",
    Description = "A site about things",
});

var app = builder.Build();
app.UseDocSite(); // [!code highlight]
await app.RunDocSiteAsync(args); // [!code highlight]
```

Three calls do all the work:

- **`AddDocSite`** registers Penn's core engine, the markdown processing pipeline, MonorailCSS utility-first styling, Razor components, and SPA navigation. It accepts a factory function that returns a `DocSiteOptions` record.
- **`UseDocSite`** wires up the middleware pipeline in the correct order: static files, antiforgery, Razor component routing, MonorailCSS processing, and SPA navigation. Calling this single method replaces what would otherwise be five or six individual middleware registrations.
- **`RunDocSiteAsync`** handles both runtime modes. With no arguments, it starts the dev server. With `build` as a command-line argument, it generates static HTML and exits.

`DocSiteOptions` has two required properties: `SiteTitle` and `Description`. Everything else has sensible defaults. You can customize colors, fonts, GitHub links, canonical URLs, and more as your site grows.

For a real-world example, here's the full `Program.cs` used by the Penn documentation site you're reading now:

```csharp:path
docs/Penn.Docs/Program.cs
```

The Penn docs site adds custom fonts, a color scheme, a GitHub link, social image, and Roslyn integration. You can add these later as your site grows.

> [!TIP]
> The `ContentRootPath` property defaults to `"Content"`, so Penn looks for your markdown files in a `Content` directory relative to your project root. You can change this to any path that fits your project structure.
</Step>

<Step stepNumber="4">
## Understand DocSiteFrontMatter

Every markdown page in a Penn.DocSite project uses `DocSiteFrontMatter` as its metadata model. This record defines what you can put in the YAML front matter block at the top of each file.

Here's the type:

```csharp:xmldocid
T:Penn.DocSite.DocSiteFrontMatter
```

Each property maps to a YAML key in your markdown files:

| Property | Purpose | Required |
|----------|---------|----------|
| `Title` | Page title, shown in navigation and headings | Yes |
| `Description` | Page description for SEO and social cards | No |
| `Order` | Sort position in navigation (lower numbers first) | No |
| `Uid` | Unique identifier for cross-references via `xref:` links | No |
| `Tags` | Categorization labels | No |
| `Section` | Groups pages under a navigation section header | No |
| `IsDraft` | Hides the page from navigation when `true` | No |
| `RedirectUrl` | Redirects this URL to another page | No |

`DocSiteFrontMatter` implements several capability interfaces: `IDraftable`, `ITaggable`, `ISectionable`, `ICrossReferenceable`, `IOrderable`, `IDescribable`, and `IRedirectable`. Penn's pipeline uses these interfaces to determine what features each page supports.

> [!NOTE]
> If `DocSiteFrontMatter` doesn't fit your needs, you can define your own record implementing `IFrontMatter` and whichever capability interfaces you want. But for most documentation sites, the built-in type covers everything.
</Step>

<Step stepNumber="5">
## Create Your First Content Page

By default, Penn looks for markdown files in a `Content` directory relative to your project root. Create it and add your first page.

```bash
mkdir Content
```

Create `Content/index.md`:

```markdown
---
title: "Welcome"
description: "The front page of my documentation site"
order: 1
---

# Hello from Penn

This is your first page. It was rendered from markdown with
automatic syntax highlighting, styled layout, and navigation.

## What's Here

Write your content in the `Content/` directory. Penn finds it,
parses the YAML front matter, and renders it. That's the workflow.
```

The YAML front matter block between the `---` delimiters maps to `DocSiteFrontMatter`. The `title` appears in sidebar navigation. The `order` value controls sort position, with lower numbers appearing first. Everything after the closing `---` is your markdown content.

Penn automatically routes this file to `/` because it's named `index.md`. A file named `getting-started.md` would be routed to `/getting-started`. The routing convention is straightforward: the file path relative to the `Content` directory becomes the URL path, with the `.md` extension removed and `index` files mapped to their parent directory.
</Step>

<Step stepNumber="6">
## Add a Second Page and a Section

One page is a file. Two pages start to show how navigation works. Create `Content/installation.md`:

```markdown
---
title: "Installation"
description: "How to install the prerequisites"
order: 2
---

## System Requirements

You'll need the .NET 11 SDK and a code editor.

## Install the SDK

Download the .NET 11 SDK from the official .NET website.
```

Now create a subdirectory for a section of related pages. Subdirectories become navigation groups automatically.

```bash
mkdir Content/guides
```

Create `Content/guides/writing-content.md`:

```markdown
---
title: "Writing Content"
description: "How to write markdown content for your site"
section: "Guides"
order: 100
---

## Markdown Basics

Penn uses standard CommonMark markdown with a few extensions
for alerts, tabs, and syntax highlighting.
```

The `section` front matter property controls the heading that appears above this group of pages in the sidebar. Pages in subdirectories with the same `section` value are grouped together under that heading. This gives you two levels of organization: the directory structure determines grouping, and `section` provides the display name.

You can add as many subdirectories as your site needs. Each one becomes a navigable section with its own group of pages.

> [!TIP]
> Use `order` values with gaps between them (like 100, 200, 300) so you can insert pages later without renumbering everything. Pages without an `order` value sort to the end of their section.
</Step>

<Step stepNumber="7">
## Configure File Watching

To get live reload working during development, tell `dotnet watch` to monitor your content files. Add this `ItemGroup` to your `.csproj`:

```xml
<ItemGroup>
    <Watch Include="Content\**\*.*" />
</ItemGroup>
```

Without this, `dotnet watch` only monitors `.cs` and `.razor` files by default. Adding the `Watch` item ensures that changes to markdown files, images, and any other content assets trigger a rebuild and browser refresh.

Your complete `.csproj` should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Penn.DocSite" Version="*-*" />
  </ItemGroup>

  <ItemGroup>
    <Watch Include="Content\**\*.*" />
  </ItemGroup>

</Project>
```
</Step>

<Step stepNumber="8">
## Run the Dev Server

Start your site:

```bash
dotnet run
```

Navigate to the URL shown in your terminal (typically `https://localhost:5001` or `http://localhost:5000`). You'll see your content rendered with Penn's doc site layout, including a styled sidebar with navigation generated from your pages.

For live reload during development, use:

```bash
dotnet watch
```

Edit `Content/index.md`, save, and watch the browser update. Add new markdown files to `Content/`, rename them, or delete them. Penn watches the filesystem and picks up all changes without a manual restart.

Try adding a third page to see navigation update automatically. Create any `.md` file with valid front matter in your `Content` directory and refresh the browser.

> [!NOTE]
> Penn generates navigation entirely from your content files and their front matter. The `title` property becomes the link text, `order` controls sort position, and subdirectories with `section` values create navigation groups. There is no separate navigation configuration file to maintain.
</Step>

<Step stepNumber="9">
## Generate Static Output

When you're ready to deploy, Penn generates a complete static site. Pass `build` as a command-line argument:

```bash
dotnet run -- build
```

This starts the server internally, crawls every known page, writes static HTML to an `output` directory, and exits. The output directory contains everything needed for deployment: HTML files, CSS, JavaScript, and static assets.

For GitHub Pages or other subdirectory deployments, pass the base path:

```bash
dotnet run -- build "/my-project/"
```

Penn rewrites all links, asset paths, and navigation URLs to include the base path prefix. In development mode, everything works at `/` as usual.

You can also specify a custom output directory:

```bash
dotnet run -- build "/" "dist"
```

The `output` directory (or your custom directory) is a self-contained static site. Upload it to any web server, CDN, or hosting service that can serve static files.

> [!TIP]
> During static generation, Penn starts the ASP.NET server, discovers every content route, requests each page, and writes the rendered HTML to disk. This means your static output is identical to what the dev server renders. If it looks right in `dotnet run`, it will look right after `dotnet run -- build`.
</Step>
</Steps>

## What You've Built

Your site now has:

- A styled page layout courtesy of Penn.DocSite and MonorailCSS
- Sidebar navigation auto-generated from your content files, their `title` front matter, and directory structure
- Proper URL routing where `Content/guides/writing-content.md` becomes `/guides/writing-content`
- Live reload in development via `dotnet watch`
- Static HTML generation via `dotnet run -- build`

The entire setup is three method calls in `Program.cs` and markdown files in a `Content` directory. No configuration files, no routing tables, no template registrations. Add content by creating markdown files, organize it with directories and front matter, and deploy by running the build command.

## Next Steps

Now that you have a working site, you can extend it in several directions:

- [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) -- add live, verified code examples pulled directly from your .NET solution so your documentation always matches your actual code
- [Using UI Elements](xref:penn.getting-started.using-ui-elements) -- enhance your pages with cards, badges, steps, and other Penn.UI components that are already available through Penn.DocSite
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) -- ship your site with a GitHub Actions workflow that builds and deploys automatically on every push to `main`
