---
title: "Writing Markdown with Pennington Extensions"
description: "Overview of Pennington's markdown extensions for code blocks, tabs, snippets, alerts, and diagrams"
uid: "penn.how-to.writing-markdown-with-penn-extensions"
order: 10
---

## Beat 1: The Markdown Pipeline

Pennington extends standard Markdown with syntax highlighting, line-level annotations, tabbed code groups, snippet regions, alert blocks, and Mermaid diagrams. This page covers the basics; see the linked guides for specific features.

### What to show
- Front matter block with `title`, `description`, `order` fields parsed by `T:Pennington.FrontMatter.FrontMatterParser` via `M:Pennington.FrontMatter.FrontMatterParser.Parse``1(System.String)`
- Reference `M:Pennington.Markdown.MarkdownPipelineFactory.CreateWithExtensions(Pennington.Highlighting.HighlightingService,System.Func{Pennington.Markdown.Extensions.CodeHighlightRenderOptions},System.Func{Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions},System.Collections.Generic.IEnumerable{Pennington.Markdown.Extensions.ICodeBlockPreprocessor})` to explain that a single factory call wires syntax highlighting, tabbed code blocks, and custom alerts together

### Key points
- The pipeline is assembled once and reused for all pages; `MarkdownPipelineFactory` is the single entry point
- `T:Pennington.Highlighting.HighlightingService` dispatches to the best available `T:Pennington.Highlighting.ICodeHighlighter` for each language via `M:Pennington.Highlighting.HighlightingService.Highlight(System.String,System.String)`

## Beat 2: Syntax-Highlighted Code Blocks

Show a fenced code block with a language identifier. Explain how `CodeHighlightRenderer` extracts the language, calls the highlighting service, and wraps the result.

### What to show
- A fenced code block with ` ```csharp ` showing a code snippet
- Reference `T:Pennington.Markdown.Extensions.CodeHighlightRenderer` and its three-step pipeline: (1) highlight via `HighlightingService.Highlight`, (2) transform via `T:Pennington.Markdown.Extensions.CodeTransformer`, (3) wrap via `T:Pennington.Markdown.Extensions.CodeBlockHtmlBuilder`
- Show the rendered output with syntax coloring applied
- Reference `T:Pennington.Markdown.Extensions.CodeHighlightRenderOptions` for CSS class customization (`P:Pennington.Markdown.Extensions.CodeHighlightRenderOptions.OuterWrapperCss`, `P:Pennington.Markdown.Extensions.CodeHighlightRenderOptions.StandaloneContainerCss`)

### Key points
- Language identifiers map through `T:Pennington.Markdown.Extensions.Tabs.LanguageNormalizer` for display names (e.g., `csharp` becomes "C#")
- The `:path` modifier on a language identifier is stripped by `CodeHighlightRenderer.ParseBaseLanguage` before highlighting

## Beat 3: Preprocessor Extension Point and Further Reading

For advanced scenarios where custom languages need special handling before the standard pipeline, Pennington provides the `ICodeBlockPreprocessor` interface.

### What to show
- Reference `T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor` and its contract: `P:Pennington.Markdown.Extensions.ICodeBlockPreprocessor.Priority` (higher runs first) and `M:Pennington.Markdown.Extensions.ICodeBlockPreprocessor.TryProcess(System.String,System.String)` which returns `T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult` or null to pass through
- Show that preprocessors are sorted by `Priority` descending in `CodeHighlightRenderer`
- The `P:Pennington.Markdown.Extensions.CodeBlockPreprocessResult.SkipTransform` flag allows a preprocessor to bypass `CodeTransformer.Transform` entirely
- The full file path `:path src/Pennington/Markdown/MarkdownPipelineFactory.cs` as the central wiring point

### Key points
- This is the extension point for language-specific rendering (e.g., a future Roslyn highlighter intercepting `csharp:xmldocid` blocks)
- Preprocessors are passed to `MarkdownPipelineFactory.CreateWithExtensions` via the `preprocessors` parameter

## See also

- [Annotating Code Blocks with Line Directives](xref:penn.how-to.annotating-code-blocks) -- diff, highlight, focus, error, warning, and word highlighting directives
- [Creating Tabbed Code Groups](xref:penn.how-to.creating-tabbed-code-groups) -- group consecutive code blocks into a tabbed interface
- [Using Snippet Regions in Code Blocks](xref:penn.how-to.using-snippet-regions) -- show only relevant portions of a source file
- [Using Alerts and Mermaid Diagrams](xref:penn.how-to.using-alerts-and-diagrams) -- callout boxes and inline architecture diagrams
