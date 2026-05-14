---
title: "Scaffold a documentation site with DocSite"
description: "Stand up the DocSite template on an empty ASP.NET project and map content areas to top-level folders."
sectionLabel: Getting Started with DocSite
order: 102010
tags: [docsite, template, areas, scaffold]
uid: tutorials.docsite.scaffold
---

By the end of this tutorial the DocSite host runs with a "Scaffold Docs" title, GitHub icon, header/footer chrome, and two content areas — Guides and Reference — each serving an index page from its own top-level folder.

This tutorial covers starting from an empty ASP.NET project, wiring the DocSite template, populating `DocSiteOptions`, and understanding how area slugs bind top-level folders to URL prefixes and sidebar tabs. For the shape the template hard-codes — and the seams it leaves open — read [Positioning DocSite as a fast path](xref:explanation.positioning.docsite-positioning) first.

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

```text
dotnet new web -n DocSiteScaffold
cd DocSiteScaffold
```

</Step>
<Step StepNumber="2">

**Add the Pennington DocSite package**

```text
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

<Checkpoint>

- `dotnet build` succeeds with no errors.
- Hold off on `dotnet run` until the next section wires `AddDocSite`.

</Checkpoint>

---

## 2. Register `AddDocSite`

`AddDocSite` is a single DI call that registers Pennington core, MonorailCSS, SPA navigation, the `ContentResolver`, and the `DocSiteArticleSlotRenderer` Razor island — all driven from one options object.

<Steps>
<Step StepNumber="1">

**Add the registration call**

`AddDocSite` takes a `Func<DocSiteOptions>` rather than an `Action`, so the call constructs and returns a fresh options record. The template registers the markdown content reader internally — no separate `AddMarkdownContent` call is needed. See <xref:reference.host.extensions> for the full signature.

</Step>
<Step StepNumber="2">

**Populate `DocSiteOptions`**

This tutorial uses five fields: `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, and `FooterContent`. Each one surfaces in the rendered chrome as soon as it's set. `DocSiteOptions` carries many more fields; see <xref:reference.api.doc-site-options> for the full surface, and [Positioning DocSite as a fast path](xref:explanation.positioning.docsite-positioning) for what the template hard-codes.

</Step>
<Step StepNumber="3">

**See the registration-only state**

At this point `AddDocSite` is wired but `UseDocSite` hasn't been called yet. The host builds, but the middleware stack is still the ASP.NET default. The `await app.RunAsync()` call is a placeholder that the next section replaces.

```csharp:xmldocid,bodyonly,usings
M:DocSiteScaffoldExample.Stage2.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` returns a default ASP.NET response — the DocSite middleware is registered in DI but not mounted in the pipeline

</Checkpoint>

---

## 3. Mount the DocSite middleware

`UseDocSite` is the middleware counterpart to `AddDocSite` — one call mounts locale routing, antiforgery, static files, Razor component routing, MonorailCSS, SPA navigation, and core Pennington middleware in the correct order.

<Steps>
<Step StepNumber="1">

**Call `UseDocSite` after `Build()`**

This single call mounts the entire DocSite middleware stack. The Razor `Pages.razor` component owns the `/{*fileName:nonfile}` route and resolves pages through `ContentResolver`.

```csharp
app.UseDocSite();
```

</Step>
<Step StepNumber="2">

**Swap `RunAsync` for `RunDocSiteAsync`**

`RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves pages live in development and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>` — one code path for both modes.

```csharp
await app.RunDocSiteAsync(args);
```

</Step>
<Step StepNumber="3">

**See the fully-wired host**

The canonical final shape has three calls that match `Program.cs` verbatim: `AddDocSite`, `UseDocSite`, `RunDocSiteAsync`.

```csharp:xmldocid,bodyonly,usings
M:DocSiteScaffoldExample.Stage3.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The DocSite layout renders: left sidebar, header with site title, search affordance, dark-mode toggle, GitHub icon linking to `GitHubUrl`, and the footer HTML from `FooterContent`

</Checkpoint>

---

## 4. Map content to areas

`DocSiteOptions.Areas` is a list of `ContentArea(Label, Slug)` pairs. Each slug binds a top-level folder under `ContentRootPath` to a URL prefix and to its own sidebar tab.

<Steps>
<Step StepNumber="1">

**Review the `ContentArea` contract**

`ContentArea` has two fields: a human-readable label that appears in the area selector, and a slug that matches the folder name and URL prefix. The order of entries in `Areas` drives the order of tabs in the sidebar.

```csharp
public record ContentArea(string Label, string Slug);
```

</Step>
<Step StepNumber="2">

**Create the area folders**

Under `Content/`, create two folders — `guides/` and `reference/` — each with an `index.md`. The `guides` slug in `DocSiteOptions.Areas` binds `Content/guides/` to the `/guides/` URL prefix and to the Guides sidebar tab. The `reference` slug works the same way.

```text:path
examples/DocSiteScaffoldExample/Content/guides/index.md
```

```text:path
examples/DocSiteScaffoldExample/Content/reference/index.md
```

</Step>
<Step StepNumber="3">

**Confirm the two-area `Areas` list**

The `Areas` block in the fully-wired host has exactly two `ContentArea` entries. The sidebar only shows the area selector when more than one area is configured, so with both entries in place the tab switcher appears for the first time.

</Step>
</Steps>

<Checkpoint>

- Visit `http://localhost:5000/guides/` — the Guides index page renders with the Guides tab selected in the sidebar
- Visit `http://localhost:5000/reference/` — the Reference index page renders, the Reference tab is now selected, and the sidebar TOC filters to the Reference area only

</Checkpoint>

---

## 5. Give the root `/` a landing page

With `Areas` configured, the URL `/` sits **outside** every area — it is not a default redirect into the first area, and the area selector shows no active tab there. To make `/` render something other than a 404, drop a markdown file at `Content/index.md` (next to the area folders, not inside them).

<Steps>
<Step StepNumber="1">

**Author `Content/index.md`**

Use the same `DocSiteFrontMatter` shape as any other page. The page resolves through the same content pipeline as area pages — the only thing that makes it the root is its location at `Content/index.md`.

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
- DocSite is a fast-path template — for the knobs it hard-codes, see [Positioning DocSite as a fast path](xref:explanation.positioning.docsite-positioning).
