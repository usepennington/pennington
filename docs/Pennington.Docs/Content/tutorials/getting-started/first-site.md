---
title: "Create your first Pennington site"
description: "Stand up a minimal ASP.NET host that serves a single markdown page through the Pennington content pipeline."
uid: tutorials.getting-started.first-site
order: 101010
sectionLabel: "Getting Started with Pennington"
tags: [getting-started, hosting, markdown, pipeline]
---

By the end of this tutorial a runnable ASP.NET project — `MyFirstPenningtonSite` — serves `Content/index.md` as HTML at `http://localhost:5000/`, with the front-matter `title` appearing in both the `<title>` tag and the page's `<h1>`.

The tutorial covers how to wire `AddPennington`, `UsePennington`, and `RunOrBuildAsync` into a minimal web host — the same foundation underneath every Pennington-powered site, whether DocSite, BlogSite, or hand-rolled. For when a bundled template is the faster path instead, [Positioning DocSite as a fast path](xref:explanation.core.docsite-positioning) walks through the tradeoffs before you pick.

## Prerequisites

Pennington targets .NET 11 with C# 15 union types. On .NET 10 the build reports language-version errors, so use the .NET 11 SDK before starting.

- .NET 11 SDK installed (preview build — `dotnet --version` reports `11.0.*`)
- A terminal and a text editor or IDE that understands C# 15

