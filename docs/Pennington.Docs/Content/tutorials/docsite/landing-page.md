---
title: "Add a Razor landing page at the site root"
description: "Route a Razor component at / so a DocSite opens on a hand-built landing page, and swap the doc-page chrome for the sidebar-free FullWidthLayout."
sectionLabel: "Getting Started with DocSite"
order: 102040
tags:
  - docsite
  - razor
  - landing-page
  - routing
uid: tutorials.docsite.landing-page
---

By the end of this tutorial the DocSite at `http://localhost:5000/` opens on a Razor landing page — a hero heading, a call to action, and two cards linking into the Guides area — laid out with the sidebar-free `FullWidthLayout` instead of the default doc-page chrome.

Along the way the tutorial covers routing a Razor component at `/`, why a `@page "/"` route wins over DocSite's catch-all, and swapping the layout a routed page renders inside.

## Prerequisites

- .NET 11 SDK installed
- Completed [Add doc pages and link between them](xref:tutorials.docsite.first-doc-page) — it provides the single-area host and the `install` / `configure` guide pages this landing page links to

The finished code for this tutorial lives in [`examples/DocSitePagesAndLinksExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSitePagesAndLinksExample).

---

## 1. See that the root has no page

The host from the previous tutorial has one area, `guides`. The root `/` sits **outside** that area — there's no `Content/index.md` and no routed component pointed at it — so a request to `/` returns a 404.

<Steps>
<Step StepNumber="1">

**Run the host and visit the root**

```bash
dotnet run
```

Open `http://localhost:5000/` in a browser.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` returns a 404 — nothing serves the root.
- `http://localhost:5000/guides/` still renders the Guides hub from the previous tutorial.

</Checkpoint>

---

## 2. Route a Razor component at the root

A routed Razor component whose `@page` template is `/` owns the root URL. `AddDocSite` adds your project's assembly to the routing assemblies it hands both the live Blazor router and the static build's page scanner, so a `@page` component in your project is picked up by both with no extra wiring. And the literal `/` route is more specific than the catch-all `/{*fileName:nonfile}` in DocSite's own `Pages.razor`, so it wins the match.

<Steps>
<Step StepNumber="1">

**Create `Components/Index.razor`**

Create a `Components/` folder at the project root and add `Index.razor` with a `@page "/"` directive and minimal markup.

```razor
@page "/"

<h1>Pages &amp; Links</h1>
<p>The site root now renders a Razor component.</p>
```

The `@page "/"` directive is the whole wiring — no `Program.cs` change, no registration call.

</Step>
<Step StepNumber="2">

**Restart the host**

A `.razor` edit is a compile change, so stop the host and run `dotnet run` again to pick up the new component.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` no longer 404s — it renders the **Pages & Links** heading.
- The page is wrapped in the default doc-page chrome: a sidebar on the left and an outline rail on the right. The next unit replaces that layout.

</Checkpoint>

---

## 3. Switch to the full-width layout

A routed component with no `@layout` directive renders inside DocSite's default, `MainLayout` — the three-column doc-page chrome with the sidebar and outline rail. A landing page wants the header and footer but not the navigation columns. `FullWidthLayout` is exactly that shape.

<Steps>
<Step StepNumber="1">

**Add a `@layout` directive to `Index.razor`**

Add one line under `@page` naming the layout by its full type name.

```razor
@page "/"
@layout Pennington.DocSite.Components.Layout.FullWidthLayout

<h1>Pages &amp; Links</h1>
<p>The site root now renders a Razor component.</p>
```

`FullWidthLayout` keeps the DocSite header and footer and gives the page the full content width — no sidebar, no outline rail.

</Step>
<Step StepNumber="2">

**Restart the host**

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` renders the heading across the full content width.
- The sidebar and outline rail are gone; the DocSite header and footer remain.

</Checkpoint>

---

## 4. Build out the landing page

With routing and layout settled, the component is just Razor markup. Fill it with a hero, a call to action, and two cards linking into the Guides area. Styling is MonorailCSS utility classes using the semantic palette — `primary`, `accent`, `base` — with a `dark:` variant on every color-bearing utility.

<Steps>
<Step StepNumber="1">

**Replace `Index.razor` with the finished landing page**

```razor:path
examples/DocSitePagesAndLinksExample/Components/Index.razor
```

The two cards link to `/guides/install` and `/guides/configure` — the pages built in the previous tutorial. `<PageTitle>` sets the browser tab text, the same component DocSite uses on doc pages.

</Step>
<Step StepNumber="2">

**Restart the host and open the root**

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/` renders the hero heading, the **Read the guides** button, and two guide cards.
- Clicking a card navigates to the matching guide page; the **Read the guides** button lands on `/guides/`.
- Run `dotnet run -- build` — the static build's page scanner picks up the same `@page "/"` route and writes the landing page to `output/index.html`.

</Checkpoint>

---

## Summary

- A Razor component with `@page "/"` owns the site root — `AddDocSite` already routes your project's assembly, so the directive is the whole wiring.
- The literal `/` route beats DocSite's catch-all `Pages.razor`, and the same route is honored by both the live host and the static build.
- A routed component defaults to `MainLayout`; a `@layout` directive naming `FullWidthLayout` drops the sidebar for a landing-page shape.
- The component body is ordinary Razor styled with MonorailCSS — semantic palette utilities, `dark:` variants, and links straight into the content areas.
