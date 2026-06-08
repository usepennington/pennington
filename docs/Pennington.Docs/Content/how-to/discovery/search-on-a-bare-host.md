---
title: "Add the search modal to a non-DocSite site"
description: "Light up the Pennington.UI search modal on a bare AddPennington host: reference the UI library, serve its scripts, and add a trigger element."
uid: how-to.discovery.search-on-a-bare-host
order: 3
sectionLabel: "Content Discovery"
tags: [search, bare-host, pennington-ui, monorailcss]
---

`AddDocSite` ships a search modal wired up for you. On a bare `AddPennington` host you wire it yourself — but the index and the modal already exist, so the work is three pieces of markup, not a search UI. `AddPennington` emits the index at `/search/{locale}/index.json`; `Pennington.UI` carries the modal in `scripts.js`; and `Pennington.MonorailCss` already safelists the modal's styles. This guide connects them.

## Before you begin
- A bare `AddPennington` host styled with [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) — see <xref:tutorials.getting-started.styling>
- The host already serves `/search/{locale}/index.json` (it does, on every `AddPennington` host). To shape what that index contains, see <xref:how-to.discovery.search>

The `BareHostSearchExample` mounts the shared Bramble corpus and lights up the modal with the wiring below.

---

## Steps

<Steps>
<Step StepNumber="1">

**Reference `Pennington.UI` and `Pennington.MonorailCss`.**

`Pennington.UI` serves `scripts.js` (the modal) and, transitively, the `DeweySearch.Web` browser client as static web assets under `/_content/`. `Pennington.MonorailCss` carries the modal's styles.

```xml:symbol
examples/BareHostSearchExample/BareHostSearchExample.csproj
```

</Step>
<Step StepNumber="2">

**Serve the `/_content/` static assets.**

`UsePennington` mounts your content folders, not the RCL assets. Call `app.MapStaticAssets()` so `/_content/Pennington.UI/scripts.js` and `/_content/DeweySearch.Web/dewey-search.js` are served — `scripts.js` fetches the latter on demand when search first opens.

```csharp:symbol
examples/BareHostSearchExample/Program.cs
```

</Step>
<Step StepNumber="3">

**Load the script and add the trigger.**

In your layout, load `scripts.js` (`defer`), set `data-default-locale` on `<body>`, and add a trigger element with `id="search-input"`. `scripts.js` self-initializes on load: it binds the click and the Ctrl/Cmd-K shortcut to that element, reads the locale attribute to locate the index, and pulls in `dewey-search.js` on demand the first time the modal opens — so you don't reference that script yourself. For a multi-locale or subpath-deployed site, also set `data-locales` and `data-base-url`.

```razor:symbol
examples/BareHostSearchExample/Components/Layout/MainLayout.razor
```

</Step>
</Steps>

---

> [!NOTE]
> No search CSS to write. The modal builds its DOM with class names (`.search-modal`, `.search-result`, …) that live only in `scripts.js`, so the MonorailCSS source scan never sees them. `Pennington.MonorailCss` declares them as `@apply` blocks in `PenningtonApplies.SearchModalApplies`, which `AddMonorailCss` emits — so the modal is styled the moment it appears. A host that brings its own (non-MonorailCSS) stylesheet defines those class names there instead.

## Verify

- Run the host and fetch `/_content/Pennington.UI/scripts.js` — it returns JavaScript (`text/javascript`), not the not-found page. If it returns HTML, `MapStaticAssets` is missing or the request is reaching the catch-all route
- Press Ctrl+K (or click the trigger). The modal opens **styled** — a centered dialog over a dimmed backdrop
- Type a query. Results deep-link to headings (`/page/#heading`) with a page breadcrumb, and the area chips filter by content area

## Related

- How-to: [Tune what the search box returns](xref:how-to.discovery.search) — configure the same index (exclude pages, weight priority, scope the indexed HTML)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
- Reference: [`SearchIndexOptions`](xref:reference.api.search-index-options)
