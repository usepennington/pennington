---
title: "Creating Your First Site"
description: "Build a complete documentation site from scratch using Penn"
uid: "penn.getting-started.creating-first-site"
order: 1001
---

Penn is opinionated, inflexible, and sufficient to the purpose. It will not try to be everything to everyone. It will, however, build you a perfectly serviceable content site with minimal ceremony — and that's what we're doing here.

By the end of this tutorial, you'll have a working site that serves markdown content in dev mode and generates static HTML when you ask nicely.

## What You'll Build

A documentation-style site with:

- Markdown content rendered to pages
- YAML front matter for metadata
- Live reload in development
- Static output for deployment

## Prerequisites

- .NET 11 SDK or later
- A code editor you have opinions about
- A terminal

<Steps>
<Step stepNumber="1">
## Create a New Project

Start with an empty ASP.NET project. Penn doesn't need Blazor templates or MVC scaffolding — it brings what it needs.

```bash
dotnet new web -n MyDocsSite
cd MyDocsSite
```
</Step>

<Step stepNumber="2">
## Add Penn.DocSite

Penn ships as a few focused packages. `Penn.DocSite` is the batteries-included option for documentation sites — it pulls in the core engine, UI components, and a MonorailCSS-based design.

```bash
dotnet add package Penn.DocSite --prerelease
```

One package. One line. Penn has opinions about what you need, and this is most of it.
</Step>

<Step stepNumber="3">
## Understand the Front Matter Model

Penn uses a front matter record to define metadata for your content pages. The `DocSiteFrontMatter` type ships with `Penn.DocSite` and covers the common cases — title, description, ordering, tags, drafts, sections, UIDs for cross-referencing, and redirects.

Here's what it looks like:

```csharp:xmldocid
T:Penn.DocSite.DocSiteFrontMatter
```

Each property maps to a YAML key in your markdown files. You don't need to fill in all of them — `Title` is the only one that matters to get started.

If `DocSiteFrontMatter` doesn't fit your needs, you can define your own record implementing `IFrontMatter` and whichever capability interfaces you want (`IDraftable`, `ITaggable`, `IOrderable`, etc.). But for most documentation sites, the built-in type is the right call.
</Step>

<Step stepNumber="4">
## Configure Program.cs

Replace the contents of `Program.cs` with Penn's configuration. Here's the actual configuration used by the Penn docs site itself:

```csharp:path
docs/Penn.Docs/Program.cs
```

That's the whole thing. `AddDocSite` registers Penn's core engine, the markdown pipeline, MonorailCSS, Razor components, and SPA navigation. `UseDocSite` wires up the middleware. `RunDocSiteAsync` handles both dev server mode and static build mode based on command-line arguments.

For a minimal starting point, you can trim it down:

```csharp
using Penn.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "A site about things",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

`DocSiteOptions` gives you control over titles, colors, fonts, GitHub links, and more. But `SiteTitle` and `Description` are the only required properties.
</Step>

<Step stepNumber="5">
## Create Content

By default, `DocSiteOptions` looks for markdown files in a `Content` directory relative to your project root. Create it:

```bash
mkdir Content
```

Now add your first page. Create `Content/index.md`:

```markdown
---
title: "Welcome"
description: "The front page"
order: 1
---

# Hello from Penn

This is your first page. It was rendered from markdown, which is
the only markup language that matters.

## Getting Started

Write your content in `Content/`. Penn finds it, parses the YAML
front matter, and renders it. That's the whole workflow.
```

The YAML front matter block at the top maps to `DocSiteFrontMatter`. The `title` appears in navigation. The `order` controls sorting. Everything after the closing `---` is your markdown content.
</Step>

<Step stepNumber="6">
## Add a Second Page

One page is a file. Two pages is a site. Create `Content/second-page.md`:

```markdown
---
title: "A Second Page"
description: "Proof that navigation works"
order: 2
---

## Another Page

Penn automatically generates navigation from your content structure.
Add files, see them appear. Remove files, watch them vanish.
No configuration required.
```
</Step>

<Step stepNumber="7">
## Configure File Watching

To get live reload working during development, tell `dotnet watch` to monitor your content files. Add this to your `.csproj`:

```xml
<ItemGroup>
    <Watch Include="Content\**\*.*" />
</ItemGroup>
```

This ensures changes to markdown files trigger a rebuild without restarting the server.
</Step>

<Step stepNumber="8">
## Run Your Site

Start the dev server:

```bash
dotnet run
```

Navigate to the URL shown in your terminal (typically `http://localhost:5000`). You'll see your content rendered with Penn's doc site layout, complete with sidebar navigation generated from your pages.

For live reload during development, use:

```bash
dotnet watch
```

Edit `Content/index.md`, save, and watch the browser update. Add new files, rename them, delete them — Penn picks up all of it without a restart.
</Step>

<Step stepNumber="9">
## Generate Static Output

When you're ready to deploy, Penn can generate a complete static site:

```bash
dotnet run -- build
```

This starts the server, crawls every page, and writes static HTML to an `output` directory. For GitHub Pages or other subdirectory deployments, pass the base path:

```bash
dotnet run -- build "/my-project/"
```

That's it. HTML files in a folder. The web's original deployment model.
</Step>
</Steps>

## What Success Looks Like

When `dotnet run` is running, your site has:

- A styled page layout courtesy of Penn.DocSite and MonorailCSS
- Sidebar navigation auto-generated from your content files and their `title` front matter
- Proper URL routing — `Content/second-page.md` becomes `/second-page`

Try editing your markdown and saving. Try adding a third page. Penn watches the filesystem and does the right thing without being asked.

## Next Steps

- [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) — embed live, verified code examples from your .NET solution
- [Using UI Elements](xref:penn.getting-started.using-ui-elements) — enhance your pages with cards, badges, steps, and more
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) — ship it with a GitHub Actions workflow
