---
title: "Create tabbed code groups"
description: "Collapse adjacent fenced code blocks into one tabbed widget and customize the rendered CSS class names."
uid: how-to.content-authoring.tabbed-code
order: 50
sectionLabel: Content Authoring
tags: [markdown, tabs, code-blocks, extensions]
---

> **In this page.** _Paraphrase TOC "Covers": marking a fenced block with `tabs=true title="…"`, grouping adjacent blocks into one tabbed widget, and customizing the rendered CSS classes via `TabbedCodeBlockRenderOptions`._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": the UI-component `<Tabs>` equivalent from Pennington.UI is a separate surface (see [Content components reference](/reference/ui/content)), and per-tab analytics are out of scope._

## When to use this

_Two sentences. Frame the goal: the reader has two or three code variants that show the same thing (bash vs. PowerShell, csproj vs. CLI, C# vs. F#) and wants them to share a single tablist instead of stacking. Do not re-teach markdown fences — link to [Code-block argument reference](/reference/markdown/code-block-args) for the info-string grammar._

## Assumptions

_Keep to three bullets. Each is a realistic prior state, not a tutorial step._

- You have an existing Pennington site rendering markdown (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- You know the fence info-string shape (language token plus key/value attributes) — the reference page above covers the grammar.
- Your host wires the default Pennington markdown pipeline, which already enables `UseTabbedCodeBlocks` under `AddDocSite`, `AddBlogSite`, or bare `AddPennington`.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/tabbed-code.md` is the fixture page this how-to fences from.

---

## Steps

_Four steps. Each is one imperative action with a single fence._

### 1. Mark adjacent fences with `tabs=true title="…"`

_One sentence: add `tabs=true` and a `title="…"` attribute to the info string of two or more adjacent fenced code blocks — the extension walks the document, finds consecutive `FencedCodeBlock`s whose `tabs` attribute equals `"true"`, and folds them into a single tablist. The `title` value becomes the tab label; the language token before the attributes still drives highlighting._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/tabbed-code.md
```

### 2. Keep the blocks adjacent — no prose in between

_One sentence: the grouping logic only collapses fences that sit next to each other in the block stream, so a paragraph, heading, or blank-lined HTML element between two fences breaks the group into two separate tablists. The first tab in each group renders active by default._

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlock
```

### 3. Inspect the default render options

_One sentence: the rendered HTML gets its CSS class names from `TabbedCodeBlockRenderOptions`. The `Default` instance ships with `not-prose` on the outer wrapper plus `tab-container`, `tab-list`, `tab-button`, and `tab-panel` on the nested elements — enough for the MonorailCSS preset to style them without extra work._

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions
```

### 4. Override the class names when you need custom CSS

_Two sentences: pass a `Func<TabbedCodeBlockRenderOptions>` into `UseTabbedCodeBlocks` (or supply one to `MarkdownPipelineFactory.CreateWithExtensions` via the `tabOptions` parameter) so the renderer picks up your values on every render. On an `AddPennington` host, do the override inside `ConfigureMarkdownPipeline` where you already have access to the `MarkdownPipelineBuilder`._

<!-- TODO: no existing example helper in examples/ configures TabbedCodeBlockRenderOptions — the DocSiteKitchenSinkExample.ServiceConfiguration surface does not cover it. Add a `ConfigureTabbedCodeBlockRenderOptions` helper (or similar) to an existing example (likely DocSiteKitchenSinkExample or ExtensibilityLabExample), then replace this fence with its xmldocid. -->

```csharp:xmldocid
TODO:M:DocSiteKitchenSinkExample.ServiceConfiguration.ConfigureTabbedCodeBlockRenderOptions
```

---

## Verify

_Three terse bullets. Each is one observable check._

- Run `dotnet run` and visit the rendered page — the adjacent fences show a tablist with one tab per `title="…"` value.
- Click each tab — only the matching panel is visible, and the first tab is active on load.
- If you overrode `TabbedCodeBlockRenderOptions`, the emitted HTML uses your class names (inspect with browser dev-tools on `.tab-container` or its replacement).

## Related

- Reference: [Markdown extensions catalog](/reference/markdown/extensions) — the tabs extension alongside every other non-CommonMark feature
- Reference: [Code-block argument reference](/reference/markdown/code-block-args) — the info-string grammar that carries `tabs=true title="…"`
- Reference: [Content components](/reference/ui/content) — the Pennington.UI `<Tabs>` component for non-code tabsets
- Background: [Dev mode and build mode share one code path](/explanation/core/dev-vs-build) — why the render options flow through one pipeline in both modes
