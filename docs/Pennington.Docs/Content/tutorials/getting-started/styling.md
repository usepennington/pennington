---
title: "Style the site with MonorailCSS"
description: "Wire up MonorailCSS, pick a color scheme, and watch Pennington regenerate the stylesheet as you add utility classes."
sectionLabel: "Getting Started with Pennington"
order: 101030
tags: [monorailcss, styling, color-scheme, utility-css]
uid: tutorials.getting-started.styling
---

By the end of this tutorial a three-page Pennington site has its HTML layout styled with MonorailCSS utility classes, served from a `/styles.css` endpoint that regenerates whenever a new class appears in the response HTML.

The tutorial covers how to register `AddMonorailCss`, point it at a `NamedColorScheme`, mount the generated stylesheet with `UseMonorailCss`, and rely on the class-collector to keep the CSS file in sync with whatever utility classes the HTML emits.

## Prerequisites

This tutorial picks up where [Add your first markdown page](xref:tutorials.getting-started.first-page) left off. It needs the same three-markdown-page scaffold and bare `AddPennington` host from that tutorial — start there first if that tutorial hasn't been completed.

- .NET 11 SDK installed
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (or a Pennington project with three markdown pages and a working `MapGet` endpoint)

The finished code for this tutorial lives in [`examples/GettingStartedStylingExample`](https://github.com/usepennington/pennington/tree/main/examples/GettingStartedStylingExample).

---

## 1. See the starting point

Let's confirm the baseline before touching anything. The starting point is an unstyled three-page site — the same shape built in the previous tutorial.

<Steps>
<Step StepNumber="1">

**Run the pre-styling host**

Here's the host as it stands at the start of this tutorial. Run it and load `/` to see the bare, unstyled HTML — no class-based styling has entered the picture yet.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage1.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`, `/about`, and `/contact`
- Three pages render with plain browser defaults — black text on white, browser-default link underlines, and no layout chrome beyond `<nav>`/`<article>`

</Checkpoint>

---

## 2. Add a utility-class layout

Before wiring MonorailCSS itself, let's create the shared `Layout.Render` helper. Its HTML carries the utility classes the class-collector reads on each response, and MonorailCSS styles them once it's registered.

<Steps>
<Step StepNumber="1">

**Create the layout helper**

Drop the `Layout` class next to `Program.cs`. The full type shows both the signature and the utility-class shell it emits.

```csharp:xmldocid
T:GettingStartedStylingExample.Layout
```

Notice that `<link rel="stylesheet" href="/styles.css">` points at an endpoint that doesn't exist yet — that's intentional. `UseMonorailCss` mounts it in section 4. Classes like `text-primary-700`, `bg-base-50`, and `border-base-200` come from the named color palette configured in the next section.

</Step>
</Steps>

<Checkpoint>

- The project builds with `dotnet build`
- `Layout.Render` is visible from `Program.cs`; pages still render unstyled because the route handler hasn't been updated to call it yet

</Checkpoint>

---

## 3. Register MonorailCSS in DI

Now let's add the MonorailCSS service registration, pick a named color scheme, and update the route handler to wrap every response in `Layout.Render`. The stylesheet endpoint still isn't mounted, so pages stay unstyled — that's deliberate. Keeping DI wiring separate from endpoint wiring makes it easier to pinpoint problems.

<Steps>
<Step StepNumber="1">

**Call `AddMonorailCss` with a `NamedColorScheme`**

Here's the updated host. Each of `PrimaryColorName`, `AccentColorName`, and `BaseColorName` takes a `ColorName` value. This tutorial uses indigo/pink/slate, but any `ColorName` constant works — swap freely.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage2.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/` — the page now has the layout's `<header>`, `<nav>`, `<article>`, and `<footer>` shell, but no styles apply
- Visit `http://localhost:5000/styles.css` and the response is a 404; the endpoint isn't mounted yet

</Checkpoint>

---

## 4. Mount the stylesheet with `UseMonorailCss`

One line stands between here and a live stylesheet. Adding `UseMonorailCss` to the middleware pipeline turns `/styles.css` into a real endpoint backed by the class-collector.

<Steps>
<Step StepNumber="1">

**Call `app.UseMonorailCss()`**

The updated host is Stage 2 with one line added: `app.UseMonorailCss()`. The default path is `/styles.css`, which already matches the `<link>` tag in `Layout.Render`.

```csharp:xmldocid,bodyonly,usings
M:GettingStartedStylingExample.Stage3.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/` — the header, nav, article, and footer now render with indigo accents, slate neutrals, and the layout spacing from the utility classes
- Visit `http://localhost:5000/styles.css` and a populated stylesheet appears, containing rules for every utility class the layout emits
- Visit `http://localhost:5000/contact` — the inline `<p class="text-primary-700 font-semibold">` in `contact.md` picks up the indigo color because the collector observed the class on its way through the response pipeline

</Checkpoint>

---

## 5. Watch the stylesheet regenerate

Let's prove the class-collector is live. Adding a new utility class to a markdown file and reloading the browser produces a new CSS rule without a server restart.

<Steps>
<Step StepNumber="1">

**Add a new utility class to a page**

Open `Content/about.md` and add the following line anywhere in the body:

```html
<p class="text-accent-600 italic">Hello MonorailCSS</p>
```

The class `text-accent-600` wasn't in the layout, so it doesn't yet exist in the stylesheet.

</Step>
<Step StepNumber="2">

**Reload and confirm the new rule**

Now reload `/about` in the browser. The paragraph renders in pink italic because MonorailCSS regenerated the stylesheet on the next `/styles.css` request after the new class flowed through the collector. Reload `/styles.css` directly and the `text-accent-600` rule is present.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/about` renders the new paragraph in pink italic
- `http://localhost:5000/styles.css` now contains a rule for `text-accent-600` that wasn't there before the markdown edit
- No server restart was required — the collector picked up the class the first time the page was served

</Checkpoint>

---

## Summary

- MonorailCSS is registered with `AddMonorailCss` and a five-color `NamedColorScheme`.
- The generated stylesheet is mounted at `/styles.css` with `UseMonorailCss`.
- A utility-class layout feeds the class-collector, which discovers every token on its way through the response pipeline.
- A new utility class added at runtime regenerates the stylesheet without a restart.
