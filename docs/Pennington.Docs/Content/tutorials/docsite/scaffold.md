---
title: "Scaffold a documentation site with DocSite"
description: "Swap the bare Pennington host for the DocSite template and map content areas to top-level folders."
sectionLabel: Getting Started with DocSite
order: 102010
tags: [docsite, template, areas, scaffold]
uid: tutorials.docsite.scaffold
---

By the end of this tutorial you'll have a running DocSite host with a "Scaffold Docs" title, GitHub icon, header/footer chrome, and two content areas — Guides and Reference — each serving an index page from its own top-level folder.

You'll know how to swap a plain Pennington host for the DocSite template, populate `DocSiteOptions`, and reason about how area slugs bind top-level folders to URL prefixes and sidebar tabs.

## Prerequisites

- .NET 11 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one page)

The finished code for this tutorial lives in [`examples/DocSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

Your host currently wires `AddPennington`, `UsePennington`, and a hand-written `MapGet` fallback that walks `IContentService` to render pages. The DocSite template replaces all of that.

### Step 1.1 — Review the pre-DocSite host shape

Here's the starting state — three moving parts: DI registration, middleware, and the fallback endpoint.

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage1.Run(System.String[])
```

Everything the DocSite template adds — sidebar, header chrome, MonorailCSS, SPA navigation, the Razor component layout — is absent here. The next step collapses those ~30 lines into a single DI call.

### Checkpoint — The bare host runs

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see unstyled HTML for your markdown — no sidebar, no header, no theme

---

## 2. Swap `AddPennington` for `AddDocSite`

`AddDocSite` is a single DI call that registers Pennington core, MonorailCSS, SPA navigation, the `ContentResolver`, and the `DocSiteArticleSlotRenderer` Razor island — all driven from one options object.

### Step 2.1 — Replace the registration call

`AddDocSite` takes a `Func<DocSiteOptions>` rather than an `Action`, so you construct and return a fresh options record. You can also drop the `AddMarkdownContent` call — the template registers it internally.

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

### Step 2.2 — Populate `DocSiteOptions`

This tutorial uses five fields: `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, and `FooterContent`. Set each one and you'll see it surface in the rendered chrome immediately. `DocSiteOptions` carries many more fields; for the full surface — and for what DocSite hard-codes, such as the single `AddMarkdownContent<DocSiteFrontMatter>` registration, `SearchIndexOptions.ContentSelector`, `LlmsTxtOptions`, and `MonorailCssOptions.CustomCssFrameworkSettings` — see [Positioning DocSite as a fast path](xref:explanation.core.docsite-positioning).

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

### Step 2.3 — See the registration-only state

At this point `AddDocSite` is wired but `UseDocSite` hasn't been called yet. The host builds, but the middleware stack is still the ASP.NET default. The `await app.RunAsync()` call is a placeholder you'll replace in the next section.

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage2.Run(System.String[])
```

### Checkpoint — Services registered, middleware not yet mounted

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` returns a default ASP.NET response — the DocSite middleware is registered in DI but not mounted in the pipeline

---

## 3. Mount the DocSite middleware

`UseDocSite` is the middleware counterpart to `AddDocSite` — one call mounts locale routing, antiforgery, static files, Razor component routing, MonorailCSS, SPA navigation, and core Pennington middleware in the correct order.

### Step 3.1 — Call `UseDocSite` after `Build()`

This single call replaces both the old `UsePennington` line and the hand-written `MapGet` fallback from stage 1. The Razor `Pages.razor` component now owns the `/{*fileName:nonfile}` route and resolves pages through `ContentResolver`.

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)
```

### Step 3.2 — Swap `RunAsync` for `RunDocSiteAsync`

`RunDocSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves pages live in development and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>` — one code path for both modes.

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

### Step 3.3 — See the fully-wired host

Here's the canonical final shape — three calls that match `Program.cs` verbatim: `AddDocSite`, `UseDocSite`, `RunDocSiteAsync`.

```csharp:xmldocid,bodyonly
M:DocSiteScaffoldExample.Stage3.Run(System.String[])
```

### Checkpoint — Full chrome renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the DocSite layout: left sidebar, header with site title + search affordance + dark-mode toggle + GitHub icon linking to your `GitHubUrl`, and the footer HTML from `FooterContent`

---

## 4. Map content to areas

`DocSiteOptions.Areas` is a list of `ContentArea(Label, Slug)` pairs. Each slug binds a top-level folder under `ContentRootPath` to a URL prefix and to its own sidebar tab.

### Step 4.1 — Review the `ContentArea` contract

`ContentArea` has two fields: a human-readable label that appears in the area selector, and a slug that matches the folder name and URL prefix. The order of entries in `Areas` drives the order of tabs in the sidebar.

```csharp:xmldocid
T:Pennington.DocSite.ContentArea
```

### Step 4.2 — Create the area folders

Under `Content/` you need two folders — `guides/` and `reference/` — each with an `index.md`. The `guides` slug in `DocSiteOptions.Areas` binds `Content/guides/` to the `/guides/` URL prefix and to the Guides sidebar tab. The `reference` slug works the same way.

```text:path
examples/DocSiteScaffoldExample/Content/guides/index.md
```

```text:path
examples/DocSiteScaffoldExample/Content/reference/index.md
```

### Step 4.3 — Confirm the two-area `Areas` list

The `Areas` block in the stage 3 host has exactly two `ContentArea` entries. The sidebar only shows the area selector when more than one area is configured, so with both entries in place you'll see the tab switcher appear for the first time.

### Checkpoint — Both areas resolve and switch independently

- Visit `http://localhost:5000/guides/` — you should see the Guides index page with the Guides tab selected in the sidebar
- Visit `http://localhost:5000/reference/` — you should see the Reference index page, the Reference tab now selected, and the sidebar TOC filtered to the Reference area only

---

## Summary

- You replaced the bare `AddPennington` host with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync` and saw the full Razor chrome render.
- You populated `DocSiteOptions` with `SiteTitle`, `Description`, `GitHubUrl`, `HeaderContent`, and `FooterContent` and watched each field appear in the rendered layout.
- You configured two `ContentArea` entries and saw how slugs bind top-level folders under `Content/` to URL prefixes and to sidebar tabs.
- You now know DocSite is a fast-path template — for the knobs it hard-codes, see [Positioning DocSite as a fast path](xref:explanation.core.docsite-positioning).
