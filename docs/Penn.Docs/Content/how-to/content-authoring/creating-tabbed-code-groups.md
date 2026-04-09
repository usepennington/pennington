---
title: "Creating Tabbed Code Groups"
description: "Group consecutive fenced code blocks into a tabbed interface using tabs=true to show the same concept in multiple languages"
uid: "penn.how-to.creating-tabbed-code-groups"
order: 14
---

## Beat 1: Basic Tabbed Group

You want to show the same code in multiple languages (e.g., C# and F#) and let the reader switch between them with tabs.

Add `tabs=true` to the info string of consecutive fenced code blocks to group them into a tabbed interface.

### What to show
- Two consecutive fenced code blocks: ` ```csharp tabs=true ` and ` ```fsharp `, each showing the HttpMonitor initialization in their respective language. Only the **first** block in a group needs `tabs=true` — subsequent consecutive code blocks are automatically included in the group
- Reference `T:Penn.Markdown.Extensions.Tabs.TabbedCodeBlocksExtension` which processes the document after parsing: it finds a `FencedCodeBlock` node with `tabs=true` in its arguments, then gathers all consecutive `FencedCodeBlock` siblings and wraps them in a `T:Penn.Markdown.Extensions.Tabs.TabbedCodeBlock` container
- Reference `T:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderer` which generates ARIA-compliant tab markup: `role="tablist"`, `role="tab"` buttons, and tab panels

### Key points
- Only consecutive code blocks are grouped; a paragraph between two `tabs=true` blocks breaks the group
- Reference `T:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions` for CSS class customization (`P:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions.ContainerCss`, `P:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions.TabListCss`, `P:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions.TabButtonCss`, `P:Penn.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions.TabPanelCss`)

## Beat 2: Custom Tab Titles and Language Display Names

Override the default tab label with a custom title, or rely on the built-in language normalizer for display names.

### What to show
- Show that tab titles default to `M:Penn.Markdown.Extensions.Tabs.LanguageNormalizer.GetLanguageName(System.String)` (e.g., "C#", "F#") unless overridden with `title='Custom'`
- Reference `M:Penn.Markdown.Extensions.CodeBlockExtensions.GetArgumentPairs(Markdig.Syntax.FencedCodeBlock)` for parsing `tabs=true` and `title='Custom Title'` from the info string
- Example: ` ```csharp tabs=true title='Service Setup' ` produces a tab labeled "Service Setup" instead of "C#"

### Key points
- `T:Penn.Markdown.Extensions.Tabs.LanguageNormalizer` maps language identifiers to human-friendly names (e.g., `csharp` becomes "C#", `fsharp` becomes "F#", `javascript` becomes "JavaScript")
- The `title` argument takes precedence over the language normalizer when both are present
- Titles are parsed from single-quoted values in the fenced code block info string

## Beat 3: Combining Tabs with Line Directives

Line directives like diff, highlight, focus, and word highlighting work inside tab panels. Each tab panel contains a full `CodeHighlightRenderer` output.

### What to show
- A tabbed group where the C# tab uses `// [!code highlight]` on key lines and the F# tab uses `// [!code focus]` on different lines
- Each tab panel contains a full `CodeHighlightRenderer` output, so all line directives work inside tabs
- The `isInTabGroup` flag in `CodeHighlightRenderer` and `M:Penn.Markdown.Extensions.CodeBlockHtmlBuilder.BuildHtml(System.String,Penn.Markdown.Extensions.CodeHighlightRenderOptions,System.Boolean)` adjusts the wrapper CSS (no standalone container class when inside a tab)

### Key points
- The three-step code block pipeline (highlight, transform, wrap) is the same whether the block is standalone or inside a tab group
- Diff, highlight, focus, error, warning, and word directives all compose correctly within tab panels
- The wrapper CSS changes when a code block is inside a tab group to avoid double-wrapping
