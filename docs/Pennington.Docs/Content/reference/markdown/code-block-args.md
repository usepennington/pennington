---
title: "Code-block argument reference"
description: "The fence info-string grammar Pennington parses — language token, key/value attributes, quoted values — and the trailing-comment [!code …] directive grammar used for line annotations."
sectionLabel: "Markdown Extensions"
order: 2
tags: [markdown, code-blocks, directives]
uid: reference.markdown.code-block-args
---

The fence info-string grammar: the opening-fence text after the three backticks, tokenised left-to-right into a language (with optional colon-suffix) followed by `key=value` attribute pairs. The `[!code …]` directive grammar runs separately, against the highlighted HTML.

## Fence info-string grammar

```text
info-string   := language [ ":" suffix ] ( WS attribute )*
language      := IDENT
suffix        := "symbol" symbol-flag*
              |  "symbol-diff" [ ",bodyonly" ]
symbol-flag   := ",bodyonly" | ",imports" | ",signatures"
attribute     := key "=" value
key           := IDENT
value         := bare-value | "'" quoted-value "'" | '"' quoted-value '"'
bare-value    := any run of non-whitespace chars
quoted-value  := any chars up to the matching quote
```

`language` is typically `csharp`, `razor`, `text`, and so on. Quoting is required only when a value contains whitespace. Markdig exposes the language and colon-suffix on `FencedCodeBlock.Info` and the attribute tail on `FencedCodeBlock.Arguments`; attribute keys are matched case-insensitively.

## Attributes

| Name | Values | Description | Example |
|---|---|---|---|
| `tabs` | `true` | Marks adjacent fenced blocks for grouping into a single tabbed widget. | ` ```csharp tabs=true title="C#"` |
| `title` | any quoted string | Tab label shown on the tabbed widget; falls back to the normalised language name when omitted. | ` ```csharp tabs=true title="Program.cs"` |

## Suffix forms (code-embedding)

| Form | Body shape | Description |
|---|---|---|
| `<lang>:symbol` | one `<file>` path, optionally followed by `> Member.Path`, per line | Embeds the whole file, or the named member's declaration and body. Concatenated in order. |
| `<lang>:symbol,bodyonly` | same as `:symbol` | Embeds only the member body, stripping the declaration line and enclosing braces. |
| `<lang>:symbol,imports` | same as `:symbol` | Prepends the file's top-of-file import/using/require statements above the snippet. Composes with `,bodyonly`; a no-op for whole-file embeds and import-less languages. |
| `<lang>:symbol,signatures` | same as `:symbol` | Replaces member bodies with an elision marker (`{ … }`, or `…` for non-brace bodies) for an outline view. Mutually exclusive with `,bodyonly`. |
| `<lang>:symbol-diff` | exactly two references, before then after | Emits a unified diff between the two members' source text. Accepts the `,bodyonly` suffix. |

The `symbol` flags (`,bodyonly`, `,imports`, `,signatures`) are comma-separated and order-independent. Suffix forms are resolved by an `ICodeBlockPreprocessor`; `Pennington.TreeSitter` ships the implementations for `symbol` and `symbol-diff`.

## `[!code …]` directives

A directive is the literal text `[!code <notation>]` wrapped in a line-trailing comment marker recognised for the block's language (`//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`); the directive pass runs against the highlighted HTML. The comment marker is stripped when the directive consumes the whole comment and preserved when trailing content remains; the directive itself is always removed from the rendered line.

| Directive | Behavior | Example |
|---|---|---|
| `highlight` (alias `hl`) | Adds class `highlight` to the line and `has-highlighted` to the `<pre>`. | `var x = 1; // [!code highlight]` |
| `++` | Adds class `diff-add` to the line and `has-diff` to the `<pre>`. | `var x = 1; // [!code ++]` |
| `--` | Adds class `diff-remove` to the line and `has-diff` to the `<pre>`. | `var x = 0; // [!code --]` |
| `focus` | Adds class `focused` to the line, `has-focused` to the `<pre>`, and `blurred` to every non-focused line. | `var x = 1; // [!code focus]` |
| `error` | Adds class `error` to the line and `has-errors` to the `<pre>`. | `throw new(); // [!code error]` |
| `warning` | Adds class `warning` to the line and `has-warnings` to the `<pre>`. | `// TODO // [!code warning]` |
| `word:TEXT` | Wraps the first occurrence of `TEXT` on the line in `<span class="word-highlight">`. | `var token = Get(); // [!code word:token]` |
| `word:TEXT&#124;MESSAGE` | As `word:`, but wraps the span in a callout carrying `MESSAGE`. | `// [!code word:token&#124;The auth token]` |
| `include-start` / `include-end` | Marks a region to retain; all lines outside paired include regions are dropped. | `// [!code include-start]` ... `// [!code include-end]` |
| `exclude-start` / `exclude-end` | Marks a region to drop; lines inside are removed from the rendered output. | `// [!code exclude-start]` ... `// [!code exclude-end]` |

## See also

- How-to: [Annotate code blocks](xref:how-to.code-samples.code-annotations)
- How-to: [Create tabbed code groups](xref:how-to.code-samples.tabbed-code)
- Related reference: [Markdown extensions catalog](xref:reference.markdown.extensions)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
