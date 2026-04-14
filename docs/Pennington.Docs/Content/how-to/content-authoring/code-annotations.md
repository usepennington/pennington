---
title: "Annotate code blocks"
description: "Apply highlight, diff, focus, and error/warning line classes to fenced code with trailing `[!code ...]` comment directives."
uid: how-to.content-authoring.code-annotations
order: 60
sectionLabel: Content Authoring
tags: [authoring, code, highlighting, annotations]
---

> **In this page.** _Paraphrase the TOC "Covers" line: applying `[!code highlight]`, `[!code ++]` / `[!code --]`, `[!code focus]`, and `[!code error]` / `[!code warning]` as trailing comments inside fenced code blocks so the rendered HTML gets the matching line classes._
>
> **Not in this page.** _Paraphrase "Does not cover": writing your own `ICodeBlockPreprocessor` to transform fence bodies before highlighting — that belongs in [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)._

## When to use this

_Two sentences. Frame the goal: the reader already has a working fenced code block and wants to call out a specific line (highlight, diff, focus) or mark a line as an error/warning without reaching for a custom extension. Do not re-teach fenced code or highlighting — link to the highlighting explanation for rationale._

## Assumptions

_Three bullets. Each is realistic prior state, not a tutorial step._

- You have an existing Pennington site rendering markdown with highlighted code fences (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- The fenced language supports a comment syntax the transformer recognises — `//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, or `/* */`.
- You are authoring in plain markdown, not injecting HTML directly — the directives are parsed from the rendered highlighter output, so they must travel through the fence as comments.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/exampleKitchenSinkExample) — `Content/main/code-annotations.md` stages one fence per directive and is the fixture this page embeds.

---

## Steps

_Five steps. Each is one imperative action. Snippets are markdown fences (the directive syntax lives inside the fenced body, not in a C# symbol), except step 5 which embeds the full fixture file via `:path`._

### 1. Highlight a single line

_One sentence: append `// [!code highlight]` (swap the comment marker to match the fenced language — `#` for YAML, `--` for SQL) to the line you want emphasized. The transformer strips the directive and adds the `highlight` class to the `.line` span._

````markdown
```csharp
public int Add(int a, int b)
{
    return a + b; // [!code highlight]
}
```
````

### 2. Mark lines as added or removed (diff)

_One sentence: use `[!code ++]` for added lines and `[!code --]` for removed lines to render a diff view — each side gets its own class (`diff-add` / `diff-remove`) so the stylesheet can paint the gutter._

````markdown
```csharp
public int Multiply(int a, int b) // [!code ++]
{
    return a * b; // [!code ++]
}
public int OldWay(int a, int b) // [!code --]
{
    return a + b; // [!code --]
}
```
````

### 3. Focus one line and dim the rest

_One sentence: drop `[!code focus]` on the line (or lines) the reader should zero in on — every other line receives a `dimmed` class so the focused line stands out._

````markdown
```csharp
var config = new Config(); // [!code focus]
config.Apply();
config.Save();
```
````

### 4. Flag errors and warnings

_One sentence: use `[!code error]` and `[!code warning]` to surface diagnostics inline — they apply `error` and `warning` classes respectively so the line is painted like compiler output._

````markdown
```csharp
var path = null; // [!code error]
var length = path.Length; // [!code warning]
```
````

### 5. Keep directives out of the final HTML

_Two sentences: the `CodeTransformer` post-processes the highlighter output, moves the directive's notation onto the enclosing `.line` span, and deletes the trailing comment so the rendered code looks clean. Embed the fixture file that shows all five directives end-to-end as they appear in source markdown._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/code-annotations.md
```

---

## Verify

_Three bullets. Each is one observable check._

- Run `dotnet run` and visit the page — each annotated line shows the expected treatment (highlighted bar, diff marker, dimmed siblings, error/warning paint) and the `// [!code ...]` text is gone from the rendered output.
- View source on the rendered `<pre>` — annotated `.line` spans carry the matching class (`highlight`, `diff-add`, `diff-remove`, `focused`, `dimmed`, `error`, `warning`).
- Swap the comment marker to match a different fenced language (for example `# [!code highlight]` in a `yaml` block) and confirm the directive is still stripped and the class still applied.

## Related

- Reference: [Highlighting interfaces](xref:reference.extension-points.highlighting) — `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor) — when a trailing-comment directive isn't enough and you need to transform fence bodies
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why the transformer runs after the highlighter and where custom highlighters plug in
