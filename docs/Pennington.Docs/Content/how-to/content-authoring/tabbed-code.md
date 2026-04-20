---
title: "Group adjacent code fences into a tabbed sample"
description: "Collapse adjacent fenced code blocks into one tabbed widget and customise the rendered CSS class names."
uid: how-to.content-authoring.tabbed-code
order: 201050
sectionLabel: Content Authoring
tags: [markdown, tabs, code-blocks, extensions]
---

When two or more code variants show the same operation — bash vs. PowerShell, a `csproj` property vs. its CLI equivalent, C# vs. F# — a tabbed group lets the audience pick one without scrolling past the others. Author each variant as a normal fenced code block with `tabs=true title="..."` in the info string and Pennington collapses adjacent matches into a single ARIA tablist. For the info-string grammar, see <xref:reference.markdown.code-block-args>.

## Assumptions

- An existing Pennington site rendering markdown (see <xref:tutorials.getting-started.first-site> if not).
- The host wires the default Pennington markdown pipeline, which already enables `UseTabbedCodeBlocks` under `AddDocSite`, `AddBlogSite`, or bare `AddPennington`.
- Familiarity with the fence info-string shape (language token plus key/value attributes) — the reference page above covers the grammar.

## Tabs and labels

Each H3 below shows the source markdown above the rendered widget. The first tab is active by default; switching tabs reveals the matching panel.

### Adjacent fences become tabs

Author two or more fenced blocks back-to-back, each with `tabs=true title="..."` in the info string. The extension walks the document, finds consecutive `FencedCodeBlock`s whose `tabs` attribute is `"true"`, and folds them into one tablist. The `title` value becomes the tab label; the language token before the attributes still drives syntax highlighting.

`````markdown
````bash tabs=true title="bash"
dotnet add package Pennington
````

````powershell tabs=true title="PowerShell"
Install-Package Pennington
````

````xml tabs=true title="csproj"
<PackageReference Include="Pennington" Version="1.0.0" />
````
`````

```bash tabs=true title="bash"
dotnet add package Pennington
```

```powershell tabs=true title="PowerShell"
Install-Package Pennington
```

```xml tabs=true title="csproj"
<PackageReference Include="Pennington" Version="1.0.0" />
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

## What the renderer emits

The rendered HTML draws its CSS class names from `TabbedCodeBlockRenderOptions`. The `Default` instance ships with `not-prose` on the outer wrapper plus `tab-container`, `tab-list`, `tab-button`, and `tab-panel` on the nested elements — enough for the MonorailCSS preset to style them without extra work.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlock
```

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions
```

To override the class names, set `PenningtonOptions.TabbedCodeBlockOptions` to a `Func<TabbedCodeBlockRenderOptions>` returning a modified `with` expression. The factory replaces the `Default` shape on the pipeline's single registration of the tabbed extension, so every rendered page picks up the new class names. This works identically on `AddPennington`, `AddDocSite`, and `AddBlogSite` because each surface plumbs the same property through to the pipeline factory.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.TabbedCodeBlockStyling.ConfigureTabbedCodeBlocksOverride(Pennington.Infrastructure.PenningtonOptions)
```

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the tabs extension alongside every other non-CommonMark feature
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — the info-string grammar that carries `tabs=true title="..."`
- Reference: [Content components](xref:reference.ui.content) — the Pennington.UI `<Tabs>` component for non-code tabsets
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the render options flow through one pipeline in both modes
