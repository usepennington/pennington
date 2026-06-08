---
title: "Scaffold a documentation site with DocSite"
description: "Stand up the DocSite template on an empty ASP.NET project and let a folder of markdown light up the sidebar."
sectionLabel: Getting Started with DocSite
order: 1
tags: [docsite, template, scaffold, navigation]
uid: tutorials.docsite.scaffold
---

By the end of this tutorial the DocSite host runs with a "Scaffold Docs" title, GitHub icon, and header/footer chrome — and a folder of markdown pages that show up in the sidebar on their own, with a landing page at the root.

`AddDocSite` wires what the [getting-started tutorials](xref:tutorials.getting-started.first-site) assemble by hand — host, layout, navigation, styling — into one call, configured for a Divio-style documentation site. Use it when that fits; build on `AddPennington` directly for anything else. For what the template wires and where the wiring stops, read [what the templates wire for you](xref:explanation.positioning.docsite-positioning) first.

> [!NOTE]
> DocSite and BlogSite can't run in the same app — pick one. If you want a blog
> alongside your documentation, stay on DocSite: it has a native blog you switch
> on by adding a `Content/blog/` folder, no `Program.cs` change required. See
> [Add a blog to your documentation site](xref:tutorials.docsite.add-a-blog).

## Prerequisites

- .NET 11 SDK installed
- A terminal and a text editor or IDE that understands C# 15

The finished code for this tutorial lives in [`examples/DocSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteScaffoldExample).

---

## 1. Scaffold a new ASP.NET project

Start from an empty ASP.NET web project. DocSite ships everything from routing to the Razor layout, so the project shell is the only scaffolding needed before `AddDocSite`.

<Steps>
<Step StepNumber="1">

**Create the web project**

```bash
dotnet new web -n DocSiteScaffold
cd DocSiteScaffold
```

</Step>
<Step StepNumber="2">

**Add the Pennington DocSite package**

```bash
dotnet add package Pennington.DocSite
```

> [!IMPORTANT]
> Pennington is in alpha — check NuGet for the current prerelease and pin every `Pennington.*` package to that same version.

</Step>
<Step StepNumber="3">

**Opt into C# preview language features**

Pennington uses C# 15 union types, which are still a preview language feature in the .NET 11 SDK. Edit the csproj to add `<LangVersion>preview</LangVersion>` so they compile:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Pennington.DocSite" Version="<?# PackageVersion /?>" />
  </ItemGroup>
</Project>
```

</Step>
</Steps>

---

## 2. Wire `AddDocSite`, `UseDocSite`, and `RunDocSiteAsync`

`AddDocSite` is a single DI call that registers Pennington core, [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/), SPA navigation, the content resolver, and the component that renders the page body into the layout — all driven from one options object. `UseDocSite` is its middleware counterpart; `RunDocSiteAsync` dispatches the host between dev-serve and static-build modes (see <xref:reference.host.cli> for the args contract).

<Steps>
<Step StepNumber="1">

**Replace `Program.cs` with the three DocSite calls**

`AddDocSite` takes a `Func<DocSiteOptions>` rather than an `Action`, so the call constructs and returns a fresh options record. The template registers the markdown content reader internally — no separate `AddMarkdownContent` call is needed. `RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves pages live in development and generates static HTML when invoked as `dotnet run -- build --base-url <url> --output <dir>`.

```csharp:symbol,bodyonly
examples/DocSiteScaffoldExample/Stage3_UseDocSite.cs > Stage3.Run
```

The five fields populated here — `SiteTitle`, `SiteDescription`, `GitHubUrl`, `HeaderContent`, `FooterContent` — each surface in the rendered chrome as soon as they're set. `DocSiteOptions` carries many more fields; see <xref:reference.api.doc-site-options> for the full surface, and [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning) for what the template hard-codes.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The DocSite layout renders: left sidebar, header with site title, search affordance, dark-mode toggle, GitHub icon linking to `GitHubUrl`, and the footer HTML from `FooterContent`

</Checkpoint>

---

## 3. Add pages and watch the sidebar fill in

DocSite builds the sidebar from the shape of `Content/`. Drop a markdown file into a folder and it becomes a sidebar entry — no routing table, no registration call. A subfolder turns into a navigation group named after the folder.

<Steps>
<Step StepNumber="1">

**Create a `guides` folder with a landing page**

Under `Content/`, create a `guides/` folder and add an `index.md`. The folder name becomes a **Guides** group in the sidebar, and `index.md` is the page it links to at `/guides/`.

```markdown:symbol
examples/DocSiteScaffoldExample/Content/guides/index.md
```

</Step>
<Step StepNumber="2">

**Add two pages inside the folder**

Add two more files next to `index.md`. Each carries a `title`, a `description`, and an `order:` — the `order:` decides which sorts first. Dropping the files in the folder is the whole wiring.

```markdown:symbol
examples/DocSiteScaffoldExample/Content/guides/getting-started.md
```

```markdown:symbol
examples/DocSiteScaffoldExample/Content/guides/configuration.md
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/guides/getting-started`
- The sidebar shows a **Guides** group with *Getting started* above *Configuration* — sorted by `order:`
- The page you're viewing is highlighted in the sidebar; clicking *Configuration* swaps to it instantly

</Checkpoint>

---

## 4. Give the root `/` a landing page

The pages under `guides/` answer to `/guides/...`, but the root `/` has no page of its own yet — a request to `/` returns a 404. To serve the root, drop a markdown file at `Content/index.md`, next to the `guides/` folder.

<Steps>
<Step StepNumber="1">

**Author `Content/index.md`**

Use the same [`DocSiteFrontMatter`](xref:reference.api.doc-site-front-matter) shape as any other page. What makes this page the root is its location — `Content/index.md` maps to `/`.

```markdown
---
title: Welcome
description: Start here, then pick a guide.
---

Welcome to Scaffold Docs.

- [Getting started](/guides/getting-started) — install and run.
- [Configuration](/guides/configuration) — where settings live.
```

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` returns the rendered `Content/index.md` page inside the DocSite chrome
- Both links navigate into the `guides/` pages and highlight them in the sidebar

</Checkpoint>

---

## Summary

- An empty ASP.NET project picked up `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, and the full Razor chrome renders.
- `DocSiteOptions` carries `SiteTitle`, `SiteDescription`, `GitHubUrl`, `HeaderContent`, and `FooterContent`, and each field appears in the rendered layout.
- Markdown files under `Content/` become sidebar entries with no extra wiring — a subfolder turns into a navigation group named after the folder, and `order:` sorts the pages inside it.
- The root `/` is served by `Content/index.md`; without it, `/` returns a 404.
- To split the sidebar into switchable areas and labeled sections, see [Organize content with sections and areas](xref:tutorials.docsite.sections-and-areas). For what the template hard-codes, see [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning).