The finished code for this tutorial lives in [`examples/GettingStartedMinimalSiteExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedMinimalSiteExample).

---

## 1. Scaffold a bare ASP.NET host

First, let's create the project shell Pennington will plug into — no Pennington code yet, a plain web app that returns a string, so the changes in step 2 stand out.

<Steps>
<Step StepNumber="1">

**Create the web project**

Run these two commands in a working folder. The `web` template produces a minimal top-level-statement `Program.cs` — no MVC, no Razor Pages — which is the starting shape we'll edit in the steps ahead.

```text
dotnet new web -n MyFirstPenningtonSite
cd MyFirstPenningtonSite
```

</Step>
<Step StepNumber="2">

**Add the Pennington package reference**

Add the Pennington package so the `AddPennington` extension method resolves. The backing example in this repo uses a `ProjectReference`, but for a new project this one command is enough.

```text
dotnet add package Pennington
```

> [!IMPORTANT]
> Pennington is in alpha — check NuGet for the current prerelease and pin every `Pennington.*` package to that same version.

</Step>
<Step StepNumber="3">

**Opt into C# preview language features**

Pennington is built on C# 15 union types, and the samples in this tutorial use `union` pattern matching. The .NET 11 preview SDK does not enable preview language features by default, so compiling against Pennington without the opt-in produces `error CS8652: The feature 'unions' is currently in Preview and *unsupported*`. Edit the csproj to add `<LangVersion>preview</LangVersion>`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Pennington" Version="0.1.0-alpha.0.20" />
  </ItemGroup>
</Project>
```

For a multi-project host, dropping a `Directory.Build.props` at the solution root with the same `<LangVersion>preview</LangVersion>` property keeps every project aligned.

</Step>
<Step StepNumber="4">

**Confirm the bare host runs**

Before adding Pennington, `Program.cs` looks like this — a plain `WebApplication` with a single `MapGet` that returns a string. This is the baseline we'll build on.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage1.Run(System.String[])
```

</Step>
</Steps>

### Checkpoint — Bare host responds

The browser shows the literal text `Hello from ASP.NET.` — no markdown involved at this stage. If HTML output appears instead, double-check that the running project is the bare scaffold from step 1.4, not a later stage.

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page body reads `Hello from ASP.NET.`
- Stop the process with `Ctrl+C` before continuing.

---

## 2. Register Pennington and point it at markdown

Now let's swap the pass-through string endpoint for the Pennington content pipeline: `AddPennington` registers the core services, `AddMarkdownContent<DocFrontMatter>` names the markdown folder, and the host gains a `ContentRootPath` it will watch for changes.

<Steps>
<Step StepNumber="1">

**Create the Content folder and an index page**

Create a `Content/` folder beside `Program.cs`, then add `index.md` with the contents below. Two things are required: a YAML front-matter block with a `title:` key, and a markdown body.

```markdown:path
examples/GettingStartedMinimalSiteExample/Content/index.md
```

</Step>
<Step StepNumber="2">

**Wire `AddPennington` in `Program.cs`**

Replace the body of `Program.cs` with the service-registration block below, which walks through `WebApplication.CreateBuilder` → `AddPennington` → `AddMarkdownContent<DocFrontMatter>` → `app.Build()`.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage2.Run(System.String[])
```

`ContentRootPath` sets the host's base for static files; the `ContentPath` passed to `AddMarkdownContent` is where this particular markdown source reads from — both point at `"Content"` here.

</Step>
</Steps>

### Checkpoint — Services resolve

`dotnet build` succeeds with no errors. The host does **not** yet render the markdown page — stage 2 only registers services, so running it at this point returns a 404 for `/`. That's expected; hold off until step 3 adds the middleware.

- Run `dotnet build` and confirm the build succeeds with no errors.
- Do not run the site yet — the middleware arrives in the next step.

---

## 3. Wire the middleware and render the page

Now we mount the middleware chain with `app.UsePennington()`, add a `MapGet` that hands each request to the content pipeline, and hand control to `RunOrBuildAsync` — the same host that serves live today will generate static HTML tomorrow with no code change.

<Steps>
<Step StepNumber="1">

**Add `UsePennington` and `RunOrBuildAsync`**

Update `Program.cs` to match the snapshot below.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

`UsePennington` installs static files, the response-processing middleware, live reload, and auto-registered endpoints like `/sitemap.xml`; `RunOrBuildAsync` serves live when called with no args and generates static HTML when passed `-- build`.

</Step>
<Step StepNumber="2">

**Add the page-rendering endpoint**

The stage-3 snapshot registered services and middleware but didn't add a rendering endpoint. Here's the complete final `Program.cs`. It adds a `MapGet` that walks the `IContentService` set, finds the matching markdown, and returns rendered HTML.

```csharp:path
examples/GettingStartedMinimalSiteExample/Program.cs
```

This `MapGet` is deliberately minimal — in the DocSite and BlogSite tutorials the template ships its own Razor layout and routing, so this endpoint falls away once we move past the bare host.

</Step>
</Steps>

### Checkpoint — The page renders with its front-matter title

That's the working site. `dotnet run` serves live, and `http://localhost:5000/` returns HTML whose `<title>` element and top-level `<h1>` both read `Welcome to your first Pennington site`, pulled straight from `Content/index.md`'s front matter.

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page title in the browser tab reads `Welcome to your first Pennington site`.
- View source and confirm the same string appears inside the `<title>` tag and the article's `<h1>`.

---

## 4. Verify dev-mode hot reload

Let's confirm that `UsePennington`'s file-watcher and live-reload WebSocket are working: restart under `dotnet watch`, edit the markdown file, and watch the browser reload without touching the terminal.

<Steps>
<Step StepNumber="1">

**Run under `dotnet watch`**

Stop the previous `dotnet run` with `Ctrl+C`, then start the watcher. Live reload is gated on the `DOTNET_WATCH` environment variable, which `dotnet watch` sets automatically — no manual setup required. Leave `http://localhost:5000/` open in the browser.

```text
dotnet watch
```

</Step>
<Step StepNumber="2">

**Edit the front-matter title**

Open `Content/index.md` and change the `title:` value to something recognizable — for example `title: Hello, Pennington` — then save. The browser tab updates on its own within a second. If it doesn't, hard-refresh once; stale HTML may be cached from the earlier `dotnet run`.

</Step>
</Steps>

### Checkpoint — Live reload fires

Without any terminal input, the browser tab updates to show the new title in both the `<h1>` and the tab title. The `dotnet watch` console logs a file-change line naming `Content/index.md`.

- Edit `Content/index.md`'s `title:` field and save.
- The browser tab title and page heading update to match — no manual refresh needed.
- `dotnet watch` logs the change in the terminal.

---

## Summary

- An ASP.NET host now serves a markdown page end-to-end through `AddPennington` and `UsePennington`.
- The content pipeline reads from a folder of markdown through `ContentRootPath` plus `AddMarkdownContent<DocFrontMatter>`.
- `RunOrBuildAsync` means the same host generates a static site on `dotnet run -- build` with no code change.
- A front-matter `title:` flows from YAML into the rendered `<h1>`, and dev-mode hot reload re-renders on save.
