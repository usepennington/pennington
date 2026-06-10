---
title: "Style the site with MonorailCSS"
description: "Layer MonorailCSS onto the Blazor-pages site through a routed `MainLayout.razor` and watch the stylesheet regenerate as new utility classes appear in the source."
sectionLabel: "Getting Started with Pennington"
order: 3
tags: [monorailcss, styling, color-scheme, blazor, layout]
uid: tutorials.getting-started.styling
---

By the end of this tutorial the Blazor-pages site from [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) is styled with [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) — a Tailwind-compatible JIT compiler in pure .NET. Every routed `@page` renders through a `MainLayout.razor` that carries the utility classes. The [MonorailCSS Discovery pipeline](xref:explanation.rendering.monorail-css) turns those classes into real CSS rules, served at `/styles.css`. The stylesheet regenerates whenever a new class appears in the source.

## Prerequisites

- .NET 10 SDK installed
- Completed [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) (or a Pennington project with `MapRazorComponents<App>()` wired and a catch-all `MarkdownPage.razor`)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample). For a documentation site, the DocSite template ships this MonorailCSS-plus-`MainLayout` stack with a sidebar, search, and theme toggle already assembled — <xref:tutorials.docsite.scaffold> covers it.

---

## 1. Wrap pages in a styled `MainLayout.razor`

Before MonorailCSS can do anything, the layout needs to carry the utility classes that will turn into CSS rules. The document shell moves out of `App.razor` and into a `MainLayout.razor` that holds those classes; `App.razor` shrinks to a bare router that wraps every routed page in the new layout.

<Steps>
<Step StepNumber="1">

**Create `Components/Layout/MainLayout.razor`**

Drop this file at `Components/Layout/MainLayout.razor`. Inheriting `LayoutComponentBase` makes it a Blazor layout — every routed page renders into the `@Body` placeholder. This component now owns the whole document shell — `<!DOCTYPE>`, `<html>`, `<head>` (with `<HeadOutlet>`), and `<body>` — moved here from `App.razor`. The `<link rel="stylesheet" href="/styles.css">` tag points at an endpoint section 2 will mount.

```razor:symbol
examples/GettingStartedStylingExample/Components/Layout/MainLayout.razor
```

The classes — `bg-base-50`, `text-primary-700`, `border-base-200`, and so on — come from the named color palette configured in the next section.

</Step>
<Step StepNumber="2">

**Reference the layout namespace from `_Imports.razor`**

`App.razor` refers to `MainLayout` by its bare name, so the project's `_Imports.razor` needs an `@using` for the layout's namespace — without it `typeof(MainLayout)` fails to compile. Add the `Components.Layout` line to the `_Imports.razor` at the project root:

```razor:symbol
examples/GettingStartedStylingExample/_Imports.razor
```

> [!NOTE]
> The embeds use the finished example's root namespace, `GettingStartedStylingExample` (in `_Imports.razor` and the section 2 `Program.cs` snippet). Swap in your own.

</Step>
<Step StepNumber="3">

**Replace `Components/App.razor`**

With the shell now in `MainLayout.razor`, `App.razor` is replaced wholesale: it drops the `<!DOCTYPE>`, `<html>`, `<head>`, and `<HeadOutlet>` it used to own and becomes just the `<Router>`. `RouteView` names `MainLayout` as the `DefaultLayout` for every matched page, and the `LayoutView` for the not-found case lets the same shell wrap the 404 message.

```razor:symbol
examples/GettingStartedStylingExample/Components/App.razor
```

</Step>
</Steps>

## 2. Register MonorailCSS and mount `/styles.css`

MonorailCSS ships in its own package, separate from the core `Pennington` package the previous tutorial added. Pull it in:

```bash
dotnet add package Pennington.MonorailCss
```

With the package referenced, wire MonorailCSS into the service container, pick a color scheme, and mount the JIT stylesheet endpoint. `AddMonorailCss` registers the services; each of `PrimaryColorName`, `AccentColorName`, and `BaseColorName` takes a `ColorName` constant (indigo/pink/slate here — any combination works). `app.UseMonorailCss()` mounts `/styles.css` as a real endpoint that regenerates on every request, matching the `<link>` tag in `MainLayout.razor`. Both highlighted blocks are new in this section; the `using Pennington.MonorailCss;` at the top is what makes `AddMonorailCss`, `MonorailCssOptions`, `NamedColorScheme`, and `ColorName` resolve.

```csharp:symbol,bodyonly,imports
examples/GettingStartedStylingExample/Stage3_UseMonorailCss.cs > Stage3.Run
```

<Checkpoint>

- Run `dotnet run --urls http://localhost:5000` and visit `http://localhost:5000/` — the header, article, and footer now render with indigo accents, slate neutrals, and the layout spacing the utility classes describe
- Visit `http://localhost:5000/styles.css` directly and a populated stylesheet appears, containing rules for every utility class the layout emits

</Checkpoint>

---

## 3. Watch the stylesheet regenerate

Under `dotnet run`, MonorailCSS rescans your project for new utility classes on the next `/styles.css` request, so classes you add in source (Razor components and other compiled C#) appear without a restart. Markdown bodies are out of scope: a utility token added to a `.md` file will not produce a CSS rule. The [MonorailCSS integration explanation](xref:explanation.rendering.monorail-css) covers why.

<Steps>
<Step StepNumber="1">

**Add a new utility class to `MainLayout.razor`**

Open `Components/Layout/MainLayout.razor` and wrap the footer's "MonorailCSS" word in an accented span:

```razor
<footer class="mt-12 pt-4 border-t border-base-200 text-xs text-base-500">
    Styled with <span class="text-accent-600 italic">MonorailCSS</span>.
</footer>
```

The class `text-accent-600` wasn't in the layout, so it doesn't yet exist in the stylesheet.

</Step>
<Step StepNumber="2">

**Reload and confirm the new rule**

Reload any page in the browser. The footer's "MonorailCSS" word renders in pink italic because the `.razor` edit refreshed the class set, and the next `/styles.css` request picked up the new token. Reload `/styles.css` directly and the `text-accent-600` rule is present.

</Step>
</Steps>

<Checkpoint>

- The footer's "MonorailCSS" word renders in pink italic on every page
- `http://localhost:5000/styles.css` now contains a rule for `text-accent-600` that wasn't there before the edit
- No server restart was required — the MonorailCSS file watcher refreshed the stylesheet under the running `dotnet run`

</Checkpoint>

---

## Summary

- `MainLayout.razor` (a Blazor `LayoutComponentBase`) holds the utility-class scaffold every routed `@page` renders into via `App.razor`'s `DefaultLayout`.
- `AddMonorailCss(...)` registers the service container; `UseMonorailCss()` mounts the `/styles.css` endpoint that regenerates on every request.
- A `NamedColorScheme` of three `ColorName` constants drives every `primary-*`, `accent-*`, and `base-*` utility prefix.
- Under `dotnet run`, adding a new utility class to a `.razor` or `.cs` file regenerates the stylesheet on the next request without a restart — markdown edits do not participate.

The site is styled, but every page is an island with no way to reach the next. The [final getting-started tutorial](xref:tutorials.getting-started.navigation) adds a navigation menu that links them.
