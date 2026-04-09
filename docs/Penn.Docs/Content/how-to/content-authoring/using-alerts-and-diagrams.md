---
title: "Using Alerts and Mermaid Diagrams"
description: "Add NOTE, TIP, IMPORTANT, WARNING, and CAUTION alert blocks and render Mermaid diagrams in fenced code blocks"
uid: "penn.how-to.using-alerts-and-diagrams"
order: 18
---

## Beat 1: Alert Blocks

You want to highlight important information with styled callout boxes or render architecture diagrams inline.

Use GitHub-style alert blocks to add NOTE, TIP, IMPORTANT, WARNING, and CAUTION callouts to your content.

### What to show
- Three alerts in a migration guide: `> [!WARNING]` for breaking changes, `> [!TIP]` for helpful tools, `> [!NOTE]` for additional context
- Reference `T:Penn.Markdown.Extensions.CustomAlertInlineParser` which parses `[!TYPE]` inside blockquotes, replacing the standard Markdig `AlertInlineParser`
- The parser creates a Markdig `AlertBlock` with CSS classes `markdown-alert` and `markdown-alert-{type}` (lowercased)
- Reference the pipeline wiring: `M:Penn.Markdown.MarkdownPipelineFactory.CreateWithExtensions` calls `UseCustomAlerts()` which replaces the built-in alert inline parser with `CustomAlertInlineParser` and adds an `AlertBlockRenderer`
- Show all five types for reference: `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION`

### Key points
- Alerts use standard blockquote syntax (`>`) with the `[!TYPE]` marker on the first line
- The custom parser (`CustomAlertInlineParser`) replaces the quote block's container with an `AlertBlock` so the Markdig rendering pipeline treats it as a distinct element
- Penn's custom parser differs from the built-in Markdig parser to support extended alert content formatting

## Beat 2: Mermaid Diagrams

Add a `mermaid` fenced code block to render flowcharts, sequence diagrams, and other diagram types inline. Penn outputs the code block and client-side Mermaid.js transforms it into an SVG.

### What to show
- A fenced code block with ` ```mermaid ` containing a flowchart definition: `Request --> Middleware --> Monitor --> Alert`
- Mermaid blocks pass through the highlighting pipeline like any other language; the `HighlightingService` falls back to `T:Penn.Highlighting.PlainTextHighlighter` since no Mermaid highlighter is registered
- The rendered output is a standard `<pre><code>` block that client-side Mermaid.js transforms into an SVG diagram
- Note that `CodeTransformer.Transform` is skipped for `markdown`/`md` language blocks (see `CodeHighlightRenderer.Write`), but Mermaid blocks do pass through the transformer -- if no `[!code]` directives are present, the output is unchanged

### Key points
- Mermaid rendering is a client-side concern; Penn outputs the code block and the site's JavaScript initializes Mermaid
- The `MarkdownPipelineBuilder.UseAdvancedExtensions()` call in the pipeline factory enables Mermaid-compatible fenced code block parsing
- Mermaid blocks can be combined with other features (e.g., placed inside a tabbed group with `tabs=true`)

## Beat 3: Combining Alerts with Other Content

Alerts can be used alongside code blocks, tabbed groups, and other markdown content. They serve as contextual callouts that complement technical content.

### What to show
- An alert block placed between code examples to warn about breaking changes or highlight tips
- Alerts inside or adjacent to tabbed code groups to provide language-specific notes
- Multiple alert types used together in a single page to create a layered information hierarchy

### Key points
- Alerts are block-level elements and can appear anywhere in the document flow
- Alert content supports standard markdown formatting including inline code, bold, italic, and links
- All five alert types render with distinct styling via their `markdown-alert-{type}` CSS classes, allowing site themes to differentiate severity levels visually
