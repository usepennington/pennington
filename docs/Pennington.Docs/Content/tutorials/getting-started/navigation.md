---
title: "Add navigation across your pages"
description: "Give the styled bare host a header menu that builds itself from the content pipeline and highlights the current page."
uid: tutorials.getting-started.navigation
order: 4
sectionLabel: "Getting Started with Pennington"
tags: [navigation, blazor, components, getting-started]
---

By the end of this tutorial the styled site from [Style the site with MonorailCSS](xref:tutorials.getting-started.styling) has a navigation menu in its header. The menu links every page on the site, builds itself from the `Content/` folder — so adding a markdown file adds a menu entry — and renders the current page in bold.

This is the last step of the getting-started arc. After it, a bare `AddPennington` host serves a complete, styled, navigable multi-page site — no template involved.

## Prerequisites

- .NET 10 SDK installed
- Completed [Style the site with MonorailCSS](xref:tutorials.getting-started.styling) — this tutorial extends that project's `MainLayout.razor` and `Content/` folder

The finished code for this tutorial lives in [`examples/GettingStartedNavigationExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedNavigationExample).

---

## 1. Add pages to navigate to

The styling site has a single home page. A menu needs somewhere to point, so let's add three more pages — two inside a `guides/` folder, one at the top level. A folder with no `index.md` becomes a section node in the tree, so `guides/` will render as a labeled **Guides** group with its two pages nested under it.

<Steps>
<Step StepNumber="1">

**Create the `guides/` folder with two pages**

Add `Content/guides/installation.md` and `Content/guides/deployment.md`. The `order:` front-matter key sets each page's position in its section — lower sorts first.

```markdown:symbol
examples/GettingStartedNavigationExample/Content/guides/installation.md
```

```markdown:symbol
examples/GettingStartedNavigationExample/Content/guides/deployment.md
```

</Step>
<Step StepNumber="2">

**Create a top-level page**

Add `Content/about.md` directly under the content root — not in a folder — so it becomes a top-level menu entry rather than part of a section.

```markdown:symbol
examples/GettingStartedNavigationExample/Content/about.md
```

</Step>
</Steps>

<Checkpoint>

- `dotnet run --urls http://localhost:5000`, then visit `http://localhost:5000/guides/installation/` and `http://localhost:5000/about/`
- Both pages render through the styled layout — but there is still no menu, so the only way to reach them is by typing the URL

</Checkpoint>

---

## 2. Build the navigation menu

`AddPennington` already registers `NavigationBuilder` — the service that turns content into a navigation tree — so the menu needs no new wiring in `Program.cs`, only a component to render it.

<Steps>
<Step StepNumber="1">

**Add the `Pennington.Navigation` namespace to `_Imports.razor`**

`NavMenu.razor` uses types from `Pennington.Navigation`, so add that namespace to the project's `_Imports.razor`:

```razor
@using Pennington.Navigation
```

`<NavMenu />` is referenced by its short tag name in `MainLayout.razor`, which resolves only when the layout folder's namespace (`<RootNamespace>.Components.Layout`) is in scope. The styling tutorial already added that line when it moved the shell into `MainLayout.razor`, so it is in place — an unresolved component tag is a build warning, not an error, and `<NavMenu />` would silently render as raw markup without it.

</Step>
<Step StepNumber="2">

**Create `Components/Layout/NavMenu.razor`**

This component renders the menu from the content pipeline:

```razor:symbol
examples/GettingStartedNavigationExample/Components/Layout/NavMenu.razor
```

`CollectTocEntriesAsync` gathers a flat list — one `ContentTocItem` per page — from every content source. `BuildTreeAsync` is what gives it shape: it sorts entries by their `order:` value and nests them by folder, so `Content/guides/` becomes a **Guides** section. Passing the current URL makes the matching node come back with `IsSelected` set. For the full picture of how the tree is folded together, see [Navigation-tree construction](xref:explanation.routing.navigation-tree).

</Step>
</Steps>

## 3. Wire the menu into the layout

Drop `<NavMenu />` into the header of `MainLayout.razor`. Because the layout wraps every routed page, the menu then appears site-wide.

```razor:symbol
examples/GettingStartedNavigationExample/Components/Layout/MainLayout.razor
```

<Checkpoint>

Run `dotnet run --urls http://localhost:5000` and open `http://localhost:5000/`.

- The header shows a menu: **Welcome**, a **Guides** group containing **Installation** and **Deployment**, then **About**
- The entry for the page you are viewing renders bold — click into `/guides/deployment/` and the highlight follows
- Add a new markdown file under `Content/` and reload — a menu entry appears for it with no edit to `NavMenu.razor` or `MainLayout.razor`

</Checkpoint>

---

## Summary

- `NavigationBuilder` ships with `AddPennington`; `NavMenu.razor` is the only new code, and `Program.cs` did not change.
- `NavMenu.razor` collects each source's table-of-contents entries and `NavigationBuilder.BuildTreeAsync` turns that flat list into a sorted, folder-nested tree.
- The bare host now serves a complete site: a content pipeline, a styled layout, and navigation — all on `AddPennington`.

That is the whole getting-started arc. `AddPennington` gives you the lower-level host: you wire the pipeline, the layout, and the navigation yourself, and you have now done each part. The [DocSite](xref:tutorials.docsite.scaffold) and [BlogSite](xref:tutorials.blogsite.scaffold) templates package this wiring for documentation and blog sites. The [beyond-basics tutorials](xref:tutorials.beyond-basics.custom-razor-component) build on the host you just finished.
