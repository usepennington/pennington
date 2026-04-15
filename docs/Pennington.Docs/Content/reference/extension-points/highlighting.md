---
title: "Highlighting interfaces"
description: "The two highlighting extension contracts — ICodeHighlighter and ICodeBlockPreprocessor — plus the HighlightingService dispatcher and the TextMateLanguageRegistry grammar registry."
sectionLabel: "Extension Points"
order: 405050
tags: [highlighting, extension-points, code-blocks, textmate]
uid: reference.extension-points.highlighting
---

The four extension points controlling Pennington's code-block highlighting: `ICodeHighlighter` (the per-language handler contract), `ICodeBlockPreprocessor` (fence interception before highlighting), `HighlightingService` (priority-based dispatcher), and `TextMateLanguageRegistry` (grammar/scope registry for the built-in TextMate highlighter). The highlighter types live in `Pennington.Highlighting`; the preprocessor contract lives in `Pennington.Markdown.Extensions`.

## Overview

| Type | Namespace | Kind | Purpose |
|---|---|---|---|
| `ICodeBlockPreprocessor` | `Pennington.Markdown.Extensions` | interface | Intercepts a fenced block before highlighting and returns pre-highlighted HTML or lets the block pass through. |
| `ICodeHighlighter` | `Pennington.Highlighting` | interface | Declares a language set, a priority, and a highlight method that produces HTML with `<span>` token wrappers. |
| `HighlightingService` | `Pennington.Highlighting` | sealed class | Dispatches `Highlight(code, language)` to the highest-priority `ICodeHighlighter` that claims the language, falling back to `PlainTextHighlighter`. |
| `TextMateLanguageRegistry` | `Pennington.Highlighting` | sealed class | Registers language-to-scope mappings and loads JSON grammars for the built-in `TextMateHighlighter`. |

## `ICodeHighlighter`

The per-language highlighter contract, registered via `HighlightingOptions.AddHighlighter<T>` or `HighlightingOptions.AddHighlighter(instance)` and dispatched by `HighlightingService` in descending `Priority` order. Built-in implementations: `ShellHighlighter` (priority 75), `TextMateHighlighter` (priority 50), `PlainTextHighlighter` (priority 0 fallback).

### Members

<ApiMemberList XmlDocId="T:Pennington.Highlighting.ICodeHighlighter" Kind="All" HeadingLevel="4" />

## `ICodeBlockPreprocessor`

Runs before `HighlightingService` for every fenced block. Implementations inspect the fence's language modifier (such as `csharp:xmldocid` or `csharp:path`) and may return a `CodeBlockPreprocessResult` with already-highlighted HTML, or `null` to pass the block through to the dispatcher. Preprocessors are ordered by descending `Priority`; the first non-null result wins. The shipped implementation is `RoslynCodeBlockPreprocessor` in `Pennington.Roslyn`.

### Members

<ApiMemberList XmlDocId="T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor" Kind="All" HeadingLevel="4" />

### Related type: `CodeBlockPreprocessResult`

Record returned by a successful `TryProcess` call.

<ApiMemberTable XmlDocId="T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult" />

## `HighlightingService`

Registered as a singleton by `AddPennington`. The constructor accepts `IEnumerable<ICodeHighlighter>` (supplied by DI from every highlighter registered through `HighlightingOptions`) and sorts them by descending `Priority` once at construction. Dispatching falls back to `PlainTextHighlighter` when no registered highlighter claims the requested language.

### Members

<ApiMemberList XmlDocId="T:Pennington.Highlighting.HighlightingService" Kind="Methods" HeadingLevel="4" />

## `TextMateLanguageRegistry`

Backs the built-in `TextMateHighlighter` with a grammar lookup and mutable scope registry. Registered as a singleton by `AddPennington` and resolvable from DI for post-registration mutation. The constructor accepts an optional `Action<TextMateLanguageRegistry>` configuration callback. Custom scope mappings and grammars take precedence over the built-in `TextMateSharp` registry when resolving a language identifier.

### Members

<ApiMemberList XmlDocId="T:Pennington.Highlighting.TextMateLanguageRegistry" Kind="Methods" HeadingLevel="4" />

## Example

For a complete custom `ICodeHighlighter` exercising `SupportedLanguages`, `Priority`, and `Highlight` (HTML with `<span class="hljs-*">` tokens), see the walkthrough at <xref:how-to.extensibility.custom-highlighter> and the source at `examples/ExtensibilityLabExample/PipelineHighlighter.cs`.

## See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)
- Related reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](xref:reference.options.auxiliary-options)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
