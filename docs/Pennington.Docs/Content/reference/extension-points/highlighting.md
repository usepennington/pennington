---
title: "Highlighting interfaces"
description: "The two highlighting extension contracts — ICodeHighlighter and ICodeBlockPreprocessor — plus the HighlightingService dispatcher and the TextMateLanguageRegistry grammar registry."
sectionLabel: "Extension Points"
order: 405050
tags: [highlighting, extension-points, code-blocks, textmate]
uid: reference.extension-points.highlighting
---

> **In this page.** `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`.
>
> **Not in this page.** Writing TextMate grammars — see the upstream TextMateSharp documentation.

## Summary

_**One sentence: what it is.** The four extension points that together control Pennington's code-block highlighting: the `ICodeHighlighter` contract that every language handler implements, the `ICodeBlockPreprocessor` contract that intercepts fences before highlighting, the `HighlightingService` dispatcher that picks a highlighter by priority, and the `TextMateLanguageRegistry` that owns grammar/scope mappings for the built-in `TextMateHighlighter`._
_**One sentence: where it lives.** Namespaces `Pennington.Highlighting` (`src/Pennington/Highlighting/`) for the highlighter contract, dispatcher, and grammar registry; `Pennington.Markdown.Extensions` (`src/Pennington/Markdown/Extensions/`) for the preprocessor contract._

## Overview

_Four-row table keyed by type. Columns: **Type**, **Namespace**, **Kind**, **Purpose**. One-sentence purposes only — this is the landing index for the four types bundled on this page. Alphabetical within each kind grouping (interfaces, then classes)._

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

_The per-language highlighter contract. Implementations are registered via `HighlightingOptions.AddHighlighter<T>` or `HighlightingOptions.AddHighlighter(instance)` and dispatched by `HighlightingService` in descending `Priority` order. Built-in implementations: `ShellHighlighter` (priority 75), `TextMateHighlighter` (priority 50), `PlainTextHighlighter` (priority 0 fallback)._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `Highlight` | `string Highlight(string code, string language)` | Returns HTML for the supplied source, with token `<span>` wrappers carrying the `hljs-*` CSS classes consumed by the stylesheet. |
| `Priority` | `int { get; }` | Dispatcher ranking; higher values win when multiple highlighters claim the same language. `ShellHighlighter` uses 75, `TextMateHighlighter` uses 50, `PlainTextHighlighter` uses 0. |
| `SupportedLanguages` | `IReadOnlySet<string> { get; }` | The language identifiers this highlighter claims (e.g., `"csharp"`, `"python"`). Returning a set that contains `"*"` matches every language. |

## `ICodeBlockPreprocessor`

```csharp:xmldocid
T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor
```

_Runs before `HighlightingService` for every fenced block. Implementations inspect the fence's language modifier (e.g., `csharp:xmldocid`, `csharp:path`) and may return a `CodeBlockPreprocessResult` with already-highlighted HTML, or `null` to pass the block through to the dispatcher. Preprocessors are ordered by descending `Priority`; the first non-null result wins. The shipped implementation is `RoslynCodeBlockPreprocessor` in `Pennington.Roslyn`._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `Priority` | `int { get; }` | Run-order ranking; higher values are consulted first. |
| `TryProcess` | `CodeBlockPreprocessResult? TryProcess(string code, string languageId)` | Returns a `CodeBlockPreprocessResult` to take over the block or `null` to pass through to the next preprocessor and ultimately `HighlightingService`. |

### Related type: `CodeBlockPreprocessResult`

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult
```

_Record returned by a successful `TryProcess` call._

| Name | Type | Description |
|---|---|---|
| `BaseLanguage` | `string` | The language identifier used for the output block's CSS class (e.g., `csharp`) so stylesheet rules key off the base language rather than the modifier. |
| `HighlightedHtml` | `string` | The fully highlighted HTML, wrapped in the `<pre><code>` tags the renderer will emit directly. |
| `SkipTransform` | `bool` | When `true`, bypasses `CodeTransformer` (tab stripping, empty-line normalization) on the output; defaults to `false`. |

## `HighlightingService`

```csharp:xmldocid
T:Pennington.Highlighting.HighlightingService
```

_The dispatcher registered as a singleton by `AddPennington`. Constructor takes `IEnumerable<ICodeHighlighter>` (supplied by DI from every highlighter registered through `HighlightingOptions`) and sorts them by descending `Priority` once at construction. Dispatching falls back to `PlainTextHighlighter` when no registered highlighter claims the requested language._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `HasHighlighter` | `bool HasHighlighter(string language)` | Returns `true` when at least one non-fallback highlighter's `SupportedLanguages` contains the supplied identifier; wildcard `"*"` entries are not consulted by this probe. |
| `Highlight` | `string Highlight(string code, string language)` | Selects the highest-priority `ICodeHighlighter` whose `SupportedLanguages` contains `language` (or `"*"`) and returns its output; falls back to `PlainTextHighlighter` (HTML-encoded code, no token spans) when none match. |

## `TextMateLanguageRegistry`

```csharp:xmldocid
T:Pennington.Highlighting.TextMateLanguageRegistry
```

_Backs the built-in `TextMateHighlighter` with a grammar lookup and mutable scope registry. Registered as a singleton by `AddPennington` and resolvable from DI for post-registration mutation. The constructor accepts an optional `Action<TextMateLanguageRegistry>` configuration callback. Custom scope mappings and grammars take precedence over the built-in `TextMateSharp` registry when resolving a language id._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `AddGrammar` | `TextMateLanguageRegistry AddGrammar(string languageId, string scopeName)` | Registers a language id → TextMate scope name mapping so a built-in or previously loaded grammar can be selected by the supplied `languageId`. Returns `this` for chaining. |
| `AddGrammarFromJson` | `TextMateLanguageRegistry AddGrammarFromJson(string languageId, string grammarJson)` | Loads a TextMate grammar from a JSON string, reads its `scopeName` (falling back to `source.{languageId}`), and registers both the grammar and the id-to-scope mapping. Returns `this` for chaining. |

## Example

_One minimal example pulled from `examples/ExtensibilityLabExample/PipelineHighlighter.cs` — a custom `ICodeHighlighter` shown at the type level so a reader recognizes the full contract surface (language set, priority, highlight method) in one place. Registration and walkthrough live in the how-to._

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.PipelineHighlighter
```

_Reference shape for a custom `ICodeHighlighter`: a `SupportedLanguages` set, a `Priority` higher than the built-in highlighters it overrides, and a `Highlight` method that returns HTML with `<span class="hljs-*">` tokens._

## See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)
- Related reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](xref:reference.options.auxiliary-options)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
