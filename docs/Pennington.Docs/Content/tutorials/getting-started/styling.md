---
title: "Style the site with MonorailCSS"
description: "Wire up MonorailCSS, pick a color scheme, and watch Pennington regenerate the stylesheet as you add utility classes."
sectionLabel: "Getting Started with Pennington"
order: 101030
tags: [monorailcss, styling, color-scheme, utility-css]
uid: tutorials.getting-started.styling
---

> **In this page.** _One sentence: writer paraphrases the TOC "Covers" line — registering `AddMonorailCss` + `UseMonorailCss`, picking a `NamedColorScheme`, adding a utility class to a layout, and watching the stylesheet regenerate on demand. Keep it to one sentence; this is the pitch, not a scope document._
>
> **Not in this page.** _One sentence pointing outward. Mention that algorithmic color schemes, custom `CssFrameworkSettings`, and dark-mode wiring belong to the [Customize MonorailCSS](xref:how-to.configuration.monorail-css) how-to, and that deployment lives in the [Deploy to GitHub Pages](xref:how-to.deployment.github-pages) how-to. Paste links exactly as shown._

## What you'll do

_**Artifact** (one sentence): describe what the reader will have running at the end — a three-page Pennington site whose HTML layout is styled with MonorailCSS utility classes, served from a `/styles.css` endpoint that regenerates whenever a new class appears in the response HTML._

_**Skill** (one sentence): name the capability — the reader will know how to register `AddMonorailCss`, point it at a `NamedColorScheme`, mount the generated stylesheet with `UseMonorailCss`, and trust the class-collector to keep the CSS file in sync with whatever utility classes their HTML emits._

## Prerequisites

_Two-sentence lead-in: this tutorial picks up where the first-page tutorial left off — same three-markdown-page scaffold, same bare `AddPennington` host. If the reader hasn't done that one, point them back; don't re-teach host bootstrapping here._

- .NET 11 SDK installed
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (or have a Pennington project with three markdown pages and a working `MapGet` endpoint)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample).

---

## 1. See the starting point

_One sentence: the reader begins with an unstyled three-page site — same shape they built in the previous tutorial — and the first unit just confirms that baseline before anything changes._

### Step 1.1 — Run the pre-styling host

_Two sentences: point at the `Stage1.Run` body as the current state, and tell the reader to `dotnet run` and load `/` to see the bare, unstyled HTML. No class-based styling has entered the picture yet._

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage1.Run(System.String[])
```

### Checkpoint — Unstyled pages render

- Run `dotnet run` and visit `http://localhost:5000/`, `/about`, and `/contact`
- You should see three pages with plain browser defaults — black text on white, browser-default link underlines, and no layout chrome beyond `<nav>`/`<article>`

---

## 2. Add a utility-class layout

_One sentence: before wiring MonorailCSS itself, introduce the shared `Layout.Render` helper whose HTML is sprayed with utility classes — this is what the class-collector will read on each response, and it's what MonorailCSS will style once it's registered._

### Step 2.1 — Create the layout helper

_One sentence: drop the `Layout` class next to `Program.cs`; show the full type so the reader can see both the signature and the utility-class shell it emits._

```csharp:xmldocid
T:GettingStartedStylingExample.Layout
```

_Explain one non-obvious line: the `<link rel="stylesheet" href="/styles.css">` points at an endpoint that doesn't exist yet — that's fine; `UseMonorailCss` will mount it in unit 4. Classes like `text-primary-700`, `bg-base-50`, and `border-base-200` come from the named color palette you'll pick in the next unit._

### Checkpoint — Layout file compiles

- The project builds with `dotnet build`
- `Layout.Render` is visible from `Program.cs`; pages still render unstyled because the route handler hasn't been updated to call it yet

---

## 3. Register MonorailCSS in DI

_One sentence: this unit adds the MonorailCSS service registration and the named color scheme, and updates the route handler to wrap every response in `Layout.Render`. The stylesheet endpoint still isn't mounted, so pages stay unstyled — that's deliberate, to isolate DI wiring from endpoint wiring._

### Step 3.1 — Call `AddMonorailCss` with a `NamedColorScheme`

_Two sentences: show the Stage 2 `Run` body. Call out that `PrimaryColorName`, `AccentColorName`, `TertiaryOneColorName`, `TertiaryTwoColorName`, and `BaseColorName` each take a `ColorNames` value, and that the tutorial uses indigo/pink/cyan/amber/slate; any `ColorNames` constant works._

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage2.Run(System.String[])
```

### Checkpoint — Services registered, pages still unstyled

- Run `dotnet run` and visit `http://localhost:5000/` — the page now has the layout's `<header>`, `<nav>`, `<article>`, and `<footer>` shell, but no styles apply
- Visit `http://localhost:5000/styles.css` and you'll get a 404; the endpoint isn't mounted yet

---

## 4. Mount the stylesheet with `UseMonorailCss`

_One sentence: the final wiring step — add a single line to the middleware pipeline so `/styles.css` becomes a real endpoint backed by the class-collector._

### Step 4.1 — Call `app.UseMonorailCss()`

_Two sentences: show the Stage 3 `Run` body — it's Stage 2 plus one line, `app.UseMonorailCss()`. Mention that the default path is `/styles.css`, matching the `<link>` tag already in `Layout.Render`._

```csharp:xmldocid,bodyonly
M:GettingStartedStylingExample.Stage3.Run(System.String[])
```

### Checkpoint — Styled pages and a live stylesheet

- Run `dotnet run` and visit `http://localhost:5000/` — the header, nav, article, and footer now render with indigo accents, slate neutrals, and the layout spacing from the utility classes
- Visit `http://localhost:5000/styles.css` and you'll see a populated stylesheet containing rules for every utility class the layout emits
- Visit `http://localhost:5000/contact` — the inline `<p class="text-primary-700 font-semibold">` in `contact.md` picks up the indigo color because the collector observed the class on its way through the response pipeline

---

## 5. Watch the stylesheet regenerate

_One sentence: prove to the reader that the class-collector is live — add a new utility class, reload, and watch it appear in the generated CSS without restarting anything._

### Step 5.1 — Add a new utility class to a page

_Two sentences: instruct the reader to open `Content/about.md` and add a paragraph like `<p class="text-accent-600 italic">Hello MonorailCSS</p>`. The class `text-accent-600` wasn't in the layout, so it doesn't yet exist in the stylesheet._

### Step 5.2 — Reload and confirm the new rule

_Two sentences: reload `/about` in the browser — the paragraph should render in pink italic, because MonorailCSS regenerated the stylesheet on the next `/styles.css` request after the new class flowed through the collector. Reload `/styles.css` directly to see the `text-accent-600` rule present._

### Checkpoint — New class, new rule, no restart

- `http://localhost:5000/about` renders the new paragraph in pink italic
- `http://localhost:5000/styles.css` now contains a rule for `text-accent-600` that wasn't there before you edited the markdown
- No server restart was required — the collector picked up the class the first time the page was served

---

## Summary

_Three to five bullets. Each names a capability, not a topic. Suggested shape:_

- You registered MonorailCSS with `AddMonorailCss` and picked a five-color `NamedColorScheme`.
- You mounted the generated stylesheet at `/styles.css` with `UseMonorailCss`.
- You built a utility-class layout and saw the class-collector discover every token on its way through the response pipeline.
- You added a new utility class at runtime and watched the stylesheet regenerate without a restart.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
