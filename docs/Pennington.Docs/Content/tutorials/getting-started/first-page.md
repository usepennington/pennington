---
title: "Serve markdown through Blazor Pages"
description: "Stand up a Pennington site whose markdown is served through a Blazor Server `@page` catch-all — the natural shape for a real app."
sectionLabel: "Getting Started with Pennington"
order: 2
tags:
  - blazor
  - razor
  - routing
  - markdown
uid: tutorials.getting-started.first-page
---

By the end of this tutorial a runnable ASP.NET project — `MyBlazorPenningtonSite` — serves markdown from `Content/` through a Blazor Server `@page "/{*Path}"` catch-all at `http://localhost:5000/`. The previous tutorial used a hand-rolled `MapGet`; this one swaps it for the production-shape Blazor catch-all a real app stays in.

## Prerequisites

- .NET 11 SDK installed
- (Optional) Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site) — this tutorial repeats its `dotnet new web` + Pennington package + `<LangVersion>preview</LangVersion>` bootstrap

The finished code for this tutorial lives in [`examples/GettingStartedBlazorPagesExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedBlazorPagesExample). The DocSite template pre-wires this same Blazor shape for documentation sites — see <xref:tutorials.docsite.scaffold> if that is exactly what you are building.

---

## 1. Set up the project shell

Start from an empty ASP.NET web project and add the Pennington package. No Pennington code yet — the shell `Program.cs` stays untouched until section 2.

<Steps>
<Step StepNumber="1">

**Create the web project**

Run these two commands in a working folder. The `web` template produces a minimal top-level-statement `Program.cs` that returns `Hello from ASP.NET.` — the starting shape we'll replace in the next section.

```bash
dotnet new web -n MyBlazorPenningtonSite
cd MyBlazorPenningtonSite
```

</Step>
<Step StepNumber="2">

**Add the Pennington package and opt into C# preview**

Add the Pennington package so the `AddPennington` extension method resolves, then edit the csproj to set `<LangVersion>preview</LangVersion>` (Pennington uses C# 15 union types, still a preview language feature in the .NET 11 SDK).

```bash
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
    <PackageReference Include="Pennington" Version="<?# PackageVersion /?>" /> <!-- [!code ++] -->
  </ItemGroup> <!-- [!code ++] -->
</Project>
```

> [!IMPORTANT]
> Pennington is in alpha — check NuGet for the current prerelease and pin every `Pennington.*` package to that same version.

</Step>
<Step StepNumber="3">

**Create `Content/index.md`**

Create a `Content/` folder beside `Program.cs` and add `index.md`. The catch-all you wire up in the next section serves anything under `Content/` — this is the file `/` will resolve to.

```markdown:symbol
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

## 2. Wire Pennington, Blazor, and the markdown page

Replace the `Program.cs` body with the host below, then add three Razor files: a `_Imports.razor` for shared `@using` lines, an `App.razor` root component that owns the document shell, and a `MarkdownPage.razor` catch-all that renders any URL to a markdown file. Two service registrations (`AddPennington` for the content pipeline, `AddRazorComponents` for Blazor SSR) and three middleware calls (`UsePennington`, `UseAntiforgery`, `MapRazorComponents<App>()`) are all the host needs.

> [!IMPORTANT]
> `app.UsePennington()` must run before `app.MapRazorComponents<App>()`. The Blazor catch-all `@page "/{*Path}"` would otherwise swallow Pennington's redirect, sitemap, and llms.txt routes.

<Steps>
<Step StepNumber="1">

**Replace `Program.cs`**

```csharp:symbol
examples/GettingStartedBlazorPagesExample/Program.cs
```

</Step>
<Step StepNumber="2">

**Add `_Imports.razor` at the project root**

`_Imports.razor` provides the `@using` set every `.razor` file in the project sees.

```razor:symbol
examples/GettingStartedBlazorPagesExample/_Imports.razor
```

</Step>
<Step StepNumber="3">

**Add `Components/App.razor`**

`App.razor` is the root component `MapRazorComponents<App>()` mounts. It owns the entire HTML document — `<!DOCTYPE>`, `<html>`, `<head>` (with `<HeadOutlet>` so each routed page's `<PageTitle>` flows in), and `<body>`. The `<Router>` inside `<body>` scans the assembly for `@page` components and routes each request to the matching one.

```razor:symbol
examples/GettingStartedBlazorPagesExample/Components/App.razor
```

</Step>
<Step StepNumber="4">

**Add `Components/Pages/MarkdownPage.razor`**

`MarkdownPage.razor` is the `@page "/{*Path}"` catch-all. Blazor binds the request path to the `Path` parameter; the component asks `IPageResolver` to resolve that URL to a rendered page and injects the HTML via `(MarkupString)`. It's the same `IPageResolver` the `MapGet` host used in the previous tutorial — only the call site has moved into a component.

```razor:symbol
examples/GettingStartedBlazorPagesExample/Components/Pages/MarkdownPage.razor
```

</Step>
</Steps>

<Checkpoint>

- `dotnet run` and visit `http://localhost:5000/` — the page renders `Content/index.md`.
- View source. The `<title>` and `<h1>` both pull from `index.md`'s front-matter `title:`.

</Checkpoint>

---

## 3. Add a second markdown file

The file-path-to-URL convention is unchanged by routing through Blazor. Pennington's file watcher picks up new files in `Content/` while the host runs — no restart, no router-table edit.

<Steps>
<Step StepNumber="1">

**Add `Content/about.md`**

Leave `dotnet run` going from the previous section and drop this file in.

```markdown:symbol
examples/GettingStartedBlazorPagesExample/Content/about.md
```

</Step>
<Step StepNumber="2">

**Navigate to `/about`**

Open `http://localhost:5000/about` in the browser. The catch-all serves the new file on the first request — no restart needed.

</Step>
</Steps>

<Checkpoint>

- Visit `/about` — the page renders, served through the same catch-all as `/`.

</Checkpoint>

---

## Summary

- A Pennington host plus a Blazor Server router is two service registrations (`AddPennington`, `AddRazorComponents`) and three middleware calls (`UsePennington`, `UseAntiforgery`, `MapRazorComponents<App>()`).
- `app.UsePennington()` must run before `app.MapRazorComponents<App>()` — the catch-all would otherwise swallow Pennington's redirect, sitemap, and llms.txt routes.
- A single `@page "/{*Path}"` component (`MarkdownPage.razor`) handles every URL, resolves it through `IPageResolver`, and injects the rendered HTML via `(MarkupString)`.
- The file-path-to-URL convention from the markdown pipeline still holds — adding or renaming a `.md` file under `Content/` is enough.
