---
title: "Create tabbed code groups"
description: "Collapse adjacent fenced code blocks into one tabbed widget and customize the rendered CSS class names."
uid: how-to.content-authoring.tabbed-code
order: 201050
sectionLabel: Content Authoring
tags: [markdown, tabs, code-blocks, extensions]
---

When two or more code variants show the same operation — bash vs. PowerShell, a `csproj` property vs. its CLI equivalent, C# vs. F# — tabbed code groups let the audience pick a variant without scrolling past alternatives. For the info-string grammar that drives this feature, see <xref:reference.markdown.code-block-args>.

## Assumptions

- An existing Pennington site rendering markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- Familiarity with the fence info-string shape (language token plus key/value attributes) — the reference page above covers the grammar.
- The host wires the default Pennington markdown pipeline, which already enables `UseTabbedCodeBlocks` under `AddDocSite`, `AddBlogSite`, or bare `AddPennington`.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/tabbed-code.md` is the fixture page this how-to fences from.

---

## Steps

### 1. Mark adjacent fences with `tabs=true title="..."`

Add `tabs=true` and a `title="..."` attribute to the info string of two or more adjacent fenced code blocks. The extension walks the document, finds consecutive `FencedCodeBlock`s whose `tabs` attribute equals `"true"`, and folds them into a single tablist. The `title` value becomes the tab label; the language token before the attributes still drives syntax highlighting.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/tabbed-code.md
```

### 2. Keep the blocks adjacent — no prose in between

The grouping logic only collapses fences that sit next to each other in the block stream. A paragraph, heading, or blank-lined HTML element between two fences splits the group into two separate tablists. The first tab in each group renders active by default.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlock
```

### 3. Inspect the default render options

The rendered HTML gets its CSS class names from `TabbedCodeBlockRenderOptions`. The `Default` instance ships with `not-prose` on the outer wrapper plus `tab-container`, `tab-list`, `tab-button`, and `tab-panel` on the nested elements — enough for the MonorailCSS preset to style them without extra work.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions
```

### 4. Override the class names for custom CSS

Set `PenningtonOptions.TabbedCodeBlockOptions` to a `Func<TabbedCodeBlockRenderOptions>` returning a modified `with` expression. The factory replaces the `Default` shape on the pipeline's single registration of the tabbed extension, so every rendered page picks up the new class names. This works identically on `AddPennington`, `AddDocSite`, and `AddBlogSite` because each surface plumbs the same property through to the pipeline factory.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.TabbedCodeBlockStyling.ConfigureTabbedCodeBlocksOverride(Pennington.Infrastructure.PenningtonOptions)
```

---

## Verify

- Run `dotnet run` and visit the rendered page — the adjacent fences show a tablist with one tab per `title="..."` value.
- Click each tab — only the matching panel is visible, and the first tab is active on load.
- When `TabbedCodeBlockRenderOptions` is overridden, the emitted HTML uses the custom class names (inspect with browser dev-tools on `.tab-container` or its replacement).

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the tabs extension alongside every other non-CommonMark feature
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the info-string grammar that carries `tabs=true title="..."`
- Reference: [Content components](xref:reference.ui.content) — the Pennington.UI `<Tabs>` component for non-code tabsets
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the render options flow through one pipeline in both modes
