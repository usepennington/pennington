---
title: "Embed diagrams"
description: "Author Mermaid diagrams in markdown with a fenced `mermaid` block and let the DocSite render them client-side with theme awareness."
uid: how-to.content-authoring.diagrams
order: 80
sectionLabel: Content Authoring
tags: [markdown, mermaid, diagrams, client-side]
---

> **In this page.** _Paraphrase TOC "Covers": authoring Mermaid blocks with `mermaid` fences, and how the DocSite's bundled client script picks them up at page load and re-renders them when the theme flips between light and dark._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": server-side (build-time) diagram rendering, non-Mermaid diagram engines such as PlantUML or Graphviz, and embedding hand-authored raw SVG are all out of scope — the pipeline ships Mermaid client-side rendering only._

## When to use this
_Two sentences. Frame the goal: the reader wants a flowchart, sequence diagram, or similar visual inside a markdown article and would rather write Mermaid text than author SVG. Do not re-teach markdown fences — link to [Code-block argument reference](xref:reference.markdown.code-block-args) for the info-string grammar._

## Assumptions
_Three bullets. Each is a realistic prior state — if any is not true, the reader is in the wrong quadrant._

- You have an existing Pennington site rendering markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- Your host uses `AddDocSite` / `AddBlogSite`, or otherwise serves the Pennington.UI script bundle — the `MermaidManager` lives in `Pennington.UI/wwwroot/scripts.js` and is what actually renders the diagrams.
- You know basic Mermaid syntax (flowchart, sequence, class, etc.) — this page does not teach Mermaid itself; see the [upstream Mermaid docs](https://mermaid.js.org/) for the grammar.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/diagrams.md` is the fixture page this how-to fences from.

---

## Steps
_Three steps. No production xmldocids apply here — Mermaid rendering is a client-side script, not a markdown extension, so there are no `T:Pennington.Markdown.Mermaid…` symbols to fence. The markdown source is the recipe._

### 1. Fence the diagram with `mermaid` as the language
_One sentence: open a fenced code block with three backticks and the word `mermaid`, then write ordinary Mermaid text inside. CommonMark keeps the block as a `<pre><code class="language-mermaid">…</code></pre>` in the rendered HTML — Pennington does not preprocess the body, so anything valid in Mermaid works as-is._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/diagrams.md
```

### 2. Let the bundled client script render it
_Two sentences: the DocSite ships `Pennington.UI/wwwroot/scripts.js`, which includes a `MermaidManager` that scans the DOM for `code.language-mermaid` on page load, lazy-loads Mermaid from CDN, and swaps each `<code>` block for the rendered SVG. You do not need to register anything server-side — the fence is rendered verbatim and the script takes over in the browser._

<!-- TODO: MermaidManager is a JS class inside Pennington.UI/wwwroot/scripts.js, not a C# symbol — there is no xmldocid to fence here. If a future refactor exposes the script wiring as a C# type (e.g. a `MermaidScriptAsset`), replace this callout with its xmldocid. For now, point the reader at the source file. -->

See [`src/Pennington.UI/wwwroot/scripts.js`](https://github.com/usepennington/pennington/blob/main/src/Pennington.UI/wwwroot/scripts.js) (`MermaidManager` class) for the full behavior — including how it dynamically imports Mermaid from `cdn.jsdelivr.net` the first time a diagram is seen on a page.

### 3. Let theme changes re-render the diagram
_Two sentences: when the DocSite theme toggle flips light/dark, the page manager calls `MermaidManager.reinitializeForTheme()`, which reinitializes Mermaid with a matching built-in theme (`default` vs. `dark`) and re-renders every diagram in place. There is no per-diagram configuration to opt into this — all `mermaid` fences on the page participate automatically._

<!-- Mermaid's own theme config (neutral, forest, etc.) is not exposed through Pennington. If you need a non-default theme per diagram, use Mermaid's inline `%%{init: { 'theme': '…' } }%%` directive at the top of the fence body — that is Mermaid syntax, not Pennington syntax. -->

---

## Verify
_Three terse bullets. Each is one observable check._

- Run `dotnet run` and visit the page — each `mermaid` fence renders as an SVG (not as a code block); view-source still shows the original `<code class="language-mermaid">` markup.
- Open browser dev-tools Network tab on first load — you should see one request to `cdn.jsdelivr.net/npm/mermaid@11/…` fetched lazily.
- Toggle the theme (light ↔ dark) — every diagram re-renders with matching colors, no page reload required.

## Related
- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features, for context on what Pennington does and does not preprocess
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the info-string grammar (`mermaid` is a bare language token, no arguments needed)
- How-to: [Add alerts and callouts](xref:how-to.content-authoring.alerts) — the neighboring visual-element authoring surface, for comparison
- Background: [MonorailCSS integration](xref:explanation.rendering.monorail-css) — how the DocSite's theme tokens (the same ones Mermaid tracks) are generated
