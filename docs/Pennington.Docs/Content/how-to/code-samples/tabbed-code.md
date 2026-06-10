---
title: "Group adjacent code fences into a tabbed sample"
description: "Collapse adjacent fenced code blocks into one tabbed widget and customize the rendered CSS class names."
uid: how-to.code-samples.tabbed-code
order: 2
sectionLabel: "Code Samples"
tags: [markdown, tabs, code-blocks, extensions]
---

When two or more code variants show the same operation — bash vs. PowerShell, a `csproj` property vs. its CLI equivalent, C# vs. F# — a tabbed group lets the audience pick one without scrolling past the others. Author each variant as a normal fenced code block with `tabs=true title="..."` in the info string and Pennington collapses adjacent matches into a single ARIA tablist. For the info-string grammar, see <xref:reference.markdown.code-block-args>.

## Before you begin
- An existing Pennington site rendering markdown (see <xref:tutorials.getting-started.first-site> if not).
- The host wires the default Pennington markdown pipeline, which already enables `UseTabbedCodeBlocks` under `AddDocSite`, `AddBlogSite`, or bare `AddPennington`.
- Familiarity with the fence info-string shape (language token plus key/value attributes) — the reference page above covers the grammar.

## Tabs and labels

Each H3 below shows the source markdown above its rendered result. In the first, adjacent fences collapse into one tablist — the first tab is active by default, and switching tabs reveals the matching panel. In the second, intervening prose splits the fences into two separate widgets.

### Adjacent fences become tabs

Author two or more fenced blocks back-to-back, each with `tabs=true title="..."` in the info string. Consecutive matches collapse into one tablist; the `title` value becomes the tab label.

`````markdown
````bash tabs=true title="bash"
dotnet add package Pennington
````

````powershell tabs=true title="PowerShell"
Install-Package Pennington
````
`````

```bash tabs=true title="bash"
dotnet add package Pennington
```

```powershell tabs=true title="PowerShell"
Install-Package Pennington
```

### Prose between fences splits the group

The grouping logic only collapses fences that sit next to each other in the block stream. A paragraph, heading, or blank-lined HTML element between two fences splits the group into two separate tablists. To keep one widget, remove the intervening block.

`````markdown
````bash tabs=true title="bash"
echo "first group"
````

A paragraph here ends the first tablist.

````bash tabs=true title="bash"
echo "second group"
````
`````

```bash tabs=true title="bash"
echo "first group"
```

A paragraph here ends the first tablist.

```bash tabs=true title="bash"
echo "second group"
```

## Verify

- Run `dotnet run` and load the page with the back-to-back fences. They render as one widget with a tab per `title`, and clicking a tab swaps the visible panel.
- View source: the group is a single `<div>` carrying the tablist classes, with one `<button role="tab">` per fence and one panel each. A fence split by intervening prose produces a second, separate `<div>`.

## Customize the tab CSS classes

The rendered HTML draws its CSS class names from `TabbedCodeBlockRenderOptions`. The `Default` instance ships with `not-prose` on the outer wrapper plus `tab-container`, `tab-list`, `tab-button`, and `tab-panel` on the nested elements — enough for the [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) preset to style them without extra work.

To override the class names, set `PenningtonOptions.TabbedCodeBlockOptions` to a `Func<TabbedCodeBlockRenderOptions>` returning a modified `with` expression.

```csharp:symbol,bodyonly
examples/ExtensibilityLabExample/TabbedCodeBlockStyling.cs > TabbedCodeBlockStyling.ConfigureTabbedCodeBlocksOverride
```

See <xref:reference.markdown.extensions> for the full `TabbedCodeBlockRenderOptions` surface.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the tabs extension alongside every other non-CommonMark feature
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the info-string grammar that carries `tabs=true title="..."`
- Reference: [Content components](xref:reference.ui.content) — the Pennington.UI `<Tabs>` component for non-code tabsets
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the render options flow through one pipeline in both modes
