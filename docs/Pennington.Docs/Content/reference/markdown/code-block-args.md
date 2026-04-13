---
title: "Code-block argument reference"
description: "The fence info-string grammar — language token, key/value attributes, quoted values — and the trailing-comment line-annotation directives recognized by CodeTransformer."
section: "markdown"
order: 20
tags: []
uid: reference.markdown.code-block-args
isDraft: true
search: false
llms: false
---

> **In this page.** The fence info-string grammar — language token, key/value attributes, quoted values — and the line-annotation syntax (`{1,3}`, `{+1}`, `{-1}`, `{focus …}`, `{error …}`).
>
> **Not in this page.** Theme selection at render time (see Explanation).

## Summary

The fence info-string that follows triple-backticks is parsed as: a language token (optionally carrying one `:modifier`), followed by space-separated `key=value` attributes. Line annotations are per-line trailing-comment directives of the form `// [!code NOTATION]` — parsed by `Pennington.Markdown.Extensions.CodeTransformer`, not a curly-brace range grammar.

## Info-string grammar

### Shape

```
```LANG[:MODIFIER] KEY=VALUE KEY='quoted value' KEY="quoted value"
```

- `LANG` — the highlighter language id (e.g. `csharp`, `bash`, `json`). Parsed by `CodeHighlightRenderer.ParseBaseLanguage` — everything before the first `:` is the base language; the rest is forwarded as a modifier.
- `:MODIFIER` — optional. Routed to a registered `ICodeBlockPreprocessor` (see table below).
- `KEY=VALUE` pairs — parsed by `CodeBlockExtensions.GetArgumentPairs`. Keys are case-insensitive. Whitespace separates pairs. Values may be bare, single-quoted, or double-quoted; quoted values may contain spaces.

### Language modifiers (Roslyn package)

Registered by `Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor` (Priority 100). Require `services.AddPenningtonRoslyn(...)`.

| Modifier | Body format | Behavior |
|---|---|---|
| `:xmldocid` | One XmlDocID per line | Resolves each id to source, concatenates fragments. |
| `:xmldocid,bodyonly` | One XmlDocID per line | As `:xmldocid`, emitting only the member body. |
| `:xmldocid-diff` | Exactly two XmlDocIDs, one per line | Line-diff between the two fragments. |
| `:xmldocid-diff,bodyonly` | Exactly two XmlDocIDs, one per line | As `:xmldocid-diff`, body-only. |
| `:path` | Single repo-relative path | Loads file, highlights by extension (`.vb` → VB, else C#). Rejects rooted paths and `..`. |

### Known attributes

| Key | Value | Consumer | Effect |
|---|---|---|---|
| `tabs` | `true` | `TabbedCodeBlocksExtension` | Groups this block with consecutive fenced blocks into a single tab widget. |
| `title` | quoted string | `TabbedCodeBlockRenderer` | Overrides the tab label (default derived from `LanguageNormalizer.GetLanguageName(info)`). |

Any other `key=value` pair is parsed and exposed on the `FencedCodeBlock` via `GetArgumentPairs()`; unrecognized keys are silently ignored.

### Value-quoting rules

| Form | Example | Parsed value |
|---|---|---|
| Bare | `tabs=true` | `true` |
| Single-quoted | `title='C# source'` | `C# source` |
| Double-quoted | `title="C# source"` | `C# source` |
| Unterminated quote | `title="unterminated` | everything after the opening quote |

## Line-annotation grammar

Annotations are trailing per-line comment directives. The comment marker must appear on the same line as the directive, with only whitespace between marker and `[!code …]`.

### Syntax

```
CODE // [!code NOTATION]
CODE # [!code NOTATION]
CODE /* [!code NOTATION] */
CODE <!-- [!code NOTATION] -->
```

Recognized comment markers: `//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`.
Optional trailing terminators: `-->`, `*/`.
The directive is stripped from the rendered line; the comment marker is dropped too if nothing follows it.

### Notations

| Notation | Line class | Wrapper `<pre>` class | Behavior |
|---|---|---|---|
| `highlight` (alias `hl`) | `highlight` | `has-highlighted` | Marks the line as highlighted. |
| `++` | `diff-add` | `has-diff` | Line is an addition. |
| `--` | `diff-remove` | `has-diff` | Line is a removal. |
| `focus` | `focused` | `has-focused` (non-focused lines get `blurred`) | Focuses the line; dims all others. |
| `error` | `error` | `has-errors` | Marks the line as an error. |
| `warning` | `warning` | `has-warnings` | Marks the line as a warning. |
| `word:WORD` | — | `has-word-highlights` | Wraps the first occurrence of `WORD` in `<span class="word-highlight">`. |
| `word:WORD\|MESSAGE` | — | `has-word-highlights` | Wraps with `word-highlight-with-message` and appends a callout bubble. |

### Snippet directives

These directives are removed from the output along with their lines. Paired directives must not nest.

| Directive | Meaning |
|---|---|
| `include-start` … `include-end` | Keep only lines strictly between the pair. When present, everything outside any `include` region is dropped. |
| `exclude-start` … `exclude-end` | Drop lines from `exclude-start` through `exclude-end` inclusive. |

Unmatched or nested pairs cause the entire snippet-region set to be discarded (no lines removed); other annotations still apply.

## Examples

### Fence info-string

````markdown
```csharp title="Program.cs"
Console.WriteLine("hi");
```
````

### Tabs

````markdown
```csharp tabs=true title="C#"
Console.WriteLine("hi");
```

```fsharp title="F#"
printfn "hi"
```
````

### Line annotations

````markdown
```csharp
var x = 1; // [!code highlight]
var y = 2; // [!code ++]
var z = 3; // [!code --]
Focus(); // [!code focus]
Fail(); // [!code error]
Warn(); // [!code warning]
Search(value); // [!code word:value|the bound input]
```
````

### Snippet regions

````markdown
```csharp
// [!code exclude-start]
using Setup;
var bootstrap = Build();
// [!code exclude-end]
var result = bootstrap.Run();
```
````

### Roslyn modifier

````markdown
```csharp:xmldocid bodyonly
M:ExampleProject.Demo.Run
```
````

## Legacy / incorrect syntax

The curly-brace forms `{1,3}`, `{+1}`, `{-1}`, `{focus 1-3}`, `{error …}` are **not supported**. There is no line-range grammar in Pennington — every annotation is attached to the line it appears on via a trailing comment directive. If seen in a fence, they will be passed through to the highlighter as literal source.

## See also

- Reference: [Markdown pipeline extensions](/reference/markdown/pipeline-extensions)
- Reference: [Tabbed code blocks](/reference/markdown/tabbed-code-blocks)
- How-to: [Highlight and annotate code samples](/how-to/markdown/annotate-code)
- Background: [Why Pennington uses comment-directive annotations](/explanation/markdown/annotation-design)
