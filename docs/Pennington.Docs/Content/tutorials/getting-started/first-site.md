---
title: "Create your first Pennington site"
description: "Stand up a minimal ASP.NET host that serves a single markdown page through the Pennington content pipeline."
uid: tutorials.getting-started.first-site
order: 101010
sectionLabel: "Getting Started with Pennington"
tags: [getting-started, hosting, markdown, pipeline]
---

> **In this page.** _One sentence paraphrased from the TOC "Covers" line: bootstrapping a minimal ASP.NET host with `AddPennington` + `UsePennington`, pointing `ContentRootPath` at a markdown folder, running in dev mode with hot reload, and verifying a page renders with front matter. Keep it warm and second-person — the reader is arriving._
>
> **Not in this page.** _One sentence paraphrased from the TOC "Does not cover" line: styling, the `AddDocSite` template, and deployment. Link forward to `/tutorials/getting-started/first-page` (adding more pages), `/tutorials/getting-started/styling` (MonorailCSS), and `/tutorials/docsite/scaffold` (the template)._

## What you'll do

_**Artifact** (one sentence): the reader will have a runnable ASP.NET project — `MyFirstPenningtonSite` — that serves a single markdown file (`Content/index.md`) as HTML at `http://localhost:5000/` with its front-matter `title` showing up in both the `<title>` tag and an `<h1>` on the page._

_**Skill** (one sentence): the reader will know how to wire `AddPennington`, `UsePennington`, and `RunOrBuildAsync` into a minimal web host and point them at a folder of markdown, which is the starting shape for every Pennington-powered site — DocSite, BlogSite, or hand-rolled._

## Prerequisites

_Keep the list short. `AddPennington` needs only the standard ASP.NET stack; there are no prior tutorials to gate on since this is the first one. Call out the preview SDK explicitly because Pennington targets .NET 11 with C# 15 union types — readers on .NET 10 will hit language-version errors. No additional tooling is required._

- .NET 11 SDK installed (preview build — `dotnet --version` should report `11.0.*`)
- A terminal and a text editor or IDE that understands C# 15

