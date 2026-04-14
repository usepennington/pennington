---
title: "Create your first Pennington site"
description: "Bootstrap a minimal ASP.NET host with AddPennington and UsePennington, point it at a folder of markdown, run it in dev mode with hot reload, and verify a page renders with its front matter."
section: "getting-started"
order: 10
tags: []
uid: tutorials.getting-started.first-site
isDraft: true
search: false
llms: false
---

> **In this page.** Bootstrapping a minimal ASP.NET host with `AddPennington` + `UsePennington`, pointing `ContentRootPath` at a folder of markdown, running in dev mode with hot reload, and verifying a page renders with front matter.
>
> **Not in this page.** The DocSite template, styling, or deployment — those arrive in later tutorials.

## What you'll do

- **Artifact:** a running ASP.NET site at `http://localhost:5000` that renders a markdown file from a `Content/` folder, with its front-matter `Title` shown on the page and the rest of the file converted to HTML.
- **Skill:** you'll know how to wire `AddPennington` + `UsePennington`, register a markdown content source with `AddMarkdownContent<BlogFrontMatter>`, and use `dotnet watch` for hot reload.

## Prerequisites

- Bullets: .NET 11 SDK installed (verified by `dotnet --version` reporting 11.x).
- Bullets: a shell and text editor / IDE of choice — no prior Pennington knowledge required.
- Bullets: no prior tutorials — this is the first one.
- Pointer line: the finished code lives in [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample).

---

## 1. Create an empty web project and add the Pennington packages

- Unit intent: get a blank `WebApplication` host on disk with references to `Pennington` and `Pennington.MonorailCss` so the next unit can wire services.
- Uses `dotnet new web -n MyFirstPenningtonSite` and `dotnet add package Pennington` / `dotnet add package Pennington.MonorailCss` — no custom code yet.
- Keep the generated `Program.cs` untouched at this point; unit 2 replaces it wholesale.

### Step 1.1 — Scaffold the project

- Run `dotnet new web -n MyFirstPenningtonSite` then `cd MyFirstPenningtonSite`.
- Single command, single outcome; no decisions.

### Step 1.2 — Add the packages

- Run `dotnet add package Pennington` and `dotnet add package Pennington.MonorailCss`.
- Mention that `Pennington.MonorailCss` provides the default stylesheet Pennington's rendered HTML expects; we will wire it in unit 2.

### Checkpoint — a buildable empty host

- Bullet: `dotnet build` succeeds.
- Bullet: the project folder contains `Program.cs`, `MyFirstPenningtonSite.csproj`, and the two new package references visible in the `.csproj`.

---

## 2. Wire `AddPennington` and `UsePennington`

- Unit intent: replace the scaffolded `Program.cs` with the canonical Pennington startup — register services, register one markdown source, call `UsePennington`, then hand off to `RunOrBuildAsync`.
- Introduces `PenningtonOptions.SiteTitle`, `PenningtonOptions.SiteDescription`, `PenningtonOptions.ContentRootPath`, and `PenningtonOptions.AddMarkdownContent<TFrontMatter>(Action<MarkdownContentOptions>)`.
- Uses the built-in `Pennington.FrontMatter.BlogFrontMatter` record so the reader does not yet have to author a custom front-matter type.
- Ends with a `RunOrBuildAsync(args)` call — the same entry point used later for static publish.

### Step 2.1 — Replace `Program.cs`

- Show the full startup file from `MinimalExample/Program.cs` as a raw-file fence (it is under 35 lines).
- Bullet walkthrough after the fence: `AddRazorComponents`, `AddPennington(...)` with `ContentRootPath = "Content"`, `AddMarkdownContent<BlogFrontMatter>` with `ContentPath = "Content"` and empty `BasePageUrl`, `AddMonorailCss`, `AddTransient<ContentHelper>`, middleware order (`UseAntiforgery` → `MapStaticAssets` → `MapRazorComponents<App>` → `UseMonorailCss` → `UsePennington`), and `RunOrBuildAsync(args)`.
- Callout: flag the known-stale name — there is no `WithMarkdownContentService<T>`; the real API is `penn.AddMarkdownContent<TFrontMatter>(md => ...)`.

```csharp file="examples/MinimalExample/Program.cs"
```

- Fence slot: raw file. Example project `MinimalExample`. Demonstrates the smallest possible startup that turns on Pennington end-to-end.

### Step 2.2 — Add the Razor host components

- Bullet: create `Components/App.razor`, `Components/Routes.razor`, `Components/Layout/MainLayout.razor`, `Components/Layout/Home.razor`, `Components/Layout/Pages.razor` by copying from `examples/MinimalExample/Components/`.
- Bullet: `Home.razor` lists every discovered page via the injected `ContentHelper`; `Pages.razor` renders a single page by URL.
- Bullet: these Razor files are the minimum presentation layer — unit 4 verifies they render the markdown you write in unit 3.

