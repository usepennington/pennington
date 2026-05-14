---
title: "Style the site with MonorailCSS"
description: "Layer MonorailCSS onto the Blazor-pages site through a routed `MainLayout.razor` and watch the stylesheet regenerate as new utility classes appear."
sectionLabel: "Getting Started with Pennington"
order: 101030
tags: [monorailcss, styling, color-scheme, blazor, layout]
uid: tutorials.getting-started.styling
---

By the end of this tutorial the Blazor-pages site from [Using Blazor Pages](xref:tutorials.getting-started.first-page) is styled with MonorailCSS. Every routed `@page` flows through a `MainLayout.razor` whose utility classes the MonorailCSS class collector turns into real CSS rules — served at `/styles.css` and regenerated whenever a new class appears in rendered HTML.

## Prerequisites

This tutorial picks up where [Using Blazor Pages](xref:tutorials.getting-started.first-page) left off. It assumes the same Blazor catch-all that renders markdown — start there first if that tutorial hasn't been completed.

- .NET 11 SDK installed
- Completed [Using Blazor Pages](xref:tutorials.getting-started.first-page) (or a Pennington project with `MapRazorComponents<App>()` wired and a catch-all `MarkdownPage.razor`)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample).

> [!NOTE]
> The bundled DocSite template ships this same MonorailCSS-plus-`MainLayout` stack out of the box, with a sidebar, search, and a theme toggle on top. Skip ahead to <xref:tutorials.docsite.scaffold> when a turnkey docs site is the goal.

---

## 1. See the unstyled starting point

Let's confirm the baseline before touching anything. The starting point is the Blazor catch-all from the previous tutorial — markdown renders, the layout shell exists, but no MonorailCSS services have been registered.

<Steps>
<Step StepNumber="1">

**Run the pre-styling host**

Here's the host as it stands at the start of this tutorial. Run it and load `/` to see the bare HTML — the layout shell from `MainLayout.razor` is in place (header, article, footer divs), but the utility classes in its markup are inert because no stylesheet exists yet.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage1.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/` and `/about`
- Both pages render with the layout's `<header>`, `<article>`, and `<footer>` shape, but with browser defaults — no colors, no spacing, no typography
- Visit `http://localhost:5000/styles.css` directly and the response is a 404; the endpoint isn't mounted yet

</Checkpoint>

---

## 2. Wrap pages in a styled `MainLayout.razor`

Before MonorailCSS can do anything, the layout needs to actually emit utility classes for the class collector to discover. Create the layout component, then point `App.razor` at it as the default layout for every routed page.

<Steps>
<Step StepNumber="1">

**Create `Components/Layout/MainLayout.razor`**

Drop this file at `Components/Layout/MainLayout.razor`. Inheriting `LayoutComponentBase` makes it a Blazor layout — every routed page renders into the `@Body` placeholder. Notice that the `<link rel="stylesheet" href="/styles.css">` tag points at an endpoint that doesn't exist yet; section 4 mounts it.

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

<Checkpoint>

- The project builds with `dotnet build`
- `dotnet run` and visit `/` — the page now shows the layout's header, article, and footer shape (and the article's `<h1>` text from the markdown), but the utility classes still produce no styling because `/styles.css` is still 404

</Checkpoint>

---

## 3. Register MonorailCSS in DI

Now wire MonorailCSS into the service container and pick a color scheme. The stylesheet endpoint still isn't mounted at this stage — keeping DI registration separate from the endpoint wiring makes it easier to pinpoint problems.

<Steps>
<Step StepNumber="1">

**Call `AddMonorailCss` with a `NamedColorScheme`**

Each of `PrimaryColorName`, `AccentColorName`, and `BaseColorName` takes a `ColorName` constant. This tutorial uses indigo/pink/slate; any combination works — swap freely.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage2.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- The project builds with `dotnet build`
- Pages still render unstyled — the endpoint that serves the stylesheet isn't mounted yet, so `<link rel="stylesheet" href="/styles.css">` still 404s

</Checkpoint>

---

## 4. Mount `/styles.css` with `UseMonorailCss`

One line stands between here and a live stylesheet. Adding `UseMonorailCss` to the middleware pipeline turns `/styles.css` into a real endpoint backed by the class collector.

<Steps>
<Step StepNumber="1">

**Call `app.UseMonorailCss()`**

The updated host adds one line to the previous step: `app.UseMonorailCss()`. The default endpoint path is `/styles.css`, which matches the `<link>` tag in `MainLayout.razor`.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage3.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/` — the header, article, and footer now render with indigo accents, slate neutrals, and the layout spacing the utility classes describe
- Visit `http://localhost:5000/styles.css` directly and a populated stylesheet appears, containing rules for every utility class the layout emits

</Checkpoint>

---

## 5. Watch the stylesheet regenerate

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
