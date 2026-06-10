---
title: "Add the search modal to a non-DocSite site"
description: "Light up the Pennington.UI search modal on a bare AddPennington host: reference the UI library, serve its scripts, and add a trigger element."
uid: how-to.discovery.search-on-a-bare-host
order: 3
sectionLabel: "Content Discovery"
tags: [search, bare-host, pennington-ui, monorailcss]
---

`AddDocSite` ships a search modal wired up for you. On a bare `AddPennington` host you wire it yourself — but the index and the modal already exist, so the work is three pieces of markup, not a search UI. `AddPennington` emits the index at `/search/{locale}/index.json`; `Pennington.UI` carries the modal in `scripts.js`; and `Pennington.MonorailCss` already safelists the modal's styles. This guide connects them. For how that index is built and queried, see <xref:explanation.discovery.search>.

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

In your layout, load `scripts.js` (`defer`), set `data-default-locale` on `<body>`, and add a trigger element with `id="search-input"`. `scripts.js` self-initializes on load: it binds the click and the Ctrl/Cmd-K shortcut to that element, reads the locale attribute to locate the index, and pulls in `dewey-search.js` on demand the first time the modal opens — so you don't reference that script yourself.

```razor:symbol
examples/BareHostSearchExample/Components/Layout/MainLayout.razor
```

A single-locale site deployed at the domain root needs only `data-default-locale`. Two more `<body>` attributes cover the other cases:

- **`data-default-locale`** — the default locale code (for example `en`). The client falls back to this when no per-locale prefix matches, so it must always be present.
- **`data-locales`** — a comma-separated list of every locale code, in any order (for example `en,fr,de`). The client matches the first URL path segment against this list to pick which `/search/{locale}/` tree to query. Leave it empty or omit it on a single-locale site.
- **`data-base-url`** — the deploy sub-path prefix, with a leading slash and no trailing slash (for example `/docs`). The client prepends it when it fetches `dewey-search.js`, because a runtime-injected `<script>` does not pass through the base-url rewriter that fixes server-rendered links. Omit it for a domain-root deployment.

A multi-locale site deployed under `/docs` sets all three:

```razor
<body data-default-locale="en" data-locales="en,fr,de" data-base-url="/docs">
```

</Step>
</Steps>

---

> [!NOTE]
> No search CSS to write. The modal builds its DOM with class names (`.search-modal`, `.search-result`, …) that live only in `scripts.js`, so the MonorailCSS source scan never sees them. `AddMonorailCss` ships their styles anyway, so the modal is styled the moment it appears. A host that brings its own (non-MonorailCSS) stylesheet defines those class names there instead.

## Verify

- Run the host and fetch `/_content/Pennington.UI/scripts.js` — it returns JavaScript (`text/javascript`), not the not-found page. If it returns HTML, `MapStaticAssets` is missing or the request is reaching the catch-all route
- Press Ctrl+K (or click the trigger). The modal opens **styled** — a centered dialog over a dimmed backdrop
- Type a query. Results deep-link to headings (`/page/#heading`) with a page breadcrumb, and the area chips filter by content area

## Related

- How-to: [Tune what the search box returns](xref:how-to.discovery.search) — configure the same index (exclude pages, weight priority, scope the indexed HTML)
- Background: [How the search index is built and queried](xref:explanation.discovery.search)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
- Reference: [`SearchIndexOptions`](xref:reference.api.search-index-options)
