---
title: "Style the site with MonorailCSS"
description: "Wire up MonorailCSS, pick a color scheme, and watch Pennington regenerate the stylesheet as you add utility classes."
sectionLabel: "Getting Started with Pennington"
order: 101030
tags: [monorailcss, styling, color-scheme, utility-css]
uid: tutorials.getting-started.styling
---

By the end of this tutorial you'll have a three-page Pennington site whose HTML layout is styled with MonorailCSS utility classes, served from a `/styles.css` endpoint that regenerates whenever a new class appears in the response HTML.

You'll know how to register `AddMonorailCss`, point it at a `NamedColorScheme`, mount the generated stylesheet with `UseMonorailCss`, and trust the class-collector to keep the CSS file in sync with whatever utility classes your HTML emits.

## Prerequisites

This tutorial picks up where [Add your first markdown page](xref:tutorials.getting-started.first-page) left off. You'll need the same three-markdown-page scaffold and bare `AddPennington` host from that tutorial — if you haven't completed it yet, start there before continuing.

- .NET 11 SDK installed
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (or have a Pennington project with three markdown pages and a working `MapGet` endpoint)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample).

---

## 1. See the starting point

Let's confirm the baseline before touching anything. You're starting with an unstyled three-page site — the same shape you built in the previous tutorial.

### Step 1.1 — Run the pre-styling host

Here's the host as it stands at the start of this tutorial. Run it and load `/` to see the bare, unstyled HTML — no class-based styling has entered the picture yet.

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage1.Run(System.String[])
```

### Checkpoint — Unstyled pages render

- Run `dotnet run` and visit `http://localhost:5000/`, `/about`, and `/contact`
- You should see three pages with plain browser defaults — black text on white, browser-default link underlines, and no layout chrome beyond `<nav>`/`<article>`

---

## 2. Add a utility-class layout

Before wiring MonorailCSS itself, you'll create the shared `Layout.Render` helper. Its HTML carries the utility classes the class-collector will read on each response, and MonorailCSS will style them once it's registered.

### Step 2.1 — Create the layout helper

Drop the `Layout` class next to `Program.cs`. The full type shows both the signature and the utility-class shell it emits.

```csharp:xmldocid
T:GettingStartedStylingExample.Layout
```

Notice that `<link rel="stylesheet" href="/styles.css">` points at an endpoint that doesn't exist yet — that's intentional. `UseMonorailCss` will mount it in section 4. Classes like `text-primary-700`, `bg-base-50`, and `border-base-200` come from the named color palette you'll configure in the next section.

### Checkpoint — Layout file compiles

- The project builds with `dotnet build`
- `Layout.Render` is visible from `Program.cs`; pages still render unstyled because the route handler hasn't been updated to call it yet

---

## 3. Register MonorailCSS in DI

Now let's add the MonorailCSS service registration, pick a named color scheme, and update the route handler to wrap every response in `Layout.Render`. The stylesheet endpoint still won't be mounted yet, so pages will stay unstyled — that's deliberate. Keeping DI wiring separate from endpoint wiring makes it easier to pinpoint problems.

### Step 3.1 — Call `AddMonorailCss` with a `NamedColorScheme`

Here's the updated host. Each of `PrimaryColorName`, `AccentColorName`, `TertiaryOneColorName`, `TertiaryTwoColorName`, and `BaseColorName` takes a `ColorNames` value. This tutorial uses indigo/pink/cyan/amber/slate, but any `ColorNames` constant works — swap freely.

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage2.Run(System.String[])
```

### Checkpoint — Services registered, pages still unstyled

- Run `dotnet run` and visit `http://localhost:5000/` — the page now has the layout's `<header>`, `<nav>`, `<article>`, and `<footer>` shell, but no styles apply
- Visit `http://localhost:5000/styles.css` and you'll get a 404; the endpoint isn't mounted yet

---

## 4. Mount the stylesheet with `UseMonorailCss`

You're one line away from a live stylesheet. Adding `UseMonorailCss` to the middleware pipeline turns `/styles.css` into a real endpoint backed by the class-collector.

### Step 4.1 — Call `app.UseMonorailCss()`

The updated host is Stage 2 with one line added: `app.UseMonorailCss()`. The default path is `/styles.css`, which already matches the `<link>` tag in `Layout.Render`.

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage3.Run(System.String[])
```

### Checkpoint — Styled pages and a live stylesheet

- Run `dotnet run` and visit `http://localhost:5000/` — the header, nav, article, and footer now render with indigo accents, slate neutrals, and the layout spacing from the utility classes
- Visit `http://localhost:5000/styles.css` and you'll see a populated stylesheet containing rules for every utility class the layout emits
- Visit `http://localhost:5000/contact` — the inline `<p class="text-primary-700 font-semibold">` in `contact.md` picks up the indigo color because the collector observed the class on its way through the response pipeline

---

## 5. Watch the stylesheet regenerate

Let's prove the class-collector is live. You'll add a new utility class to a markdown file, reload the browser, and watch a new CSS rule appear without restarting the server.

### Step 5.1 — Add a new utility class to a page

Open `Content/about.md` and add the following line anywhere in the body:

```html
<p class="text-accent-600 italic">Hello MonorailCSS</p>
```

The class `text-accent-600` wasn't in the layout, so it doesn't yet exist in the stylesheet.

### Step 5.2 — Reload and confirm the new rule

Now reload `/about` in the browser. The paragraph renders in pink italic because MonorailCSS regenerated the stylesheet on the next `/styles.css` request after the new class flowed through the collector. Reload `/styles.css` directly and you'll see the `text-accent-600` rule present.

### Checkpoint — New class, new rule, no restart

- `http://localhost:5000/about` renders the new paragraph in pink italic
- `http://localhost:5000/styles.css` now contains a rule for `text-accent-600` that wasn't there before you edited the markdown
- No server restart was required — the collector picked up the class the first time the page was served

---

## Summary

- You registered MonorailCSS with `AddMonorailCss` and picked a five-color `NamedColorScheme`.
- You mounted the generated stylesheet at `/styles.css` with `UseMonorailCss`.
- You built a utility-class layout and saw the class-collector discover every token on its way through the response pipeline.
- You added a new utility class at runtime and watched the stylesheet regenerate without a restart.
