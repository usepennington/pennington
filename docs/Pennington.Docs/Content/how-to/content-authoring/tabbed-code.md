---
title: "Create tabbed code groups"
description: "Mark adjacent fenced blocks with `tabs=true title=\"…\"` so Pennington groups them into a single tabbed widget."
section: content-authoring
order: 50
uid: how-to.content-authoring.tabbed-code
isDraft: true
search: false
llms: false
tags: []
---

> **In this page.** Marking a fenced block with `tabs=true title="…"`, grouping adjacent blocks into a single tabbed widget, and customizing the rendered CSS classes via `TabbedCodeBlockRenderOptions`.
>
> **Not in this page.** The UI-component `<Tabs>` equivalent from Pennington.UI or per-tab analytics.

## When to use this

When a page needs the same example presented in several languages or configs inside one visual tab strip — one C# snippet alongside the matching `appsettings.json` and `values.yaml`, for example.

## Assumptions

- You have an existing Pennington site with at least one markdown content source (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- `AddPennington` or `AddDocSite` is already registered.
- You are comfortable authoring fenced code blocks with Markdig attribute syntax (`{.lang key=value}`).
- If the site uses `Pennington.MonorailCss`, CSS for the default class names (`tab-container`, `tab-list`, `tab-button`, `tab-panel`) is supplied automatically; otherwise you provide your own.

---

## Steps

### 1. Mark the first fence with `tabs=true` and a `title`

Set `tabs=true` as the opt-in flag and `title="…"` as the tab label. Attributes use `key=value` or `key="value with spaces"`. If `title` is omitted, the tab label falls back to the language name derived from the fence.

```markdown
```csharp tabs=true title="C#"
builder.Services.AddBeacon(options =>
{
    options.DefaultInterval = TimeSpan.FromMinutes(5);
});
```
```

### 2. Place additional fences directly after it

Consecutive fences are grouped automatically — any non-code block (paragraph, heading, blank line rendered as a paragraph break) terminates the group. Only the first fence needs `tabs=true`; subsequent fences are absorbed regardless of their own attributes. Give each fence its own `title="…"` or it falls back to a language-derived label.

If no adjacent fence follows, the block renders as a plain highlighted code block — no empty tab widget.

```markdown
```csharp tabs=true title="C#"
builder.Services.AddBeacon(options => { /* ... */ });
```
```json title="appsettings.json"
{
  "Beacon": { "DefaultInterval": "00:05:00" }
}
```
```yaml title="values.yaml"
beacon:
  defaultInterval: 00:05:00
```
```

### 3. Override the emitted CSS classes

`TabbedCodeBlockRenderOptions` is a public record with five `init` properties — `OuterWrapperCss`, `ContainerCss`, `TabListCss`, `TabButtonCss`, `TabPanelCss`. Defaults are `not-prose` / `tab-container` / `tab-list` / `tab-button` / `tab-panel`, matching the utility classes `Pennington.MonorailCss` bundles. To customize, register a custom `MarkdownPipeline` after `AddPennington` that passes a `tabOptions` factory through the pipeline factory.

```csharp
using Pennington.Highlighting;
using Pennington.Markdown;
using Pennington.Markdown.Extensions.Tabs;
using Markdig;

builder.Services.AddPennington(penn => { /* existing setup */ });

// Replace the default MarkdownPipeline with one that carries custom tab classes.
builder.Services.AddSingleton<MarkdownPipeline>(sp =>
    MarkdownPipelineFactory.CreateWithExtensions(
        sp.GetRequiredService<HighlightingService>(),
        tabOptions: () => new TabbedCodeBlockRenderOptions
        {
            OuterWrapperCss = "not-prose my-6",
            ContainerCss = "rounded-xl border shadow",
            TabListCss = "flex gap-2 px-4 pt-2 border-b",
            TabButtonCss = "px-3 py-2 text-sm data-[state=active]:font-semibold",
            TabPanelCss = "p-4 hidden data-[state=active]:block",
        }));
```

---

## Verify

- Run `dotnet run --project examples/BeaconDocsExample` and open a page containing adjacent fences marked `tabs=true` — expect one panel with a tab button per fence.
- Inspect the HTML — the group is one `<div class="not-prose">` wrapping `<div class="tab-container">`, a `<div role="tablist">`, and one `<div ... class="tab-panel">` per fence. The first tab carries `aria-selected="true"` and `data-state="active"`.
- With only one fence, the output is a normal `<pre>` block — confirming the "needs ≥ 2 adjacent fences" rule.

## Related

- Reference: [Markdown extensions](/reference/markdown/extensions) — the `tabs=true` / `title` attributes.
- Reference: [Code-block argument reference](/reference/markdown/code-block-args)
- Background: [Syntax highlighting](/explanation/rendering/highlighting)
