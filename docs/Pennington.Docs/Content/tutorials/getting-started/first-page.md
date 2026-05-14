---
title: "Using Blazor Pages"
description: "Stand up a Pennington site whose markdown is served through a Blazor Server `@page` catch-all — the natural shape for a real app."
sectionLabel: "Getting Started with Pennington"
order: 101020
tags:
  - blazor
  - razor
  - routing
  - markdown
uid: tutorials.getting-started.first-page
---

By the end of this tutorial a runnable ASP.NET project — `MyBlazorPenningtonSite` — serves markdown from `Content/` through a Blazor Server `@page "/{*Path}"` catch-all at `http://localhost:5000/`. The previous tutorial used a hand-rolled `MapGet` so the URL → markdown file → rendered HTML chain stayed visible in one place; that's a fine teaching shape, but it's not the shape a real app stays in. This tutorial spins up the production-shape host from scratch.

## Prerequisites

- .NET 11 SDK installed
- (Optional) Completed [Spin up a minimal Pennington site](xref:tutorials.getting-started.first-site) — this tutorial repeats its `dotnet new web` + Pennington package + `<LangVersion>preview</LangVersion>` bootstrap

The finished code for this tutorial lives in [`examples/GettingStartedBlazorPagesExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedBlazorPagesExample).

> [!NOTE]
> If a custom Blazor host is not what you need, the bundled DocSite template wires this same shape — Blazor catch-all included — out of the box. Skip ahead to <xref:tutorials.docsite.scaffold> when "I just want a docs site" is the goal.

---

## 1. Set up the project shell

Start from an empty ASP.NET web project and add the Pennington package. No Pennington code yet — just the shell `Program.cs` will go into in section 2.

<Steps>
<Step StepNumber="1">

**Create the web project**

Run these two commands in a working folder. The `web` template produces a minimal top-level-statement `Program.cs` that returns `Hello from ASP.NET.` — the starting shape we'll replace in the next section.

```text
dotnet new web -n MyBlazorPenningtonSite
cd MyBlazorPenningtonSite
```

</Step>
<Step StepNumber="2">

**Add the Pennington package and opt into C# preview**

Add the Pennington package so the `AddPennington` extension method resolves, then edit the csproj to set `<LangVersion>preview</LangVersion>` (Pennington uses C# 15 union types, still a preview language feature in the .NET 11 SDK).

```text
dotnet add package Pennington
```

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <LangVersion>preview</LangVersion> <!-- [!code ++] -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup> <!-- [!code ++] -->
    <PackageReference Include="Pennington" Version="0.1.0-alpha.0.20" /> <!-- [!code ++] -->
  </ItemGroup> <!-- [!code ++] -->
</Project>
```

> [!IMPORTANT]
> Pennington is in alpha — check NuGet for the current prerelease and pin every `Pennington.*` package to that same version.

</Step>
<Step StepNumber="3">

**Create `Content/index.md`**

Create a `Content/` folder beside `Program.cs` and add `index.md`. The catch-all you wire up in the next section serves anything under `Content/` — this is the file `/` will resolve to.

```markdown:path
examples/GettingStartedBlazorPagesExample/Content/index.md
```

</Step>
</Steps>

<Checkpoint>

- `dotnet build` succeeds with no errors
- `dotnet run` followed by visiting `http://localhost:5000/` returns the literal text `Hello from ASP.NET.` — the bare web template's response. Pennington takes over in the next section
- Stop the process with `Ctrl+C` before continuing

</Checkpoint>

---

## 2. Wire Pennington and Blazor in `Program.cs`

Replace the `Program.cs` body with the host below. Two service registrations (`AddPennington` for the content pipeline, `AddRazorComponents` for Blazor's static server-side rendering) and three middleware calls (`UsePennington`, `UseAntiforgery`, `MapRazorComponents<App>()`) are all the wiring this host needs.

```csharp:path
examples/GettingStartedBlazorPagesExample/Program.cs
```

A walk-through of the calls:

- `AddPennington` registers the core services and sets `ContentRootPath` to `Content/`, the folder it watches for changes.
- `AddMarkdownContent<DocFrontMatter>` declares one markdown source rooted at `Content/`. Every `.md` file there becomes a discoverable content item.
- `AddRazorComponents` registers Blazor's static server-side rendering — what `MapRazorComponents` needs to actually route to `@page` components.
- `UsePennington` installs the static-files, response-processing, live-reload, and auto-registered endpoints (`/sitemap.xml`, `/llms.txt`).
- `UseAntiforgery` is required because Blazor's routed components opt into antiforgery metadata even when no form ships in the page.
- `MapRazorComponents<App>()` hands routing to Blazor. The next section adds the `App.razor` it points at.

> [!IMPORTANT]
> Endpoint ordering matters. `app.UsePennington()` must run before `app.MapRazorComponents<App>()`. Pennington's middleware registers redirect routes plus the `/sitemap.xml` and `/llms.txt` endpoints; the Blazor catch-all `@page "/{*Path}"` from the next section would swallow those routes if it were mapped first.

<Checkpoint>

- `dotnet build` succeeds
- `dotnet run` and visit `http://localhost:5000/` — the response is a 404. The `App.razor` and `MarkdownPage.razor` that handle every URL arrive in the next section

</Checkpoint>

---

## 3. Add the Blazor router and the markdown page

Three Razor files give Blazor a router, a document shell, and the catch-all `@page` that resolves any URL to a markdown file.

<Steps>
<Step StepNumber="1">

**Add `_Imports.razor` at the project root**

`_Imports.razor` provides the `@using` set every `.razor` file in the project sees. Drop it next to `Program.cs`.

```razor:path
examples/GettingStartedBlazorPagesExample/_Imports.razor
```

</Step>
<Step StepNumber="2">

**Add `Components/App.razor`**

`App.razor` is the root component `MapRazorComponents<App>()` mounts. It owns the entire HTML document in this tutorial: `<!DOCTYPE>`, `<html>`, `<head>` (with `<HeadOutlet>` so each routed page's `<PageTitle>` flows in), and `<body>`. The `<Router>` inside `<body>` scans the assembly for `@page` components and routes each request to the matching one.

```razor:path
examples/GettingStartedBlazorPagesExample/Components/App.razor
```

</Step>
<Step StepNumber="3">

**Add `Components/Pages/MarkdownPage.razor`**

`MarkdownPage.razor` is the `@page "/{*Path}"` catch-all. Blazor binds the request path to the `Path` parameter; the component injects the same `IEnumerable<IContentService>`, `IContentParser`, and `IContentRenderer` the bare-host MapGet from the previous tutorial used, walks them to find the matching markdown, and injects the rendered HTML via `(MarkupString)`.

```razor:path
examples/GettingStartedBlazorPagesExample/Components/Pages/MarkdownPage.razor
```

</Step>
</Steps>

<Checkpoint>

- `dotnet run` and visit `http://localhost:5000/` — the page renders `Content/index.md`
- View source. The `<title>` and `<h1>` both pull from `index.md`'s front-matter `title:`

</Checkpoint>

---

## 4. Add a second markdown file

The file-path-to-URL convention is unchanged by routing through Blazor. Pennington's file watcher picks up new and renamed files in `Content/` while the host runs — no restart, no router-table edit.

<Steps>
<Step StepNumber="1">

**Add `Content/about.md`**

Leave `dotnet run` going from the previous section and drop this file in.

```markdown:path
examples/GettingStartedBlazorPagesExample/Content/about.md
```

</Step>
<Step StepNumber="2">

**Navigate to `/about`**

Open `http://localhost:5000/about` in the browser. The catch-all serves the new file on the first request — no restart needed.

</Step>
<Step StepNumber="3">

**Rename the file to see the URL follow it**

Rename `Content/about.md` to `Content/reach-out.md` and visit `/reach-out`. The watcher re-discovers the file and the catch-all serves it at the new URL. Rename it back to `about.md` before continuing so the next tutorial matches.

</Step>
</Steps>

<Checkpoint>

- Visit `/about` — the page renders, served through the same catch-all as `/`
- Rename `about.md` to `reach-out.md`, hit `/reach-out` — same page, new URL
- Rename it back to `about.md`

</Checkpoint>

---

## Summary

- A Pennington host plus a Blazor Server router is two service registrations (`AddPennington`, `AddRazorComponents`) and three middleware calls (`UsePennington`, `UseAntiforgery`, `MapRazorComponents<App>()`).
- `app.UsePennington()` must run before `app.MapRazorComponents<App>()` — the catch-all would otherwise swallow Pennington's redirect, sitemap, and llms.txt routes.
- A single `@page "/{*Path}"` component (`MarkdownPage.razor`) handles every URL, walks the content pipeline, and injects the rendered HTML via `(MarkupString)`.
- The file-path-to-URL convention from the markdown pipeline still holds — adding or renaming a `.md` file under `Content/` is enough.
