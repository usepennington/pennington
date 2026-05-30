---
title: Diagrams
description: Mermaid fences render client-side with theme awareness.
tags: [authoring, diagrams]
sectionLabel: authoring
order: 90
uid: kitchen-sink.main.diagrams
---

Fence a block with the `mermaid` language and the DocSite's bundled
`MermaidManager` picks it up at page load, lazy-loads mermaid from CDN,
and replaces the `<code>` element with the rendered SVG. Theme changes
(light / dark) trigger a re-render with a matching mermaid theme.

```mermaid
flowchart LR
    A[Markdown file] --> B[MarkdownContentParser]
    B --> C[ContentPipeline]
    C --> D[MarkdownContentRenderer]
    D --> E[Response processors]
    E --> F[Rendered HTML]
```

Sequence diagrams work the same way:

```mermaid
sequenceDiagram
    Browser->>Pennington: GET /main/diagrams
    Pennington-->>Browser: HTML with <code class="language-mermaid">
    Note right of Browser: MermaidManager scans the DOM
    Browser->>CDN: import('mermaid')
    CDN-->>Browser: mermaid module
    Browser->>Browser: render diagrams in place
```

Diagrams render on both the live dev server and the static build output;
the client-side script walks the DOM either way.
