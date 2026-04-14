---
title: "Code-block argument reference"
description: "The fence info-string grammar Pennington parses — language token, key/value attributes, quoted values — and the trailing-comment [!code …] directive grammar used for line annotations."
sectionLabel: "Markdown Extensions"
order: 20
tags: [markdown, code-blocks, directives]
uid: reference.markdown.code-block-args
---

> **In this page.** _One sentence paraphrased from `docs-toc.md`: the fence info-string grammar — language token, key/value attributes, quoted values — and the trailing-comment `[!code …]` directive grammar used for line annotations._
>
> **Not in this page.** _One sentence paraphrased from `docs-toc.md`: theme selection at render time belongs to the syntax-highlighting cascade — see Explanation [The syntax-highlighting cascade](/explanation/rendering/highlighting)._

## Summary

_**One sentence: what it is.** The two grammars Pennington applies to a fenced code block — the info-string tokens on the opening fence line (language, suffix, key/value attributes), and the `[!code …]` directives embedded in line-trailing comments._
_**One sentence: where it lives.** Info-string parsing is implemented by `CodeBlockExtensions.GetArgumentPairs` in namespace `Pennington.Markdown.Extensions`; directive handling is implemented by `CodeTransformer.Transform` in the same namespace, consumed by `CodeHighlightRenderer`._

## Fence info-string grammar

_One sentence placing the grammar: an info string is the text on the opening fence line after the three backticks, tokenised left-to-right by the Markdig parser and post-processed by `CodeBlockExtensions.GetArgumentPairs`. Tokens are whitespace-separated; the first token is the language (optionally with a colon-suffix), and remaining tokens are `key=value` attribute pairs. Values may be bare, single-quoted, or double-quoted — quoting is required only when the value contains whitespace._

```text
info-string   := language [ ":" suffix ] ( WS attribute )*
language      := IDENT                              ; e.g. csharp, razor, text
suffix        := "path" | "xmldocid" | "xmldocid,bodyonly" | "xmldocid-diff" | "xmldocid-diff,bodyonly"
attribute     := key "=" value
key           := IDENT
value         := bare-value | "'" quoted-value "'" | '"' quoted-value '"'
bare-value    := any run of non-whitespace chars
quoted-value  := any chars up to the matching quote
```

_One sentence on the parser: `CodeBlockExtensions.GetArgumentPairs` returns a case-insensitive `Dictionary<string, string>` containing only the `key=value` attributes — it does not return the language or suffix, which Markdig exposes separately on `FencedCodeBlock.Info` and `FencedCodeBlock.Arguments`._

```csharp:xmldocid
M:Pennington.Markdown.Extensions.CodeBlockExtensions.GetArgumentPairs(Markdig.Syntax.FencedCodeBlock)
```

## Attributes

_One sentence scoping the table: the `key=value` attributes Pennington's built-in extensions consume from the info string. Custom extensions registered via `ICodeBlockPreprocessor` or a Markdig extension may read additional keys from the same dictionary; only the keys below are product-dictated._

| Name | Values | Description | Example |
|---|---|---|---|
| `tabs` | `true` | Marks adjacent fenced blocks for grouping into a single tabbed widget by `TabbedCodeBlocksExtension`. | ` ```csharp tabs=true title="C#"` |
| `title` | any quoted string | Tab label shown by `TabbedCodeBlockRenderer`; falls back to the normalised language name when omitted. | ` ```csharp tabs=true title="Program.cs"` |

## Suffix forms (code-embedding)

_One sentence scoping the table: the four colon-suffix forms that switch a fenced block from literal content to a code-embedding directive preprocessed before highlighting. Suffix forms are resolved by an `ICodeBlockPreprocessor` — `Pennington.Roslyn` ships the implementation that handles `xmldocid` and `xmldocid-diff`. The canonical grammar lives in `docs/Pennington.Docs/CLAUDE.md`._

| Form | Body shape | Description |
|---|---|---|
| `<lang>:path` | one file path relative to the solution directory | Embeds the entire file contents as the code-block body. |
| `<lang>:xmldocid` | one XmlDocId per line (`T:`, `M:`, `P:`, `F:`, `E:`) | Embeds each symbol's declaration and body, concatenated in order. |
| `<lang>:xmldocid,bodyonly` | one XmlDocId per line | Embeds only the member body, stripping the declaration line and enclosing braces. |
| `<lang>:xmldocid-diff` | exactly two XmlDocIds, before then after | Emits a unified diff between the two symbols' source text; accepts the `,bodyonly` suffix. |

## `[!code …]` directives

_One sentence placing the directive grammar: a directive is the literal text `[!code <notation>]` wrapped in a line-trailing comment marker recognised for the block's language (`//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`), matched by `CodeTransformer.FindDirective` after highlighting. The comment marker is stripped when the directive consumes the whole comment, and preserved when trailing content remains; the directive itself is always removed from the rendered line._

```csharp:xmldocid
M:Pennington.Markdown.Extensions.CodeTransformer.Transform(System.String)
```

| Directive | Behavior | Example |
|---|---|---|
| `highlight` (alias `hl`) | Adds class `highlight` to the line and `has-highlighted` to the `<pre>`. | `var x = 1; // [!code highlight]` |
| `++` | Adds class `diff-add` to the line and `has-diff` to the `<pre>`. | `var x = 1; // [!code ++]` |
| `--` | Adds class `diff-remove` to the line and `has-diff` to the `<pre>`. | `var x = 0; // [!code --]` |
| `focus` | Adds class `focused` to the line, `has-focused` to the `<pre>`, and `blurred` to every non-focused line. | `var x = 1; // [!code focus]` |
| `error` | Adds class `error` to the line and `has-errors` to the `<pre>`. | `throw new(); // [!code error]` |
| `warning` | Adds class `warning` to the line and `has-warnings` to the `<pre>`. | `// TODO // [!code warning]` |
| `word:TEXT` | Wraps the first occurrence of `TEXT` on the line in `<span class="word-highlight">`. | `var token = Get(); // [!code word:token]` |
| `word:TEXT\|MESSAGE` | As `word:`, but wraps the span in a callout carrying `MESSAGE`. | `// [!code word:token\|The auth token]` |
| `include-start` / `include-end` | Marks a region to retain; all lines outside paired include regions are dropped. | `// [!code include-start]` ... `// [!code include-end]` |
| `exclude-start` / `exclude-end` | Marks a region to drop; lines inside are removed from the rendered output. | `// [!code exclude-start]` ... `// [!code exclude-end]` |

## See also

- How-to: [Annotate code blocks](/how-to/content-authoring/code-annotations)
- How-to: [Create tabbed code groups](/how-to/content-authoring/tabbed-code)
- Related reference: [Markdown extensions catalog](/reference/markdown/extensions)
- Background: [The syntax-highlighting cascade](/explanation/rendering/highlighting)