```razor file="examples/MinimalExample/Components/Layout/Pages.razor"
```

- Fence slot: raw file. Example project `MinimalExample`. Demonstrates a catch-all Razor page that calls `ContentHelper.GetPageByUrlAsync` and writes the HTML with `(MarkupString)`.

### Checkpoint — the host starts

- Bullet: `dotnet build` succeeds.
- Bullet: `dotnet run` starts a listener on `http://localhost:5000` (look for the `Now listening on:` line in the console).
- Bullet: visiting `/` returns "Not found" for now — that is expected because `Content/` is empty. Unit 3 fixes it.

---

## 3. Add a `Content/` folder with one markdown page

- Unit intent: create the folder `ContentRootPath` points at, drop in a single markdown file with YAML front matter, and confirm Pennington discovers it.
- Teaches the minimum front-matter shape — `title:` is the only required key; `description`, `date`, `tags`, and `isDraft` come along with `BlogFrontMatter`.
- No mention of custom capability interfaces, localization, or draft handling rules beyond "`isDraft: false` means published."

### Step 3.1 — Create `Content/index.md`

- Bullet: create a folder named `Content` at the project root (matches the `ContentRootPath` from step 2.1).
- Bullet: add `Content/index.md` with `title:`, `description:`, `date:`, `tags:`, and `isDraft: false`, followed by a short markdown body that includes a heading and a fenced code block so the reader can see highlighting work in unit 4.

```markdown file="examples/MinimalExample/Content/index.md"
```

- Fence slot: raw file. Example project `MinimalExample`. Demonstrates the front-matter shape matching `BlogFrontMatter` and shows a fenced `csharp` code block that the Pennington highlighter will color.

### Checkpoint — the file is on disk

- Bullet: `Content/index.md` exists at the project root.
- Bullet: the front matter starts and ends with `---` on its own line.
- Bullet: `isDraft: false` is present (drafts are skipped by the pipeline, which would make verification fail).

---

## 4. Run in dev mode and verify the page renders

- Unit intent: start the site under `dotnet watch`, visit the URL, see the title from front matter as the `<h1>` and the body converted to HTML, then edit the markdown file and watch the browser reload.
- Introduces live reload: in dev mode the browser reconnects to Pennington's reload endpoint and refreshes when content changes.
- Reinforces that `ContentHelper` is a thin wrapper around the discover/parse/render path Pennington exposes.

### Step 4.1 — Start the watch loop

- Bullet: run `dotnet watch` from the project root.
- Bullet: watch for the `Now listening on:` and `Hot reload started` lines in the console.
- Bullet: open `http://localhost:5000/` in a browser.

### Step 4.2 — Inspect the rendered page

- Bullet: the home page lists the discovered pages (one entry — the `index` page is filtered out of the listing in `Home.razor`; reader will instead navigate directly to see content).
- Bullet: navigate to `/` (the file `Content/index.md` maps to the root URL because `BasePageUrl = ""`). The `<h1>` shows "Welcome to My Content Site" from `title:` in the front matter. The body is converted to HTML with a highlighted code block.
- Bullet: point at the `ContentHelper.GetPageByUrlAsync` method as the code path that did the rendering.

```csharp:xmldocid
M:MinimalExample.ContentHelper.GetPageByUrlAsync(System.String)
```

- Fence slot: method body. Example project `MinimalExample`. Demonstrates how one file is rendered on demand through Pennington's discover, parse, and render pipeline.

### Step 4.3 — Edit the markdown and watch hot reload

- Bullet: change the `title:` in `Content/index.md` to something new and save.
- Bullet: the browser tab reloads automatically; the new `<h1>` appears without restarting `dotnet watch`.
- Bullet: note that Pennington's live-reload WebSocket is doing the refresh.

### Checkpoint — the site is alive

- Bullet: `http://localhost:5000/` renders the title from front matter.
- Bullet: editing `Content/index.md` and saving refreshes the page without a manual reload.
- Bullet: the console shows no warnings — the page is discovered, parsed, and rendered cleanly.

---

## Summary

- You have a running Pennington site that discovers markdown under `Content/` and renders it through the same pipeline used for production static builds.
- You can wire `AddPennington` with a `ContentRootPath` and at least one `AddMarkdownContent<TFrontMatter>` source.
- You can run `dotnet watch` for hot reload and confirm changes land in the browser without restarting.
- You can trace how a page moves through Pennington's discover, parse, and render steps in a minimal host.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
