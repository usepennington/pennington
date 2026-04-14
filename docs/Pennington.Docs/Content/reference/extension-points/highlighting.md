---
title: "Highlighting interfaces"
description: "Covers ICodeHighlighter, ICodeBlockPreprocessor, HighlightingService, and TextMateLanguageRegistry."
section: "extension-points"
order: 50
tags: []
uid: reference.extension-points.highlighting
isDraft: true
search: false
llms: false
---

> **In this page.** Covers `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`.
>
> **Not in this page.** Does not cover: writing TextMate grammars.

## Summary

- One sentence: what it is — the extension surfaces for adding custom syntax highlighters and pre-highlighted code-block producers to the Markdown pipeline.
- One sentence: where it lives — `Pennington.Highlighting` (three types) and `Pennington.Markdown.Extensions` (one type), in `src/Pennington/`.

## Declaration

- `ICodeHighlighter` — `src/Pennington/Highlighting/ICodeHighlighter.cs`
  - `xmldocid="T:Pennington.Highlighting.ICodeHighlighter"`
- `HighlightingService` — `src/Pennington/Highlighting/HighlightingService.cs`
  - `xmldocid="T:Pennington.Highlighting.HighlightingService"`
- `TextMateLanguageRegistry` — `src/Pennington/Highlighting/TextMateLanguageRegistry.cs`
  - `xmldocid="T:Pennington.Highlighting.TextMateLanguageRegistry"`
- `ICodeBlockPreprocessor` — `src/Pennington/Markdown/Extensions/ICodeBlockPreprocessor.cs`
  - `xmldocid="T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor"`
  - Companion record: `CodeBlockPreprocessResult` (`T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult`)

## `ICodeHighlighter`

Namespace: `Pennington.Highlighting`. Registered via `PenningtonOptions.Highlighting.AddHighlighter<T>()` or `AddHighlighter(instance)`.

### Members

| Name | Type | Description |
|---|---|---|
| `SupportedLanguages` | `IReadOnlySet<string>` | Language ids this highlighter handles; `"*"` matches any language. |
| `Priority` | `int` | Higher wins when multiple highlighters support the same language. |
| `Highlight(string code, string language)` | `string` | Returns highlighted HTML (typically wrapped in `<pre><code>`). |

### Built-in implementations

| Type | Priority | `SupportedLanguages` |
|---|---|---|
| `ShellHighlighter` | 75 | `bash`, `shell`, `sh` |
| `TextMateHighlighter` | 50 | `*` |
| `PlainTextHighlighter` | 0 | `*` (fallback only) |

## `HighlightingService`

Namespace: `Pennington.Highlighting`. Sealed class. Constructor takes `IEnumerable<ICodeHighlighter>`; registered as a singleton by `AddPennington`.

### Members

| Name | Signature | Description |
|---|---|---|
| `.ctor` | `HighlightingService(IEnumerable<ICodeHighlighter> highlighters)` | Sorts inputs by `Priority` descending. |
| `Highlight` | `string Highlight(string code, string language)` | Dispatches to the first supporting highlighter; falls back to `PlainTextHighlighter`. |
| `HasHighlighter` | `bool HasHighlighter(string language)` | `true` if any registered (non-fallback) highlighter matches `language`. |

## `TextMateLanguageRegistry`

Namespace: `Pennington.Highlighting`. Sealed class. Registered as a singleton by `AddPennington`; injected into `TextMateHighlighter`. Uses `TextMateSharp` with the `DarkPlus` theme.

### Members

| Name | Signature | Description |
|---|---|---|
| `.ctor` | `TextMateLanguageRegistry(Action<TextMateLanguageRegistry>? configure = null)` | Optional callback for registering custom languages at construction. |
| `AddGrammar` | `TextMateLanguageRegistry AddGrammar(string languageId, string scopeName)` | Maps an id to an existing TextMate scope name. Returns `this`. |
| `AddGrammarFromJson` | `TextMateLanguageRegistry AddGrammarFromJson(string languageId, string grammarJson)` | Loads a JSON grammar at runtime; scope defaults to `source.{languageId}` unless the JSON declares `scopeName`. Returns `this`. |

Internal surface not part of the public contract: `Registry`, `GetScopeNameForLanguage`.

## `ICodeBlockPreprocessor`

Namespace: `Pennington.Markdown.Extensions`. Consumed by `CodeHighlightRenderer` before dispatch to `HighlightingService`. Registered as `IEnumerable<ICodeBlockPreprocessor>` via standard DI (`services.AddSingleton<ICodeBlockPreprocessor, T>()`).

### Members

| Name | Type | Description |
|---|---|---|
| `Priority` | `int` | Higher runs first. |
| `TryProcess(string code, string languageId)` | `CodeBlockPreprocessResult?` | Returns a result to short-circuit highlighting, or `null` to pass through. |

### `CodeBlockPreprocessResult`

| Name | Type | Default | Description |
|---|---|---|---|
| `HighlightedHtml` | `string` | — | Fully highlighted HTML (wrapped in `<pre>`/`<code>`). |
| `BaseLanguage` | `string` | — | Base language id used for CSS class emission. |
| `SkipTransform` | `bool` | `false` | When `true`, skip `CodeTransformer` on the output. |

## Example

A custom `ICodeHighlighter` registered via `PenningtonOptions.Highlighting.AddHighlighter<T>()`.

```csharp:xmldocid,bodyonly
T:ForgePortalExample.PipelineHighlighter
```

## See also

- Related reference: [Auxiliary options classes (`HighlightingOptions`)](/reference/options/auxiliary-options)
- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- How-to: [Add a custom syntax highlighter](/how-to/extensibility/custom-highlighter)
- Background: [The syntax-highlighting cascade](/explanation/rendering/highlighting)
