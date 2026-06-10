---
title: "Embed a Mermaid diagram in a markdown page"
description: "Author Mermaid diagrams in markdown with a fenced `mermaid` block and let the DocSite render them client-side with theme awareness."
uid: how-to.rich-content.diagrams
order: 2
sectionLabel: "Rich Content"
tags: [markdown, mermaid, diagrams, client-side]
---

To drop a flowchart, sequence diagram, or other visual into a markdown article without authoring SVG by hand, fence the diagram with `mermaid` as the language. The DocSite renders the fence body verbatim, then a client script swaps each block for an SVG in the browser. Sites that build offline or behind a firewall must vendor Mermaid themselves — see [Vendor the library for offline builds](#vendor-the-library-for-offline-builds) below.

## Before you begin
- An existing Pennington site renders markdown (see <xref:tutorials.getting-started.first-site> if not).
- The host uses `AddDocSite` or `AddBlogSite`, or — on a bare `AddPennington` host — references `Pennington.UI` and emits its script bundle from the layout (`<script type="module" src="/_content/Pennington.UI/scripts.js" defer></script>`).
- Familiarity with Mermaid syntax — this page covers the fence wiring, not Mermaid itself. See the [upstream Mermaid docs](https://mermaid.js.org/) for the grammar.

## Diagram syntaxes

Pennington does not preprocess the fence body — anything valid in Mermaid renders as-is. The two most common shapes are below.

### Flowchart

Fence a block with `mermaid` as the language and write a `flowchart` body. The client script swaps the `<code>` element for an SVG at page load.

````markdown
```mermaid
flowchart LR
    A[Markdown file] --> B[MarkdownContentParser]
    B --> C[ContentPipeline]
    C --> D[MarkdownContentRenderer]
    D --> E[Response processors]
    E --> F[Rendered HTML]
```
````

```mermaid
flowchart LR
    A[Markdown file] --> B[MarkdownContentParser]
    B --> C[ContentPipeline]
    C --> D[MarkdownContentRenderer]
    D --> E[Response processors]
    E --> F[Rendered HTML]
```

### Sequence diagram

Sequence diagrams use the same `mermaid` fence with a `sequenceDiagram` body.

````markdown
```mermaid
sequenceDiagram
    Alice->>Bob: Hello Bob, how are you?
    Bob-->>Alice: I'm good, thanks!
    Alice->>Bob: Want to grab lunch?
    Bob-->>Alice: Sounds great.
```
````

```mermaid
sequenceDiagram
    Alice->>Bob: Hello Bob, how are you?
    Bob-->>Alice: I'm good, thanks!
    Alice->>Bob: Want to grab lunch?
    Bob-->>Alice: Sounds great.
```

## What the renderer emits

Each fence renders as `<pre><code class="language-mermaid">…</code></pre>` with the body verbatim — Pennington does not transform it server-side. The browser script then loads Mermaid from `cdn.jsdelivr.net` and replaces each block with an inline SVG. The theme toggle re-renders every diagram with the matching built-in Mermaid theme, so diagrams track light and dark mode. Diagrams render on both the live dev server and the static build output.

For per-diagram theme overrides, use Mermaid's inline `%%{init: { 'theme': '…' } }%%` directive at the top of the fence body — Mermaid syntax, not Pennington syntax.

## Vendor the library for offline builds

The bundled support loads Mermaid from `cdn.jsdelivr.net` at first render. A site that builds offline or behind a firewall must serve the library itself: vendor the Mermaid module into `wwwroot` and load it from your own layout. This is the same pattern any CDN-backed widget follows — see [Load the library and your script](xref:how-to.rich-content.client-side-widget#load-the-library-and-your-script) for the vendoring recipe.

## Verify

- Open a page with a diagram in the browser. The fence renders as an SVG, not as a raw code block. A diagram still showing its `flowchart`/`sequenceDiagram` text means the script never replaced it.
- On a failure, open the browser network tab and confirm the `import` from `cdn.jsdelivr.net` succeeds. A blocked or 404'd jsdelivr request is the silent-failure signature — Mermaid never loads and the original code block stays in place. Vendor the library to fix it.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features, for context on what Pennington does and does not preprocess
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the info-string grammar (`mermaid` is a bare language token, no arguments needed)
- How-to: <xref:how-to.rich-content.alerts> — the neighboring visual-element authoring surface, for comparison
- Background: [MonorailCSS integration](xref:explanation.rendering.monorail-css) — how the DocSite's theme tokens (the same ones Mermaid tracks) are generated
