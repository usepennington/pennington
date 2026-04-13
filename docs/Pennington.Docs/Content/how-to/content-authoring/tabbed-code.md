---
title: "Create tabbed code groups"
description: "Mark adjacent fenced blocks with `tabs=true title=\"…\"` so Pennington groups them into a single tabbed widget, and customize the rendered CSS classes via `TabbedCodeBlockRenderOptions`."
section: content-authoring
order: 30
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

- Outline: one sentence — reader has a markdown page and wants the same code example presented in several languages/configs inside one visual tab strip.
- Outline: one sentence — do not retread the markdown pipeline setup; link to the alerts how-to for the parallel recipe.

## Assumptions

- Bullet: existing Pennington site with at least one markdown content source (link to Getting Started tutorial).
- Bullet: `AddPennington` / `AddDocSite` already registered so `MarkdownPipelineFactory.CreateWithExtensions` is in effect — this is what wires `UseTabbedCodeBlocks` on top of the default Markdig pipeline.
- Bullet: reader is comfortable authoring fenced code blocks with Markdig attribute syntax (the same `{.lang key=value}` info-string shape used by `UseAdvancedExtensions`).
- Bullet: CSS for the default class names (`tab-container`, `tab-list`, `tab-button`, `tab-panel`) is supplied automatically if the site is using `Pennington.MonorailCss`; otherwise the reader provides their own.

---

## Steps

### 1. Mark the first fence with `tabs=true` and a `title`

- Bullet: the attribute pair is parsed by `CodeBlockExtensions.GetArgumentPairs` — it accepts `key=value` and `key="value with spaces"` only; bracketed labels like `[C#]` are not recognized.
- Bullet: `tabs=true` is the opt-in flag that `TabbedCodeBlocksExtension` scans for during `DocumentProcessed`; blocks without it pass through as ordinary highlighted code.
- Bullet: `title="..."` is the tab label; if omitted, `LanguageNormalizer.GetLanguageName(codeBlock.Info)` falls back to the language name derived from the fence's language identifier.

```markdown
```csharp tabs=true title="C#"
builder.Services.AddBeacon(options =>
{
    options.DefaultInterval = TimeSpan.FromMinutes(5);
});
```
```

### 2. Place additional fences directly after it (no blank separator needed)

- Bullet: `TabbedCodeBlocksExtension` groups *consecutive* `FencedCodeBlock`s — any non-code block (paragraph, heading, blank line rendered as a paragraph break) terminates the group.
- Bullet: only the *first* fence needs `tabs=true`; subsequent fences are absorbed automatically regardless of their own attributes.
- Bullet: each subsequent fence should carry its own `title="…"`; otherwise every tab button falls back to a language-derived label.
- Bullet: if only one fence carries `tabs=true` (no adjacent followup), the block is emitted as a plain highlighted code block — no empty tab widget is rendered.

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

### 3. (Optional) Override the emitted CSS classes

- Bullet: `TabbedCodeBlockRenderOptions` is a public record in `Pennington.Markdown.Extensions.Tabs` with five required `init` properties — `OuterWrapperCss`, `ContainerCss`, `TabListCss`, `TabButtonCss`, `TabPanelCss`.
- Bullet: the defaults (`TabbedCodeBlockRenderOptions.Default`) are `not-prose` / `tab-container` / `tab-list` / `tab-button` / `tab-panel` — these match the utility classes `Pennington.MonorailCss` bundles via `MonorailCssOptions.TabApplies`.
- Bullet: there is currently no `PenningtonOptions` surface for these render options; to customize, re-register `MarkdownPipeline` after `AddPennington` and pass a `tabOptions` factory to `MarkdownPipelineFactory.CreateWithExtensions`.
- Bullet: plain C# snippet illustrating the replacement — this does not point at an existing example project because no shipped sample overrides the defaults.

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

- Bullet: run `dotnet run --project examples/BeaconDocsExample` and open a page containing adjacent fences marked `tabs=true` — expect one panel with a tab button per fence.
- Bullet: inspect the HTML — the group is one `<div class="not-prose">` wrapping `<div class="tab-container">`, a `<div role="tablist">`, and one `<div ... class="tab-panel">` per fence; the first tab has `aria-selected="true"` and `data-state="active"`.
- Bullet: if only one fence is present, the output is a normal `<pre>` block rather than a tab widget — confirming the "needs ≥ 2 adjacent fences" rule.

## Related

- Reference: Markdown extensions reference entry for `UseTabbedCodeBlocks` and the `tabs=true` / `title` attributes parsed by `CodeBlockExtensions.GetArgumentPairs`.
- Reference: `TabbedCodeBlockRenderOptions` record — default values and the five `*Css` members.
- Background: Explanation of how `MarkdownPipelineFactory.CreateWithExtensions` composes the Pennington-specific Markdig extensions on top of `UseAdvancedExtensions`.
