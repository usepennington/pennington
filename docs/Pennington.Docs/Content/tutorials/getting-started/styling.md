---
title: "Style the site with MonorailCSS"
description: "Layer MonorailCSS onto the Blazor-pages site through a routed `MainLayout.razor` and watch the stylesheet regenerate as new utility classes appear."
sectionLabel: "Getting Started with Pennington"
order: 101030
tags: [monorailcss, styling, color-scheme, blazor, layout]
uid: tutorials.getting-started.styling
---

By the end of this tutorial the Blazor-pages site from [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) is styled with MonorailCSS. Every routed `@page` flows through a `MainLayout.razor` whose utility classes the MonorailCSS [class collector](xref:explanation.rendering.monorail-css) turns into real CSS rules — served at `/styles.css` and regenerated whenever a new class appears in rendered HTML.

## Prerequisites

- .NET 11 SDK installed
- Completed [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) (or a Pennington project with `MapRazorComponents<App>()` wired and a catch-all `MarkdownPage.razor`)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample). For a documentation site, the DocSite template ships this MonorailCSS-plus-`MainLayout` stack with a sidebar, search, and theme toggle already assembled — <xref:tutorials.docsite.scaffold> covers it.

---

## 1. Wrap pages in a styled `MainLayout.razor`

Before MonorailCSS can do anything, the layout needs to emit utility classes for the class collector to discover. Create the layout component, then point `App.razor` at it as the default layout for every routed page.

<Steps>
<Step StepNumber="1">

**Create `Components/Layout/MainLayout.razor`**

Drop this file at `Components/Layout/MainLayout.razor`. Inheriting `LayoutComponentBase` makes it a Blazor layout — every routed page renders into the `@Body` placeholder. The `<link rel="stylesheet" href="/styles.css">` tag points at an endpoint section 2 will mount.

```razor:path
examples/GettingStartedStylingExample/Components/Layout/MainLayout.razor
```

The classes — `bg-base-50`, `text-primary-700`, `border-base-200`, and so on — come from the named color palette configured in the next section.

</Step>
<Step StepNumber="2">

**Point `App.razor` at the layout**

Update `Components/App.razor` so the `<Router>` uses `MainLayout` as its default layout. The `LayoutView` for the not-found case lets the same shell wrap the 404 message.

```razor:path
examples/GettingStartedStylingExample/Components/App.razor
```

</Step>
</Steps>

## 2. Register MonorailCSS and mount `/styles.css`

Wire MonorailCSS into the service container, pick a color scheme, and mount the JIT stylesheet endpoint. `AddMonorailCss` registers the services; each of `PrimaryColorName`, `AccentColorName`, and `BaseColorName` takes a `ColorName` constant (indigo/pink/slate here — any combination works). `app.UseMonorailCss()` turns `/styles.css` into a real endpoint backed by the class collector, matching the `<link>` tag in `MainLayout.razor`.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage3.Run(System.String[])
```

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/` — the header, article, and footer now render with indigo accents, slate neutrals, and the layout spacing the utility classes describe
- Visit `http://localhost:5000/styles.css` directly and a populated stylesheet appears, containing rules for every utility class the layout emits

</Checkpoint>

---

## 3. Watch the stylesheet regenerate

The class collector watches the `Content/` directory in development. Adding a new utility class to a markdown file and reloading the browser produces a new CSS rule without a server restart — provided the host is running under `dotnet watch`.

<Steps>
<Step StepNumber="1">

**Restart under `dotnet watch`**

Stop the previous `dotnet run` with `Ctrl+C` and start the watcher instead. The class collector's file-watcher needs `dotnet watch` (or `ASPNETCORE_ENVIRONMENT=Development`) active to pick up runtime edits.

```bash
dotnet watch
```

</Step>
<Step StepNumber="2">

**Add a new utility class to a page**

Open `Content/about.md` and add the following line anywhere in the body:

```html
<p class="text-accent-600 italic">Hello MonorailCSS</p>
```

The class `text-accent-600` wasn't in the layout, so it doesn't yet exist in the stylesheet.

</Step>
<Step StepNumber="3">

**Reload and confirm the new rule**

Reload `/about` in the browser. The paragraph renders in pink italic because the file watcher signaled the class collector to rescan, and the next `/styles.css` request picked up the new token. Reload `/styles.css` directly and the `text-accent-600` rule is present.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/about` renders the new paragraph in pink italic
- `http://localhost:5000/styles.css` now contains a rule for `text-accent-600` that wasn't there before the markdown edit
- No server restart was required — the watcher picked up the file change

</Checkpoint>

---

## Summary

- `MainLayout.razor` (a Blazor `LayoutComponentBase`) holds the utility-class scaffold every routed `@page` renders into via `App.razor`'s `DefaultLayout`.
- `AddMonorailCss(...)` registers the service container; `UseMonorailCss()` mounts the `/styles.css` endpoint.
- A `NamedColorScheme` of three `ColorName` constants drives every `primary-*`, `accent-*`, and `base-*` utility prefix.
- Under `dotnet watch`, adding a new utility class to a markdown file regenerates the stylesheet on the next request without a restart.

The site is styled, but every page is an island with no way to reach the next. The [final getting-started tutorial](xref:tutorials.getting-started.navigation) adds a navigation menu that links them.
