---
title: "Style the site with MonorailCSS"
description: "Register AddMonorailCss + UseMonorailCss, pick a NamedColorScheme, add a utility class to a layout, and watch the stylesheet regenerate on demand."
section: "getting-started"
order: 30
tags: []
uid: tutorials.getting-started.styling
isDraft: true
search: false
llms: false
---

> **In this page.** Registering `AddMonorailCss` + `UseMonorailCss`, picking a `NamedColorScheme`, adding a utility class to a layout, and watching the stylesheet regenerate on demand.
>
> **Not in this page.** Algorithmic color schemes, custom `CssFrameworkSettings`, or dark-mode wiring — see the how-to on theme customization.

## What you'll do

- **Artifact:** a Pennington site with a live, on-demand-generated stylesheet and a color scheme applied from a single `NamedColorScheme` value.
- **Skill:** you'll know how MonorailCSS plugs into `AddPennington`, how the stylesheet is produced, and how to change the look of the whole site with one option.

## Prerequisites

- .NET 11 SDK installed
- Completed [Create your first Pennington site](/tutorials/getting-started/first-site) and [Add your first markdown page](/tutorials/getting-started/first-page)
- A running Pennington site with a `MainLayout.razor` you can edit

The finished code for this tutorial lives in [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample) — the smallest Pennington site that still wires MonorailCSS.

---

## 1. Add MonorailCSS to the host

- Bullets to cover under this unit:
- Explain that MonorailCSS is a utility-first CSS generator that runs inside the Pennington host; no Node.js, no build step.
- The stylesheet is produced at response time by scanning the rendered HTML for utility-class tokens and generating matching CSS on the fly.
- The generator runs behind an HTTP endpoint mapped by `UseMonorailCss` (default `/css/tailwind.css`), so the stylesheet is always the current set of utility classes actually in use.

### Step 1.1 — Register `AddMonorailCss`

- Open `Program.cs` from the site you scaffolded in the first tutorial.
- Immediately after `builder.Services.AddPennington(...)`, add `builder.Services.AddMonorailCss(...)`.
- Pass a minimal lambda returning a `MonorailCssOptions` record with a `ColorScheme`.

```csharp file="examples/MinimalExample/Program.cs"
```

- _Use the `AddPennington` / `AddMonorailCss` pairing at the top of that file as the template._

### Step 1.2 — Call `UseMonorailCss` on the pipeline

- After `app.UsePennington()`, add `app.UseMonorailCss()`.
- This maps the stylesheet endpoint and turns on the utility-class scanner that feeds it.

### Checkpoint — the stylesheet endpoint is live

- Run `dotnet run`.
- Open `http://localhost:5000/css/tailwind.css` in a browser; you should see a generated stylesheet (not a 404).
- Any HTML page's `<head>` should reference this stylesheet; view-source on the home page to confirm.

---

## 2. Pick a `NamedColorScheme`

- Bullets to cover under this unit:
- `NamedColorScheme` takes a single Pennington palette name (e.g., `"slate"`, `"zinc"`, `"emerald"`, `"rose"`); the generator produces a full named color family for it.
- The color family drives every `text-primary-*`, `bg-primary-*`, `border-primary-*` utility used across Pennington.UI components.
- Swap the name, reload, and the whole site's accent color changes.

### Step 2.1 — Set `ColorScheme` on `MonorailCssOptions`

- Edit the options object passed to `AddMonorailCss`.
- Set `ColorScheme = new NamedColorScheme("emerald")` (or any supported palette name).
- Save and let the dev server pick up the change.

### Checkpoint — the accent color is visible

- Refresh the site.
- Confirm accent-colored UI (links, headings, badges) now uses the new palette.
- Revert to `"slate"` (or whichever default you started with) once you have the shape.

---

## 3. Add a utility class to a layout

- Bullets to cover under this unit:
- Pennington uses Tailwind-style utility classes rendered by MonorailCSS.
- Any utility token you write in a Razor layout or markdown HTML gets collected and turned into CSS on the next response.
- There is no build step and no CSS file to edit manually.

### Step 3.1 — Edit `MainLayout.razor`

- Open `Components/Layout/MainLayout.razor`.
- Add a utility class (e.g., `class="max-w-4xl mx-auto px-4"`) to the top-level wrapper.
- Save the file; `dotnet watch` (if you started with it) recompiles the Razor and the next HTTP response triggers CSS regeneration.

```razor file="examples/MinimalExample/Components/Layout/MainLayout.razor"
```

- _Use this layout as a reference for where utility classes sit._

### Step 3.2 — Verify the class landed in the stylesheet

- Refresh the page.
- Visit `/css/tailwind.css` again; search for the utility you added.
- Confirm the class now has a CSS rule it did not have before.

### Checkpoint — the feedback loop is intact

- You add a utility class.
- You refresh the page once.
- The stylesheet has the rule and the layout reflects it.

---

## Summary

- You wired `AddMonorailCss` + `UseMonorailCss` into a Pennington host.
- You picked a `NamedColorScheme` and saw it applied across the site in one option change.
- You added a utility class to a layout and saw it reach the generated stylesheet without any build step.
- You know where the stylesheet is served (`/css/tailwind.css`) and what drives its contents.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
