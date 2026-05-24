---
title: "Scaffold a documentation site with DocSite"
description: "Stand up the DocSite template on an empty ASP.NET project and map content areas to top-level folders."
sectionLabel: Getting Started with DocSite
order: 102010
tags: [docsite, template, areas, scaffold]
uid: tutorials.docsite.scaffold
---

By the end of this tutorial the DocSite host runs with a "Scaffold Docs" title, GitHub icon, header/footer chrome, and two content areas — Guides and Reference — each serving an index page from its own top-level folder.

`AddDocSite` is a shortcut. It pre-wires what the [getting-started tutorials](xref:tutorials.getting-started.first-site) assemble by hand — host, layout, navigation, styling — into one call, tuned for a single shape: a Divio-style documentation site. Reach for it when that shape fits; build on `AddPennington` directly for anything else. For what the template wires and where the wiring stops, read [what the templates wire for you](xref:explanation.positioning.docsite-positioning) first.

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
    <PackageReference Include="Pennington.DocSite" Version="0.1.0-alpha.0.20" />
  </ItemGroup>
</Project>
```

</Step>
</Steps>

---

## 2. Wire `AddDocSite`, `UseDocSite`, and `RunDocSiteAsync`

`AddDocSite` is a single DI call that registers Pennington core, MonorailCSS, SPA navigation, the content resolver, and the article-slot renderer — all driven from one options object. `UseDocSite` is its middleware counterpart; `RunDocSiteAsync` dispatches the host between dev-serve and static-build modes (see <xref:reference.host.cli> for the args contract).

<Steps>
<Step StepNumber="1">

**Replace `Program.cs` with the three DocSite calls**

`AddDocSite` takes a `Func<DocSiteOptions>` rather than an `Action`, so the call constructs and returns a fresh options record. The template registers the markdown content reader internally — no separate `AddMarkdownContent` call is needed. `RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves pages live in development and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>`.

```csharp:symbol,bodyonly
examples/DocSiteScaffoldExample/Stage3_UseDocSite.cs > Stage3.Run
```

The five fields populated here — `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, `FooterContent` — each surface in the rendered chrome as soon as they're set. `DocSiteOptions` carries many more fields; see <xref:reference.api.doc-site-options> for the full surface, and [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning) for what the template hard-codes.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The DocSite layout renders: left sidebar, header with site title, search affordance, dark-mode toggle, GitHub icon linking to `GitHubUrl`, and the footer HTML from `FooterContent`

</Checkpoint>

---

## 3. Map content to areas

`DocSiteOptions.Areas` is a list of `ContentArea(Label, Slug)` pairs. Each slug binds a top-level folder under `ContentRootPath` to a URL prefix and to its own sidebar tab. `ContentArea` is a two-field record (`record ContentArea(string Label, string Slug)`); the order of entries in `Areas` drives the order of tabs in the sidebar.

<Steps>
<Step StepNumber="1">

**Add two `ContentArea` entries to `DocSiteOptions`**

Update the `Areas` block in `Program.cs` to register `guides` and `reference`. The sidebar only shows the area selector when more than one area is configured, so two entries are the minimum to surface the tab switcher.

```csharp
Areas =
[
    new ContentArea("Guides", "guides"),
    new ContentArea("Reference", "reference"),
],
```

</Step>
<Step StepNumber="2">

**Create the area folders**

Under `Content/`, create two folders — `guides/` and `reference/` — each with an `index.md`. The `guides` slug binds `Content/guides/` to the `/guides/` URL prefix and to the Guides sidebar tab; `reference` works the same way.

```markdown:symbol
examples/DocSiteScaffoldExample/Content/guides/index.md
```

```markdown:symbol
examples/DocSiteScaffoldExample/Content/reference/index.md
```

</Step>
</Steps>

<Checkpoint>

- Visit `http://localhost:5000/guides/` — the Guides index page renders with the Guides tab selected in the sidebar
- Visit `http://localhost:5000/reference/` — the Reference index page renders, the Reference tab is now selected, and the sidebar TOC filters to the Reference area only

</Checkpoint>

---

## 4. Give the root `/` a landing page

With `Areas` configured, the URL `/` sits **outside** every area — it is not a default redirect into the first area, and the area selector shows no active tab there. To make `/` render something other than a 404, drop a markdown file at `Content/index.md` (next to the area folders, not inside them).

<Steps>
<Step StepNumber="1">

**Author `Content/index.md`**

Use the same [`DocSiteFrontMatter`](xref:reference.api.doc-site-front-matter) shape as any other page. The page resolves through the same content pipeline as area pages — the only thing that makes it the root is its location at `Content/index.md`.

```markdown
---
title: Welcome to Scaffold Docs
description: Pick an area to get started.
---

# Welcome

- [Guides](/guides/) — task walk-throughs and onboarding.
- [Reference](/reference/) — every option, key, and surface.
```

</Step>
<Step StepNumber="2">

**Verify the root renders without an active area**

Visit `http://localhost:5000/` — the page renders inside the DocSite chrome, the area selector shows no active tab (because the root is outside every area), and the sidebar is empty for the same reason. Any `/guides/...` or `/reference/...` link inside the page navigates into the matching area and lights up the corresponding sidebar tab.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` returns the rendered `Content/index.md` page with the DocSite chrome around it
- The area selector shows neither *Guides* nor *Reference* as active until the reader clicks into one
- A request to `/some-area/` still resolves the matching area as in unit 4

</Checkpoint>

---

## Summary

- An empty ASP.NET project picked up `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, and the full Razor chrome renders.
- `DocSiteOptions` carries `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, and `FooterContent`, and each field appears in the rendered layout.
- Two `ContentArea` entries bind top-level folders under `Content/` to URL prefixes and to sidebar tabs.
- The root `/` is served by `Content/index.md`, which sits outside every area — without it, `/` returns a 404 even when areas are configured.
- For the seams DocSite leaves open and what it hard-codes, see [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning).
