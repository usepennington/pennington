---
title: "Create your first Pennington site"
description: "Stand up a minimal ASP.NET host that serves a single markdown page through the Pennington content pipeline."
uid: tutorials.getting-started.first-site
order: 101010
sectionLabel: "Getting Started with Pennington"
tags: [getting-started, hosting, markdown, pipeline]
---

By the end of this tutorial you'll have a runnable ASP.NET project — `MyFirstPenningtonSite` — that serves `Content/index.md` as HTML at `http://localhost:5000/`, with the front-matter `title` appearing in both the `<title>` tag and the page's `<h1>`.

You'll know how to wire `AddPennington`, `UsePennington`, and `RunOrBuildAsync` into a minimal web host — the same foundation underneath every Pennington-powered site, whether DocSite, BlogSite, or hand-rolled.

## Prerequisites

Pennington targets .NET 11 with C# 15 union types, so readers on .NET 10 will hit language-version errors — make sure you're on the right SDK before starting.

- .NET 11 SDK installed (preview build — `dotnet --version` should report `11.0.*`)
- A terminal and a text editor or IDE that understands C# 15

The finished code for this tutorial lives in [`examples/GettingStartedMinimalSiteExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedMinimalSiteExample).

---

## 1. Scaffold a bare ASP.NET host

First, let's create the project shell Pennington will plug into — no Pennington code yet, a plain web app that returns a string, so the changes in step 2 stand out clearly.

### Step 1.1 — Create the web project

Run these two commands in a working folder. The `web` template produces a minimal top-level-statement `Program.cs` — no MVC, no Razor Pages — which is exactly the starting shape we'll edit in the steps ahead.

```text
dotnet new web -n MyFirstPenningtonSite
cd MyFirstPenningtonSite
```

### Step 1.2 — Add the Pennington package reference

Add the Pennington package so the `AddPennington` extension method resolves. The backing example in this repo uses a `ProjectReference`, but for your own project this one command is all you need.

```text
dotnet add package Pennington
```

### Step 1.3 — Confirm the bare host runs

Before adding Pennington, your `Program.cs` should look like this — a plain `WebApplication` with a single `MapGet` that returns a string. This is the baseline we'll build on.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage1.Run(System.String[])
```

### Checkpoint — Bare host responds

You should see the literal text `Hello from ASP.NET.` in the browser — no markdown involved at this stage. If you see HTML output, double-check you're running the bare scaffold from step 1.3, not a later stage.

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page body reads `Hello from ASP.NET.`
- Stop the process with `Ctrl+C` before continuing.

---

## 2. Register Pennington and point it at markdown

Now let's swap the pass-through string endpoint for the Pennington content pipeline: `AddPennington` registers the core services, `AddMarkdownContent<DocFrontMatter>` names the markdown folder, and the host gains a `ContentRootPath` it will watch for changes.

### Step 2.1 — Create the Content folder and an index page

Create a `Content/` folder beside `Program.cs`, then add `index.md` with the contents below. Two things are required: a YAML front-matter block with at least a `title:` key, and a markdown body.

```markdown:path
examples/GettingStartedMinimalSiteExample/Content/index.md
```

### Step 2.2 — Wire `AddPennington` in `Program.cs`

Replace the body of `Program.cs` with the service-registration block below, which walks through `WebApplication.CreateBuilder` → `AddPennington` → `AddMarkdownContent<DocFrontMatter>` → `app.Build()`.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage2.Run(System.String[])
```

`ContentRootPath` sets the host's base for static files; the `ContentPath` passed to `AddMarkdownContent` is where this particular markdown source reads from — both point at `"Content"` here.

### Checkpoint — Services resolve

`dotnet build` should succeed with no errors. The host does **not** yet render the markdown page — stage 2 only registers services, so running it at this point returns a 404 for `/`. That's expected; hold off until step 3 adds the middleware.

- Run `dotnet build` and confirm the build succeeds with no errors.
- Do not run the site yet — the middleware arrives in the next step.

---

## 3. Wire the middleware and render the page

Now we mount the middleware chain with `app.UsePennington()`, add a `MapGet` that hands each request to the content pipeline, and hand control to `RunOrBuildAsync` — the same host that serves live today will generate static HTML tomorrow with no code change.

### Step 3.1 — Add `UsePennington` and `RunOrBuildAsync`

Update `Program.cs` to match the snapshot below.

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

`UsePennington` installs static files, the response-processing middleware, live reload, and auto-registered endpoints like `/sitemap.xml`; `RunOrBuildAsync` serves live when called with no args and generates static HTML when passed `-- build`.

### Step 3.2 — Add the page-rendering endpoint

The stage-3 snapshot registered services and middleware but didn't add a rendering endpoint. Here's the complete final `Program.cs` — copy it verbatim. It adds a `MapGet` that walks the `IContentService` set, finds the matching markdown, and returns rendered HTML.

```csharp:path
examples/GettingStartedMinimalSiteExample/Program.cs
```

This `MapGet` is deliberately minimal — in the DocSite and BlogSite tutorials the template ships its own Razor layout and routing, so you won't write this endpoint by hand once you move on from the bare host.

### Checkpoint — The page renders with its front-matter title

That's it — you've got a working site. `dotnet run` serves live, and `http://localhost:5000/` returns HTML whose `<title>` element and top-level `<h1>` both read `Welcome to your first Pennington site`, pulled straight from `Content/index.md`'s front matter.

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page title in the browser tab reads `Welcome to your first Pennington site`.
- View source and confirm the same string appears inside the `<title>` tag and the article's `<h1>`.

---

## 4. Verify dev-mode hot reload

Let's confirm that `UsePennington`'s file-watcher and live-reload WebSocket are working: restart under `dotnet watch`, edit the markdown file, and watch the browser reload without touching the terminal.

### Step 4.1 — Run under `dotnet watch`

Stop the previous `dotnet run` with `Ctrl+C`, then start the watcher. Live reload is gated on the `DOTNET_WATCH` environment variable, which `dotnet watch` sets automatically — you don't set it by hand. Leave `http://localhost:5000/` open in the browser.

```text
dotnet watch
```

### Step 4.2 — Edit the front-matter title

Open `Content/index.md` and change the `title:` value to something recognizable — for example `title: Hello, Pennington` — then save. The browser tab updates on its own within a second. If it doesn't, hard-refresh once; you may have had stale HTML cached from the earlier `dotnet run`.

### Checkpoint — Live reload fires

Without touching the terminal, the browser tab updates to show the new title in both the `<h1>` and the tab title. Check the `dotnet watch` console — it logs a file-change line naming `Content/index.md`.

- Edit `Content/index.md`'s `title:` field and save.
- Watch the browser tab title and page heading update to match — no manual refresh needed.
- Confirm `dotnet watch` logs the change in the terminal.

---

## Summary

- You stood up an ASP.NET host that serves a markdown page end-to-end through `AddPennington` and `UsePennington`.
- You know how to point the content pipeline at a folder of markdown with `ContentRootPath` plus `AddMarkdownContent<DocFrontMatter>`.
- You handed the process to `RunOrBuildAsync`, which means the same host will later generate a static site on `dotnet run -- build` with no code change.
- You watched a front-matter `title:` flow from YAML into the rendered `<h1>`, and confirmed dev-mode hot reload re-renders on save.
