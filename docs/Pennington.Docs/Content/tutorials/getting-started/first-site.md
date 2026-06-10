---
title: "Create your first Pennington site"
description: "Stand up a minimal ASP.NET host that serves a single markdown page through the Pennington content pipeline."
uid: tutorials.getting-started.first-site
order: 1
sectionLabel: "Getting Started with Pennington"
tags: [getting-started, hosting, markdown, pipeline]
---

By the end of this tutorial a runnable ASP.NET project — `MyFirstPenningtonSite` — serves `Content/index.md` as HTML at `http://localhost:5000/`, with the front-matter `title` appearing in both the `<title>` tag and the page's `<h1>`. The next tutorial swaps the bare `MapGet` for a Blazor Server catch-all.

## Prerequisites

Pennington's published packages target .NET 10, so the stable .NET 10 SDK is all you need to build a site — no preview language flag. The .NET 11 beta SDK is an opt-in that only matters when you extend the pipeline yourself; [the SDK and the union shim](xref:explanation.positioning.sdk-and-the-union-shim) explains when that pays off.

- .NET 10 SDK installed
- A terminal and a text editor or IDE

The finished code for this tutorial lives in [`examples/GettingStartedMinimalSiteExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedMinimalSiteExample).

---

## 1. Scaffold a bare ASP.NET host

First, let's create the project shell Pennington will plug into — no Pennington code yet, a plain web app that returns a string, so the changes in step 2 stand out.

<Steps>
<Step StepNumber="1">

**Create the web project**

Run these two commands in a working folder. The `web` template produces a minimal top-level-statement `Program.cs` — no MVC, no Razor Pages — which is the starting shape we'll edit in the steps ahead.

```bash
dotnet new web -n MyFirstPenningtonSite
cd MyFirstPenningtonSite
```

</Step>
<Step StepNumber="2">

**Add the Pennington package reference**

Add the Pennington package so the `AddPennington` extension method resolves. The backing example in this repo uses a `ProjectReference`, but for a new project this one command is enough.

```bash
dotnet add package Pennington
```

> [!IMPORTANT]
> Pennington is in alpha — check NuGet for the current prerelease and pin every `Pennington.*` package to that same version.

</Step>
<Step StepNumber="3">

**Run the bare host**

The `dotnet new web` template produces a `Program.cs` with a single `MapGet` returning `"Hello World!"`. Change that string to `"Hello from ASP.NET."` — a value the template never writes, so seeing it in the browser proves you're running your own edited code and not a cached default — then run the host to confirm the shell works before Pennington takes over.

```bash
dotnet run --urls http://localhost:5000
```

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` returns the literal text `Hello from ASP.NET.`
- Stop the process with `Ctrl+C` before continuing.

</Checkpoint>

---

## 2. Register Pennington and point it at markdown

Now let's swap the pass-through string endpoint for the Pennington content pipeline: `AddPennington` registers the core services, `AddMarkdownContent<DocFrontMatter>` names the markdown folder (see [`DocFrontMatter`](xref:reference.api.doc-front-matter)), and the host gains a `ContentRootPath` it will watch for changes.

<Steps>
<Step StepNumber="1">

**Create the Content folder and an index page**

Create a `Content/` folder beside `Program.cs`, then add `index.md` with the contents below. Two things are required: a YAML front-matter block with a `title:` key, and a markdown body.

```markdown:symbol
examples/GettingStartedMinimalSiteExample/Content/index.md
```

</Step>
<Step StepNumber="2">

**Wire `AddPennington` in `Program.cs`**

Replace the body of `Program.cs` with the service-registration block below, which walks through `WebApplication.CreateBuilder` → `AddPennington` → `AddMarkdownContent<DocFrontMatter>` → `app.Build()`. The two `using` directives at the top bring in `DocFrontMatter` and the `AddPennington` extension — keep them, or the file won't compile.

```csharp:symbol,bodyonly,imports
examples/GettingStartedMinimalSiteExample/Stage2_AddPennington.cs > Stage2.Run
```

`ContentRootPath` sets the host's base for static files; the `ContentPath` passed to `AddMarkdownContent` is where this particular markdown source reads from — both point at `"Content"` here.

Don't run it yet — the services are registered but nothing serves them. The middleware and the rendering endpoint go in next, and you'll run the finished host then.

</Step>
</Steps>

## 3. Wire the middleware and render the page

Now we mount the middleware chain with `app.UsePennington()`, add a `MapGet` that hands each request to `IPageResolver`, and hand control to [`RunOrBuildAsync`](xref:reference.host.cli) — which uses the same app for development and static output with no code change. `IPageResolver` is the one service you need to turn a URL into a rendered page: it walks the registered content sources, parses the markdown that matches, and renders it. A Razor page would normally call it, but a `MapGet` keeps the wiring visible in one place for this tutorial.

<Steps>
<Step StepNumber="1">

**Add `UsePennington`, `RunOrBuildAsync`, and the rendering endpoint**

Update `Program.cs` to match the complete file below. `UsePennington` installs static files, the response-processing middleware, live reload, and auto-registered endpoints like `/sitemap.xml`; `RunOrBuildAsync` serves live when called with no args and generates static HTML when passed `-- build`; the `MapGet` asks `IPageResolver` to resolve the request to a rendered page, then returns its HTML (or a 404 when nothing matches).

```csharp:symbol
examples/GettingStartedMinimalSiteExample/Program.cs
```

This `MapGet` is deliberately minimal. `IPageResolver` collapses the discover → parse → render flow into one call; if you want to see the four-stage union pipeline it runs underneath, read [The content pipeline and union types](xref:explanation.core.content-pipeline). The next tutorial replaces this `MapGet` with a Blazor Server `@page` catch-all, the form a real Pennington app stays in.

</Step>
</Steps>

<Checkpoint>

That's the working site. `dotnet run --urls http://localhost:5000` serves live, and `http://localhost:5000/` returns HTML whose `<title>` element and top-level `<h1>` both read `Welcome to your first Pennington site`, pulled straight from `Content/index.md`'s front matter.

- Run `dotnet run --urls http://localhost:5000` from the project folder.
- Open `http://localhost:5000/` and confirm the page title in the browser tab reads `Welcome to your first Pennington site`.
- View source and confirm the same string appears inside the `<title>` tag and the article's `<h1>`.

</Checkpoint>

The rendered page is plain unstyled HTML — Times-New-Roman serif, default browser margins, blue underlined links. That is on purpose: this host wires only the content pipeline, not the CSS layer. Replacing the bare `MapGet` with a Blazor Server `@page` catch-all is the next tutorial: [Serve markdown through Blazor Pages](xref:tutorials.getting-started.first-page).

---

## 4. Verify dev-mode hot reload

Let's confirm that `UsePennington`'s file-watcher and live-reload WebSocket are working: with `dotnet run --urls http://localhost:5000` still serving, edit the markdown file and watch the browser reload without touching the terminal.

<Steps>
<Step StepNumber="1">

**Edit the front-matter title**

Leave `dotnet run --urls http://localhost:5000` serving and keep `http://localhost:5000/` open in the browser. Open `Content/index.md` and change the `title:` value to something recognizable — for example `title: Hello, Pennington` — then save. The browser tab updates on its own within a second. If it doesn't, hard-refresh once; stale HTML may be cached from before the edit.

</Step>
</Steps>

<Checkpoint>

Without any terminal input, the browser tab updates to show the new title in both the `<h1>` and the tab title. The running console logs a file-change line naming `Content/index.md`.

- Edit `Content/index.md`'s `title:` field and save.
- The browser tab title and page heading update to match — no manual refresh needed.
- The terminal logs the change.

</Checkpoint>

---

> [!NOTE]
> Wiring the host yourself is the normal path, and the next tutorials build straight on it. If your site is a plain documentation site, the DocSite template pre-wires this same shape — sidebar, search, Razor catch-all — in one `AddDocSite` call; <xref:tutorials.docsite.scaffold> picks up there.

## Summary

- An ASP.NET host now serves a markdown page end-to-end through `AddPennington` and `UsePennington`.
- The content pipeline reads from a folder of markdown through `ContentRootPath` plus `AddMarkdownContent<DocFrontMatter>`.
- `RunOrBuildAsync` means the same host generates a static site on `dotnet run -- build` with no code change.
- A front-matter `title:` flows from YAML into the rendered `<h1>`, and dev-mode hot reload re-renders on save.
