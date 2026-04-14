---
title: "Scaffold a documentation site with DocSite"
description: "Swap the bare Pennington host for the DocSite template and map content areas to top-level folders."
sectionLabel: Getting Started with DocSite
order: 10
tags: [docsite, template, areas, scaffold]
uid: tutorials.docsite.scaffold
---

> **In this page.** Replace the barebones setup with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`, configure `DocSiteOptions` (site title, GitHub URL, header/footer), and see how areas map to top-level folders.
>
> **Not in this page.** Authoring markdown content (covered in [Author a documentation page with DocFrontMatter](/tutorials/docsite/first-doc-page)) or overriding the DocSite layout (treated as a customization how-to).

## What you'll do

_**Artifact** (one sentence): describe the concrete output — a running DocSite host with a "Scaffold Docs" title, GitHub icon, header/footer chrome, and two areas (Guides, Reference) each serving an index page from its own top-level folder._

_**Skill** (one sentence): describe what the reader walks away able to do — swap a plain Pennington host for the DocSite template, populate `DocSiteOptions`, and reason about the area → folder → URL-prefix mapping._

## Prerequisites

_Keep this list to tools and prior tutorials only. The reader arrives here with the bare `AddPennington` host from tutorial 1 and a `Content/` folder of markdown already in place._

- .NET 11 SDK installed
- Completed [Create your first Pennington site](/tutorials/getting-started/first-site)
- Completed [Add your first markdown page](/tutorials/getting-started/first-page) (so `Content/` already has at least one page)

The finished code for this tutorial lives in [`examples/DocSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

_One sentence: remind the reader what their host currently looks like — `AddPennington`, `UsePennington`, and a hand-written `MapGet` fallback that walks `IContentService` to render pages — so the diff in the next unit is visible._

### Step 1.1 — Review the pre-DocSite host shape

_Show the starting state verbatim. Call out the three moving parts (DI registration, middleware, fallback endpoint) without explaining why — this is the shape they already built._

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage1.Run(System.String[])
```

_One sentence: note that everything the DocSite template adds — sidebar, header chrome, MonorailCSS, SPA navigation, the Razor component layout — is missing here, and the next unit will replace these ~30 lines with a single DI call._

### Checkpoint — The bare host runs

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see unstyled HTML for your markdown — no sidebar, no header, no theme

---

## 2. Swap `AddPennington` for `AddDocSite`

_One sentence: introduce the DocSite template as a single DI call that registers Pennington core, MonorailCSS, SPA navigation, the `ContentResolver`, and the `DocSiteArticleSlotRenderer` Razor island — all driven from one options object._

### Step 2.1 — Replace the registration call

_Point the reader at the signature: `AddDocSite` takes a `Func<DocSiteOptions>` (not an `Action`), so they construct and return a fresh options record. No need to keep the `AddMarkdownContent` block — the template registers it internally._

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

### Step 2.2 — Populate `DocSiteOptions`

_Walk through the five knobs this tutorial exercises: `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, `FooterContent`. Keep it mechanical — the reader copies each line and sees it surface in the rendered chrome. The options record carries many more fields; point forward to [Positioning DocSite as a fast path](/explanation/core/docsite-positioning) for the full surface and for the caps DocSite hard-codes (single `AddMarkdownContent<DocSiteFrontMatter>` registration, `SearchIndexOptions.ContentSelector`, `LlmsTxtOptions`, `MonorailCssOptions.CustomCssFrameworkSettings`)._

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

### Step 2.3 — See the registration-only state

_Show the stage 2 body: `AddDocSite` is wired but `UseDocSite` is not yet called, so the host builds but the middleware stack is still ASP.NET default. The `await app.RunAsync()` line is a placeholder the next unit replaces._

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage2.Run(System.String[])
```

### Checkpoint — Services registered, middleware not yet mounted

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` returns a default ASP.NET response — the DocSite middleware is registered in DI but not mounted in the pipeline

---

## 3. Mount the DocSite middleware

_One sentence: `UseDocSite` is the middleware counterpart to `AddDocSite` — one call mounts locale routing, antiforgery, static files, Razor component routing, MonorailCSS, SPA navigation, and core Pennington middleware in the right order._

### Step 3.1 — Call `UseDocSite` after `Build()`

_Show the signature. Emphasize that this single call replaces the old `UsePennington` line and the hand-written `MapGet` fallback from stage 1 — the Razor `Pages.razor` component now owns `/{*fileName:nonfile}` and resolves pages via `ContentResolver`._

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)
```

### Step 3.2 — Swap `RunAsync` for `RunDocSiteAsync`

_`RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves live in dev and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>`. Don't explain the unified-path invariant here — link to the dev-vs-build explanation if you need to reference it._

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

### Step 3.3 — See the fully-wired host

_Show stage 3 — this is the canonical final shape and it matches `Program.cs` verbatim. Three calls: `AddDocSite`, `UseDocSite`, `RunDocSiteAsync`._

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage3.Run(System.String[])
```

### Checkpoint — Full chrome renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the DocSite layout: left sidebar, header with site title + search affordance + dark-mode toggle + GitHub icon linking to your `GitHubUrl`, and the footer HTML from `FooterContent`

---

## 4. Map content to areas

_One sentence: `DocSiteOptions.Areas` is a list of `ContentArea(Label, Slug)` pairs; each slug binds a top-level folder under `ContentRootPath` to a URL prefix and to its own sidebar tab._

### Step 4.1 — Review the `ContentArea` contract

_Show the type. Two fields: the human-readable label (appears in the area selector) and the slug (matches folder name and URL prefix). Order in `Areas` drives order in the sidebar._

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

### Step 4.2 — Create the area folders

_Walk through the filesystem. Under `Content/` create `guides/` and `reference/`, each with an `index.md`. The `guides` slug in `DocSiteOptions.Areas` binds `Content/guides/` to `/guides/` and to the Guides sidebar tab; same story for `reference`._

```text:path
examples/DocSiteScaffoldExample/Content/guides/index.md
```

```text:path
examples/DocSiteScaffoldExample/Content/reference/index.md
```

### Step 4.3 — Confirm the two-area `Areas` list

_The final `Areas` block in stage 3 has exactly two `ContentArea` entries. The sidebar only shows the area selector when more than one area is configured — with two entries, the reader sees the tab switcher appear._

### Checkpoint — Both areas resolve and switch independently

- Visit `http://localhost:5000/guides/` — you should see the Guides index page with the Guides tab selected in the sidebar
- Visit `http://localhost:5000/reference/` — you should see the Reference index page, the Reference tab now selected, and the sidebar TOC filtered to the Reference area only

---

## Summary

- You replaced the bare `AddPennington` host with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync` and saw the full Razor chrome render.
- You populated `DocSiteOptions` with `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, and `FooterContent` and watched each field appear in the rendered layout.
- You configured two `ContentArea` entries and saw how slugs bind top-level folders under `Content/` to URL prefixes and to sidebar tabs.
- You now know DocSite is a fast-path template — for the knobs it hard-codes, see [Positioning DocSite as a fast path](/explanation/core/docsite-positioning).
