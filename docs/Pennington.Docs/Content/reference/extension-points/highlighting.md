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

```csharp:xmldocid
T:Pennington.Highlighting.ICodeHighlighter
```

The per-language highlighter contract, registered via `HighlightingOptions.AddHighlighter<T>` or `HighlightingOptions.AddHighlighter(instance)` and dispatched by `HighlightingService` in descending `Priority` order. Built-in implementations: `ShellHighlighter` (priority 75), `TextMateHighlighter` (priority 50), `PlainTextHighlighter` (priority 0 fallback).

### Members

| Name | Signature | Description |
|---|---|---|
| `Highlight` | `string Highlight(string code, string language)` | Returns HTML for the supplied source, with token `<span>` wrappers carrying the `hljs-*` CSS classes consumed by the stylesheet. |
| `Priority` | `int { get; }` | Dispatcher ranking; higher values win when multiple highlighters claim the same language. `ShellHighlighter` uses 75, `TextMateHighlighter` uses 50, `PlainTextHighlighter` uses 0. |
| `SupportedLanguages` | `IReadOnlySet<string> { get; }` | The language identifiers this highlighter claims (for example, `"csharp"`, `"python"`). Returning a set that contains `"*"` matches every language. |

## `ICodeBlockPreprocessor`

```csharp:xmldocid
T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor
```

Runs before `HighlightingService` for every fenced block. Implementations inspect the fence's language modifier (such as `csharp:xmldocid` or `csharp:path`) and may return a `CodeBlockPreprocessResult` with already-highlighted HTML, or `null` to pass the block through to the dispatcher. Preprocessors are ordered by descending `Priority`; the first non-null result wins. The shipped implementation is `RoslynCodeBlockPreprocessor` in `Pennington.Roslyn`.

### Members

| Name | Signature | Description |
|---|---|---|
| `Priority` | `int { get; }` | Run-order ranking; higher values are consulted first. |
| `TryProcess` | `CodeBlockPreprocessResult? TryProcess(string code, string languageId)` | Returns a `CodeBlockPreprocessResult` to take over the block or `null` to pass through to the next preprocessor and ultimately `HighlightingService`. |

### Related type: `CodeBlockPreprocessResult`

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult
```

Record returned by a successful `TryProcess` call.

| Name | Type | Default | Description |
|---|---|---|---|
| `BaseLanguage` | `string` | — | The language identifier used for the output block's CSS class (such as `csharp`) so stylesheet rules key off the base language rather than the modifier. |
| `HighlightedHtml` | `string` | — | The fully highlighted HTML, wrapped in the `<pre><code>` tags the renderer emits directly. |
| `SkipTransform` | `bool` | `false` | When `true`, bypasses `CodeTransformer` (tab stripping, empty-line normalization) on the output. |

## `HighlightingService`

```csharp:xmldocid
T:Pennington.Highlighting.HighlightingService
```

Registered as a singleton by `AddPennington`. The constructor accepts `IEnumerable<ICodeHighlighter>` (supplied by DI from every highlighter registered through `HighlightingOptions`) and sorts them by descending `Priority` once at construction. Dispatching falls back to `PlainTextHighlighter` when no registered highlighter claims the requested language.

### Members

| Name | Signature | Description |
|---|---|---|
| `HasHighlighter` | `bool HasHighlighter(string language)` | Returns `true` when at least one non-fallback highlighter's `SupportedLanguages` contains the supplied identifier; wildcard `"*"` entries are not consulted by this probe. |
| `Highlight` | `string Highlight(string code, string language)` | Selects the highest-priority `ICodeHighlighter` whose `SupportedLanguages` contains `language` (or `"*"`) and returns its output; falls back to `PlainTextHighlighter` (HTML-encoded code, no token spans) when none match. |

## `TextMateLanguageRegistry`

```csharp:xmldocid
T:Pennington.Highlighting.TextMateLanguageRegistry
```

Backs the built-in `TextMateHighlighter` with a grammar lookup and mutable scope registry. Registered as a singleton by `AddPennington` and resolvable from DI for post-registration mutation. The constructor accepts an optional `Action<TextMateLanguageRegistry>` configuration callback. Custom scope mappings and grammars take precedence over the built-in `TextMateSharp` registry when resolving a language identifier.

### Members

| Name | Signature | Description |
|---|---|---|
| `AddGrammar` | `TextMateLanguageRegistry AddGrammar(string languageId, string scopeName)` | Registers a language id → TextMate scope name mapping so a built-in or previously loaded grammar can be selected by the supplied `languageId`. Returns `this` for chaining. |
| `AddGrammarFromJson` | `TextMateLanguageRegistry AddGrammarFromJson(string languageId, string grammarJson)` | Loads a TextMate grammar from a JSON string, reads its `scopeName` (falling back to `source.{languageId}`), and registers both the grammar and the id-to-scope mapping. Returns `this` for chaining. |

## Example

A custom `ICodeHighlighter` showing the full contract surface: a `SupportedLanguages` set, a `Priority` value higher than any built-in highlighter it overrides, and a `Highlight` method returning HTML with `<span class="hljs-*">` tokens.

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.PipelineHighlighter
```

## See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)
- Related reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](xref:reference.options.auxiliary-options)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