The finished code for this tutorial lives in [`examples/GettingStartedMinimalSiteExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedMinimalSiteExample).

---

## 1. Scaffold a bare ASP.NET host

_One sentence: the reader creates the project shell that Pennington will later plug into — no Pennington code yet, just a web app that returns a string, so step 2's diff makes the `AddPennington` change visible._

### Step 1.1 — Create the web project

_Direct the reader to run `dotnet new web -n MyFirstPenningtonSite` in a working folder, then `cd` into it. Mention that `dotnet new web` produces the minimal top-level-statement `Program.cs` the rest of the tutorial will edit — no MVC, no Razor Pages. Keep it to two commands, no branching._

```text
dotnet new web -n MyFirstPenningtonSite
cd MyFirstPenningtonSite
```

### Step 1.2 — Add the Pennington package reference

_Tell the reader to add a project reference (or package reference) to Pennington's core library so `AddPennington` resolves. Point out that the backing example uses a `ProjectReference` because it lives inside the repo, but readers outside the repo should use `dotnet add package Pennington`. Keep this deterministic — one command._

```text
dotnet add package Pennington
```

### Step 1.3 — Confirm the bare host runs

_Show the stage-1 snapshot that the reader's `Program.cs` currently matches in spirit: a plain `WebApplication` with a single `MapGet("/")` that returns a string. The code fence embeds the body of `Stage1.Run` from the backing example so the reader sees a known-good baseline before any Pennington wiring._

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage1.Run(System.String[])
```

### Checkpoint — Bare host responds

_Verifiable outcome: `dotnet run` boots, the console prints the listening URL, and `http://localhost:5000/` responds with the literal string "Hello from ASP.NET." — no markdown involved yet. If the reader sees HTML they are on the wrong step._

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page body reads `Hello from ASP.NET.`
- Stop the process with `Ctrl+C` before continuing.

---

## 2. Register Pennington and point it at markdown

_One sentence: the reader swaps the pass-through string endpoint for the Pennington content pipeline — `AddPennington` wires services, `AddMarkdownContent<DocFrontMatter>` names the folder, and the host gains a `ContentRootPath` it will watch._

### Step 2.1 — Create the Content folder and an index page

_Direct the reader to create `Content/index.md` beside `Program.cs` and paste the front-matter + body below. Call out the two required ingredients: the YAML front-matter block with a `title:` key, and a markdown body. The file on disk uses the backing example's canonical page._

```markdown:path
examples/GettingStartedMinimalSiteExample/Content/index.md
```

### Step 2.2 — Wire `AddPennington` in `Program.cs`

_Have the reader replace the body of `Program.cs` with the service-registration block. The embedded stage-2 snippet is exactly the `WebApplication.CreateBuilder` → `AddPennington` → `AddMarkdownContent<DocFrontMatter>` → `app.Build()` sequence. Explain (one sentence, after the fence) that `ContentRootPath` and the per-source `ContentPath` both point at `"Content"` — the former is the host's base for static files, the latter is where this particular markdown source reads from._

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage2.Run(System.String[])
```

### Checkpoint — Services resolve

_Verifiable outcome: `dotnet build` succeeds. The host does **not** yet render the markdown page — stage 2 only registers services. Running it at this point will return 404 for `/`, which is expected. Tell the reader not to run the site yet; step 3 wires the middleware._

- Run `dotnet build` and confirm the build succeeds with no errors.
- Do not run the site yet — the middleware arrives in the next step.

---

## 3. Wire the middleware and render the page

_One sentence: the reader calls `app.UsePennington()` to mount the middleware chain, adds a `MapGet` that hands each request to the content pipeline, and hands control to `RunOrBuildAsync` so the same host will later build static HTML without any code change._

### Step 3.1 — Add `UsePennington` and `RunOrBuildAsync`

_Show the stage-3 snapshot. After the fence, explain in one sentence that `UsePennington` installs static files, the response-processing middleware, live reload, and auto-registered endpoints like `/sitemap.xml`; `RunOrBuildAsync` serves live when called with no args and generates static HTML when called as `dotnet run -- build`. Do not digress into why — push rationale to the Explanation page on dev-vs-build._

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

### Step 3.2 — Add the page-rendering endpoint

_The stage-3 body stops one step short of rendering HTML — readers still need a `MapGet` that walks the `IContentService` set, parses the matching markdown, and returns the rendered HTML. Embed the canonical `Program.cs` (top-level statements, no xmldocid-addressable symbol) so the reader can copy the final file verbatim._

```csharp:path
examples/GettingStartedMinimalSiteExample/Program.cs
```

_One sentence after the fence: explain that this `MapGet` is deliberately minimal — in the DocSite and BlogSite tutorials the template ships its own Razor layout and routing, so the reader never writes this endpoint by hand once they move on from the bare host._

### Checkpoint — The page renders with its front-matter title

_Verifiable outcome: `dotnet run` serves live, and `http://localhost:5000/` returns HTML whose `<title>` element and top-level `<h1>` both read `Welcome to your first Pennington site` — pulled straight from `Content/index.md`'s front matter._

- Run `dotnet run` from the project folder.
- Open `http://localhost:5000/` and confirm the page title in the browser tab reads `Welcome to your first Pennington site`.
- View source and confirm the same string appears inside the `<title>` tag and the article's `<h1>`.

---

## 4. Verify dev-mode hot reload

_One sentence: the reader restarts the host under `dotnet watch`, edits the markdown file, and sees the browser reload without stopping the process — confirming the file-watcher and live-reload WebSocket are wired by `UsePennington`._

### Step 4.1 — Run under `dotnet watch`

_Direct the reader to stop the previous `dotnet run`, then run `dotnet watch` instead. Mention (one sentence) that live reload is gated on the `DOTNET_WATCH` environment variable, which `dotnet watch` sets automatically — the reader does not set it by hand. Leave the browser open on `http://localhost:5000/`._

```text
dotnet watch
```

### Step 4.2 — Edit the front-matter title

_Have the reader open `Content/index.md`, change the `title:` value to anything recognizable (e.g. `title: Hello, Pennington`), and save. The browser tab should update on its own within a second. If it doesn't, the reader should hard-refresh once — they may have been running stale HTML from step 3._

### Checkpoint — Live reload fires

_Verifiable outcome: without touching the terminal, the browser tab updates to show the new title in the `<h1>` and tab title. The `dotnet watch` console logs a file change line naming `Content/index.md`._

- Edit `Content/index.md`'s `title:` field and save.
- Watch the browser tab title and page heading update to match — no manual refresh needed.
- Confirm `dotnet watch` logs the change in the terminal.

---

## Summary

_Three to five bullets. Each bullet names a capability the reader now holds — not a feature the tutorial covered. Write in second person, past tense for the action, present tense for the capability._

- You stood up an ASP.NET host that serves a markdown page end-to-end through `AddPennington` and `UsePennington`.
- You know how to point the content pipeline at a folder of markdown with `ContentRootPath` plus `AddMarkdownContent<DocFrontMatter>`.
- You handed the process to `RunOrBuildAsync`, which means the same host will later generate a static site on `dotnet run -- build` with no code change.
- You watched a front-matter `title:` flow from YAML into the rendered `<h1>`, and confirmed dev-mode hot reload re-renders on save.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
